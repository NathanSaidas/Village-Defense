using UnityEngine;
using System.Collections;

namespace Gem
{
    namespace Networking
    {
        [RequireComponent(typeof(UIChatbox))]
        public class NetworkChat : MonoBehaviour , IChatInterceptor
        {


            private UIChatbox m_Chatbox = null;
            [SerializeField]
            private string m_Identifer = string.Empty;
            [SerializeField]
            private bool m_PrefixWithName = true;
            private bool m_HasQuit = false;

            private void Start()
            {
                m_Chatbox = GetComponent<UIChatbox>();
                m_Chatbox.interceptor = this;
                NetworkManager.RegisterChat(this);
            }

            private void OnApplicationQuit()
            {
                m_HasQuit = true;
                NetworkManager.UnregisterChat(this);
            }

            private void OnDestroy()
            {
                if(!m_HasQuit)
                {
                    NetworkManager.UnregisterChat(this);
                }
            }

            private void OnDisconnectedFromServer(NetworkDisconnection aInfo)
            {
                m_Chatbox.Clear();
            }

            public void OnSubmitMessage(string aMessage, UIChatbox aChatbox)
            {
                NetworkUser user = NetworkManager.GetCurrentUser();
                string message = string.Empty;
                if(prefixWithName)
                {
                    message = "[" + user.username + "]:" + aMessage;
                }
                else
                {
                    message = aMessage;
                }
                NetworkManager.SendChatMessage(identifier, message);
            }

            public void OnSubmitMessage(string aMessage)
            {
                m_Chatbox.AddMessage(aMessage);
            }

            public string identifier
            {
                get { return m_Identifer; }
                set { m_Identifer = value; }
            }

            public bool prefixWithName
            {
                get { return m_PrefixWithName; }
                set { m_PrefixWithName = value; }
            }
        }

    }
}

