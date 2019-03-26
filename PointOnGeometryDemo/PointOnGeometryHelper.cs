using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PointOnGeometryDemo
{
    /// <summary>
    /// 曲线上的点帮助类
    /// </summary>
    public static class PointOnGeometryHelper
    {

        #region 获取曲线上的纵坐标

        /// <summary>
        /// 获取曲线上的纵坐标
        /// </summary>
        /// <param name="abscissa"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static List<Point> GetPointsOnPath(double abscissa, Geometry geometry)
        {
            List<Point> points = new List<Point>();
            PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry();
            foreach (var figure in pathGeometry.Figures)
            {
                var ordinateOnPathFigureByAbscissa = GetOrdinateOnPathFigureByAbscissa(figure, abscissa);
                points.AddRange(ordinateOnPathFigureByAbscissa);
            }
            return points;
        }
        private static List<Point> GetOrdinateOnPathFigureByAbscissa(PathFigure figure, double abscissa)
        {
            List<Point> outputPoints = new List<Point>();
            Point current = figure.StartPoint;
            foreach (PathSegment s in figure.Segments)
            {
                PolyLineSegment segment = s as PolyLineSegment;
                LineSegment line = s as LineSegment;
                Point[] points;
                if (segment != null)
                {
                    points = segment.Points.ToArray();
                }
                else if (line != null)
                {
                    points = new[] { line.Point };
                }
                else
                {
                    throw new InvalidOperationException();
                }
                foreach (Point next in points)
                {
                    if (TryGetOrdinateOnVectorByAbscissa(current, next, abscissa, out double ordinate))
                    {
                        outputPoints.Add(new Point(abscissa, ordinate));
                    }
                    current = next;
                }
            }
            return outputPoints;
        }
        private static bool TryGetOrdinateOnVectorByAbscissa(Point start, Point end, double abscissa, out double ordinate)
        {
            ordinate = 0.0;
            if ((start.X < end.X && abscissa >= start.X && abscissa <= end.X) ||
                (start.X > end.X && abscissa <= start.X && abscissa >= end.X))
            {
                var xRatio = (abscissa - start.X) / (end.X - start.X);
                var yLength = end.Y - start.Y;
                var y = yLength * xRatio + start.Y;
                ordinate = y;
                return true;
            }
            return false;
        }

        #endregion

        #region 获取曲线上最近的点

        public static Point GetClosestPointOnPath(Point p, Geometry geometry)
        {
            PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry();

            var points = pathGeometry.Figures.Select(f => GetClosestPointOnPathFigure(f, p))
                .OrderBy(t => t.Item2).FirstOrDefault();
            return points?.Item1 ?? new Point(0, 0);
        }

        private static Tuple<Point, double> GetClosestPointOnPathFigure(PathFigure figure, Point p)
        {
            List<Tuple<Point, double>> closePoints = new List<Tuple<Point, double>>();
            Point current = figure.StartPoint;
            foreach (PathSegment s in figure.Segments)
            {
                PolyLineSegment segment = s as PolyLineSegment;
                LineSegment line = s as LineSegment;
                Point[] points;
                if (segment != null)
                {
                    points = segment.Points.ToArray();
                }
                else if (line != null)
                {
                    points = new[] { line.Point };
                }
                else
                {
                    throw new InvalidOperationException();
                }
                foreach (Point next in points)
                {
                    Point closestPoint = GetClosestPointOnLine(current, next, p);
                    double d = (closestPoint - p).LengthSquared;
                    closePoints.Add(new Tuple<Point, double>(closestPoint, d));
                    current = next;
                }
            }
            return closePoints.OrderBy(t => t.Item2).First();
        }

        private static Point GetClosestPointOnLine(Point start, Point end, Point p)
        {
            double length = (start - end).LengthSquared;
            if (Math.Abs(length) < 0.01)
            {
                return start;
            }
            Vector v = end - start;
            double param = (p - start) * v / length;
            return (param < 0.0) ? start : (param > 1.0) ? end : (start + param * v);
        }

        #endregion

    }
}
