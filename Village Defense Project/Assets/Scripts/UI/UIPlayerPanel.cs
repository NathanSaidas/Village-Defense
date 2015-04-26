#region CHANGE LOG
// -- April     22, 2015 - Nathan Hanlan - Added UIPlayerPanel class.
#endregion

using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    /// <summary>
    /// Manages a UIPlayerPanel GameObject.
    /// </summary>
    public class UIPlayerPanel : MonoBehaviour
    {
        NetworkUser m_CurrentPlayer = NetworkUser.BAD_USER;
        PlayerIndex m_PlayerIndex = 0;
        bool m_IsOpen = true;
        [SerializeField]
        private Text m_PlayerName = null;
        [SerializeField]
        private Button m_KickButton = null;

        public void SetPlayer(NetworkUser aPlayer)
        {
            m_IsOpen = false;
            m_CurrentPlayer = aPlayer;
            if(m_PlayerName != null)
            {
                m_PlayerName.text = aPlayer.username;
            }
        }

        public void ClearPlayer()
        {
            m_IsOpen = true;
            m_CurrentPlayer = NetworkUser.BAD_USER;
            if(m_PlayerName != null)
            {
                m_PlayerName.text = "<Open>";
            }
        }

        public NetworkUser currentPlayer
        {
            get { return m_CurrentPlayer; }
        }
        public bool isOpen
        {
            get { return m_IsOpen; }
        }

        public PlayerIndex playerIndex
        {
            get { return m_PlayerIndex; }
            set { m_PlayerIndex = value; }
        }

        public Text playerName
        {
            get { return m_PlayerName; }
            set { m_PlayerName = value; }
        }

        public Button kickButton
        {
            get { return m_KickButton; }
            set { m_KickButton = value; }
        }

        

        
    }

}

