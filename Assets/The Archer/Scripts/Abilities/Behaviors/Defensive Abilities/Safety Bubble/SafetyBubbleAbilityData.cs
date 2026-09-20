using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "SafetyBubble Ability Data", menuName = "October/Abilities/Defensive Abilities/SafetyBubble")]
    public class SafetyBubbleAbilityData : GenericAbilityData<SafetyBubbleAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Defence_SafetyBubble;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Defence_SafetyBubble;
        }
    }

    [System.Serializable]
    public class SafetyBubbleAbilityLevel : AbilityLevel
    {
        [SerializeField] protected int hitsToDisableProtection = 1;
        public int HitsToDisableProtection => hitsToDisableProtection;
    }
}