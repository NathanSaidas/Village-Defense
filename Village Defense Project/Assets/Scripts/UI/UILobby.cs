using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

using Gem.Networking;
using Gem.Events;

namespace Gem
{
    public class UILobby : UIMenu 
    {
        [SerializeField]
        private UIChatbox m_Chatbox = null;
        [SerializeField]
        private Image m_MapGraphic = null;
        [SerializeField]
        private Button m_StartButton = null;
        [SerializeField]
        private Button m_BackButton = null;

        [SerializeField]
        private UIPlayerPanel[] m_Players = null;

        private bool m_HasQuit = false;
        
        private void Start()
        {
            if(m_Chatbox == null)
            {
                DebugUtils.MissingProperty<UIChatbox>("m_Chatbox", gameObject);
            }
            if(m_MapGraphic == null)
            {
                DebugUtils.MissingProperty<Image>("m_MapGraphic", gameObject);
            }

            SetupButton(m_StartButton, "m_StartButton");
            SetupButton(m_BackButton, "m_BackButton");

            foreach (UIPlayerPanel player in m_Players)
            {
                if (player == null)
                {
                    DebugUtils.LogError("Missing player in UILobby");
                    continue;
                }
                player.kickButton.onClick.AddListener(() => OnKickPlayer(player));
            }

            NetworkManager.RegisterNetworkCallback(NetworkEvent.OnPlayerConnected, OnPlayerJoined);
            NetworkManager.RegisterNetworkCallback(NetworkEvent.OnPlayerDisconnected, OnPlayerLeave);
            NetworkManager.RegisterNetworkCallback(NetworkEvent.OnRefreshConnections, OnUpdateConnectedPlayers);

            BaseInitialization();
        }

        private void OnApplicationQuit()
        {
            m_HasQuit = true;
            NetworkManager.UnregisterNetworkCallback(NetworkEvent.OnPlayerConnected, OnPlayerJoined);
            NetworkManager.UnregisterNetworkCallback(NetworkEvent.OnPlayerDisconnected, OnPlayerLeave);
            NetworkManager.UnregisterNetworkCallback(NetworkEvent.OnRefreshConnections, OnUpdateConnectedPlayers);
        }

        private void OnDestroy()
        {
            if(!m_HasQuit)
            {
                NetworkManager.UnregisterNetworkCallback(NetworkEvent.OnPlayerConnected, OnPlayerJoined);
                NetworkManager.UnregisterNetworkCallback(NetworkEvent.OnPlayerDisconnected, OnPlayerLeave);
                NetworkManager.UnregisterNetworkCallback(NetworkEvent.OnRefreshConnections, OnUpdateConnectedPlayers);
            }
        }

        private void MakeVisible(bool aIsHost)
        {
            
            if(m_StartButton != null)
            {
                m_StartButton.gameObject.SetActive(aIsHost);
            }

            int index = 1;
            foreach(UIPlayerPanel player in m_Players)
            {
                if(player != null)
                {
                    player.ClearPlayer();
                    player.kickButton.gameObject.SetActive(aIsHost);
                    if (aIsHost)
                    {
                        player.playerIndex = index;
                    }
                    index++;
                }
            }

            if (aIsHost)
            {
                OnUpdateConnectedPlayers(null);
            } 
        }


        public override void OnTransitionBegin()
        {
            //TODO(Nathan): Find out if were the host are not and call MakeVisible
            MakeVisible(NetworkManager.IsServerHost());
        }

        protected override void OnButtonClick(Button aButton)
        {
            if(aButton == m_BackButton)
            {
                //If were the host destroy server
                //If were client send a message were leaving then go back to the previous menu.
                NetworkManager.LeaveLobby();
                Previous();
            }
        }

        private void OnKickPlayer(UIPlayerPanel aPlayer)
        {
            //can only occur on host side.
        }


        private void OnPlayerJoined(params EventProperty[] aProperties)
        {
            EventProperty connectingUserProperty = aProperties.FirstOrDefault<EventProperty>(Element => Element.name == Constants.NETWORK_EVENT_PROPERTY_CONNECTING_USERS);
            if (connectingUserProperty != null)
            {
                NetworkUser user = (NetworkUser)connectingUserProperty.data;
                foreach (UIPlayerPanel player in m_Players)
                {
                    if (player.isOpen)
                    {
                        DebugUtils.Log("Setting Slot: " + player.playerIndex + " to player " + user.username);
                        player.SetPlayer(user);
                        break;
                    }
                }
            }
            else
            {
                DebugUtils.LogError("User joined but missing property");
            }
        }
        private void OnPlayerLeave(params EventProperty[] aProperties)
        {
            EventProperty disconnectingUserProperty = aProperties.FirstOrDefault<EventProperty>(Element => Element.name == Constants.NETWORK_EVENT_PROPERTY_DISCONNECTING_USERS);
            if (disconnectingUserProperty != null)
            {
                NetworkUser user = (NetworkUser)disconnectingUserProperty.data;
                foreach(UIPlayerPanel player in m_Players)
                {
                    if(player.currentPlayer == user)
                    {
                        DebugUtils.Log("Clearing Slot: " + player.playerIndex + "(" + player.playerName.GetInstanceID() + ")");
                        player.ClearPlayer();
                        player.playerName.text = "<Open>";
                        break;
                    }
                }
            }
            else
            {
                DebugUtils.LogError("User left but missing property");
            }
        }

        private void OnUpdateConnectedPlayers(params EventProperty[] aProperties)
        {
            //Updating player connections.
            NetworkUser[] newUsers = NetworkManager.GetConnectedUsers();
            
            for(int i = 0; i < m_Players.Length; i++)
            {
                if(i >= newUsers.Length)
                {
                    m_Players[i].ClearPlayer();
                }
                else
                {
                    m_Players[i].ClearPlayer();
                    m_Players[i].SetPlayer(newUsers[i]);
                }
            }

        }

        
    }
}


