
using System.IO;
using System.Runtime.Serialization;

namespace Gem
{
    namespace Tools
    {
        public interface ISaveable
        {
            void OnSave(Stream aStream, IFormatter aFormatter);
            void OnLoad(Stream aStream, IFormatter aFormatter);
        }

        public interface ISaveableVersion
        {
            void OnSave(int aVersion, Stream aStream, IFormatter aFormatter);
            void OnLoad(int aVersion, Stream aStream, IFormatter aFormatter);
        }
    }
}

