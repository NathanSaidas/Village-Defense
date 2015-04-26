using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;

#region CHANGE LOG
// -- April    15, 2015 - Nathan Hanlan - Added class.
// -- April    16, 2015 - Nathan Hanlan - Added in serialize / deserialize methods
#endregion

namespace Gem
{
    [Serializable]
    public class Request
    {
        public static readonly Request EMPTY = new Request(null,new NetworkUser("@@Dummy@@"), RequestType.Authentication);

        [SerializeField]
        private UID m_ID = new UID();
        private float m_RegisterTime = 0.0f;
        private float m_Timeout = 0.0f;
        private RequestCallback m_Callback = null;
        private NetworkUser m_User = default(NetworkUser);
        private RequestStatus m_Status = RequestStatus.Invalid;
        [SerializeField]
        private RequestType m_RequestType = RequestType.ConnectionList;
        
        
        public Request(RequestCallback aCallback, NetworkUser aUser, RequestType aType)
        {
            m_Callback = aCallback;
            m_User = aUser;
            m_ID = UID.New();
            m_Status = RequestStatus.Pending;
            m_RegisterTime = Time.time;
            m_Timeout = -1.0f;
            m_RequestType = aType;
        }

        public Request(RequestCallback aCallback, NetworkUser aUser, RequestType aType, float aTimeout)
        {
            m_Callback = aCallback;
            m_User = aUser;
            m_ID = UID.New();
            m_Status = RequestStatus.Pending;
            m_RegisterTime = Time.time;
            m_Timeout = aTimeout;
            m_RequestType = aType;
        }


        public void Serialize(Stream aStream, IFormatter aFormatter)
        {
            aFormatter.Serialize(aStream, m_ID.id);
            aFormatter.Serialize(aStream, m_RegisterTime);
            aFormatter.Serialize(aStream, m_Timeout);
            m_User.Serialize(aStream, aFormatter);
            aFormatter.Serialize(aStream, m_Status);
            aFormatter.Serialize(aStream, m_RequestType);
        }

        public void Deserialize(Stream aStream, IFormatter aFormatter)
        {
            m_ID = (ushort)aFormatter.Deserialize(aStream);
            m_RegisterTime = (float)aFormatter.Deserialize(aStream);
            m_Timeout = (float)aFormatter.Deserialize(aStream);
            m_User.Deserialize(aStream, aFormatter);
            m_Status = (RequestStatus)aFormatter.Deserialize(aStream);
            m_RequestType = (RequestType)aFormatter.Deserialize(aStream);
        }


        /// <summary>
        /// Invokes the callback.
        /// </summary>
        /// <param name="aData"></param>
        public void Callback(RequestData aData)
        {
            CheckStatus();

            if(m_Callback != null && m_Status == RequestStatus.Complete)
            {
                m_Callback.Invoke(aData);
            }
        }

        private void CheckStatus()
        {
            if (Time.time - m_RegisterTime > m_Timeout && m_Timeout > 0.1f && m_Status != RequestStatus.Invalid)
            {
                m_Status = RequestStatus.TimedOut;
            }
        }

        public UID id
        {
            get { return m_ID; }
        }
        public float registerTime
        {
            get { return m_RegisterTime; }
        }
        public float timeout
        {
            get { return m_Timeout; }
        }
        public NetworkUser user
        {
            get { return m_User; }
        }
        public RequestStatus status
        {
            get { CheckStatus(); return m_Status; }
        }
        public RequestType type
        {
            get { return m_RequestType; }
        }
    }

}

