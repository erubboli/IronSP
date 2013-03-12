using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.SharePoint;

namespace IronSharePoint.IronLog
{
    public class IronLogger
    {
        private readonly IronRuntime _runtime;

        public IronLogger(IronRuntime runtime)
        {
            _runtime = runtime;
        }

        public void Log(string message)
        {
            Log(message, LogLevel.Info);
        }

        public void Log(string message, LogLevel level)
        {
            var logText = string.Format("[{0}]\t{1} - {2}", level, DateTime.Now, message);

            try
            {
                var site = _runtime.Site;

                site.AddWorkItem(Guid.NewGuid(),
                                 DateTime.UtcNow,
                                 IronLogWorkItemJobDefinition.WorkItemGuid,
                                 site.RootWeb.ID,
                                 site.ID,
                                 1,
                                 false,
                                 site.RootWeb.GetList(site.RootWeb.ServerRelativeUrl + "/" + IronConstant.IronLogsListPath).ID,
                                 Guid.Empty,
                                 site.SystemAccount.ID,
                                 null,
                                 logText,
                                 Guid.Empty
                    );
            }
            catch
            {
            }
        }
    }
}