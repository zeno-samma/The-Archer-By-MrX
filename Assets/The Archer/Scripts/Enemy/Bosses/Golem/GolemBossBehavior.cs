using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Projectile;
using OctoberStudio.StatusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.Enemy
{
    public class GolemBossBehavior : EnemyBehavior
    {
        [SerializeField] protected List<AttackType> attacksSequence = new List<AttackType>
        {
            AttackType.Lightning,
            AttackType.Charge,
            AttackType.Jump,
        };

        [Space]
        [SerializeField] protected Float idleDuration = 1.5f;

        [Header("Lightning Attack")]
        [SerializeField] protected GameObject lightningAttackProjectilePrefab;
        [SerializeField] protected int lightningAttacksCount = 3;
        [SerializeField] protected Float lightningAttackDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float lightningAttackProjectileSpawnHeight = 1f;
        [SerializeField] protected Int lightningAttackProjectileBounceCount = 0;

        [Space]
        [SerializeField] protected ParticleSystem lightningAttackParticle;
        
        [Space]
        [SerializeField] protected Transform lightningAttackWarningLinesParent;
        [SerializeField] protected List<WaspChargeLineBehavior> lightningAttackWarningLines;

        [Space]
        [SerializeField] protected AudioData lightningAttackSound;

        [Header("Charge Attack")]
        [SerializeField] protected GameObject chargeAttackProjectilePrefab;
        [SerializeField] protected Transform chargeAttackParticleSpawnPosition;
        [SerializeField] protected Float chargeAttackColliderRadius = 3f;
        [SerializeField] protected Float chargeAttackDuration = 10f;
        [SerializeField] protected Float chargeProjectileSpawnInterval = 1f;
        [SerializeField] protected Float chargeAttackProjectileDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AnimationCurve chargeAttackAccelerationAnimationCurve;

        [Space]
        [SerializeField] protected ParticleSystem chargeAttackParticle;
        [FormerlySerializedAs("chargeAttackParticles")]
        [SerializeField] protected List<ParticleSystem> chargeAttackHandsParticles;

        [SerializeField] protected AudioData chargeAttackSpinSound;
        [SerializeField] protected float chargeAttackSpinFadeInDuration;
        [SerializeField] protected float chargeAttackSpinFadeOutDuration;

        [Header("Jump Attack")]
        [SerializeField] protected GameObject jumpAttackProjectilePrefab;
        [SerializeField] protected Float jumpAttackDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float jumpAttackDuration = 5;
        [SerializeField] protected Float jumpAttackProjectileSpawnInterval = 0.9f;

        [Space]
        [SerializeField] protected Transform jumpAttackProjectileSpawnPositionsParent;

        [Space]
        [SerializeField] protected ParticleSystem flyAwayParticle;
        [SerializeField] protected ParticleSystem landParticle;

        [Space]
        [SerializeField] protected AudioData jumpAttackFlyingSound;
        [SerializeField] protected AudioData jumpAttackLandingSound;
        [SerializeField] protected AudioData jumpAttackLoopedSound;
        [SerializeField] protected float jumpAttackLoopedSoundFadeInDuration;
        [SerializeField] protected float jumpAttackLoopedSoundFadeOutDuration;

        protected List<AbstractProjectile> launchedLightningProjectiles = new List<AbstractProjectile>();
        protected List<AbstractProjectile> launchedChargeProjectiles = new List<AbstractProjectile>();
        protected List<AbstractProjectile> launchedJumpProjectiles = new List<AbstractProjectile>();

        public AttackType CurrentAttackType { get; protected set; }

        protected int lightningAttackCounter = 0;

        protected Vector3 spawnPosition;

        protected CapsuleCollider golemCollider;
        protected float golemColliderRadius;

        protected float LightningDamage => Damage * lightningAttackDamageMultiplier;
        protected float ChargeProjectileDamage => Damage * chargeAttackProjectileDamageMultiplier;
        protected float JumpDamage => Damage * jumpAttackDamageMultiplier;

        protected AudioSource chargeAttackSpinSource;
        protected AudioSource jumpAttackSource;

        protected IEasingCoroutine chargeAttackSpinEasingCoroutine;
        protected IEasingCoroutine jumpAttackEasingCoroutine;

        protected override void Awake()
        {
            base.Awake();

            StageController.ProjectilesManager.RegisterProjectile(lightningAttackProjectilePrefab);
            StageController.ProjectilesManager.RegisterProjectile(chargeAttackProjectilePrefab);
            StageController.ProjectilesManager.RegisterProjectile(jumpAttackProjectilePrefab);

            golemCollider = enemyCollider as CapsuleCollider;
            golemColliderRadius = golemCollider.radius;
        }

        protected override void Start()
        {
            base.Start();

            spawnPosition = transform.position;

            navigationHandler.SetRotationAllowed(false);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            enemyCollider.enabled = false;

            waitForSpawnToEnd.Reset();
            yield return waitForSpawnToEnd;

            enemyCollider.enabled = true;

            int i = 0;
            while (IsAlive)
            {
                yield return new WaitForSeconds(idleDuration);

                switch(attacksSequence[i % attacksSequence.Count])
                {
                    case AttackType.Lightning:
                        yield return LightningAttack();
                        break;
                    case AttackType.Charge:
                        yield return ChargeAttack();
                        break;
                    case AttackType.Jump:
                        yield return JumpAttack();
                        break;
                }

                i++;
            }
        }

        protected virtual IEnumerator LightningAttack()
        {
            CurrentAttackType = AttackType.Lightning;
            animator.SetInteger(ATTACK_TYPE_INT, (int)CurrentAttackType);
            animator.SetTrigger(ATTACK_TRIGGER);

            LookAtPlayer = true;

            if (Target == null)
            {
                lightningAttackWarningLinesParent.forward = Vector3.forward;
            }
            else
            {
                lightningAttackWarningLinesParent.forward = lightningAttackWarningLinesParent.DirectionToXZ(Target.Position);
            }

            for (int i = 0; i < lightningAttackWarningLines.Count; i++)
            {
                lightningAttackWarningLines[i].Show(30, 0.2f);
            }

            lightningAttackCounter = 0;
            
            while (lightningAttackCounter < lightningAttacksCount)
            {
                yield return null;

                if(Target != null)
                {
                    lightningAttackWarningLinesParent.forward = lightningAttackWarningLinesParent.DirectionToXZ(Target.Position);
                }
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
            waitForAttackToEnd.Reset();

            yield return waitForAttackToEnd;
        }

        protected virtual IEnumerator ChargeAttack()
        {
            CurrentAttackType = AttackType.Charge;
            animator.SetInteger(ATTACK_TYPE_INT, (int)CurrentAttackType);
            animator.SetTrigger(ATTACK_TRIGGER);

            for(int i = 0; i < chargeAttackHandsParticles.Count; i++)
            {
                chargeAttackHandsParticles[i].Play();
            }

            TryFindTarget();
            var direction = Target != null ? transform.DirectionToXZ(Target.Position) : Vector3.back;

            SetMovementSpeedMultiplier(0);
            EasingManager.DoFloat(0, 1, 1.5f, SetMovementSpeedMultiplier).SetEasingCurve(chargeAttackAccelerationAnimationCurve);

            chargeAttackSpinSource = GameController.AudioManager.PlayAudio(chargeAttackSpinSound);

            if(chargeAttackSpinSource != null)
            {
                chargeAttackSpinSource.loop = true;
                var volume = chargeAttackSpinSource.volume;
                chargeAttackSpinSource.volume = 0;
                chargeAttackSpinEasingCoroutine = chargeAttackSpinSource.DoVolume(volume, chargeAttackSpinFadeInDuration);
            }

            var startTime = Time.time;
            var projectileSpawnTime = startTime + chargeProjectileSpawnInterval;
            while (Time.time < startTime + chargeAttackDuration)
            {
                var farAwayPoint = transform.position + direction * 1000;
                StageController.NavigationManager.IsStraightPathAvailable(transform.position, farAwayPoint, out var hitPoint, out var hitNormal);

                navigationHandler.Move(hitPoint - direction);

                while(!navigationHandler.HasReachedDestination())
                {
                    if(animator.speed > 0.1f)
                    {
                        if (Time.time >= projectileSpawnTime)
                        {
                            var projectile = StageController.ProjectilesManager.GetProjectile(chargeAttackProjectilePrefab);
                            projectile.transform.position = chargeAttackParticleSpawnPosition.position;
                            projectile.transform.forward = Random.insideUnitSphere.SetY(0).normalized;

                            projectile.Launch(ChargeProjectileDamage);

                            projectile.onProjectileHidden += OnChargeProjectileHidden;
                            launchedChargeProjectiles.Add(projectile);

                            projectileSpawnTime += chargeProjectileSpawnInterval;
                        }
                    } else
                    {
                        projectileSpawnTime += Time.deltaTime;
                        startTime += Time.deltaTime;
                    }

                    yield return null;
                }

                direction = Vector3.Reflect(direction, hitNormal).normalized;
            }

            navigationHandler.Move(spawnPosition);

            yield return navigationHandler.WaitUntilReachedDestination();

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);

            if(chargeAttackSpinSource != null)
            {
                chargeAttackSpinEasingCoroutine = chargeAttackSpinSource.DoVolume(0, chargeAttackSpinFadeOutDuration).SetOnFinish(() =>
                {
                    chargeAttackSpinSource.loop = false;
                    chargeAttackSpinSource.Stop();
                    chargeAttackSpinSource = null;
                });
            }

            golemCollider.radius = golemColliderRadius; // Reset the collider radius after the charge attack

            for (int i = 0; i < chargeAttackHandsParticles.Count; i++)
            {
                chargeAttackHandsParticles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (chargeAttackParticle != null) chargeAttackParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        protected virtual void OnChargeProjectileHidden(AbstractProjectile projectile)
        {
            launchedChargeProjectiles.Remove(projectile);
            projectile.onProjectileHidden -= OnChargeProjectileHidden;
        }

        protected virtual void SetMovementSpeedMultiplier(float multiplier)
        {
            navigationHandler.SetMovementSpeedMultiplier(multiplier);
        }

        protected bool jumpAttackEnded = false;

        protected virtual IEnumerator JumpAttack()
        {
            CurrentAttackType = AttackType.Jump;
            animator.SetInteger(ATTACK_TYPE_INT, (int)CurrentAttackType);
            animator.SetTrigger(ATTACK_TRIGGER);

            jumpAttackEnded = false;

            enemyCollider.enabled = false;

            yield return new WaitForSeconds(jumpAttackDuration);

            jumpAttackEnded = true;

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);

            yield return waitForAttackToEnd;

            enemyCollider.enabled = true;
        }

        public override void OnSpawnEndedEventFired()
        {
            base.OnSpawnEndedEventFired();

            waitForSpawnToEnd.Complete();
        }

        public override void OnAttackEndedEventFired()
        {
            base.OnAttackEndedEventFired();

            waitForAttackToEnd.Complete();
        }

        public override void OnAttackEventFired()
        {
            base.OnAttackEventFired();

            switch(CurrentAttackType)
            {
                case AttackType.Lightning:
                    OnLightningAttackEventFired();
                    break;
                case AttackType.Charge:
                    golemCollider.radius = chargeAttackColliderRadius;
                    if (chargeAttackParticle != null) chargeAttackParticle.Play();
                    break;
                case AttackType.Jump:
                    StartCoroutine(SpawnJumpAttackProjectiles());
                    break;
            }
        }

        protected virtual void OnLightningAttackEventFired()
        {
            if(lightningAttackCounter == 0)
            {
                LookAtPlayer = false;

                for (int i = 0; i < lightningAttackWarningLines.Count; i++)
                {
                    var line = lightningAttackWarningLines[i];
                    line.Hide();
                }
            }

            for(int i = 0; i < lightningAttackWarningLines.Count; i++)
            {
                var line = lightningAttackWarningLines[i];

                var projectile = StageController.ProjectilesManager.GetProjectile(lightningAttackProjectilePrefab);

                projectile.transform.position = line.transform.position;
                projectile.transform.forward = line.transform.forward;

                projectile.SetBounceData(lightningAttackProjectileBounceCount, 1f);

                projectile.onProjectileHidden += OnLightningProjectileHidden;
                launchedLightningProjectiles.Add(projectile);

                projectile.Launch(LightningDamage);
            }

            if(lightningAttackParticle != null)
            {
                lightningAttackParticle.Play();
            }

            GameController.AudioManager.PlayAudio(lightningAttackSound);

            lightningAttackCounter++;
        }

        protected virtual void OnLightningProjectileHidden(AbstractProjectile projectile)
        {
            launchedLightningProjectiles.Remove(projectile);
            projectile.onProjectileHidden -= OnLightningProjectileHidden;
        }

        protected virtual IEnumerator SpawnJumpAttackProjectiles()
        {
            transform.forward = Vector3.back; // Ensure the Golem is facing the correct direction for the jump attack

            int counter = 0;
            while (!jumpAttackEnded)
            {
                var spawnPosition = jumpAttackProjectileSpawnPositionsParent.GetChild(counter).position;

                var projectile = StageController.ProjectilesManager.GetProjectile(jumpAttackProjectilePrefab);

                projectile.transform.position = spawnPosition;
                projectile.transform.forward = Vector3.back;

                projectile.IgnoresObstacles = true; // Ensure the projectile ignores obstacles during the jump attack
                projectile.IsPiercingEnabled = true; // Allow the projectile to pierce through enemies
                projectile.Launch(JumpDamage);

                projectile.onProjectileHidden += OnJumpProjectileHidden;
                launchedJumpProjectiles.Add(projectile);

                counter++;
                if(counter >= jumpAttackProjectileSpawnPositionsParent.childCount)
                {
                    counter = 0;
                }

                yield return new WaitForSeconds(jumpAttackProjectileSpawnInterval);
            }
            
        }

        protected virtual void OnJumpProjectileHidden(AbstractProjectile projectile)
        {
            launchedJumpProjectiles.Remove(projectile);
            projectile.onProjectileHidden -= OnJumpProjectileHidden;
        }

        public virtual void OnFlyAwayEventFired()
        {
            if(flyAwayParticle != null) flyAwayParticle.Play();
            GameController.AudioManager.PlayAudio(jumpAttackFlyingSound);

            jumpAttackSource = GameController.AudioManager.PlayAudio(jumpAttackLoopedSound);
            if(jumpAttackSource != null)
            {
                jumpAttackSource.loop = true;
                var volume = jumpAttackSource.volume;
                jumpAttackSource.volume = 0;
                jumpAttackEasingCoroutine = jumpAttackSource.DoVolume(volume, jumpAttackLoopedSoundFadeInDuration);
            }
        }

        public virtual void OnLandEventFired()
        {
            if(landParticle != null) landParticle.Play();
            GameController.AudioManager.PlayAudio(jumpAttackLandingSound);

            if(jumpAttackSource != null)
            {
                jumpAttackEasingCoroutine = jumpAttackSource.DoVolume(0, jumpAttackLoopedSoundFadeOutDuration).SetOnFinish(() => {
                    jumpAttackSource.loop = false;
                    jumpAttackSource.Stop();
                    jumpAttackSource = null;
                });
            }
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();

            for(int i = 0; i < launchedLightningProjectiles.Count; i++)
            {
                launchedLightningProjectiles[i].onProjectileHidden -= OnLightningProjectileHidden;
                launchedLightningProjectiles[i].Clear();
            }
            launchedLightningProjectiles.Clear();

            for (int i = 0; i < launchedChargeProjectiles.Count; i++)
            {
                launchedChargeProjectiles[i].onProjectileHidden -= OnChargeProjectileHidden;
                launchedChargeProjectiles[i].Clear();
            }
            launchedChargeProjectiles.Clear();

            for (int i = 0; i < launchedJumpProjectiles.Count; i++)
            {
                launchedJumpProjectiles[i].onProjectileHidden -= OnJumpProjectileHidden;
                launchedJumpProjectiles[i].Clear();
            }
            launchedJumpProjectiles.Clear();

            if(chargeAttackSpinSource != null)
            {
                chargeAttackSpinSource.loop = false;
                chargeAttackSpinSource.Stop();
                chargeAttackSpinSource = null;

                chargeAttackSpinEasingCoroutine.StopIfExists();
            }

            if (jumpAttackSource != null)
            {
                jumpAttackSource.loop = false;
                jumpAttackSource.Stop();
                jumpAttackSource = null;

                jumpAttackEasingCoroutine.StopIfExists();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            StageController.onDefeat += OnPlayerDefeated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StageController.onDefeat -= OnPlayerDefeated;
        }

        public override bool ApplyStatusEffect(StatusEffectType type, float multiplier)
        {
            var applied = base.ApplyStatusEffect(type, multiplier);

            if (applied && type == StatusEffectType.Freeze)
            {
                if (chargeAttackSpinSource != null)
                {
                    chargeAttackSpinSource.Pause();
                }
            }

            return applied;
        }

        public override void RemoveStatusEffect(StatusEffectType type)
        {
            base.RemoveStatusEffect(type);

            if (type == StatusEffectType.Freeze)
            {
                if (chargeAttackSpinSource != null)
                {
                    chargeAttackSpinSource.Play();
                }
            }
        }

        protected virtual void OnPlayerDefeated()
        {
            if (chargeAttackSpinSource != null)
            {
                chargeAttackSpinSource.loop = false;
                chargeAttackSpinSource.Stop();
                chargeAttackSpinSource = null;

                chargeAttackSpinEasingCoroutine.StopIfExists();
            }

            if (jumpAttackSource != null)
            {
                jumpAttackSource.loop = false;
                jumpAttackSource.Stop();
                jumpAttackSource = null;

                jumpAttackEasingCoroutine.StopIfExists();
            }
        }

        public enum AttackType
        {
            Lightning,
            Charge,
            Jump
        }
    }
}