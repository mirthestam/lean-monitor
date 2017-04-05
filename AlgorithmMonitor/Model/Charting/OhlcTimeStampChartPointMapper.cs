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
                    case Resolution.Ticks:
                        return m.X.ElapsedTicks;
                        
                    case Resolution.Second:
                        return m.X.ElapsedSeconds;
                        
                    case Resolution.Minute:
                        return m.X.ElapsedMinutes;
                        
                    case Resolution.Hour:
                        return m.X.ElapsedHours;
                        
                    case Resolution.Day:
                        return m.X.ElapsedDays;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });            
            Open(m => m.Open);
            Close(m => m.Close);
            High(m => m.High);
            Low(m => m.Low);

        }
    }
}