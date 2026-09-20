using OctoberStudio.Enemy;

namespace OctoberStudio.Abilities
{
    public class BossSlayerAbilityBehavior : AbilityBehavior<BossSlayerAbilityData, BossSlayerAbilityLevel>
    {
        protected StatMultiplier damageToBossesMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);
            StageController.Player.Stats.DamageToBossesMultiplierStat.AddMultiplier(damageToBossesMultiplier);

            StageController.DoAfterRoomLoaded(() =>
            {
                StageController.Room.onBossfightStarted += OnBossfightStarted;
            });
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            damageToBossesMultiplier.Value = AbilityLevel.DamageToBossesMultiplier;
        }

        protected virtual void OnBossfightStarted(EnemyData bossData)
        {
            StageController.Player.HealProportion(1f);
        }

        public override void Clear()
        {
            StageController.Player.Stats.DamageToBossesMultiplierStat.RemoveMultiplier(damageToBossesMultiplier);

            StageController.Room.onBossfightStarted -= OnBossfightStarted;

            base.Clear();
        }
    }
}