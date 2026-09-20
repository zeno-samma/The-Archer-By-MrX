using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class KillHealAbilityBehavior : AbilityBehavior<KillHealAbilityData, KillToHealAbilityLevel>
    {
        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.DoAfterRoomLoaded(() => StageController.Room.onEnemyDefeated += OnEnemyDefeated);
        }

        protected virtual void OnEnemyDefeated()
        {
            if (Random.value < AbilityLevel.HealChance)
            {
                StageController.Player.HealProportion(AbilityLevel.MaxHealthProportionHeal);
            }
        }

        public override void Clear()
        {
            StageController.Room.onEnemyDefeated -= OnEnemyDefeated;

            base.Clear();
        }
    }
}