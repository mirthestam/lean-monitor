using System;
using System.Collections.Generic;
using System.Linq;
using LiveCharts.Geared;
using QuantConnect.Lean.Monitor.Model;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    /// <summary>
    /// Specialized view model for a benchmark chart.
    /// </summary>
    public class BenchmarkChartViewModel : ChartViewModelBase, IResultParser
    {
        private GearedValues<TimeStampChartPoint> _benchmarkChartValues = new GearedValues<TimeStampChartPoint>();

        private bool _benchmarkVisible = true;

        public bool IsBenchmarkVisible
        {
            get { return _benchmarkVisible; }
            set
            {
                _benchmarkVisible = value;
                RaisePropertyChanged();
            }
        }

        public override bool CanClose => false;

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
            Title = "Benchmark";

            ParseBenchmark(result);
            // TODO: Add other interesting series here for the benchmark tab. I.e. price relative values

            if (BenchmarkChartValues.Count <= 0) return;
            LastUpdated = BenchmarkChartValues[BenchmarkChartValues.Count - 1].X;
        }

        protected override TimeStamp GetXTimeStamp(int index)
        {
            index = Math.Min(index, _benchmarkChartValues.Count - 1);
            return _benchmarkChartValues.Count < index
                ? new TimeStamp()
                : _benchmarkChartValues[index].X;
        }

        private void ParseBenchmark(Result result)
        {
            Chart chart;
            Series series;

            if (!result.Charts.TryGetValue("Benchmark", out chart)) return;
            if (!chart.Series.TryGetValue("Benchmark", out series)) return;
            series = series.Since(LastUpdated);

            var values = series.Values.Select(cp => cp.ToTimeStampChartPoint());
            BenchmarkChartValues.AddRange(values);

            // Update the view window for the chart
            ZoomFrom = 0;
            ZoomTo = BenchmarkChartValues.Count - 1;
        }
    }
}
