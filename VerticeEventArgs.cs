using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGRAPH
{
    public class VerticeEventArgs:EventArgs
    {
        public Vertice vertice { get; set; }
        public VerticeEventArgs(Vertice vertice)
        {
            this.vertice = vertice;
        }
    }
}
