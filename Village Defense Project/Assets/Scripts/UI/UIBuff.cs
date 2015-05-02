using UnityEngine;
using System.Collections;

namespace Gem
{
    [RequireComponent(typeof(UIAbilityTooltip),typeof(RectTransform))]
    public class UIBuff : UIBase
    {
        private UIAbilityTooltip m_Tooltip = null;
        private string m_Title = string.Empty;
        private string m_Description = string.Empty;

        private RectTransform m_RectTransform = null;
        
        void Start()
        {
            m_Tooltip = GetComponent<UIAbilityTooltip>();
            m_RectTransform = GetComponent<RectTransform>();
        }

        public void Setup(string aTitle, string aDescription)
        {
            m_Title = aTitle;
            m_Description = aDescription;

            if(m_Tooltip != null)
            {
                m_Tooltip.title = aTitle;
                m_Tooltip.description = aDescription;
            }
        }

        public string title
        {
            get { return m_Title; }
        }

        public string description
        {
            get { return m_Description; }
        }

        public RectTransform rectTransform
        {
            get { return m_RectTransform; }
        }
    }
}


