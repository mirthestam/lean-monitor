using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Utils;
using Color = System.Drawing.Color;
using LiveChartSeries = LiveCharts.Wpf.Series;
using QuantChartSeries = QuantConnect.Series;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    /// <summary>
    /// View model for generic charts
    /// </summary>
    public class ChartViewModel : ChartViewModelBase, IChartParser
    {        
        private ObservableCollection<ChildChartViewModel> _children = new ObservableCollection<ChildChartViewModel>();
        private SeriesCollection _scrollSeriesCollection = new SeriesCollection();        

        public override bool CanClose => false;

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

        public static ChartResolution DetectResolution(QuantChartSeries series)
        {
            if (series.SeriesType == SeriesType.Candle)
            {
                // Candle data is supposed to be grouped.
                // Currently we group candle data by day.
                return ChartResolution.Day;
            }

            var chartPoints = series.Values.Select(cp => cp.ToTimeStampChartPoint()).ToList();

            // Check whether we have duplicates in day mode
            var dayDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedDays).Any(g => g.Count() > 1);
            if (!dayDuplicates) return ChartResolution.Day;

            var hourDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedHours).Any(g => g.Count() > 1);
            if (!hourDuplicates) return ChartResolution.Hour;

            var minuteDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedMinutes).Any(g => g.Count() > 1);
            if (!minuteDuplicates) return ChartResolution.Minute;

            var secondDuplicates = chartPoints.GroupBy(cp => cp.X.ElapsedSeconds).Any(g => g.Count() > 1);
            if (!secondDuplicates) return ChartResolution.Second;

            throw new Exception("Resolution is below second which is not supported.");
        }

        public void ParseChart(Chart sourceChart)
        {
            // Update the title
            Title = sourceChart.Name;

            // Validate the chart
            if (sourceChart.Series.Count == 0) return;

            var lastUpdate = TimeStamp.FromDays(0);

            // Group series by their Index.
            // This index is the index of a chart they need to be drawn upon.
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
                                StrokeThickness = 1,
                            }
                        }
                    });

                    // Build the series
                    foreach (var quantSeries in sourceSeriesGroup
                        .OrderByDescending(sg => sg.Value.Values.Count)
                        .Select(sg => sg.Value))
                    {
                        var series = BuildSeries(quantSeries);
                        childModel.SeriesCollection.Add(series);
                    }
                }

                // Update the series
                var seriesIndex = 0;
                foreach (var quantSeries in sourceSeriesGroup
                    .OrderByDescending(sg => sg.Value.Values.Count)
                    .Select(sg => sg.Value))
                {
                    var series = childModel.SeriesCollection[seriesIndex];

                    var updates = quantSeries.Since(LastUpdated);
                    UpdateSeries(series, updates);
                    seriesIndex++;

                    lastUpdate = TimeStamp.FromSeconds(quantSeries.Values[quantSeries.Values.Count - 1].x);                    
                }
            }


            var sourceScrollSeries = sourceChart.Series
                .Select(s => s.Value)
                .OrderByDescending(s => s.SeriesType == SeriesType.Line)
                .ThenByDescending(s => s.SeriesType == SeriesType.Candle)
                .First(s => s.Index == 0);

            var scrollSeries = (LiveChartSeries) ScrollSeriesCollection.FirstOrDefault();
            if (scrollSeries == null)
            {
                scrollSeries = BuildSeries(sourceScrollSeries);
                ScrollSeriesCollection.Add(scrollSeries);
            }

            var scrollSeriesUpdates = sourceScrollSeries.Since(LastUpdated);
            UpdateSeries(scrollSeries, scrollSeriesUpdates);

            // Update our timestamps based upon the new updates for the scroll series
            TimeStamps.AddRange(scrollSeriesUpdates.Values.Select(v => v.ToTimeStampChartPoint().X));

            if (ZoomTo == 1)
            {
                // Zoom to the known number of values.
                ZoomTo = ScrollSeriesCollection[0].Values.Count;
            }

            // Update the last update
            LastUpdated = lastUpdate;

        }

        private LiveChartSeries BuildSeries(QuantChartSeries sourceSeries)
        {
            LiveChartSeries series;

            switch (sourceSeries.SeriesType)
            {
                case SeriesType.Line:
                    series = new GLineSeries
                    {
                        Configuration = ChartPointEvaluator,
                        Fill = Brushes.Transparent
                    };
                    break;

                case SeriesType.Bar:
                    series = new GColumnSeries
                    {
                        Configuration = ChartPointEvaluator,
                        Fill = Brushes.Transparent
                    };
                    break;

                case SeriesType.Candle:
                    series = new GOhlcSeries
                    {
                        Configuration = OhlcChartPointEvaluator,
                        Fill = Brushes.Transparent
                    };
                    break;

                case SeriesType.Scatter:
                    series = new GScatterSeries
                    {
                        Configuration = ChartPointEvaluator,
                        StrokeThickness = 1
                    };
                    break;

                default:
                    throw new NotSupportedException();
            }

            series.Title = sourceSeries.Name;
            series.PointGeometry = GetPointGeometry(sourceSeries.ScatterMarkerSymbol);
            
            // Default color is '0'.Do not replace the stroke in this case
            if (sourceSeries.Color.Name != "0")  
            {                
                var color = System.Windows.Media.Color.FromArgb(sourceSeries.Color.A, sourceSeries.Color.R, sourceSeries.Color.G, sourceSeries.Color.B);
                series.Stroke = new SolidColorBrush(color);
                if (!Equals(series.Fill, Brushes.Transparent))
                {
                    series.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, color.R, color.G, color.B));    
                }
            }

            return series;
        }

        private void UpdateSeries(ISeriesView targetSeries, QuantChartSeries sourceSeries)
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
                    var existingCommonValues = (GearedValues<TimeStampChartPoint>) (targetSeries.Values ?? (targetSeries.Values = new GearedValues<TimeStampChartPoint>()));
                    existingCommonValues.AddRange(sourceSeries.Values.Select(cp => cp.ToTimeStampChartPoint()));
                    break;

                case SeriesType.Candle:
                    // Build daily candles
                    var existingCandleValues = (GearedValues<TimeStampOhlcChartPoint>)(targetSeries.Values ?? (targetSeries.Values = new GearedValues<TimeStampOhlcChartPoint>()));
                    var newValues = sourceSeries.Values.Select(cp => cp.ToTimeStampChartPoint()).GroupBy(cp => cp.X.ElapsedDays).Select(
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
                        });

                    existingCandleValues.AddRange(newValues);
                    break;

                default:
                    throw new Exception($"SeriesType {sourceSeries.SeriesType} is not supported.");
            }
        }

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
                case ScatterMarkerSymbol.TriangleDown:
                    return DefaultGeometries.Triangle;

                default:
                    return DefaultGeometries.Circle;

            }
        }
    }
}