#region CHANGE LOG
// -- April    16, 2015 - Nathan Hanlan - Added class AuthenticationServer from previous project.
// -- April    27, 2015 - Nathan Hanlan - Implemented BaseServer class.
#endregion

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Gem.Coroutines;

namespace Gem
{

    namespace Networking
    {


        public class AuthenticationServer : BaseServer, ICommandProcessor
        {
            private const int DEFAULT_MAX_CONNECTIONS = 64;
            private const int DEFAULT_PORT = 25070;

            public const int AUTHENTICATION_SERVER_FILE_VERSION = 1;
            public const string AUTHENTICATION_SERVER_DIRECTORY = "\\Ancients_Settlers_Pre_Alpha\\Authentication\\";
            public const string AUTHENTICATION_SERVER_FILE = "\\Ancients_Settlers_Pre_Alpha\\Authentication\\Authentication.bin";
            public const int AUTHENTICATION_SERVER_AUTOSAVE_INTERVAL = 60 * 5; // 5 Mins


            /// <summary>
            /// A dictionary of accounts.
            /// </summary>
            private Dictionary<string, AccountDetails> m_Accounts = new Dictionary<string, AccountDetails>();
            
            /// <summary>
            /// Whether or not the server needs to save.
            /// </summary>
            private bool m_IsDirty = false;

            /// <summary>
            /// The current time for the autosave interval
            /// </summary>
            private float m_CurrentTime = 0.0f;

            /// <summary>
            /// A flag for whether or not the server logs connections.
            /// </summary>
            private bool m_LogConnections = false;
            
            /// <summary>
            /// A routine used for cleaning up.
            /// </summary>
            private Routine m_CleanUp = null;

            /// <summary>
            /// Initialize the authentication server and start it.
            /// </summary>
            protected override void OnInitialize()
            {
                ///Set the defaults;
                maxUsers = DEFAULT_MAX_CONNECTIONS;
                maxConnections = DEFAULT_MAX_CONNECTIONS;
                port = DEFAULT_PORT;
                serverType = Constants.SERVER_TYPE_AUTHENTICATION;
                serverName = "GEM-Authentication";

                //Set the processor for processing commands from the debug console.
                DebugUtils.processor = this;
                //Load data from the hard disc.
                LoadData();
                //Set the autosave interval
                m_CurrentTime = AUTHENTICATION_SERVER_AUTOSAVE_INTERVAL;
                //Start the server with security.
                StartServer(true);
            }


            protected override void OnDestroy()
            {

            }

            protected override void OnServerInitialized()
            {
                m_LogConnections = true;
            }

            /// <summary>
            /// When the server is shut down save.
            /// </summary>
            protected override void OnServerShutdown()
            {
                Save();
            }

            /// <summary>
            /// Update for auto save.
            /// </summary>
            public override void Update()
            {
                m_CurrentTime -= Time.deltaTime;
                if (m_CurrentTime < 0.0f)
                {
                    m_CurrentTime = AUTHENTICATION_SERVER_AUTOSAVE_INTERVAL;
                    SetDirty();
                }
            }

            /// <summary>
            /// Log connections made.
            /// </summary>
            /// <param name="aPlayer">The player connecting.</param>
            public override void OnPlayerConnected(NetworkPlayer aPlayer)
            {
                if (m_LogConnections)
                {
                    DebugUtils.Log("Player Connected: " + aPlayer.externalIP);
                }
            }

            /// <summary>
            /// Log disconnections made.
            /// </summary>
            /// <param name="aPlayer">The player disconnecting.</param>
            public override void OnPlayerDisconnected(NetworkPlayer aPlayer)
            {
                if (m_LogConnections)
                {
                    DebugUtils.Log("Player Disconnected: " + aPlayer.externalIP);
                }
            }

            /// <summary>
            /// Adds an account under the following conditions.
            /// If GOOD is returned the account is added and the server is SetDirty
            /// 
            /// • Username cannot be empty or null
            /// • Password cannot be empty or null
            /// • Account must not already exist with the same username
            /// 
            /// </summary>
            /// <param name="aUsername">The username of the new account</param>
            /// <param name="aPassword">The password of the new account </param>
            /// <returns>Returns a code signalling the result of adding the account. See NetworkStatus constants</returns>
            public int AddAccount(string aUsername, string aPassword)
            {
                if (string.IsNullOrEmpty(aUsername))
                {

                    return NetworkStatus.INVALID_USERNAME;
                }
                else if (string.IsNullOrEmpty(aPassword))
                {
                    return NetworkStatus.INVALID_PASSWORD;
                }

                AccountDetails account = GetAccount(aUsername);
                if (account != null)
                {
                    if (m_LogConnections)
                    {
                        DebugUtils.Log("Failed to add user, user already exists");
                    }
                    return NetworkStatus.ACCOUNT_ALREADY_EXISTS;
                }

                ///Create the account
                account = new AccountDetails();
                account.username = aUsername;
                account.password = aPassword;

                m_Accounts.Add(account.username, account);
                SetDirty();
                return NetworkStatus.GOOD;
            }

            /// <summary>
            /// Removes an account under the following conditions.
            /// 
            /// 
            /// • Username cannot be empty or null
            /// • Password cannot be empty or null
            /// • The account must exist
            /// • The username and password must match
            /// 
            /// </summary>
            /// <param name="aUsername">The username of the account to be removed</param>
            /// <param name="aPassword">The password of the account to be removed</param>
            /// <returns>Returns a code signalling the result of removing the account. See NetworkStatus constants</returns>
            public int RemoveAccount(string aUsername, string aPassword)
            {
                if (string.IsNullOrEmpty(aUsername))
                {
                    return NetworkStatus.INVALID_USERNAME;
                }
                else if (string.IsNullOrEmpty(aPassword))
                {
                    return NetworkStatus.INVALID_PASSWORD;
                }

                AccountDetails account = GetAccount(aUsername);
                if (account == null)
                {
                    if (m_LogConnections)
                    {
                        DebugUtils.Log("Failed to add user, user does not exist");
                    }
                    return NetworkStatus.INVALID_USERNAME;
                }

                if (account.username != aUsername)
                {
                    return NetworkStatus.INVALID_USERNAME;
                }
                if (account.password != aPassword)
                {
                    return NetworkStatus.INVALID_PASSWORD;
                }

                m_Accounts.Remove(account.username);
                SetDirty();
                if (m_LogConnections)
                {
                    DebugUtils.Log("Authenticated User: " + account.username);
                }
                return NetworkStatus.GOOD;

            }


            /// <summary>
            /// Authenticates a user under the following conditions.
            /// 
            /// • Username cannot be empty or null
            /// • Password cannot be empty or null
            /// • The account must exist
            /// • The username and password must match
            /// 
            /// </summary>
            /// <param name="aUsername">The username of the account to be authenticated.</param>
            /// <param name="aPassword">The password of the account to be authenticated.</param>
            /// <returns>Returns a code signalling the result of authenticating the account. See NetworkStatus constants.</returns>
            public int Authenticate(string aUsername, string aPassword)
            {
                if (string.IsNullOrEmpty(aUsername))
                {
                    DebugUtils.LogError("Authentication Fail: INVALID_USERNAME(" + NetworkStatus.INVALID_USERNAME + ")");
                    return NetworkStatus.INVALID_USERNAME;
                }
                else if (string.IsNullOrEmpty(aPassword))
                {
                    DebugUtils.LogError("Authentication Fail: INVALID_PASSWORD(" + NetworkStatus.INVALID_PASSWORD + ")");
                    return NetworkStatus.INVALID_PASSWORD;
                }

                AccountDetails account = GetAccount(aUsername);
                if (account == null)
                {
                    DebugUtils.LogError("Authentication Fail: INVALID_USERNAME(" + NetworkStatus.INVALID_USERNAME + ")");
                    return NetworkStatus.INVALID_USERNAME;
                }

                if (account.username != aUsername)
                {
                    DebugUtils.LogError("Authentication Fail: INVALID_USERNAME(" + NetworkStatus.INVALID_USERNAME + ")");
                    return NetworkStatus.INVALID_USERNAME;
                }
                if (account.password != aPassword)
                {
                    DebugUtils.LogError("Authentication Fail: INVALID_PASSWORD(" + NetworkStatus.INVALID_PASSWORD + ")");
                    return NetworkStatus.INVALID_PASSWORD;
                }
                DebugUtils.Log("Authentication Successful");
                return NetworkStatus.GOOD;
            }

            /// <summary>
            /// Saves all the profiles to disk.
            /// </summary>
            public void Save()
            {
                //if (!m_StartServerOnLoad)
                //{
                //    return;
                //}
                DebugUtils.Log("Begin Save");
                try
                {
                    //FileStream stream = File.Open(Application.persistentDataPath + Constants.NETWORK_AUTHENTICATION_SERVER_FILE, FileMode.Create, FileAccess.Write);
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream();

                    formatter.Serialize(memoryStream, AUTHENTICATION_SERVER_FILE);
                    formatter.Serialize(memoryStream, AUTHENTICATION_SERVER_FILE_VERSION);
                    formatter.Serialize(memoryStream, m_Accounts.Count);

                    foreach (KeyValuePair<string, AccountDetails> account in m_Accounts)
                    {
                        account.Value.Save(memoryStream, formatter);
                    }

                    File.WriteAllBytes(Application.persistentDataPath + AUTHENTICATION_SERVER_FILE, memoryStream.ToArray());
                }
                catch (Exception aException)
                {
                    DebugUtils.LogException(aException);
                }
                DebugUtils.Log("Saving Complete");

            }


            /// <summary>
            /// Loads all the profiles from disk.
            /// </summary>
            public void LoadData()
            {
                //if (!m_StartServerOnLoad)
                //{
                //    return;
                //}
                DebugUtils.Log("Being Load");
                m_Accounts.Clear();

                try
                {

                    if (!Directory.Exists(Application.persistentDataPath + AUTHENTICATION_SERVER_DIRECTORY))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + AUTHENTICATION_SERVER_DIRECTORY);
                    }

                    if (!File.Exists(Application.persistentDataPath + AUTHENTICATION_SERVER_FILE))
                    {
                        Save();
                    }


                    byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + AUTHENTICATION_SERVER_FILE);

                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream(bytes);

                    string header = (string)formatter.Deserialize(memoryStream);
                    int version = (int)formatter.Deserialize(memoryStream);
                    int count = (int)formatter.Deserialize(memoryStream);

                    if (version != AUTHENTICATION_SERVER_FILE_VERSION)
                    {
                        DebugUtils.LogError("Invalid file version: " + header + " (" + version + ").\nCurrent Version: " + AUTHENTICATION_SERVER_FILE_VERSION);
                        return;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        AccountDetails account = new AccountDetails();
                        account.Load(memoryStream, formatter);
                        m_Accounts.Add(account.username, account);
                    }


                }
                catch (Exception aException)
                {
                    DebugUtils.LogException(aException);
                }
                DebugUtils.Log("Load Complete");
            }


            /// <summary>
            /// Retrieves an account using the username.
            /// </summary>
            /// <param name="aUsername"></param>
            /// <returns></returns>
            public AccountDetails GetAccount(string aUsername)
            {
                AccountDetails account = null;

                try
                {
                    if (m_Accounts.TryGetValue(aUsername, out account))
                    {
                        return account;
                    }
                }
                catch (Exception aException)
                {
                    DebugUtils.LogException(aException);
                }
                return null;
            }

            /// <summary>
            /// Used to make the server save at the end of frame.
            /// </summary>
            private void SetDirty()
            {
                if (!m_IsDirty)
                {
                    m_IsDirty = true;

                    if (m_CleanUp != null)
                    {
                        m_CleanUp.Stop();
                    }
                    m_CleanUp = new Routine(new YieldWaitEndOfFrame(1));
                    m_CleanUp.onCoroutineFinish = CleanUpRoutine;
                    m_CleanUp.Start();
                }
            }

            /// <summary>
            /// Saves the server at the end of the frame.
            /// </summary>
            private void CleanUpRoutine(CoroutineEx aRoutine)
            {
                Save();
                m_IsDirty = false;
            }


            #region CONSOLE

            #region COMMAND LIST
            // • clear
            // • isonline
            // • create <username> <password>
            // • delete <username> <password>
            // • authenticate <username> <password>
            // • save
            // • load
            // • logconnections <on/off>
            // • start
            // • stop 

            #endregion

            public void Process(List<string> aWords, List<string> aLowerWords)
            {
                if (aLowerWords == null || aLowerWords.Count == 0)
                {
                    return;
                }
                switch (aLowerWords[0])
                {
                    case "clear":
                        DebugUtils.ConsoleClear();
                        break;
                    case "help":
                    case "?":
                    case "cmd":
                    case "command":
                        {
                            DebugUtils.Log("--- List Of Available Commands ---");
                            DebugUtils.Log("clear");
                            DebugUtils.Log("isonline");
                            DebugUtils.Log("create <username> <password>");
                            DebugUtils.Log("delete <username> <password>");
                            DebugUtils.Log("authenticate <username> <password>");
                            DebugUtils.Log("save");
                            DebugUtils.Log("load");
                            DebugUtils.Log("logconnections <on/off>");
                            DebugUtils.Log("start");
                            DebugUtils.Log("stop [opt]<true/false>");
                        }
                        break;
                    case "isonline":
                        {
                            if (Network.isServer)
                            {
                                DebugUtils.Log("Server is online");
                            }
                            else
                            {
                                DebugUtils.Log("Server is offline");
                            }
                        }
                        break;
                    case "save":
                        {
                            SetDirty();
                        }
                        break;
                    case "load":
                        {
                            LoadData();
                        }
                        break;
                    case "authenticate":
                        {
                            if (aWords.Count >= 3)
                            {
                                int result = Authenticate(aWords[1], aWords[2]);
                                PrintResult(result);
                            }
                        }
                        break;
                    case "create":
                        {
                            if (aWords.Count >= 3)
                            {
                                int result = AddAccount(aWords[1], aWords[2]);
                                PrintResult(result);
                            }
                        }
                        break;
                    case "delete":
                        {
                            if (aWords.Count >= 3)
                            {
                                int result = RemoveAccount(aWords[1], aWords[2]);
                                PrintResult(result);
                            }
                        }
                        break;
                    case "logconnections":
                        {
                            if (aLowerWords.Count >= 2)
                            {
                                if (aLowerWords[1] == "on")
                                {
                                    m_LogConnections = true;
                                }
                                else if (aLowerWords[1] == "off")
                                {
                                    m_LogConnections = false;
                                }
                                else if (aLowerWords[1] == "?")
                                {
                                    if (m_LogConnections)
                                    {
                                        DebugUtils.Log("Logging Connections is On");
                                    }
                                    else
                                    {
                                        DebugUtils.Log("Logging Connections is Off");
                                    }
                                }
                            }
                        }
                        break;
                    case "start":
                        {
                            if (!Network.isServer)
                            {
                                StartServer();
                            }
                        }
                        break;
                    case "stop":
                        {
                            StopServer();
                            Application.Quit();
                        }
                        break;

                }
            }

            /// <summary>
            /// Log out a network request result to the console.
            /// </summary>
            /// <param name="aResult">The result to be logged out.</param>
            private void PrintResult(int aResult)
            {
                switch (aResult)
                {
                    case NetworkStatus.INVALID_USERNAME:
                        DebugUtils.Log("Bad Username String");
                        break;
                    case NetworkStatus.INVALID_PASSWORD:
                        DebugUtils.Log("Bad Password String");
                        break;
                    //case Constants.NETWORK_BAD_REQUEST:
                    //    DebugUtils.Log("Bad Request");
                    //    break;
                    //case Constants.NETWORK_INVALID_PASSWORD:
                    //    DebugUtils.Log("Invalid Password");
                    //    break;
                    //case Constants.NETWORK_INVALID_USERNAME:
                    //    DebugUtils.Log("Invalid Username");
                    //    break;
                    //case Constants.NETWORK_INVALID_USERNAME_OR_PASSWORD:
                    //    DebugUtils.Log("Invalid username or password");
                    //    break;
                    case NetworkStatus.ACCOUNT_ALREADY_EXISTS:
                        DebugUtils.Log("User Exists");
                        break;
                }
            }
            #endregion

        }
    }
}

