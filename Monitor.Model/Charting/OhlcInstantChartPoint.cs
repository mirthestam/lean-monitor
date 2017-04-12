using NodaTime;

namespace Monitor.Model.Charting
{
    public class OhlcInstantChartPoint : IInstantChartPoint
    {
        public Instant X { get; set; }
        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }

        public override string ToString()
        {
            return $"{X} O:{Open} H:{High} L:{Low} C:{Close}";
        }
    }
}