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
        private string _script;

        public string Script
        {
            get { return _script; }
            set { _script = value; }
        }

        [Persisted]
        private Guid _siteId;

        public Guid SiteId
        {
            get { return _siteId; }
            set { _siteId = value; }
        }

        [Persisted]
        private string _data;

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        [Persisted]
        private string _languageName;

        public string LanguageName
        {
            get { return _languageName; }
            set { _languageName = value; }
        }

        public IronJob() : base(){} 
        
        public IronJob(string jobName, SPWebApplication webApplication, SPServer server, SPJobLockType lockType, Guid siteId) : base(jobName, webApplication, SPServer.Local, lockType) 
        {
            this.Title = jobName;
            this.SiteId = siteId;
            this.Script = String.Empty;
        } 
        
        public override void Execute(Guid targetInstanceId) 
        { 
            using(SPSite site = new SPSite(SiteId))
            {
                var runtime = IronRuntime.GetDefaultIronRuntime(site);
               
                if (String.IsNullOrEmpty(Script))
                {
                    runtime.Console.Execute(Script, LanguageName).Wait();
                }
            }
        } 
    }
}
