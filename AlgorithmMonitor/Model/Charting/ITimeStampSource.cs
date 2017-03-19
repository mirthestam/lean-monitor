using QuantConnect.Lean.Monitor.Utils;

namespace QuantConnect.Lean.Monitor.Model.Charting
{
    public interface ITimeStampSource
    {
        int IndexOf(TimeStamp item);
        TimeStamp GetTimeStamp(int index);
    }
}