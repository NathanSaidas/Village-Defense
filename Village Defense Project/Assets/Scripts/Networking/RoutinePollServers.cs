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
using System;
using Gem.Coroutines;

namespace Gem
{
    /// <summary>
    /// Makes a request to the Master server for hostdata.
    /// 
    /// Make use of onCoroutineFinish callback to get the host data.
    /// </summary>
    [Serializable]
    public class RoutinePollServers : CoroutineEx
    {

        /// <summary>
        /// The game type to search for.
        /// </summary>
        private string m_GameTypename = string.Empty;
        /// <summary>
        /// The ID of the request.
        /// </summary>
        private UID m_RequestID = UID.BAD_ID;
        /// <summary>
        /// Creates the routine. Uses YieldWaitForSeconds by default.
        /// </summary>
        /// <param name="aWaitTime">The amount of time to wait before getting the host data.</param>
        /// <param name="aGameTypename">The type of game to search for.</param>
        public RoutinePollServers(float aWaitTime, string aGameTypename, UID aRequestID) : base(new YieldWaitForSeconds(aWaitTime))
        {
            m_GameTypename = aGameTypename;
            m_RequestID = aRequestID;
        }

        /// <summary>
        /// Gets called before yield
        /// </summary>
        protected override void OnExecute()
        {
            MasterServer.ClearHostList();
            MasterServer.RequestHostList(m_GameTypename);
        }

        /// <summary>
        /// Gets called after yield.
        /// </summary>
        protected override void OnPostExecute()
        {
            
        }

        public string gameTypename
        {
            get { return m_GameTypename; }
        }
        public UID requestID
        {
            get { return m_RequestID; }
        }
    }
}

