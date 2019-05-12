using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace VGRAPH
{
    public class EdgeLabel : Label
    {
        public static DependencyProperty LeftCanvasProperty =
            DependencyProperty.Register("LeftCanvas", typeof(double), typeof(EdgeLabel), new FrameworkPropertyMetadata(20.0, null));
        public static DependencyProperty TopCanvasProperty =
           DependencyProperty.Register("TopCanvas", typeof(double), typeof(EdgeLabel), new FrameworkPropertyMetadata(90.0, null));
        //hide inherited properties

        internal Popup renamePopup;
        public EdgeLabel():base()
        {
            Canvas.SetZIndex(this, 3);
            this.ContentTemplateSelector = new DataTemplateSelector();
        }
        internal void SetCanvasLeft(double p)
        {
            Canvas.SetLeft(this, p);
            //LeftCanvas = p;
            this.SetValue(LeftCanvasProperty, p);

        }

        internal void SetCanvasTop(double p)
        {
            Canvas.SetTop(this, p);
            this.SetValue(TopCanvasProperty, p);
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