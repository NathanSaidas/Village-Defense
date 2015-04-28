using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Gem
{
    namespace Events
    {
        public class EventManager 
        {
            private static EventManager s_Instance = null;
            private static EventManager instance
            {
                get { return s_Instance; }
            }


            Dictionary<OutterEventType, List<IEventReceiver>> m_Receivers = new Dictionary<OutterEventType, List<IEventReceiver>>();
            EventArgs m_CurrentEvent = null;


            public EventManager()
            {
                if(s_Instance != null)
                {
                    DebugUtils.LogError("Multiple Event Managers created");
                    return;
                }
                s_Instance = this;
            }

            public void Destroy()
            {
                if(s_Instance == this)
                {
                    s_Instance = null;
                }
            }

            public static void RegisterReceiver(EventType aEventType, IEventReceiver aReceiver)
            {
                if (instance == null)
                {
                    return;
                }
                OutterEventType outterEventType = ConvertToOutterEventType(aEventType);
                instance.AddReceiver(outterEventType, aEventType, aReceiver);
            }

            public static void UnregisterReceiver(EventType aEventType, IEventReceiver aReceiver)
            {
                if(instance == null)
                {
                    return;
                }

                OutterEventType outterEventType = ConvertToOutterEventType(aEventType);
                instance.RemoveReceiver(outterEventType,aEventType,aReceiver);
            }

            public static void InvokeEvent(IEventCaller aCaller, EventType aEventType)
            {
                InvokeEvent(aCaller, aEventType, ConvertToOutterEventType(aEventType), null);
            }

            public static void InvokeEvent(IEventCaller aCaller, EventType aEventType, params EventProperty[] aProperties)
            {
                InvokeEvent(aCaller, aEventType, ConvertToOutterEventType(aEventType), aProperties);
            }

            public static void InvokeEvent(IEventCaller aCaller, EventType aEventType, OutterEventType aOutterEventType, params EventProperty[] aProperties)
            {
                EventArgs eventArgs = new EventArgs(aCaller, aCaller.GetType(), aEventType, aOutterEventType, Guid.NewGuid().ToString(), Time.time, aProperties);
                InvokeEvent(eventArgs);
            }

            public static void InvokeEvent(EventArgs aArgs)
            {
                if(instance == null)
                {
                    return;
                }
                instance.ProcessEventArgs(aArgs);
            }

            public static EventArgs GetCurrentEvent()
            {
                return instance != null ? instance.m_CurrentEvent : null;
            }

            public static OutterEventType ConvertToOutterEventType(EventType aEventType)
            {
                switch (aEventType)
                {
                    case EventType.NetworkingPlayerConnected:
                    case EventType.NetworkingPlayerDisconnected:
                    case EventType.NetworkingRefreshConnectionList:
                        return OutterEventType.Networking;
                    default:
                        return OutterEventType.Unknown;
                }
            }


            private void AddReceiver(OutterEventType aOutterEventType, EventType aEventType, IEventReceiver aReceiver)
            {
                List<IEventReceiver> receivers = null;
                ///Outter event exists.
                if(m_Receivers.ContainsKey(aOutterEventType) && m_Receivers.TryGetValue(aOutterEventType, out receivers))
                {
                    if(!receivers.Contains(aReceiver))
                    {
                        //Add Receiver.
                        receivers.Add(aReceiver);
                        aReceiver.OnRegistered(aOutterEventType,aEventType);
                    }
                }
                else//Doesnt exist.
                {
                    receivers = new List<IEventReceiver>();
                    //Add receiver.
                    receivers.Add(aReceiver);
                    m_Receivers.Add(aOutterEventType, receivers);
                    aReceiver.OnRegistered(aOutterEventType, aEventType);
                }
                
            }

            private void RemoveReceiver(OutterEventType aOutterEventType, EventType aEventType, IEventReceiver aReceiver)
            {
                if (aReceiver.GetRegisterCount(aOutterEventType) != 1)
                {
                    if(aReceiver.GetRegisterCount(aOutterEventType) > 1)
                    {
                        aReceiver.OnUnregistered(aOutterEventType,aEventType);
                    }
                    return;
                }

                List<IEventReceiver> receivers = null;
                if(m_Receivers.ContainsKey(aOutterEventType) && m_Receivers.TryGetValue(aOutterEventType,out receivers))
                {
                    receivers.Remove(aReceiver);
                    aReceiver.OnUnregistered(aOutterEventType, aEventType);
                }
            }

            private void ProcessEventArgs(EventArgs aEventArgs)
            {
                if(aEventArgs == null || aEventArgs.caller == null)
                {
                    return;
                }

                List<IEventReceiver> receivers = null;
                if(m_Receivers.ContainsKey(aEventArgs.outterEventType) && m_Receivers.TryGetValue(aEventArgs.outterEventType,out receivers))
                {
                    foreach(IEventReceiver receiver in receivers)
                    {
                        receiver.OnEventTriggered();
                    }
                }
            }



        }

    }
}

