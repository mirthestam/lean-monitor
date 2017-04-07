using System;
using Monitor.Model;
using Monitor.Model.Charting;
using QuantConnect;

namespace Monitor.Charting
{
    public class OhlcInstantChartPointMapper : FinancialMapperBase<OhlcInstantChartPoint>
    {
        public OhlcInstantChartPointMapper(IResolutionSource source) : base(source)
        {
            X(m =>
            {
                switch (source.Resolution)
                {
                    case Resolution.Tick:
                        return m.X.ToUnixTimeTicks();

                    case Resolution.Second:
                        return m.X.ToUnixTimeSeconds();
                        
                    case Resolution.Minute:
                        return m.X.ToUnixTimeSeconds() / 60;
                        
                    case Resolution.Hour:
                        return m.X.ToUnixTimeSeconds() / 60 / 60;
                        
                    case Resolution.Daily:
                        return m.X.ToUnixTimeSeconds() / 60 /60 / 24;
                    
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