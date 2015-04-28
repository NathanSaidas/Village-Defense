using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    public class UIMenu : UIBase
    {
        [SerializeField]
        private bool m_IsStartMenu = false;
        [SerializeField]
        private UIMenu m_PreviousMenu = null;
        [SerializeField]
        private UIMenu m_NextMenu = null;

        protected void BaseInitialization()
        {
            if(!m_IsStartMenu)
            {
                gameObject.SetActive(false);
            }
        }

        protected void SetupButton(Button aButton, string aName)
        {
            if (aButton != null)
            {
                aButton.onClick.AddListener(() => OnButtonClick(aButton));
            }
            else
            {
                DebugUtils.MissingProperty<Button>(aName, gameObject);
            }
        }

        protected virtual void OnButtonClick(Button aButton)
        {

        }

        public virtual void OnTransitionBegin()
        {

        }

        public virtual void OnTransitionComplete()
        {

        }

        public void Next()
        {
            UIMenuSystem menuSystem = UIMenuSystem.current;
            if(menuSystem != null && menuSystem.currentMenu == this && m_NextMenu != null)
            {
                menuSystem.TransitionNext(m_NextMenu);
            }
        }

        public void Previous()
        {
            UIMenuSystem menuSystem = UIMenuSystem.current;
            if (menuSystem != null && menuSystem.currentMenu == this && m_PreviousMenu != null)
            {
                menuSystem.TransitionPrevious(m_PreviousMenu);
            }
        }

    }
}


