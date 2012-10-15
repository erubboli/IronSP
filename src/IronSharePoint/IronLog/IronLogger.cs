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

        [Serializable]
        internal class Entry
        {
            static readonly IFormatter _formatter = new BinaryFormatter();

            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }

            public string Base64Serialize()
            {
                using (var ms = new MemoryStream())
                {
                    _formatter.Serialize(ms, this);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }

            public static Entry Base64Deserialize(string base64)
            {
                var bytes = Convert.FromBase64String(base64);
                using (var stream = new MemoryStream(bytes))
                {
                    return (Entry)_formatter.Deserialize(stream);
                }
            }

            public override string ToString()
            {
                return string.Format("[{0}] {1} - {2}", Level, Timestamp, Message);
            }
        }

        public void Log(string message)
        {
            Log(message, LogLevel.Info);
        }

        public void Log(string message, LogLevel level)
        {
            var entry = new IronLogger.Entry()
                            {
                                Level = level,
                                Message = message,
                                Timestamp = DateTime.Now
                            };

            try
            {
                SPSite site = _runtime.IronHive.Site;

                var guid = site.AddWorkItem(Guid.NewGuid(),
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
                                 entry.Base64Serialize(),
                                 Guid.Empty
                    );
                Console.WriteLine(guid);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}