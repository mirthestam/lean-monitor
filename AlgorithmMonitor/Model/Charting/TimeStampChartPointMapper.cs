namespace QuantConnect.Lean.Monitor.Model.Charting
{
    public class TimeStampChartPointMapper : CartesianMapperBase<TimeStampChartPoint>
    {
        public TimeStampChartPointMapper(ITimeStampSource source) : base(source)
        {
            // Ask our TimeStamp source for the index of this TimeStamp
            X(m => source.IndexOf(m.X));
            Y(m => (double)m.Y);
        }
    }
}