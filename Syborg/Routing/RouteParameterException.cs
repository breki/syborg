using System;
using System.Runtime.Serialization;

namespace Syborg.Routing
{
    [Serializable]
    public class RouteParameterException : Exception
    {
        public RouteParameterException(string message) : base(message)
        {
        }

        public RouteParameterException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RouteParameterException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}