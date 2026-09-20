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
    }
}