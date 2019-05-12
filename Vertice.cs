using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace VGRAPH
{
    public  class Vertice : Button
    {
        internal Popup renamePopup { get; set; }
        string verticeTemplate = @"<ControlTemplate 
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        xmlns:local='clr-namespace:VGRAPH'
     x:Key='buttonTemplate' >
<Grid><Ellipse Fill = '{TemplateBinding Background}' Stroke='{TemplateBinding BorderBrush}' StrokeThickness='1'></Ellipse><Label HorizontalContentAlignment = 'Center' VerticalContentAlignment='Center' Content='{TemplateBinding Content }'></Label></Grid></ControlTemplate>";

        public static DependencyProperty LeftCanvasProperty =
            DependencyProperty.Register("LeftCanvas", typeof(double), typeof(Vertice), new FrameworkPropertyMetadata(20.0, null));
        public static DependencyProperty TopCanvasProperty =
           DependencyProperty.Register("TopCanvas", typeof(double), typeof(Vertice), new FrameworkPropertyMetadata(90.0, null));
        string text;
         public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                this.Content = value;
            }
        }
        double x;
        public double X
        {
            get { return this.x; }
            set
            {
                this.SetCanvasLeft(value);
            }
        }
        double y;
        public double Y
        {
            get { return this.y; }
            set
            {
                this.SetCanvasTop(value);
            }
        }
        private bool isSelected;

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value == true) base.Background =SelectionColor ;
                else { base.Background = Color; }
                isSelected = value;
            }
        }
        private Brush color;
        public Brush Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                base.Background = value;
            }
        }

        public Brush SelectionColor
        {
            get; set;
        } = DrawingBoard.GetBrushColor("#1abc9c");
        public Vertice() : base()
        {
           
            Canvas.SetZIndex(this, 2);
            ControlTemplate t;
            using (var stringReader = new StringReader(verticeTemplate))
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            {
                t= (ControlTemplate)XamlReader.Load(xmlReader);
                t.TargetType = typeof(Vertice);
                this.Template = t;
                this.Loaded += (ei, s) =>
                {

                    if (this.Style == null)
                        this.Style = DefaultStyle();
                    this.Color = base.Background;

                };
            }
            this.AddContextMenu();
        }
        Style DefaultStyle()
        {
            Style s = new Style(typeof(Vertice));
            Setter set1 = new Setter(BackgroundProperty, Brushes.White);
            Setter set2 = new Setter(BorderBrushProperty,DrawingBoard.GetBrushColor("#3498db"));
            Setter set3 = new Setter(MinHeightProperty, 30.0);
            Setter set4 = new Setter(MinWidthProperty, 30.0);
            s.Setters.Add(set1);
            s.Setters.Add(set2);
            s.Setters.Add(set3);
            s.Setters.Add(set4);
            return s;
        }
       
         void SetCanvasLeft(double p)
        {
            Canvas.SetLeft(this, p);
            this.SetValue(LeftCanvasProperty, p);
            this.x = p;

        }
         void SetCanvasTop(double p)
        {
            Canvas.SetTop(this, p);
            this.SetValue(TopCanvasProperty, p);
            this.y = p;
        }
        
        void AddContextMenu()
        {
            ContextMenu mn = new ContextMenu();
            MenuItem renameItem = new MenuItem();
            renameItem.Header = "rename";
            renameItem.Click += async (sd, f) =>
            {
                this.Text = await ShowRenameDialogue();
            };
            mn.Items.Add(renameItem);
            this.ContextMenu = mn;
        }
        public  async System.Threading.Tasks.Task<string>  ShowRenameDialogue()
        {
            this.renamePopup = new Popup();
            RenamePopup rp = new RenamePopup(this, this.renamePopup);
            this.renamePopup.Child = rp;
            this.renamePopup.PlacementTarget = this; this.renamePopup.AllowsTransparency = true;
            this.renamePopup.StaysOpen = true;
            this.renamePopup.Placement = PlacementMode.Center;
            this.renamePopup.IsOpen = true;
            return await rp.AwaitResult();
        } 
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    
}
