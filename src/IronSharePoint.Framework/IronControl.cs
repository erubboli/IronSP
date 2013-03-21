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
        public WebPart WebPart { get; set; }
        public IIronDataStore DataStore { get; set; }

        public virtual List<EditorPart> CreateEditorParts()
        {
            return new List<EditorPart>();
        }
    }
}
