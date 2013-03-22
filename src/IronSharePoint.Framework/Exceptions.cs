using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IronSharePoint.Exceptions
{
    [Serializable]
    public class IronRuntimeAccesssException : Exception
    {
        public Guid SiteId { get; set; }

        public IronRuntimeAccesssException() {}

        public IronRuntimeAccesssException(string message)
            : base(message) {}

        public IronRuntimeAccesssException(string message, Exception inner)
            : base(message, inner) {}

        protected IronRuntimeAccesssException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }

    [Serializable]
    public class RubyFrameworkInitializationException : Exception
    {
        public RubyFrameworkInitializationException() {}

        public RubyFrameworkInitializationException(string message)
            : base(message) {}

        public RubyFrameworkInitializationException(string message, Exception inner)
            : base(message, inner) {}

        protected RubyFrameworkInitializationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }

    [Serializable]
    public class DynamicInstanceInitializationException : Exception
    {
        public DynamicInstanceInitializationException() {}

        public DynamicInstanceInitializationException(string message)
            : base(message) {}

        public DynamicInstanceInitializationException(string message, Exception inner)
            : base(message, inner) {}

        protected DynamicInstanceInitializationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}
