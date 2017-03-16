using System;
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
        private ObservableCollection<ChartSeriesCollectionViewModel> _seriesCollections = new ObservableCollection<ChartSeriesCollectionViewModel>();
        private SeriesCollection _summarySeriesCollection = new SeriesCollection();
        
        public override bool CanClose => false;

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

        public void ParseChart(Chart chart)
        {
            // Update the title
            Title = chart.Name;

            // Validate the chart
            if (chart.Series.Count == 0) return;

            var lastUpdate = TimeStamp.FromDays(0);

            // Group series by their Index.
            // This index is the index of a chart they need to be drawn upon.
            foreach (var seriesGroup in chart.Series.OrderBy(s => s.Value.Index).GroupBy(s => s.Value.Index))            
            {
                // Get the model representing this index
                var model = GroupedSeries.FirstOrDefault(g => g.Index == seriesGroup.Key);
                if (model == null)
                {
                    // Build a new model
                    model = new ChartSeriesCollectionViewModel(seriesGroup.Key, this);
                    GroupedSeries.Add(model);
                    model.Index = seriesGroup.Key;

                    // Create Y axis for the series
                    model.YAxesCollection.Add(new Axis
                    {
                        // Title is with combined series
                        Title = string.Join(", ", seriesGroup.Select(s => s.Value.Name).ToArray()),
                        Position = AxisPosition.RightTop
                    });

                    // Build the series
                    foreach (var quantSeries in seriesGroup.Select(sg => sg.Value))
                    {
                        var series = BuildSeries(quantSeries);
                        model.SeriesCollection.Add(series);
                    }
                }

                // Update the series
                var seriesIndex = 0;
                foreach (var quantSeries in seriesGroup.Select(sg => sg.Value))
                {
                    var series = model.SeriesCollection[seriesIndex];

                    var updates = quantSeries.Since(LastUpdated);
                    UpdateSeries(series, updates);
                    seriesIndex++;

                    lastUpdate = TimeStamp.FromSeconds(quantSeries.Values[quantSeries.Values.Count - 1].x);                    
                }
            }


            // Build summary series
            if (SummarySeriesCollection.Count == 0)
            {
                var newSummarySeries = BuildSeries(chart.Series.OrderBy(s => s.Value.Index).Select(s => s.Value).First());
                SummarySeriesCollection.Add(newSummarySeries);
            }

            // Update summary series
            var summarySeries = SummarySeriesCollection.First();
            var summarySeriesSource = chart.Series.First().Value;
            var summaryUpdates = summarySeriesSource.Since(LastUpdated);
            UpdateSeries(summarySeries, summaryUpdates);


            if (ZoomTo == 1)
            {
                // Zoom to the known number of values.
                ZoomTo = SummarySeriesCollection[0].Values.Count;
            }

            // Update the last update
            LastUpdated = lastUpdate;

        }

        private LiveChartSeries BuildSeries(QuantChartSeries qSeries)
        {
            LiveChartSeries series;

            switch (qSeries.SeriesType)
            {
                case SeriesType.Line:
                    series = new GLineSeries();
                    break;

                case SeriesType.Bar:
                    series = new GColumnSeries();
                    break;

                case SeriesType.Candle:
                    series = new GOhlcSeries();
                    break;

                default:
                    throw new NotSupportedException();
            }

            series.Configuration = ChartPointMapper;
            series.Title = qSeries.Name;
            series.PointGeometry = GetPointGeometry(qSeries.ScatterMarkerSymbol);
            
            // Default color is '0'.Do not replace the stroke in this case
            if (qSeries.Color.Name != "0")  
            {                
                var color = System.Windows.Media.Color.FromArgb(qSeries.Color.A, qSeries.Color.R, qSeries.Color.G, qSeries.Color.B);
                series.Stroke = new SolidColorBrush(color);
                //series.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, color.R, color.G, color.B));
            }
            series.Fill = Brushes.Transparent;

            return series;
        }

        private void UpdateSeries(ISeriesView series, QuantChartSeries qSeries)
        {
            var detectedResolution = DetectResolution(qSeries);
            if (Resolution > detectedResolution)
            {
                Resolution = detectedResolution;
            }

            // QuantChart series are unix timestamp
            switch (qSeries.SeriesType)
            {
                case SeriesType.Line:
                    var existingLineValues = (GearedValues<TimeStampChartPoint>) (series.Values ?? (series.Values = new GearedValues<TimeStampChartPoint>()));
                    existingLineValues.AddRange(qSeries.Values.Select(cp => cp.ToTimeStampChartPoint()));

                    // Update range
                    break;

                case SeriesType.Bar:
                    var existingBarValues = (GearedValues<TimeStampChartPoint>)(series.Values ?? (series.Values = new GearedValues<TimeStampChartPoint>()));
                    existingBarValues.AddRange(qSeries.Values.Select(cp => cp.ToTimeStampChartPoint()));
                    break;

                case SeriesType.Candle:
                    // Build daily candles
                    // TODO: Candle allows for custom resolution
                    var existingCandleValues = (GearedValues<TimeStampOhlcChartPoint>)(series.Values ?? (series.Values = new GearedValues<TimeStampOhlcChartPoint>()));
                    var newValues = qSeries.Values.Select(cp => cp.ToTimeStampChartPoint()).GroupBy(cp => cp.X).Select(
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
            }
        }

        public ObservableCollection<ChartSeriesCollectionViewModel> GroupedSeries
        {
            get { return _seriesCollections; }
            set
            {
                _seriesCollections = value;
                RaisePropertyChanged();
            }
        }

        public SeriesCollection SummarySeriesCollection
        {
            get { return _summarySeriesCollection; }
            set
            {
                _summarySeriesCollection = value;
                RaisePropertyChanged();
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

        protected override TimeStamp GetXTimeStamp(int index)
        {
            var referenceSeries = SummarySeriesCollection.FirstOrDefault();
            if (referenceSeries == null) return new TimeStamp();

            index = Math.Min(index, referenceSeries.Values.Count - 1);
            var timeStampSource = (ITimeStampChartPoint)referenceSeries.Values[index];
            return timeStampSource.X;
        }
    }
}