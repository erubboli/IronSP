using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronSharePoint
{
    public interface IIronDataStore
    {
        void SaveData(string data);
        string GetData();
    }
}
