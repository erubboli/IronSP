using System;
using System.Collections.Specialized;
using System.Web;
namespace IronSharePoint
{
    public interface IIronScriptEngine
    {
        void Start(string language);
        object CallMethod(string method, params object[] parameter);
        void Dispose();
        string ExecuteScript(string script);
        object GetVariable(string name);
        void SetVariable(string name, object value);
        void Stop();

        string ExecuteWebRequest(string path, string url, string queryString, NameValueCollection form, string script);
    }
}
