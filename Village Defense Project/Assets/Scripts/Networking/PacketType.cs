using UnityEngine;
using System.Collections;

namespace Gem
{
    namespace Networking
    {
        public enum PacketType
        {
            Invalid,

            //Authentication
            Authentication,
            AuthenticationStatus,

            //Connection
            ConnectionRequest,
            ConnectionStatus,
            ConnectionList,
            ConnectionKicked,

            //Object
            ObjectCreate,
            ObjectDestroy,
        }
    }
}
