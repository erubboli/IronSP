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
        private string _html = "<div class='ironsp-not-loaded'></div>";
        private AsyncTaskDelegate _dlgt;

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

        public string Html
        {
            get { return _html; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Page != null && IsAsync)
            {
                Page.RegisterAsyncTask(new PageAsyncTask(BeginRenderAsync, EndRenderAsync, RenderAsyncTimeout, SPContext.Current, true));
            }
        }

        IAsyncResult BeginRenderAsync(object src, EventArgs e, AsyncCallback cb, object state)
        {
            _dlgt = new AsyncTaskDelegate(() =>
                                              {
                                                  try
                                                  {
                                                      _html = ToHtml(state);
                                                  }
                                                  catch (Exception ex)
                                                  {
                                                      RenderException = ex;
                                                      _html =
                                                          string.Format(
                                                              "<div class='iron-control-error'>Error in {0}</div>",
                                                              GetType().Name);
                                                  }
                                              });
            var result = _dlgt.BeginInvoke(cb, state);

            return result;
        }

        void EndRenderAsync(IAsyncResult ar)
        {
            _dlgt.EndInvoke(ar);
        }

        void RenderAsyncTimeout(IAsyncResult ar)
        {
            _html = string.Format("<div class='iron-control-error'>Control {0} timed out</div>",
                  GetType().Name);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var html = IsAsync ? Html : ToHtml(SPContext.Current);
            writer.Write(html);
        }

        public virtual string ToHtml(object state)
        {
            return "";
        }
    }
}
