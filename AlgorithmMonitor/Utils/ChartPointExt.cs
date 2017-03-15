using QuantConnect.Lean.Monitor.Model.Charting;

namespace QuantConnect.Lean.Monitor.Utils
{
    public static class ChartPointExt
    {
        public static TimeStampChartPoint ToTimeStampChartPoint(this ChartPoint point)
        {
            return new TimeStampChartPoint
            {                
                // QuantConnect chartpoints are always in Unix TimeStamp
                X = TimeStamp.FromSeconds(point.x),
                Y = point.y
            };
        }
    }
}