using UnityEngine;
using System.Linq;
using System.Collections.Generic;


namespace Gem
{
    namespace Events
    {
        public class EventCounter
        {
            public class Pair
            {
                private OutterEventType m_OutterEventType = OutterEventType.None;
                private int m_Count = 0;

                public Pair()
                {

                }
                public Pair(OutterEventType aOutterEventType)
                {
                    m_OutterEventType = aOutterEventType;
                }

                public OutterEventType outterEventType
                {
                    get { return m_OutterEventType; }
                    set { m_OutterEventType = value; }
                }

                public int count
                {
                    get { return m_Count; }
                    set { m_Count = value; }
                }
            }

            public const int BAD_COUNT = -1;

            private List<Pair> m_EventTally = new List<Pair>();


            public void AddCount(OutterEventType aType)
            {
                Pair pair = m_EventTally.FirstOrDefault<Pair>(Element => Element.outterEventType == aType);
                if(pair != null)
                {
                    pair.count++;
                }
                else
                {
                    pair = new Pair(aType);
                    pair.count++;
                    m_EventTally.Add(pair);
                }
            }

            public void RemoveCount(OutterEventType aType)
            {
                Pair pair = m_EventTally.FirstOrDefault<Pair>(Element => Element.outterEventType == aType);
                if(pair != null)
                {
                    pair.count--;
                    if(pair.count <= 0)
                    {
                        m_EventTally.Remove(pair);
                    }
                }
            }

            public int GetCount(OutterEventType aType)
            {
                Pair pair = m_EventTally.FirstOrDefault<Pair>(Element => Element.outterEventType == aType);
                return pair != null ? pair.count : -1;
            }
        }
    }
}


