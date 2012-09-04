using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace IronSharePoint
{
    public interface IIronControl
    {
        IronEngine Engine { get; set; }
        WebPart WebPart { get; set; }
        IIronDataStore Data { get; set; }
        List<EditorPart> CreateEditorParts();
    }
}
