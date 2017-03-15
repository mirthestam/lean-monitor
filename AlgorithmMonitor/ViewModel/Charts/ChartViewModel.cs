using System;
using System.Linq;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Utils;
using LiveChartSeries = LiveCharts.Wpf.Series;
using QuantChartSeries = QuantConnect.Series;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    /// <summary>
    /// View model for generic charts
    /// </summary>
    public class ChartViewModel : ChartViewModelBase, IChartParser
    {
        private SeriesCollection _seriesCollection = new SeriesCollection();
        private SeriesCollection _summarySeriesCollection = new SeriesCollection();
        private AxesCollection _yAxesCollection = new AxesCollection();
        
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

            // Build YAxes
            if (YAxesCollection.Count == 0)
            {
                // Chart series have indexes, and can share an yaxis.
                var axesCount = chart.Series.Values.Max(v => v.Index + 1);
                for (int axis = 0; axis < axesCount; axis++)
                {
                    // Get the series shown on this axis
                    var series = chart.Series.Values.Where(v => v.Index == axis);

                    // Create Y axis for the series
                    YAxesCollection.Add(new Axis
                    {
                        // Title is with combined series
                        Title = string.Join(", ", series.Select(s => s.Name).ToArray()),
                        Position = AxisPosition.RightTop
                    });
                }
            }

            // Build Series
            var yAxeIndex = 0;

            var isNewSeries = SeriesCollection.Count == 0;
            if (isNewSeries)
            {
                // Build the series
                foreach (var quantSeries in chart.Series.Values)
                {
                    var series = BuildSeries(quantSeries);
                    series.ScalesYAt = quantSeries.Index;
                    SeriesCollection.Add(series);
                    yAxeIndex++;
                }

                // Build the summary series
                var summarySeries = BuildSeries(chart.Series.Values.First());
                SummarySeriesCollection.Add(summarySeries);
            }

            // Update the series
            yAxeIndex = 0;
            var lastUpdate = TimeStamp.FromDays(0);
            foreach (var quantSeries in chart.Series.Values)
            {
                var series = SeriesCollection[yAxeIndex];

                var updates = quantSeries.Since(LastUpdated);
                UpdateSeries(series, updates);
                if (yAxeIndex == 0)
                {
                    var summarySeries = SummarySeriesCollection.First();
                    UpdateSeries(summarySeries, updates);
                }

                if (isNewSeries && yAxeIndex == 0)
                {
                    // Use this first series for the from and to view
                    ZoomFrom = 0;
                    ZoomTo = quantSeries.Values.Count - 1;
                    lastUpdate = TimeStamp.FromSeconds(quantSeries.Values[quantSeries.Values.Count - 1].x);
                }
                else
                {
                    lastUpdate = TimeStamp.FromSeconds(quantSeries.Values[quantSeries.Values.Count - 1].x);
                }

                yAxeIndex++;
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

        public AxesCollection YAxesCollection
        {
            get { return _yAxesCollection; }
            set
            {
                _yAxesCollection = value;
                RaisePropertyChanged();
            }
        }

        public SeriesCollection SeriesCollection
        {
            get { return _seriesCollection; }
            set
            {
                _seriesCollection = value;
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
                    return DefaultGeometries.Circle;

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
            var refCollection = _seriesCollection[0];
            index = Math.Min(index, refCollection.Values.Count - 1);
            var timeStampSource = (ITimeStampChartPoint)refCollection.Values[index];
            return timeStampSource.X;
        }
    }
}