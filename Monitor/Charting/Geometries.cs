using System.Windows.Media;

namespace Monitor.Charting
{
    public class Geometries
    {
        public static Geometry TriangleDown
        {
            get
            {
                var geometry = Geometry.Parse("M0,2L1,3 -1,3 0,2z");
                geometry.Freeze();
                return geometry;
            }
        }
    }
}