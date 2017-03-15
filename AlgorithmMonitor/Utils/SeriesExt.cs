using System.Linq;
using QuantConnect.Lean.Monitor.Model.Charting;

namespace QuantConnect.Lean.Monitor.Utils
{
    public static class SeriesExt
    {
        public static Series Since(this Series series, TimeStamp x)
        {
            // Create a new empty series based upon the source settings
            var copy = new Series(series.Name, series.SeriesType, series.Index, series.Unit)
            {
                Color = series.Color,
                ScatterMarkerSymbol = series.ScatterMarkerSymbol
            };

            // Add all values since the provided timestamp
            copy.Values.AddRange(series.Values.OrderBy(cp => cp.x).SkipWhile(cp => cp.x <= x.ElapsedSeconds));

            return copy;
        }
    }
}