using UnityEngine;
using System.Text;
using System.Collections;

namespace Gem
{
    public class Tester : MonoBehaviour
    {

        Request m_CurrentRequest = null;
        
        public string m_Username = string.Empty;
        public string m_Password = string.Empty;
        // Use this for initialization
        void Start()
        {
            StartTest();
        }
        
        // Update is called once per frame
        void Update()
        {
        
	    }
        
        void StartTest()
        {
            Debug.Log("Querying Authentciation Servers");
            m_CurrentRequest = NetworkManager.RequestAuthenticationServers(OnReceiveAuthenticationServers);
        }
        
        void OnReceiveAuthenticationServers(RequestData aData)
        {
            Debug.Log("Receiving Authentciation Servers");
        
            CheckData(aData);
        
            NetworkServer[] servers = (NetworkServer[])aData.data;
            
            if(servers != null && servers.Length > 0)
            {
                //Connect to the first authentication server.
                Debug.Log("Request Authentciation Server Connection");
                m_CurrentRequest = NetworkManager.RequestConnection(OnConnectToAuthenticationServer, servers[0]);
            }
            else
            {
                Debug.Log("No servers available");
            }
        }
        
        void OnConnectToAuthenticationServer(RequestData aData)
        {
            CheckData(aData);
        
            object[] data = (object[])aData.data;
            if(data != null && data.Length == 2)
            {
                int status = (int)data[1];
                if(status == NetworkStatus.GOOD)
                {
                    Debug.Log("Connected to Authentication Server, Requesting Authentication");
                    m_CurrentRequest = NetworkManager.RequestAuthentication(OnAuthenticateAccount, m_Username, m_Password);
                }
                else if(status == NetworkStatus.FULL)
                {
                    Debug.Log("Failed to connect to Authentication Server: Reason FULL");
                    m_CurrentRequest = null;
                }
                else if(status == NetworkStatus.ERROR)
                {
                    Debug.Log("Failed to connect to Authentication Server: Reason ERROR");
                    m_CurrentRequest = null;
                }
            }
        }
        
        void OnAuthenticateAccount(RequestData aData)
        {
            Debug.Log("Authentication Complete");
            CheckData(aData);
        
            int status = (int)aData.data;
            if (status == NetworkStatus.GOOD)
            {
                Debug.Log("Authentication Successful");
                m_CurrentRequest = null;
            }
            else if (status == NetworkStatus.INVALID_USERNAME)
            {
                Debug.Log("Authentication Failed: Invalid Username");
                m_CurrentRequest = null;
            }
            else if (status == NetworkStatus.INVALID_PASSWORD)
            {
                Debug.Log("Authentication Failed: Invalid Password");
                m_CurrentRequest = null;
            }
        }
        
        void CheckData(RequestData aData)
        {
            if (aData.request != m_CurrentRequest)
            {
                Debug.LogError("Bad request match");
            }
        }
    }
}


