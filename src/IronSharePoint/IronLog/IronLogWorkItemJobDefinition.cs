﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace IronSharePoint.IronLog
{
    public class IronLogWorkItemJobDefinition : SPWorkItemJobDefinition
    {
        public static readonly Guid WorkItemGuid = new Guid("{2C59953B-1344-4D94-8ADF-3FB291342D30}");
        public static readonly String JobName = "IronLog Worker";

        public IronLogWorkItemJobDefinition()
            :base()
        {
        }

        public IronLogWorkItemJobDefinition(SPWebApplication webApplication)
            : base(JobName, webApplication)
        {
            
        }

        public override Guid WorkItemType()
        {
            return WorkItemGuid;
        }

        protected override bool ProcessWorkItem(SPContentDatabase contentDatabase,
                                                Microsoft.SharePoint.SPWorkItemCollection workItems,
                                                Microsoft.SharePoint.SPWorkItem workItem, SPJobState jobState)
        {
            // process the workItem
            using (var site = new SPSite(workItem.SiteId))
            {
                using (var web = site.OpenWeb(workItem.WebId))
                {
                    //var logEntry = IronLogger.Entry.Base64Deserialize(workItem.TextPayload);

                    if (true)//logEntry.Level <= WebLogLevel(web))
                    {
                        SPDocumentLibrary logLib = null;
                        try
                        {
                            logLib = (SPDocumentLibrary)web.GetList(web.ServerRelativeUrl + "/" + IronConstants.IronLogsListPath);
                        }
                        catch (Exception ex)
                        {
                            throw new SPException("Couldn't find IronLog document library", ex);
                            throw ex;
                        }
                        if (logLib != null)
                        {
                            var logFileCandidates = logLib.GetItems(LogFileQuery(2));
                            if (logFileCandidates.Count > 0)
                            {
                                var logFile = logFileCandidates[0].File;
                                AppendToFile(logFile, workItem.TextPayload);
                            }
                            else
                            {
                                var url = DateTime.Now.ToString("s").Replace(":", "_");
                                url = string.Format("{0}.log", url);

                                logLib.RootFolder.Files.Add(url, GetBytes(workItem.TextPayload));
                            }
                        }
                    }
                    // delete the workItem after we've processed it
                    workItems.SubCollection(site, web, 0,
                                            (uint) workItems.Count).DeleteWorkItem(workItem.Id);
                }
            }

            // returning true allows the timer job to process additional items in the queue
            // returning false will end this occurance of the timer job
            return true;
        }

        LogLevel WebLogLevel(SPWeb web)
        {
            string logLevelName = web.Properties[IronHelper.GetPrefixedKey("LogLevel")];

            if (String.IsNullOrEmpty(logLevelName))
            {
                return LogLevel.All;
            }
            return (LogLevel) Enum.Parse(typeof (LogLevel), logLevelName);
        }

        SPQuery LogFileQuery(double sizeMB)
        {
            var sizeByte = (int) Math.Round(sizeMB*1024*1024);
            var queryString = @"
<Query>
    <Where>
        <Lt>
            <FieldRef Name='File_x0020_Size' />
            <Value Type='Integer'>{0}</Value>
        </Lt>
    </Where>
    <OrderBy>
        <FieldRef Name='Modified' Ascending='False' />
    </OrderBy>
</Query>";

            return new SPQuery() {Query = String.Format(queryString, sizeByte)};
        }

        public void AppendToFile(SPFile file, string message)
        {
            using (var stream = file.OpenBinaryStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    
                    var content = reader.ReadToEnd();
                    if (!content.EndsWith(Environment.NewLine))
                    {
                        content += Environment.NewLine;
                    }
                    var writeStream = new MemoryStream();
                    using (var writer = new StreamWriter(writeStream, Encoding.UTF8))
                    {
                        writer.Write(content);
                        writer.WriteLine(message);
                    }
                    file.SaveBinary(writeStream.ToArray());
                }
            }
        }

        public byte[] GetBytes(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }
    }
}