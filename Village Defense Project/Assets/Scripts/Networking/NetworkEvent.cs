#region CHANGE LOG
// -- April     28, 2015 - Nathan Hanlan - Added NetworkEvent enum and Callback delegate.
#endregion

using Gem.Events;

namespace Gem
{
    namespace Networking
    {
        public delegate void NetworkEventCallback(params EventProperty[] aEventProperties);
        public enum NetworkEvent
        {
            OnPlayerConnected,
            OnPlayerDisconnected,
            OnRefreshConnections,
            OnPlayerKicked,
            OnWasKicked,

            OnObjectCreated,
            OnObjectDestroyed,
        }
    }
}
