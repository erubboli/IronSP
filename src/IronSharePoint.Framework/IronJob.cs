using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronJob : SPJobDefinition
    {
        [Persisted]
        private string script;

        public string Script
        {
            get { return script; }
            set { script = value; }
        }

        [Persisted]
        private Guid hiveId;

        public Guid HiveId
        {
            get { return hiveId; }
            set { hiveId = value; }
        }

        [Persisted]
        private string data;

        public string Data
        {
            get { return data; }
            set { data = value; }
        }


        public IronJob() : base(){} 
        
        public IronJob(string jobName, SPWebApplication webApplication, SPServer server, SPJobLockType lockType, Guid hiveId) : base(jobName, webApplication, SPServer.Local, lockType) 
        {
            this.Title = jobName;
            this.HiveId = hiveId;
            this.Script = String.Empty;
        } 
        
        public override void Execute(Guid targetInstanceId) 
        { 
            using(SPSite site = new SPSite(HiveId))
            {
                var runtime = IronRuntime.GetDefaultIronRuntime(site);
               
                if (String.IsNullOrEmpty(Script))
                {
                    runtime.IronConsole.Execute(Script, ".rb", true);
                }
            }
        } 
    }
}
