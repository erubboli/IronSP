using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint
{
    public static class IronConstant
    {
        public const string IronHiveLibraryPath = "_catalogs/IronHive";
        public const string IronLogsListPath = "Lists/IronLogs";

        public const string IronSiteFeatureId = "a1752f91-1403-40c1-a257-69eddf8976cf";
        public const string IronHiveSiteFeatureId = "354ee774-7d04-4ad6-91f9-1bc433a70bee";
        public const string IronRubyFeatureId = "183909b9-cfb3-477c-a48c-02e3d5e59844";

        public const string IronPrefix = "IronSP_";

        public const string IronRubyLanguageName = "IronRuby";
        public const string IronRuntimeKey = IronPrefix + "Runtime";

        public static string IronSPRootDirectory =
            @"C:\Program Files\Common Files\microsoft shared\Web Server Extensions\14\TEMPLATE\FEATURES\IronSP_Root";
        public static string IronRubyRootDirectory =
            @"C:\Program Files\Common Files\microsoft shared\Web Server Extensions\15\TEMPLATE\FEATURES\IronSP_IronRuby";

        public static string GetPrefixed(string s)
        {
            return string.Format("{0}_{1}", IronPrefix, s);
        }

        public static IronEnvironment IronEnv
        {
            get
            {
                var env = IronEnvironment.Production;

                var environmentVariable = System.Environment.GetEnvironmentVariable("IRONSP_ENV");
                if (!String.IsNullOrEmpty(environmentVariable))
                {
                    try
                    {
                        env = (IronEnvironment) Enum.Parse(typeof (IronEnvironment), environmentVariable, true);
                    }
                    catch(ArgumentException)
                    {
                        env = IronEnvironment.Production;
                    }
                }

                return env;
            }
        }

    //    static IronConstant()
    //    {
    //        var siteFeatureDirectory = SPFarm.Local.FeatureDefinitions[IronConstant.IronHiveSiteFeatureId].RootDirectory;
    //        var featureDirectory = new DirectoryInfo(siteFeatureDirectory).Parent.FullName;
    //        IronSPRootDirectory = Path.Combine(featureDirectory, "IronSP_Root");
    //    }
    }
}
