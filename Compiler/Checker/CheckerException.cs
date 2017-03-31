using System;
using System.Runtime.Serialization;

namespace RappiSharp.Compiler.Checker.Visitors
{
    [Serializable]
    internal class CheckerException : Exception
    {
        public CheckerException()
        {
        }

        public CheckerException(string message) : base(message)
        {
        }

        public CheckerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CheckerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}