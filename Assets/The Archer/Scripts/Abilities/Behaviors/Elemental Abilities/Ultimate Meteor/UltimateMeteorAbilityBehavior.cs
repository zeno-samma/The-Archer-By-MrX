using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class UltimateMeteorAbilityBehavior : AbilityBehavior<UltimateMeteorAbilityData, UltimateMeteorAbilityLevel>
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
            var meteor = StageController.ProjectilesManager.GetProjectile(meteorProjectilePrefab);

            var playerPosition = StageController.Player.transform.position;

            var counter = 0;
            var position = playerPosition;
            var found = false;
            while (counter < 30)
            {
                var enemy = StageController.Room.GetRandomEnemy();
                if (enemy == null) continue;

                position = enemy.Position + Random.onUnitSphere.SetY(0) * AbilityLevel.MaxOffsetFromEnemy;
                if (StageController.NavigationManager.IsPositionAvailable(position))
                {
                    found = true;
                    break;
                }
                counter++;
            }

            if(!found) position = playerPosition + Random.onUnitSphere.SetY(0).normalized * 5;

            meteor.transform.position = position;
            meteor.Launch(AbilityLevel.MeteorDamageMultiplier * StageController.Player.Stats.AttackDamageStat);
        }
    }
}