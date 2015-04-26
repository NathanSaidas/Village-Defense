using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

            foreach(UIPlayerPanel player in m_Players)
            {
                if(player == null)
                {
                    DebugUtils.LogError("Missing player in UILobby");
                    continue;
                }
                player.kickButton.onClick.AddListener(() => OnKickPlayer(player));
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
        }


        public override void OnTransitionBegin()
        {
            //TODO(Nathan): Find out if were the host are not and call MakeVisible
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


        private void OnPlayerJoined(NetworkUser aUser)
        {
            bool slotOpen = false;
            foreach(UIPlayerPanel player in m_Players)
            {
                if(player.isOpen)
                {
                    player.SetPlayer(aUser);
                    slotOpen = true;
                }
            }

            if(slotOpen)
            {
                //Accept player: Send m_Player list to 
            }
        }
        private void OnPlayerLeave(NetworkUser aUser)
        {

        }

        private void OnUpdateConnectedPlayers(NetworkUser[] aConnectedPlayers)
        {
            //Received on client side to update the connected player list
        }

    }
}


