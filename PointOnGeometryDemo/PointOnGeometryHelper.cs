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
                    throw new InvalidOperationException("Unexpected segment type");
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
            if ((start.X < end.X && abscissa > start.X && abscissa < end.X) ||
                (start.X > end.X && abscissa < start.X && abscissa > end.X))
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

    }
}
