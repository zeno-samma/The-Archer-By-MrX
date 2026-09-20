using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Projectile;
using OctoberStudio.StatusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class TotemBossBehavior : EnemyBehavior
    {
        protected static readonly int ATTACK_1_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Attack Speed Multipler");

        [SerializeField]
        protected List<TotemAttackType> attacksSequence = new List<TotemAttackType>
        {
            TotemAttackType.Laser,
            TotemAttackType.Fist,
            TotemAttackType.Tornado,
        };

        [SerializeField] protected ParticleSystem spawnParticle;
        [SerializeField] protected Float idleDuration = 2f;

        [Header("Lasers Attack")]
        [SerializeField] protected GameObject totemLaserPrefab;
        [Tooltip("Lasers would originate in this positions and point in the direction of their 'forward' axis")]
        [SerializeField] protected List<Transform> laserSpawnPositions;

        [Space]
        [SerializeField] protected AnimationCurve laserAttackRotationStartCurve;
        [SerializeField] protected AnimationCurve laserAttackRotationEndCurve;
        [SerializeField] protected Float laserAttackRotationStartDuration;
        [SerializeField] protected Float laserAttackRotationEndDuration;

        [Space]
        [Tooltip("How many times will the human totem head rotate during attack")]
        [SerializeField] protected int laserAttackRotationsCount = 2;
        [Tooltip("Controlls the speed of human head rotation for fine tuning lasers rotational speed")]
        [SerializeField] protected Float laserAttackRotationSpeedMultiplier = 1f;
        [SerializeField] protected Float laserAttackDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData laserActivationSound;

        [Space]
        [SerializeField] protected AudioData laserWorkingSound;
        [SerializeField] protected float laserWorkingSoundFadeInDuration;
        [SerializeField] protected float laserWorkingSoundFadeOutDuration;

        [Space]
        [SerializeField] protected AudioData laserTotemTurningSound;
        [SerializeField] protected float laserTotemTurningSoundFadeInDuration;
        [SerializeField] protected float laserTotemTurningSoundFadeOutDuration;

        [Header("Tornado Attack")]
        [SerializeField] protected GameObject tornadoPrefab;
        [SerializeField] protected List<GameObject> tornadoWingsTrails;

        [Space]
        [Tooltip("How many times will the bird totem head rotate during attack")]
        [SerializeField] protected int tornadoAttackRotationsCount = 2;
        [Tooltip("How many tornadoes are spawned at the same time in burst")]
        [SerializeField] protected int tornadoProjectilesCountInBurst = 6;
        [Tooltip("Burst will happen every 'delayBetweenTornadoBursts' seconds untill the head stops rotating")]
        [SerializeField] protected Float delayBetweenTornadoBursts = 1f;
        [Tooltip("Tornados will spawn this distance away from the totem")]
        [SerializeField] protected Float tornadoSpawnRadius = 1f;
        [SerializeField] protected Float tornadoAttackDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData tornadoTotemTurningSound;
        [SerializeField] protected float tornadoTotemTurningSoundFadeInDuration;
        [SerializeField] protected float tornadoTotemTurningSoundFadeOutDuration;

        [Header("Fist Attack")]
        [SerializeField] protected GameObject fistPrefab;
        [SerializeField] protected ParticleSystem monkeyHeadSlamParticle;

        [Space]
        [Tooltip("How many times will the monkey totem head jumps during attack")]
        [SerializeField] protected int fistAttackJumpsCount = 5;
        [Tooltip("Fist will spawn every 'delayBetweenTornadoBursts' seconds untill the head stops jumping")]
        [SerializeField] protected Float fistSpawnDelay = 0.5f;
        [SerializeField] protected Float fistAttackDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData monkeyJumpingSound;

        protected List<AbstractProjectile> lasers = new List<AbstractProjectile>();
        protected List<AbstractProjectile> tornados = new List<AbstractProjectile>();
        protected List<AbstractProjectile> fists = new List<AbstractProjectile>();

        public float LaserAttackDamage => Damage * laserAttackDamageMultiplier;
        public float TornadoAttackDamage => Damage * tornadoAttackDamageMultiplier;
        public float FistAttackDamage => Damage * fistAttackDamageMultiplier;

        protected IEasingCoroutine rotationCoroutine;

        protected AudioSource laserWorkingAudioSource;
        protected AudioSource laserTotemTurningAudioSource;

        protected AudioSource tornadoTotemTurningAudioSource;

        protected IEasingCoroutine laserWorkingAudioEasingCoroutine;
        protected IEasingCoroutine laserTotemTurningAudioEasingCoroutine;

        protected IEasingCoroutine tornadoTotemTurningAudioEasingCoroutine;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(tornadoPrefab);
            StageController.ProjectilesManager.RegisterProjectile(totemLaserPrefab);
            StageController.ProjectilesManager.RegisterProjectile(fistPrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            LookAtPlayer = false;

            // Disabling collider to prevent the player from hitting the totem while it is spawning
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
                    case TotemAttackType.Laser:
                        yield return LaserAttackCoroutine();
                        break;
                    case TotemAttackType.Tornado:
                        yield return TornadoAttackCoroutine();
                        break;
                    case TotemAttackType.Fist:
                        yield return FistAttackCoroutine();
                        break;
                }

                i++;
            }
        }

        #region Animation Events

        public override void OnSpawnEndedEventFired()
        {
            waitForSpawnToEnd.Complete();
        }

        public override void OnAttackEndedEventFired()
        {
            waitForAttackToEnd.Complete();
        }

        public override void OnAttackEventFired()
        {
            var attackType = (TotemAttackType)animator.GetInteger(ATTACK_TYPE_INT);

            switch (attackType)
            {
                case TotemAttackType.Laser:
                    SpawnLasers();
                    break;

                case TotemAttackType.Tornado:
                    StartCoroutine(SpawnTornado());
                    break;

                case TotemAttackType.Fist:
                    SpawnFists();
                    break;
            }
        }

        #endregion

        #region Attacks

        protected virtual IEnumerator LaserAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)TotemAttackType.Laser);
            animator.SetTrigger(ATTACK_TRIGGER);

            for(int i = 0; i < laserAttackRotationsCount; i++)
            {
                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;
            }

            if(laserWorkingAudioSource != null)
            {
                laserWorkingAudioEasingCoroutine = laserWorkingAudioSource.DoVolume(0, laserWorkingSoundFadeOutDuration).SetOnFinish(() =>
                {
                    laserWorkingAudioSource.loop = false;
                    laserWorkingAudioSource.Stop();
                    laserWorkingAudioSource = null;
                });
            }

            if (laserTotemTurningAudioSource != null)
            {
                laserTotemTurningAudioEasingCoroutine = laserTotemTurningAudioSource.DoVolume(0, laserTotemTurningSoundFadeOutDuration).SetOnFinish(() => 
                {
                    laserTotemTurningAudioSource.loop = false;
                    laserTotemTurningAudioSource.Stop();
                    laserTotemTurningAudioSource = null;
                });
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);

            for(int i = 0; i < lasers.Count; i++)
            {
                lasers[i].Hide();
            }

            lasers.Clear();
        }

        protected virtual IEnumerator TornadoAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)TotemAttackType.Tornado);
            animator.SetTrigger(ATTACK_TRIGGER);

            for(int i = 0; i < tornadoWingsTrails.Count; i++)
            {
                tornadoWingsTrails[i].SetActive(true);
            }

            for (int i = 0; i < tornadoAttackRotationsCount; i++)
            {
                waitForAttackToEnd.Reset();

                yield return waitForAttackToEnd;
            }

            if (tornadoTotemTurningAudioSource != null)
            {
                tornadoTotemTurningAudioEasingCoroutine = tornadoTotemTurningAudioSource.DoVolume(0, tornadoTotemTurningSoundFadeInDuration).SetOnFinish(() =>
                {
                    tornadoTotemTurningAudioSource.loop = false;
                    tornadoTotemTurningAudioSource.Stop();
                    tornadoTotemTurningAudioSource = null;
                });
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);

            for (int i = 0; i < tornadoWingsTrails.Count; i++)
            {
                tornadoWingsTrails[i].SetActive(false);
            }

            rotationCoroutine.StopIfExists();
            rotationCoroutine = EasingManager.DoFloat(laserAttackRotationSpeedMultiplier, 0, laserAttackRotationEndDuration, SetLaserAttackRotationSpeed).SetEasingCurve(laserAttackRotationEndCurve);
        }

        protected virtual IEnumerator FistAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)TotemAttackType.Fist);
            animator.SetTrigger(ATTACK_TRIGGER);

            for (int i = 0; i < fistAttackJumpsCount; i++)
            {
                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;

                yield return new WaitForSeconds(fistSpawnDelay);

                animator.SetTrigger(ATTACK_TRIGGER);
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        #endregion

        #region Projectile Spawns

        protected virtual void SpawnLasers()
        {
            animator.SetFloat(ATTACK_1_SPEED_MULTIPLIER_FLOAT, 0);

            GameController.AudioManager.PlayAudio(laserActivationSound);

            laserWorkingAudioSource = GameController.AudioManager.PlayAudio(laserWorkingSound);

            if(laserWorkingAudioSource != null)
            {
                laserWorkingAudioSource.loop = true;
                var volume = laserWorkingAudioSource.volume;
                laserWorkingAudioSource.volume = 0;
                laserWorkingAudioEasingCoroutine = laserWorkingAudioSource.DoVolume(volume, laserWorkingSoundFadeInDuration);
            }

            laserTotemTurningAudioSource = GameController.AudioManager.PlayAudio(laserTotemTurningSound);

            if (laserTotemTurningAudioSource != null)
            {
                laserTotemTurningAudioSource.loop = true;
                var volume = laserTotemTurningAudioSource.volume;
                laserTotemTurningAudioSource.volume = 0;
                laserTotemTurningAudioEasingCoroutine = laserTotemTurningAudioSource.DoVolume(volume, laserTotemTurningSoundFadeInDuration);
            }

            rotationCoroutine = EasingManager.DoFloat(0, laserAttackRotationSpeedMultiplier, laserAttackRotationStartDuration, SetLaserAttackRotationSpeed).SetEasingCurve(laserAttackRotationStartCurve);

            for (int i = 0; i < laserSpawnPositions.Count; i++)
            {
                var spawnPosition = laserSpawnPositions[i];

                var laser = StageController.ProjectilesManager.GetProjectile(totemLaserPrefab);
                laser.transform.SetParent(spawnPosition);

                laser.transform.localPosition = Vector3.zero;
                laser.transform.localRotation = Quaternion.identity;

                laser.Launch(LaserAttackDamage);

                lasers.Add(laser);
            }
        }

        protected virtual void SetLaserAttackRotationSpeed(float speedMultiplier)
        {
            animator.SetFloat(ATTACK_1_SPEED_MULTIPLIER_FLOAT, speedMultiplier);
        }

        protected virtual IEnumerator SpawnTornado()
        {
            var time = 0f;
            var nextSpawn = 0f;

            tornadoTotemTurningAudioSource = GameController.AudioManager.PlayAudio(tornadoTotemTurningSound);

            if (tornadoTotemTurningAudioSource != null)
            {
                tornadoTotemTurningAudioSource.loop = true;
                var volume = tornadoTotemTurningAudioSource.volume;
                tornadoTotemTurningAudioSource.volume = 0;
                tornadoTotemTurningAudioEasingCoroutine = tornadoTotemTurningAudioSource.DoVolume(volume, tornadoTotemTurningSoundFadeOutDuration);
            }

            while (waitForAttackToEnd)
            {
                time += Time.deltaTime;

                if(animator.speed > 0.1f)
                {
                    while (nextSpawn < time)
                    {
                        nextSpawn += delayBetweenTornadoBursts;

                        var startAngle = new Vector3(0, Random.Range(0, 360f / tornadoProjectilesCountInBurst), 0);

                        var step = new Vector3(0, 360f / tornadoProjectilesCountInBurst, 0);

                        for (int i = 0; i < tornadoProjectilesCountInBurst; i++)
                        {
                            var rotation = Quaternion.Euler(startAngle + step * i);

                            var tornado = StageController.ProjectilesManager.GetProjectile(tornadoPrefab);
                            tornado.transform.position = transform.position + rotation * Vector3.forward * tornadoSpawnRadius;
                            tornado.transform.rotation = rotation;

                            tornado.Launch(TornadoAttackDamage);

                            tornado.onProjectileHidden += OnTornadoHidden;
                            tornados.Add(tornado);
                        }
                    }
                } else
                {
                    nextSpawn += Time.deltaTime;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Spawns Fist Projectile on top of the Player or in the random positing around the totem if the Player hasn't been detected
        /// </summary>
        protected virtual void SpawnFists()
        {
            Vector3 targetPosition;

            if (Target != null || TryFindTarget())
            {
                targetPosition = Target.Position;
            }
            else
            {
                targetPosition = GetRandomPositionInRadius(10, 1, true);
            }

            var fist = StageController.ProjectilesManager.GetProjectile(fistPrefab);
            fist.transform.position = targetPosition;

            fist.Launch(FistAttackDamage);

            fist.onProjectileHidden += OnFistHidden;
            fists.Add(fist);

            if(monkeyHeadSlamParticle != null) monkeyHeadSlamParticle.Play();

            GameController.AudioManager.PlayAudio(monkeyJumpingSound);
        }

        #endregion

        protected virtual void OnTornadoHidden(AbstractProjectile tornado)
        {
            tornado.onProjectileHidden -= OnTornadoHidden;

            tornados.Remove(tornado);
        }

        protected virtual void OnFistHidden(AbstractProjectile fist)
        {
            fist.onProjectileHidden -= OnFistHidden;

            tornados.Remove(fist);
        }

        public virtual void OnSpawnParticleEventFired()
        {
            if(spawnParticle != null) spawnParticle.Play();
        }

        public virtual void OnMonkeyChargeJumpEventFired()
        {
            GameController.AudioManager.PlayAudio(monkeyJumpingSound);
        }

        protected override void ClearOnDefeatCallbacks()
        {
            base.ClearOnDefeatCallbacks();

            // Clearing all projectiles that are left in the scene, preventing them from killing the player after the boss has been beaten;

            for(int i = 0; i < lasers.Count; i++)
            {
                lasers[i].Clear();
            }

            for (int i = 0; i < tornados.Count; i++)
            {
                tornados[i].onProjectileHidden -= OnTornadoHidden;
                tornados[i].Clear();
            }

            for (int i = 0; i < fists.Count; i++)
            {
                fists[i].onProjectileHidden -= OnFistHidden;
                fists[i].Clear();
            }

            lasers.Clear();
            tornados.Clear();
            fists.Clear();

            rotationCoroutine.StopIfExists();
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

            if(applied && type == StatusEffectType.Freeze)
            {
                if(laserTotemTurningAudioSource != null)
                {
                    laserTotemTurningAudioSource.Pause();
                }

                if (tornadoTotemTurningAudioSource != null)
                {
                    tornadoTotemTurningAudioSource.Pause();
                }
            }

            return applied;
        }

        public override void RemoveStatusEffect(StatusEffectType type)
        {
            base.RemoveStatusEffect(type);

            if (type == StatusEffectType.Freeze)
            {
                if (laserTotemTurningAudioSource != null)
                {
                    laserTotemTurningAudioSource.Play();
                }

                if (tornadoTotemTurningAudioSource != null)
                {
                    tornadoTotemTurningAudioSource.Play();
                }
            }
        }

        protected virtual void OnPlayerDefeated()
        {
            if (laserWorkingAudioSource != null)
            {
                laserWorkingAudioSource.loop = false;
                laserWorkingAudioSource.Stop();
                laserWorkingAudioSource = null;

                laserWorkingAudioEasingCoroutine.StopIfExists();
            }

            if (laserTotemTurningAudioSource != null)
            {
                laserTotemTurningAudioSource.loop = false;
                laserTotemTurningAudioSource.Stop();
                laserTotemTurningAudioSource = null;

                laserTotemTurningAudioEasingCoroutine.StopIfExists();
            }

            if (tornadoTotemTurningAudioSource != null)
            {
                tornadoTotemTurningAudioSource.loop = false;
                tornadoTotemTurningAudioSource.Stop();
                tornadoTotemTurningAudioSource = null;

                tornadoTotemTurningAudioEasingCoroutine.StopIfExists();
            }
        }

        protected override void Defeat()
        {
            base.Defeat();

            if (laserWorkingAudioSource != null)
            {
                laserWorkingAudioSource.loop = false;
                laserWorkingAudioSource.Stop();
                laserWorkingAudioSource = null;

                laserWorkingAudioEasingCoroutine.StopIfExists();
            }

            if (laserTotemTurningAudioSource != null)
            {
                laserTotemTurningAudioSource.loop = false;
                laserTotemTurningAudioSource.Stop();
                laserTotemTurningAudioSource = null;

                laserTotemTurningAudioEasingCoroutine.StopIfExists();
            }

            if (tornadoTotemTurningAudioSource != null)
            {
                tornadoTotemTurningAudioSource.loop = false;
                tornadoTotemTurningAudioSource.Stop();
                tornadoTotemTurningAudioSource = null;

                tornadoTotemTurningAudioEasingCoroutine.StopIfExists();
            }
        }

        public enum TotemAttackType
        {
            Laser = 0,
            Tornado = 1,
            Fist = 2,
        }
    }
}