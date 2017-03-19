using Monitor.Utils;

namespace Monitor.Model.Charting
{
    public struct TimeStampChartPoint : ITimeStampChartPoint
    {
        public TimeStamp X { get; set; }

        public decimal Y { get; set; }

        public TimeStampChartPoint(TimeStamp x, decimal y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X.DateTime:o} - {Y}";
        }
    }
}