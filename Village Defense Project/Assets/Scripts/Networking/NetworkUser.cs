using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Gem
{
    namespace Networking
    {
        /// <summary>
        /// A user currently playing the game.
        /// </summary>
        [Serializable]
        public struct NetworkUser
        {
            /// <summary>
            /// The default user. Use this to compare and check if its null.
            /// </summary>
            public static readonly NetworkUser BAD_USER = new NetworkUser("@@BAD_USER@@");

            /// <summary>
            /// The username of the network user.
            /// </summary>
            [DebugLabel]
            [SerializeField]
            private string m_Username;
            /// <summary>
            /// The GUID of the network user. This is unique to each and player and is set from NetworkPlayer variable.
            /// </summary>
            [DebugLabel]
            [SerializeField]
            private string m_GUID;

            /// <summary>
            /// Network Player variable.
            /// </summary>
            private NetworkPlayer m_NetworkPlayer;

            public NetworkUser(string aUsername)
            {

                m_Username = aUsername;
                m_NetworkPlayer = default(NetworkPlayer);
                m_GUID = string.Empty;
            }

            public static bool operator ==(NetworkUser A, NetworkUser B)
            {
                return A.username == B.username;
            }
            public static bool operator !=(NetworkUser A, NetworkUser B)
            {
                return A.username != B.username;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (obj.GetType() == typeof(NetworkUser))
                {
                    return ((NetworkUser)obj).username == username;
                }
                return false;
            }

            public bool Equals(NetworkUser aServer)
            {
                return username == aServer.username;
            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + username.GetHashCode();
                hash = (hash * 7) + guid.GetHashCode();
                hash = (hash * 7) + networkPlayer.GetHashCode();
                return hash;
            }

            public override string ToString()
            {
                return "Username: " + username + "\nID: " + guid;
            }

            public void Serialize(Stream aStream, IFormatter aFormatter)
            {
                aFormatter.Serialize(aStream, username);
                aFormatter.Serialize(aStream, guid);
            }
            public void Deserialize(Stream aStream, IFormatter aFormatter)
            {
                username = (string)aFormatter.Deserialize(aStream);
                guid = (string)aFormatter.Deserialize(aStream);
            }

            public string username
            {
                get { return m_Username; }
                set { m_Username = value; }
            }
            public NetworkPlayer networkPlayer
            {
                get { return m_NetworkPlayer; }
                set { m_NetworkPlayer = value; m_GUID = m_NetworkPlayer.guid; }
            }
            public string guid
            {
                get { return m_GUID; }
                set { m_GUID = value; }
            }
        }
    }
}