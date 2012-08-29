using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public class DynamicControl : Control, IDynamicControl
    {
        public IronEngine Engine { get; set; }
        public WebPart WebPart { get; set; }
        public IIronDataStore Data { get; set; }

        public List<EditorPart> CreateEditorParts()
        {
            return new List<EditorPart>();
        }
    }
}
