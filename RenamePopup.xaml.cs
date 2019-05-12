using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VGRAPH
{
    /// <summary>
    /// Interaction logic for RenamePopup.xaml
    /// </summary>
    public partial class RenamePopup : UserControl
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public Popup popupParent;
        public object parentContent;
        private bool responded = false;
        private string content; 
        internal RenamePopup(Vertice vertice,Popup popupParent)
        {
            InitializeComponent();
            this.popupParent = popupParent;
            nameTxt.Text= vertice.Content.ToString();
            content = vertice.Content.ToString();
            okBtn.Click += (s, e) =>
            {
                content = nameTxt.Text;
                responded = true;
            };
            popupParent.Closed += Popup_Closed;
        }
        internal RenamePopup(string Text, Popup popupParent)
        {
            InitializeComponent();
            this.popupParent = popupParent;
            nameTxt.Text =Text;
            content = Text;

            okBtn.Click += (s, e) =>
            {
                content = nameTxt.Text;
                this.responded = true;
            };
            popupParent.Closed += Popup_Closed;
        }
        
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            closePopupParent();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            closePopupParent();
        }
        private void closePopupParent()
        {
            if (popupParent == null) return;
            responded = true;
            popupParent.IsOpen = false;
            
        }
        private void Popup_Closed(object sender, EventArgs e)
        {
            responded =true;
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!responded)
            {
                Thread.Sleep(100);
                //Do nothing while waiting for the user to respond
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
        }
        internal void Rename()
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }
        public Task<string> AwaitResult()
        {
            return Task.Run(() =>
            {
                while (!responded )
                {
                    Thread.Sleep(100);
                    //Do nothing while waiting for the user to respond
                }
                return content;
            });
        }
    }
}
