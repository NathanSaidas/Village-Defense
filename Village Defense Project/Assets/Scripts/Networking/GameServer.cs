using UnityEngine;
using System;
using System.Collections.Generic;

namespace Gem
{
    namespace Networking
    {
        public class GameServer : BaseServer
        {
            private NetworkUser m_Host = NetworkUser.BAD_USER;
            private List<NetworkPlayer> m_NetworkPlayers = new List<NetworkPlayer>();
            private List<NetworkUser> m_NetworkUsers = new List<NetworkUser>();


            protected override void OnInitialize()
            {
                
            }

            protected override void OnDestroy()
            {
                
            }

            protected override void OnServerInitialized()
            {
                
            }

            protected override void OnServerShutdown()
            {
                
            }

            /// <summary>
            /// gets called when a player is connected.
            /// </summary>
            /// <param name="aPlayer"></param>
            public override void OnPlayerConnected(NetworkPlayer aPlayer)
            {
                m_NetworkPlayers.Add(aPlayer);
            }

            /// <summary>
            /// Gets called when a player is disconnected.
            /// </summary>
            /// <param name="aPlayer"></param>
            public override void OnPlayerDisconnected(NetworkPlayer aPlayer)
            {
                UnregisterPlayer(aPlayer);
                for(int i = 0; i < m_NetworkPlayers.Count; i++)
                {
                    if(m_NetworkPlayers[i].guid == aPlayer.guid)
                    {
                        m_NetworkPlayers.RemoveAt(i);
                    }
                }
            }

            /// <summary>
            /// Registers a user with a player. 
            /// 
            /// If the player does not exist this function returns false
            /// If the current connected users is greater than maxUsers this function returns false.
            /// </summary>
            /// <param name="aUser">The user registering.</param>
            /// <param name="aPlayer">The NetworkPlayer key used to check if they have already connected.</param>
            /// <returns>Returns true if successful, or false if unsuccessful.</returns>
            public bool RegisterPlayer(NetworkUser aUser, NetworkPlayer aPlayer)
            {
                //Failure: Server Full
                if(m_NetworkUsers.Count >= maxUsers)
                {
                    DebugUtils.LogError("Cannot register user because the server is already full",LogVerbosity.LevelThree);
                    return false;
                }

                //Failure: User already exists.
                foreach(NetworkUser user in m_NetworkUsers)
                {
                    if(user.username == aUser.username)
                    {
                        DebugUtils.LogError("Cannot register user because a user with that name already exists.\nUsername = " + aUser.username, LogVerbosity.LevelThree);
                        return false;
                    }
                }

                //Sucess:
                for (int i = 0; i < m_NetworkPlayers.Count; i++)
                {
                    if(m_NetworkPlayers[i] == aPlayer)
                    {
                        aUser.networkPlayer = m_NetworkPlayers[i];
                        m_NetworkPlayers.RemoveAt(i);
                        m_NetworkUsers.Add(aUser);
                        return true;
                    }
                }

                //Failure: User not connected.
                DebugUtils.LogError("Cannot register user because they are not connected", LogVerbosity.LevelThree);
                return false;
            }

            /// <summary>
            /// Unregisters a player using a NetworkPlayer as an ID.
            /// </summary>
            /// <param name="aPlayer">The player to unregister.</param>
            /// <returns>Returns true if the player was successfully unregistered, false otherwise.</returns>
            private bool UnregisterPlayer(NetworkPlayer aPlayer)
            {
                NetworkUser user = GetUser(aPlayer);
                if(user != NetworkUser.BAD_USER)
                {
                    return UnregisterPlayer(user.username);
                }
                return false;
            }

            /// <summary>
            /// Unregisters a player using the Users username.
            /// </summary>
            /// <param name="aUser">The user to unregister.</param>
            /// <returns>Returns true if the player was successfully unregistered, false otherwise.</returns>
            public bool UnregisterPlayer(NetworkUser aUser)
            {
                return UnregisterPlayer(aUser.username);
            }

            /// <summary>
            /// Unregisters a player of the specified username.
            /// </summary>
            /// <param name="aUsername">The username of the player.</param>
            /// <returns>Returns true if the player was successfully unregistered, false otherwise.</returns>
            public bool UnregisterPlayer(string aUsername)
            {
                for(int i = 0; i < m_NetworkUsers.Count; i++)
                {
                    if(m_NetworkUsers[i].username == aUsername)
                    {
                        m_NetworkUsers.RemoveAt(i);
                        return true;
                    }
                }
                DebugUtils.LogError("Failed to unregister player, the player " + aUsername + " does not exist", LogVerbosity.LevelThree);
                return false;
            }

            /// <summary>
            /// Kicks a player from the game server.
            /// </summary>
            /// <param name="aUsername">The username of the player to kick</param>
            /// <returns></returns>
            public bool KickPlayer(string aUsername)
            {
                NetworkUser user = GetUser(aUsername);
                if(user == NetworkUser.BAD_USER)
                {
                    return false;
                }

                Debug.Log("Kicking player: " + aUsername);

                for (int i = 0; i < m_NetworkPlayers.Count; i++ )
                {
                    if (m_NetworkPlayers[i].guid == user.guid)
                    {
                        m_NetworkPlayers.RemoveAt(i);
                        break;
                    }
                }
                return UnregisterPlayer(aUsername);
            }

            /// <summary>
            /// Gets the user based on the username.
            /// </summary>
            /// <param name="aUsername">The username of the user.</param>
            /// <returns></returns>
            public NetworkUser GetUser(string aUsername)
            {
                foreach(NetworkUser user in m_NetworkUsers)
                {
                    if(user.username == aUsername)
                    {
                        return user;
                    }
                }
                return NetworkUser.BAD_USER;
            }

            /// <summary>
            /// Gets the user based on the NetworkPlayer ID.
            /// </summary>
            /// <param name="aPlayer"></param>
            /// <returns></returns>
            public NetworkUser GetUser(NetworkPlayer aPlayer)
            {
                string guid = aPlayer.guid;

                foreach(NetworkUser user in m_NetworkUsers)
                {
                    
                    if(user.guid == guid)
                    {
                        return user;
                    }
                }
                return NetworkUser.BAD_USER;
            }

            public void KickUser(NetworkUser aUser)
            {
                NetworkUser user = GetUser(aUser.username);
                if(user != NetworkUser.BAD_USER)
                {
                    //Send Kick Message
                    //Broadcast Kick Message.
                    //Network.CloseConnection(user.networkPlayer, true);
                }
            }

            public override NetworkServer GetServerInfo()
            {
                return new NetworkServer(serverName, serverType, m_Host.username, connectedUsers);
            }

            public NetworkUser[] connectedUsers
            {
                get { return m_NetworkUsers.ToArray(); }
            }

            public NetworkUser host
            {
                get { return m_Host; }
                set { m_Host = value; }
            }

            public bool isFull
            {
                get { return m_NetworkUsers.Count >= maxUsers; }
            }

        }

    }
}

