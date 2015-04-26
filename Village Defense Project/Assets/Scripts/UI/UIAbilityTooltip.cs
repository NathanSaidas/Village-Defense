using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    public class UIAbilityTooltip : MonoBehaviour
    {
        [SerializeField]
        [TextArea]
        private string m_Description = string.Empty;
        [SerializeField]
        private Text m_Text = null;
        

        

        private void OnValidate()
        {
            description = m_Description;
        }

        public string description
        {
            get { return m_Description; }
            set { m_Description = value; if (m_Text != null) { m_Text.text = m_Description; } }
        }
    }

}

