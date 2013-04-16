using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace IronSharePoint.Diagnostics
{
    public class IronULSLogger
    {
        private readonly IronDiagnosticsService _service;

        const string ErrorFormat = "Exception '{0}' - {1}\n{2}";

        public IronULSLogger(IronDiagnosticsService service)
        {
            _service = service;
        }

        public IronULSLogger() : this(IronDiagnosticsService.Local)
        {
        }

        static readonly Lazy<IronULSLogger> _local = new Lazy<IronULSLogger>(() => new IronULSLogger(), true);

        public static IronULSLogger Local
        {
            get { return _local.Value; }
        }

        public void Verbose(string message, IronCategoryDiagnosticsId categoryId)
        {
            IronDiagnosticsService.Local.WriteTrace(1, IronDiagnosticsService.Local[categoryId],
                                                    TraceSeverity.Verbose, message);
        }

        public void Error(string message, IronCategoryDiagnosticsId categoryId = IronCategoryDiagnosticsId.None)
        {
            _service.WriteTrace(1, IronDiagnosticsService.Local[categoryId], TraceSeverity.Unexpected, message);
        }

        public string Error(string message, Exception ex, IronCategoryDiagnosticsId categoryId = IronCategoryDiagnosticsId.None)
        {
            string output;
            if (!IsRubyException(ex) || !TryFormatRubyException(ex, out output))
            {
                output = String.Format(ErrorFormat,
                                           ex.GetType().Name,
                                           ex.Message,
                                           ex.StackTrace);
            }
            if (!String.IsNullOrWhiteSpace(message)) output = message + " - " + output;
            _service.WriteTrace(1, IronDiagnosticsService.Local[categoryId], TraceSeverity.Unexpected, output);

            return output;
        }

        bool IsRubyException(Exception ex)
        {
            return ex.TargetSite != null
                   && ex.TargetSite.DeclaringType != null
                   && ex.TargetSite.DeclaringType.FullName.StartsWith("Microsoft.Scripting.Interpreter");
        }

        bool TryFormatRubyException(Exception ex, out string output)
        {
            output = string.Empty;
            if (SPContext.Current != null)
            {
                try
                {
                    var runtime = IronRuntime.GetDefaultIronRuntime(SPContext.Current.Site);
                    var expcetionOperations = runtime.RubyEngine.GetService<ExceptionOperations>();

                    output = expcetionOperations.FormatException(ex);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
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
