using UnityEngine;
using UnityEngine.UI;

namespace Gem
{
    public class UICreateGameWindow : UIWindow
    {
        public delegate void CreateGameCallback(string aGamename);

        [SerializeField]
        private InputField m_GameNameInputField = null;
        [SerializeField]
        private Button m_CreateButton = null;
        [SerializeField]
        private Button m_CancelButton = null;

        private CreateGameCallback m_CreateGameCallback = null;

        // Use this for initialization
        void Start()
        {
            if(m_GameNameInputField != null)
            {
                m_GameNameInputField.onEndEdit.AddListener((text) => OnSubmitText(text));
            }
            else
            {
                DebugUtils.MissingProperty<InputField>("m_GameNameInputField", gameObject);
            }
            if(m_CreateButton != null)
            {
                m_CreateButton.onClick.AddListener(() => OnSubmit());
            }
            else
            {
                DebugUtils.MissingProperty<Button>("m_CreateButton", gameObject);
            }
            if(m_CancelButton != null)
            {
                m_CancelButton.onClick.AddListener(() => OnCancel());
            }
            else
            {
                DebugUtils.MissingProperty<Button>("m_CancelButton", gameObject);
            }
        }

        void OnSubmitText(string aText)
        {
            OnSubmit();
        }

        void OnSubmit()
        {
            if(m_GameNameInputField == null)
            {
                DebugUtils.MissingProperty<InputField>("m_GameNameInputField", gameObject);
                return;
            }

            if(string.IsNullOrEmpty(m_GameNameInputField.text))
            {
                return;
            }

            if(m_CreateGameCallback != null)
            {
                m_CreateGameCallback.Invoke(m_GameNameInputField.text);
            }
        }

        void OnCancel()
        {
            if(m_GameNameInputField != null)
            {
                m_GameNameInputField.text = string.Empty;
            }
            Hide();
        }

        public Button createButton
        {
            get { return m_CreateButton; }
            set { m_CreateButton = value; }
        }

        public Button cancelButton
        {
            get { return m_CancelButton; }
            set { m_CancelButton = value; }
        }

        public InputField gameNameInputField
        {
            get { return m_GameNameInputField; }
            set { m_GameNameInputField = value; }
        }

        public CreateGameCallback createGameCallback
        {
            get { return m_CreateGameCallback; }
            set { m_CreateGameCallback = value; }
        }
    }

}

