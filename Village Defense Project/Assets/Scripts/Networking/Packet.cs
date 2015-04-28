using UnityEngine;
using System.Collections;


namespace Gem
{
    namespace Networking
    {
        public class Packet
        {
            byte[] m_Bytes = null;

            public Packet()
            {
                bytes = null;
            }

            public Packet(byte[] aBytes)
            {
                bytes = aBytes;
            }

            public byte[] bytes
            {
                get { return m_Bytes; }
                set { m_Bytes = value; }
            }
        }
    }
}


