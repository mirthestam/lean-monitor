using System.Linq;
using QuantConnect.Lean.Monitor.Utils;
using QuantConnect.Lean.Monitor.ViewModel.Charts;

namespace QuantConnect.Lean.Monitor.ViewModel.Grids
{
    /// <summary>
    /// View model for generic chart grid
    /// </summary>
    public class GridPanelViewModel : GridPanelViewModelBase, IChartParser
    {
        // Allow this tab to be closed. It can be reopened using the chart tab
        public override bool CanClose => true;

        public void ParseChart(Chart chart)
        {
            if (chart.Series.Count == 0) return;

            // Modify our tab titel to include the Grid tab
            Title = $"{chart.Name} [Grid]";

            // Put all series into table dataholders containing all the points
            Series = chart.Series.Values.Select(s =>
            {
                var holder = new GridSerie { Name = s.Name };
                holder.AddRange(s.Values.Select(p => new GridPoint
                {
                    // Use the actual timestamp as the X value
                    X = DateTimeExt.FromTimeStamp(p.x).ToString("yy-MM-dd HH:mm:ss"),
                    Y = p.y
                }));
                return holder;
            }).ToList();

            SelectedSeries = Series.First();
        }
    }
}