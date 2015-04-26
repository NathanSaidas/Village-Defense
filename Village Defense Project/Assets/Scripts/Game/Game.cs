//Copyright (c) 2015 Nathan Hanlan
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.


#region CHANGE LOG
// -- April     11, 2015 - Nathan Hanlan - Added Game Class.
#endregion

using UnityEngine;
using System.Collections.Generic;
using Gem.Coroutines;

namespace Gem
{
    public class Game : MonoBehaviour
    {
        #region SINGLETON
        /// <summary>
        /// The singleton instance of game.
        /// </summary>
        private static Game s_Instance = null;
        /// <summary>
        /// Accessor to the game singleton. Use this when referring to the object as opposed to s_Instance.
        /// </summary>
        private static Game instance
        {
            get { if (s_Instance == null) { CheckInstance(); } return s_Instance; }
        }
        /// <summary>
        /// Checks for an instance of game on where it should be. If not found game is loaded from the start.
        /// </summary>
        private static void CheckInstance()
        {
            GameObject persistent = GameObject.Find(Constants.GAME_OBJECT_PERSISTENT);
            if (persistent != null)
            {
                s_Instance = persistent.GetComponent<Game>();
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
        private static bool SetInstance(Game aInstance)
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
        private static void DestroyInstance(Game aInstance)
        {
            if (s_Instance == aInstance)
            {
                s_Instance = null;
            }
        }

        /// <summary>
        /// Determines if the game exists or not.
        /// </summary>
        /// <returns></returns>
        public static bool Exists()
        {
            return s_Instance != null;
        }
        #endregion

        private static bool s_IsQuitting = false;
        public static bool isQuitting
        {
            get { return s_IsQuitting; }
        }

        private GameCache m_GameCache = new GameCache();
        [SerializeField]
        private string m_StartScene = string.Empty;

        

        /// <summary>
        /// Loads in from the init_scene
        /// </summary>
        public static void LoadFromStart()
        {
            LoadLevel(Constants.SCENE_INIT);
        }

        /// <summary>
        /// A wrapper for loading levels. This will be used later with the scene manager.
        /// </summary>
        /// <param name="aLevelName">The name of the level to load.</param>
        public static void LoadLevel(string aLevelName)
        {
            Application.LoadLevel(aLevelName);
        }

        public static void Quit()
        {
            Application.Quit();
        }

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

        private void OnApplicationQuit()
        {
            s_IsQuitting = true;
        }

        private void Start()
        {
            CoroutineEx.InitializeCoroutineExtensions(StartCoroutineEx, StopCoroutineEx);

            if(!string.IsNullOrEmpty(m_StartScene))
            {
                LoadLevel(m_StartScene);
            }
        }

        private void OnLevelWasLoaded(int aLevel)
        {
            //If were loading into the init scene. Clear the unity console from the 100's of errors.
            //Tell the user the game started from a invalid scene and there may be existing errors.
            //Tell the user of the reasons why the restart happened.
            if(Application.loadedLevelName == Constants.SCENE_INIT)
            {
                //Clear the
                DebugUtils.ClearUnityConsole();
                DebugUtils.LogError(ErrorCode.INVALID_START_GAME);
                DebugUtils.LogError("Reasons for invalid start game.\n" + GameLoader.loadError);
            }
        }


        private void StartCoroutineEx(IEnumerator<YieldInstruction> aRoutine)
        {
            StartCoroutine(aRoutine);
        }
        private void StopCoroutineEx(IEnumerator<YieldInstruction> aRoutine)
        {
            StopCoroutine(aRoutine);
        }

        public static GameCache cache
        {
            get { return instance != null ? instance.m_GameCache : null; }
        }
    }
}


