using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointOnGeometryDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown -= MainWindow_MouseLeftButtonDownForClosedPoint;
            this.MouseLeftButtonDown -= MainWindow_MouseLeftButtonDownForOrdinate;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDownForOrdinate;
        }

        #region 曲线上的点-获取指定横坐标对应的纵坐标值 
        private void OrdinateComboBoxItem_OnSelected(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown -= MainWindow_MouseLeftButtonDownForClosedPoint;
            this.MouseLeftButtonDown -= MainWindow_MouseLeftButtonDownForOrdinate;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDownForOrdinate;
        }

        private void MainWindow_MouseLeftButtonDownForOrdinate(object sender, MouseButtonEventArgs e)
        {
            var ellipses = ContentCanvas.Children.OfType<Ellipse>().ToList();
            foreach (var ellipse in ellipses)
            {
                ContentCanvas.Children.Remove(ellipse);
            }

            Point p = e.GetPosition(ContentCanvas);
            var pointsOnPath = GetPointsOnPath(p.X, GeometryPath.Data);
            foreach (var point in pointsOnPath)
            {
                var ellipse = new Ellipse()
                {
                    Width = 10,
                    Height = 10,
                    Margin = new Thickness(-5, -5, 0, 0),
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(ellipse, point.X);
                Canvas.SetTop(ellipse, point.Y);
                ContentCanvas.Children.Add(ellipse);
            }
        }
        /// <summary>
        /// 获取曲线上的纵坐标
        /// </summary>
        /// <param name="abscissa"></param>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public List<Point> GetPointsOnPath(double abscissa, Geometry geometry)
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
        private List<Point> GetOrdinateOnPathFigureByAbscissa(PathFigure figure, double abscissa)
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
                    //var ellipse = new Ellipse()
                    //{
                    //    Width = 6,
                    //    Height = 6,
                    //    Margin = new Thickness(-3,-3,0,0),
                    //    Fill = Brushes.Blue
                    //};
                    //Canvas.SetTop(ellipse, next.Y);
                    //Canvas.SetLeft(ellipse, next.X);
                    //ContentCanvas.Children.Add(ellipse);
                    if (TryGetOrdinateOnVectorByAbscissa(current, next, abscissa, out double ordinate))
                    {
                        outputPoints.Add(new Point(abscissa, ordinate));
                    }
                    current = next;
                }
            }
            return outputPoints;
        }
        private bool TryGetOrdinateOnVectorByAbscissa(Point start, Point end, double abscissa, out double ordinate)
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

        #region 曲线上的点-获取最近的点

        private void ClosedPointComboBoxItem_OnSelected(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown -= MainWindow_MouseLeftButtonDownForOrdinate;
            this.MouseLeftButtonDown -= MainWindow_MouseLeftButtonDownForClosedPoint;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDownForClosedPoint;
        }

        private void MainWindow_MouseLeftButtonDownForClosedPoint(object sender, MouseButtonEventArgs e)
        {
            var ellipses = ContentCanvas.Children.OfType<Ellipse>().ToList();
            foreach (var ellipse in ellipses)
            {
                ContentCanvas.Children.Remove(ellipse);
            }

            Point p = e.GetPosition(ContentCanvas);
            var pointsOnPath = PointOnGeometryHelper.GetClosestPointOnPath(p, GeometryPath.Data);
            var newEllipse = new Ellipse()
            {
                Width = 10,
                Height = 10,
                Margin = new Thickness(-5, -5, 0, 0),
                Fill = Brushes.Red
            };
            Canvas.SetLeft(newEllipse, pointsOnPath.X);
            Canvas.SetTop(newEllipse, pointsOnPath.Y);
            ContentCanvas.Children.Add(newEllipse);
        }

        #endregion

    }
}
