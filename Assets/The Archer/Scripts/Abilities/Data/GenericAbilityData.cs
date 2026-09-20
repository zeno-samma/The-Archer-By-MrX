using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class GenericAbilityData<T> : AbilityData where T : AbilityLevel
    {
        [SerializeField] protected T[] levels;
        public override AbilityLevel[] Levels => levels;

        public virtual new T GetLevel(int index)
        {
            return levels[index];
        }
    }
}