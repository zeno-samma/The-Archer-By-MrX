using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Armory
{
    [System.Serializable]
    public class WeaponAnimationSetDataList
    {
        [SerializeField] protected WeaponAnimationSetData[] weaponAnimationSets;

        public int SetsCount => weaponAnimationSets.Length;
        public WeaponAnimationSetData[] Sets => weaponAnimationSets;

        public virtual void Validate(ArmoryDatabase database)
        {
            var list = new List<WeaponAnimationSetData>(weaponAnimationSets);

            for (int i = 0; i < list.Count; i++)
            {
                var set = list[i];

                if (database.GetHero(set.HeroId) == null)
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }

                if (database.GetItem(set.WeaponId) == null)
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            weaponAnimationSets = list.ToArray();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(database);
#endif
        }
    }
}