using UnityEngine.UI;
using UnityEngine;

namespace Gem
{

    [RequireComponent(typeof(RectTransform))]
    public class UIBar : UIBase
    {
        [SerializeField]
        private Image m_Foreground = null;
        [SerializeField]
        private Image m_Background = null;

        [SerializeField]
        private float m_Width = 100.0f;
        [SerializeField]
        private float m_Height = 15.0f;
        [SerializeField]
        [Range(0.0f,1.0f)]
        private float m_Percent = 1.0f;
        [SerializeField]
        private bool m_Vertical = false;

        private RectTransform m_RectTransform = null;
        private RectTransform m_ForegroundTransform = null;
        private RectTransform m_BackgroundTransform = null;

        private void OnEnable()
        {
            if(m_Foreground != null)
            {
                m_ForegroundTransform = m_Foreground.GetComponent<RectTransform>();
            }
            else
            {
                DebugUtils.MissingProperty<Image>("m_Foreground", gameObject);
                enabled = false;
                return;
            }
            if(m_Background != null)
            {
                m_BackgroundTransform = m_Background.GetComponent<RectTransform>();
            }
            else
            {
                DebugUtils.MissingProperty<Image>("m_Background", gameObject);
                enabled = false;
                return;
            }

            m_RectTransform = GetComponent<RectTransform>();
        }



        private void Update()
        {
            UpdateSize();
        }

        private void OnValidate()
        {
            if(m_Foreground != null && m_Background != null)
            {
                m_ForegroundTransform = m_Foreground.GetComponent<RectTransform>();
                m_BackgroundTransform = m_Background.GetComponent<RectTransform>();
            }

            m_RectTransform = GetComponent<RectTransform>();

            width = m_RectTransform.sizeDelta.x;
            height = m_RectTransform.sizeDelta.y;

            UpdateSize();
        }

        private void UpdateSize()
        {
            if(m_ForegroundTransform == null || m_BackgroundTransform == null)
            {
                return;
            }


            m_RectTransform.sizeDelta = new Vector2(width, height);
            m_BackgroundTransform.sizeDelta = new Vector2(width, height);
            m_BackgroundTransform.anchoredPosition = new Vector2(-width * 0.5f, -height * 0.5f);
            m_ForegroundTransform.anchoredPosition = new Vector2(-width * 0.5f, -height * 0.5f);
            if(m_Vertical)
            {
                m_ForegroundTransform.sizeDelta = new Vector2(width, height * Mathf.Clamp01(percent));
            }
            else
            {
                m_ForegroundTransform.sizeDelta = new Vector2(width * Mathf.Clamp01(percent), height);
            }
            
        }

        public Image foreground
        {
            get { return m_Foreground; }
        }
        public Image background
        {
            get { return m_Background; }
        }

        public float width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        public float height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public float percent
        {
            get { return m_Percent; }
            set { m_Percent = value; }
        }
        public bool vertical
        {
            get { return m_Vertical; }
            set { m_Vertical = value; }
        }

        
       
    }
}

