using UnityEngine;
using System.Collections;

namespace Gem
{
    namespace Coroutines
    {
        public class Routine : CoroutineEx
        {
            public delegate void Execution();

            private Execution m_OnExecute = null;
            private Execution m_OnPostExecute = null;

            public Routine(CoroutineYield aYield, Execution aExecute, Execution aPostExecute) : base(aYield)
            {
                m_OnExecute = aExecute;
                m_OnPostExecute = aPostExecute;
            }

            public Routine(CoroutineYield aYield)
                : base(aYield)
            {
                m_OnExecute = null;
                m_OnPostExecute = null;
            }

            protected override void OnExecute()
            {
                if(m_OnExecute != null)
                {
                    m_OnExecute.Invoke();
                }
            }

            protected override void OnPostExecute()
            {
                if(m_OnPostExecute != null)
                {
                    m_OnPostExecute.Invoke();
                }
            }
            
            public Execution onExecute
            {
                get { return m_OnExecute; }
                set { m_OnExecute = value; }
            }
            public Execution onPostExecute
            {
                get { return m_OnPostExecute; }
                set { m_OnPostExecute = value; }
            }
        }
    }
}


