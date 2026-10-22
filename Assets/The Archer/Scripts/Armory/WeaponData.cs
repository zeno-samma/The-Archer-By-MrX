using MrX.Armory;
using UnityEngine;

namespace MrX.Weapon
{
    [CreateAssetMenu(fileName = "Weapon Data 001", menuName = "MrX Studio/Armory/Weapon Data")]
    public class WeaponData : ItemData
    {
        [Space]
        [SerializeField] protected GameObject weaponPrefab;
        public GameObject WeaponPrefab => weaponPrefab;

        [SerializeField] protected bool isRightHandWeapon = true;
        public bool IsRightHandWeapon => isRightHandWeapon;

        [Header("Class equipment")]
        [SerializeField] private HeroClasses allowedClasses = HeroClasses.All;
        [SerializeField] private GameObject offhandVisualPrefab;
        [SerializeField] private bool isMelee;
        [SerializeField, Min(.5f)] private float attackReach = 2.6f;
        public bool IsMelee => isMelee;
        public float AttackReach => Mathf.Max(.5f, attackReach);
        public GameObject OffhandVisualPrefab => offhandVisualPrefab;
        public bool Supports(HeroData hero) => hero != null && (allowedClasses & (HeroClasses)(1 << (int)hero.HeroClass)) != 0;

        protected override void OnValidate()
        {
            base.OnValidate();

            itemType = ItemType.Weapon;
        }
    }
}
