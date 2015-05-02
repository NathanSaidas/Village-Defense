using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    [RequireComponent(typeof(RectTransform))]
    public class UITooltip : UIBase
    {

        [SerializeField]
        [TextArea]
        private string m_Description = string.Empty;
        [SerializeField]
        private Text m_Text = null;
        [SerializeField]
        private bool m_ShowAlways = false;

        private bool m_HasMouseFocus = false;
        private RectTransform m_TextTransform = null;
        private RectTransform m_RectTransform = null;

        private void OnEnable()
        {
            if(m_Text == null)
            {
                DebugUtils.MissingProperty<Text>("m_Text", gameObject);
                enabled = false;
                return;
            }
            
        }

        private void Update()
        {
            if(m_TextTransform != null)
            {
                m_TextTransform.localScale = Vector3.one * 0.5f;
                m_TextTransform.sizeDelta = m_RectTransform.sizeDelta * 2.0f;
            }

            if(!m_ShowAlways)
            {
                if(m_Text.gameObject.activeSelf != m_HasMouseFocus)
                {
                    m_Text.gameObject.SetActive(m_HasMouseFocus);
                }
            }
        }

        private void OnValidate()
        {
            if(m_Text == null)
            {
                return;
            }
            m_TextTransform = m_Text.GetComponent<RectTransform>();
            m_RectTransform = GetComponent<RectTransform>();

            if(m_TextTransform != null)
            {
                m_TextTransform.localScale = Vector3.one * 0.5f;
                m_TextTransform.sizeDelta = m_RectTransform.sizeDelta * 2.0f;
            }
            description = m_Description;
        }

        public void OnMouseEvent(bool aEnter)
        {
            m_HasMouseFocus = aEnter;
        }

        public string description
        {
            get { return m_Description; }
            set { m_Description = value; if (m_Text != null) { m_Text.text = m_Description; } }
        }
    }
}


