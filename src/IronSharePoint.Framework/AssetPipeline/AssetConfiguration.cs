using System.Collections.Generic;

namespace IronSharePoint.AssetPipeline
{
    public class AssetConfiguration
    {
        public AssetConfiguration()
        {
            Paths = new List<string>();
        }

        public IList<string> Paths { get; private set; }

        public static AssetConfiguration Local
        {
            get
            {
                return new AssetConfiguration
                    {
                        Paths =
                            {
                                "Javascripts",
                                "Stylesheetes"
                            }
                    };
            }
        }
    }
}