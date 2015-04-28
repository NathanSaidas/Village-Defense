using System;
using System.IO;
using System.Runtime.Serialization;

using UnityEngine;
using Gem.Tools;

namespace Gem
{
    namespace Events
    {
        [Serializable]
        public class EventProperty : EventBase
        {
            /// <summary>
            /// The name of the property.
            /// </summary>
            [SerializeField]
            private string m_Name = string.Empty;

            /// <summary>
            /// Raw data of the property.
            /// </summary>
            private object m_Data = null;
            /// <summary>
            /// Saveable interface of the property.
            /// </summary>
            [SerializeField]
            private ISaveableVersion m_SaveableData = null;

            public EventProperty()
            {
                m_Name = string.Empty;
                m_Data = null;
                m_SaveableData = null;
            }

            public EventProperty(string aName, ISaveableVersion aData)
            {
                m_Name = aName;
                m_Data = aData;
                m_SaveableData = aData;
            }

            public EventProperty(string aName, object aData)
            {
                m_Name = aName;
                m_Data = aData;
                m_SaveableData = null;
            }
            
            public string name
            {
                get { return m_Name; }
            }

            public object data
            {
                get { return m_Data; }
            }

            public bool isSaveable
            {
                get { return m_SaveableData != null; }
            }

            public ISaveableVersion saveableData
            {
                get { return m_SaveableData; }
            }
            
        }
    }
}


