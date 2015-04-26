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

using UnityEngine;
using System.Text;

#region CHANGE LOG
// -- April		11, 2015 - Nathan Hanlan - Adding in game loader class.
#endregion

namespace Gem
{
    /// <summary>
    /// Check the state of the game and its singletons with the CheckGameState method.
    /// If singletons are not initialized the game is loadeed from the init_scene and an error string is stored.
    /// </summary>
	public class GameLoader : MonoBehaviour 
	{
        /// <summary>
        /// The errors for why the game started from the init scene.
        /// If this string is empty it means there is no errors.
        /// </summary>
        private static string s_LoadError = string.Empty;

        /// <summary>
        /// Checks the state of the game and leaves a string with a bunch of errors.
        /// </summary>
        public static void CheckGameState()
        {
            StringBuilder loadErrorString = new StringBuilder();

            if(!DebugUtils.Exists())
            {
                loadErrorString.Append("Failed to load DebugUtils.");
            }
            if(!UIManager.Exists())
            {
                if(string.IsNullOrEmpty(loadErrorString.ToString()))
                {
                    loadErrorString.Append("\n");
                }
                loadErrorString.Append("Failed to load UIManager.");
            }
            if(!NetworkManager.Exists())
            {
                if (string.IsNullOrEmpty(loadErrorString.ToString()))
                {
                    loadErrorString.Append("\n");
                }
                loadErrorString.Append("Failed to load NetworkManager.");
            }
            if(!Game.Exists())
            {
                if(string.IsNullOrEmpty(loadErrorString.ToString()))
                {
                    loadErrorString.Append("\n");
                }
                loadErrorString.Append("Failed to load Game");
            }

            s_LoadError = loadErrorString.ToString();
        }

        /// <summary>
        /// The errors for why the game started from the init scene.
        /// If this string is empty it means there is no errors.
        /// </summary>
        public static string loadError
        {
            get { return s_LoadError; }
            set { s_LoadError = value; }
        }
		
		
	}//End Class GameLoader
}//End Namespace Gem