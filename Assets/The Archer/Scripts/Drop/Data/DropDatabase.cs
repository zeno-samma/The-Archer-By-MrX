using System.Collections.Generic;
using UnityEngine;

namespace MrX.Drop
{
    [CreateAssetMenu(fileName = "Drop Database", menuName = "MrX Studio/Drop Database")]
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