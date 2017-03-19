namespace QuantConnect.Lean.Monitor.Model.Charting
{
    public class OhlcTimeStampChartPointMapper : FinancialMapperBase<TimeStampOhlcChartPoint>
    {
        public OhlcTimeStampChartPointMapper(ITimeStampSource source) : base(source)
        {
            X(m => source.IndexOf(m.X));
            Open(m => m.Open);
            Close(m => m.Close);
            High(m => m.High);
            Low(m => m.Low);
        }
    }
}