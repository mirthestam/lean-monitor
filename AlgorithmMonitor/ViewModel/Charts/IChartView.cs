using Monitor.Model.Charting;

namespace Monitor.ViewModel.Charts
{
    public interface IChartView
    {
        void ParseChart(ChartDefinition chart);
    }
}