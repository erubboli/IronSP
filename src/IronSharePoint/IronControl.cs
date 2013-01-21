using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace IronSharePoint
{
    public class IronControl : Control, IIronControl
    {
        public IronEngine Engine { get; set; }
        public WebPart WebPart { get; set; }
        public IIronDataStore Data { get; set; }
        public string Config { get; set; }

        public Exception RenderException { get; set; }
        public bool IsAsync { get; set; }

        protected delegate void AsyncTaskDelegate();

        public virtual List<EditorPart> CreateEditorParts()
        {
            return new List<EditorPart>();
        }
    }
}
