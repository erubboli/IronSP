using System;
using System.IO;

namespace IronSharePoint.Test.Unit
{
    static class Assets
    {
        public static string Load(string key)
        {
            return File.ReadAllText(String.Format(@".\assets\{0}", key));
        }
    }
}
