using LiveCharts.Configurations;

namespace QuantConnect.Lean.Monitor.Model.Charting
{
    public abstract class FinancialMapperBase<T> : FinancialMapper<T>
    {
        protected ITimeStampSource Source;

        protected FinancialMapperBase(ITimeStampSource source)
        {
            Source = source;
        }
    }
}