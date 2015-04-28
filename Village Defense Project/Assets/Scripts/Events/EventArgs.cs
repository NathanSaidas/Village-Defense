using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Gem.Tools;


namespace Gem
{
    namespace Events
    {
        /// <summary>
        /// This class is a ISaveableVersion which means it can be sent over the network. It's risky however and data is lost if there are properties that are nore Saveable.
        /// 
        /// 
        /// </summary>
        public class EventArgs : EventBase , ISaveableVersion
        {
            private IEventCaller m_Caller = null;
            private bool m_IsUnityType = false;
            private EventType m_InnerEventType = EventType.None;
            private OutterEventType m_OutterEventType = OutterEventType.None;
            private string m_GUID = Guid.NewGuid().ToString();
            private float m_Time = -1.0f;
            //Network Object ID
            private List<EventProperty> m_EventProperties = new List<EventProperty>();

            public EventArgs(IEventCaller aCaller, Type aType, EventType aInnerType, OutterEventType aOutterType, string aGuid, float aTime, EventProperty[] aProperties)
            {
                m_Caller = aCaller;
                m_IsUnityType = aType.IsSubclassOf(typeof(UnityEngine.Object));
                m_InnerEventType = aInnerType;
                m_OutterEventType = aOutterType;
                m_GUID = aGuid;
                m_Time = aTime;
                if(aProperties != null)
                {
                    m_EventProperties.AddRange(aProperties);
                }
            }

            public EventArgs(int aVersion, Stream aStream, IFormatter aFormatter)
            {
                OnLoad(aVersion, aStream, aFormatter);
            }

            public EventProperty GetProperty(string aPropertyName)
            {
                return m_EventProperties.FirstOrDefault<EventProperty>(Element => Element.name == aPropertyName);
            }

            public EventProperty[] GetSaveableProperties()
            {
                if(m_EventProperties.Count == 0)
                {
                    return null;
                }
                EventProperty[] buffer = new EventProperty[m_EventProperties.Count];

                int index = 0;

                for(int i = 0; i < buffer.Length; i++)
                {
                    if(m_EventProperties[i].isSaveable)
                    {
                        buffer[index] = m_EventProperties[i];
                        index++;
                    }
                }
                if(index == 0)
                {
                    return null;
                }

                EventProperty[] properties = new EventProperty[index];
                Array.Copy(buffer, properties, index);
                return properties;
            }

            public void OnSave(int aVersion, Stream aStream, IFormatter aFormatter)
            {
                //Save Normal Data
                aFormatter.Serialize(aStream, m_Caller.GetType().FullName);
                m_Caller.OnSave(aVersion, aStream, aFormatter);
                aFormatter.Serialize(aStream, m_IsUnityType);
                aFormatter.Serialize(aStream, m_InnerEventType);
                aFormatter.Serialize(aStream, m_OutterEventType);
                aFormatter.Serialize(aStream, m_GUID);
                aFormatter.Serialize(aStream, m_Time);

                //Save all the properties.
                EventProperty[] properties = GetSaveableProperties();

                if(properties == null)
                {
                    aFormatter.Serialize(aStream, 0);
                }
                else
                {
                    aFormatter.Serialize(aStream, properties.Length);
                    foreach(EventProperty property in properties)
                    {
                        aFormatter.Serialize(aStream, property.name);
                        aFormatter.Serialize(aStream, property.saveableData.GetType().FullName);
                        property.saveableData.OnSave(aVersion, aStream, aFormatter);
                    }
                }
            }

            public void OnLoad(int aVersion, Stream aStream, IFormatter aFormatter)
            {
                m_EventProperties.Clear();

                //Load Normal Data
                string typename = (string)aFormatter.Deserialize(aStream);
                Type type = Type.GetType(typename);
                m_Caller = (IEventCaller)Activator.CreateInstance(type);
                m_Caller.OnLoad(aVersion, aStream, aFormatter);
                m_IsUnityType = (bool)aFormatter.Deserialize(aStream);
                m_InnerEventType = (EventType)aFormatter.Deserialize(aStream);
                m_OutterEventType = (OutterEventType)aFormatter.Deserialize(aStream);
                m_GUID = (string)aFormatter.Deserialize(aStream);
                m_Time = (float)aFormatter.Deserialize(aStream);

                //Load Properties.
                int propertyCount = (int)aFormatter.Deserialize(aStream);

                for(int i = 0; i < propertyCount; i++)
                {
                    string propertyName = (string)aFormatter.Deserialize(aStream);
                    typename = (string)aFormatter.Deserialize(aStream);
                    type = Type.GetType(typename);
                    ISaveableVersion propertyData = (ISaveableVersion)Activator.CreateInstance(type);
                    EventProperty property = new EventProperty(propertyName, propertyData);
                    m_EventProperties.Add(property);
                }

                
            }


            public IEventCaller caller
            {
                get 
                {
                    if(m_IsUnityType)
                    {
                        UnityEngine.Object obj = (UnityEngine.Object)m_Caller;
                        if (obj == null)
                        {
                            return null;
                        }
                        else
                        {
                            return m_Caller;
                        }
                    }
                    else
                    {
                        return m_Caller;
                    }
                }
            }

            public bool isUnityType
            {
                get { return m_IsUnityType; }

            }
            public EventType innerEventType
            {
                get { return m_InnerEventType; }
            }
            public OutterEventType outterEventType
            {
                get { return m_OutterEventType; }
            }
            public string guid
            {
                get { return m_GUID; }
            }
            public float time
            {
                get { return m_Time; }
            }
            public EventProperty[] eventProperties
            {
                get { return m_EventProperties.ToArray(); }
            }


            
        }
    }
}


