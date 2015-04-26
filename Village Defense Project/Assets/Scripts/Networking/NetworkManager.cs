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

using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections.Generic;
using Gem.Coroutines;

namespace Gem
{
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
            if(persistent != null)
            {
                s_Instance = persistent.GetComponent<NetworkManager>();
            }

            GameLoader.CheckGameState();
            if(!string.IsNullOrEmpty(GameLoader.loadError))
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

        public const float DEFAULT_TIMEOUT = -1.0f;
        public const int DEFAULT_PORT = 25071;
        public const string AUTHENTICATION_TYPE = "GEM-VD-Authentication";
        public const string GAME_TYPE = "GEM-VD-Game";
        public const int MAX_USERS_PER_GAME = 6;

        /* 
         * Authentication Server Variables
         */
        [SerializeField]
        private bool m_ConfigureAuthenticationServer = false;
        private AuthenticationServer m_AuthenticationServer = null;
        //[DebugLabel]
        [SerializeField]
        private NetworkUser m_CurrentUser = NetworkUser.BAD_USER;
        //[DebugLabel]
        [SerializeField]
        private NetworkState m_CurrentState = NetworkState.Offline;
        private NetworkView m_NetworkView = null;
        //[DebugLabel]
        [SerializeField]
        private NetworkServer m_ConnectedServer = NetworkServer.BAD_SERVER;
        //[DebugLabel]
        [SerializeField]
        private NetworkServer m_TargetConnection = NetworkServer.BAD_SERVER;
        private bool m_IsHost = false;
        [DebugLabel]
        [SerializeField]
        private int m_GamePort = DEFAULT_PORT;
        [DebugLabel]
        [SerializeField]
        private string m_GameName = "";
        

        private RoutinePollServers m_AuthenticationPoll = null;
        private RoutinePollServers m_GamePoll = null;
        private RoutinePollServers m_CurrentPoll = null;

        private List<NetworkUser> m_ConnectedUsers = new List<NetworkUser>();

        //Requests Queues..
        private Dictionary<RequestType, List<Request>> m_Requests = new Dictionary<RequestType, List<Request>>();
        [DebugLabel]
        [SerializeField]
        private List<Request> m_RequestList = new List<Request>();




        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>BAD_USER returns means there is no current user.</returns>
        public static NetworkUser GetCurrentUser()
        {
            if(instance != null)
            {
                return instance.m_CurrentUser;
            }
            return NetworkUser.BAD_USER;
        }

        /// <summary>
        /// Sets the current user with the given username.
        /// </summary>
        /// <param name="aUsername"></param>
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
        /// <returns></returns>
        public static bool IsLoggedIn()
        {
            return instance != null ? instance.m_CurrentState == NetworkState.Online : false;
        }

        public static bool IsServerHost()
        {
            return instance != null ? (instance.m_IsHost && GetConnectedServer() != NetworkServer.BAD_SERVER) : false;
        }

        /// <summary>
        /// Gets the connected server
        /// </summary>
        /// <returns></returns>
        public static NetworkServer GetConnectedServer()
        {
            return instance != null ? instance.m_ConnectedServer : NetworkServer.BAD_SERVER;
        }

        private static void SetConnectedServer(NetworkServer aServer)
        {
            if(instance != null)
            {
                if(instance.m_ConnectedServer != NetworkServer.BAD_SERVER)
                {
                    DebugUtils.LogError(ErrorCode.CONNECTION_ALREADY_ESTABLISHED);
                    return;
                }
                instance.m_ConnectedServer = aServer;
            }
        }

        public static NetworkServer GetTargetConnection()
        {
            return instance != null ? instance.m_TargetConnection : NetworkServer.BAD_SERVER;
        }

        private static void SetTargetConnection(NetworkServer aServer)
        {
            if(instance != null)
            {
                instance.m_TargetConnection = aServer;
            }
        }

        public static void LogOut()
        {
            if(instance != null)
            {
                if(instance.m_CurrentState != NetworkState.LoggedIn)
                {
                    DebugUtils.LogWarning("Logging out from a state which is not LoggedIn may have unexpected results.\nCurrent State: " + instance.m_CurrentState);
                }
                instance.m_CurrentState = NetworkState.Offline;
                instance.m_CurrentUser = NetworkUser.BAD_USER;
            }
        }

        public static void LeaveLobby()
        {
            if(instance != null)
            {
                if(instance.m_CurrentState != NetworkState.InLobby)
                {
                    DebugUtils.LogWarning("Leaving the lobby while the state is not InLobby may have unexpected results.\nCurrent State: " + instance.m_CurrentState);
                }
                if(IsServerHost())
                {
                    if(!Network.isServer)
                    {
                        DebugUtils.LogError(ErrorCode.INVALID_SERVER_HOST_STATE);
                    }
                    DestroyGameServer();
                }
                else
                {
                    RequestDisconnect(null);
                }
                
                
            }
        }

        /// <summary>
        /// Creates the game server with the specified game name.
        /// </summary>
        /// <param name="aGamename">The name of the server to create.</param>
        public static void CreateGameServer(string aGamename)
        {
            CreateGameServer(aGamename, DEFAULT_PORT);
        }

        public static void CreateGameServer(string aGamename, int aPort)
        {
            if (instance == null)
            {
                return;
            }
            if (instance.m_CurrentState != NetworkState.LoggedIn)
            {
                DebugUtils.LogWarning("Creating a game server while not logged in. Possible errors may exist.\nCurrent State: " + instance.m_CurrentState);
            }
            //Setup game information.
            instance.m_GameName = aGamename;
            instance.m_GamePort = aPort;
            //Start the game
            StartGameServer();
        }

        

        private static void StartGameServer()
        {
            //Exit: No instance.
            if(instance == null)
            {
                return;
            }
            //Exit: User is not setup.
            NetworkUser currentUser = GetCurrentUser();
            if(currentUser == NetworkUser.BAD_USER)
            {
                DebugUtils.LogError("Failed to Start Game Server: Currenet user is BAD_USER");
                return;
            }

            NetworkConnectionError error = Network.InitializeServer(MAX_USERS_PER_GAME, instance.m_GamePort, !Network.HavePublicAddress());
            if(error == NetworkConnectionError.NoError)
            {
                //Register host and set the connected server.
                MasterServer.RegisterHost(NetworkManager.GAME_TYPE, instance.m_GameName + "-" + currentUser.username);
                instance.m_ConnectedServer = new NetworkServer(instance.m_GameName, GAME_TYPE);
                instance.m_IsHost = true; //this variable might be useless.
                instance.m_CurrentState = NetworkState.InLobby;
                currentUser.isHost = true;
                SetCurrentUser(currentUser);
            }
            else
            {
                DebugUtils.LogError("Failed to Start Game Server: " + error);
                instance.m_ConnectedServer = NetworkServer.BAD_SERVER;
            }
        }

        private static void DestroyGameServer()
        {
            MasterServer.UnregisterHost();
            Disconnect();
        }

        private static void Disconnect()
        {
            if(instance != null)
            {
                instance.m_ConnectedServer = NetworkServer.BAD_SERVER;
                if(Network.isClient || Network.isServer)
                {
                    Network.Disconnect();
                }
            }
        }

        /// <summary>
        /// Adds a request to the dictionary
        /// </summary>
        /// <param name="aRequest"></param>
        private static void AddRequest(Request aRequest)
        {
            if(instance == null)
            {
                return;
            }
            //Requests should never be null.
            if(aRequest == null)
            {
                DebugUtils.ArgumentNull("aRequest");
                return;
            }

            //If the dictionary contains the queue try and get the queue and add the request.
            Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
            List<Request> queue = null;
            if(requests.ContainsKey(aRequest.type))
            {
                if (requests.TryGetValue(aRequest.type, out queue))
                {
                    if(queue != null)
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
            if(instance == null)
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
            if(requests.TryGetValue(aRequest.type, out queue))
            {
                if(queue != null)
                {
                    if(queue.Remove(aRequest))
                    {
                        aRequest.id.Release();
                    }
                    
                    if(queue.Count == 0)
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
            if(instance == null)
            {
                return null;
            }

            if(aID == UID.BAD_ID)
            {
                DebugUtils.InvalidArgument("aID");
                return null;
            }

            Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
            List<Request> queue = null;
            if(requests.TryGetValue(aType,out queue))
            {
                if(queue != null)
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
            if(instance == null)
            {
                return null;
            }

            Dictionary<RequestType, List<Request>> requests = instance.m_Requests;
            List<Request> queue = null;
            if(requests.TryGetValue(aType,out queue))
            {
                if(queue != null)
                {
                    return queue.ToArray<Request>();
                }
            }
            return null;
        }

        public static NetworkUser[] GetConnectedUsers()
        {
            if(instance == null)
            {
                return null;
            }
            //TODO(Nathan): Sort Users

            return instance.m_ConnectedUsers.ToArray<NetworkUser>();
        }

        private static bool KickUser(NetworkUser aKickingPlayer, NetworkUser aKickedPlayer)
        {
            bool existed = false;
            //TODO(Nathan): Implement NetworkUser authority system.
            //If aKickingPlayer can kick aKickedPlayer
            {
                //TODO(Nathan): Implement user disconnect -> Send Message
                if(instance != null)
                {
                    existed = instance.m_ConnectedUsers.Remove(aKickedPlayer);
                }
            }

            //If the player didnt exist return false
            if(!existed)
            {
                DebugUtils.LogWarning(aKickedPlayer.username + " never existed");
                return false;
            }
            return false;
        }

        /// <summary>
        /// Sends an authentication request to the authentication server.
        /// RequestData = int
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestAuthentication(RequestCallback aCallback, string aUsername, string aPassword, float aTimeout)
        {
            if(instance == null)
            {
                return null;
            }
            //Check if the user is logged in. If they are make a null request.
            if(IsLoggedIn())
            {
                DebugUtils.LogError(ErrorCode.USER_ALREADY_LOGGED_IN);
                return null;
            }

            //Exit: Bad username
            if(string.IsNullOrEmpty(aUsername))
            {
                DebugUtils.LogError(ErrorCode.INVALID_USERNAME);
                return null;
            }

            //Exit: Bad password
            if(string.IsNullOrEmpty(aPassword))
            {
                DebugUtils.LogError(ErrorCode.INVALID_PASSWORD);
                return null;
            }

            //Exit: Not connected to authentication server.
            NetworkServer server = GetConnectedServer();
            if(server.serverType != AUTHENTICATION_TYPE)
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

            //Serialize all data into bytes
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            request.Serialize(memoryStream, formatter);
            formatter.Serialize(memoryStream, encryptedUsername);
            formatter.Serialize(memoryStream, encryptedPassword);

            byte[] bytes = memoryStream.ToArray();

            Send(NetworkRPC.MANAGER_ON_REQUEST_AUTHENTICATION, RPCMode.Server, bytes);

            AddRequest(request);
            return request;
        }

        /// <summary>
        /// Sends an authentication request to the authentication server.
        /// RequestData = int
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestAuthentication(RequestCallback aCallback, string aUsername, string aPassword)
        {
            return RequestAuthentication(aCallback,aUsername,aPassword, DEFAULT_TIMEOUT);
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
            if(instance == null)
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
            return RequestAuthenticationServers(aCallback, DEFAULT_TIMEOUT);
        }


        /// <summary>
        /// Sends for a request of the available servers.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestAvailableServers(RequestCallback aCallback, float aTimeout)
        {
            if(instance == null)
            {
                return null;
            }
            Request request = new Request(aCallback, GetCurrentUser(),RequestType.AvailableServers, aTimeout);
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
            return RequestAvailableServers(aCallback, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends for a request of the currently connected players.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestConnectionList(RequestCallback aCallback, float aTimeout)
        {
            if(instance == null)
            {
                return null;
            }

            //Exit: Not connected to authentication server.
            NetworkServer server = GetConnectedServer();
            if(server.serverType != GAME_TYPE)
            {
                DebugUtils.LogError(ErrorCode.INVALID_CONNECTION);
                return null;
            }

            Request request = new Request(aCallback, GetCurrentUser(), RequestType.ConnectionList, aTimeout);


            if(Network.isServer)
            {
                //If server.
                NetworkUser[] connectedUsers = GetConnectedUsers();
                RequestData requestData = new RequestData();
                requestData.request = request;
                requestData.data = connectedUsers;

                SetRequestStatus(request, RequestStatus.Complete);
                request.Callback(requestData);
                SetRequestStatus(request, RequestStatus.Invalid);

            }
            else if(Network.isClient)
            {
                //If client serialize and send request and wait.
                BinaryFormatter formattter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();

                request.Serialize(memoryStream, formattter);
                byte[] bytes = memoryStream.ToArray();

                Send(NetworkRPC.MANAGER_ON_REQUEST_CONNECTION_LIST, RPCMode.Server, bytes);
                AddRequest(request);
            }
            else
            {
                DebugUtils.LogError(ErrorCode.INVALID_NETWORK_STATE);
                SetRequestStatus(request, RequestStatus.Invalid);
            }
            return request;
        }

        /// <summary>
        /// Sends for a request of the currently connected players.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestConnectionList(RequestCallback aCallback)
        {
            return RequestConnectionList(aCallback, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends for a request to connect to a server.
        /// Request Data = object[2] where [0] = NetworkServer, [1] = int
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aServer"> The server to connect to.</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request  RequestConnection(RequestCallback aCallback, NetworkServer aServer, float aTimeout)
        {
            //Exit: Instance not created yet.
            if(instance == null)
            {
                return null;
            }
            //Exit: Bad Server
            if(aServer == NetworkServer.BAD_SERVER)
            {
                DebugUtils.ArgumentNull("aServer");
                return null;
            }

            //Exit: Already connected
            NetworkServer connectedServer = GetConnectedServer();
            if(connectedServer != NetworkServer.BAD_SERVER)
            {
                DebugUtils.LogError(ErrorCode.CONNECTION_ALREADY_ESTABLISHED);
                return null;
            }

            //Create request.
            Request request = null;
            
            if(aServer.serverType == AUTHENTICATION_TYPE)
            {
                request = new Request(aCallback, GetCurrentUser(), RequestType.ConnectionAuthentication, aTimeout);
            }
            else if(aServer.serverType == GAME_TYPE)
            {
                request = new Request(aCallback, GetCurrentUser(), RequestType.ConnectionGame, aTimeout);
            }
            
            
            //Try Connect
            SetTargetConnection(aServer);
            NetworkConnectionError error = Network.Connect(aServer.hostData);
            if(error != NetworkConnectionError.NoError)
            {
                DebugUtils.LogError("Network: " + error.ToString());
            }


            AddRequest(request);

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
            return RequestConnection(aCallback, aServer, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends for a request of the curerntly connected players.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aUser">The user to be kicked.</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestKick(RequestCallback aCallback, NetworkUser aUser, float aTimeout)
        {
            if(instance == null)
            {
                return null;
            }

            //Exit: Not connected to server.
            NetworkServer connectedServer = GetConnectedServer();
            if (connectedServer == NetworkServer.BAD_SERVER)
            {
                DebugUtils.LogError(ErrorCode.INVALID_CONNECTION);
                return null;
            }

            Request request = new Request(aCallback, GetCurrentUser(), RequestType.Kick, aTimeout);

            if(Network.isServer)
            {
                //Kick the user locally.
                int status = KickUser(request.user, aUser) ? NetworkStatus.GOOD : NetworkStatus.BAD;
                
                RequestData requestData = new RequestData();
                requestData.request = request;
                requestData.data = status;

                SetRequestStatus(request, RequestStatus.Complete);
                request.Callback(requestData);
                SetRequestStatus(request, RequestStatus.Invalid);
            }
            else if(Network.isClient)
            {
                //Send request to kick user.
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();
                request.Serialize(memoryStream, formatter);
                aUser.Serialize(memoryStream, formatter);
                byte[] bytes = memoryStream.ToArray();
                Send(NetworkRPC.MANAGER_ON_REQUEST_KICK, RPCMode.Server, bytes);
                AddRequest(request);
            }
            else
            {
                //Bad network state check server connection.
                DebugUtils.LogError(ErrorCode.INVALID_NETWORK_STATE);
                SetRequestStatus(request, RequestStatus.Invalid);
            }
            

            return request;
        }

        /// <summary>
        /// Sends for a request of the curerntly connected players.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aUser">The user to be kicked.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestKick(RequestCallback aCallback, NetworkUser aUser)
        {
            return RequestKick(aCallback, aUser, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends for a request to disconnect this client.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestDisconnect(RequestCallback aCallback, float aTimeout)
        {
            if(instance == null)
            {
                return null;
            }

            Request request = new Request(aCallback, GetCurrentUser(), RequestType.Disconnect, aTimeout);
            NetworkUser currentUser = GetCurrentUser();
            if(Network.isServer)
            {
                //TODO(Nathan): Find best host.
                //TODO(Nathan): Tell all clients to connect to new host
                //TODO(Nathan): Drop this server.
            }
            else if(Network.isClient)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();
                request.Serialize(memoryStream, formatter);
                currentUser.Serialize(memoryStream, formatter);
                formatter.Serialize(memoryStream, NetworkDisconnect.Quit);
                byte[] bytes = memoryStream.ToArray();
                Send(NetworkRPC.MANAGER_ON_REQUEST_DISCONNECT, RPCMode.Server, bytes);
                AddRequest(request);
            }
            else
            {
                //Bad network state check server connection.
                DebugUtils.LogError(ErrorCode.INVALID_NETWORK_STATE);
                SetRequestStatus(request, RequestStatus.Invalid);
            }
            return request;
        }

        /// <summary>
        /// Sends for a request to disconnect this client.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestDisconnect(RequestCallback aCallback)
        {
            return RequestDisconnect(aCallback,DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends for a request for the server to select the hero for this player.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aHeroType">The type of hero to select.</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestSelectHero(RequestCallback aCallback, int aHeroType, float aTimeout)
        {
            Request request = new Request(aCallback, GetCurrentUser(), RequestType.SelectHero, aTimeout);
            //TODO(Nathan): Send Request.



            

            return request;
        }

        /// <summary>
        /// Sends for a request for the server to select the hero for this player.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aHeroType">The type of hero to select.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestSelectHero(RequestCallback aCallback, int aHeroType)
        {
            return RequestSelectHero(aCallback,aHeroType, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends a request to start the game.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestStartGame(RequestCallback aCallback, float aTimeout)
        {
            Request request = new Request(aCallback, GetCurrentUser(), RequestType.StartGame, aTimeout);
            //TODO(Nathan): Send Request.
            return request;
        }

        /// <summary>
        /// Sends a request to start the game.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestStartGame(RequestCallback aCallback)
        {
            return RequestStartGame(aCallback, DEFAULT_TIMEOUT);
        }


        /// <summary>
        /// Sends a request to spawn the object for the player.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to spawn. </param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestSpawn(RequestCallback aCallback, int aObjectID, float aTimeout)
        {
            Request request = new Request(aCallback, GetCurrentUser(), RequestType.Spawn, aTimeout);
            //TODO(Nathan): Send Request.
            return request;
        }

        /// <summary>
        /// Sends a request to spawn the object for the player.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to spawn. </param>
        /// <returns>Returns a request.</returns>
        public static Request RequestSpawn(RequestCallback aCallback, int aObjectID)
        {
            return RequestSpawn(aCallback, aObjectID, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends a request for ownership over a object.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to get ownership of.</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestOwnership(RequestCallback aCallback, int aObjectID, float aTimeout)
        {
            Request request = new Request(aCallback, GetCurrentUser(), RequestType.Ownership, aTimeout);
            //TODO(Nathan): Send Request.
            return request;
        }

        /// <summary>
        /// Sends a request for ownership over a object.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to get ownership of. </param>
        /// <returns>Returns a request.</returns>
        public static Request RequestOwnership(RequestCallback aCallback, int aObjectID)
        {
            return RequestOwnership(aCallback, aObjectID, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends a request to destroy an object.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to get ownership of.</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestDestroy(RequestCallback aCallback, int aObjectID, float aTimeout)
        {
            Request request = new Request(aCallback, GetCurrentUser(), RequestType.Destroy, aTimeout);
            //TODO(Nathan): Send Request.
            return request;
        }

        /// <summary>
        /// Sends a request to destroy an object.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to get ownership of. </param>
        /// <returns>Returns a request.</returns>
        public static Request RequestDestroy(RequestCallback aCallback, int aObjectID)
        {
            return RequestDestroy(aCallback, aObjectID, DEFAULT_TIMEOUT);
        }

        /// <summary>
        /// Sends a request to issue an ability 
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to get ownership of.</param>
        /// <param name="aAbilityID">The ID of the ability for the object to use</param>
        /// <param name="aTimeout">How long before the request should time out.</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestIssueAbility(RequestCallback aCallback, int aObjectID, int aAbilityID, float aTimeout)
        {
            Request request = new Request(aCallback, GetCurrentUser(), RequestType.IssueAbility, aTimeout);
            //TODO(Nathan): Send Request.
            return request;
        }

        /// <summary>
        /// Sends a request to destroy an object.
        /// </summary>
        /// <param name="aCallback">The callback to invoke when the request has been completed</param>
        /// <param name="aObjectID">The object ID to get ownership of. </param>
        /// <param name="aAbilityID">The ID of the ability for the object to use</param>
        /// <returns>Returns a request.</returns>
        public static Request RequestIssueAbility(RequestCallback aCallback, int aObjectID, int aAbilityID)
        {
            return RequestIssueAbility(aCallback, aObjectID, aAbilityID, DEFAULT_TIMEOUT);
        }

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
        /// Updates the connection list for the server and all connected peers. Called on Server Only. 
        /// </summary>
        private static void UpdateConnectionList()
        {
            if(instance == null)
            {
                return;
            }

            if(!IsServerHost())
            {
                return;
            }

            if(!Network.isServer)
            {
                DebugUtils.LogError(ErrorCode.INVALID_SERVER_HOST_STATE);
                return;
            }

            int hostCount = 0;

            for (int i = 0; i < instance.m_ConnectedUsers.Count; i++)
            {
                if(instance.m_ConnectedUsers[i].isHost)
                {
                    hostCount++;
                    //Swap host
                    if(i != 0)
                    {
                        NetworkUser temp = instance.m_ConnectedUsers[0];
                        instance.m_ConnectedUsers[0] = instance.m_ConnectedUsers[i];
                        instance.m_ConnectedUsers[i] = temp;
                    }
                }
            }
            //Error Check
            if(hostCount > 1)
            {
                DebugUtils.LogError("Multiple hosts is not allowed.");
            }
            else if(hostCount == 0)
            {
                DebugUtils.LogError("None of the players are flagged as a host");
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            formatter.Serialize(memoryStream, instance.m_ConnectedUsers.Count);

            foreach(NetworkUser player in instance.m_ConnectedUsers)
            {
                player.Serialize(memoryStream, formatter);
            }

            byte[] bytes = memoryStream.ToArray();
            Send(NetworkRPC.MANAGER_ON_RECEIVE_CONNECTION_LIST, RPCMode.Others, bytes);
            
        }

        private static bool HandlePlayerDisconnect(NetworkUser aUser)
        {
            if(instance == null)
            {
                return false;
            }

            if(aUser == NetworkUser.BAD_USER)
            {
                DebugUtils.LogError("Cannot handle a user disconnect for a BAD_USER");
                return false;
            }
            for (int i = 0; i < instance.m_ConnectedUsers.Count; i++)
            {
                if(instance.m_ConnectedUsers[i] == aUser)
                {
                    instance.m_ConnectedUsers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        private static bool HandlePlayerDisconnect(NetworkPlayer aPlayer)
        {
            return HandlePlayerDisconnect(GetUser(aPlayer));
        }

        private static NetworkUser GetUser(NetworkPlayer aPlayer)
        {
            if(instance == null)
            {
                return NetworkUser.BAD_USER;
            }
            
            foreach(NetworkUser user in instance.m_ConnectedUsers)
            {
                if(user.networkPlayer == aPlayer)
                {
                    return user;
                }
            }

            return NetworkUser.BAD_USER;
        }

        /// <summary>
        /// Sets the reuqest status field of the Request using Reflection.
        /// </summary>
        /// <param name="aRequest">The request being affected.</param>
        /// <param name="aStatus">The status to set.</param>
        private static void SetRequestStatus(Request aRequest, RequestStatus aStatus)
        {
            if(aRequest != null)
            {
                Type type = aRequest.GetType();
                type.InvokeMember("m_Status", 
                    System.Reflection.BindingFlags.SetField | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance, 
                    null, 
                    aRequest, 
                    new object[] {aStatus});
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
            if(!SetInstance(this))
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

            if(m_ConfigureAuthenticationServer)
            {
                m_AuthenticationServer = new AuthenticationServer();
                m_AuthenticationServer.Start();
            }

        }

        private void OnDestroy()
        {
            if(m_AuthenticationServer != null)
            {
                m_AuthenticationServer.OnDestroy();
            }

            DestroyInstance(this);

        }

        private void Update()
        { 
            if(m_AuthenticationServer != null)
            {
                m_AuthenticationServer.Update();
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
        }

        public void OnPlayerDisconnected(NetworkPlayer aPlayer)
        {
            if (m_AuthenticationServer != null)
            {
                m_AuthenticationServer.OnPlayerDisconnected(aPlayer);
            }
        }

        private void OnConnectedToServer()
        {
            Debug.Log("Connected to server");
            //On connection to server
            //Get all requests to connect to a server. Authentication | Game and invoke the Callback.
            if (m_TargetConnection.serverType == AUTHENTICATION_TYPE)
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
                        requestData.data = new object[] {m_ConnectedServer, NetworkStatus.GOOD};

                        SetRequestStatus(request, RequestStatus.Complete);
                        request.Callback(requestData);
                        SetRequestStatus(request, RequestStatus.Invalid);
                        RemoveRequest(request);
                    }
                }
                else
                {
                    Debug.Log("No requests to process");
                }
            }
            else if (m_TargetConnection.serverType == GAME_TYPE)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream();

                NetworkUser currentPlayer = GetCurrentUser();
                currentPlayer.Serialize(memoryStream, formatter);
                byte[] bytes = memoryStream.ToArray();

                Send(NetworkRPC.MANAGER_ON_REQUEST_CONNECTION, RPCMode.Server, bytes);


            }
            else
            {
                DebugUtils.LogWarning("Connected to unknown server type: " + m_TargetConnection.serverType);
            }

        }

        private void OnDisconnectedFromServer(NetworkDisconnection aInfo)
        {
            if(m_ConnectedServer != NetworkServer.BAD_SERVER)
            {
                DebugUtils.LogError("Server disconnected incorrectly and was left in a bad state");
            }
            if(m_TargetConnection != NetworkServer.BAD_SERVER)
            {
                DebugUtils.LogWarning("Target server is still currently but a connection was lost. Was this intentional?");
            }

            if(Network.isServer)
            {

            }
            else
            {
                m_ConnectedServer = NetworkServer.BAD_SERVER;
            }
        }
        private void OnServerInitialized()
        {
            m_IsHost = true;
            m_ConnectedServer = new NetworkServer(m_GameName + "-" + GetCurrentUser().username, GAME_TYPE);
            //Make the host Data not null, not to confuse it with BAD_SERVER
            m_ConnectedServer.hostData = new HostData();

        }


        private bool AddUser(NetworkUser aUser)
        {
            if(!IsServerHost())
            {
                return false;
            }

            if(aUser == NetworkUser.BAD_USER)
            {
                return false;
            }

            //Can only add users while the host of a server.
            
            foreach(NetworkUser user in m_ConnectedUsers)
            {
                //Exit: User already exists.
                if (user.username == aUser.username)
                {
                    return false;
                }
            }

            m_ConnectedUsers.Add(aUser);

            //Sort and Fix
            for(int i = 0; i < m_ConnectedUsers.Count; i++)
            {
                if(m_ConnectedUsers[i].isHost)
                {
                    //Fix any problems
                    if(m_ConnectedUsers[i] != GetCurrentUser())
                    {
                        NetworkUser user = m_ConnectedUsers[i];
                        user.isHost = false;
                        m_ConnectedUsers[i] = user;
                    }
                    //Swap user to first index
                    else
                    {
                        if(i != 0)
                        {
                            NetworkUser user = m_ConnectedUsers[i];
                            m_ConnectedUsers.RemoveAt(i);
                            m_ConnectedUsers.Insert(0, user);
                        }
                    }
                }
            }
            return true;
        }

        private void RemoveUser(NetworkUser aUser)
        {
            if (!IsServerHost())
            {
                DebugUtils.LogError("Clients removing users is not allowed.");
                return;
            }

            if (aUser == NetworkUser.BAD_USER)
            {
                DebugUtils.LogError("Kicking BAD_USER is not allowed.");
                return;
            }



            //Swap Connection.
            if(aUser == GetCurrentUser())
            {
                SwapConnection();
            }
            else
            {
                for (int i = 0; i < m_ConnectedUsers.Count; i++)
                {
                    if(m_ConnectedUsers[i].username == aUser.username)
                    {
                        m_ConnectedUsers.RemoveAt(i);
                    }
                }
            }

        }

        private void SwapConnection()
        {
            //Find user with lowest ping.
            NetworkUser bestHost = m_ConnectedUsers[0];
            int lowestPing = Network.GetAveragePing(bestHost.networkPlayer);

            for (int i = 1; i < m_ConnectedUsers.Count; i++)
            {
                NetworkUser current = m_ConnectedUsers[i];
                int ping = Network.GetAveragePing(current.networkPlayer);
                if (ping < lowestPing)
                {
                    bestHost = current;
                    lowestPing = ping;
                }
            }


            bestHost.isHost = true;


            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            //Set bestHost as new host
            string gamename = "h-" + m_ConnectedServer.serverName + "-" + bestHost.username;
            bestHost.Serialize(memoryStream, formatter);
            formatter.Serialize(memoryStream, gamename);
            formatter.Serialize(memoryStream, m_ConnectedUsers.Count);

            for(int i = 0; i < m_ConnectedUsers.Count; i++)
            {
                if(m_ConnectedUsers[i] == m_CurrentUser)
                {
                    continue;
                }
                if(m_ConnectedUsers[i] == bestHost)
                {
                    m_ConnectedUsers[i] = bestHost;
                }

                m_ConnectedUsers[i].Serialize(memoryStream, formatter);
            }

            byte[] bytes = memoryStream.ToArray();

            Send(NetworkRPC.MANAGER_ON_ESTABLISH_NEW_HOST, RPCMode.Others, bytes);
        }


        /// <summary>
        /// Starts a poll routine to get the authenticaiton servers.
        /// </summary>
        private void GetAuthenticationServers(Request aRequest)
        {
            if(aRequest == null)
            {
                DebugUtils.ArgumentNull("aRequest");
                return;
            }
            if(m_CurrentPoll != null)
            {
                m_CurrentPoll.Stop();
                Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableAuthenticationServers);
                if(request == null)
                {
                    request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableServers);
                }
                if(request != null)
                {
                    SetRequestStatus(request, RequestStatus.Invalid);
                    RemoveRequest(request);
                    DebugUtils.LogWarning("Removing a request that could not be completed: " + request.id + " | " + request.type);
                }
            }

            MasterServer.ClearHostList();
            MasterServer.RequestHostList(AUTHENTICATION_TYPE);

            m_AuthenticationPoll = new RoutinePollServers(1.5f, AUTHENTICATION_TYPE, aRequest.id);
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
            if(aRequest == null)
            {
                DebugUtils.ArgumentNull("aRequest");
                return;
            }

            if(m_CurrentPoll != null)
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
            MasterServer.RequestHostList(GAME_TYPE);

            m_GamePoll = new RoutinePollServers(1.5f, GAME_TYPE, aRequest.id);
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
            if(aRoutine == null)
            {
                throw new ArgumentNullException("aRoutine", "OnCoroutineStop should never receive a null routine");
            }

            if(aRoutine == m_CurrentPoll)
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
            if(aRoutine == null)
            {
                DebugUtils.ArgumentNull("aRoutine", "OnCoroutineStop should never receive a null routine");
            }

            if(aRoutine != m_CurrentPoll)
            {
                DebugUtils.InvalidArgument("aRoutine");
            }

            //If this was the authentication poll routine.
            if(aRoutine == m_AuthenticationPoll)
            {
                
                //Parse the host data into server data.
                NetworkServer[] servers = ParseHostData(MasterServer.PollHostList());
                if(servers != null)
                {
                    //Find the request and make the request data then set the status and invoke the callback.
                    Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableAuthenticationServers);
                    if(request != null)
                    {
                        //Create request data
                        RequestData requestData = new RequestData();
                        requestData.request = request;
                        requestData.data = servers;
                        //Set the status and invoke the callback.
                        SetRequestStatus(request, RequestStatus.Complete);
                        request.Callback(requestData);
                        //Remove the request.
                        SetRequestStatus(request, RequestStatus.Invalid);
                        RemoveRequest(request);
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

            if(aRoutine == m_GamePoll)
            {
                NetworkServer[] servers = ParseHostData(MasterServer.PollHostList());
                if(servers != null)
                {
                    Request request = GetRequest(m_CurrentPoll.requestID, RequestType.AvailableServers);
                    if(request != null)
                    {
                        //Create request data
                        RequestData requestData = new RequestData();
                        requestData.request = request;
                        requestData.data = servers;
                        //Set the status and invoke the callback.
                        SetRequestStatus(request, RequestStatus.Complete);
                        request.Callback(requestData);
                        //Remove the request.
                        SetRequestStatus(request, RequestStatus.Invalid);
                        RemoveRequest(request);
                    }
                    else
                    {
                        DebugUtils.LogError("Failed to find request for GetGameServers");
                    }
                }
                else
                {
                    Debug.Log("No servers found");
                }
            }

            m_CurrentPoll = null;
        }

        


        #endregion


        #region SERVER ONLY FUNCTIONS

        #region GENERAL
        /// <summary>
        /// Gets called when a player requests a authentication
        /// </summary>
        /// <param name="aBytes"></param>
        [RPC]
        private void OnRequestAuthentication(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);

            Request request = new Request(null, new NetworkUser("@@Dummy@@"), RequestType.Authentication);
            request.Deserialize(memoryStream, formatter);

            //Change this value later.
            int status = 1;

            string encryptedUsername = (string)formatter.Deserialize(memoryStream);
            string encryptedPassword = (string)formatter.Deserialize(memoryStream);

            if(!string.IsNullOrEmpty(encryptedUsername) && !string.IsNullOrEmpty(encryptedPassword))
            {
                string username = Security.DecryptString(encryptedUsername);
                string password = Security.DecryptString(encryptedPassword);

                AuthenticationServer server = GetComponent<AuthenticationServer>();
                if(server != null)
                {
                    status = server.Authenticate(username, password);
                }
            }

            formatter = new BinaryFormatter();
            memoryStream = new MemoryStream();
            request.Serialize(memoryStream,formatter);
            formatter.Serialize(memoryStream, status);
            byte[] bytes = memoryStream.ToArray();

            Send(NetworkRPC.MANAGER_ON_RECEIVE_AUTHENTICATION_STATUS, aInfo.sender, bytes);
        }

        [RPC]
        private void OnRequestConnectionList(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Get the connection list and send it back
            if(!IsServerHost())
            {
                return;
            }

            if(!Network.isServer)
            {
                DebugUtils.LogError(ErrorCode.INVALID_SERVER_HOST_STATE);
                return;
            }

            UpdateConnectionList();
        }

        [RPC]
        private void OnRequestConnection(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);

            NetworkUser connectingPlayer = new NetworkUser();
            connectingPlayer.Deserialize(memoryStream, formatter);

            
            int status = NetworkStatus.ERROR;

            if(connectingPlayer == NetworkUser.BAD_USER)
            {
                DebugUtils.LogError("Bad user attempting to login");
                status = NetworkStatus.ERROR;
            }
            else if (m_ConnectedUsers.Count < MAX_USERS_PER_GAME)
            {
                status = NetworkStatus.GOOD;
            }
            else
            {
                status = NetworkStatus.BAD;
            }

            formatter = new BinaryFormatter();
            memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, status);
            byte[] bytes = memoryStream.ToArray();

            Send(NetworkRPC.MANAGER_ON_RECEIVE_CONNECTIONS_STATUS, aInfo.sender, bytes);

            if(status == NetworkStatus.GOOD)
            {
                connectingPlayer.networkPlayer = aInfo.sender;
                m_ConnectedUsers.Add(connectingPlayer);
                UpdateConnectionList();
            }

            

        }

        [RPC]
        private void OnRequestKick(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check to see if the sender is a higher status than the person being kicked.
        }

        [RPC]
        private void OnRequestDisconnect(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            if(!IsServerHost())
            {
                DebugUtils.LogError("Clients cannot handle a server request");
                return;
            }

            if(!Network.isServer)
            {
                DebugUtils.LogError(ErrorCode.INVALID_SERVER_HOST_STATE);
                return;
            }

            //Deserailize.
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);
            Request request = Request.EMPTY;
            request.Deserialize(memoryStream, formatter);
            NetworkUser user = new NetworkUser();
            user.Deserialize(memoryStream, formatter);
            NetworkDisconnect reason = (NetworkDisconnect)formatter.Deserialize(memoryStream);

            //Handle the disconnection.
            int status = HandlePlayerDisconnect(user) == true ? NetworkStatus.GOOD : NetworkStatus.ERROR;

            //Send the status back.
            formatter = new BinaryFormatter();
            memoryStream = new MemoryStream();
            request.Serialize(memoryStream, formatter);
            formatter.Serialize(memoryStream, status);
            byte[] bytes = memoryStream.ToArray();
            Send(NetworkRPC.MANAGER_ON_RECEIVE_DISCONNECT_STATUS, aInfo.sender, bytes);

            if(status == NetworkStatus.GOOD)
            {
                //Send a disconnect message to all players.
                formatter = new BinaryFormatter();
                memoryStream = new MemoryStream();
                user.Serialize(memoryStream, formatter);
                formatter.Serialize(memoryStream, reason);
                bytes = memoryStream.ToArray();
                Send(NetworkRPC.MANAGER_ON_PLAYER_DISCONNECT, RPCMode.Others, bytes, aInfo.sender);
                OnPlayerDisconnect(bytes, aInfo.sender);

                if (m_CurrentState == NetworkState.InLobby)
                {
                    //Update the connection list only while in the lobby and not in game.
                    UpdateConnectionList();
                }
            }
        }

        #endregion

        #region MAIN MENU

        [RPC]
        private void OnRequestSelectHero(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check if the hero is available and the game state is good.
            //TODO(Nathan): Send event across server to all clients to update their selection list
            //TODO(Nathan): Send back status GOOD or BAD.
        }
        
        [RPC]
        private void OnRequestStartGame(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check game state.
            //TODO(Nathan): Send event across server to all clients to start loading.
            //TODO(Nathan): Change game state.
        }

        #endregion

        #region GAMEPLAY

        [RPC]
        private void OnRequestSpawn(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check if player can do action, where action == spawn
            //TODO(Nathan): Spawn Object across server, give ownership to sending player.
        }

        [RPC]
        private void OnRequestOwnership(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check if player can do action, where action == claim ownership
            //TODO(Nathan): Set ownership of object across server.
        }

        [RPC]
        private void OnRequestDestroy(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check if player can do action, where action == destroy object
            //TODO(Nathan): Destroy object across server.
        }

        [RPC]
        private void OnRequestIssueAbility(byte[] aBytes, NetworkMessageInfo aInfo)
        {
            //TODO(Nathan): Check if the player owns the object
            //TODO(Nathan): Check if the unit can cast the ability
            //TODO(Nathan): Cast Ability - Notify all clients.
            //TODO(Nathan): 
            
        }


        #endregion

        #endregion

        #region CLIENT ONLY FUNCTIONS

        [RPC]
        private void OnReceiveAuthenticationStatus(byte[] aBytes)
        {
            //Deserialize the data. This order must match the order specified by OnRequestAuthentication method.
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);

            Request request = Request.EMPTY;
            request.Deserialize(memoryStream, formatter);
            int status = (int)formatter.Deserialize(memoryStream);

            //If the status was bad. Set the user to bad user.
            //Otherwise change state to Online
            if(status == NetworkStatus.GOOD)
            {
                m_CurrentState = NetworkState.LoggedIn;
            }
            else
            {
                SetCurrentUser(NetworkUser.BAD_USER);
            }

            //Get the request and invoke the callback then remove it.
            request = GetRequest(request.id, request.type);
            if(request != null)
            {
                RequestData requestData = new RequestData();
                requestData.request = request;
                requestData.data = status;

                SetRequestStatus(request, RequestStatus.Complete);
                request.Callback(requestData);
                SetRequestStatus(request, RequestStatus.Invalid);
                RemoveRequest(request);
            }
            //Disconnect from authentication server.
            Disconnect();
            SetTargetConnection(NetworkServer.BAD_SERVER);
        }


        [RPC]
        private void OnReceiveConnectionStatus(byte[] aBytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);
            int status = (int)formatter.Deserialize(memoryStream);

            if(status == NetworkStatus.GOOD)
            {
                SetConnectedServer(GetTargetConnection());
                SetTargetConnection(NetworkServer.BAD_SERVER);
            }
            else
            {
                SetTargetConnection(NetworkServer.BAD_SERVER);
                SetConnectedServer(NetworkServer.BAD_SERVER);
            }
            

            Request[] requests = GetRequests(RequestType.ConnectionGame);
            if (requests != null && requests.Length > 0)
            {
                foreach (Request request in requests)
                {
                    RequestData requestData = new RequestData();
                    requestData.request = request;
                    requestData.data = new object[] {m_ConnectedServer, status};

                    SetRequestStatus(request, RequestStatus.Complete);
                    request.Callback(requestData);
                    SetRequestStatus(request, RequestStatus.Invalid);
                    RemoveRequest(request);
                }
            }


            
        }

        [RPC]
        private void OnReceiveDisconnectStatus(byte[] aBytes)
        {
            //deserialize
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);
            Request request = Request.EMPTY;
            request.Deserialize(memoryStream, formatter);
            //NetworkDisconnect status = (NetworkDisconnect)formatter.Deserialize(memoryStream);

            //Get the request and restore the state.
            Disconnect();
            request = GetRequest(request.id, request.type);
            instance.m_CurrentState = NetworkState.LoggedIn;
            RemoveRequest(request);
        }

        [RPC]
        private void OnEstablishNewHost(byte[] aBytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);

            NetworkUser bestHost = NetworkUser.BAD_USER;

            bestHost.Deserialize(memoryStream, formatter);
            string gamename = (string)formatter.Deserialize(memoryStream);
            int count = (int)formatter.Deserialize(memoryStream);


            List<NetworkUser> users = new List<NetworkUser>();

            for (int i = 0; i < count; i++)
            {
                NetworkUser user = NetworkUser.BAD_USER;
                user.Deserialize(memoryStream, formatter);
                users.Add(user);
            }

            if(bestHost == NetworkUser.BAD_USER)
            {
                Debug.LogWarning("Received bad host");
            }

            
            if(m_CurrentUser == bestHost)
            {
                //If is host
                Network.Disconnect();
            }
            else
            {
                //Wait Connect
            }

        }

        [RPC]
        private void OnReceiveConnectionList(byte[] aBytes)
        {
            if(instance == null)
            {
                DebugUtils.LogError("Received Connection List: Missing instance");
                return;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            int count = (int)formatter.Deserialize(memoryStream);

            instance.m_ConnectedUsers.Clear();
            
            for(int i = 0; i < count; i++)
            {
                NetworkUser user = new NetworkUser();
                user.Deserialize(memoryStream, formatter);
                instance.m_ConnectedUsers.Add(user);
            }
        }

        [RPC]
        private void OnPlayerDisconnect(byte[] aBytes, NetworkPlayer aPlayer)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream(aBytes);
            NetworkUser user = new NetworkUser();
            user.Deserialize(memoryStream, formatter);
            NetworkDisconnect reason = (NetworkDisconnect)formatter.Deserialize(memoryStream);

            DebugUtils.Log("User disconnected: " + user);
        }

        [RPC]
        private void OnPlayerKicked(byte[] aBytes, NetworkPlayer aPlayer)
        {

        }

        #endregion

    }
}


