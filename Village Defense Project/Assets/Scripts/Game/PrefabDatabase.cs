using UnityEngine;
using System.Collections.Generic;

using Gem.Networking;

namespace Gem
{
    public class PrefabDatabase : MonoBehaviour
    {

        #region SINGLETON
        private static PrefabDatabase s_Instance = null;
        private static PrefabDatabase instance
        {
            get { if (s_Instance == null) { CreateInstance(); } return s_Instance; }
        }

        private static void CreateInstance()
        {
            GameObject persistent = GameObject.Find(Constants.GAME_OBJECT_PERSISTENT);
            if (persistent != null)
            {
                s_Instance = persistent.GetComponent<PrefabDatabase>();
            }

            GameLoader.CheckGameState();
            if (!string.IsNullOrEmpty(GameLoader.loadError))
            {
                Game.LoadFromStart();
            }
        }

        /// <summary>
        /// Claim ownership of the singleton instance.
        /// </summary>
        /// <param name="aInstance">The instance attempting to claim ownership.</param>
        /// <returns>Returns false if the instance was already owned.</returns>
        private static bool SetInstance(PrefabDatabase aInstance)
        {
            if (s_Instance != null && s_Instance != aInstance)
            {
                return false;
            }
            s_Instance = aInstance;
            return true;
        }
        /// <summary>
        /// Remove ownership from singleton instance.
        /// </summary>
        /// <param name="aInstance">The instance trying to unclaim ownership.</param>
        private static void DestroyInstance(PrefabDatabase aInstance)
        {
            if (s_Instance == aInstance)
            {
                s_Instance = null;
            }
        }

        public static bool Exists()
        {
            return s_Instance != null;
        }
        #endregion


        [SerializeField]
        private List<Prefab> m_Prefabs = new List<Prefab>();

        [SerializeField]
        private List<PrefabID> m_ErrorIDs = new List<PrefabID>();
        [SerializeField]
        private List<GameObject> m_ErrorGameObjects = new List<GameObject>();

        private Dictionary<PrefabID, Prefab> m_Database = new Dictionary<PrefabID, Prefab>();

        private void Awake()
        {
            if(!SetInstance(this))
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }

        private void OnDestroy()
        {
            DestroyInstance(this);
        }

        public void InitializeDatabase()
        {
            m_Database.Clear();
            m_ErrorIDs.Clear();

            bool error = false;
            bool invalidObject = false;

            foreach(Prefab prefab in m_Prefabs)
            {
                //Null check
                if(prefab == null)
                {
                    error = true;
                    invalidObject = true;
                    continue;
                }

                //Check for ID
                if(prefab.GetComponent<NetworkID>() == null)
                {
                    error = true;
                    m_ErrorGameObjects.Add(prefab.gameObject);
                    continue;
                }

                if(!m_Database.ContainsKey(prefab.id))
                {
                    m_Database.Add(prefab.id, prefab);
                }
                else
                {
                    error = true;
                    m_ErrorIDs.Add(prefab.id);
                }
            }

            if(error)
            {
                string errorString = "Initialized the database with errors.";

                if (m_ErrorIDs.Count != 0)
                {
                    errorString += "\nRepeaked Keys: " + m_ErrorIDs.Count;
                }

                if (m_ErrorGameObjects.Count != 0)
                {
                    errorString += "\nInvalid GameObjects: " + m_ErrorGameObjects.Count;
                }

                if (invalidObject)
                {
                    errorString += "\nNull Object: " + errorString;
                }

                DebugUtils.LogError(errorString);
            }

            
        }

        public static GameObject GetPrefab(PrefabID aPrefabID)
        {
            if(instance == null)
            {
                return null;
            }

            Prefab prefab = null;

            if(instance.m_Database.TryGetValue(aPrefabID,out prefab))
            {
                if(prefab != null)
                {
                    return prefab.gameObject;
                }
            }
            return null;
        }

        
    }
}




