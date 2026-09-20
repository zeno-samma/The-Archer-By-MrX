using OctoberStudio.Extensions;
using OctoberStudio.StatusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class SpiritBehavior : MonoBehaviour
    {
        protected List<StatusEffect> appliedEffects = new List<StatusEffect>();

        protected Transform Position { get; set; }

        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;

        [Header("Settings")]
        [SerializeField] protected AnimationCurve speedDistanceCurve;

        public MultiplicativeStat DamageStat { get; set; }
        public MultiplicativeStat AttackDelayStat { get; set; }

        protected float speed = 0;

        protected IProjectileTarget Target { get; set; }

        protected virtual void Start()
        {
            if (projectilePrefab != null)
            {
                StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
            }

            DamageStat = 1;
            AttackDelayStat = 1;

            StageController.Player.SpiritsManager.RegisterSpirit(this);

            StartCoroutine(AttackCoroutine());
        }

        public virtual void ApplyEffect(StatusEffect effect)
        {
            appliedEffects.Add(effect);
        }

        public virtual void SetPosition(Transform position)
        {
            Position = position;
        }

        public virtual void HardResetPosition()
        {
            transform.position = Position.position;
        }

        protected virtual void Update()
        {
            if (Target != null)
            {
                var rotationToTarget = Quaternion.LookRotation(transform.position.DirectionToXZ(Target.Position), Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, Time.deltaTime * 8);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Position.transform.rotation, Time.deltaTime * 8);
            }

            var distance = (Position.position - transform.position).magnitude;
            if (distance > 0.1f)
            {
                var direction = transform.position.DirectionTo(Position.position);

                var speed = speedDistanceCurve.Evaluate(distance);

                var frameMove = direction * Time.deltaTime * speed;
                if (frameMove.magnitude >= distance)
                {
                    transform.position = Position.position;

                }
                else
                {
                    transform.position += frameMove;
                }
            }
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            var targetSearchWait = new WaitForSeconds(0.2f);

            while (true)
            {
                var targetWait = 0f;

                if (Target != null)
                {
                    Target.UnsubscribeOnDefeat(OnTargetDefeated);
                }

                Target = StageController.Player.ClosestEnemy;

                if (Target != null)
                {
                    Target.SubscribeOnDefeat(OnTargetDefeated);
                }

                while (Target == null)
                {
                    targetWait += 0.2f;
                    yield return targetSearchWait;

                    Target = StageController.Player.ClosestEnemy;
                    if (Target != null)
                    {
                        Target.SubscribeOnDefeat(OnTargetDefeated);
                    }
                }

                var attackWait = AttackDelayStat - targetWait;

                if (attackWait > 0)
                {
                    yield return new WaitForSeconds(attackWait);
                }
                else
                {
                    yield return null;
                }

                if (Target != null && projectilePrefab != null)
                {
                    var projectile = StageController.ProjectilesManager.GetProjectile(projectilePrefab);

                    projectile.transform.position = projectileSpawnPosition.position;

                    projectile.transform.forward = projectile.transform.position.DirectionTo(Target.Position + Vector3.up * 0.5f);
                    projectile.Target = Target.Transform;

                    projectile.ApplyStatusEffects(appliedEffects);
                    projectile.Launch(StageController.Player.Damage * DamageStat.Value);
                }
            }
        }

        protected void OnTargetDefeated(IDefeatable defeatable)
        {
            Target.UnsubscribeOnDefeat(OnTargetDefeated);
            Target = null;
        }

        public void Clear()
        {
            StageController.Player.SpiritsManager.RemoveSpirit(this);
        }
    }
}