using NodaTime;

namespace Monitor.Model.Charting
{
    public struct InstantChartPoint : IInstantChartPoint
    {
        public Instant X { get; set; }

        public decimal Y { get; set; }

        public InstantChartPoint(Instant x, decimal y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X} - {Y}";
        }
    }
}