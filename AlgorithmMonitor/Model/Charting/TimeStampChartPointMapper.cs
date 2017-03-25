using System;
using LiveCharts.Configurations;

namespace Monitor.Model.Charting
{
    public class TimeStampChartPointMapper : CartesianMapperBase<TimeStampChartPoint>
    {
        public TimeStampChartPointMapper(IResolutionProvider source) : base(source)
        {        
            X(m =>
            {
                switch (source.Resolution)
                {
                    case Resolution.Day:
                        return m.X.ElapsedTicks / TimeSpan.TicksPerDay;

                    case Resolution.Hour:
                        return m.X.ElapsedTicks / TimeSpan.TicksPerHour;

                    case Resolution.Minute:
                        return m.X.ElapsedTicks / TimeSpan.TicksPerMinute;

                    case Resolution.Second:
                        return m.X.ElapsedTicks / TimeSpan.TicksPerSecond;

                    default:
                        return m.X.ElapsedTicks;
                }
            });

            Y(m => (double)m.Y);
        }
    }
}