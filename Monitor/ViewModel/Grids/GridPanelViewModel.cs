using System.Linq;
using Monitor.Model.Charting;
using Monitor.ViewModel.Charts;

namespace Monitor.ViewModel.Grids
{
    /// <summary>
    /// View model for generic chart grid
    /// </summary>
    public class GridPanelViewModel : GridPanelViewModelBase, IChartParser
    {
        public GridPanelViewModel()
        {
            CanClose = true;
        }

        public void ParseChart(ChartDefinition chart)
        {
            if (chart.Series.Count == 0) return;
            
            // Put all series into table dataholders containing all the points
            Series = chart.Series.Values.Select(s =>
            {
                var holder = new GridSerie { Name = s.Name };
                holder.AddRange(s.Values.Select(p => new GridPoint
                {
                    // Use the actual timestamp as the X value
                    X = p.X.ToString(),
                    Y = p.Y
                }));
                return holder;
            }).ToList();

            SelectedSeries = Series.First();

            // Modify our tab titel to include the Grid tab
            ChartName = chart.Name;
        }
    }
}