using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Projectile;
using OctoberStudio.StatusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OctoberStudio.Enemy
{
    public class MaskBossBehavior : EnemyBehavior
    {
        [SerializeField] protected List<MaskAttackType> attacksSequence = new List<MaskAttackType>
        {
            MaskAttackType.Projectiles,
            MaskAttackType.Waves,
            MaskAttackType.Hands,
        };

        [SerializeField] protected float idleDuration = 2f;
        [SerializeField, Min(0)] protected float maxAngleDeviation = 20f;

        [Header("Projectiles Attack")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform projectileSpawnPosition;
        [SerializeField] protected List<Transform> projectileLaunchPositions;
        [SerializeField] protected Int projectilesAttackCount = 3;
        [SerializeField] protected Float projectileAttackDelay = 0.3f;
        [SerializeField] protected Float projectileSpawnDuration = 0.2f;
        [SerializeField] protected Float projectileDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData projectilesSpawnedSound;
        [SerializeField] protected AudioData projectilesLaunchedSound;

        [Header("Hands Attack")]
        [SerializeField] protected float handsAttacksCount = 3;
        [SerializeField] protected List<Transform> handsSpawnPositions;

        [Space]
        [SerializeField] protected AudioData handsAttackSound;

        [Header("Waves Attack")]
        [SerializeField] protected GameObject wavePrefab;
        [SerializeField] protected Transform waveSpawnPosition;
        [SerializeField] protected Int wavesCount = 5;
        [SerializeField] protected Int wavesAttackCount = 3;
        [SerializeField] protected Float waveDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData wavesAttackSound;

        protected List<SimpleProjectile> spawnedProjectiles = new List<SimpleProjectile>();
        protected List<SimpleProjectile> launchedProjectiles = new List<SimpleProjectile>();
        protected List<IEasingCoroutine> projectilesSpawnEasingCoroutines = new List<IEasingCoroutine>();

        protected List<SimpleProjectile> launchedWaves = new List<SimpleProjectile>();

        protected List<HandEnemyBehavior> spawnedHands = new List<HandEnemyBehavior>();

        public MaskAttackType CurrentAttackType { get; protected set; }

        protected int handsCounter = 0;

        public float WaveDamage => Damage * waveDamageMultiplier;
        public float ProjectileDamage => Damage * projectileDamageMultiplier;

        protected AudioSource handsAttackAudioSource;

        protected override void Awake()
        {
            base.Awake();

            StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
            StageController.ProjectilesManager.RegisterProjectile(wavePrefab);
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
                LookAtPlayer = true;
                yield return new WaitForSeconds(idleDuration);

                if (attacksSequence[i % attacksSequence.Count] == MaskAttackType.Hands && spawnedHands.Count > 0) i++;

                switch (attacksSequence[i % attacksSequence.Count])
                {
                    case MaskAttackType.Projectiles:
                        yield return ProjectilesAttackCoroutine();
                        break;
                    case MaskAttackType.Hands:
                        yield return HandsAttackCoroutine();
                        break;
                    case MaskAttackType.Waves:
                        yield return WavesAttackCoroutine();
                        break;
                }

                i++;
            }
        }

        protected virtual IEnumerator ProjectilesAttackCoroutine()
        {
            LookAtPlayer = true;

            CurrentAttackType = MaskAttackType.Projectiles;

            for(int i = 0; i < projectilesAttackCount; i++)
            {
                animator.SetInteger(ATTACK_TYPE_INT, (int)CurrentAttackType);
                animator.SetTrigger(ATTACK_TRIGGER);
                waitForAttackToEnd.Reset();

                yield return waitForAttackToEnd;

                TryFindTarget();

                for (int j = 0; j < spawnedProjectiles.Count; j++)
                {
                    var projectile = spawnedProjectiles[j];
                    if (Target != null)
                    {
                        var direction = projectile.transform.position.DirectionTo(Target.Position).normalized;
                        var directionXZ = direction.X0Z().normalized;

                        projectile.transform.forward = projectile.transform.position.DirectionTo(Target.Position + directionXZ * 0.5f).normalized;
                    }
                    else
                    {
                        projectile.transform.forward = Vector3.back;
                    }

                    projectile.Target = Target?.Transform;
                    projectile.Launch(ProjectileDamage);

                    GameController.AudioManager.PlayAudio(projectilesLaunchedSound);

                    launchedProjectiles.Add(projectile);
                    projectile.onProjectileHidden += OnProjectileHidden;
                    spawnedProjectiles.RemoveAt(j);
                    j--; // Adjust index since we removed an item from the list

                    if (j != spawnedProjectiles.Count - 1)
                    {
                        yield return new WaitForSeconds(projectileAttackDelay);

                        if(animator.speed < 0.1f)
                        {
                            yield return new WaitUntil(() => animator.speed > 0.1f);
                        }
                    }
                }
            }
        }

        protected virtual void OnProjectileHidden(AbstractProjectile projectile)
        {
            launchedProjectiles.Remove(projectile as SimpleProjectile);
            projectile.onProjectileHidden -= OnProjectileHidden;
        }

        protected virtual IEnumerator HandsAttackCoroutine()
        {
            LookAtPlayer = false;

            CurrentAttackType = MaskAttackType.Hands;

            for(int i = 0; i < handsAttacksCount; i++)
            {
                animator.SetInteger(ATTACK_TYPE_INT, (int)CurrentAttackType);
                animator.SetTrigger(ATTACK_TRIGGER);
                waitForAttackToEnd.Reset();

                handsCounter = 0;

                GameController.AudioManager.PlayAudio(handsAttackSound);

                yield return waitForAttackToEnd;

                handsAttackAudioSource = null;
            }
        }

        protected virtual IEnumerator WavesAttackCoroutine()
        {
            LookAtPlayer = false;

            CurrentAttackType = MaskAttackType.Waves;

            for (int i = 0; i < wavesAttackCount; i++)
            {
                animator.SetInteger(ATTACK_TYPE_INT, (int)CurrentAttackType);
                animator.SetTrigger(ATTACK_TRIGGER);
                waitForAttackToEnd.Reset();

                yield return waitForAttackToEnd;
            }
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

            if (!IsAlive) return;

            switch (CurrentAttackType)
            {
                case MaskAttackType.Projectiles:
                    SpawnProjectiles();
                    break;

                case MaskAttackType.Hands:
                    SpawnHand();
                    break;

                case MaskAttackType.Waves:
                    SpawnWaves();
                    break;
            }
        }

        protected virtual void SpawnHand()
        {
            var handSpawnPosition = handsSpawnPositions[handsCounter].position;

            var hand = StageController.Room.SpawnAdditionalEnemy(EnemyType.Hand, handSpawnPosition, transform.rotation);
            hand.SubscribeOnDefeat(OnHandDefeated);

            spawnedHands.Add(hand as HandEnemyBehavior);

            handsCounter++;
        }

        protected virtual void OnHandDefeated(IDefeatable hand)
        {
            hand.UnsubscribeOnDefeat(OnHandDefeated);

            spawnedHands.Remove(hand as HandEnemyBehavior);
        }

        protected virtual void SpawnProjectiles() {
        
            for(int i = 0; i < projectileLaunchPositions.Count; i++)
            {
                var projectile = StageController.ProjectilesManager.GetProjectile<SimpleProjectile>(projectilePrefab);
                projectile.transform.position = projectileSpawnPosition.position;
                spawnedProjectiles.Add(projectile);

                var spawnPoint = projectileLaunchPositions[i];

                IEasingCoroutine easingCoroutine = null;
                easingCoroutine = projectile.transform.DoPosition(spawnPoint.position, projectileSpawnDuration).SetEasing(EasingType.SineOut).SetOnFinish(() => OnProjectileSpawnEnded(easingCoroutine));
                projectilesSpawnEasingCoroutines.Add(easingCoroutine);
            }

            GameController.AudioManager.PlayAudio(projectilesSpawnedSound);
        }

        protected virtual void OnProjectileSpawnEnded(IEasingCoroutine coroutine)
        {
            projectilesSpawnEasingCoroutines.Remove(coroutine);
        }

        protected virtual void SpawnWaves()
        {
            var angleStep = 180f / (wavesCount - 1);
            var startAngle = -90f;

            TryFindTarget();

            var toTarget = Target != null ? waveSpawnPosition.DirectionToXZ(Target.Position): Vector3.back;
            for (int i = 0; i < wavesCount; i++)
            {
                var wave = StageController.ProjectilesManager.GetProjectile<SimpleProjectile>(wavePrefab);
                wave.transform.position = waveSpawnPosition.position;
                wave.transform.forward = Quaternion.Euler(0f, startAngle + angleStep * i, 0f) * toTarget;

                wave.onProjectileHidden += OnWaveHidden;
                launchedWaves.Add(wave);

                wave.Launch(WaveDamage);
            }

            GameController.AudioManager.PlayAudio(wavesAttackSound);
        }

        protected virtual void OnWaveHidden(AbstractProjectile wave)
        {
            launchedWaves.Remove(wave as SimpleProjectile);
            wave.onProjectileHidden -= OnWaveHidden;
        }

        protected override void LookAtPlayerUpdate()
        {
            if (LookAtPlayer)
            {
                if (TargetDetector == null)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((StageController.Player.Position - Position).NormalizeXZ()), Time.deltaTime * 10 * LookAtPlayerMultiplier);
                    
                }
                else if (TargetDetector.HasDetectedPlayers)
                {
                    var closestPlayer = TargetDetector.GetClosestPlayer();
                    if (closestPlayer != null)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((closestPlayer.Position - Position).NormalizeXZ()), Time.deltaTime * 10 * LookAtPlayerMultiplier);
                    }
                }
            } else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.back), Time.deltaTime * 10 * LookAtPlayerMultiplier);
            }

            if(transform.eulerAngles.y > 0)
            {
                if (transform.eulerAngles.y < -maxAngleDeviation + 180)
                {
                    transform.eulerAngles = transform.eulerAngles.SetY(-maxAngleDeviation + 180);
                }
                else if (transform.eulerAngles.y > maxAngleDeviation + 180)
                {
                    transform.eulerAngles = transform.eulerAngles.SetY(maxAngleDeviation + 180);
                }
            } else if(transform.eulerAngles.y < 0)
            {
                if (transform.eulerAngles.y < -maxAngleDeviation - 180)
                {
                    transform.eulerAngles = transform.eulerAngles.SetY(-maxAngleDeviation - 180);
                }
                else if (transform.eulerAngles.y > maxAngleDeviation - 180)
                {
                    transform.eulerAngles = transform.eulerAngles.SetY(maxAngleDeviation - 180);
                }
            }
        }

        protected override void Defeat()
        {
            base.Defeat();

            for(int i = spawnedHands.Count - 1; i >= 0; i--)
            {
                var hand = spawnedHands[i];

                spawnedHands.RemoveAt(i);
                hand.UnsubscribeOnDefeat(OnHandDefeated);

                hand.Kill();
            }

            StopCoroutine(behaviorCoroutine);

            for(int i = 0; i < spawnedProjectiles.Count; i++)
            {
                spawnedProjectiles[i].Clear();
            }
            spawnedProjectiles.Clear();

            for (int i = 0; i < projectilesSpawnEasingCoroutines.Count; i++)
            {
                projectilesSpawnEasingCoroutines[i].StopIfExists();
            }
            projectilesSpawnEasingCoroutines.Clear();

            for(int i = 0; i < launchedProjectiles.Count; i++)
            {
                launchedProjectiles[i].onProjectileHidden -= OnProjectileHidden;
                launchedProjectiles[i].Clear();
            }
            launchedProjectiles.Clear();

            for (int i = 0; i < launchedWaves.Count; i++)
            {
                launchedWaves[i].onProjectileHidden -= OnWaveHidden;
                launchedWaves[i].Clear();
            }
            launchedWaves.Clear();

            if(handsAttackAudioSource != null && handsAttackAudioSource.gameObject.activeSelf)
            {
                var data = GameController.AudioManager.GetAudioData(handsAttackAudioSource);
                if(data == handsAttackSound)
                {
                    handsAttackAudioSource.Stop();
                    handsAttackAudioSource = null;
                }
            }
        }

        public override bool ApplyStatusEffect(StatusEffectType type, float multiplier)
        {
            var applied = base.ApplyStatusEffect(type, multiplier);

            if (applied && type == StatusEffectType.Freeze)
            {
                if (handsAttackAudioSource != null && handsAttackAudioSource.gameObject.activeSelf)
                {
                    var data = GameController.AudioManager.GetAudioData(handsAttackAudioSource);
                    if (data == handsAttackSound)
                    {
                        handsAttackAudioSource.Pause();
                    }
                }
            }

            return applied;
        }

        public override void RemoveStatusEffect(StatusEffectType type)
        {
            base.RemoveStatusEffect(type);

            if (type == StatusEffectType.Freeze)
            {
                if (handsAttackAudioSource != null && handsAttackAudioSource.gameObject.activeSelf)
                {
                    var data = GameController.AudioManager.GetAudioData(handsAttackAudioSource);
                    if (data == handsAttackSound)
                    {
                        handsAttackAudioSource.Play();
                    }
                }
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

        protected virtual void OnPlayerDefeated()
        {
            if (handsAttackAudioSource != null && handsAttackAudioSource.gameObject.activeSelf)
            {
                var data = GameController.AudioManager.GetAudioData(handsAttackAudioSource);
                if (data == handsAttackSound)
                {
                    handsAttackAudioSource.Stop();
                    handsAttackAudioSource = null;
                }
            }
        }

        public enum MaskAttackType
        {
            Projectiles = 0,
            Hands = 1,
            Waves = 2
        }
    }
}