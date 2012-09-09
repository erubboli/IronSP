using System;
using Newtonsoft.Json;

namespace IronSharePoint.IronConsole
{
    public class Response
    {
        public string Output { get; set; }
        public string Result { get; set; }
        public string Error { get; set; }
        public string StackTrace { get; set; }

        public bool HasError
        {
            get { return !String.IsNullOrEmpty((Error ?? "").Trim()); }
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void LoadJson(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }

        public static Response FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Response>(json);
        }
    }
}