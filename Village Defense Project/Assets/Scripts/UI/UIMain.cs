using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    /// <summary>
    /// The menu for the logging in process. Transitions to UIMatchFinder
    /// </summary>
    public class UIMain : UIMenu
    {
        
        [SerializeField]
        private UILoginWindow m_LoginWindow = null;
        [SerializeField]
        private Button m_PlayButton = null;
        [SerializeField]
        private Button m_OptionsButton = null;
        [SerializeField]
        private Button m_CreditsButton = null;
        [SerializeField]
        private Button m_QuitButton = null;

        //The current state of logging in.
        Request m_LoginRequest = null;

        private string m_Username = string.Empty;
        private string m_Password = string.Empty;

        

        void Start()
        {
            SetupButton(m_PlayButton, "m_PlayButton");
            SetupButton(m_OptionsButton, "m_OptionsButton");
            SetupButton(m_CreditsButton, "m_CreditsButton");
            SetupButton(m_QuitButton, "m_QuitButton");

            if(m_OptionsButton != null)
            {
                m_OptionsButton.interactable = false;
            }
            if(m_CreditsButton != null)
            {
                m_CreditsButton.interactable = false;
            }

            if(m_LoginWindow != null)
            {
                m_LoginWindow.loginCallback = OnLogin;
            }
        }

        

        void Update()
        {
            if(m_LoginWindow != null)
            {
                if (m_PlayButton != null)
                {
                    m_PlayButton.interactable = !m_LoginWindow.isShowing;
                }
                if (m_PlayButton != null)
                {
                    m_QuitButton.interactable = !m_LoginWindow.isShowing;
                }
            }
        }


        protected override void OnButtonClick(Button aButton)
        {
            if(aButton == m_PlayButton)
            {
                if(m_LoginWindow != null)
                {
                    m_LoginWindow.Show();
                }
            }
            else if(aButton == m_QuitButton)
            {
                Game.Quit();
            }
        }

        void OnLogin(string aUsername, string aPassword)
        {
            if(UIErrorWindow.current != null || m_LoginRequest != null)
            {
                return;
            }
            Debug.Log("Logging in with username: " + aUsername);
            m_Username = aUsername;
            m_Password = aPassword;

            m_LoginRequest = NetworkManager.RequestAuthenticationServers(OnReceiveAuthenticationServers, 35.0f);
        }

        void OnReceiveAuthenticationServers(RequestData aData)
        {
            CheckData(aData);
            if(aData.data == null)
            {
                UIErrorWindow.Create("Authentication Error", "Error reading network data.");
                m_LoginRequest = null;
                return;
            }
            NetworkServer[] servers = (NetworkServer[])aData.data;

            if(servers != null && servers.Length > 0)
            {
                m_LoginRequest = NetworkManager.RequestConnection(OnConnectToAuthenticationServer, servers[0], 35.0f);
            }
        }

        void OnConnectToAuthenticationServer(RequestData aData)
        {
            
            CheckData(aData);
            if (aData.data == null)
            {
                UIErrorWindow.Create("Authentication Error", "Error reading network data.");
                m_LoginRequest = null;
                DebugUtils.LogError("Error reading network data");
                return;
            }
            object[] data = (object[])aData.data;
            if(data != null && data.Length == 2)
            {
                int status = (int)data[1];
                if(status == NetworkStatus.GOOD)
                {
                    Debug.Log("Requesting authentication");
                    m_LoginRequest = NetworkManager.RequestAuthentication(OnAuthenticateAccount, m_Username, m_Password);
                }
                else
                {
                    UIErrorWindow.Create("Authentication Error", "Could not reach server");
                    m_LoginRequest = null;
                }
            }
            else
            {
                UIErrorWindow.Create("Authentication Error", "Error reading network data.");
                m_LoginRequest = null;
                return;
            }
        }

        void OnAuthenticateAccount(RequestData aData)
        {
            CheckData(aData);
            m_Password = string.Empty;
            m_Username = string.Empty;
            m_LoginRequest = null;

            if (aData.data == null)
            {
                UIErrorWindow.Create("Authentication Error", "Error reading network data.");
                
                return;
            }

            int status = (int)aData.data;

            if(status == NetworkStatus.GOOD)
            {
                Next();
                if(m_LoginWindow != null)
                {
                    m_LoginWindow.Hide();
                }
            }
            else if(status == NetworkStatus.INVALID_USERNAME)
            {
                UIErrorWindow.Create("Authentication Error", "Invalid username");
                DebugUtils.LogError("Invalid username");
            }
            else if(status == NetworkStatus.INVALID_PASSWORD)
            {
                UIErrorWindow.Create("Authentication Error", "Invalid password");
                DebugUtils.LogError("Invalid password");
            }
            else
            {
                DebugUtils.LogError("Invalid network status returned");
                DebugUtils.LogError("Invalid status " + status);
            }
        }

        void CheckData(RequestData aData)
        {
            if (aData.request != m_LoginRequest)
            {
                DebugUtils.LogError("Bad request match");
            }
        }
    }

}


