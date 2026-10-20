using UnityEngine;

namespace MrX.Abilities
{
    [CreateAssetMenu(fileName = "PerfectStance Ability Data", menuName = "MrX Studio/Abilities/Buff Abilities/PerfectStance")]
    public class PerfectStanceAbilityData : GenericAbilityData<PerfectStanceAbilityLevel>
    {
        protected virtual void Awake()
        {
            type = AbilityType.Buff_PerfectStance;
        }

        protected virtual void OnValidate()
        {
            type = AbilityType.Buff_PerfectStance;
        }
    }

    [System.Serializable]
    public class PerfectStanceAbilityLevel : AbilityLevel
    {
        [SerializeField] protected AnimationCurve timeDamageMultiplierCurve;
        public AnimationCurve TimeDamageMultiplierCurve => timeDamageMultiplierCurve;

        [SerializeField] protected AnimationCurve timeAttackSpeedMultiplierCurve;
        public AnimationCurve TimeAttackSpeedMultiplierCurve => timeAttackSpeedMultiplierCurve;

        [SerializeField] protected float perfectStanceParticleShowTime = 2f;
        public float PerfectStanceParticleShowTime => perfectStanceParticleShowTime;
    }
}