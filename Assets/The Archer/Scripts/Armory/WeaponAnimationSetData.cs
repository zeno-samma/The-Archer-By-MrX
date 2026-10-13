using UnityEngine;

namespace OctoberStudio.Armory
{
    [System.Serializable]
    public class WeaponAnimationSetData
    {
        [SerializeField] protected string weaponId;
        [SerializeField] protected string heroId;
        [SerializeField] protected WeaponAnimationsSet animationsSet;

        public string WeaponId => weaponId;
        public string HeroId => heroId;
        public WeaponAnimationsSet AnimationsSet => animationsSet;

#if UNITY_EDITOR
        public virtual WeaponAnimationSetData Clone(string newHeroId)
        {
            var newSet = new WeaponAnimationSetData();
            newSet.weaponId = weaponId;
            newSet.heroId = newHeroId;
            newSet.animationsSet = animationsSet;

            return newSet;
        }
#endif
    }
}