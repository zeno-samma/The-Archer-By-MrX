using OctoberStudio.Armory;
using UnityEngine;

namespace OctoberStudio.Weapon
{
    [CreateAssetMenu(fileName = "Weapon Data 001", menuName = "October/Armory/Weapon Data")]
    public class WeaponData : ItemData
    {
        [Space]
        [SerializeField] protected GameObject weaponPrefab;
        public GameObject WeaponPrefab => weaponPrefab;

        [SerializeField] protected bool isRightHandWeapon = true;
        public bool IsRightHandWeapon => isRightHandWeapon;

        protected override void OnValidate()
        {
            base.OnValidate();

            itemType = ItemType.Weapon;
        }
    }
}