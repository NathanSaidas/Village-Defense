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

#region CHANGE LOG
// -- April		12, 2015 - Nathan Hanlan - Adding in class/file
#endregion

namespace Gem
{
	public class UILoginWindow : UIWindow 
	{
        public delegate void LoginCallback(string aUsername, string aPassword);

        [SerializeField]
        private Toggle m_OfflineToggle = null;
        [SerializeField]
        private InputField m_UsernameInputField = null;
        [SerializeField]
        private InputField m_PasswordInputField = null;
        [SerializeField]
        private Button m_PlayButton = null;
        [SerializeField]
        private Button m_CancelButton = null;

        public LoginCallback m_LoginCallback = null;

        private void Start()
        {
            if(m_OfflineToggle != null)
            {
                m_OfflineToggle.onValueChanged.AddListener((value) => ToggleOnlineMode(value));
                ToggleOnlineMode(m_OfflineToggle.isOn);
            }
            else
            {
                DebugUtils.MissingProperty<Toggle>("m_OfflineToggle", gameObject);
            }
            if(m_UsernameInputField != null)
            {
                m_UsernameInputField.onEndEdit.AddListener((text) => InputFieldSubmit(text));
            }
            else
            {
                DebugUtils.MissingProperty<InputField>("m_UsernameInputField", gameObject);
            }
            if(m_PasswordInputField != null)
            {
                m_PasswordInputField.onEndEdit.AddListener((text) => InputFieldSubmit(text));
            }
            else
            {
                DebugUtils.MissingProperty<InputField>("m_PasswordInputField", gameObject);
            }
            if(m_PlayButton != null)
            {
                m_PlayButton.onClick.AddListener(() => SubmitLogin());
            }
            else
            {
                DebugUtils.MissingProperty<Button>("m_PlayButton", gameObject);
            }

            if(m_CancelButton != null)
            {
                m_CancelButton.onClick.AddListener(() => CancelLogin());
            }
            else
            {
                DebugUtils.MissingProperty<Button>("m_CancelButton", gameObject);
            }
            

        }

        private void InputFieldSubmit(string aText)
        {
            SubmitLogin();
        }

        private void SubmitLogin()
        {
            if(m_UsernameInputField == null)
            {
                DebugUtils.MissingProperty<InputField>("m_UsernameInputField", gameObject);
                return;
            }
            if(m_PasswordInputField == null)
            {
                DebugUtils.MissingProperty<InputField>("m_PasswordInputField", gameObject);
                return;
            }

            if(string.IsNullOrEmpty(m_UsernameInputField.text) 
                || string.IsNullOrEmpty(m_PasswordInputField.text))
            {
                return;
            }

            if(m_LoginCallback != null)
            {
                m_LoginCallback.Invoke(m_UsernameInputField.text, m_PasswordInputField.text);
            }
        }

        private void CancelLogin()
        {
            if(m_UsernameInputField != null)
            {
                m_UsernameInputField.text = string.Empty;
            }
            if(m_PasswordInputField != null)
            {
                m_PasswordInputField.text = string.Empty;
            }
                 
            Hide();
        }


        private void ToggleOnlineMode(bool aToggle)
        {
            if (m_UsernameInputField == null)
            {
                DebugUtils.MissingProperty<InputField>("m_UsernameInputField", gameObject);
                return;
            }
            if (m_PasswordInputField == null)
            {
                DebugUtils.MissingProperty<InputField>("m_PasswordInputField", gameObject);
                return;
            }
            m_UsernameInputField.interactable = !aToggle;
            m_PasswordInputField.interactable = !aToggle;

            if(aToggle == false)
            {
                m_UsernameInputField.text = string.Empty;
                m_PasswordInputField.text = string.Empty;
            }
        }
		

        public LoginCallback loginCallback
        {
            get { return m_LoginCallback; }
            set { m_LoginCallback = value; }
        }
	}//End Class UILoginForm
}//End Namespace Gem
