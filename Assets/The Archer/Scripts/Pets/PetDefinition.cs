using MrX.Abilities;
using UnityEngine;

namespace MrX.Pets
{
    [CreateAssetMenu(fileName = "ArcBornPet", menuName = "ArcBorn/Pets/Pet Definition")]
    public sealed class PetDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Freeze Spirit";
        [SerializeField] private Sprite portrait;
        [SerializeField] private FreezeSpiritAbilityData combatAbility;

        [Header("Food")]
        [SerializeField, Min(0)] private int startingCakes = 3;
        [SerializeField, Min(1)] private int cakesPerFeeding = 1;

        [Header("Owner bonuses (percent)")]
        [SerializeField, Min(0)] private float attackBonus;
        [SerializeField, Min(0)] private float maxHealthBonus;
        [SerializeField, Min(0)] private float criticalChanceBonus;
        [SerializeField, Min(0)] private float movementSpeedBonus;

        public string DisplayName => displayName;
        public Sprite Portrait => portrait;
        public FreezeSpiritAbilityData CombatAbility => combatAbility;
        public int StartingCakes => Mathf.Max(0, startingCakes);
        public int CakesPerFeeding => Mathf.Max(1, cakesPerFeeding);
        public float AttackBonus => attackBonus;
        public float MaxHealthBonus => maxHealthBonus;
        public float CriticalChanceBonus => criticalChanceBonus;
        public float MovementSpeedBonus => movementSpeedBonus;
    }
}
