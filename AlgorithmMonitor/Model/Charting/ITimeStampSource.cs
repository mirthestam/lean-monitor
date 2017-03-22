using Monitor.Utils;

namespace Monitor.Model.Charting
{
    public interface ITimeStampSource
    {
        int IndexOf(TimeStamp item);
        TimeStamp GetTimeStamp(int index);
    }
}