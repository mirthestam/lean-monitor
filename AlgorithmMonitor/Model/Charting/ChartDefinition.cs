using System.Collections.Generic;

namespace Monitor.Model.Charting
{
    public class ChartDefinition
    {
        public string Name { get; set; }

        public Dictionary<string, SeriesDefinition> Series = new Dictionary<string, SeriesDefinition>();
    }
}