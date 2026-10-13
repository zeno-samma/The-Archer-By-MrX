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

#if UNITY_EDITOR
        public virtual void AddClonedHero(HeroData newHero, HeroData oldHero)
        {
            var list = new List<WeaponAnimationSetData>(weaponAnimationSets);

            var oldSets = list.FindAll((set) => set.HeroId == oldHero.Id);

            if (oldSets == null || oldSets.Count == 0) return;

            foreach (var oldSet in oldSets)
            {
                var newSet = oldSet.Clone(newHero.Id);
                list.Add(newSet);
            }

            weaponAnimationSets = list.ToArray();
        }
#endif
    }
}