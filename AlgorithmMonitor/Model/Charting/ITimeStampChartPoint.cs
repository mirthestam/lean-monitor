using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.Model.Charting
{
    public interface ITimeStampChartPoint
    {
        TimeStamp X { get; set; }
    }
}