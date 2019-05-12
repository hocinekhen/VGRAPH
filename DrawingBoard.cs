    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    namespace VGRAPH
    {

    public class DrawingBoard : DraggableCanvas
    {
        private Vertice startVertice, endVertice;
        public List<Vertice> Vertices { get; set; } = new List<Vertice>();
        public List<Edge> Edges { get; set; } = new List<Edge>();
        public bool EnableShowRenameDialogue;
        private int verticeNumber = 0;
        private bool isDrawingEdge = false;
        private bool isDrawingVertice = false;
        public delegate void EdgeAddedEventHandler(object sender, EdgeEventArgs e);
        public event EdgeAddedEventHandler EdgeAdded;
        public delegate void VerticeAddedEventHandler(object sender, VerticeEventArgs e);
        public event VerticeAddedEventHandler VerticeAdded;
        public DrawingBoard()
        {
            IsDraggable = true;
            this.Background = Brushes.Transparent;

        }
        public void AddVertice(Vertice vertice)
        {
            if (vertice == null) return;
            this.Children.Add(vertice);
            if (!this.Vertices.Contains(vertice))
            {
                this.Vertices.Add(vertice);
            }
            VerticeEventArgs ve = new VerticeEventArgs(vertice);
            OnVerticeAdded(ve);
        }
        public void RemoveVertice(Vertice vertice)
        {
            this.Children.Remove(vertice);
            this.Vertices.Remove(vertice);
        }
        public void AddEdge(Edge edge)
        {
            if (edge == null) return;
            if (edge.Parent != null && edge.Parent != this)
                throw new HasAnotherParent("this Edge is a logical child of another parent");
            if (!this.Edges.Contains(edge))
            {
                if (edge.Start == null || edge.End == null)
                    throw new NoEndVerticesException("Start or End Vertice is not specified");
                edge.Parent = this;
                edge.AddVisualizeToParent();
                List<Edge> edgesWithSameEnds = (Edges.Where<Edge>(x => (x.Start == edge.Start && x.End == edge.End) || (x.Start == edge.End && x.End == edge.Start))).ToList<Edge>();
                if (edgesWithSameEnds.Count > 0)
                    edge.LineType = "down";
                this.Edges.Add(edge);
                if (!this.EnableShowRenameDialogue)
                {
                    this.OnEdgeAdded(new EdgeEventArgs(edge));
                }
                else
                    edge.Rename();
                edge.isEdgeAdded_FiredBefore = false;

            }
        }
        public void RemoveEdge(Edge edge)
        {
            if (edge == null) return;
            edge.RemoveVisualizeFromParent();
            this.Edges.Remove(edge);
        }
        public static SolidColorBrush GetBrushColor(string hexCode)
        {
            BrushConverter bc = new BrushConverter();
            SolidColorBrush s = (SolidColorBrush)(bc.ConvertFrom(hexCode));
            return s;
        }
        public void StartDrawVertice()
        {
            if (this.isDrawingVertice) return;
            this.StopDrawEdge();
            this.StartDragging();
            this.isDrawingVertice = true;
            this.PreviewMouseLeftButtonDown += DrawVertice;
        }
        public void StopDrawVertice()
        {
            this.isDrawingVertice = false;
            this.PreviewMouseLeftButtonDown -= DrawVertice;
            this.StartDragging();
        }
        public void StartDrawEdge()
        {
            if (this.isDrawingEdge) return;
            this.StopDrawVertice();
            StopDragging();
            IsDraggable = false;
            this.isDrawingEdge = true;
            foreach (Vertice item in Vertices)
            {
                item.PreviewMouseLeftButtonDown += Edge_Clicked;
            }
        }
        public void StopDrawEdge()
        {
            this.isDrawingEdge = false;
            if (startVertice != null)
                startVertice.IsSelected = false;
            startVertice = null;
            this.PreviewMouseLeftButtonDown -= DrawVertice;
            this.StartDragging();
            foreach (Vertice item in Vertices)
            {
                item.PreviewMouseLeftButtonDown -= Edge_Clicked;
            }
        }
        internal void StartDragging()
        {
            this.IsDraggable = true;
        }
        internal void StopDragging()
        {
            IsDraggable = false;
        }

        protected void DrawVertice(object sender, MouseButtonEventArgs e)
        {
            Vertice v = new Vertice();
            v.Content = this.verticeNumber;
            v.X = e.GetPosition(this).X;
            v.Y = e.GetPosition(this).Y;
            this.AddVertice(v);
            this.verticeNumber++;
        }
        private void Edge_Clicked(object sender, RoutedEventArgs e)
        {
            Vertice clickedVertice = (Vertice)sender;
            if (startVertice == null)
            {
                startVertice = clickedVertice;
                startVertice.IsSelected = true;
            }
            else
            {
                endVertice = clickedVertice;
                Edge ed = new Edge();
                ed.Start = startVertice;
                ed.End = endVertice;
                this.AddEdge(ed);
                startVertice.IsSelected = false;
                this.startVertice = null;
                this.endVertice = null;

            }
        }
        protected internal virtual void OnEdgeAdded(EdgeEventArgs e)
        {
            if (EdgeAdded != null) EdgeAdded(this, e);
        }
        protected virtual void OnVerticeAdded(VerticeEventArgs e)
        {
            if (VerticeAdded != null) VerticeAdded(this, e);
        }
    }

}
