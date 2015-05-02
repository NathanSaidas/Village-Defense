using UnityEngine;
using System;
using System.Collections.Generic;

namespace Gem
{
    namespace Networking
    {
        /// <summary>
        /// A component used to establish network ID's on the network.
        /// </summary>
        [RequireComponent(typeof(NetworkView), typeof(NetworkView))]
        public class NetworkID : MonoBehaviour
        {
            /// <summary>
            /// ID of the network object
            /// </summary>
            private Guid m_GUID = Guid.Empty;
            /// <summary>
            /// The owner of the network object.
            /// </summary>
            private NetworkUser m_Owner = NetworkUser.BAD_USER;
            /// <summary>
            /// A local reference to the networkview component.
            /// </summary>
            private NetworkView m_NetworkView = null;
            /// <summary>
            /// A local reference to the prefab component.
            /// </summary>
            private Prefab m_Prefab = null;

            #region DEBUG VARIABLES
            [DebugLabel]
            [SerializeField]
            private string Debug_GUID = string.Empty;
            #endregion

            private void Start()
            {
                SetupReferences();
            }

            private void OnDestroy()
            {
                NetworkManager.UnregisterNetworkID(this);
            }

            /// <summary>
            /// This method will assign both the ID and the owner of the object.
            /// Call this after creating a object over the network.
            /// </summary>
            /// <param name="aGUID">The ID of the object</param>
            /// <param name="aOwner">The owner of the object.</param>
            public void AssignID(Guid aGUID, NetworkUser aOwner)
            {
                SetupReferences();

                m_GUID = aGUID;
                AssignOwner(aOwner);
                NetworkManager.RegisterNetworkID(this);

                if(Network.isServer)
                {
                    Packet packet = PacketFactory.CreateObjectCreatePacket(aGUID, m_Prefab.id, transform.position, transform.rotation, aOwner);
                    if (packet != null)
                    {
                        m_NetworkView.RPC(NetworkRPC.NETWORKID_ON_INITIALIZE_NETWORK_ID, RPCMode.OthersBuffered, packet.bytes);
                    }
                }
            }

            /// <summary>
            /// This method will change the owner of the object to the specified owner.
            /// </summary>
            /// <param name="aOwner"></param>
            public void AssignOwner(NetworkUser aOwner)
            {
                m_Owner = aOwner;
            }

            /// <summary>
            /// Sets up the references.
            /// </summary>
            private void SetupReferences()
            {
                if (m_NetworkView == null)
                {
                    m_NetworkView = GetComponent<NetworkView>();
                } 
                if(m_Prefab == null)
                {
                    m_Prefab = GetComponent<Prefab>();
                }
            }

            /// <summary>
            /// Gets called to initialize a network view on clients.
            /// </summary>
            /// <param name="aBytes"></param>
            [RPC]
            private void OnInitializeNetworkID(byte[] aBytes)
            {
                Guid guid = Guid.Empty;
                PrefabID prefabID = PrefabID.None;
                Vector3 position = Vector3.zero;
                Quaternion rotation = Quaternion.identity;
                NetworkUser user = NetworkUser.BAD_USER;

                if(PacketFactory.GetObjectCreatePacketData(new Packet(aBytes), out guid, out prefabID, out position, out rotation, out user))
                {
                    AssignID(guid, user);
                }
                else
                {
                    DebugUtils.LogError("Failed to initialize network ID\nGameObject: " + gameObject.name + "\nInstance ID: " + GetInstanceID());
                }

                
            }


            /// <summary>
            /// Access to the owner of the object.
            /// </summary>
            public NetworkUser owner
            {
                get { return m_Owner; }
            }

            /// <summary>
            /// Access to the ID of the object.
            /// </summary>
            public Guid guid
            {
                get { return m_GUID; }
            }

            
            public NetworkView view
            {
                get { return m_NetworkView; }
            }

            public Prefab prefab
            {
                get { return m_Prefab; }
            }
        }
    }
}

