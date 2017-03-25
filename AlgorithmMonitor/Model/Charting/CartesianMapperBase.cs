using LiveCharts.Configurations;

namespace Monitor.Model.Charting
{
    public abstract class CartesianMapperBase<T> : CartesianMapper<T>
    {
        protected IResolutionProvider Source;

        protected CartesianMapperBase(IResolutionProvider source)
        {
            Source = source;
        }
    }
}