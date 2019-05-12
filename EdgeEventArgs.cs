using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGRAPH
{
    public class EdgeEventArgs
    {
        public Edge edge { get; set; }
        public EdgeEventArgs(Edge edge)
        {
            this.edge = edge;
        }
    }
}
