using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace IronSharePoint.Diagnostics
{
    public enum IronCategoryDiagnosticsId
    {
        None = 0,
        Core = 100,
        Controls = 200,
        WebParts = 300,
        EventReceivers = 400,
        Jobs = 900,
        Services = 1000,
        Unknown = 9999
    }

    [System.Runtime.InteropServices.GuidAttribute("8D5F89BA-0DE0-4D46-811C-E631B6FAC228")]
    public class IronDiagnosticsService : SPDiagnosticsServiceBase
    {
        const string DiagnosticsAreaName = "IronSP";

        public IronDiagnosticsService()
        {
        }

        public IronDiagnosticsService(string name, SPFarm farm)
            : base(name, farm)
        {
        }

        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            var categories = new List<SPDiagnosticsCategory>();
            foreach (var catName in Enum.GetNames(typeof(IronCategoryDiagnosticsId)))
            {
                var catId = (uint) (int)Enum.Parse(typeof(IronCategoryDiagnosticsId), catName);
                categories.Add(new SPDiagnosticsCategory(catName, TraceSeverity.Verbose, EventSeverity.Error, 0, catId));
            }

            yield return new SPDiagnosticsArea(DiagnosticsAreaName, categories);
        }

        public static IronDiagnosticsService Local
        {
            get
            {
                return GetLocal<IronDiagnosticsService>();
            }
        }

        public SPDiagnosticsCategory this[IronCategoryDiagnosticsId id]
        {
            get
            {
                return Areas[DiagnosticsAreaName].Categories[id.ToString()];
            }
        }
    }
}
