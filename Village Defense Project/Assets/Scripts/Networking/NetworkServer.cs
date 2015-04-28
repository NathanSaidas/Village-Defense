using System;
using System.IO;
using System.Runtime.Serialization;

using UnityEngine;

namespace Gem
{
    namespace Networking
    {
        [Serializable]
        public struct NetworkServer
        {
            public static readonly NetworkServer BAD_SERVER = new NetworkServer("@@BAD_SERVER@@", "@@BAD_SERVER@@");
            [DebugLabel]
            [SerializeField]
            private string m_ServerName;
            [DebugLabel]
            [SerializeField]
            private string m_ServerType;
            private HostData m_HostData;
            [DebugLabel]
            [SerializeField]
            private string m_HostUsername;

            [SerializeField]
            private NetworkUser[] m_CurrentUsers;

            public NetworkServer(HostData aHost)
            {
                m_HostData = aHost;
                m_HostUsername = string.Empty;
                m_CurrentUsers = null;

                if (aHost != null)
                {
                    m_ServerName = aHost.gameName;
                    m_ServerType = aHost.gameType;
                }
                else
                {
                    m_ServerName = string.Empty;
                    m_ServerType = string.Empty;
                }
            }

            public NetworkServer(string aServerName, string aServerType)
            {
                m_HostData = null;
                m_ServerName = aServerName;
                m_ServerType = aServerType;
                m_CurrentUsers = null;
                m_HostUsername = string.Empty;
            }

            public NetworkServer(string aServerName, string aServerType, string aHostUsername, NetworkUser[] aCurrentUsers)
            {
                m_HostData = null;
                m_ServerName = aServerName;
                m_ServerType = aServerType;
                m_CurrentUsers = aCurrentUsers;
                m_HostUsername = aHostUsername;
            }

            public static bool operator ==(NetworkServer A, NetworkServer B)
            {
                return A.serverName == B.serverName && A.serverType == B.serverType;
            }
            public static bool operator !=(NetworkServer A, NetworkServer B)
            {
                return !(A == B);
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (obj.GetType() == typeof(NetworkServer))
                {
                    return ((NetworkServer)obj).serverName == serverName && ((NetworkServer)obj).serverType == serverType;
                }
                return false;
            }

            public bool Equals(NetworkServer aServer)
            {
                return serverType == aServer.serverType && serverName == aServer.serverName;
            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + serverName.GetHashCode();
                hash = (hash * 7) + serverType.GetHashCode();
                hash = (hash * 7) + hostData.GetHashCode();
                return hash;
            }

            public void Serialize(Stream aStream, IFormatter aFormatter)
            {
                aFormatter.Serialize(aStream, serverName);
                aFormatter.Serialize(aStream, serverType);
                aFormatter.Serialize(aStream, hostUsername);
                if(currentUsers != null)
                {
                    aFormatter.Serialize(aStream, m_CurrentUsers.Length);
                }
                else
                {
                    aFormatter.Serialize(aStream, 0);
                }

                foreach(NetworkUser user in currentUsers)
                {
                    user.Serialize(aStream, aFormatter);
                }
            }
            public void Deserialize(Stream aStream, IFormatter aFormatter)
            {
                serverName = (string)aFormatter.Deserialize(aStream);
                serverType = (string)aFormatter.Deserialize(aStream);
                hostUsername = (string)aFormatter.Deserialize(aStream);
                int count = (int)aFormatter.Deserialize(aStream);

                m_CurrentUsers = new NetworkUser[count];
                for(int i = 0; i < count; i++)
                {
                    NetworkUser user = NetworkUser.BAD_USER;
                    user.Deserialize(aStream, aFormatter);
                    m_CurrentUsers[i] = user;
                }
            }

            public string serverName { get { return m_ServerName; } set { m_ServerName = value; } }
            public string serverType { get { return m_ServerType; } set { m_ServerType = value; } }
            public HostData hostData { get { return m_HostData; } set { m_HostData = value; } }
            public string hostUsername { get { return m_HostUsername; } set { m_HostUsername = value; } }
            public NetworkUser[] currentUsers { get { return m_CurrentUsers; } set { m_CurrentUsers = value; } }

        }
    }
}