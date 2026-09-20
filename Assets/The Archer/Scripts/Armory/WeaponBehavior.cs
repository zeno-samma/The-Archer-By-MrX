using OctoberStudio.Audio;
using OctoberStudio.Enemy;
using OctoberStudio.Extensions;
using OctoberStudio.Projectile;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Weapon
{
    public class WeaponBehavior : AbstractWeaponBehavior
    {
        protected static readonly int CAN_ATTACK_BOOL = Animator.StringToHash("Can Attack");
        protected static readonly int ATTACK_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Attack Speed Multiplier");

        [SerializeField] protected Transform projectileSpawn;
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected AudioData shootingSound;
        [Space]
        [SerializeField] protected float frontArrowSpacing = 0.3f;

        [Space]
        [Tooltip("Plays shooting animation when attached to the hero. Make sure that the shootinh animation is synchronized with the shooting animation of the hero")]
        [SerializeField] protected bool animateInHands = true;
        [Tooltip("Plays shooting animation when detached from the hero (Flying Weapon ability)")]
        [SerializeField] protected bool animateDetached = true;

        [Space]
        [SerializeField] protected Vector3 detachedPosition;
        [SerializeField] protected Vector3 detachedRotation;
        [SerializeField] protected Vector3 detachedScale;

        public override Vector3 DetachedPosition => detachedPosition;
        public override Vector3 DetachedRotation => detachedRotation;
        public override Vector3 DetachedScale => detachedScale;

        [SerializeField] protected Animator animator;
        [SerializeField] protected ParticleSystem shootParticle;

        protected float animationLength;
        protected Coroutine detachedShootingCoroutine;
        
        public override void Init(WeaponData weaponData)
        {
            base.Init(weaponData);

            if (StageController.IsLoaded)
            {
                StageController.Player.Stats.AttackSpeedStat.onStatChanged += OnAttackSpeedChanged;
                StageController.Player.onStartedMoving += OnPlayerStartedMoving;
                StageController.Player.onStoppedMoving += OnPlayerStoppedMoving;
                StageController.Player.onClosestEnemyChanged += OnClosestEnemyChanged;
            }
        }

        protected virtual void Start()
        {
            if (StageController.IsLoaded)
            {
                StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
            }
        }

        protected virtual void OnPlayerStartedMoving()
        {
            if (!IsDetached && animator != null)
            {
                animator.SetBool(CAN_ATTACK_BOOL, false);
            }
        }

        protected virtual void OnPlayerStoppedMoving()
        {
            if (animator == null || StageController.Player.ClosestEnemy == null) return;

            if (IsDetached)
            {
                animator.SetBool(CAN_ATTACK_BOOL, animateDetached);
            } else
            {
                animator.SetBool(CAN_ATTACK_BOOL, animateInHands);
            }
        }

        protected virtual void OnClosestEnemyChanged(EnemyBehavior closestEnemy)
        {
            if (animator == null) return;

            if (StageController.Player.MovedThisFrame && !IsDetached)
            {
                animator.SetBool(CAN_ATTACK_BOOL, false);
            }
            else
            {
                if (closestEnemy == null)
                {
                    animator.SetBool(CAN_ATTACK_BOOL, false);
                } else
                {
                    if (IsDetached)
                    {
                        animator.SetBool(CAN_ATTACK_BOOL, animateDetached);
                    }
                    else
                    {
                        animator.SetBool(CAN_ATTACK_BOOL, animateInHands);
                    }
                }
            }
        }

        public override void OnAttachedToHero()
        {
            IsDetached = false;

            if (!animateDetached)
            {
                if (detachedShootingCoroutine != null) StopCoroutine(detachedShootingCoroutine);
            }
        }

        public override void OnDetachedFromHero()
        {
            IsDetached = true;

            if (!animateDetached)
            {
                detachedShootingCoroutine = StartCoroutine(DetachedShootingCoroutine());
            }
        }

        protected virtual IEnumerator DetachedShootingCoroutine()
        {
            while (true)
            {
                var target = StageController.Player.ClosestEnemy;
                if(target != null)
                {
                    var targetTransform = target.transform;
                    var direction = transform.DirectionToXZ(targetTransform.position);
                    Shoot(target.transform, direction);
                }
                
                yield return new WaitForSeconds(1f / StageController.Player.Stats.AttackSpeedStat);
            }
        }

        public override void SetAnimationLength(float animationLength)
        {
            this.animationLength = animationLength;
        }

        protected virtual void OnAttackSpeedChanged(MultiplicativeStat attackSpeedStat)
        {
            if(animator != null) animator.SetFloat(ATTACK_SPEED_MULTIPLIER_FLOAT, attackSpeedStat / animationLength);
        }

        public override void OnShootEventFired()
        {
            if (IsDetached)
            {
                var target = StageController.Player.ClosestEnemy;
                var targetTransform = target == null ? null : target.transform;
                var direction = targetTransform == null ? transform.forward : transform.DirectionToXZ(targetTransform.position);
                Shoot(target?.transform, direction);
            }
        }

        public override void Shoot(Transform target, Vector3 direction)
        {
            var player = StageController.Player;
            Shoot(target, projectileSpawn.position, direction.NormalizeXZ(), player.Stats.FrontArrowsCountStat, frontArrowSpacing, player.Stats.FrontArrowDamageStat);

            if (player.Stats.RearArrowsCountStat > 0)
            {
                Shoot(target, projectileSpawn.position, -direction.NormalizeXZ(), player.Stats.RearArrowsCountStat, frontArrowSpacing, player.Stats.RearArrowDamageStat);
            }

            if(player.Stats.DiagonalArrowsCountStat > 0)
            {
                Shoot(target, projectileSpawn.position, Quaternion.Euler(0, -45, 0) * direction.NormalizeXZ(), player.Stats.DiagonalArrowsCountStat, frontArrowSpacing, player.Stats.DiagonalArrowDamageStat);
                Shoot(target, projectileSpawn.position, Quaternion.Euler(0, 45, 0) * direction.NormalizeXZ(), player.Stats.DiagonalArrowsCountStat, frontArrowSpacing, player.Stats.DiagonalArrowDamageStat);
            }

            if(shootingSound != null) GameController.AudioManager.PlayAudio(shootingSound);
        }

        protected virtual void Shoot(Transform target, Vector3 spawnPosition, Vector3 direction, int arrowsCount, float spacing, float damage)
        {
            var player = StageController.Player;

            var left = Quaternion.Euler(0, -90, 0) * direction.NormalizeXZ();
            var step = -left * spacing;

            if (arrowsCount > 1)
            {
                spawnPosition += left * spacing * (arrowsCount - 1) / 2f;
            }

            for (int i = 0; i < arrowsCount; i++)
            {
                var position = spawnPosition + step * i;

                var projectile = StageController.ProjectilesManager.GetProjectile<SimpleProjectile>(projectilePrefab);
                projectile.transform.position = position;

                var spreadAngle = player.Stats.ArrowSpreadStat;

                var forward = Quaternion.Euler(0, Random.Range(-1f, 1f) * spreadAngle, 0) * direction;
                if (forward == Vector3.zero) forward = Vector3.forward;

                projectile.transform.forward = forward;

                projectile.SetBounceData(player.Stats.ArrowBounceCountStat, player.Stats.ArrowBounceDamageStat);
                projectile.SetRicochetData(player.Stats.ArrowRicochetCountStat, player.Stats.ArrowRicochetDamageStat);
                projectile.IsPiercingEnabled = player.IsPiercingArrowEnabled;

                projectile.DamageWithDistanceMultiplier = player.DistanceDamageMultiplierCurve;
                projectile.IgnoresObstacles = player.ProjectilesIgnoreObstacles;
                projectile.Magnetism = player.Stats.MagnetismStat.Value;

                projectile.transform.localScale = Vector3.one * player.Stats.ArrowSizeStat;

                projectile.SplitCount = player.Stats.SplitArrowCountStat;
                projectile.SplitDamageMultiplier = player.Stats.SpliArrowDamageMultiplierStat;

                if (player.Stats.ArrowRangeStat.IsModified())
                {
                    projectile.Lifetime = (player.Stats.ArrowRangeStat / projectile.Speed) * 0.8f;
                }

                if (appliedEffects.Count > 0)
                {
                    projectile.ApplyStatusEffects(appliedEffects);

                    var biggestMultiplier = 0f;
                    for (int j = 0; j < appliedEffects.Count; j++)
                    {
                        var multiplier = player.Stats.GetArrowSpeedMultiplier(appliedEffects[j].Type);
                        if (multiplier > biggestMultiplier)
                        {
                            biggestMultiplier = multiplier;
                        }
                    }

                    projectile.Speed *= biggestMultiplier;
                }
                
                if(Random.value * 100 < player.Stats.CritChanceStat)
                {
                    projectile.IsCriticalHit = true;
                    damage *= player.Stats.CritDamageMultiplierStat;
                }

                projectile.Launch(damage);

                projectile.Target = target;

                if(shootParticle != null) shootParticle.Play();
            }
        }

        protected virtual void OnDestroy()
        {
            if (StageController.IsLoaded)
            {
                StageController.Player.Stats.AttackSpeedStat.onStatChanged -= OnAttackSpeedChanged;
                StageController.Player.onStartedMoving -= OnPlayerStartedMoving;
                StageController.Player.onStoppedMoving -= OnPlayerStoppedMoving;
                StageController.Player.onClosestEnemyChanged -= OnClosestEnemyChanged;
            }
        }
    }
}