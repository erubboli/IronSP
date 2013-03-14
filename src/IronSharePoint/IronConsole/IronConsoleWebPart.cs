using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

namespace IronSharePoint.IronConsole
{
    [ToolboxItem(false)]
    public class IronConsoleWebPart : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        const string _ascxPath = @"~/_CONTROLTEMPLATES/15/IronSP/IronConsole/IronConsoleUserControl.ascx";

        protected override void CreateChildControls()
        {
            Control control = Page.LoadControl(_ascxPath);
            Controls.Add(control);
        }
    }
}