using LiveCharts.Configurations;
using Monitor.Model;

namespace Monitor.Charting
{
    public abstract class CartesianMapperBase<T> : CartesianMapper<T>
    {
        protected IResolutionSource Source;

        protected CartesianMapperBase(IResolutionSource source)
        {
            Source = source;
        }
    }
}