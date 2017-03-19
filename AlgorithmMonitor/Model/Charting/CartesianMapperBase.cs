using LiveCharts.Configurations;

namespace Monitor.Model.Charting
{
    public abstract class CartesianMapperBase<T> : CartesianMapper<T>
    {
        protected ITimeStampSource Source;

        protected CartesianMapperBase(ITimeStampSource source)
        {
            Source = source;
        }
    }
}