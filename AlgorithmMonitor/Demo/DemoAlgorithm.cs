using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Indicators;

namespace DemoAlgorithm
{
    /// <summary>
    /// The Algorithm used to generate the DemoAlgorithm.json
    /// </summary>
    public class DemoAlgorithm : QCAlgorithm
    {
        // These are Amsterdam stocks
        private string[] _symbols = { "AALB", "NN", "WKL" };

        private Dictionary<string, RelativeStrengthIndex> _relativeStrengthIndices = new Dictionary<string, RelativeStrengthIndex>();
        private Dictionary<string, AroonOscillator> _aroonOscillators = new Dictionary<string, AroonOscillator>();

        public override void Initialize()
        {
            // Configure the backtest
            SetStartDate(2016, 03, 01);
            SetEndDate(2016, 10, 31);
            SetCash(100000);

            foreach (var symbol in _symbols)
            {
                InitializeSymbol(symbol);
            }

            // Use the first stock as benchmark
            SetBenchmark("NN");
        }

        private void InitializeSymbol(string symbol)
        {
            // Add the symbol
            AddEquity(symbol, Resolution.Hour);

            // Create RSI
            _relativeStrengthIndices[symbol] = RSI(symbol, 14, MovingAverageType.Wilders);
            _aroonOscillators[symbol] = AROON(symbol, 14);

            // Create chart and configure stockvalue on second index
            var chart = new Chart(symbol);

            // Change appearance of aroon
            var aroonSeries = new Series(_aroonOscillators[symbol].Name, SeriesType.Line, 1)
            {
                Color = Color.Coral,
                ScatterMarkerSymbol = ScatterMarkerSymbol.Diamond
            };
            chart.AddSeries(aroonSeries);

            // Change appearance of RSI
            // Change for GOOG: RSI to a 3rd chart instead of the 2nd as demo
            var rsiSeries = new Series(_relativeStrengthIndices[symbol].Name, SeriesType.Line, symbol == "AALB" ? 2 : 1)
            {
                Color = Color.Aquamarine,
                ScatterMarkerSymbol = ScatterMarkerSymbol.Square
            };
            chart.AddSeries(rsiSeries);

            // Add the chart
            AddChart(chart);
        }

        public override void OnData(Slice slice)
        {
            foreach (var symbol in _symbols)
            {
                // Plot the charts
                var aroon = _aroonOscillators[symbol];
                if (aroon.IsReady) Plot(symbol, aroon);

                var rsi = _relativeStrengthIndices[symbol];
                if (rsi.IsReady) Plot(symbol, rsi);

                if (slice.ContainsKey(symbol))
                {
                    var value = slice[symbol];
                    Plot(symbol, symbol, value.Price);

                    // Logic is bad for wallet, but does show nice chart data
                    if (aroon > 60)
                    {
                        Buy(symbol, 10);
                    }
                    else if (aroon < 30)
                    {
                        Sell(symbol, 10);
                    }
                }
            }
        }
    }
}
