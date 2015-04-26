using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    public class UIMatchFinder : UIMenu
    {
        /// <summary>
        /// A window to allow the user to specify the name of the game they wish to create.
        /// </summary>
        [SerializeField]
        private UICreateGameWindow m_CreateGameWindow = null;
        /// <summary>
        /// A scroll area for viewing available matches.
        /// </summary>
        [SerializeField]
        private UIScrollArea m_MatchListView = null;
        /// <summary>
        /// The prefab to use for matches used in the Match List View
        /// Requires component UIGameLobby
        /// </summary>
        [SerializeField]
        private GameObject m_Prefab = null;

        /// <summary>
        /// An input field to specify the search parameters.
        /// </summary>
        [SerializeField]
        private InputField m_SearchFilter = null;
        /// <summary>
        /// A button to signal creation of a game.
        /// </summary>
        [SerializeField]
        private Button m_CreateButton = null;
        /// <summary>
        /// A button to signal refreshing of the match list view.
        /// </summary>
        [SerializeField]
        private Button m_RefreshButton = null;
        /// <summary>
        /// A button for quitting back to the main menu
        /// </summary>
        [SerializeField]
        private Button m_QuitButton = null;
        /// <summary>
        /// A button for the options.
        /// </summary>
        [SerializeField]
        private Button m_OptionsButton = null;
        /// <summary>
        /// The rate at which to refresh the Match List view. This value cannot be lower than 1.5
        /// </summary>
        [SerializeField]
        private float m_RefreshRate = 2.0f;


        /// <summary>
        /// The current time of the timer.
        /// </summary>
        private float m_CurrentTime = 0.0f;
        /// <summary>
        /// Whether or not a request has been made.
        /// </summary>
        private bool m_MadeRequest = false;
        /// <summary>
        /// Whether or not the player is currently trying to join a server.
        /// </summary>
        private bool m_JoiningServer = false;
        /// <summary>
        /// An array of currently available servers.
        /// </summary>
        private NetworkServer[] m_CurrentServers = null;
        /// <summary>
        /// A buffer for servers to go into before being processed.
        /// </summary>
        private NetworkServer[] m_BufferServers = null;

        private Request m_ConnectionRequest = null;
        private Request m_GetServersRequest = null;

        /// <summary>
        /// Initialize the UI Match Finder.
        /// </summary>
        private void Start()
        {
            //Setup buttons
            SetupButton(m_CreateButton, "m_CreateButton");
            SetupButton(m_RefreshButton, "m_RefreshButton");
            SetupButton(m_QuitButton, "m_QuitButton");
            SetupButton(m_OptionsButton, "m_OptionsButton");

            //Disable the options button till options menu is created.
            if(m_OptionsButton != null)
            {
                m_OptionsButton.interactable = false;
            }

            //Register the search filter for value changes
            if(m_SearchFilter != null)
            {
                m_SearchFilter.onValueChange.AddListener((text) => OnSearchValueChange(text));
            }

            //Register the window callback to get CreateGame events.
            if(m_CreateGameWindow != null)
            {
                m_CreateGameWindow.createGameCallback = OnCreateGame;
            }
        }


        /// <summary>
        /// A callback used to handle button clicks.
        /// </summary>
        /// <param name="aButton">Any button used with SetupButton method</param>
        protected override void OnButtonClick(Button aButton)
        {
            //Ignore events while creating game.
            if(m_CreateGameWindow != null && m_CreateGameWindow.isShowing)
            {
                return;
            }

            if(m_CreateButton == aButton)
            {
                if(m_CreateGameWindow != null)
                {

                    m_CreateGameWindow.Show();
                }
            }

            if(m_RefreshButton == aButton)
            {
                GetServers();
            }

            if(m_OptionsButton == aButton)
            {
                //TODO(Nathan): Show the options menu.
            }

            if(m_QuitButton == aButton)
            {
                NetworkManager.LogOut();
                Previous();
            }

        }

        /// <summary>
        /// Gets called when the search field is changed.
        /// </summary>
        /// <param name="aText">The text of the search field.</param>
        private void OnSearchValueChange(string aText)
        {
            //Refresh the list.
            RefreshList();
        }

        /// <summary>
        /// Gets called when the create window closes successfully.
        /// </summary>
        /// <param name="aGamename">The name of the game to create</param>
        private void OnCreateGame(string aGamename)
        {
            NetworkManager.CreateGameServer(aGamename);
            if (NetworkManager.GetConnectedServer() != NetworkServer.BAD_SERVER)
            {
                //Reset the game window and go to the next menu.
                Next();
                if(m_CreateGameWindow != null)
                {
                    m_CreateGameWindow.Hide();
                    if(m_CreateGameWindow.gameNameInputField != null)
                    {
                        m_CreateGameWindow.gameNameInputField.text = string.Empty;
                    }
                }
            }
            else
            {
                DebugUtils.LogError("Failed to create a game with the name " + aGamename);
            }
        }

        /// <summary>
        /// Gets called when joining a game.
        /// </summary>
        /// <param name="aGamename">The name of the game the player is trying to join.</param>
        private void OnJoinGame(string aGamename)
        {
            if(m_JoiningServer)
            {
                return;
            }
            foreach(NetworkServer server in m_CurrentServers)
            {
                if(server.serverName == aGamename)
                {
                    DebugUtils.Log("Attempting to connect to " + aGamename);
                    m_JoiningServer = true;
                    m_ConnectionRequest = NetworkManager.RequestConnection(OnConnectedToGame,server,30.0f);
                }
            }

            
            
        }

        /// <summary>
        /// Gets called when the player has joined or not joined the game.
        /// </summary>
        /// <param name="aData"></param>
        private void OnConnectedToGame(RequestData aData)
        {
            m_JoiningServer = false;

            if (aData.data == null)
            {
                DebugUtils.LogError("Failed to join game, Missing request data");
                return;
            }

            //Get the status
            object[] data = (object[])aData.data;
            int status = (int)data[1];

            if (status == NetworkStatus.GOOD)
            {
                Next();
            }
            else if (status == NetworkStatus.FULL)
            {
                DebugUtils.Log("Failed to join game, the room was full");
            }
        }

        /// <summary>
        /// Pushes the buffer servers to the current servers.
        /// </summary>
        private void PushBuffer()
        {
            m_CurrentServers = m_BufferServers;
        }

        /// <summary>
        /// Makes a request to get the available servers.
        /// </summary>
        private void GetServers()
        {
            if(m_MadeRequest)
            {
                return;
            }
            m_MadeRequest = true;
            m_GetServersRequest = NetworkManager.RequestAvailableServers(OnReceiveAvailableServers, 15.0f);

        }

        /// <summary>
        /// Gets called when the request has been completed.
        /// </summary>
        /// <param name="aData"></param>
        private void OnReceiveAvailableServers(RequestData aData)
        {
            m_MadeRequest = false;
            if(aData.data == null)
            {
                return;
            }
            m_BufferServers = (NetworkServer[])aData.data;
            PushBuffer();
            RefreshList();
        }

        private void Update()
        {
            m_CurrentTime += Time.deltaTime;
            if(m_CurrentTime > m_RefreshRate)
            {
                m_CurrentTime = 0.0f;
                GetServers();
            }

            if(m_ConnectionRequest != null)
            {
                if(m_ConnectionRequest.status == RequestStatus.TimedOut)
                {
                    DebugUtils.LogWarning("Connection Request Timed Out");
                    m_ConnectionRequest = null;
                    m_JoiningServer = false;
                }
                else if(m_ConnectionRequest.status == RequestStatus.Invalid)
                {
                    m_ConnectionRequest = null;
                }
                
            }
            
            if(m_GetServersRequest != null)
            {
                if(m_GetServersRequest.status == RequestStatus.TimedOut)
                {
                    DebugUtils.LogWarning("GetServers Request Timed Out");
                    m_GetServersRequest = null;
                }
                else if (m_GetServersRequest.status == RequestStatus.Invalid)
                {
                    m_GetServersRequest = null;
                }
            }
        }

        /// <summary>
        /// Refreshes the MatchListView.
        /// Recreates the UIGameLobby objects.
        /// </summary>
        private void RefreshList()
        {
            if(m_MatchListView == null || m_CurrentServers == null)
            {
                return;
            }
            m_MatchListView.Clear();

            string filter = m_SearchFilter != null ? m_SearchFilter.text : string.Empty;

            foreach(NetworkServer server in m_CurrentServers)
            {
                if((filter != string.Empty && server.serverName.Contains(filter)) || filter == string.Empty)
                {
                    GameObject gameLobby = Instantiate(m_Prefab) as GameObject;
                    UIGameLobby info = gameLobby.GetComponent<UIGameLobby>();
                    if (info != null)
                    {
                        info.Setup(server.hostData);
                        info.button.onClick.AddListener(() => OnJoinGame(server.serverName));
                    }
                    m_MatchListView.AddContent(gameLobby);
                }
            }
        }
    }
}


