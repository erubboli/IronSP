using System;
using System.Web.UI;
using Microsoft.SharePoint;

namespace IronSharePoint.IronConsole
{
    public partial class IronConsoleUserControl : UserControl
    {
        public SPContext CurrentContext
        {
            get { return SPContext.Current; }
        }

        protected void Page_Load(object sender, EventArgs e) { }
    }
}