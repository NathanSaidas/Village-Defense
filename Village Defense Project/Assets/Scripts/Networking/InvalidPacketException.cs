using System;
using System.Runtime.Serialization;

namespace Gem
{
    namespace Networking
    {
        public class InvalidPacketException : Exception
        {
            public InvalidPacketException()
                : base()
            {

            }

            public InvalidPacketException(string aMessage)
                : base(aMessage)
            {

            }

            public InvalidPacketException(string aMessage, Exception aInnerException)
                : base(aMessage, aInnerException)
            {

            }

            protected InvalidPacketException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
        }
    }
}
