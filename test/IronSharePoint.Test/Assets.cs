using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IronSharePoint.Test
{
    static class Assets
    {
        public static string Load(string key)
        {
            return File.ReadAllText(String.Format(@".\assets\{0}", key));
        }
    }
}
