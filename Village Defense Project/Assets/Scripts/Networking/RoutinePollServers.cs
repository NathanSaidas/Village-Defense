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

