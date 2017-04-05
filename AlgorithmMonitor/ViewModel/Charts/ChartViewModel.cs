﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Monitor.Model;
using Monitor.Model.Charting;
using Monitor.Model.Messages;
using Monitor.Properties;
using Monitor.Utils;

namespace Monitor.ViewModel.Charts
{
    /// <summary>
    /// View model for generic charts
    /// </summary>
    public class ChartViewModel : ChartViewModelBase, IChartView
    {        
        private readonly Dictionary<string, TimeStamp> _lastUpdates = new Dictionary<string, TimeStamp>();
        private readonly IMessenger _messenger;
        private readonly int _seriesMaximum = Settings.Default.ChartSeriesLimit;

        private TimeStamp _startPoint = TimeStamp.FromDays(0);        

        public ChartViewModel(IMessenger messenger)
        {
            _messenger = messenger;
        }

        private readonly List<Color> _defaultColors = new List<Color>
        {
            Color.FromArgb(0, 0, 0, 0),
            Color.FromArgb(255, 0, 0, 0)
        };

        private ObservableCollection<ChildChartViewModel> _children = new ObservableCollection<ChildChartViewModel>();
        private SeriesCollection _scrollSeriesCollection = new SeriesCollection();
        private DateTime _referenceDateTime = TimeStamp.FromDays(0).DateTime;

        public override bool CanClose => false;

        public DateTime ReferenceDateTime
        {
            get { return _referenceDateTime; }
            set
            {
                _referenceDateTime = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the children of this chart
        /// </summary>
        public ObservableCollection<ChildChartViewModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the seriesCollection of the horizontal scroll bar chart
        /// </summary>
        public SeriesCollection ScrollSeriesCollection
        {
            get { return _scrollSeriesCollection; }
            set
            {
                _scrollSeriesCollection = value;
                RaisePropertyChanged();
            }
        }

        public static Resolution DetectResolution(SeriesDefinition series)
        {
            if (series.SeriesType == SeriesType.Candle)
            {
                // Candle data is supposed to be grouped.
                // Currently we group candle data by day.
                return Resolution.Day;
            }

            var chartPoints = series.Values;

            // Check whether we have duplicates in day mode
            var dayDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedDays).Any(g => g.Count() > 1);
            if (!dayDuplicates) return Resolution.Day;

            var hourDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedHours).Any(g => g.Count() > 1);
            if (!hourDuplicates) return Resolution.Hour;

            var minuteDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedMinutes).Any(g => g.Count() > 1);
            if (!minuteDuplicates) return Resolution.Minute;

            var secondDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedSeconds).Any(g => g.Count() > 1);
            if (!secondDuplicates) return Resolution.Second;

            return Resolution.Ticks;
        }
        
        public void ParseChart(ChartDefinition sourceChart)
        {
            // Update the title
            Title = sourceChart.Name;

            // Validate the chart
            if (sourceChart.Series.Count == 0) return;

            // Group series by their Index.
            // This index is the index of a chart they need to be drawn upon.

            var timeStamps = new List<TimeStamp>();

            foreach (var sourceSeriesGroup in sourceChart.Series
                .OrderByDescending(s => s.Value.Index)
                .GroupBy(s => s.Value.Index))            
            {
                // Get the model representing this index
                var childModel = Children.FirstOrDefault(g => g.Index == sourceSeriesGroup.Key);
                if (childModel == null)
                {
                    // Build a new model
                    childModel = new ChildChartViewModel(sourceSeriesGroup.Key, this);
                    Children.Add(childModel);
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
                var seriesIndex = 0;
                foreach (var quantSeries in sourceSeriesGroup
                    .Select(sg => sg.Value)
                    .Where(v => v.Values.Count > 0))
                {
                    //var series = childModel.SeriesCollection[seriesIndex];
                    var series = childModel.SeriesCollection.FirstOrDefault(x => x.Title == quantSeries.Name);

                    if (series == null)
                    {
                        series = BuildSeries(quantSeries);
                        childModel.SeriesCollection.Add(series);
                    }

                    if (!childModel.LastUpdates.ContainsKey(series.Title)) childModel.LastUpdates[series.Title] = TimeStamp.MinValue;
                    
                    var updates = quantSeries.Since(childModel.LastUpdates[series.Title]);
                    UpdateSeries(series, updates);
                    timeStamps.AddRange(updates.Values.Select(u => u.X));
                    if (updates.Values.Any()) childModel.LastUpdates[series.Title] = updates.Values.Last().X;

                    if (updates.Values.Any() && series.Values.Count == _seriesMaximum)
                    {
                        // This series is probably truncated by the LEAN engine. Add warning visual elemeent
                        var lastValue = updates.Values.Last();
                        childModel.CreateTruncatedVisuaLElement(0, lastValue.X, lastValue.Y);
                        _messenger.Send(new LogEntryReceivedMessage(DateTime.Now, $"Series { this.Title}.{series.Title} is possibly truncated by the LEAN engine", LogItemType.Monitor));
                    }

                    seriesIndex++;
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
                _startPoint = sourceScrollSeries.Values[0].X;
                scrollSeries = BuildSeries(sourceScrollSeries);
                ScrollSeriesCollection.Add(scrollSeries);
            }

            if (!_lastUpdates.ContainsKey("Scroll")) _lastUpdates["Scroll"] = TimeStamp.MinValue;            
            var scrollSeriesUpdates = sourceScrollSeries.Since(_lastUpdates["Scroll"]);

            if (scrollSeries != null)
            { 
                UpdateSeries(scrollSeries, scrollSeriesUpdates);
                if (scrollSeriesUpdates.Values.Any()) _lastUpdates["Scroll"] = scrollSeriesUpdates.Values.Last().X;
            }

            if (ZoomTo == 1)
            {
                // Zoom to the known number of values.
                ZoomTo = _lastUpdates["Scroll"].ElapsedTicks / AxisModifier;

                double diff;
                
                // Determine a default scale
                switch (Resolution)
                {
                    case Resolution.Second:
                    case Resolution.Minute:
                    case Resolution.Hour:
                        diff = (TimeSpan.TicksPerDay * 30) / AxisModifier; // Show a month
                        break;

                    case Resolution.Day:
                        diff = (TimeSpan.TicksPerDay * 60) / AxisModifier; // Show approx 2 months
                        break;
                    default:
                        diff = (TimeSpan.TicksPerDay * 60) / AxisModifier;
                        break;
                }

                ZoomFrom = ZoomTo - diff;
            }
            else if (!IsPositionLocked)
            {
                // Scroll to latest data
                var diff = ZoomTo - ZoomFrom;
                ZoomTo = _lastUpdates["Scroll"].ElapsedTicks / AxisModifier;
                ZoomFrom = ZoomTo - diff;
            }
        }

        protected override void ZoomToFit()
        {
            ZoomFrom = _startPoint.ElapsedTicks / AxisModifier;
            ZoomTo = Math.Max(1, _lastUpdates["Scroll"].ElapsedTicks / AxisModifier);
        }

        private Series BuildSeries(SeriesDefinition sourceSeries)
        {
            Series series;

            switch (sourceSeries.SeriesType)
            {
                case SeriesType.Line:
                    series = new LineSeries
                    {
                        Configuration = ChartPointEvaluator,
                        Fill = Brushes.Transparent
                    };
                    break;

                case SeriesType.Bar:
                    series = new ColumnSeries
                    {
                        Configuration = ChartPointEvaluator,
                    };
                    break;

                case SeriesType.Candle:
                    series = new CandleSeries
                    {
                        Configuration = OhlcChartPointEvaluator,
                        IncreaseBrush = Brushes.LightCoral,
                        DecreaseBrush = Brushes.Aquamarine,
                        Fill = Brushes.Transparent
                    };
                    break;

                case SeriesType.Scatter:
                    //series = new GScatterSeries
                    //{
                    //    Configuration = ChartPointEvaluator,
                    //    StrokeThickness = 1,
                    //    GearedPointGeometry = GetGearedPointGeometry(sourceSeries.ScatterMarkerSymbol)
                    //};
                    series = new ScatterSeries
                    {
                        Configuration = ChartPointEvaluator,
                        StrokeThickness = 1,
                    };

                    break;

                default:
                    throw new NotSupportedException();
            }

            series.Title = sourceSeries.Name;
            series.PointGeometry = GetPointGeometry(sourceSeries.ScatterMarkerSymbol);
            
            // Check whether the series has a color configured
            if (_defaultColors.All(c => !sourceSeries.Color.Equals(c)))
            {                
                // No default color present. use it for the stroke
                var brush = new SolidColorBrush(sourceSeries.Color);
                brush.Freeze();

                switch (sourceSeries.SeriesType)
                {
                    case SeriesType.Candle:
                        series.Fill = brush;
                        break;

                    default:
                        series.Stroke = brush;
                        break;
                }                
            }

            return series;
        }

        private void UpdateSeries(ISeriesView targetSeries, SeriesDefinition sourceSeries)
        {
            // Detect the data resolution of the source series.
            // Use it as chart resolution if needed.
            var detectedResolution = DetectResolution(sourceSeries);
            if (Resolution > detectedResolution) Resolution = detectedResolution;

            // QuantChart series are unix timestamp
            switch (sourceSeries.SeriesType)
            {
                case SeriesType.Scatter:
                case SeriesType.Bar:
                case SeriesType.Line:
                    var existingCommonValues = (ChartValues<TimeStampChartPoint>) (targetSeries.Values ?? (targetSeries.Values = new ChartValues<TimeStampChartPoint>()));
                    existingCommonValues.AddRange(sourceSeries.Values);
                    break;

                case SeriesType.Candle:
                    // Build daily candles
                    var existingCandleValues = (ChartValues<TimeStampOhlcChartPoint>)(targetSeries.Values ?? (targetSeries.Values = new ChartValues<TimeStampOhlcChartPoint>()));
                    var newValues = sourceSeries.Values.GroupBy(cp => cp.X.ElapsedDays).Select(
                        g =>
                        {
                            return new TimeStampOhlcChartPoint
                            {
                                X = g.First().X,
                                Open = (double)g.First().Y,
                                Close = (double)g.Last().Y,
                                Low = (double)g.Min(z => z.Y),
                                High = (double)g.Max(z => z.Y)
                            };
                        }).ToList();

                    // Update existing ohlc points.
                    UpdateExistingOhlcPoints(existingCandleValues, newValues, Resolution.Day);

                    existingCandleValues.AddRange(newValues);
                    break;

                default:
                    throw new Exception($"SeriesType {sourceSeries.SeriesType} is not supported.");
            }
        }

        //private static GeometryShape GetGearedPointGeometry(ScatterMarkerSymbol symbol)
        //{
        //    switch (symbol)
        //    {
        //        case ScatterMarkerSymbol.None:
        //            return null;
                    
        //        case ScatterMarkerSymbol.Circle:
        //            return new Circle(); 
                    
        //        case ScatterMarkerSymbol.Square:
        //            return new Square();
                    
        //        case ScatterMarkerSymbol.Diamond:
        //            return new Diamond();
                    
        //        case ScatterMarkerSymbol.Triangle:
        //            return new Triangle();
                    
        //        case ScatterMarkerSymbol.TriangleDown:
        //            return new TriangleDown();
                    
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null);
        //    }
        //}

        private static Geometry GetPointGeometry(ScatterMarkerSymbol symbol)
        {
            switch (symbol)
            {
                case ScatterMarkerSymbol.None:
                    return DefaultGeometries.None;

                case ScatterMarkerSymbol.Circle:
                    return DefaultGeometries.Circle;

                case ScatterMarkerSymbol.Diamond:
                    return DefaultGeometries.Diamond;

                case ScatterMarkerSymbol.Square:
                    return DefaultGeometries.Square;

                case ScatterMarkerSymbol.Triangle:
                    return DefaultGeometries.Triangle;

                case ScatterMarkerSymbol.TriangleDown:
                    return Geometries.TriangleDown;

                default:
                    return DefaultGeometries.Circle;

            }
        }
    }
}