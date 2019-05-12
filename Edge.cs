using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VGRAPH
{
    public class Edge : DependencyObject
    {
        public static DependencyProperty LineTypeProperty =
           DependencyProperty.Register("LineType", typeof(string), typeof(Edge), new FrameworkPropertyMetadata("line", null));
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        DrawingBoard parent;
        protected internal bool isEdgeAdded_FiredBefore = false;
        public DrawingBoard Parent
        {
            get { return this.parent; }
            set
            {
                if (value == null)
                {
                    this.parent = value;
                }
                else
                {
                    if (value != parent) this.RemoveVisualizeFromParent();
                    this.parent = value;
                }
            }
        }

        public Vertice Start { get; set; }
        public Vertice End { get; set; }
        public Path Line { get; set; }
        public Polygon Arrow { get; set; }

        public string LineType { get { return (string)this.GetValue(LineTypeProperty); } set { this.SetValue(LineTypeProperty, value); } }
        private string text = "";
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (label != null) label.Content = value;
                if (value == "" || value == null) label.Visibility = Visibility.Collapsed;
                else
                {
                    if (label == null) InitilizeLabel();
                    if (label != null) label.Visibility = Visibility.Visible;
                }
            }
        }
        private EdgeLabel label;
        private Brush color;
        public Brush Color
        {
            get { return this.color; }
            set
            {
                if (this.Line != null) this.Line.Stroke = value;
                if (this.Arrow != null) { this.Arrow.Fill = value; this.Arrow.Stroke = value; }
                this.color = value;
            }
        }
        private double strokeThickness;
        public double StrokeThickness
        {
            get { return this.strokeThickness; }
            set
            {
                if (this.Line != null) this.Line.StrokeThickness = value;
                if (this.Arrow != null) this.Arrow.StrokeThickness = value;
                this.strokeThickness = value;
            }
        }
        public Edge()
        {
            this.Color = DrawingBoard.GetBrushColor("#3498db");
            this.StrokeThickness = 2;
        }
        protected internal void InitilizeEdge()
        {
            Path pp = new Path();
            MultiBinding mb = new MultiBinding();
            Binding xHead = new Binding("LeftCanvas");
            xHead.Source = Start;
            xHead.Mode = BindingMode.TwoWay;
            Binding yHead = new Binding("TopCanvas");
            yHead.Source = Start;
            yHead.Mode = BindingMode.TwoWay;

            Point p1 = new Point(System.Windows.Controls.Canvas.GetLeft(End), System.Windows.Controls.Canvas.GetTop(End));
            Point p3 = new Point(System.Windows.Controls.Canvas.GetLeft(Start), System.Windows.Controls.Canvas.GetTop(Start));
            Point p2 = new Point((p1.X + p3.X) / 2, (p1.Y + p3.Y));
            BezierSegment sg = new BezierSegment(p1, p2, p3, true);
            PathFigure fg = new PathFigure();
            fg.StartPoint = p1; fg.IsClosed = false;
            fg.Segments.Add(sg);
            pp.Data = new PathGeometry(new PathFigure[] { fg });///
            //this is for tail
            Binding xTail = new Binding("LeftCanvas");
            xTail.Source = End;
            xTail.Mode = BindingMode.TwoWay;
            Binding yTail = new Binding("TopCanvas");
            yTail.Source = End;
            yTail.Mode = BindingMode.TwoWay;
            //
            this.Line = pp;
            Binding typeBinding = new Binding("LineType");
            typeBinding.Source = this;
            typeBinding.Mode = BindingMode.TwoWay;
            mb.Bindings.Add(xHead); mb.Bindings.Add(yHead);
            mb.Bindings.Add(xTail); mb.Bindings.Add(yTail);
            mb.Bindings.Add(typeBinding);
            if (Start == End)
            {
                mb.Converter = new CircuitConveter();
            }
            else
            {
                mb.Converter = new BezierConveter();
            }

            pp.SetBinding(Path.DataProperty, mb);
            this.Line.Stroke = this.Color;
            Canvas.SetZIndex(this.Line, 1);
            InitilizeArrow(xHead, yHead, xTail, yTail);
            InitilizeLabel();
        }
        public void AddVisualizeToParent()
        {
            if (Parent == null) return;
            this.InitilizeEdge();
            this.Parent.Children.Add(this.Line);
            this.Parent.Children.Add(this.Arrow);
            this.parent.Children.Add(this.label);

        }
        void InitilizeArrow(Binding xHead, Binding yHead, Binding xTail, Binding yTail)
        {
            Polygon arrowHead = new Polygon();
            arrowHead.Stroke = this.Color;
            arrowHead.Fill = this.Color;
            this.Arrow = arrowHead;
            System.Windows.Controls.Canvas.SetZIndex(arrowHead, 1);
            //binding arrow points to position of tailBtn
            MultiBinding bnd = new MultiBinding();
            bnd.Bindings.Add(xHead); bnd.Bindings.Add(yHead);
            bnd.Bindings.Add(xTail); bnd.Bindings.Add(yTail);
            Binding bbn = new Binding(Path.DataProperty.Name);
            bbn.Source = this.Line;
            bbn.Mode = BindingMode.TwoWay;
            bnd.Bindings.Add(bbn);
            bnd.Converter = new ArrowConveter();
            Canvas.SetZIndex(this.Arrow, 1);
            this.Arrow.ContextMenu = AddContextMenuToLine();
            this.Line.ContextMenu = AddContextMenuToArrow();
            arrowHead.SetBinding(Polygon.PointsProperty, bnd);

        }
        void InitilizeLabel()
        {

            if (parent == null || Start == null || End == null)
                return;
            this.label = new EdgeLabel();
            label.Content = this.Text;
            label.Background = GetBrushColor("#3498db");
            Binding xTail1 = new Binding("LeftCanvas");
            xTail1.Source = End;
            xTail1.Mode = BindingMode.TwoWay;
            Binding yTail1 = new Binding("TopCanvas");
            yTail1.Source = End;
            yTail1.Mode = BindingMode.TwoWay;
            Binding xHead1 = new Binding("LeftCanvas");
            xHead1.Source = Start;
            xHead1.Mode = BindingMode.TwoWay;
            Binding yHead1 = new Binding("TopCanvas");
            yHead1.Source = Start;
            yHead1.Mode = BindingMode.TwoWay;
            Binding xlinetype = new Binding("LineType");
            xlinetype.Source = this;
            xlinetype.Mode = BindingMode.TwoWay;
            MultiBinding xlabBinding = new MultiBinding();
            xlabBinding.Bindings.Add(xHead1);
            xlabBinding.Bindings.Add(xTail1);
            xlabBinding.Bindings.Add(xlinetype);

            Binding ylinetype = new Binding("LineType");
            ylinetype.Source = this;
            ylinetype.Mode = BindingMode.TwoWay;
            MultiBinding ylabBinding = new MultiBinding();

            ylabBinding.Bindings.Add(yHead1);
            ylabBinding.Bindings.Add(yTail1);
            ylabBinding.Bindings.Add(ylinetype);
            if (Start == End)
            {
                xlabBinding.Converter = new XCircLabelConveter();
                ylabBinding.Converter = new YCircLabelConveter();
            }
            else
            {
                xlabBinding.Converter = new XLabelConveter();
                ylabBinding.Converter = new YLabelConveter();
            }
            label.SetBinding(Canvas.LeftProperty, xlabBinding);
            label.SetBinding(Canvas.TopProperty, ylabBinding);
            AddContextMenuToLabel();
            if (this.Text == null || this.Text == "")
                label.Visibility = Visibility.Collapsed;
            else label.Content = this.Text;
        }
        internal void RemoveVisualizeFromParent()
        {
            if (Parent == null) return;
            this.Parent.Children.Remove(this.Line);
            this.Parent.Children.Remove(this.Arrow);
            this.Parent.Children.Remove(this.label);
            this.Parent = null;
            this.DisposeVisualEdge();
        }
        internal async void Rename()
        {
            this.Text = await ShowRenameDialogue();
            if (!isEdgeAdded_FiredBefore)
            {
                AddVisualizeToParent();
                Parent.OnEdgeAdded(new EdgeEventArgs(this));
                this.isEdgeAdded_FiredBefore = true;
            }

        }
        public async System.Threading.Tasks.Task<string> ShowRenameDialogue()
        {

            label.renamePopup = new Popup();
            RenamePopup rp = new RenamePopup(this.Text, label.renamePopup);
            label.renamePopup.Child = rp; label.renamePopup.PlacementTarget = label; label.renamePopup.AllowsTransparency = true; label.renamePopup.Placement = PlacementMode.Center;
            label.renamePopup.IsOpen = true;

            return await rp.AwaitResult();

        }
        ContextMenu AddContextMenuToLine()
        {
            ContextMenu mn = new ContextMenu();
            MenuItem renameItem = new MenuItem();
            renameItem.Header = "change the value";
            renameItem.Click += (sd, f) =>
            {

                Rename();


            };
            mn.Items.Add(renameItem);
            mn.Items.Add(ContextMenuItemDelete());
            return mn;
        }
        ContextMenu AddContextMenuToArrow()
        {
            ContextMenu mn = new ContextMenu();
            MenuItem renameItem = new MenuItem();
            renameItem.Header = "change the value";
            renameItem.Click += (sd, f) =>
            {

                Rename();


            };
            mn.Items.Add(renameItem);
            mn.Items.Add(ContextMenuItemDelete());
            return mn;
        }
        void AddContextMenuToLabel()
        {
            if (this.label == null) return;
            ContextMenu mn = new ContextMenu();
            MenuItem renameItem = new MenuItem();
            renameItem.Header = "change the value";
            renameItem.Click += (sd, f) =>
            {

                Rename();


            };
            mn.Items.Add(renameItem);
            mn.Items.Add(ContextMenuItemDelete());
            label.ContextMenu = mn;
        }
        MenuItem ContextMenuItemDelete()
        {
            MenuItem item = new MenuItem();
            item.Header = "supprimer";
            item.Click += (sd, f) =>
            {
                parent.RemoveEdge(this);
            };
            return item;
        }
        SolidColorBrush GetBrushColor(string hexCode)
        {
            BrushConverter bc = new BrushConverter();
            SolidColorBrush s = (SolidColorBrush)(bc.ConvertFrom(hexCode));
            return s;
        }
        void DisposeVisualEdge()
        {
            this.Line = null;
            this.Arrow = null;
            this.label = null;
        }
    }
}
