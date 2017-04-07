using LiveCharts.Configurations;
using Monitor.Model;

namespace Monitor.Charting
{
    public abstract class FinancialMapperBase<T> : FinancialMapper<T>
    {
        protected IResolutionSource Source;

        protected FinancialMapperBase(IResolutionSource source)
        {
            Source = source;
        }
    }
}