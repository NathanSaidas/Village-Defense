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
/// -- April    16, 2015 - Nathan Hanlan - Added class AuthenticationServer from previous project.
#endregion

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Gem.Coroutines;

namespace Gem
{
    public class AuthenticationServer : ICommandProcessor
    {
        public const int AUTHENTICATION_SERVER_FILE_VERSION = 1;
        public const string AUTHENTICATION_SERVER_DIRECTORY = "\\Ancients_Settlers_Pre_Alpha\\Authentication\\";
        public const string AUTHENTICATION_SERVER_FILE = "\\Ancients_Settlers_Pre_Alpha\\Authentication\\Authentication.bin";
        public const int AUTHENTICATION_SERVER_AUTOSAVE_INTERVAL = 60 * 5; // 5 Mins

        private int m_MaxConnections = 64;
        private int m_Port = 25070;

        private Dictionary<string, AccountDetails> m_Accounts = new Dictionary<string, AccountDetails>();
        private bool m_IsDirty = false;
        private float m_CurrentTime = 0.0f;
        private bool m_LogConnections = false;

        private Routine m_CleanUp = null;

        // Use this for initialization
        public void Start()
        {
            Application.runInBackground = true;
            DebugUtils.processor = this;

            LoadData();
            m_CurrentTime = AUTHENTICATION_SERVER_AUTOSAVE_INTERVAL;

            StartServer();
        }


        public void OnDestroy()
        {
            StopServer(true);
        }

        // Update is called once per frame
        public void Update()
        {
            m_CurrentTime -= Time.deltaTime;
            if(m_CurrentTime < 0.0f)
            {
                m_CurrentTime = AUTHENTICATION_SERVER_AUTOSAVE_INTERVAL;
                SetDirty();
            }
        }


        public void OnPlayerConnected(NetworkPlayer aPlayer)
        {
            if (m_LogConnections)
            {
                DebugUtils.Log("Player Connected: " + aPlayer.externalIP);
            }
        }

        public void OnPlayerDisconnected(NetworkPlayer aPlayer)
        {
            if (m_LogConnections)
            {
                DebugUtils.Log("Player Disconnected: " + aPlayer.externalIP);
            }
        }

        /// <summary>
        /// Starts the server up.
        /// </summary>
        public void StartServer()
        {
            Network.InitializeSecurity();
            NetworkConnectionError error = Network.InitializeServer(m_MaxConnections, m_Port, !Network.HavePublicAddress());
            if (error == NetworkConnectionError.NoError)
            {
                MasterServer.RegisterHost(NetworkManager.AUTHENTICATION_TYPE, "Gem-Authentication-Server");
            }
            else
            {
                DebugUtils.LogError(error);
            }
        }

        /// <summary>
        /// Stops the server from running.
        /// </summary>
        /// <param name="aSave">Whether or not to save the server before stopping.</param>
        public void StopServer(bool aSave)
        {
            if (Network.isServer)
            {
                if (aSave)
                {
                    Save();
                }
                MasterServer.UnregisterHost();
                Network.Disconnect();
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

            if(account.username != aUsername)
            {
                return NetworkStatus.INVALID_USERNAME;
            }
            if(account.password != aPassword)
            {
                return NetworkStatus.INVALID_PASSWORD;
            }

            m_Accounts.Remove(account.username);
            SetDirty();
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

                if(version != AUTHENTICATION_SERVER_FILE_VERSION)
                {
                    DebugUtils.LogError("Invalid file version: " + header + " (" + version + ").\nCurrent Version: " + AUTHENTICATION_SERVER_FILE_VERSION);
                    return;
                }

                for(int i = 0; i < count; i++)
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

                if(m_CleanUp != null)
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

        
        /// <returns></returns>
        //IEnumerator<YieldInstruction> CleanUpRoutine()
        //{
        //    yield return new WaitForEndOfFrame();
        //    
        //}

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
        // • stop [opt]<true/false>

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
                        if (Network.isServer)
                        {
                            if (aLowerWords.Count > 1)
                            {
                                if (aLowerWords[1] == "true")
                                {
                                    StopServer(true);
                                }
                                else if (aLowerWords[1] == "false")
                                {
                                    StopServer(false);
                                }
                                else
                                {
                                    StopServer(true);
                                }
                            }
                            else
                            {
                                StopServer(true);
                            }
                        }
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

