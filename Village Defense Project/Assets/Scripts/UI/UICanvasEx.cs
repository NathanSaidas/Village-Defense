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
// -- April		11, 2015 - Nathan Hanlan - Added UICanvasEx class/file
#endregion

namespace Gem
{
    /// <summary>
    /// This component will constantly update the CanvasScalers resoltion sice.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
	public class UICanvasEx : MonoBehaviour 
	{
        private CanvasScaler m_CanvasScale = null;

        private void Start()
        {
            m_CanvasScale = GetComponent<CanvasScaler>();

            Canvas canvas = GetComponent<Canvas>();
            if(canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }
        private void Update()
        {
            m_CanvasScale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            m_CanvasScale.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            m_CanvasScale.referenceResolution = new Vector2(Screen.width, Screen.height);
            
        }
		
        public CanvasScaler canvasScaler
        {
            get { return m_CanvasScale; }
        }
		
	}//End Class UICanvas
}//End Namespace Gem
