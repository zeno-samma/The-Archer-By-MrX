using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Diagonal Shot Ability Data", menuName = "October/Abilities/Weapon Abilities/Diagonal Shot")]
    public class DiagonalShotAbilityData : GenericAbilityData<DiagonalShotAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Weapon_DiagonalShot;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Weapon_DiagonalShot;
        }
    }

    [System.Serializable]
    public class DiagonalShotAbilityLevel : AbilityLevel
    {
        [Tooltip("The amount of arrows per diagonal. If the value is 1, the Player will shoot two additional arrows, one per diagonal")]
        [SerializeField, Min(1)] int diagonalArrowsCount = 1;
        public int DiagonalArrowsCount => diagonalArrowsCount;

        [SerializeField, Min(0.01f)] float diagonalArrowsDamageMultiplier = 0.8f;
        public float DiagonalArrowsDamageMultiplier => diagonalArrowsDamageMultiplier;
    }
}