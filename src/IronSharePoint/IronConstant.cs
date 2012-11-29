using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronSharePoint.Diagnostics;

namespace IronSharePoint
{
    public static class IronConstant
    {
        public const string IronHiveRoot = "IronHive://";
        public const string IronHiveListPath = "_catalogs/IronHive";
        public const string IronLogsListPath = "Lists/IronLogs";
        public const string IronSiteFeatureId = "a1752f91-1403-40c1-a257-69eddf8976cf";
        public const string IronHiveSiteFeatureId = "354ee774-7d04-4ad6-91f9-1bc433a70bee";
        public const string IronRubyFeatureId = "183909b9-cfb3-477c-a48c-02e3d5e59844";
        public const string IronRubyLanguageName = "IronRuby";
        public const string IronPrefix = "IronSP_";
        public const string IronSpRoot = IronPrefix + "Root";

        public static IronEnvironment IronEnv
        {
            get
            {
                var env = IronEnvironment.Development;

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
    }
}
