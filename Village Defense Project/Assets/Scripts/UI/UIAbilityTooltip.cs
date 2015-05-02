using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    public class UIAbilityTooltip : MonoBehaviour
    {
        [SerializeField]
        private string m_Title = string.Empty;

        [SerializeField]
        [TextArea]
        private string m_Description = string.Empty;

        [SerializeField]
        private Text m_TitleText = null;
        [SerializeField]
        private Text m_DescriptionText = null;
        

        

        private void OnValidate()
        {
            title = m_Title;
            description = m_Description;
        }

        public string title
        {
            get { return m_Title; }
            set { m_Title = value; if (m_TitleText != null) { m_TitleText.text = m_Title; } }
        }

        public string description
        {
            get { return m_Description; }
            set { m_Description = value; if (m_DescriptionText != null) { m_DescriptionText.text = m_Description; } }
        }
    }

}

