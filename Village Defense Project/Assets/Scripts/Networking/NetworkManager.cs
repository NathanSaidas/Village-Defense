using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections.Generic;
using Gem.Coroutines;
using Gem.Events;

#region CHANGE LOG
// -- April     28, 2015 - Nathan Hanlan - Tested Data Accessor Methods, Completed Authentication Server / Game Server functions.
#endregion

namespace Gem
{
    namespace Networking
    {
        

        /// <summary>
        /// This is a singleton class that manages the majority of networking done.
        /// 
        /// Quick Reference
        /// 
        /// ==Data Accessor Methods==
        ///     •GetCurrentUser
        ///     •IsLoggedIn
        ///     •IsServerHost
        ///     •GetConnectedServer
        ///     •GetTargetConnection
        ///     •LogOut
        ///     •LeaveLobby
        ///     •CreateGameServer
        ///     •DestroyGameServer
        ///     •Disconnect
        ///     •GetConnectedUsers
        ///     •GetRequest
        ///     •GetRequests
        ///     
        /// == Event Handling ==
        ///     • RegisterNetworkCallback
        ///     • UnregisterNetworkCallback
        /// 
        /// </summary>
        public class NetworkManager : MonoBehaviour
        {
            

            #region SINGLETON
            private static NetworkManager s_Instance = null;
            private static NetworkManager instance
            {
                get { if (s_Instance == null) { CreateInstance(); } return s_Instance; }
            }

            private static void CreateInstance()
            {
                GameObject persistent = GameObject.Find(Constants.GAME_OBJECT_PERSISTENT);
                if (persistent != null)
                {
                    s_Instance = persistent.GetComponent<NetworkManager>();
                }

                GameLoader.CheckGameState();
                if (!string.IsNullOrEmpty(GameLoader.loadError))
                {
                    Game.LoadFromStart();
                }
            }

            /// <summary>
            /// Claim ownership of the singleton instance.
            /// </summary>
            /// <param name="aInstance">The instance attempting to claim ownership.</param>
            /// <returns>Returns false if the instance was already owned.</returns>
            private static bool SetInstance(NetworkManager aInstance)
            {
                if (s_Instance != null && s_Instance != aInstance)
                {
                    return false;
                }
                s_Instance = aInstance;
                return true;
            }
            /// <summary>
            /// Remove ownership from singleton instance.
            /// </summary>
            /// <param name="aInstance">The instance trying to unclaim ownership.</param>
            private static void DestroyInstance(NetworkManager aInstance)
            {
                if (s_Instance == aInstance)
                {
                    s_Instance = null;
                }
            }

            public static bool Exists()
            {
                return s_Instance != null;
            }
            #endregion


            /// <summary>
            /// If set true the NetworkManager will configure an authentication server at start.
            /// </summary>
            [SerializeField]
            private bool m_ConfigureAuthenticationServer = false;

            /// <summary>
            /// A reference to the authentication server.
            /// </summary>
            private AuthenticationServer m_AuthenticationServer = null;

            /// <summary>
            /// A reference to the running game server. If this variable is not null the user is considered a host.
            /// </summary>
            private GameServer m_GameServer = null;

            /// <summary>
            /// Local NetworkView component of the network manager.
            /// </summary>
            private NetworkView m_NetworkView = null;

            #region GENERAL INFORMATION VARIABLES
            // ==========================
            // General Information Variables
            // ==========================

            /// <summary>
            /// Current user, gets set when authenticated and logged out. Access with GetCurrentUser() method.
            /// </summary>
            [SerializeField]
            private NetworkUser m_CurrentUser = NetworkUser.BAD_USER;

            /// <summary>
            /// Current server connected variable info.
            /// </summary>
            [SerializeField]
            private NetworkServer m_ConnectedServer = NetworkServer.BAD_SERVER;

            /// <summary>
            /// All of the users currently connected. The server will always update this.
            /// </summary>
            [SerializeField]
            private NetworkUser[] m_ConnectedUsers = null;

            /// <summary>
            /// Target connection info. The server the Network is attempting to connect to.
            /// </summary>
            [SerializeField]
            private NetworkServer m_TargetConnection = NetworkServer.BAD_SERVER;

            /// <summary>
            /// Current state of the network. Used to check errors.
            /// </summary>
            [DebugLabel]
            [SerializeField]
            private NetworkState m_CurrentState = NetworkState.Offline;

            #endregion

            #region SERVER POLLING ROUTINE VARIABLES
            // ==========================
            // Server Polling routines.
            // ==========================

            /// <summary>
            /// A routine used for getting the authentication servers.
            /// </summary>
            private RoutinePollServers m_AuthenticationPoll = null;
            /// <summary>
            /// A routine used for getting the game serers.
            /// </summary>
            private RoutinePollServers m_GamePoll = null;
            /// <summary>
            /// Represents the current routine active. If null it means no servers are being polled at that moment.
            /// </summary>
            private RoutinePollServers m_CurrentPoll = null;

            #endregion

            /// <summary>
            /// Dictionary of requests allow quick search for requests by key.
            /// </summary>
            private Dictionary<RequestType, List<Request>> m_Requests = new Dictionary<RequestType, List<Request>>();
            /// <summary>
            /// List of requests allow for easy traversal of requests to update.
            /// </summary>
            [DebugLabel]
            [SerializeField]
            private List<Request> m_RequestList = new List<Request>();

            #region EVENTS

            private event NetworkEventCallback m_OnPlayerConnected;
            private event NetworkEventCallback m_OnPlayerDisconnected;
            private event NetworkEventCallback m_OnRefreshConnections;

            #endregion


            #region DATA ACCESSOR METHODS
            /// <summary>
            /// Gets the current user.
            /// </summary>
            /// <returns>BAD_USER returns means there is no current user.</returns>
            public static NetworkUser GetCurrentUser()
            {
                if (instance != null)
                {
                    return instance.m_CurrentUser;
                }
                return NetworkUser.BAD_USER;
            }

            /// <summary>
            /// Sets the current user with the given username.
            /// </summary>
            /// <param name="aUsername">The username of the user</param>
            private static void SetCurrentUser(string aUsername)
            {
                if (string.IsNullOrEmpty(aUsername)
                    || aUsername.Contains("@"))
                {
                    DebugUtils.LogError(ErrorCode.INVALID_USERNAME);
                    return;
                }
                SetCurrentUser(new NetworkUser(aUsername));
            }

            /// <summary>
            /// Sets the current user.
            /// </summary>
            /// <param name="aUser">The current user.</param>
            private static void SetCurrentUser(NetworkUser aUser)
            {
                if (instance != null)
                {
                    instance.m_CurrentUser = aUser;
                }
            }

            /// <summary>
            /// Gets whether or not the player is logged in.
            /// </summary>
            /// <returns>Returns true if the state of the NetworkManager is not Offline</returns>
            public static bool IsLoggedIn()
            {
                return instance != null ? instance.m_CurrentState != NetworkState.Offline : false;
            }

            /// <summary>
            /// Determines if this player is the server host. If they are they have a game server.
            /// </summary>
            /// <returns>Returns true if the m_GameServer is not null.</returns>
            public static bool IsServerHost()
            {
                return instance != null ? instance.m_GameServer != null : false;
            }

            /// <summary>
            /// Gets the connected server
            /// </summary>
            /// <returns>Returns the currently connected server. BAD_SERVER means there is no connection made.</returns>
            public static NetworkServer GetConnectedServer()
            {
                return instance != null ? instance.m_ConnectedServer : NetworkServer.BAD_SERVER;
            }

            /// <summary>
            /// Sets the connected server.
            /// </summary>
            /// <param name="aServer">The info of the server to connect to.</param>
            private static void SetConnectedServer(NetworkServer aServer)
            {
                if (instance != null)
                {
                    if (instance.m_ConnectedServer != NetworkServer.BAD_SERVER)
                    {
                        DebugUtils.LogError(ErrorCode.CONNECTION_ALREADY_ESTABLISHED);
                        return;
                    }
                    instance.m_ConnectedServer = aServer;
                }
            }

            /// <summary>
            /// Gets the server the network is attempting to connect to.
            /// </summary>
            /// <returns>Returns the server the network is attempting to connect to.</returns>
            public static NetworkServer GetTargetConnection()
            {
                return instance != null ? instance.m_TargetConnection : NetworkServer.BAD_SERVER;
            }

            /// <summary>
            /// Sets the target connection.
            /// </summary>
            /// <param name="aServer"></param>
            private static void SetTargetConnection(NetworkServer aServer)
            {
                if (instance != null)
                {
                    instance.m_TargetConnection = aServer;
                }
            }

            /// <summary>
            /// Logs the server out. Disconnecting it from any server and destroys the server if they are host.
            /// </summary>
            public static void LogOut()
            {
                if (instance != null)
                {
                    if (instance.m_CurrentState != NetworkState.LoggedIn)
                    {
                        DebugUtils.LogWarning("Logging out from a state which is not LoggedIn may have unexpected results.\nCurrent State: " + instance.m_CurrentState);
                    }
                    if(IsServerHost())
                    {
                        DestroyGameServer();
                    }
                    else if(GetConnectedServer() != NetworkServer.BAD_SERVER)
                    {
                        Disconnect();
                    }
                    instance.m_CurrentState = NetworkState.Offline;
                    instance.m_CurrentUser = NetworkUser.BAD_USER;
                }
            }

            /// <summary>
            /// Leaves the game lobby and sets the state to being LoggedIn
            /// </summary>
            public static void LeaveLobby()
            {
                if (instance != null)
                {
                    if (instance.m_CurrentState != NetworkState.InLobby)
                    {
                        DebugUtils.LogWarning("Leaving the lobby while the state is not InLobby may have unexpected results.\nCurrent State: " + instance.m_CurrentState);
                    }
                    if (IsServerHost())
                    {
                        if (!Network.isServer)
                        {
                            DebugUtils.LogError(ErrorCode.INVALID_SERVER_HOST_STATE);
                        }
                        DestroyGameServer();
                    }
                    else
                    {
                        instance.m_CurrentState = NetworkState.LoggedIn;
                        Disconnect();
                    }


                }
            }

            #endregion

            #region GAME SERVER FUNCTIONS

            /// <summary>
            /// Creates the game server with the specified game name and default port.
            /// </summary>
            /// <param name="aGamename">The name of the server to create.</param>
            public static void CreateGameServer(string aGamename)
            {
                CreateGameServer(aGamename, Constants.DEFAULT_GAME_PORT);
            }

            /// <summary>
            /// Creates the game server with the specified game name and port.
            /// </summary>
            /// <param name="aGamename">The name of the server to create.</param>
            /// <param name="aPort">The port to use for creating the server.</param>
            public static void CreateGameServer(string aGamename, int aPort)
            {
                //Failure: No instance found
                if (instance == null)
                {
                    return;
                }
                //Failure: Game Server already exists.
                if(instance.m_GameServer != null)
                {
                    DebugUtils.LogError("Shutdown the current game server before creating a new one");
                    return;
                }

                //Warning: Should only create servers once authenticated.
                if (instance.m_CurrentState != NetworkState.LoggedIn)
                {
                    DebugUtils.LogWarning("Creating a game server while not logged in. Possible errors may exist.\nCurrent State: " + instance.m_CurrentState);
                }

                NetworkUser currentUser = GetCurrentUser();
                if(currentUser == NetworkUser.BAD_USER)
                {
                    DebugUtils.LogWarning("Creating a game server while the current user is a BAD_USER");
                }

                //Setup the game server
                GameServer gameServer = new GameServer();
                gameServer.serverName = aGamename;
                gameServer.serverType = Constants.SERVER_TYPE_GAME;
                gameServer.port = aPort;
                gameServer.maxConnections = 16;
                gameServer.maxUsers = 6;

                //Set the game server variable.
                instance.m_GameServer = gameServer;
                gameServer.Start();

                //Start the server.
                if(!gameServer.StartServer())
                {
                    instance.m_GameServer = null;
                    instance.m_ConnectedServer = NetworkServer.BAD_SERVER;
                    return;
                }
                //Setup the Host
                currentUser.networkPlayer = Network.player;
                gameServer.host = currentUser;
                gameServer.OnPlayerConnected(currentUser.networkPlayer);
                gameServer.RegisterPlayer(currentUser, currentUser.networkPlayer);
                SetCurrentUser(currentUser);
                
                //Set the states
                instance.m_CurrentState = NetworkState.InLobby;
                instance.m_ConnectedServer = gameServer.GetServerInfo();
            }

            /// <summary>
            /// Destroys the game server if there is a game server currently.
            /// </summary>
            public static void DestroyGameServer()
            {
                if(instance == null)
                {
                    return;
                }

                if(instance.m_GameServer == null)
                {
                    DebugUtils.LogError("Cannot destroy game server, game server is not running", LogVerbosity.LevelThree);
                    return;
                }

                if(!(instance.m_CurrentState == NetworkState.InLobby || instance.m_CurrentState == NetworkState.InGame))
                {
                    DebugUtils.LogWarning("State Error, destroying game server while not InLobby or InGame");
                }

                GameServer gameServer = instance.m_GameServer;

                instance.m_GameServer = null;
                instance.m_CurrentState = NetworkState.LoggedIn;
                instance.m_ConnectedServer = NetworkServer.BAD_SERVER;

                gameServer.Destroy();

                
            }

            /// <summary>
            /// Gets the connected users.
            /// </summary>
            /// <returns>Returns the connected users array.</returns>
            public static NetworkUser[] GetConnectedUsers()
            {
                if(instance == null)
                {
                    return null;
                }
                //Server Call
                if(instance.m_GameServer != null)
                {
                    return instance.m_GameServer.connectedUsers;
                }
                //Client Call
                else
                {
                    return instance.m_ConnectedUsers;
                }
            }

            /// <summary>
            /// Disconnects the client. This has no effect for server hosts, they should use DestroyGameServer instead.
            /// </summary>
            public static void Disconnect()
            {
                if (instance == null)
                {
                    return;
                }

                if (Network.isClient)
                {
                    instance.m_ConnectedServer = NetworkServer.BAD_SERVER;
                    Network.Disconnect();
                }
                else if (Network.isServer)
                {
                    DebugUtils.LogWarning("Servers cannot disconnect, use DestroyGameServer method instead");
                }

            }
            #endregion

            #region REQUEST MANAGEMENT

            /// <summary>
            /// Adds a request to the dictionary
            /// </summary>
            /// <param name="aRequest"></param>
            private static void AddRequest(Request aRequest)
            {
                if (instance == null)
                {
                    return;
                }
                //Requests should never be null.
                if (aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return;
                }

                //If the dictionary contains the queue try and get the queue and add the request.
                Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
                List<Request> queue = null;
                if (requests.ContainsKey(aRequest.type))
                {
                    if (requests.TryGetValue(aRequest.type, out queue))
                    {
                        if (queue != null)
                        {
                            queue.Add(aRequest);
                        }
                    }
                }
                else //Create a queue and add it.
                {
                    queue = new List<Request>();
                    queue.Add(aRequest);
                    requests.Add(aRequest.type, queue);
                }

                instance.m_RequestList.Add(aRequest);
            }

            /// <summary>
            /// Removes a request to the dictionary.
            /// </summary>
            /// <param name="aRequest"></param>
            private static void RemoveRequest(Request aRequest)
            {
                if (instance == null)
                {
                    return;
                }

                //Requests should never be null.
                if (aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return;
                }

                //If the dictionary contains the queue try and get the queue and remove the request.
                Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
                List<Request> queue = null;
                if (requests.TryGetValue(aRequest.type, out queue))
                {
                    if (queue != null)
                    {
                        if (queue.Remove(aRequest))
                        {
                            aRequest.id.Release();
                        }

                        if (queue.Count == 0)
                        {
                            requests.Remove(aRequest.type);
                        }
                    }
                }


                instance.m_RequestList.Remove(aRequest);
            }

            /// <summary>
            /// Finds a request from the dictionary using the ID and type as search paramaters.
            /// </summary>
            /// <param name="aID">The ID of the request.</param>
            /// <param name="aType">The type of request made.</param>
            /// <returns></returns>
            public static Request GetRequest(UID aID, RequestType aType)
            {
                if (instance == null)
                {
                    return null;
                }

                if (aID == UID.BAD_ID)
                {
                    DebugUtils.InvalidArgument("aID");
                    return null;
                }

                Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
                List<Request> queue = null;
                if (requests.TryGetValue(aType, out queue))
                {
                    if (queue != null)
                    {
                        return queue.FirstOrDefault<Request>(Element => Element.id == aID);
                    }
                }
                DebugUtils.LogError("Failed to get queue with key: " + aType);
                return null;
            }

            /// <summary>
            /// Finds all requests of a certain type.
            /// </summary>
            /// <param name="aType">The type of requests made.</param>
            /// <returns></returns>
            public static Request[] GetRequests(RequestType aType)
            {
                if (instance == null)
                {
                    return null;
                }

                Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
                List<Request> queue = null;
                if (requests.TryGetValue(aType, out queue))
                {
                    if (queue != null)
                    {
                        return queue.ToArray<Request>();
                    }
                }
                return null;
            }

            /// <summary>
            /// Completes the request invoking the callback.
            /// </summary>
            /// <param name="aRequestData">The request data to send back.</param>
            private static void CompleteRequest(RequestData aRequestData)
            {
                if(aRequestData.request == null)
                {
                    return;
                }

                SetRequestStatus(aRequestData.request, RequestStatus.Complete);
                aRequestData.request.Callback(aRequestData);
                SetRequestStatus(aRequestData.request, RequestStatus.Invalid);
                RemoveRequest(aRequestData.request);
            }

            #endregion

            #region AUTHENTICATION REQUESTS

            /// <summary>
            /// Sends an authentication request to the authentication server.
            /// RequestData = int
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed</param>
            /// <param name="aTimeout">How long before the request should time out.</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestAuthentication(RequestCallback aCallback, string aUsername, string aPassword, float aTimeout)
            {
                //Failure: No instance found.
                if (instance == null)
                {
                    return null;
                }
                //Check if the user is logged in. If they are make a null request.
                if (IsLoggedIn())
                {
                    DebugUtils.LogError(ErrorCode.USER_ALREADY_LOGGED_IN);
                    return null;
                }

                //Exit: Bad username
                if (string.IsNullOrEmpty(aUsername))
                {
                    DebugUtils.LogError(ErrorCode.INVALID_USERNAME);
                    return null;
                }

                //Exit: Bad password
                if (string.IsNullOrEmpty(aPassword))
                {
                    DebugUtils.LogError(ErrorCode.INVALID_PASSWORD);
                    return null;
                }

                //Exit: Not connected to authentication server.
                NetworkServer server = GetConnectedServer();
                if (server.serverType != Constants.SERVER_TYPE_AUTHENTICATION)
                {
                    DebugUtils.LogError(ErrorCode.INVALID_CONNECTION);
                    return null;
                }

                //Set the current user.
                SetCurrentUser(aUsername);
                NetworkUser currentUser = GetCurrentUser();

                //Create a request
                Request request = new Request(aCallback, currentUser, RequestType.Authentication, aTimeout);
                //Encrypt Sensitive Data
                string encryptedUsername = Security.EncryptString(currentUser.username);
                string encryptedPassword = Security.EncryptString(aPassword);

                //Create the packet.
                Packet packet = PacketFactory.CreateAuthenticationPacket(request, encryptedUsername, encryptedPassword);
                if(packet != null && packet.bytes != null)
                {
                    if(packet.bytes.Length == 0)
                    {
                        DebugUtils.LogError("The length of bytes of the packet is 0.");
                    }
                    Send(NetworkRPC.MANAGER_ON_REQUEST_AUTHENTICATION, RPCMode.Server, packet.bytes);
                    AddRequest(request);
                    return request;
                }
                return null;
            }

            /// <summary>
            /// Sends an authentication request to the authentication server.
            /// RequestData = int
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestAuthentication(RequestCallback aCallback, string aUsername, string aPassword)
            {
                return RequestAuthentication(aCallback, aUsername, aPassword, Constants.DEFAULT_TIME_OUT);
            }

            /// <summary>
            /// Sends a request to get the authentication servers.
            /// RequestData = NetworkServers[]
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed.</param>
            /// <param name="aTimeout">How long before the request should time out.</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestAuthenticationServers(RequestCallback aCallback, float aTimeout)
            {
                if (instance == null)
                {
                    return null;
                }
                Request request = new Request(aCallback, GetCurrentUser(), RequestType.AvailableAuthenticationServers, aTimeout);
                instance.GetAuthenticationServers(request);
                AddRequest(request);
                return request;
            }

            /// <summary>
            /// Sends a request to get the authentication servers.
            /// RequestData = NetworkServers[]
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed.</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestAuthenticationServers(RequestCallback aCallback)
            {
                return RequestAuthenticationServers(aCallback, Constants.DEFAULT_TIME_OUT);
            }

            #endregion

            #region SERVER CONNECTION REQUESTS

            /// <summary>
            /// Sends for a request of the available servers.
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed</param>
            /// <param name="aTimeout">How long before the request should time out.</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestAvailableServers(RequestCallback aCallback, float aTimeout)
            {
                if (instance == null)
                {
                    return null;
                }
                Request request = new Request(aCallback, GetCurrentUser(), RequestType.AvailableServers, aTimeout);
                instance.GetGameServers(request);
                AddRequest(request);
                return request;
            }

            /// <summary>
            /// Sends for a request of the available servers.
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestAvailableServers(RequestCallback aCallback)
            {
                return RequestAvailableServers(aCallback, Constants.DEFAULT_TIME_OUT);
            }

            /// <summary>
            /// Sends for a request to connect to a server.
            /// Request Data = object[2] where [0] = NetworkServer, [1] = int
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed</param>
            /// <param name="aServer"> The server to connect to.</param>
            /// <param name="aTimeout">How long before the request should time out.</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestConnection(RequestCallback aCallback, NetworkServer aServer, float aTimeout)
            {
                //Failure: Instance not created yet.
                if (instance == null)
                {
                    return null;
                }
                //Failure: Bad Server argument.
                if (aServer == NetworkServer.BAD_SERVER)
                {
                    DebugUtils.ArgumentNull("aServer");
                    return null;
                }

                //Exit: Already connected
                NetworkServer connectedServer = GetConnectedServer();
                if (connectedServer != NetworkServer.BAD_SERVER)
                {
                    DebugUtils.LogError(ErrorCode.CONNECTION_ALREADY_ESTABLISHED);
                    return null;
                }

                


                //Create request.
                Request request = null;

                if (aServer.serverType == Constants.SERVER_TYPE_AUTHENTICATION)
                {
                    request = new Request(aCallback, GetCurrentUser(), RequestType.ConnectionAuthentication, aTimeout);
                }
                else if (aServer.serverType == Constants.SERVER_TYPE_GAME)
                {
                    Request[] connectionRequests = GetRequests(RequestType.ConnectionGame);
                    if(connectionRequests != null && connectionRequests.Length > 0)
                    {
                        DebugUtils.LogError("Cannot make a request to connect to game server, request already made");
                        return null;
                    }
                    request = new Request(aCallback, GetCurrentUser(), RequestType.ConnectionGame, aTimeout);
                }
                //Try Connect
                SetTargetConnection(aServer);
                AddRequest(request);
                NetworkConnectionError error = Network.Connect(aServer.hostData);
                if (error != NetworkConnectionError.NoError)
                {
                    DebugUtils.LogError("Network: " + error.ToString());
                }
                return request;
            }

            /// <summary>
            /// Sends for a request of the curerntly connected players.
            /// Request Data = object[2] where [0] = NetworkServer, [1] = int
            /// </summary>
            /// <param name="aCallback">The callback to invoke when the request has been completed</param>
            /// <param name="aServer"> The server to connect to.</param>
            /// <returns>Returns a request.</returns>
            public static Request RequestConnection(RequestCallback aCallback, NetworkServer aServer)
            {
                return RequestConnection(aCallback, aServer, Constants.DEFAULT_TIME_OUT);
            }

            #endregion

            #region UTILITY

            /// <summary>
            /// A wrapper around the RPC function
            /// </summary>
            /// <param name="aRPCName">The name of the RPC function.</param>
            /// <param name="aMode">The players receiving the call.</param>
            /// <param name="aArgs">The arguments to be sent.</param>
            private static void Send(string aRPCName, RPCMode aMode, params object[] aArgs)
            {
                if (instance != null)
                {
                    NetworkView networkView = instance.m_NetworkView;
                    if (networkView != null)
                    {
                        networkView.RPC(aRPCName, aMode, aArgs);
                    }
                }
            }

            /// <summary>
            /// A wrapper around RPC function.
            /// </summary>
            /// <param name="aRPCName">The name of the RPC function.</param>
            /// <param name="aPlayer">The player receiving the call.</param>
            /// <param name="aArgs">The arguments to be sent.</param>
            private static void Send(string aRPCName, NetworkPlayer aPlayer, params object[] aArgs)
            {
                if (instance != null)
                {
                    NetworkView networkView = instance.m_NetworkView;
                    if (networkView != null)
                    {
                        networkView.RPC(aRPCName, aPlayer, aArgs);
                    }
                }
            }

            /// <summary>
            /// Logs out an RPC call error for clients.
            /// Use this when a server receives a client only RPC call
            /// </summary>
            /// <param name="aRPCName">The name of the RPC function.</param>
            /// <param name="aConstant">The constant within NetworkRPC that matches the RPC call.</param>
            private static void ClientRPCError(string aRPCName, string aConstant)
            {
                DebugUtils.LogError("Only Clients can receive this RPC call.\nRPC name = " + aRPCName + "\nConstant name = " + aConstant);
            }

            /// <summary>
            /// Logs out an RPC call error for servers.
            /// Use this when client receives a server only RPC call
            /// </summary>
            /// <param name="aRPCName">The name of the RPC function.</param>
            /// <param name="aConstant">The constant within NetworkRPC that matches the RPC call.</param>
            private static void ServerRPCError(string aRPCName, string aConstant)
            {
                DebugUtils.LogError("Only Servers can receive this RPC call.\nRPC name = " + aRPCName + "\nConstant name = " + aConstant);
            }

            /// <summary>
            /// Updates the connection list for the server and all connected peers. Called on Server Only. 
            /// </summary>
            private static void UpdateConnectionList()
            {
                if (instance == null)
                {
                    return;
                }

                if (!IsServerHost())
                {
                    return;
                }

                if (!Network.isServer)
                {
                    DebugUtils.LogError(ErrorCode.INVALID_SERVER_HOST_STATE);
                    return;
                }
                //Push the connected users to the field within the network manager.
                instance.m_ConnectedUsers = instance.m_GameServer.connectedUsers;
                //Create a packet with all of the users and send it across the network.
                Packet packet = PacketFactory.CreateConnectionListPacket(instance.m_ConnectedUsers);
                Send(NetworkRPC.MANAGER_ON_RECEIVE_CONNECTION_LIST, RPCMode.Others, packet.bytes);
                //Send event out for refreshing connection list.
                if(instance.m_OnRefreshConnections != null)
                {
                    instance.m_OnRefreshConnections.Invoke();
                }
            }

            /// <summary>
            /// Sets the reuqest status field of the Request using Reflection.
            /// </summary>
            /// <param name="aRequest">The request being affected.</param>
            /// <param name="aStatus">The status to set.</param>
            private static void SetRequestStatus(Request aRequest, RequestStatus aStatus)
            {
                //Use reflection to set the m_Status member of the Request to the given status.
                if (aRequest != null)
                {
                    Type type = aRequest.GetType();
                    type.InvokeMember("m_Status",
                        System.Reflection.BindingFlags.SetField |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance,
                        null,
                        aRequest,
                        new object[] { aStatus });
                }
            }

            /// <summary>
            /// Converts HostData into server data.
            /// </summary>
            /// <param name="aHostData">The host data to convert.</param>
            /// <returns>Returns all of the NetworkServers built from HostData</returns>
            private static NetworkServer[] ParseHostData(HostData[] aHostData)
            {
                if (aHostData != null && aHostData.Length > 0)
                {
                    NetworkServer[] servers = new NetworkServer[aHostData.Length];
                    for (int i = 0; i < servers.Length; i++)
                    {
                        NetworkServer serverData = new NetworkServer(aHostData[i]);
                        //serverData.hostData = aHostData[i];
                        //serverData.serverName = aHostData[i].gameName;
                        servers[i] = serverData;
                    }
                    return servers;
                }
                return null;
            }


            #endregion

            #region LOCAL
            private void Awake()
            {
                if (!SetInstance(this))
                {
                    Destroy(this);
                    return;
                }
                DontDestroyOnLoad(gameObject);
            }

            private void Start()
            {
                Application.runInBackground = true;
                m_NetworkView = GetComponent<NetworkView>();

                if (m_ConfigureAuthenticationServer)
                {
                    m_AuthenticationServer = new AuthenticationServer();
                    m_AuthenticationServer.Start();
                }

            }

            private void OnDestroy()
            {
                if (m_AuthenticationServer != null)
                {
                    m_AuthenticationServer.Destroy();
                }
                if(m_GameServer != null)
                {
                    m_GameServer.Destroy();
                }
                 
                DestroyInstance(this);

            }

            private void Update()
            {

                if (m_AuthenticationServer != null)
                {
                    m_AuthenticationServer.Update();
                }
                if(m_GameServer != null)
                {
                    m_GameServer.Update();
                    
                }

                //Timeout Requests.
                for (int i = m_RequestList.Count - 1; i >= 0; i--)
                {
                    if (m_RequestList[i].status == RequestStatus.TimedOut)
                    {
                        RemoveRequest(m_RequestList[i]);
                    }
                }
            }

            private void OnFailedToConnect(NetworkConnectionError aError)
            {
                DebugUtils.LogError("Failed to connect to server: " + aError.ToString());
            }

            private void OnFailedtoConnectToMasterServer(NetworkConnectionError aError)
            {
                DebugUtils.LogError("Failed to connect to master server: " + aError.ToString());
            }

            private void OnMasterServerEvent(MasterServerEvent aEvent)
            {

            }

            public void OnPlayerConnected(NetworkPlayer aPlayer)
            {
                if (m_AuthenticationServer != null)
                {
                    m_AuthenticationServer.OnPlayerConnected(aPlayer);
                }

                if(m_GameServer != null)
                {
                    m_GameServer.OnPlayerConnected(aPlayer);
                }
            }

            public void OnPlayerDisconnected(NetworkPlayer aPlayer)
            {
                if (m_AuthenticationServer != null)
                {
                    m_AuthenticationServer.OnPlayerDisconnected(aPlayer);
                }
                if(m_GameServer != null)
                {
                    NetworkUser user = m_GameServer.GetUser(aPlayer);
                    m_GameServer.OnPlayerDisconnected(aPlayer);
                    if(m_OnPlayerDisconnected != null && user != NetworkUser.BAD_USER)
                    {
                        m_OnPlayerDisconnected.Invoke(new EventProperty(Constants.NETWORK_EVENT_PROPERTY_DISCONNECTING_USERS, user));
                    }
                    UpdateConnectionList();
                }
            }

            private void OnConnectedToServer()
            {
                Debug.Log("Connected to server");
                //On connection to server
                //Get all requests to connect to a server. Authentication | Game and invoke the Callback.
                if (m_TargetConnection.serverType == Constants.SERVER_TYPE_AUTHENTICATION)
                {
                    SetConnectedServer(GetTargetConnection());
                    SetTargetConnection(NetworkServer.BAD_SERVER);

                    Request[] requests = GetRequests(RequestType.ConnectionAuthentication);
                    if (requests != null && requests.Length > 0)
                    {
                        foreach (Request request in requests)
                        {
                            RequestData requestData = new RequestData();
                            requestData.request = request;
                            requestData.data = new object[] { m_ConnectedServer, NetworkStatus.GOOD };
                            CompleteRequest(requestData);
                        }
                    }
                    else
                    {
                        Debug.Log("No requests to process");
                    }
                }
                //Register this player with the server.
                else if (m_TargetConnection.serverType == Constants.SERVER_TYPE_GAME)
                {
                    Request[] requests = GetRequests(RequestType.ConnectionGame);
                    if(requests != null && requests.Length >= 1)
                    {
                        Packet packet = PacketFactory.CreateConnectionRequestPacket(requests[0], GetCurrentUser());
                        if(packet != null)
                        {
                            Send(NetworkRPC.MANAGER_ON_REQUEST_CONNECTION, RPCMode.Server, packet.bytes);
                        }
                        else
                        {
                            DebugUtils.LogError("Failed to create packet, Disconnecting");
                            Disconnect();
                        }
                    }
                    else
                    {
                        DebugUtils.LogError("Connected to a server but there is no request to connect. Disconnecting");
                        Disconnect();
                    }
                }
                else
                {
                    DebugUtils.LogWarning("Connected to unknown server type: " + m_TargetConnection.serverType +". Disconnecting");
                    Disconnect();
                }

            }

            private void OnDisconnectedFromServer(NetworkDisconnection aInfo)
            {
                if (m_ConnectedServer != NetworkServer.BAD_SERVER)
                {
                    DebugUtils.LogError("Server disconnected incorrectly and was left in a bad state");
                }
                if (m_TargetConnection != NetworkServer.BAD_SERVER)
                {
                    DebugUtils.LogWarning("Target server is still currently but a connection was lost. Was this intentional?");
                }

                if(m_ConnectedServer != NetworkServer.BAD_SERVER)
                {
                    if(m_ConnectedServer.serverType == Constants.SERVER_TYPE_AUTHENTICATION)
                    {

                    }
                    else if(m_ConnectedServer.serverType == Constants.SERVER_TYPE_GAME)
                    {
                        m_CurrentState = NetworkState.LoggedIn;
                    }
                }

                m_ConnectedServer = NetworkServer.BAD_SERVER;
               

                if (Network.isServer)
                {

                }
                else
                {
                    
                }
            }

            private void OnServerInitialized()
            {

            }

            /// <summary>
            /// Starts a poll routine to get the authenticaiton servers.
            /// </summary>
            private void GetAuthenticationServers(Request aRequest)
            {
                if (aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return;
                }
                if (m_CurrentPoll != null)
                {
                    m_CurrentPoll.Stop();
                    Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableAuthenticationServers);
                    if (request == null)
                    {
                        request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableServers);
                    }
                    if (request != null)
                    {
                        SetRequestStatus(request, RequestStatus.Invalid);
                        RemoveRequest(request);
                        DebugUtils.LogWarning("Removing a request that could not be completed: " + request.id + " | " + request.type);
                    }
                }

                MasterServer.ClearHostList();
                MasterServer.RequestHostList(Constants.SERVER_TYPE_AUTHENTICATION);

                m_AuthenticationPoll = new RoutinePollServers(1.5f, Constants.SERVER_TYPE_AUTHENTICATION, aRequest.id);
                m_AuthenticationPoll.onCoroutineFinish = OnCoroutineFinish;
                m_AuthenticationPoll.onCoroutineStopped = OnCoroutineStop;
                m_CurrentPoll = m_AuthenticationPoll;
                m_CurrentPoll.Start();
                DebugUtils.Log("Polling for Authentication Servers");
            }

            /// <summary>
            /// Starts a poll routine to get the game servers.
            /// </summary>
            /// <param name="aRequest"></param>
            private void GetGameServers(Request aRequest)
            {
                if (aRequest == null)
                {
                    DebugUtils.ArgumentNull("aRequest");
                    return;
                }

                if (m_CurrentPoll != null)
                {
                    m_CurrentPoll.Stop();
                    Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableAuthenticationServers);
                    if (request == null)
                    {
                        request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableServers);
                    }
                    if (request != null)
                    {
                        SetRequestStatus(request, RequestStatus.Invalid);
                        RemoveRequest(request);
                        DebugUtils.LogWarning("Removing a request that could not be completed: " + request.id + " | " + request.type);
                    }
                }

                MasterServer.ClearHostList();
                MasterServer.RequestHostList(Constants.SERVER_TYPE_GAME);

                m_GamePoll = new RoutinePollServers(1.5f, Constants.SERVER_TYPE_GAME, aRequest.id);
                m_GamePoll.onCoroutineFinish = OnCoroutineFinish;
                m_GamePoll.onCoroutineStopped = OnCoroutineStop;
                m_CurrentPoll = m_GamePoll;
                m_CurrentPoll.Start();
            }

            /// <summary>
            /// Callback for PollRoutineStops. This function does not clean up requests.
            /// </summary>
            /// <param name="aRoutine"></param>
            private void OnCoroutineStop(CoroutineEx aRoutine)
            {
                if (aRoutine == null)
                {
                    throw new ArgumentNullException("aRoutine", "OnCoroutineStop should never receive a null routine");
                }

                if (aRoutine == m_CurrentPoll)
                {
                    m_CurrentPoll = null;
                }

            }

            /// <summary>
            /// Callback for PollRoutines
            /// </summary>
            /// <param name="aRoutine"></param>
            private void OnCoroutineFinish(CoroutineEx aRoutine)
            {
                if (aRoutine == null)
                {
                    DebugUtils.ArgumentNull("aRoutine", "OnCoroutineStop should never receive a null routine");
                }

                if (aRoutine != m_CurrentPoll)
                {
                    DebugUtils.InvalidArgument("aRoutine");
                }

                //If this was the authentication poll routine.
                if (aRoutine == m_AuthenticationPoll)
                {

                    //Parse the host data into server data.
                    NetworkServer[] servers = ParseHostData(MasterServer.PollHostList());
                    if (servers != null)
                    {
                        //Find the request and make the request data then set the status and invoke the callback.
                        Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableAuthenticationServers);
                        if (request != null)
                        {
                            //Create request data
                            RequestData requestData = new RequestData();
                            requestData.request = request;
                            requestData.data = servers;
                            CompleteRequest(requestData);    
                        }
                        else
                        {
                            DebugUtils.LogError(ErrorCode.INVALID_REQUEST);
                            DebugUtils.LogError("Invalid Request ID: " + m_CurrentPoll.requestID.id);
                        }

                    }
                    else
                    {
                        DebugUtils.Log("Authentication server poll finished and found no servers");
                    }

                }
                else if (aRoutine == m_GamePoll)
                {

                    NetworkServer[] servers = ParseHostData(MasterServer.PollHostList());
                    Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableServers);
                    if (request != null)
                    {
                        //Create request data
                        RequestData requestData = new RequestData();
                        requestData.request = request;
                        requestData.data = servers;
                        CompleteRequest(requestData);
                    }
                    else
                    {
                        DebugUtils.LogError("Failed to find request for GetGameServers");
                    }
                }

                m_CurrentPoll = null;
            }

            #endregion


            #region SERVER ONLY FUNCTIONS

            /// <summary>
            /// Gets called when a player requests a authentication
            /// </summary>
            /// <param name="aBytes"></param>
            [RPC]
            private void OnRequestAuthentication(byte[] aBytes, NetworkMessageInfo aInfo)
            {
                if(!Network.isServer)
                {
                    ServerRPCError("OnRequestAuthentication", NetworkRPC.MANAGER_ON_REQUEST_AUTHENTICATION);
                    return;
                }

                Packet packet = null;
                if (aBytes == null || aBytes.Length == 0)
                {
                    packet = PacketFactory.CreateAuthenticationStatusPacket(Request.EMPTY, NetworkStatus.ERROR);
                    DebugUtils.Log("Received bad bytes: OnRequestAuthentication");
                }
                else
                {
                    Request request = Request.EMPTY;
                    string encryptedUsername = string.Empty;
                    string encryptedPassword = string.Empty;
                    int status = NetworkStatus.ERROR;

                    if (PacketFactory.GetAuthenticationPacketData(new Packet(aBytes), out request, out encryptedUsername, out encryptedPassword))
                    {
                        if (!string.IsNullOrEmpty(encryptedUsername) && !string.IsNullOrEmpty(encryptedPassword))
                        {
                            string username = Security.DecryptString(encryptedUsername);
                            string password = Security.DecryptString(encryptedPassword);

                            AuthenticationServer server = m_AuthenticationServer;
                            if (server != null)
                            {
                                status = server.Authenticate(username, password);
                            }
                        }
                        else
                        {
                            DebugUtils.LogError("Bad Username");
                        }
                    }
                    else
                    {
                        DebugUtils.LogError("Couldn't decipher packet");
                    }
                    packet = PacketFactory.CreateAuthenticationStatusPacket(request, status);

                }
                Send(NetworkRPC.MANAGER_ON_RECEIVE_AUTHENTICATION_STATUS, aInfo.sender, packet.bytes);
            }

            /// <summary>
            /// Gets called when the client connects to register the player with the game server.
            /// </summary>
            /// <param name="aBytes"></param>
            /// <param name="aInfo"></param>
            [RPC]
            private void OnRequestConnection(byte[] aBytes, NetworkMessageInfo aInfo)
            {
                if(!Network.isServer)
                {
                    ServerRPCError("OnRequestConnection", NetworkRPC.MANAGER_ON_REQUEST_CONNECTION);
                    return;
                }

                int status = NetworkStatus.ERROR;
                Packet packet = null;
                Request request = Request.EMPTY;
                NetworkUser user = NetworkUser.BAD_USER;
                if(aBytes == null || aBytes.Length == 0 || m_GameServer == null)
                {
                    //Send status
                    packet = PacketFactory.CreateConnectionStatusPacket(Request.EMPTY, status);
                }
                else
                {
                    
                    if (PacketFactory.GetConnectionRequestPacketData(new Packet(aBytes), out request, out user))
                    {
                        if(user == NetworkUser.BAD_USER)
                        {
                            DebugUtils.LogError("Bad user attempting to login");
                            status = NetworkStatus.ERROR;
                        }
                        else if(m_GameServer.RegisterPlayer(user,aInfo.sender))
                        {
                            status = NetworkStatus.GOOD;
                        }
                        else
                        {
                            status = NetworkStatus.BAD;
                            if(m_GameServer.isFull)
                            {
                                status = NetworkStatus.FULL;
                            }
                        }
                    }
                    packet = PacketFactory.CreateConnectionStatusPacket(request, status);
                }

                Send(NetworkRPC.MANAGER_ON_RECEIVE_CONNECTIONS_STATUS, aInfo.sender, packet.bytes);

                if (status == NetworkStatus.GOOD)
                {
                    if(m_OnPlayerConnected != null)
                    {
                        m_OnPlayerConnected.Invoke(new EventProperty(Constants.NETWORK_EVENT_PROPERTY_CONNECTING_USERS, user));
                    }
                    UpdateConnectionList();
                    
                }
            }

            [RPC]
            private void OnRequestKick(byte[] aBytes, NetworkMessageInfo aInfo)
            {
                //TODO(Nathan): Check to see if the sender is a higher status than the person being kicked.
            }

            #endregion

            #region CLIENT ONLY FUNCTIONS

            [RPC]
            private void OnReceiveAuthenticationStatus(byte[] aBytes)
            {
                if (!Network.isClient)
                {
                    ClientRPCError("OnReceiveAuthenticationStatus", NetworkRPC.MANAGER_ON_RECEIVE_AUTHENTICATION_STATUS);
                    return;
                }

                if (aBytes != null && aBytes.Length > 0)
                {
                    Request request = Request.EMPTY;
                    int status = NetworkStatus.ERROR;

                    if(PacketFactory.GetAuthenticationStatusPacketData(new Packet(aBytes), out request, out status))
                    {
                        //If the status was bad. Set the user to bad user.
                        //Otherwise change state to Online
                        if (status == NetworkStatus.GOOD)
                        {
                            m_CurrentState = NetworkState.LoggedIn;
                        }
                        else
                        {
                            SetCurrentUser(NetworkUser.BAD_USER);
                        }

                        //Get the request and invoke the callback then remove it.
                        request = GetRequest(request.id, request.type);
                        if (request != null)
                        {
                            RequestData requestData = new RequestData();
                            requestData.request = request;
                            requestData.data = status;

                            CompleteRequest(requestData);
                        }
                    }
                    else
                    {
                        DebugUtils.LogError("Failed to get back packet data.");
                    }

                }
                else
                {
                    DebugUtils.LogError("Received bad data from connection status RPC callback");
                    
                }


                //Disconnect from authentication server.
                SetTargetConnection(NetworkServer.BAD_SERVER);
                //SetConnectedServer(NetworkServer.BAD_SERVER);
                Disconnect();

                
            }

            /// <summary>
            /// Gets called when the connection status is received from OnRequestConnection from the server.
            /// </summary>
            /// <param name="aBytes"></param>
            [RPC]
            private void OnReceiveConnectionStatus(byte[] aBytes)
            {
                if(!Network.isClient)
                {
                    ClientRPCError("OnReceiveConnectionStatus", NetworkRPC.MANAGER_ON_RECEIVE_CONNECTIONS_STATUS);
                    return;
                }

                if (aBytes != null && aBytes.Length > 0)
                {
                    Request request = Request.EMPTY;
                    int status = 0;

                    if (PacketFactory.GetConnectionStatusPacketData(new Packet(aBytes), out request, out status))
                    {
                        request = GetRequest(request.id, request.type);
                        if (status == NetworkStatus.GOOD)
                        {
                            
                            SetConnectedServer(GetTargetConnection());
                            SetTargetConnection(NetworkServer.BAD_SERVER);
                            
                        }
                        else
                        {
                            SetTargetConnection(NetworkServer.BAD_SERVER);
                            SetConnectedServer(NetworkServer.BAD_SERVER);
                            Disconnect();

                            if(status == NetworkStatus.FULL)
                            {
                                DebugUtils.LogError("Failed to connect to server, the server was full: " + status);
                            }
                            else if(status == NetworkStatus.ERROR)
                            {
                                DebugUtils.LogError("Failed to connect to server, there was an error: " + status);
                            }
                            else
                            {
                                DebugUtils.LogError("Failed to connect to server for reason: " + status);
                            }

                        }
                        //Complete the request
                        RequestData requestData = new RequestData();
                        requestData.request = request;
                        requestData.data = status;
                        CompleteRequest(requestData);
                    }
                }
                else
                {
                    DebugUtils.LogError("Received bad data from connection status RPC callback");
                    SetTargetConnection(NetworkServer.BAD_SERVER);
                    SetConnectedServer(NetworkServer.BAD_SERVER);
                    Disconnect();
                }
            }

            [RPC]
            private void OnReceiveConnectionList(byte[] aBytes)
            {
                NetworkUser[] users = null;
                if(PacketFactory.GetConnectionListPacketData(new Packet(aBytes), out users))
                {
                    m_ConnectedUsers = users;
                    if(m_OnRefreshConnections != null)
                    {
                        m_OnRefreshConnections.Invoke();
                    }
                }

            }


            [RPC]
            private void OnPlayerDisconnect(byte[] aBytes, NetworkPlayer aPlayer)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(aBytes);
                NetworkUser user = new NetworkUser();
                user.Deserialize(memoryStream, formatter);
                DisconnectReason reason = (DisconnectReason)formatter.Deserialize(memoryStream);

                DebugUtils.Log("User disconnected: " + user + "\nReason: " + reason);
            }


            public static void RegisterNetworkCallback(NetworkEvent aEvent, NetworkEventCallback aCallback)
            {
                if(instance == null)
                {
                    return;
                }

                if(aCallback == null)
                {
                    DebugUtils.ArgumentNull("aCallback");
                    return;
                }

                switch(aEvent)
                {
                    case NetworkEvent.OnPlayerConnected:
                        instance.m_OnPlayerConnected += aCallback;
                        break;
                    case NetworkEvent.OnPlayerDisconnected:
                        instance.m_OnPlayerDisconnected += aCallback;
                        break;
                    case NetworkEvent.OnRefreshConnections:
                        instance.m_OnRefreshConnections += aCallback;
                        break;
                }
            }
            public static void UnregisterNetworkCallback(NetworkEvent aEvent, NetworkEventCallback aCallback)
            {
                if (instance == null)
                {
                    return;
                }

                if (aCallback == null)
                {
                    DebugUtils.ArgumentNull("aCallback");
                    return;
                }

                switch (aEvent)
                {
                    case NetworkEvent.OnPlayerConnected:
                        instance.m_OnPlayerConnected -= aCallback;
                        break;
                    case NetworkEvent.OnPlayerDisconnected:
                        instance.m_OnPlayerDisconnected -= aCallback;
                        break;
                    case NetworkEvent.OnRefreshConnections:
                        instance.m_OnRefreshConnections -= aCallback;
                        break;
                }
            }

            #endregion
        }
    }
}


