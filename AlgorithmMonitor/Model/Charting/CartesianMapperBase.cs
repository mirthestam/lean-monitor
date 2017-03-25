using LiveCharts.Configurations;

namespace Monitor.Model.Charting
{
    public interface IResolutionProvider
    {
        Resolution Resolution { get; }    
    }

    public abstract class CartesianMapperBase<T> : CartesianMapper<T>
    {
        protected IResolutionProvider Source;

        protected CartesianMapperBase(IResolutionProvider source)
        {
            Source = source;
        }
    }
}