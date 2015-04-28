using UnityEngine;
using System;

#region CHANGE LOG
// -- April     27, 2015 - Nathan Hanlan - Added class BaseServer.
#endregion

namespace Gem
{
    namespace Networking
    {
        /// <summary>
        /// Base class for all future server implementations.
        /// </summary>
        [Serializable]
        public abstract class BaseServer
        {
            
            /// <summary>
            /// The name of the server.
            /// </summary>
            private string m_ServerName = string.Empty;

            /// <summary>
            /// The name for the server type
            /// </summary>
            private string m_ServerType = string.Empty;

            /// <summary>
            /// The port for the game to use.
            /// </summary>
            private int m_Port = Constants.DEFAULT_AUTHENTICATION_PORT;

            /// <summary>
            /// The maximum number of connections.
            /// </summary>
            private int m_MaxConnections = 0;

            /// <summary>
            /// The maximum number of users allowed on the server.
            /// </summary>
            private int m_MaxUsers = 0;

             
            /// <summary>
            /// Initiailzes the server object.
            /// </summary>
            public void Start()
            {
                Application.runInBackground = true;
                OnInitialize();
            }

            /// <summary>
            /// Destroys the server object.
            /// </summary>
            public void Destroy()
            {
                StopServer();
                OnDestroy();
            }

            /// <summary>
            /// Override to make use of Update functions
            /// </summary>
            public virtual void Update()
            {

            }
            
            /// <summary>
            /// Override to make use of OnPlayerConnected messages
            /// </summary>
            /// <param name="aPlayer">The player connecting.</param>
            public virtual void OnPlayerConnected(NetworkPlayer aPlayer)
            {

            }

            /// <summary>
            /// Override to make use of OnPlayerDisconnected messages
            /// </summary>
            /// <param name="aPlayer">The player disconnecting.</param>
            public virtual void OnPlayerDisconnected(NetworkPlayer aPlayer)
            {

            }

            /// <summary>
            /// Starts the server without security.
            /// </summary>
            public bool StartServer()
            {
                return StartServer(false);
            }

            /// <summary>
            /// Starts the server with security.
            /// </summary>
            /// <param name="aUseSecurity">Whether or not to use security.</param>
            public bool StartServer(bool aUseSecurity)
            {
                if(Network.peerType != NetworkPeerType.Disconnected)
                {
                    return false;
                }
                if(aUseSecurity)
                {
                    Network.InitializeSecurity();
                }
                NetworkConnectionError error = Network.InitializeServer(maxConnections, port, !Network.HavePublicAddress());
                if(error == NetworkConnectionError.NoError)
                {
                    MasterServer.RegisterHost(m_ServerType, m_ServerName);
                    OnServerInitialized();
                }
                else
                {
                    DebugUtils.LogError(error);
                }

                return error == NetworkConnectionError.NoError;
            }

            /// <summary>
            /// Stops the server.
            /// </summary>
            public void StopServer()
            {
                if(Network.isServer)
                {
                    OnServerShutdown();
                    MasterServer.UnregisterHost();
                    Network.Disconnect();
                }
            }

            public virtual NetworkServer GetServerInfo()
            {
                return new NetworkServer(m_ServerName, m_ServerType);
            }

            
            /// <summary>
            /// Gets called when the server object is initialized.
            /// </summary>
            protected abstract void OnInitialize();
            /// <summary>
            /// Gets called when the server object is destroyed.
            /// </summary>
            protected abstract void OnDestroy();
            /// <summary>
            /// Gets called when the server is initialized.
            /// </summary>
            protected abstract void OnServerInitialized();
            /// <summary>
            /// Gets called when the server is shut down.
            /// </summary>
            protected abstract void OnServerShutdown();

            /// <summary>
            /// Accessor to the server name.
            /// </summary>
            public string serverName
            {
                get { return m_ServerName; }
                set { if (!isConnected) { m_ServerName = value; } }
            }

            public string serverType
            {
                get { return m_ServerType; }
                set { if (!isConnected) { m_ServerType = value; } }
            }

            public int port
            {
                get { return m_Port; }
                set { if (!isConnected) { m_Port = value; } }
            }
                
            public int maxConnections
            {
                get { return m_MaxConnections; }
                set { if (!isConnected) { m_MaxConnections = value; } }
            }

            public int maxUsers
            {
                get { return m_MaxUsers; }
                set { m_MaxUsers = value; }
            }

            public bool isConnected
            {
                get { return Network.peerType != NetworkPeerType.Disconnected; }
            }

        }
    }
}
