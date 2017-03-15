namespace QuantConnect.Lean.Monitor.ViewModel.Charts
{
    public interface IChartParser
    {
        void ParseChart(Chart chart);
    }
}