namespace Gem
{
    namespace Events
    {
        public interface IEventReceiver
        {

            int GetRegisterCount(OutterEventType aEventType);
            void OnRegistered(OutterEventType aOutterEventType, EventType aEventType);
            void OnUnregistered(OutterEventType aOutterEventType, EventType aEventType);

            void OnEventTriggered();
        }
    }
}


