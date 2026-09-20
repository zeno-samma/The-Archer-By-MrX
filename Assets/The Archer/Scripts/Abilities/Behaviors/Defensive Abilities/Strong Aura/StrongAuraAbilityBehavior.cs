using OctoberStudio.Enemy;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.Abilities
{
    public class StrongAuraAbilityBehavior : AbilityBehavior<StrongAuraAbilityData, StrongAuraAbilityLevel>
    {
        [FormerlySerializedAs("particle")]
        [SerializeField] protected ParticleSystem auraParticle;
        [SerializeField] protected SphereCollider auraCollider;

        protected StatMultiplier enemyDamageReceivedStatMultiplier = 1;
        protected StatMultiplier attackDamageStatMultiplier = 1;

        protected List<EnemyBehavior> enemiesInRange = new List<EnemyBehavior>();

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageStatMultiplier);

            auraParticle.gameObject.SetActive(true);
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            auraParticle.transform.localScale = Vector3.one * AbilityLevel.AbilityRadius * 2;
            auraCollider.radius = AbilityLevel.AbilityRadius;

            enemyDamageReceivedStatMultiplier.Value = AbilityLevel.EnemyDamageReceivedMultiplierInsideRadius;
            attackDamageStatMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var enemy = other.GetComponent<EnemyBehavior>();

            if(enemy != null)
            {
                enemy.TakeDamage(StageController.Player.Damage * AbilityLevel.EnemyDamageMultiplierWhenEntering, DamageType.Physical);

                if (enemy.IsAlive)
                {
                    enemy.ReceivedDamageMultiplierStat.AddMultiplier(enemyDamageReceivedStatMultiplier);
                    enemy.SubscribeOnDefeat(OnEnemyDefeated);
                    enemiesInRange.Add(enemy);
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var enemy = other.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                enemy.ReceivedDamageMultiplierStat.RemoveMultiplier(enemyDamageReceivedStatMultiplier);
                enemy.UnsubscribeOnDefeat(OnEnemyDefeated);
                enemiesInRange.Remove(enemy);
            }
        }

        protected virtual void OnEnemyDefeated(IDefeatable defeatable)
        {
            var enemy = defeatable as EnemyBehavior;
            if (enemy == null) return;

            enemy.UnsubscribeOnDefeat(OnEnemyDefeated);
            enemy.ReceivedDamageMultiplierStat.RemoveMultiplier(enemyDamageReceivedStatMultiplier);
            enemiesInRange.Remove(enemy);
        }

        public override void Clear()
        {
            foreach(var enemy in enemiesInRange)
            {
                enemy.UnsubscribeOnDefeat(OnEnemyDefeated);
                enemy.ReceivedDamageMultiplierStat.RemoveMultiplier(enemyDamageReceivedStatMultiplier);
            }

            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageStatMultiplier);

            base.Clear();
        }
    }
}