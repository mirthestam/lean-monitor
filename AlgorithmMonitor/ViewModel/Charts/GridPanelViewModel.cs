using System.Linq;
using QuantConnect.Lean.Monitor.Model.Charting;
using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.ViewModel.Charts
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
                    X = DateTimeExt.FromTimeStamp(p.x).ToString("yy-MM-dd HH:mm"),
                    Y = p.y
                }));
                return holder;
            }).ToList();

            SelectedSeries = Series.First();
        }

        protected override TimeStamp GetXTimeStamp(int index)
        {
            // TODO: Review whether this is needed for grid panels
            return TimeStamp.FromDays(index);
        }
    }
}