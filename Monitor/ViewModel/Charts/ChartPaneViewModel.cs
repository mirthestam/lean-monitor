using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using Monitor.Model;
using Monitor.Model.Charting;
using Monitor.Model.Messages;
using Monitor.Properties;
using NodaTime;
using QuantConnect;
using Series = LiveCharts.Wpf.Series;
using SeriesType = Monitor.Model.Charting.SeriesType;

namespace Monitor.ViewModel.Charts
{
    /// <summary>
    /// View model for generic charts
    /// </summary>
    public class ChartPaneViewModel : DocumentPaneViewModel, IChartView
    {
        private readonly IMessenger _messenger;
        private readonly SeriesChartComponent _chartParser;
        private readonly int _seriesMaximum = Settings.Default.ChartSeriesLimit;

        private DateTime _initialDateTime = Instant.FromUnixTimeSeconds(0).ToDateTimeUtc();
        private bool _isPositionLocked;
        private ObservableCollection<ChartViewModel> _chartViewModels = new ObservableCollection<ChartViewModel>();
        private SeriesCollection _scrollSeriesCollection = new SeriesCollection();
               
        public RelayCommand ShowGridCommand { get; private set; }

        /// <inheritdoc />
        public Resolution Resolution { get; set; } = Resolution.Daily;

        /// <inheritdoc />
        public Dictionary<string, Instant> LastUpdates { get; } = new Dictionary<string, Instant>();

        /// <inheritdoc />
        public bool IsPositionLocked
        {
            get { return _isPositionLocked; }
            set
            {
                _isPositionLocked = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand ZoomFitCommand { get; private set; }

        public ZoomChartComponent Zoom { get; set; }

        /// <summary>
        /// Gets the DateTime representing the first X value
        /// </summary>
        public DateTime InitialDateTime
        {
            get { return _initialDateTime; }
            protected set
            {
                _initialDateTime = value;
                RaisePropertyChanged();
            }
        }

        /// <inheritdoc />
        public double AxisModifier
        {
            get
            {
                switch (Resolution)
                {
                    case Resolution.Second:
                        return NodaConstants.TicksPerSecond;

                    case Resolution.Minute:
                        return NodaConstants.TicksPerMinute;

                    case Resolution.Hour:
                        return NodaConstants.TicksPerHour;

                    case Resolution.Daily:
                        return NodaConstants.TicksPerDay;

                    case Resolution.Tick:
                        return 1;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ObservableCollection<ChartViewModel> Charts
        {
            get { return _chartViewModels; }
            set
            {
                _chartViewModels = value;
                RaisePropertyChanged();
            }
        }

        public SeriesCollection ScrollSeriesCollection
        {
            get { return _scrollSeriesCollection; }
            set
            {
                _scrollSeriesCollection = value;
                RaisePropertyChanged();
            }
        }

        public ChartPaneViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            
            // Create our child components
            _chartParser = new SeriesChartComponent(this);
            Zoom = new ZoomChartComponent(this);

            // Bind the commands
            ShowGridCommand = new RelayCommand(() => Messenger.Default.Send(new GridRequestMessage(Key)));
            ZoomFitCommand = new RelayCommand(Zoom.ZoomToFit);
        }        
                
        public void ParseChart(ChartDefinition sourceChart)
        {
            // Update the title
            Name = sourceChart.Name;

            // Validate the chart
            if (sourceChart.Series.Count == 0) return;

            // Group series by their Index.
            // This index is the index of a chart they need to be drawn upon.
            foreach (var sourceSeriesGroup in sourceChart.Series
                .OrderByDescending(s => s.Value.Index)
                .GroupBy(s => s.Value.Index))            
            {
                // Get the model representing this index
                var childModel = Charts.FirstOrDefault(g => g.Index == sourceSeriesGroup.Key);
                if (childModel == null)
                {
                    // Build a new model
                    childModel = new ChartViewModel(sourceSeriesGroup.Key, this);
                    Charts.Add(childModel);
                    childModel.Index = sourceSeriesGroup.Key;

                    // Create Y axis for the series
                    childModel.YAxesCollection.Add(new Axis
                    {
                        // Title is with combined series
                        Title = string.Join(", ", sourceSeriesGroup.Select(s => s.Value.Name).ToArray()),
                        Position = AxisPosition.RightTop,
                        Sections = new SectionsCollection
                        {
                            // Horizontal 0 value line
                            new AxisSection
                            {
                                Value = 0,
                                Stroke = Brushes.Gray,
                                StrokeThickness = 1
                            }
                        }
                    });
                }

                // Update the series
                foreach (var quantSeries in sourceSeriesGroup
                    .Select(sg => sg.Value)
                    .Where(v => v.Values.Count > 0))
                {
                    //var series = childModel.SeriesCollection[seriesIndex];
                    var series = childModel.SeriesCollection.FirstOrDefault(x => x.Title == quantSeries.Name);

                    if (series == null)
                    {
                        series = _chartParser.BuildSeries(quantSeries);
                        childModel.SeriesCollection.Add(series);
                    }

                    if (!childModel.LastUpdates.ContainsKey(series.Title)) childModel.LastUpdates[series.Title] = Instant.MinValue;
                    
                    var updates = quantSeries.Since(childModel.LastUpdates[series.Title]);
                    _chartParser.UpdateSeries(series, updates);

                    if (updates.Values.Any()) childModel.LastUpdates[series.Title] = updates.Values.Last().X;
                    if (updates.Values.Any() && series.Values.Count == _seriesMaximum)
                    {
                        // This series is probably truncated by the LEAN engine. Add warning visual elemeent
                        var lastValue = updates.Values.Last();
                        childModel.CreateTruncatedVisuaLElement(0, lastValue.X, lastValue.Y);
                        _messenger.Send(new LogEntryReceivedMessage(DateTime.Now, $"Series { Name}.{series.Title} is possibly truncated by the LEAN engine", LogItemType.Monitor));
                    }
                }
            }

            var sourceScrollSeries = sourceChart.Series
                .Select(s => s.Value)
                .OrderByDescending(s => s.SeriesType == SeriesType.Line)
                .ThenByDescending(s => s.SeriesType == SeriesType.Candle)
                .First(s => s.Index == 0);

            var scrollSeries = (Series) ScrollSeriesCollection.FirstOrDefault();
            if (scrollSeries == null && sourceScrollSeries.Values.Any())
            {                
                Zoom.StartPoint = sourceScrollSeries.Values[0].X;
                scrollSeries = _chartParser.BuildSeries(sourceScrollSeries);
                ScrollSeriesCollection.Add(scrollSeries);
            }

            if (!LastUpdates.ContainsKey("Scroll")) LastUpdates["Scroll"] = Instant.MinValue;            
            var scrollSeriesUpdates = sourceScrollSeries.Since(LastUpdates["Scroll"]);

            if (scrollSeries != null)
            {
                _chartParser.UpdateSeries(scrollSeries, scrollSeriesUpdates);
                if (scrollSeriesUpdates.Values.Any()) LastUpdates["Scroll"] = scrollSeriesUpdates.Values.Last().X;
            }

            Zoom.AutoZoom();
        }
    }
}