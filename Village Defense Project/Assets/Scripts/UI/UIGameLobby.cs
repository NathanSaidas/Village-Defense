#region CHANGE LOG
// -- April     22, 2015 - Nathan Hanlan - Added UIGameLobby
#endregion

using UnityEngine;
using UnityEngine.UI;

using Gem.Networking;

namespace Gem
{
    /// <summary>
    /// Manages two text components for game lobby infos.
    /// </summary>
    public class UIGameLobby : UIBase
    {
        [SerializeField]
        private Button m_Button = null;
        [SerializeField]
        private Text m_GameName = null;
        [SerializeField]
        private Text m_Connections = null;
        
        public void Setup(HostData aData)
        {
            if(aData != null)
            {
                SetGameName(aData.gameName);
                SetConnectionName(aData.connectedPlayers);
            }
        }

        public void SetGameName(string aGameName)
        {
            if(m_GameName != null)
            {
                m_GameName.text = "Name: " + aGameName;
            }
        }

        public void SetConnectionName(int aConnectedPlayers)
        {
            if(m_Connections != null)
            {
                m_Connections.text = "Connections: " + aConnectedPlayers.ToString() + "/" + Constants.SERVER_MAX_USERS.ToString();
            }
        }

        public Text gameName
        {
            get { return m_GameName; }
            set { m_GameName = value; }
        }

        public Text connections
        {
            get { return m_Connections; }
            set { m_Connections = value; }
        }
        public Button button
        {
            get { return m_Button; }
            set { m_Button = value; }
        }
	
    }
}


