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

#region CHANGE LOG
// -- April		12, 2015 - Nathan Hanlan - Added UIWindow
#endregion

namespace Gem
{
    /// <summary>
    /// Base class for UI Windows.
    /// </summary>
	public class UIWindow : UIBase 
	{
        public const int CLOSE_FORCED = -1;

        /// <summary>
        /// Delegate for window closing events.
        /// </summary>
        /// <param name="aCloseStatus">A status for why the window closed.</param>
        public delegate void WindowCloseCallback(int aCloseStatus);

        /// <summary>
        /// A callback handler to the close function. Use this if you want to get a callback when the window closes.
        /// </summary>
        private WindowCloseCallback m_CloseCallback;

        public virtual void CloseWindow(int aCloseStatus)
        {
            if (m_CloseCallback != null)
            {
                m_CloseCallback.Invoke(aCloseStatus);
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual bool isShowing
        {
            get { return gameObject.activeSelf; }
        }

        public WindowCloseCallback onClose
        {
            get { return m_CloseCallback; }
            set { m_CloseCallback = value; }
        }
		
	}//End Class UIWindow
}//End Namespace Gem
