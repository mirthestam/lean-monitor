using Monitor.Utils;

namespace Monitor.Model.Charting
{
    public struct TimeStampOhlcChartPoint : ITimeStampChartPoint
    {
        public TimeStamp X { get; set; }
        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }

        public override string ToString()
        {
            return $"{X.DateTime.ToShortDateString()} O:{Open} H:{High} L:{Low} C:{Close}";
        }
    }
}