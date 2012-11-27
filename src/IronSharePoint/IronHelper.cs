using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronSharePoint
{
    public class IronHelper
    {
        public static string GetPrefixedKey(string name)
        {
            return IronConstants.IronPrefix + name;
        }
    }
}
