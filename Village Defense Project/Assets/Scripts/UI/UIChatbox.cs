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
// -- April		13, 2015 - Nathan Hanlan - <COMMENT>
#endregion

namespace Gem
{
	public class UIChatbox : UIScrollArea 
	{
        public delegate void EnterMessageCallback(string aMessage);

        [SerializeField]
        private InputField m_InputField = null;
        [SerializeField]
        private Button m_SendButton = null;

        private IChatInterceptor m_Interceptor = null;

        private EnterMessageCallback m_OnEnterMessage = null;

        

        protected override void Start()
        {
            

            if(m_InputField != null)
            {
                if (m_SendButton != null)
                {
                    m_SendButton.onClick.AddListener(() => SubmitMessage(m_InputField.text));
                }
                else
                {
                    DebugUtils.MissingProperty<Button>("m_SendButton", gameObject);
                }
                m_InputField.onEndEdit.AddListener((text) => SubmitMessage(text));
            }
            else
            {
                DebugUtils.MissingProperty<InputField>("m_InputField", gameObject);
            }
            base.Start();
        }

        protected override void LateUpdate()
        { 
            base.LateUpdate();
        }
		

        private void SubmitMessage(string aText)
        {
            if(m_InputField != null)
            {
                if(interceptor != null)
                {
                    interceptor.OnSubmitMessage(aText, this);
                }
                else
                {
                    AddMessage(aText);
                }
                m_InputField.text = string.Empty;
            }
        }

        public void AddMessage(string aMessage)
        {
            GameObject message = UIUtils.CreateUIText(null, "Message[" + Time.time + "]", aMessage, Vector2.zero, new Vector2(area.width * 0.90f, area.height));
            if(message == null)
            {
                return;
            }
            ContentSizeFitter sizeFitter = message.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.SetLayoutVertical();
            if(AddContent(message) && onEnterMessage != null)
            {
                onEnterMessage.Invoke(aMessage);
            }
        }

        public IChatInterceptor interceptor
        {
            get { return m_Interceptor; }
            set { m_Interceptor = value; }
        }

        public EnterMessageCallback onEnterMessage
        {
            get { return m_OnEnterMessage; }
            set { m_OnEnterMessage = value; }
        }
		
	}//End Class UIChatbox
}//End Namespace Gem
