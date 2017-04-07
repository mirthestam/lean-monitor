using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Definitions.Series;
using LiveCharts.Geared;
using LiveCharts.Geared.Geometries;
using LiveCharts.Wpf;
using Monitor.Charting;
using Monitor.Model.Charting;
using NodaTime;
using QuantConnect;
using ScatterMarkerSymbol = Monitor.Model.Charting.ScatterMarkerSymbol;
using Series = LiveCharts.Wpf.Series;
using SeriesType = Monitor.Model.Charting.SeriesType;

namespace Monitor.ViewModel.Charts
{
    /// <summary>
    /// Chart component responsible for working with series
    /// </summary>
    public class SeriesChartComponent
    {
        private readonly IChartView _view;
        private readonly List<Color> _defaultColors = new List<Color>
        {
            Color.FromArgb(0, 0, 0, 0),
            Color.FromArgb(255, 0, 0, 0)
        };

        private IPointEvaluator<OhlcInstantChartPoint> _ohlcChartPointEvaluator;
        private IPointEvaluator<InstantChartPoint> _chartPointEvaluator;

        private IPointEvaluator<InstantChartPoint> ChartPointEvaluator => _chartPointEvaluator ?? (_chartPointEvaluator = new InstantChartPointMapper(_view));
        private IPointEvaluator<OhlcInstantChartPoint> OhlcChartPointEvaluator => _ohlcChartPointEvaluator ?? (_ohlcChartPointEvaluator = new OhlcInstantChartPointMapper(_view));
        
        public SeriesChartComponent(IChartView view)
        {
            _view = view;
        }

        public static Resolution DetectResolution(SeriesDefinition series)
        {
            if (series.SeriesType == SeriesType.Candle)
            {
                // Candle data is supposed to be grouped.
                // Currently we group candle data by day.
                return Resolution.Daily;
            }

            var chartPoints = series.Values;

            // Check whether we have duplicates in day mode

            var dayDuplicates = chartPoints.GroupBy(cp => cp.X.ToUnixTimeTicks() / NodaConstants.TicksPerDay).Any(g => g.Count() > 1);
            if (!dayDuplicates) return Resolution.Daily;

            var hourDuplicates = chartPoints.GroupBy(cp => cp.X.ToUnixTimeTicks() / NodaConstants.TicksPerHour).Any(g => g.Count() > 1);
            if (!hourDuplicates) return Resolution.Hour;

            var minuteDuplicates = chartPoints.GroupBy(cp => cp.X.ToUnixTimeTicks() / NodaConstants.TicksPerMinute).Any(g => g.Count() > 1);
            if (!minuteDuplicates) return Resolution.Minute;

            var secondDuplicates = chartPoints.GroupBy(cp => cp.X.ToUnixTimeTicks() / NodaConstants.TicksPerSecond).Any(g => g.Count() > 1);
            if (!secondDuplicates) return Resolution.Second;

            return Resolution.Tick;
        }

        public Series BuildSeries(SeriesDefinition sourceSeries)
        {
            Series series;

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
                    };
                    break;

                case SeriesType.Candle:
                    series = new GCandleSeries
                    {
                        Configuration = OhlcChartPointEvaluator,
                        IncreaseBrush = Brushes.Aquamarine,
                        DecreaseBrush = Brushes.LightCoral,
                        Fill = Brushes.Transparent
                    };
                    break;

                case SeriesType.Scatter:
                    series = new GScatterSeries
                    {
                        Configuration = ChartPointEvaluator,
                        StrokeThickness = 1,
                        GearedPointGeometry = GetGearedPointGeometry(sourceSeries.ScatterMarkerSymbol)
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

        public void UpdateSeries(ISeriesView targetSeries, SeriesDefinition sourceSeries)
        {
            // Detect the data resolution of the source series.
            // Use it as chart resolution if needed.
            var detectedResolution = DetectResolution(sourceSeries);
            if (_view.Resolution > detectedResolution) _view.Resolution = detectedResolution;           

            // QuantChart series are unix timestamp
            switch (sourceSeries.SeriesType)
            {
                case SeriesType.Scatter:
                case SeriesType.Bar:
                case SeriesType.Line:
                    var existingCommonValues = (GearedValues<InstantChartPoint>)(targetSeries.Values ?? (targetSeries.Values = new GearedValues<InstantChartPoint>()));
                    existingCommonValues.AddRange(sourceSeries.Values);
                    break;

                case SeriesType.Candle:
                    // Build daily candles
                    var existingCandleValues = (ChartValues<OhlcInstantChartPoint>)(targetSeries.Values ?? (targetSeries.Values = new ChartValues<OhlcInstantChartPoint>()));
                    var newValues = sourceSeries.Values.GroupBy(cp => Instant.MaxValue.Minus(cp.X).Days).Select(
                        g =>
                        {
                            return new OhlcInstantChartPoint
                            {
                                X = g.First().X,
                                Open = (double)g.First().Y,
                                Close = (double)g.Last().Y,
                                Low = (double)g.Min(z => z.Y),
                                High = (double)g.Max(z => z.Y)
                            };
                        }).ToList();

                    // Update existing ohlc points.
                    UpdateExistingOhlcPoints(existingCandleValues, newValues, Resolution.Daily);

                    existingCandleValues.AddRange(newValues);
                    break;

                default:
                    throw new Exception($"SeriesType {sourceSeries.SeriesType} is not supported.");
            }
        }

        public void UpdateExistingOhlcPoints(IList<OhlcInstantChartPoint> existingPoints, IList<OhlcInstantChartPoint> updatedPoints, Resolution resolution)
        {
            // Check whether we are updating existing points
            if (existingPoints.Count <= 0) return;

            if (resolution != Resolution.Daily)
            {
                throw new ArgumentOutOfRangeException($"Resolution {resolution} is not supported. Only Day is supported.");
            }

            // Check whether we have new information for the last ohlc point
            var lastKnownDay = existingPoints.Last().X.ToUnixTimeSeconds() * 60 * 24;
            while (updatedPoints.Any() && (updatedPoints.First().X.ToUnixTimeSeconds() * 60 * 24 <= lastKnownDay)) // We assume we always show ohlc in day groups
            {
                // Update the last ohlc point with this inforrmation
                var refval = updatedPoints.First();

                // find the value matching this day
                var ohlcEquityChartValue = existingPoints.Last();

                // Update ohlc point with highest and lowest, and with the new closing price
                // Update the normal point with the new closing value
                ohlcEquityChartValue.High = Math.Max(refval.High, ohlcEquityChartValue.High);
                ohlcEquityChartValue.Low = Math.Min(refval.Low, ohlcEquityChartValue.Low);
                ohlcEquityChartValue.Close = refval.Close;

                // Remove this value, as it has been parsed into existing chart points
                updatedPoints.RemoveAt(0);
            }
        }

        private static GeometryShape GetGearedPointGeometry(ScatterMarkerSymbol symbol)
        {
            switch (symbol)
            {
                case ScatterMarkerSymbol.None:
                    return null;

                case ScatterMarkerSymbol.Circle:
                    return new Circle();

                case ScatterMarkerSymbol.Square:
                    return new Square();

                case ScatterMarkerSymbol.Diamond:
                    return new Diamond();

                case ScatterMarkerSymbol.Triangle:
                    return new Triangle();

                case ScatterMarkerSymbol.TriangleDown:
                    return new TriangleDown();

                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null);
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
                    return DefaultGeometries.Triangle;

                case ScatterMarkerSymbol.TriangleDown:
                    return Geometries.TriangleDown;

                default:
                    return DefaultGeometries.Circle;

            }
        }        
    }
}