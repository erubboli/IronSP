using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.SharePoint;
using System.IO;
using System.Web;

namespace IronSharePoint
{
    public class IronPlatformAdaptationLayer : PlatformAdaptationLayer
    {
        private readonly IronHive _ironHive;

        public IronPlatformAdaptationLayer(IronHive host)
        {
            _ironHive=host;
        }

        public override bool FileExists(string file)
        {
            bool fileExists = !file.StartsWith(IronConstant.IronHiveRoot) && base.FileExists(file);

            if (!fileExists && file.StartsWith(IronConstant.IronHiveRoot))
            {
                fileExists = _ironHive.ContainsFile(file);
            }
         
            return fileExists;
        }
       
        public override System.IO.Stream OpenOutputFileStream(string file)
        {
            var spFile = _ironHive.LoadFile(file);

            if (spFile != null)
            {
                return spFile.OpenBinaryStream();
            }

            return base.OpenOutputFileStream(file);
        }

        public override string GetFullPath(string file)
        {
            if (_ironHive.ContainsFile(file))
            {
                return _ironHive.GetFullPath(file);
            }
            return base.GetFullPath(file);
        }

        public override System.IO.Stream OpenInputFileStream(string file)
        {
            var spFile = _ironHive.LoadFile(file);

            if (spFile != null)
            {
                return spFile.OpenBinaryStream();
            }

            return base.OpenInputFileStream(file);
        }
    }
}
