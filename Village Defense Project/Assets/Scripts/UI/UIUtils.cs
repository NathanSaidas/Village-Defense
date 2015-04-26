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
using UnityEngine.EventSystems;

#region CHANGE LOG
// -- April		11, 2015 - Nathan Hanlan - Adding in UIUtils File/Class
#endregion

namespace Gem
{
    /// <summary>
    /// A utility for UI methods.
    /// </summary>
	public class UIUtils : MonoBehaviour 
	{
        private const string UI_RESOURCE = "UI/";

        private const string UI_RESOURCE_SCROLL_AREA = "UIScrollArea";
        private const string UI_RESOURCE_CHAT_BOX = "UIChatbox";

        public const string DEFAULT_SCROLLAREA_NAME = "ScrollArea";


        /// <summary>
        /// Checks for an event system.
        /// </summary>
        /// <returns>Returns true if one already exists.</returns>
        public static bool HasEventSystem()
        {
            return EventSystem.current != null;
        }

        /// <summary>
        /// Creates an event system if one does not already exist.
        /// </summary>
        public static GameObject CreateEventSystem()
        {
            //If there is an event system ignore this.
            if(HasEventSystem())
            {
                return EventSystem.current.gameObject;
            }
            //Create gameobject and add the event system / Standlone Input Module
            GameObject gameObject = new GameObject(
                Constants.GAME_OBJECT_EVENT_SYSTEM, 
                typeof(EventSystem), 
                typeof(StandaloneInputModule));

            return gameObject;
        }

        /// <summary>
        /// Creates a UI canvas.
        /// </summary>
        /// <param name="aName"></param>
        /// <returns></returns>
        public static GameObject CreateUICanvas(string aName)
        {
            GameObject gameObject = new GameObject(
                aName, 
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(UICanvasEx));

            gameObject.layer = LayerMask.NameToLayer("UI");
            return gameObject;
        }

        public static GameObject CreateUIButton(Transform aParent,string aObjectName, string aButtonText, Vector2 aPosition, Vector2 aSize)
        {
            GameObject gameObject = new GameObject(
                aObjectName, 
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            //Set Button Rect Transform
            {
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.SetParent(aParent);
                rectTransform.anchorMin = Vector2.one * 0.5f;
                rectTransform.anchorMax = Vector2.one * 0.5f;
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.anchoredPosition = aPosition;
                rectTransform.sizeDelta = aSize;
            }

            GameObject textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));

            //Set Text Rect Transform
            {
                RectTransform rectTransform = textObject.GetComponent<RectTransform>();
                rectTransform.SetParent(gameObject.transform);
                rectTransform.anchorMin = Vector2.one * 0.5f;
                rectTransform.anchorMax = Vector2.one * 0.5f;
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(96.0f, 30.0f);
            }

            Text text = textObject.GetComponent<Text>();
            text.text = aButtonText;
            text.color = (Color.white * 0.025f) + new Color(0.0f, 0.0f, 0.0f, 1.0f);
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.alignment = TextAnchor.MiddleCenter;

            return gameObject;
        }
		
        public static GameObject CreateUIText(Transform aParent,string aObjectName, string aText, Vector2 aPosition, Vector2 aSize)
        {
            GameObject gameObject = new GameObject(
                aObjectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));

            //Set Text Rect Transform
            {
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.SetParent(aParent);
                rectTransform.anchorMin = Vector2.one * 0.5f;
                rectTransform.anchorMax = Vector2.one * 0.5f;
                rectTransform.pivot = Vector2.one * 0.5f;
                rectTransform.anchoredPosition = aPosition;
                rectTransform.sizeDelta = aSize;
            }

            Text text = gameObject.GetComponent<Text>();
            text.text = aText;
            text.color = (Color.white * 0.025f) + new Color(0.0f, 0.0f, 0.0f, 1.0f);
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
           

            return gameObject;
        }

        public static GameObject CreateScrollArea(Transform aParemt, Vector2 aPosition, Vector2 aSize)
        {
            return CreateScrollArea(aParemt, aPosition, aSize, 45.0f, false);
        }

        public static GameObject CreateScrollArea(Transform aParent, Vector2 aPosition, Vector2 aSize, float aScrollBarWidth, bool aScrollBarLeft)
        {
            GameObject gameObject = Resources.Load<GameObject>(UI_RESOURCE + UI_RESOURCE_SCROLL_AREA);
            if(gameObject == null)
            {
                return null;
            }
            gameObject = Instantiate(gameObject) as GameObject;
            
            if(gameObject != null)
            {
                UIScrollArea scrollArea = gameObject.GetComponent<UIScrollArea>();
                if(scrollArea != null)
                {
                    scrollArea.area = new Rect(aPosition.x, aPosition.y, aSize.x, aSize.y);
                    scrollArea.scrollBarWidth = aScrollBarWidth;
                    scrollArea.scrollBarLeft = aScrollBarLeft;
                }

                gameObject.transform.SetParent(aParent);
            }
            else
            {
                DebugUtils.MissingProperty<Resources>(UI_RESOURCE + UI_RESOURCE_SCROLL_AREA);
            }
            return gameObject;
        }

        public static GameObject CreateChatBox(Transform aParent, Vector2 aPosition)
        {
            GameObject gameObject = Resources.Load<GameObject>(UI_RESOURCE + UI_RESOURCE_CHAT_BOX);
            if(gameObject == null)
            {
                return null;
            }
            gameObject = Instantiate(gameObject) as GameObject;

            if(gameObject != null)
            {
                UIChatbox chatbox = gameObject.GetComponent<UIChatbox>();
                if(chatbox != null)
                {
                    chatbox.area = new Rect(aPosition.x, aPosition.y, chatbox.area.width, chatbox.area.height);
                }

                gameObject.transform.SetParent(aParent);
            }
            else
            {
                DebugUtils.MissingProperty<Resources>(UI_RESOURCE + UI_RESOURCE_SCROLL_AREA);
            }
            return gameObject;
        }
		
	}//End Class UIUtils
}//End Namespace Gem
