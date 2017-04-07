using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LiveCharts.Geared.Geometries;

namespace Monitor.Charting
{
    public class TriangleDown : GeometryShape
    {
        private PathFigure _path;
        private LineSegment _lineA;
        private LineSegment _lineB;

        public override void Draw(PathGeometry path, Point location, bool animate, TimeSpan animationsSpeed)
        {
            if (_path == null) Initialize(path);

            var middle = Diameter / 2.0;
            if (animate)
            {
                _path.BeginAnimation(PathFigure.StartPointProperty, new PointAnimation(new Point(location.X - middle, location.Y - middle), animationsSpeed));
                _lineA.BeginAnimation(LineSegment.PointProperty, new PointAnimation(new Point(location.X + middle, location.Y - middle), animationsSpeed));
                _lineB.BeginAnimation(LineSegment.PointProperty, new PointAnimation(new Point(location.X, location.Y + middle), animationsSpeed));
            }
            else
            {
                _path.StartPoint = new Point(location.X - middle, location.Y - middle);
                _lineA.Point = new Point(location.X + middle, location.Y - middle);
                _lineB.Point = new Point(location.X, location.Y + middle);
            }
        }

        public override void Erase(PathGeometry path)
        {
            path.Figures.Remove(_path);
        }

        private void Initialize(PathGeometry path)
        {
            _lineA = new LineSegment();
            _lineB = new LineSegment();
            var pathFigure = new PathFigure
            {
                Segments = new PathSegmentCollection(3)
                {
                    _lineA,
                    _lineB
                },
                IsClosed = true
            };

            _path = pathFigure;
            path.Figures.Add(_path);
        }
    }
}
