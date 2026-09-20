using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Drop
{
    [CreateAssetMenu(fileName = "Drop Database", menuName = "October/Drop Database")]
    public class DropDatabase : ScriptableObject
    {
        [SerializeField] protected List<DropData> drop;

        public int GemsCount => drop.Count;

        public virtual DropData GetGemData(int index)
        {
            return drop[index];
        }

        public virtual DropData GetGemData(DropType dropType)
        {
            for (int i = 0; i < drop.Count; i++)
            {
                if (drop[i].DropType == dropType) return drop[i];
            }
            return null;
        }
    }
}