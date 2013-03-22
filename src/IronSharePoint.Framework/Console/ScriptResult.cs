using System;
using Newtonsoft.Json;

namespace IronSharePoint.Console
{
    public class ScriptResult
    {
        public string Output { get; set; }
        public string ReturnValue { get; set; }
        public string Error { get; set; }
        public long ExecutionTime { get; set; }

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

        public static ScriptResult FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ScriptResult>(json);
        }
    }
}