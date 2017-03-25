using LiveCharts.Configurations;

namespace Monitor.Model.Charting
{
    public abstract class FinancialMapperBase<T> : FinancialMapper<T>
    {
        protected IResolutionProvider Source;

        protected FinancialMapperBase(IResolutionProvider source)
        {
            Source = source;
        }
    }
}