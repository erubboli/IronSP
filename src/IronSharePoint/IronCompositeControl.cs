using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class IronCompositeControl : CompositeControl, IIronControl
    {
        public IronEngine Engine { get; set; }
        public WebPart WebPart { get; set; }
        public IIronDataStore Data { get; set; }
        public string Config { get; set; }

        public virtual List<EditorPart> CreateEditorParts()
        {
            return new List<EditorPart>();
        }
    }
}
