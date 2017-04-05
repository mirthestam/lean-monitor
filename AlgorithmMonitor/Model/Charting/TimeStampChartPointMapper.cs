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
                        return m.X.ElapsedDays;

                    case Resolution.Hour:
                        return m.X.ElapsedHours;

                    case Resolution.Minute:
                        return m.X.ElapsedMinutes;

                    case Resolution.Second:
                        return m.X.ElapsedSeconds;

                    default:
                        return m.X.ElapsedTicks;
                }
            });

            Y(m => (double)m.Y);
        }
    }
}