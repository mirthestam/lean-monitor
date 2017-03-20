using System.Collections.Generic;
using System.Linq;
using LiveCharts.Geared;
using Monitor.Model;
using Monitor.Model.Charting;
using Monitor.Utils;

namespace Monitor.ViewModel.Charts
{
    public class StrategyEquityChartViewModel : ChartViewModelBase, IResultParser
    {
        private GearedValues<TimeStampOhlcChartPoint> _equityOhlcChartValues = new GearedValues<TimeStampOhlcChartPoint>();
        private GearedValues<TimeStampChartPoint> _equityChartValues = new GearedValues<TimeStampChartPoint>();
        private GearedValues<TimeStampChartPoint> _dailyPerformanceChartValues = new GearedValues<TimeStampChartPoint>();
        private GearedValues<TimeStampChartPoint> _benchmarkChartValues = new GearedValues<TimeStampChartPoint>();

        private Dictionary<string, TimeStamp> _lastUpdates = new Dictionary<string, TimeStamp>();

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
            lock (_equityChartValues)
            {
                Title = "Strategy Equity";

                ParseEquity(result);
                ParseDailyPerformance(result);
                ParseBenchmark(result);
            }
        }

        private void ParseDailyPerformance(Result result)
        {
            ChartDefinition chart;
            SeriesDefinition series;

            if (!result.Charts.TryGetValue("Strategy Equity", out chart)) return;
            if (!chart.Series.TryGetValue("Daily Performance", out series)) return;

            if (!_lastUpdates.ContainsKey("Daily Performance")) _lastUpdates["Daily Performance"] = TimeStamp.MinValue;
            var lastUpdate = _lastUpdates["Daily Performance"];

            series = series.Since(lastUpdate);
            if (series.Values.Count == 0) return;

            var values = series.Values
                .OrderBy(cp => cp.X)
                .GroupBy(cp => cp.X)
                .Select(
                    g =>
                    {
                        return new TimeStampChartPoint
                        {
                            X = TimeStamp.FromDays(g.First().X.ElapsedDays),
                            Y = g.Max(a => a.Y)
                        };
                    }).ToList();

            DailyPerformanceChartValues.AddRange(values);
            
            _lastUpdates["Daily Performance"] = values.Last().X;
        }

        private void ParseEquity(Result result)
        {
            ChartDefinition chart;
            SeriesDefinition series;

            if (!result.Charts.TryGetValue("Strategy Equity", out chart)) return;
            if (!chart.Series.TryGetValue("Equity", out series)) return;

            if (!_lastUpdates.ContainsKey("Strategy Equity")) _lastUpdates["Strategy Equity"] = TimeStamp.MinValue;
            var lastUpdate = _lastUpdates["Strategy Equity"];
            
            series = series.Since(lastUpdate);
            if (series.Values.Count == 0) return;

            var values = series.Values
                .OrderBy(cp => cp.X.ElapsedDays)
                .GroupBy(cp => cp.X.ElapsedDays)
                .Select(
                    g =>
                    {
                        return new TimeStampOhlcChartPoint
                        {
                            X = TimeStamp.FromDays(g.First().X.ElapsedDays), // The FromDays creates a new timestamp ignoring known hours, minutes etc
                            Open = (double)g.First().Y,
                            Close = (double)g.Last().Y,
                            Low = (double)g.Min(z => z.Y),
                            High = (double)g.Max(z => z.Y)
                        };
                    }).ToList();

            EquityOhlcChartValues.AddRange(values);
            TimeStamps.AddRange(values.Select(v => v.X));
            RebuildTimeStampIndex();

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

            _lastUpdates["Strategy Equity"] = values.Last().X;
        }

        private void ParseBenchmark(Result result)
        {
            ChartDefinition benchmarkChart;
            SeriesDefinition benchmarkSeries;

            if (!result.Charts.TryGetValue("Benchmark", out benchmarkChart)) return;
            if (!benchmarkChart.Series.TryGetValue("Benchmark", out benchmarkSeries)) return;

            if (!_lastUpdates.ContainsKey("Benchmark")) _lastUpdates["Benchmark"] = TimeStamp.MinValue;
            var lastUpdate = _lastUpdates["Benchmark"];

            benchmarkSeries = benchmarkSeries.Since(lastUpdate);

            var benchmarkValues = benchmarkSeries.Values;

            if (!benchmarkValues.Any()) return;

            var relValues = new List<TimeStampChartPoint>();

            // This assumes the ParseBenchmark is called after the ParseEquity method.
            var equityOpenValue = EquityChartValues[0].Y;

            relValues.Add(new TimeStampChartPoint(benchmarkValues[0].X, equityOpenValue));
            for (var i = 1; i < benchmarkValues.Count; i++)
            {
                var originalX = benchmarkValues[i].X;
                var x = TimeStamp.FromDays(originalX.ElapsedDays);

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

            _lastUpdates["Benchmark"] = relValues.Last().X;
            BenchmarkChartValues.AddRange(relValues);
        }
    }
}