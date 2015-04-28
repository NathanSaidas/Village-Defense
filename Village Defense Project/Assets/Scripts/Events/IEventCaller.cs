using System.IO;
using System.Runtime.Serialization;

using Gem.Networking;

namespace Gem
{
    namespace Events
    {
        public interface IEventCaller
        {
            NetworkUser GetCallingPlayer();
            bool IsObject();
            bool IsDecorative();
            bool IsUnit();
            void OnSave(int aVersion, Stream aStream, IFormatter aFormatter);
            void OnLoad(int aVersion, Stream aStream, IFormatter aFormatter);
        }
    }
}


