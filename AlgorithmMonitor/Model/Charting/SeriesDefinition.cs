using System.Collections.Generic;
using System.Windows.Media;
using QuantConnect;

namespace Monitor.Model.Charting
{
    public class SeriesDefinition
    {
        public string Name { get; set; }

        public List<TimeStampChartPoint> Values { get; set; } = new List<TimeStampChartPoint>();

        public SeriesType SeriesType { get; set; } = SeriesType.Line;

        public Color Color { get; set; } = Colors.CornflowerBlue;

        public string Unit { get; set; } = "?";

        public ScatterMarkerSymbol ScatterMarkerSymbol { get; set; } = ScatterMarkerSymbol.None;

        public int Index { get; set; }
    }
}