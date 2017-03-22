using Monitor.Model.Charting;

namespace Monitor.ViewModel.Charts
{
    public interface IChartParser
    {
        void ParseChart(ChartDefinition chart);
    }
}