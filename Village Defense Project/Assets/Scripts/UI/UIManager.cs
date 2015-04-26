using UnityEngine;
using System.Collections.Generic;

#region CHANGE LOG
// -- <MONTH>		<DAY>, 2015 - Nathan Hanlan - <COMMENT>
#endregion

namespace Gem
{
	public class UIManager : MonoBehaviour
    {
        #region SINGLETON
        private static UIManager s_Instance = null;
        private static UIManager instance
        {
            get { if (s_Instance == null) { CreateInstance(); } return s_Instance; }
        }
		
        private static void CreateInstance()
        {
            //Find the persistent game object 
            GameObject persistent = GameObject.Find(Constants.GAME_OBJECT_PERSISTENT);
            if (persistent != null)
            {
                s_Instance = persistent.GetComponent<UIManager>();
            }

            //Check the game's state. If there was an error load from the init scene. 
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
        private static bool SetInstance(UIManager aInstance)
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
        private static void DestroyInstance(UIManager aInstance)
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
        private Canvas m_MainCanvas = null;

        private List<UIWindow> m_RegisteredWindows = new List<UIWindow>();
        private UIWindow m_CurrentWindow = null;

        private void Awake()
        {
            if(!SetInstance(this))
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            DestroyInstance(this);
        }


        private static void RegisterWindow(UIWindow aWindow)
        {
            UIManager manager = instance;
            if(manager != null)
            {
                if(!manager.m_RegisteredWindows.Contains(aWindow))
                {
                    manager.m_RegisteredWindows.Add(aWindow);
                }
            }
        }
        private static void UnregisterWindow(UIWindow aWindow)
        {
            UIManager manager = instance;
            if(manager != null)
            {
                manager.m_RegisteredWindows.Remove(aWindow);
            }
        }

        private static void HideAllWindows()
        {
            UIManager manager = instance;
            if(manager != null)
            {

            }

        }

        private static UIWindow GetCurrentWindow()
        {
            if(instance != null)
            {
                return instance.m_CurrentWindow;
            }
            return null;
        }

        private static void MakeCurrentWindow(UIWindow aWindow)
        {
            if(instance != null)
            {
                instance.m_CurrentWindow = aWindow;
            }
        }

        public static Canvas mainCanvas
        {
            get { return instance != null ? instance.m_MainCanvas : null; }
        }



    }//End Class UIManager
}//End Namespace Gem
