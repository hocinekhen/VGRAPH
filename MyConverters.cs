using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VGRAPH
{
    ///this is for Bezier Converter
   
    public class CircuitConveter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Double xHead = (Double)values[0]+15;
                Double yHead = (Double)values[1]+15;
                GeometryGroup gr = new GeometryGroup();
                PathGeometry pg2 = new PathGeometry();
                Geometry g2 = PathGeometry.Parse($"M {xHead},{yHead} C {xHead - 100},{yHead - 70} {xHead + 100},{yHead - 40} {xHead},{yHead}");
                // pg1.AddGeometry(g1);
                pg2.AddGeometry(g2);
                //gr.Children.Add(pg1);
                gr.Children.Add(pg2);
                return gr;

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return Binding.DoNothing;
            }

        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }


    }
    public class BezierConveter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {

                Double xTail = (Double)values[2];
                Double yTail = (Double)values[3];
                Double xHead = (Double)values[0];
                Double yHead = (Double)values[1];
                string type = (string)values[4];
                double width = 30;
                
                if (type == "line")
                {
                     return PathGeometry.Parse($" M {xHead + width / 2} {yHead + width / 2} L{xTail + width / 2} {yTail + width / 2}");
                }
                PathFigure fg = new PathFigure();
                Point p1 = new Point(xTail + width / 2, yTail + width / 2);
                Point p3 = new Point(xHead + width / 2, yHead + width / 2);
                fg.StartPoint = p1; fg.IsClosed = false;
                Point p2= new Point((p1.X + p3.X) / 2 + 50, (p1.Y + p3.Y) / 2 - 50); ;
                if (type=="down")
                {
                     p2 = new Point((p1.X + p3.X) / 2 -50, (p1.Y + p3.Y) / 2 + 50);                 
                }
                
                BezierSegment sg = new BezierSegment(p1, p2, p3, true);
                fg.Segments.Add(sg);
               
                
                return new PathGeometry(new PathFigure[] { fg });
            }
            catch (Exception ex)
            {
                return Binding.DoNothing;
            }

        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    //end of BezierConverter
    //ArrowConverter
    public class ArrowConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double xHead = (double)values[0]; double yHead = (double)values[1];
            double xTail = (double)values[2] ; double yTail = (double)values[3] ;
            Geometry lineData = (Geometry)values[4];           
            Rect rec = new Rect();
            rec.X = xTail-15;rec.Y = yTail-15;
            rec.Height = 50;
            rec.Width = 50;
            Rect outrec = new Rect();
            outrec.X = xTail - 17; outrec.Y = yTail - 17;
            outrec.Height = 54;
            outrec.Width = 54;
            
            try
            {
                RectangleGeometry recgeo = new RectangleGeometry(rec);
                RectangleGeometry outgeo = new RectangleGeometry(outrec);
                Point po = ((Point[])GetIntersectionPoints(lineData, recgeo))[0];
                Point outp= ((Point[])GetIntersectionPoints(lineData, outgeo))[0];
                return InternalDrawArrowGeometry(outp.X, outp.Y, po.X, po.Y, 3, 3);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        private PointCollection InternalDrawArrowGeometry(double X1, double Y1, double X2, double Y2, double HeadHeight, double HeadWidth)
        {
            double theta = Math.Atan2(Y1 - Y2, X1 - X2);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            Point ptI = new Point(X2, Y2);
            Point pt3 = new Point(
                X2 + (HeadWidth * cost - HeadHeight * sint),
                Y2 + (HeadWidth * sint + HeadHeight * cost));

            Point pt4 = new Point(
                X2 + (HeadWidth * cost + HeadHeight * sint),
                Y2 - (HeadHeight * cost - HeadWidth * sint));
            Point ptF = new Point(X2, Y2);
            PointCollection pc = new PointCollection();
            pc.Add(ptI);pc.Add(pt3); pc.Add(pt4); pc.Add(ptF);
            return pc;
        }
            public static Polygon DrawArrowHead( PathGeometry linePath, Rect shapeRect, Color color,Canvas cnv)
        {
            // Get the intersection point of the imaginary, slightly
            // larger rectangle that surrounds the targer shape.
            Rect outerRect = new Rect(shapeRect.Left - 10, shapeRect.Top - 10, shapeRect.Width + 20, shapeRect.Height + 20);

            RectangleGeometry shapeGeometry = new RectangleGeometry(shapeRect);
            Rectangle rect = new Rectangle(); rect.StrokeThickness = 5; rect.Stroke = Brushes.Orange;
            rect.RadiusX = 0; rect.RadiusY = 0; rect.Height = shapeGeometry.Bounds.Height; rect.Width = shapeGeometry.Bounds.Width;
            Canvas.SetLeft(rect, shapeGeometry.Bounds.X); Canvas.SetTop(rect, shapeGeometry.Bounds.Y);
            //cnv.Children.Add(rect);
            Point[] intersectPoints = GetIntersectionPoints(linePath, shapeGeometry);


            double innerLeft = intersectPoints[0].X;
            double innerTop = intersectPoints[0].Y;

            shapeGeometry = new RectangleGeometry(outerRect);
             rect = new Rectangle(); rect.StrokeThickness = 5; rect.Stroke = Brushes.Orange;
            rect.RadiusX = 0; rect.RadiusY = 0; rect.Height = shapeGeometry.Bounds.Height; rect.Width = shapeGeometry.Bounds.Width;
            Canvas.SetLeft(rect, shapeGeometry.Bounds.X); Canvas.SetTop(rect, shapeGeometry.Bounds.Y);
           // cnv.Children.Add(rect);
            intersectPoints = GetIntersectionPoints(linePath, shapeGeometry);

            double outerLeft = intersectPoints[0].X;
            double outerTop = intersectPoints[0].Y;

            Polygon arrowHead = new Polygon();
            arrowHead.Points = new PointCollection();
            arrowHead.Points.Add(new Point(innerLeft, innerTop));
            arrowHead.Points.Add(new Point(innerLeft + 10, innerTop + 5));
            arrowHead.Points.Add(new Point(innerLeft + 10, innerTop - 5));
            arrowHead.Points.Add(new Point(innerLeft, innerTop));
            arrowHead.Stroke = new SolidColorBrush(color);
            arrowHead.Fill = new SolidColorBrush(color);

            // The differences between the intersection points on
            // the inner and outer shapes gives us the base and
            // perpendicular of the right-angled triangle
            double baseSize = innerLeft - outerLeft;
            double perpSize = innerTop - outerTop;
            // Calculate the angle in degrees using ATan
            double angle = Math.Atan(perpSize / baseSize) * 180 / Math.PI;
            // Rotate another 180 degrees for lines in the 3rd & 4th quadrants
            if (baseSize >= 0) angle += 180;

            // Apply the rotation to the arrow head
            RotateTransform rt = new RotateTransform(angle, innerLeft, innerTop);
            arrowHead.RenderTransform = rt;
            return arrowHead;
        }
        public static  Point[] GetIntersectionPoints(Geometry g1, Geometry g2)
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            Geometry og2 = g2.GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));

            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);
            PathGeometry pg = cg.GetFlattenedPathGeometry();
            Point[] result = new Point[pg.Figures.Count];
            for (int i = 0; i < pg.Figures.Count; i++)
            {
                Rect fig = new PathGeometry(new PathFigure[] { pg.Figures[i] }).Bounds;
                result[i] = new Point(fig.Left + fig.Width / 2.0, fig.Top + fig.Height / 2.0);
            }
            return result;
        }

    }    
    //end of ArrowConverter
    public class XLabelConveter : IMultiValueConverter
    {

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                string linetype = (string)values[2];
                double xHead = (double)values[0]; double xTail = (double)values[1];
                //double xTail = (double)values[2];double yTail = (double)values[3];             
                if(linetype=="line") return (xHead + xTail) / 2;
                else if (linetype != "down")
                    return (xHead + xTail) / 2 + 20;
                else
                return (xHead + xTail) / 2 - 20;
            }
                catch (Exception )
                {
                    //MessageBox.Show(ex.Message);
                    return Binding.DoNothing;
                }
            }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class YLabelConveter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string linetype = (string)values[2];
                double yHead = (double)values[0]; double yTail = (double)values[1];
                //double xTail = (double)values[2];double yTail = (double)values[3];
                if (linetype == "up")
                    return (yHead + yTail) / 2 - 20;
                else
                    if (linetype == "line") return (yHead + yTail) / 2;
                else
                    return (yHead + yTail) / 2 + 40;

            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
                return Binding.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class XCircLabelConveter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double xHead = (double)values[0];
               
               
                    return (xHead);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return Binding.DoNothing;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class YCircLabelConveter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string linetype = (string)values[2];
                double yHead = (double)values[0]; double yTail = (double)values[1];
              return yTail-35;

            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                return Binding.DoNothing;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
