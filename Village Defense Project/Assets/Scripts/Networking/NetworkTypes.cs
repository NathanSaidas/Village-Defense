//Copyright (c) 2015 Nathan Hanlan
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

#region CHANGE LOG
// -- April     15, 2015 - Nathan Hanlan - Added in basic types
// -- April     16, 2015 - Nathan Hanlan - Added in constructor / serialize / deserialize methods NetworkUser
#endregion

using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Gem
{
    /// <summary>
    /// A callback for network requests made.
    /// </summary>
    /// <param name="aData"></param>
    public delegate void RequestCallback(RequestData aData);

    /// <summary>
    /// Data returned back upon request complete.
    /// </summary>
    public struct RequestData
    {
        public object data { get; set; }
        public Request request { get; set; }
    }

    /// <summary>
    /// A user currently playing the game.
    /// </summary>
    [Serializable]
    public struct NetworkUser
    {
        public static readonly NetworkUser BAD_USER = new NetworkUser("@@BAD_USER@@");
        [DebugLabel]
        [SerializeField]
        private string m_Username;
        [DebugLabel]
        [SerializeField]
        private bool m_IsConnected;
        [DebugLabel]
        [SerializeField]
        private bool m_IsHost;
        private NetworkPlayer m_NetworkPlayer;

        public NetworkUser(string aUsername)
        {
            m_Username = aUsername;
            m_IsConnected = false;
            m_IsHost = false;
            m_NetworkPlayer = default(NetworkPlayer);
        }

        public static bool operator ==(NetworkUser A, NetworkUser B)
        {
            return A.username == B.username;
        }
        public static bool operator !=(NetworkUser A, NetworkUser B)
        {
            return A.username == B.username;
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
            hash = (hash * 7) + isConnected.GetHashCode();
            hash = (hash * 7) + isHost.GetHashCode();
            hash = (hash * 7) + networkPlayer.GetHashCode();
            return hash;
        }

        public void Serialize(Stream aStream, IFormatter aFormatter)
        {
            aFormatter.Serialize(aStream, username);
            aFormatter.Serialize(aStream, isConnected);
            aFormatter.Serialize(aStream, isHost);
        }
        public void Deserialize(Stream aStream, IFormatter aFormatter)
        {
            username = (string)aFormatter.Deserialize(aStream);
            isConnected = (bool)aFormatter.Deserialize(aStream);
            isHost = (bool)aFormatter.Deserialize(aStream);
        }

        public string username
        {
            get{return m_Username;}
            set{m_Username = value;}
        }

        public bool isConnected
        {
            get { return m_IsConnected; }
            set { m_IsConnected = value; }
        }

        public bool isHost
        {
            get { return m_IsHost; }
            set { m_IsHost = value; }
        }

        public NetworkPlayer networkPlayer
        {
            get { return m_NetworkPlayer; }
            set { m_NetworkPlayer = value; }
        }
    }

    [Serializable]
    public struct NetworkServer
    {
        public static readonly NetworkServer BAD_SERVER = new NetworkServer("@@BAD_SERVER@@","@@BAD_SERVER@@");
        [DebugLabel]
        [SerializeField]
        private string m_ServerName;
        [DebugLabel]
        [SerializeField]
        private string m_ServerType;
        private HostData m_HostData;
        
        public NetworkServer(HostData aHost)
        {
            m_HostData = aHost;
            
            if(aHost != null)
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
        }

        public static bool operator == (NetworkServer A, NetworkServer B)
        {
            return A.serverName == B.serverName && A.serverType == B.serverType;
        }
        public static bool operator !=(NetworkServer A, NetworkServer B)
        {
            return !(A == B);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }
            if(obj.GetType() == typeof(NetworkServer))
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

        public string serverName { get { return m_ServerName; } set { m_ServerName = value; } }
        public string serverType { get { return m_ServerType; } set { m_ServerType = value; } }
        public HostData hostData { get { return m_HostData; } set { m_HostData = value; } }

    }

    public static class NetworkStatus
    {
        public const int ERROR = -1;
        public const int BAD = 0;
        public const int GOOD = 1;
        public const int FULL = 2;
        public const int INVALID_USERNAME = 3;
        public const int INVALID_PASSWORD = 4;
        public const int ACCOUNT_ALREADY_EXISTS = 5;
    }

    public enum NetworkDisconnect
    {
        Quit,
        Kicked,
        LostConnection
    }

    public enum NetworkState
    {
        Offline,
        LoggedIn,
        InLobby,
        InGame,
        Online,
        CreatingServer,
        ConnectingHost,
    }

    public enum RequestStatus
    {
        Invalid,
        Pending,
        Complete,
        TimedOut
    }

    public enum RequestType
    {
        //General Network Requests
        Authentication,
        AvailableServers,
        AvailableAuthenticationServers,
        ConnectionList,
        ConnectionAuthentication,
        ConnectionGame,
        Kick,
        Disconnect,

        //Main menu
        SelectHero,
        StartGame,

        //Gameplay
        Spawn,
        Ownership,
        Destroy,
        IssueAbility
    }

}

