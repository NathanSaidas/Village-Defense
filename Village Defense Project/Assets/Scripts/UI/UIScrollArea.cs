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
using System.Linq;
using System.Collections.Generic;

#region CHANGE LOG
// -- April		12, 2015 - Nathan Hanlan - Added file UIScrollArea
#endregion

namespace Gem
{
    /// <summary>
    /// This is a scroll area. 
    /// 
    /// Useful methods.
    /// -- AddContent
    /// -- Remove Content
    /// 
    /// </summary>
	public class UIScrollArea : UIBase 
	{
        /// <summary>
        /// The size and position of the UIScrollArea
        /// </summary>
        [SerializeField]
        private Rect m_Area = new Rect(0.0f, 0.0f, 450.0f, 180.0f);
        /// <summary>
        /// The size of the scroll bar.
        /// </summary>
        [SerializeField]
        private float m_ScrollBarWidth = 45.0f;
        /// <summary>
        /// Whether or not the scroll bar is on the left or right.
        /// </summary>
        [SerializeField]
        private bool m_ScrollBarOnLeft = false;
        /// <summary>
        /// The distance between each content.
        /// </summary>
        [SerializeField]
        private float m_ContentMargin = 1.0f;
        /// <summary>
        /// Reference to the main panel. 
        /// </summary>
        [SerializeField]
        private RectTransform m_MainPanel = null;
        /// <summary>
        /// Reference to the scroll bar.
        /// </summary>
        [SerializeField]
        private RectTransform m_ScrollBar = null;
        /// <summary>
        /// Reference to the mask.
        /// </summary>
        [SerializeField]
        private RectTransform m_Mask = null;
        /// <summary>
        /// Reference to the content panel. This constantly gets resized.
        /// </summary>
        [SerializeField]
        private RectTransform m_ContentPanel = null;


        /// <summary>
        /// The list of content within the scroll area. The content must have a RectTransform.
        /// </summary>
        [SerializeField]
        private List<GameObject> m_Content = new List<GameObject>();
        /// <summary>
        /// Whether or not to update the panel
        /// </summary>
        private bool m_IsDirty = false;

        /// <summary>
        /// Updates the size immediately at start.
        /// </summary>
        protected virtual void Start()
        {
            UpdateSize();
        }

        protected virtual void LateUpdate()
        {
            if (m_IsDirty)
            {
                UpdateSize();
                m_IsDirty = false;
            }
        }

       

#if UNITY_EDITOR
        /// <summary>
        /// Gets called on editor change. Update the size on field change.
        /// </summary>
        private void OnValidate()
        {
            if (m_ContentPanel != null
                && m_Mask != null
                && m_ScrollBar != null
                && m_MainPanel != null)
            {
                RectTransform rectTransform = GetComponent<RectTransform>();
                m_Area.x = rectTransform.anchoredPosition.x;
                m_Area.y = rectTransform.anchoredPosition.y;
                m_Area.width = rectTransform.sizeDelta.x;
                m_Area.height = rectTransform.sizeDelta.y;

                UpdateSize();
            }
        }
#endif
        /// <summary>
        /// Updates the size of the content.
        /// </summary>
        protected void UpdateSize()
        {
            if (m_MainPanel != null)
            {
                m_MainPanel.anchoredPosition = new Vector2(m_Area.x, m_Area.y);
                m_MainPanel.sizeDelta = new Vector2(m_Area.width, m_Area.height);
            }
            else
            {
                DebugUtils.MissingProperty<RectTransform>("m_MainPanel", gameObject);
            }

            if (m_ScrollBar != null)
            {
                m_ScrollBar.anchoredPosition = m_ScrollBarOnLeft ? new Vector2(-m_Area.width * 0.5f, 0.0f) : new Vector2(m_Area.width * 0.5f, 0.0f);
                m_ScrollBar.sizeDelta = new Vector2(m_ScrollBarWidth, m_Area.height);
            }
            else
            {
                DebugUtils.MissingProperty<RectTransform>("m_ScrollBar", gameObject);
            }

            if (m_Mask != null)
            {
                m_Mask.anchoredPosition = Vector2.zero;
                m_Mask.sizeDelta = new Vector2(m_Area.width, m_Area.height);
            }
            else
            {
                DebugUtils.MissingProperty<RectTransform>("m_Mask", gameObject);
            }

            UpdateContentPanel();
        }
	
        /// <summary>
        /// Updates the content panel appropriately.
        /// </summary>
        protected void UpdateContentPanel()
        {
            if(m_ContentPanel == null)
            {
                DebugUtils.MissingProperty<RectTransform>("m_ContentPanel", gameObject);
                return;
            }

            float currentPos = 0.0f;
            //Update all of the content elements positions.
            foreach(GameObject element in m_Content)
            {
                RectTransform rectTransform = element.GetComponent<RectTransform>();
                if(rectTransform == null)
                {
                    DebugUtils.LogError("Missing a rect transform on the element " + element.name);
                    continue;
                }

                rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
                rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
                rectTransform.pivot = new Vector2(0.5f, 1.0f);
                rectTransform.anchoredPosition = new Vector2(0.0f, currentPos);

                currentPos -= (rectTransform.sizeDelta.y + m_ContentMargin);
            }

            //Adjust the size of content panel.
            if(m_ContentPanel != null)
            {
                float height = Mathf.Max(m_Area.height, -currentPos);
                m_ContentPanel.sizeDelta = new Vector2(m_ContentPanel.sizeDelta.x, height);
            }

        }

        /// <summary>
        /// Adds a content object in the list.
        /// Content must have a rect transform.
        /// </summary>
        /// <param name="aContent">The content to add.</param>
        /// <returns>Returns true if content was successfully added.</returns>
        public bool AddContent(GameObject aContent)
        {
            if(!m_Content.Contains(aContent) && aContent.GetComponent<RectTransform>() != null)
            {
                m_Content.Add(aContent);
                if(m_ContentPanel != null)
                {
                    aContent.transform.SetParent(m_ContentPanel);
                }
                UpdateContentPanel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the content object from the list.
        /// </summary>
        /// <param name="aContent">The content to remove.</param>
        /// <returns>Returns true if the content was removed.</returns>
        public bool RemoveContent(GameObject aContent)
        {
            bool status = m_Content.Remove(aContent);
            UpdateContentPanel();
            return status;
        }

        /// <summary>
        /// Removes all of the content from the list.
        /// </summary>
        public void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Removes all of the content from the list.
        /// </summary>
        /// <param name="aDestroy">If true the content will be destroyed.</param>
        public void Clear(bool aDestroy)
        {
            if(aDestroy == true)
            {
                foreach(GameObject gObject in m_Content)
                {
                    Destroy(gObject);
                }
            }

            m_Content.Clear();
            UpdateContentPanel();
        }

        /// <summary>
        /// Gets the first content item by name.
        /// </summary>
        /// <param name="aName">The name of the content item to search for.</param>
        /// <returns></returns>
        public GameObject GetContent(string aName)
        {
            return m_Content.FirstOrDefault<GameObject>(Element => Element.name == aName);
        }

        /// <summary>
        /// Gets the first content item by instance ID.
        /// </summary>
        /// <param name="aInstanceID"></param>
        /// <returns></returns>
        public GameObject GetContent(int aInstanceID)
        {
            return m_Content.FirstOrDefault<GameObject>(Element => Element.GetInstanceID() == aInstanceID);
        }

        /// <summary>
        /// Marks the scroll area as dirty and updates the size on the next frame.
        /// </summary>
        public void SetDirty()
        {
            m_IsDirty = true;
        }

        /// <summary>
        /// The size and position of the UIScrollArea
        /// </summary>
        public Rect area
        {
            get { return m_Area; }
            set { m_Area = value; SetDirty(); }
        }

        /// <summary>
        /// The size of the scroll bar.
        /// </summary>
        public float scrollBarWidth
        {
            get { return m_ScrollBarWidth; }
            set { m_ScrollBarWidth = value; SetDirty(); }
        }

        /// <summary>
        /// Whether or not the scroll bar is on the left or right.
        /// </summary>
        public bool scrollBarLeft
        {
            get { return m_ScrollBarOnLeft; }
            set { m_ScrollBarOnLeft = value; SetDirty(); }
        }

        /// <summary>
        /// The distance between each content.
        /// </summary>
        public float contentMargin
        {
            get { return m_ContentMargin; }
            set { m_ContentMargin = value; }
        }
        

	}//End Class UIScrollArea
}//End Namespace Gem
