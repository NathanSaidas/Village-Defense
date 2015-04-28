#region CHANGE LOG
// -- April     22, 2015 - Nathan Hanlan - Added PlayerIndex struct.
#endregion

using System;

namespace Gem
{
    /// <summary>
    /// Typedef version of an integer value
    /// </summary>
    [Serializable]
    public struct PlayerIndex
    {
        public static readonly PlayerIndex One = new PlayerIndex(0);
        public static readonly PlayerIndex Two = new PlayerIndex(1);
        public static readonly PlayerIndex Three = new PlayerIndex(2);
        public static readonly PlayerIndex Four = new PlayerIndex(3);
        public static readonly PlayerIndex Five = new PlayerIndex(4);
        public static readonly PlayerIndex Six = new PlayerIndex(5);

        private int m_Value;
        public PlayerIndex(int aValue)
        {
            m_Value = aValue;
        }

        public static implicit operator int(PlayerIndex aValue)
        {
            return aValue.m_Value;
        }
        public static implicit operator PlayerIndex(int aValue)
        {
            return new PlayerIndex(aValue);
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }
    }
}


