using System;
using System.Runtime.Serialization;
using IronSharePoint.Administration;

namespace IronSharePoint.Exceptions
{
    [Serializable]
    public class HiveInstantiationException : Exception
    {
        public HiveInstantiationException(string message, Exception innerException, HiveSetup hiveSetup)
            : base(message, innerException)
        {
            HiveSetup = hiveSetup;
        }

        public HiveInstantiationException(string message, HiveSetup hiveSetup)
            : base(message)
        {
            HiveSetup = hiveSetup;
        }

        public HiveInstantiationException(HiveSetup hiveSetup)
        {
            HiveSetup = hiveSetup;
        }

        protected HiveInstantiationException(SerializationInfo info, StreamingContext context, HiveSetup hiveSetup)
            : base(info, context)
        {
            HiveSetup = hiveSetup;
        }

        public HiveInstantiationException() {}

        public HiveInstantiationException(string message)
            : base(message) {}

        public HiveInstantiationException(string message, Exception inner)
            : base(message, inner) {}

        protected HiveInstantiationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}

        public HiveSetup HiveSetup { get; private set; }
    }

    [Serializable]
    public class IronRuntimeAccesssException : Exception
    {
        public IronRuntimeAccesssException() {}

        public IronRuntimeAccesssException(string message)
            : base(message) {}

        public IronRuntimeAccesssException(string message, Exception inner)
            : base(message, inner) {}

        protected IronRuntimeAccesssException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}

        public Guid SiteId { get; set; }
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
    public class DynamicInstantiationException : Exception
    {
        public DynamicInstantiationException() {}

        public DynamicInstantiationException(string message)
            : base(message) {}

        public DynamicInstantiationException(string message, Exception inner)
            : base(message, inner) {}

        protected DynamicInstantiationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}