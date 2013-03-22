using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronSharePoint
{
    interface IWrapperControl
    {
        string ControlName { get; set; }
        Exception InstantiationException { get; set; }
    }
}
