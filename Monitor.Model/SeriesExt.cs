using System.Linq;
using Monitor.Model.Charting;
using NodaTime;

namespace Monitor.Model
{
    public static class SeriesExt
    {
        public static SeriesDefinition Since(this SeriesDefinition series, Instant x)
        {
            // Create a new empty series based upon the source settings
            var copy = new SeriesDefinition
            {
                Name =  series.Name,
                SeriesType = series.SeriesType,
                Index = series.Index,
                Unit = series.Unit,
                Color = series.Color,
                ScatterMarkerSymbol = series.ScatterMarkerSymbol
            };

            // Add all values since the provided timestamp
            var newValues = series.Values
                .OrderBy(cp => cp.X)
                .SkipWhile(cp => cp.X.ToUnixTimeSeconds() <= x.ToUnixTimeSeconds()); // TODO: Can i not directly compare them?

            copy.Values.AddRange(newValues);

            return copy;
        }
    }
}