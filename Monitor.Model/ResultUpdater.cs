using System.Linq;

namespace Monitor.Model
{
    public static class ResultUpdater
    {
        public static void Merge(Result target, Result source)
        {
            MergeCharts(target, source);
            MergeOrders(target, source);
            MergeStatistics(target, source);

            UpdateProfitLoss(target, source);

            UpdateRollingWindow(target, source);
        }

        private static void UpdateRollingWindow(Result target, Result source)
        {
            foreach (var x in source.RollingWindow)
            {
                target.RollingWindow[x.Key] = x.Value;
            }
        }

        private static void MergeCharts(Result target, Result source)
        {
            // Update charts
            foreach (var sourceChart in source.Charts)
            {
                // Check whether the chart is already known
                if (target.Charts.ContainsKey(sourceChart.Key))
                {
                    var targetChart = target.Charts[sourceChart.Key];

                    // Existing chart. Check whether series are known
                    foreach (var sourceSeries in sourceChart.Value.Series)
                    {
                        if (targetChart.Series.ContainsKey(sourceSeries.Key))
                        {
                            // Series is already known. Update it with new values
                            var targetSeries = targetChart.Series[sourceSeries.Key];
                            targetSeries.Values.AddRange(sourceSeries.Value.Values.Except(targetSeries.Values));
                        }
                        else
                        {
                            // This is a new series. Add it.
                            target.Charts[sourceChart.Key].Series.Add(sourceSeries.Key, sourceSeries.Value);
                        }
                    }
                }
                else
                {
                    // New chart. Add it recursively.
                    target.Charts.Add(sourceChart.Key, sourceChart.Value);
                }
            }
        }

        private static void MergeOrders(Result target, Result source)
        {
            foreach (var order in source.Orders)
            {
                if (target.Orders.ContainsKey(order.Key))
                {
                    target.Orders.Remove(order.Key);
                }
                target.Orders.Add(order.Key, order.Value);
            }
        }

        private static void MergeStatistics(Result target, Result source)
        {
            foreach (var x in source.Statistics)
            {
                if (target.Statistics.ContainsKey(x.Key))
                {
                    target.Statistics[x.Key] = x.Value;
                }
                else
                {
                    target.Statistics.Add(x.Key, x.Value);
                }
            }

            foreach (var x in source.RuntimeStatistics)
            {
                if (target.RuntimeStatistics.ContainsKey(x.Key))
                {
                    target.RuntimeStatistics[x.Key] = x.Value;
                }
                else
                {
                    target.RuntimeStatistics.Add(x.Key, x.Value);
                }
            }
        }

        private static void UpdateProfitLoss(Result target, Result source)
        {
            foreach (var x in source.ProfitLoss)
            {
                target.ProfitLoss[x.Key] = x.Value;
            }
        }
    }
}