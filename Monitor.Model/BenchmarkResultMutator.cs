using System.Collections.Generic;
using System.Linq;
using Monitor.Model.Charting;
using NodaTime;

namespace Monitor.Model
{
    public class BenchmarkResultMutator : IResultMutator
    {
        public void Mutate(Result result)
        {
            if (!result.Charts.ContainsKey("Benchmark")) return;
            var benchmarkChart = result.Charts["Benchmark"];

            if (!benchmarkChart.Series.ContainsKey("Benchmark")) return;
            var benchmarkSeries = benchmarkChart.Series["Benchmark"];


            if (!result.Charts.ContainsKey("Strategy Equity")) return;
            var equityChart = result.Charts["Strategy Equity"];

            if (!equityChart.Series.ContainsKey("Equity")) return;
            var equitySeries = equityChart.Series["Equity"];

            var benchmarkLastUpdated = Instant.MinValue;
            SeriesDefinition relativeBenchmarkSeries;
            if (!equityChart.Series.ContainsKey("Relative Benchmark"))
            {
                relativeBenchmarkSeries = new SeriesDefinition
                {
                    SeriesType = SeriesType.Line,
                    Name = "Relative Benchmark"
                };
                equityChart.Series.Add("Relative Benchmark", relativeBenchmarkSeries);
            }
            else
            {
                relativeBenchmarkSeries = equityChart.Series["Relative Benchmark"];
                benchmarkLastUpdated = relativeBenchmarkSeries.Values.Last().X;
            }            

            Update(relativeBenchmarkSeries, benchmarkSeries, equitySeries, benchmarkLastUpdated);
        }

        private void Update(SeriesDefinition relativeBenchmarkSeries, SeriesDefinition benchmarkSeries, SeriesDefinition equitySeries, Instant lastUpdate)
        {
            benchmarkSeries = benchmarkSeries.Since(lastUpdate);

            var benchmarkValues = benchmarkSeries.Values;

            if (!benchmarkValues.Any()) return;

            var relValues = new List<InstantChartPoint>();

            // This assumes the ParseBenchmark is called after the ParseEquity method.
            var equityOpenValue = equitySeries.Values[0].Y;

            relValues.Add(new InstantChartPoint(benchmarkValues[0].X, equityOpenValue));
            for (var i = 1; i < benchmarkValues.Count; i++)
            {
                var originalX = benchmarkValues[i].X;
                var x = Instant.FromUnixTimeTicks(originalX.ToUnixTimeTicks()); // TODO: Instant is struct. Clone it?

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
                relValues.Add(new InstantChartPoint(x, y));
            }

            relativeBenchmarkSeries.Values.AddRange(relValues);
        }
    }
}
