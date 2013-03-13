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
        WebPart WebPart { get; set; }
        IIronDataStore DataStore { get; set; }
        List<EditorPart> CreateEditorParts();
    }
}
