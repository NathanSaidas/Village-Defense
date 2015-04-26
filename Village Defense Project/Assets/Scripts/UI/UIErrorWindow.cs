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
using UnityEngine.UI;
using System.Collections.Generic;

#region CHANGE LOG
// -- April		11, 2015 - Nathan Hanlan - Adding File/UIErrorWindow
#endregion

namespace Gem
{
    /// <summary>
    /// A class for showing error windows.
    /// </summary>
	public class UIErrorWindow : UIWindow 
	{
        /// <summary>
        /// A stack of error windows. The window on the top of the stack is always displayed.
        /// </summary>
        private static Stack<UIErrorWindow> s_ErrorWindows = new Stack<UIErrorWindow>();
        /// <summary>
        /// The size of the buttons
        /// </summary>
        private static readonly Vector2 BUTTON_SIZE = new Vector2(95.0f, 30.0f);

        #region CONSTANTS
        /// <summary>
        /// Represents an error window with only an OK button.
        /// </summary>
        public const int TYPE_OK = 1;
        /// <summary>
        /// Represents an error window with an YES and NO button
        /// </summary>
        public const int TYPE_YES_NO = 2;
        /// <summary>
        /// Represents a close state of OK.
        /// </summary>
        public const int CLOSE_OK = 1;
        /// <summary>
        /// Represents a close state of YES.
        /// </summary>
        public const int CLOSE_YES = 1;
        /// <summary>
        /// Represents a close state of NO.
        /// </summary>
        public const int CLOSE_NO = 0;
        #endregion
        
        /// <summary>
        /// Whether or not to destroy the canvas upon close.
        /// </summary>
        private bool m_DestroyCanvas = false;
        /// <summary>
        /// The type of window. OK or YES_NO
        /// </summary>
        private int m_WindowType = 0;
        

        /// <summary>
        /// Shows a window with the title and description. Uses OK window by default
        /// </summary>
        /// <param name="aTitle">The title of the error.</param>
        /// <param name="aDescription">A description of the error message.</param>
        /// <returns>Returns a gameobject reference of the error window.</returns>
        public static GameObject Create(string aTitle, string aDescription)
        {
            return Create(aTitle, aDescription, TYPE_OK);
        }
        
        /// <summary>
        /// Shows a window with the title and description.
        /// </summary>
        /// <param name="aTitle">he title of the error.</param>
        /// <param name="aDescription">A description of the error message.</param>
        /// <param name="aType">The type of window this should be. See TYPE prefixed constants.</param>
        /// <returns>Returns a gameobject reference of the error window.</returns>
        public static GameObject Create(string aTitle, string aDescription, int aType)
        {
            if(!UIUtils.HasEventSystem())
            {
                DebugUtils.LogError(ErrorCode.MISSING_EVENT_SYSTEM);
                UIUtils.CreateEventSystem();
            }

            if(!(aType == TYPE_OK || aType == TYPE_YES_NO))
            {
                DebugUtils.LogError(ErrorCode.INVALID_ERROR_WINDOW_TYPE);
                Debug.Log("Type: " + aType);
                return null;
            }
            bool destroyCanvas = false;
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if(canvas == null)
            {
                canvas = UIUtils.CreateUICanvas("UIErrorWindow Canvas" + aTitle).GetComponent<Canvas>();
                destroyCanvas = true;
            }

            GameObject window = new GameObject(
                "UIErrorWindow (" + aTitle +")", 
                typeof(RectTransform), 
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(UIErrorWindow));
            //Set Window Rect Transform
            {
                RectTransform rectTransform = window.GetComponent<RectTransform>();
                rectTransform.SetParent(canvas.transform);
                rectTransform.anchorMin = Vector2.one * 0.5f;
                rectTransform.anchorMax = Vector2.one * 0.5f;
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(265.0f, 148.0f);
            }

            UIErrorWindow errorWindow = window.GetComponent<UIErrorWindow>();
            errorWindow.m_DestroyCanvas = destroyCanvas;
            errorWindow.m_WindowType = aType;

             UIUtils.CreateUIText(window.transform,"Title", aTitle, new Vector2(0.0f, 57.5f), new Vector2(90.0f, 17.0f));
            UIUtils.CreateUIText(window.transform, "Description", aDescription, Vector2.zero, new Vector2(215.0f, 57.0f));
            if(aType == TYPE_OK)
            {
                GameObject ok = UIUtils.CreateUIButton(window.transform, "Ok", "Ok", new Vector2(0.0f, -50.0f), BUTTON_SIZE);
                Button okButton = ok.GetComponent<Button>();
                if(okButton != null)
                {
                    okButton.onClick.AddListener(() => errorWindow.OnCloseOk());
                }
            }
            else if(aType == TYPE_YES_NO)
            {
                GameObject yes = UIUtils.CreateUIButton(window.transform, "Yes", "Yes", new Vector2(-60.0f, -50.0f), BUTTON_SIZE);
                Button yesButton = yes.GetComponent<Button>();
                if (yesButton != null)
                {
                    yesButton.onClick.AddListener(() => errorWindow.OnCloseYes());
                }
                GameObject no = UIUtils.CreateUIButton(window.transform, "No", "No", new Vector2(60.0f, -50.0f), BUTTON_SIZE);
                Button noButton = no.GetComponent<Button>();
                if (noButton != null)
                {
                    noButton.onClick.AddListener(() => errorWindow.OnCloseNo());
                }
            }

            //Hide current window
            UIErrorWindow currentWindow = current;
            if (currentWindow != null)
            {
                currentWindow.Hide();
            }
            //Add new window
            if (!s_ErrorWindows.Contains(errorWindow))
            {
                s_ErrorWindows.Push(errorWindow);
            }
            

            return window;

        }

        /// <summary>
        /// Closes the next error window (The current one being shown).
        /// </summary>
        public static void CloseNext()
        {
            UIErrorWindow window = s_ErrorWindows.Pop();
            if(window != null)
            {
                window.CloseWindow(UIWindow.CLOSE_FORCED);
            }
        }

        /// <summary>
        /// Closes all of the error windows.
        /// </summary>
        public static void CloseAll()
        {
            UIErrorWindow window = s_ErrorWindows.Pop();
            while(window != null)
            {
                window.CloseWindow(UIWindow.CLOSE_FORCED);
                window = s_ErrorWindows.Pop();
            }
        }

        /// <summary>
        /// Gets called for when the OK button is clicked for TYPE_OK windows
        /// </summary>
        private void OnCloseOk()
        {
            CloseWindow(CLOSE_OK);
        }

        /// <summary>
        /// Gets called for when the YES button is clicked for TYPE_YES_NO windows
        /// </summary>
        private void OnCloseYes()
        {
            CloseWindow(CLOSE_YES);
        }

        /// <summary>
        /// Gets called for when the NO button is clicked for TYPE_YES_NO windows
        /// </summary>
        private void OnCloseNo()
        {
            CloseWindow(CLOSE_NO);
        }

        /// <summary>
        /// Override the CloseWindow function to implement its functionality.
        /// </summary>
        /// <param name="aStatus"></param>
        public override void CloseWindow(int aStatus)
        {
            //Close only if this is the current window.
            if(current == this)
            {
                s_ErrorWindows.Pop();
            }
            else
            {
                DebugUtils.LogError(ErrorCode.INVALID_ERROR_WINDOW_CLOSE_OPERATION);
                return;
            }

            //Do base class closing operations.
            base.CloseWindow(aStatus);
            //Destroy the canvas if we own it.
            if (m_DestroyCanvas)
            {
                if (transform.parent != null)
                {
                    Destroy(transform.parent.gameObject);
                }
            }
            else //Else just destroy self
            {
                Destroy(gameObject);
            }

            //Show next window.
            UIErrorWindow errorWindow = current;
            if (errorWindow != null)
            {
                errorWindow.Show();
            }
        }
        
        /// <summary>
        /// The current active UIErrorWindow
        /// </summary>
        public static UIErrorWindow current
        {
            get { return s_ErrorWindows.Count > 0 ? s_ErrorWindows.Peek() : null; }
        }

        public int windowType
        {
            get { return m_WindowType; }
            set { m_WindowType = value; }
        }
		
	}//End Class UIErrorWindow
}//End Namespace Gem
