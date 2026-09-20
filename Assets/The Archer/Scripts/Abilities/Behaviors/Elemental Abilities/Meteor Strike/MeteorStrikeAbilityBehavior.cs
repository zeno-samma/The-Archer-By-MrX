using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class MeteorStrikeAbilityBehavior : AbilityBehavior<MeteorStrikeAbilityData, MeteorStrikeAbilityLevel>
    {
        [SerializeField] protected GameObject meteorProjectilePrefab;

        protected float lastTimeMeteorLaunched;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.ProjectilesManager.RegisterProjectile(meteorProjectilePrefab);
            lastTimeMeteorLaunched = Time.time;
        }

        protected virtual void Update()
        {
            if (Time.time - lastTimeMeteorLaunched >= AbilityLevel.MeteorSpawnDelay * StageController.Player.Stats.MeteorSpawnDelayStat)
            {
                if (StageController.Room.AliveEnemiesCount == 0) return;

                LaunchMeteor();
                lastTimeMeteorLaunched = Time.time;
            }
        }

        protected virtual void LaunchMeteor()
        {
            var enemy = StageController.Room.GetRandomEnemy();
            if (enemy == null) return;

            var meteor = StageController.ProjectilesManager.GetProjectile(meteorProjectilePrefab);
            meteor.transform.position = enemy.Position;
            meteor.Launch(AbilityLevel.MeteorDamageMultiplier * StageController.Player.Stats.AttackDamageStat);
        }
    }
}