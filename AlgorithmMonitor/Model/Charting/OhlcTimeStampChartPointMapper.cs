using System;
using LiveCharts.Configurations;
using QuantConnect.Data.Market;

namespace Monitor.Model.Charting
{
    public class OhlcTimeStampChartPointMapper : FinancialMapperBase<TimeStampOhlcChartPoint>
    {
        public OhlcTimeStampChartPointMapper(IResolutionProvider source) : base(source)
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
            Open(m => m.Open);
            Close(m => m.Close);
            High(m => m.High);
            Low(m => m.Low);

        }
    }
}