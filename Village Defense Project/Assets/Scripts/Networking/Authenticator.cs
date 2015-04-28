using UnityEngine;
using System.Collections;

namespace Gem
{
    namespace Networking
    {
        /// <summary>
        /// Delegate used for logging in.
        /// </summary>
        /// <param name="aStatus"></param>
        public delegate void LoginCallback(int aStatus);

        /// <summary>
        /// This class wraps the network authentication calls to authenticate an account.
        /// 
        /// Simply call login and check the status returned from the callback or the current network state.
        /// </summary>
        public class Authenticator
        {
            /// <summary>
            /// There can only be one login process at a time. Use the current property to see if a login process is currently happening.
            /// </summary>
            private static Authenticator s_Current = null;
            public static Authenticator current
            {
                get { return s_Current; }
            }

            /// <summary>
            /// Internal state.
            /// </summary>
            private enum State
            {
                None,
                GettingServers,
                Connecting,
                Authenticating,
            }
            /// <summary>
            /// The current request being made.
            /// </summary>
            private Request m_LoginRequest = null;
            /// <summary>
            /// The current state of the authentication process.
            /// </summary>
            private State m_CurrentState = State.None;

            /// <summary>
            /// The username for the authentication.
            /// </summary>
            private string m_Username = string.Empty;
            /// <summary>
            /// The password for the authentication.
            /// </summary>
            private string m_Password = string.Empty;
            /// <summary>
            /// The time for timeout.
            /// </summary>
            private float m_Timeout = 0.0f;
            /// <summary>
            /// A callback to notify of events.
            /// </summary>
            private LoginCallback m_Callback = null;


            /// <summary>
            /// Login with a username and password, uses a default time out.
            /// </summary>
            /// <param name="aUsername">The username to login with.</param>
            /// <param name="aPassword">The password to login with.</param>
            /// <returns>Returns true if login started successfull, returns false if there was an error.</returns>
            public bool Login(string aUsername, string aPassword)
            {
                return Login(aUsername, aPassword, null, Constants.DEFAULT_TIME_OUT);
            }

            /// <summary>
            /// Login with a username and password, uses a default time out.
            /// </summary>
            /// <param name="aUsername">The username to login with.</param>
            /// <param name="aPassword">The password to login with.</param>
            /// <param name="aTimeout"> The time to wait before timing out the request.</param>
            /// <returns>Returns true if login started successfull, returns false if there was an error.</returns>
            public bool Login(string aUsername, string aPassword, float aTimeout)
            {
                return Login(aUsername, aPassword, null,  aTimeout);
            }

            /// <summary>
            /// Login with a username and password, uses a default time out.
            /// </summary>
            /// <param name="aUsername">The username to login with.</param>
            /// <param name="aPassword">The password to login with.</param>
            /// <param name="aCallback">The callback to invoke for events.</param>
            /// <returns>Returns true if login started successfull, returns false if there was an error.</returns>
            public bool Login(string aUsername, string aPassword, LoginCallback aCallback)
            {
                return Login(aUsername, aPassword, aCallback, Constants.DEFAULT_TIME_OUT);
            }

            /// <summary>
            /// Login with a username and password, uses a default time out.
            /// </summary>
            /// <param name="aUsername">The username to login with.</param>
            /// <param name="aPassword">The password to login with.</param>
            /// <param name="aCallback">The callback to invoke for events.</param>
            /// <param name="aTimeout">The time to wait before timing out the request.</param>
            /// <returns>Returns true if login started successfull, returns false if there was an error.</returns>
            public bool Login(string aUsername, string aPassword, LoginCallback aCallback, float aTimeout)
            {
                if(current != null || m_LoginRequest != null || isLoggingIn)
                {
                    return false;
                }

                s_Current = this;

                m_Username = aUsername;
                m_Password = aPassword;
                m_Timeout = aTimeout;
                m_Callback = aCallback;

                m_LoginRequest = NetworkManager.RequestAuthenticationServers(LoginRoutine, m_Timeout);

                if(m_LoginRequest != null)
                {
                    m_CurrentState = State.GettingServers;
                    return true;
                }
                else
                {
                    Error();
                    return false;
                }
            }

            /// <summary>
            /// Handles all login requests after getting the servers.
            /// </summary>
            /// <param name="aData"></param>
            private void LoginRoutine(RequestData aData)
            {
                if(aData.request == m_LoginRequest)
                {
                    if(m_LoginRequest.status == RequestStatus.TimedOut)
                    {
                        m_LoginRequest = null;
                        DebugUtils.LogError("Login request timed out");
                        if(m_Callback != null)
                        {
                            m_Callback.Invoke(NetworkStatus.TIMED_OUT);
                        }
                        ClearState();
                        return;
                    }

                    if(m_CurrentState == State.GettingServers)
                    {
                        if(aData.data != null)
                        {
                            NetworkServer[] servers = (NetworkServer[])aData.data;
                            if(servers != null && servers.Length > 0)
                            {
                                m_LoginRequest = NetworkManager.RequestConnection(LoginRoutine, servers[0],m_Timeout);
                                m_CurrentState = State.Connecting;
                            }
                            else
                            {
                                Error();
                            }
                        }
                        else
                        {
                            Error();
                        }
                    }
                    else if(m_CurrentState == State.Connecting)
                    {
                        if(aData.data != null)
                        {
                            object[] data = (object[])aData.data;
                            if(data != null && data.Length == 2)
                            {
                                int status = (int)data[1];
                                if(status == NetworkStatus.GOOD)
                                {
                                    m_LoginRequest = NetworkManager.RequestAuthentication(LoginRoutine, m_Username, m_Password, m_Timeout);
                                    m_CurrentState = State.Authenticating;
                                }
                                else
                                {
                                    Error();
                                }
                            }
                            else
                            {
                                Error();
                            }
                        }
                        else
                        {
                            Error();
                        }
                    }
                    else if(m_CurrentState == State.Authenticating)
                    {
                        int status = (int)aData.data;
                        if(m_Callback != null)
                        {
                            m_Callback.Invoke(status);
                        }
                        ClearState();
                    }

                }
            }

            /// <summary>
            /// Invokes an error callback and clears the state.
            /// </summary>
            private void Error()
            {
                if(m_Callback != null)
                {
                    m_Callback.Invoke(NetworkStatus.ERROR);
                }
                ClearState();
            }

            /// <summary>
            /// Clears the state back to default state.
            /// </summary>
            private void ClearState()
            {
                m_CurrentState = State.None;
                m_Username = string.Empty;
                m_Password = string.Empty;
                m_Timeout = 0.0f;
                m_Callback = null;
                m_LoginRequest = null;
                if(s_Current == this)
                {
                    s_Current = null;
                }
            }
           
            public bool isLoggingIn
            {
                get { return m_CurrentState != State.None; }
            }

            public string currentState
            {
                get { return m_CurrentState.ToString(); }
            }

        }
    }
}


