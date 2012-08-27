using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronSharePoint
{
    public class IronScriptCache
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Microsoft.Scripting.Hosting.CompiledCode CompiledCode { get; set; }  
        public string Script { get; set; }
    }
}
