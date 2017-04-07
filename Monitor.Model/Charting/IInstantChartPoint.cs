using NodaTime;

namespace Monitor.Model.Charting
{
    public interface IInstantChartPoint
    {
        Instant X { get; set; }
    }
}