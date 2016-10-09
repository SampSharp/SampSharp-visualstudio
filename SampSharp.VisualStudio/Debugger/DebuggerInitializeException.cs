using System;
using System.Runtime.Serialization;

namespace SampSharp.VisualStudio.Debugger
{
    [Serializable]
    public class DebuggerInitializeException : Exception
    {
        public DebuggerInitializeException()
        {
        }

        public DebuggerInitializeException(string message) : base(message)
        {
        }

        public DebuggerInitializeException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DebuggerInitializeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}