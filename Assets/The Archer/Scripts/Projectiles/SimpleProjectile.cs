using OctoberStudio.Audio;
using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class SimpleProjectile : AbstractProjectile
    {
        public IProjectileTarget TargetToIgnore { get; set; }

        public float Speed { get; set; }
        public float Magnetism { get; set; }

        [SerializeField] protected float lifetime = 10;
        [SerializeField] protected float speed = 10;

        [Space]
        [SerializeField] protected bool ignoreGround = false;
        [SerializeField] protected bool useYAxisInCalculations = true;

        [Header("Magnetism")]
        [SerializeField] protected float magnetism = 0;
        [SerializeField] protected float magnetismStrongestDistance = 0f;
        [SerializeField] protected float magnetismWeakestDistance = 5f;

        [Header("VFX")]
        [SerializeField] protected GameObject onDeathParticlePrefab;
        [SerializeField] protected GameObject splitProjectilePrefab;
        [SerializeField] protected List<TrailRenderer> trailsToDisableOnHit;

        [Space]
        [SerializeField] protected bool blockTargetsHitSound;
        [SerializeField] protected AudioData projectileHitTargetSound;
        [SerializeField] protected AudioData projectileHitObstacleSound;

        protected float launchTime;

        protected float BounceDamageMultiplier { get; set; }
        protected float MaxBouncesCount { get; set; }

        protected float RicochetDamageMultiplier { get; set; }
        protected float MaxRicochetCount { get; set; }

        protected float BouncesCount { get; set; }
        protected float RicochetCount { get; set; }

        public float SplitDamageMultiplier { get; set; }
        public int SplitCount { get; set; }

        public bool IsCriticalHit { get; set; }

        public bool IsLaunched { get; protected set; }

        protected virtual void Awake()
        {
            ResetOverrides();

            for (int i = 0; i < trailsToDisableOnHit.Count; i++)
            {
                trailsToDisableOnHit[i].Clear();
                trailsToDisableOnHit[i].enabled = false;
            }
        }

        protected virtual void Start()
        {
            if (onDeathParticlePrefab != null) StageController.ParticlesManager.RegisterParticle(onDeathParticlePrefab);
            if (splitProjectilePrefab != null) StageController.ProjectilesManager.RegisterProjectile(splitProjectilePrefab);
        }

        public override void Launch(float damage)
        {
            Damage = damage;
            launchTime = Time.time;

            BouncesCount = 0;
            RicochetCount = 0;

            IsLaunched = true;

            for (int i = 0; i < trailsToDisableOnHit.Count; i++)
            {
                trailsToDisableOnHit[i].Clear();
                trailsToDisableOnHit[i].enabled = true;
            }
        }

        public override void Hide()
        {
            base.Hide();

            if (onDeathParticlePrefab != null)
            {
                var deathParticle = StageController.ParticlesManager.GetParticle(onDeathParticlePrefab);
                deathParticle.transform.position = transform.position;
                deathParticle.Play();
            }

            gameObject.SetActive(false);
            ResetOverrides();

            for (int i = 0; i < trailsToDisableOnHit.Count; i++)
            {
                trailsToDisableOnHit[i].Clear();
                trailsToDisableOnHit[i].enabled = false;
            }
        }

        public override void SetBounceData(int maxBounceCount, float bounceDamageMultiplier)
        {
            BounceDamageMultiplier = bounceDamageMultiplier;
            MaxBouncesCount = maxBounceCount;
        }

        public override void SetRicochetData(int maxRecochetCount, float recochetDamageMultiplier)
        {
            RicochetDamageMultiplier = recochetDamageMultiplier;
            MaxRicochetCount = maxRecochetCount;
        }

        public virtual void ResetOverrides()
        {
            Target = null;
            Lifetime = lifetime;
            Speed = speed;
            Magnetism = magnetism;

            MaxBouncesCount = 0;
            BounceDamageMultiplier = 0;

            MaxRicochetCount = 0;
            RicochetDamageMultiplier = 0;

            SplitDamageMultiplier = 0;
            SplitCount = 0;

            DamageWithDistanceMultiplier = null;

            IsCriticalHit = false;

            IsLaunched = false;
        }

        protected virtual void TryRicochet(IProjectileTarget target)
        {
            if (RicochetCount < MaxRicochetCount)
            {
                var colliders = Physics.OverlapSphere(target.Position, 20, 1 << target.Layer);

#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
                IProjectileTarget closestNewTarget = StageController.Room.GetClosestEnemy(target.Position, (enemy) => enemy != target);
#pragma warning restore CS0253

                if (closestNewTarget != null)
                {
                    transform.position = target.Position.SetY(transform.position.y);
                    transform.forward = transform.position.DirectionToXZ(closestNewTarget.Position);
                    RicochetCount++;
                    Damage *= RicochetDamageMultiplier;
                }
                else
                {
                    if (!IsPiercingEnabled)
                    {
                        TrySplitArrow(target.Position.SetY(transform.position.y), target);
                        Hide();
                    }
                }
            }
            else
            {
                if (!IsPiercingEnabled)
                {
                    TrySplitArrow(target.Position.SetY(transform.position.y), target);
                    Hide();
                }
            }
        }

        protected virtual void TryBounce()
        {
            if (BouncesCount < MaxBouncesCount)
            {
                if (Physics.Raycast(transform.position - transform.forward * 2, transform.forward, out var hit, 10f, 4096))
                {
                    transform.forward = Vector3.Reflect(transform.forward, hit.normal).SetY(0).normalized;
                    BouncesCount++;
                    Damage *= BounceDamageMultiplier;

                    if (onDeathParticlePrefab != null)
                    {
                        var deathParticle = StageController.ParticlesManager.GetParticle(onDeathParticlePrefab);
                        deathParticle.transform.position = transform.position;
                        deathParticle.Play();
                    }
                }
                else
                {
                    TrySplitArrow(transform.position - transform.forward * 0.1f, null);
                    Hide();
                }
            }
            else
            {
                TrySplitArrow(transform.position - transform.forward * 0.1f, null);
                Hide();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (StageController.Room.AliveEnemiesCount == 0)
            {
                Hide();
                return;
            }

            var target = other.GetComponent<IProjectileTarget>();
            if (target != null)
            {
                if (target == TargetToIgnore) return;

                target.TakeDamage(GetDamage(), GetDamageType(), IsCriticalHit);
                target.ShowHitEffect(transform.forward, true);

                if (target.IsAlive)
                {
                    for (int i = 0; i < appliedStatusEffects.Count; i++)
                    {
                        appliedStatusEffects[i].ApplyToTarget(target, GetDamage());
                    }
                }

                GameController.AudioManager.PlayAudio(projectileHitTargetSound);

                TryRicochet(target);
            }
            else
            {
                if (!IgnoresObstacles)
                {
                    if (other.tag == "Wall" || other.tag == "Obstacle")
                    {
                        GameController.AudioManager.PlayAudio(projectileHitObstacleSound);
                        TryBounce();
                    }
                    else if (!(other.CompareTag("Ground") && ignoreGround))
                    {
                        GameController.AudioManager.PlayAudio(projectileHitObstacleSound);
                        Hide();
                    }
                }
                else if (other.tag == "Wall")
                {
                    GameController.AudioManager.PlayAudio(projectileHitObstacleSound);
                    TryBounce();
                }
            }
        }

        protected virtual void TrySplitArrow(Vector3 position, IProjectileTarget target)
        {
            for (int i = 0; i < SplitCount; i++)
            {
                var angle = 360f / SplitCount * i;

                var direction = Quaternion.Euler(0, angle, 0) * transform.forward;

                CreateSplitShotProjectile(position, direction, target);
            }
        }

        protected virtual void Update()
        {
            if (!IsLaunched) return;

            if (Target != null && Target.gameObject.activeSelf && Magnetism > 0)
            {
                var distanceToTarget = Vector3.Distance(transform.position, Target.position);
                var distanceFactor = Mathf.InverseLerp(magnetismWeakestDistance, magnetismStrongestDistance, distanceToTarget);

                var directionToTarget = (Target.position - transform.position).normalized;
                if (!useYAxisInCalculations) directionToTarget = directionToTarget.NormalizeXZ();

                var forward = Vector3.RotateTowards(transform.forward, directionToTarget, Magnetism * Time.deltaTime * distanceFactor, 0.0f);
                transform.forward = forward;
            }

            transform.position += transform.forward * Time.deltaTime * Speed;

            if (CheckLifetime()) Hide();
        }

        protected override float GetDamageDistanceMultiplier()
        {
            if (DamageWithDistanceMultiplier == null) return 1;

            var duration = Time.time - launchTime;
            var distance = duration * Speed;

            return DamageWithDistanceMultiplier.Evaluate(distance);
        }

        protected virtual void CreateSplitShotProjectile(Vector3 position, Vector3 direction, IProjectileTarget targetToIgnore)
        {
            if (splitProjectilePrefab == null) return;

            var projectile = StageController.ProjectilesManager.GetProjectile<SimpleProjectile>(splitProjectilePrefab);
            projectile.transform.position = position;

            projectile.transform.forward = direction;

            projectile.IsPiercingEnabled = IsPiercingEnabled;

            projectile.IgnoresObstacles = IgnoresObstacles;
            projectile.TargetToIgnore = targetToIgnore;

            projectile.Launch(GetDamage() * SplitDamageMultiplier);
        }

        protected virtual bool CheckLifetime()
        {
            return launchTime + Lifetime <= Time.time;
        }

        public override void Clear()
        {
            base.Clear();

            gameObject.SetActive(false);
            ResetOverrides();
        }
    }
}