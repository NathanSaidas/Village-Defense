using UnityEngine;
using System.Collections;

namespace Gem
{
    public class Prefab : MonoBehaviour
    {
        [SerializeField]
        private PrefabID m_ID = PrefabID.None;

        public PrefabID id
        {
            get { return m_ID; }
        }
    }

}

