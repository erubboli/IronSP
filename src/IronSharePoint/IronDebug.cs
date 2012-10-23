#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronSharePoint
{
    public class IronDebug
    {
        public static string IronDevHivePah
        {

             get{ return System.Environment.GetEnvironmentVariable("IRONSPDEVHIVE").Replace("\\","//");}
        }
    }
}
#endif