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


using System;
using System.Collections.Generic;
// -- Unity
using UnityEngine;

#region CHANGE LOG
/*  February 12 2015 - Nathan Hanlan - Added CoroutineEx file / class
 * 
 */
#endregion

namespace Gem
{
    namespace Coroutines
    {
        public class YieldWaitForSecondsLoop : CoroutineYield
        {
            private float m_Seconds = 0.0f;
            private bool m_IsDone = false;
            private int m_LoopCount = 0;
            private int m_CurrentLoopCount = 0;

            public YieldWaitForSecondsLoop(float aSeconds, int aLoopCount)
            {
                m_Seconds = aSeconds;
                m_LoopCount = aLoopCount;
            }

            public override YieldInstruction Yield()
            {
                return new WaitForSeconds(m_Seconds);
            }

            public override void PostYield()
            {
                m_CurrentLoopCount++;
                if(m_CurrentLoopCount >= m_LoopCount)
                {
                    m_IsDone = true;
                }
            }

            public override bool IsDone()
            {
                return m_IsDone;
            }
        }
    }
}