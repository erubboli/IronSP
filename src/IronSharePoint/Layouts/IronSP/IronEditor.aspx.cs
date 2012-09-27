using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Text;

namespace IronSharePoint.Layouts.IronSP
{
    public partial class IronEditor : LayoutsPageBase
    {
        protected string Script { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var web = SPContext.Current.Web;

                var script = web.GetFileAsString(Request["file"]);

                Script = Server.HtmlEncode(script);
            }

            buttonSave.Click += new EventHandler(buttonSave_Click);
        }

        void buttonSave_Click(object sender, EventArgs e)
        {
            var web = SPContext.Current.Web;
            var file = web.GetFile(Request["file"]);

            var script = Request["script"];
            Script = Server.HtmlEncode(script);

            file.SaveBinary(Encoding.Default.GetBytes(script));
        }
    }
}
