using UnityEngine;
using System.Collections.Generic;

namespace Gem
{
    public class UIBuffBar : UIBase
    {
        [SerializeField]
        private GameObject m_BuffPrefab = null;
        [SerializeField]
        private float m_Spacing = 3.0f;

        private List<UIBuff> m_Buffs = new List<UIBuff>();

        public void AddBuff(string aTitle, string aDescription)
        {
            if(string.IsNullOrEmpty(aTitle))
            {
                DebugUtils.ArgumentNull("aTitle");
                return;
            }
            if(string.IsNullOrEmpty(aDescription))
            {
                DebugUtils.ArgumentNull("aDescription");
                return;
            }

            if(m_BuffPrefab != null)
            {
                GameObject prefab = Instantiate<GameObject>(m_BuffPrefab);
                UIBuff buff = prefab.GetComponent<UIBuff>();

                if(buff == null)
                {
                    Destroy(prefab);
                    return;
                }

                buff.Setup(aTitle, aDescription);
                m_Buffs.Add(buff);
                UpdateBuffPositions();
            }
        }

        public void RemoveBuff(string aTitle)
        {
            for(int i = 0; i < m_Buffs.Count; i++)
            {
                if(m_Buffs[i].title == aTitle)
                {
                    m_Buffs.RemoveAt(i);
                    UpdateBuffPositions();
                    break;
                }
            }
        }

        public void UpdateBuffPositions()
        {
            float start = 0.0f;

            for(int i = 0; i < m_Buffs.Count; i++)
            {
                m_Buffs[i].rectTransform.anchoredPosition = new Vector2(start, 0.0f);
                start += m_Buffs[i].rectTransform.sizeDelta.x + m_Spacing;
            }
        }

    }
}
