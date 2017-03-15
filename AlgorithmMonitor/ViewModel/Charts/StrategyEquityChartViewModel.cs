using System;
using System.Collections.Generic;
using System.Linq;
using LiveCharts.Geared;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    public class StrategyEquityChartViewModel : ChartViewModelBase, IResultParser
    {
        private GearedValues<TimeStampOhlcChartPoint> _equityOhlcChartValues = new GearedValues<TimeStampOhlcChartPoint>();
        private GearedValues<TimeStampChartPoint> _equityChartValues = new GearedValues<TimeStampChartPoint>();
        private GearedValues<TimeStampChartPoint> _dailyPerformanceChartValues = new GearedValues<TimeStampChartPoint>();
        private GearedValues<TimeStampChartPoint> _benchmarkChartValues = new GearedValues<TimeStampChartPoint>();

        private bool _isDailyPerformanceSeriesVisible = true;
        private bool _isBenchmarkSeriesVisible = true;

        public bool IsDailyPerformanceSeriesVisible
        {
            get { return _isDailyPerformanceSeriesVisible; }
            set
            {
                _isDailyPerformanceSeriesVisible = value;
                RaisePropertyChanged();
            }
        }
        public bool IsBenchmarkSeriesVisible
        {
            get { return _isBenchmarkSeriesVisible; }
            set
            {
                _isBenchmarkSeriesVisible = value;
                RaisePropertyChanged();
            }
        }

        public override bool CanClose => false;

        public GearedValues<TimeStampOhlcChartPoint> EquityOhlcChartValues
        {
            get { return _equityOhlcChartValues; }
            set
            {
                _equityOhlcChartValues = value;
                RaisePropertyChanged();
            }
        }
        public GearedValues<TimeStampChartPoint> EquityChartValues
        {
            get { return _equityChartValues; }
            set
            {
                _equityChartValues = value;
                RaisePropertyChanged();
            }
        }
        public GearedValues<TimeStampChartPoint> DailyPerformanceChartValues
        {
            get { return _dailyPerformanceChartValues; }
            set
            {
                _dailyPerformanceChartValues = value;
                RaisePropertyChanged();
            }
        }
        public GearedValues<TimeStampChartPoint> BenchmarkChartValues
        {
            get { return _benchmarkChartValues; }
            set
            {
                _benchmarkChartValues = value;
                RaisePropertyChanged();
            }
        }

        public void ParseResult(Result result)
        {
            Title = "Strategy Equity";

            ParseEquity(result);
            ParseBenchmark(result);
            ParseDailyPerformance(result);
            
            // Update the last updated date
            if (EquityChartValues.Count <= 0) return;
            LastUpdated = EquityChartValues[EquityChartValues.Count - 1].X;
        }

        protected override TimeStamp GetXTimeStamp(int index)
        {
            // Our X index is based upon the Ohlc chart values.
            index = Math.Min(index, _equityOhlcChartValues.Count - 1);
            return _equityOhlcChartValues[index].X;
        }

        private void ParseDailyPerformance(Result result)
        {
            Chart chart;
            Series series;

            if (!result.Charts.TryGetValue("Strategy Equity", out chart)) return;
            if (!chart.Series.TryGetValue("Daily Performance", out series)) return;
            series = series.Since(LastUpdated);

            var values = series.Values.Select(cp => cp.ToTimeStampChartPoint())
                .OrderBy(cp => cp.X)
                .GroupBy(cp => cp.X)
                .Select(
                    g =>
                    {
                        return new TimeStampChartPoint
                        {
                            X = g.First().X,
                            Y = g.Max(a => a.Y)
                        };
                    });

            DailyPerformanceChartValues.AddRange(values);
        }

        private void ParseEquity(Result result)
        {
            Chart chart;
            Series series;

            if (!result.Charts.TryGetValue("Strategy Equity", out chart)) return;
            if (!chart.Series.TryGetValue("Equity", out series)) return;

            series = series.Since(LastUpdated);

            var values = series.Values.Select(cp => cp.ToTimeStampChartPoint())
                .OrderBy(cp => cp.X.ElapsedDays) 
                .GroupBy(cp => cp.X.ElapsedDays)
                .Select(
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

            EquityOhlcChartValues.AddRange(values);

            // Update normal chart with only the close values
            EquityChartValues.AddRange(values.Select(v => new TimeStampChartPoint
            {
                X = v.X,
                Y = (decimal)v.Close
            }));

            // Update the view window for the chart
            if (EquityChartValues.Count > 0)
            {
                ZoomFrom = 0;
                ZoomTo = EquityChartValues.Count - 1;
            }
        }

        private void ParseBenchmark(Result result)
        {
            Chart benchmarkChart;
            Series benchmarkSeries;

            if (!result.Charts.TryGetValue("Benchmark", out benchmarkChart)) return;
            if (!benchmarkChart.Series.TryGetValue("Benchmark", out benchmarkSeries)) return;
            benchmarkSeries = benchmarkSeries.Since(LastUpdated);

            var benchmarkValues = benchmarkSeries.Values.Select(cp => cp.ToTimeStampChartPoint()).ToList();
            var relValues = new List<TimeStampChartPoint>();

            // This assumes the ParseBenchmark is called after the ParseEquity method.
            var equityOpenValue = EquityChartValues[0].Y;

            relValues.Add(new TimeStampChartPoint(benchmarkValues[0].X, equityOpenValue));
            for (var i = 1; i < benchmarkValues.Count; i++)
            {
                var x = benchmarkValues[i].X;
                decimal y;

                var curBenchmarkValue = benchmarkValues[i].Y;
                var prefBenchmarkValue = benchmarkValues[i - 1].Y;
                if (prefBenchmarkValue == 0 || curBenchmarkValue == 0)
                {
                    // TODO: Cannot divide by 0. Investigate how this can happen
                    y = relValues[i - 1].Y;
                }
                else
                {
                    y = relValues[i - 1].Y * (curBenchmarkValue / prefBenchmarkValue);
                }
                relValues.Add(new TimeStampChartPoint(x, y));
            }

            BenchmarkChartValues.AddRange(relValues);
        }
    }
}