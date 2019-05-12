using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGRAPH
{
    class NoEndVerticesException:Exception
    {
        public NoEndVerticesException(string message):base(message)
        {

        }
    }
    class HasAnotherParent : Exception
    {
        public HasAnotherParent(string message) : base(message)
        {

        }
    }
}
