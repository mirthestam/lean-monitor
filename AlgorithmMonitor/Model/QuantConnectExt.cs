using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Monitor.Model.Charting;
using Monitor.Utils;
using QuantConnect;
using ScatterMarkerSymbol = Monitor.Model.Charting.ScatterMarkerSymbol;
using SeriesType = Monitor.Model.Charting.SeriesType;

namespace Monitor.Model
{
    public static class QuantConnectExt
    {
        public static Dictionary<string, ChartDefinition> ToModelChart(this IDictionary<string, Chart> sourceDictionary)
        {
            return sourceDictionary.ToDictionary(entry => entry.Key, entry => ToModelChart(entry.Value));
        }

        public static TimeStampChartPoint ToTimeStampChartPoint(this ChartPoint point)
        {
            return new TimeStampChartPoint
            {
                // QuantConnect chartpoints are always in Unix TimeStamp
                X = TimeStamp.FromSeconds(point.x),
                Y = point.y
            };
        }

        public static ChartDefinition ToModelChart(this Chart sourceChart)
        {
            return new ChartDefinition
            {
                Name = sourceChart.Name,                
                Series = sourceChart.Series.ToModelSeries()
            };
        }

        public static Dictionary<string, SeriesDefinition> ToModelSeries(this IDictionary<string, Series> sourceSeries)
        {
            return sourceSeries.ToDictionary(entry => entry.Key, entry => entry.Value.ToModelSeries());
        }

        public static ScatterMarkerSymbol ToModelSeries(this QuantConnect.ScatterMarkerSymbol symbol)
        {
            switch (symbol)
            {
                case QuantConnect.ScatterMarkerSymbol.None:
                    return ScatterMarkerSymbol.None;
                    
                case QuantConnect.ScatterMarkerSymbol.Circle:
                    return ScatterMarkerSymbol.Circle;
                    
                case QuantConnect.ScatterMarkerSymbol.Square:
                    return ScatterMarkerSymbol.Square;
                    
                case QuantConnect.ScatterMarkerSymbol.Diamond:
                    return ScatterMarkerSymbol.Diamond;
                    
                case QuantConnect.ScatterMarkerSymbol.Triangle:
                    return ScatterMarkerSymbol.Triangle;

                case QuantConnect.ScatterMarkerSymbol.TriangleDown:
                    return ScatterMarkerSymbol.TriangleDown;

                default:
                    throw new NotSupportedException($"ScatterMarkerSymbol {symbol} is not supported.");
            }
        }

        public static SeriesType ToModelSeries(this QuantConnect.SeriesType seriesType)
        {
            switch (seriesType)
            {
                case QuantConnect.SeriesType.Line:
                    return SeriesType.Line;
                    
                case QuantConnect.SeriesType.Scatter:
                    return SeriesType.Scatter;

                case QuantConnect.SeriesType.Candle:
                    return SeriesType.Candle;

                case QuantConnect.SeriesType.Bar:
                    return SeriesType.Bar;
                    
                default:
                    throw new NotSupportedException($"SeriesType {seriesType} is not supported.");
            }
        }

        public static SeriesDefinition ToModelSeries(this Series sourceSeries)
        {
            return new SeriesDefinition
            {
                Color = Color.FromArgb(sourceSeries.Color.A, sourceSeries.Color.R, sourceSeries.Color.G,sourceSeries.Color.B),
                Index = sourceSeries.Index,
                Name = sourceSeries.Name,
                ScatterMarkerSymbol = sourceSeries.ScatterMarkerSymbol.ToModelSeries(),
                SeriesType = sourceSeries.SeriesType.ToModelSeries(),
                Unit = sourceSeries.Unit,
                Values = sourceSeries.Values.Select(v => v.ToTimeStampChartPoint()).ToList()
            };
        }  
    }
}