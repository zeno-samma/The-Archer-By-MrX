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
    public class CactusBossBehavior : EnemyBehavior
    {
        [SerializeField] protected List<CactusAttackType> attacksSequence = new List<CactusAttackType>
        {
            CactusAttackType.Flex,
            CactusAttackType.MachineGun,
            CactusAttackType.Spikes,
        };

        [Header("Idle")]
        [SerializeField] protected Float idleDuration = 1.5f;

        [Header("Flex Attack")]
        [SerializeField] protected GameObject flexAttackProjectilePrefab;

        [Space]
        [SerializeField] protected Transform warningLinesParent;
        [SerializeField] protected List<WaspChargeLineBehavior> warningLines;

        [Space]
        [SerializeField] protected Int flexAttacksCount = (2, 3);
        [SerializeField] protected Float flexAttackDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float flexAttackProjectileSpawnHeight = 1f;

        [Space]
        [SerializeField] protected AudioData flexAttackChargingSound;
        [SerializeField] protected AudioData flexAttackShootingSound;

        [Header("Machine Gun Attack")]
        [SerializeField] protected GameObject machineGunAttackProjectilePrefab;
        [SerializeField] protected ParticleSystem machineGunParticle;

        [Space]
        [SerializeField] protected Transform machineGunProjectileSpawnPosition;
        [SerializeField] protected Float machineGunProjectileSpawnRadius = 0.4f;

        [Space]
        [SerializeField] protected Int machineGunAttacksCount = 2;
        [SerializeField] protected Float machineGunAttackDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float machineGunAttackDuration = 3f;
        [SerializeField] protected Float machineGunAttackSpread = 2f;
        [SerializeField] protected Float machineGunAttackDelayBetweenProjectiles = 0.1f;

        [Space]
        [SerializeField] protected AudioData machineGunPreparationSound;
        [SerializeField] protected AudioData machineGunFiringSound;
        [SerializeField] protected float machineGunFiringSoundFadeInDuration;
        [SerializeField] protected float machineGunFiringSoundFadeOutDuration;

        [Header("Spikes Attack")]
        [SerializeField] protected GameObject spikePrefab;
        [SerializeField] protected ParticleSystem leftHandInsertedParticle;
        [SerializeField] protected ParticleSystem rightHandInsertedParticle;

        [Space]
        [SerializeField] protected Float spikesAttackDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float spikesAttackDuration = 5;
        [SerializeField] protected Float spikesAttackDelayBetweenSpikes = 0.8f;

        [Space]
        [SerializeField] protected AudioData handInsertedSound;

        protected List<AbstractProjectile> spawnedSpikes = new List<AbstractProjectile>();
        protected List<AbstractProjectile> spawnedflexProjectiles = new List<AbstractProjectile>();
        protected List<AbstractProjectile> spawnedMachineGunProjectiles = new List<AbstractProjectile>();

        protected float FlexDamage => Damage * flexAttackDamageMultiplier;
        protected float MachineGunDamage => Damage * machineGunAttackDamageMultiplier;
        protected float SpikesDamage => Damage * spikesAttackDamageMultiplier;

        protected AudioSource machineGunFiringAudioSource;

        protected IEasingCoroutine machineGunFiringAudioEasingCoroutine;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(flexAttackProjectilePrefab);
            StageController.ProjectilesManager.RegisterProjectile(machineGunAttackProjectilePrefab);
            StageController.ProjectilesManager.RegisterProjectile(spikePrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            LookAtPlayer = true;

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
                    case CactusAttackType.Flex:
                        yield return FlexAttackCoroutine();
                        break;
                    case CactusAttackType.MachineGun:
                        yield return MachineGunAttackCoroutine();
                        break;
                    case CactusAttackType.Spikes:
                        yield return SpikesAttackCoroutine();
                        break;
                }

                i++;
            }
        }

        protected virtual IEnumerator FlexAttackCoroutine()
        {
            for (int i = 0; i < flexAttacksCount; i++)
            {
                if (TryFindTarget())
                {
                    warningLinesParent.SetParent(null);
                    if (Target == null)
                    {
                        warningLinesParent.forward = Vector3.forward;
                    }
                    else
                    {
                        warningLinesParent.forward = (Target.Position - warningLinesParent.position).NormalizeXZ();
                    }

                    for (int j = 0; j < warningLines.Count; j++)
                    {
                        warningLines[j].Show(30, 0.2f);
                    }

                    animator.SetInteger(ATTACK_TYPE_INT, (int)CactusAttackType.Flex);
                    animator.SetTrigger(ATTACK_TRIGGER);

                    GameController.AudioManager.PlayAudio(flexAttackChargingSound);

                    waitForAttackToEnd.Reset();

                    yield return waitForAttackToEnd;

                    warningLinesParent.SetParent(transform);
                    LookAtPlayer = true;
                }
                else
                {
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            } 
        }

        protected virtual IEnumerator MachineGunAttackCoroutine()
        {
            for (int i = 0; i < machineGunAttacksCount; i++)
            {
                if (TryFindTarget())
                {
                    GameController.AudioManager.PlayAudio(machineGunPreparationSound);

                    animator.SetInteger(ATTACK_TYPE_INT, (int)CactusAttackType.MachineGun);
                    animator.SetTrigger(ATTACK_TRIGGER);

                    LookAtPlayer = true;

                    waitForAttackToEnd.Reset();
                    yield return waitForAttackToEnd;
                }
                else
                {
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        protected virtual IEnumerator SpikesAttackCoroutine()
        {
            if (TryFindTarget())
            {
                LookAtPlayer = false;

                animator.SetInteger(ATTACK_TYPE_INT, (int)CactusAttackType.Spikes);
                animator.SetTrigger(ATTACK_TRIGGER);

                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;

                LookAtPlayer = true;
            }
        }

        public override void OnAttackEventFired()
        {
            base.OnAttackEventFired();

            if(!IsAlive) return;

            var attackType = (CactusAttackType)animator.GetInteger(ATTACK_TYPE_INT);

            switch (attackType)
            {
                case CactusAttackType.Flex:
                    SpawnFlexProjectiles();
                    break;

                case CactusAttackType.Spikes:
                    StartCoroutine(SpawnSpikesCoroutine());
                    break;

                case CactusAttackType.MachineGun:
                    StartCoroutine(SpawnMachineGunCoroutine());
                    break;
            }
        }

        protected virtual IEnumerator SpawnSpikesCoroutine()
        {
            var time = 0f;
            var nextSpawn = 0f;

            while (time < spikesAttackDuration)
            {
                time += Time.deltaTime;

                if (animator.speed > 0.1f)
                {
                    while (nextSpawn < time)
                    {
                        nextSpawn += spikesAttackDelayBetweenSpikes;

                        var spike = StageController.ProjectilesManager.GetProjectile(spikePrefab);

                        if (Target != null)
                        {
                            spike.transform.position = Target.transform.position;
                        }
                        else
                        {
                            spike.transform.position = GetRandomPositionInRadius(10, 2, true);
                        }

                        spike.Launch(SpikesDamage);
                        spike.onProjectileHidden += OnSpikeHidden;
                        spawnedSpikes.Add(spike);
                    }
                } else
                {
                    nextSpawn += Time.deltaTime;
                }

                yield return null;
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
            waitForAttackToEnd.Complete();
        }

        protected virtual void OnSpikeHidden(AbstractProjectile spike)
        {
            spawnedSpikes.Remove(spike);
            spike.onProjectileHidden -= OnSpikeHidden;
        }

        protected virtual void SpawnFlexProjectiles()
        {
            LookAtPlayer = false;

            for (int i = 0; i < warningLines.Count; i++)
            {
                var line = warningLines[i];
                line.Hide();

                var projectile = StageController.ProjectilesManager.GetProjectile(flexAttackProjectilePrefab);

                projectile.transform.position = line.transform.position.SetY(flexAttackProjectileSpawnHeight);
                projectile.transform.forward = line.transform.forward;

                projectile.onProjectileHidden += OnFlexProjectileHidden;
                spawnedflexProjectiles.Add(projectile);

                projectile.Launch(FlexDamage);
            }

            GameController.AudioManager.PlayAudio(flexAttackShootingSound);
        }

        protected virtual void OnFlexProjectileHidden(AbstractProjectile projectile)
        {
            spawnedflexProjectiles.Remove(projectile);
            projectile.onProjectileHidden -= OnFlexProjectileHidden;
        }

        protected virtual IEnumerator SpawnMachineGunCoroutine()
        {
            var time = 0f;
            var nextSpawn = 0f;

            if(machineGunParticle != null)
            {
                machineGunParticle.Play();
            }

            machineGunFiringAudioSource = GameController.AudioManager.PlayAudio(machineGunFiringSound);

            if (machineGunFiringAudioSource != null)
            {
                machineGunFiringAudioSource.loop = true;
                var volume = machineGunFiringAudioSource.volume;
                machineGunFiringAudioSource.volume = 0;
                machineGunFiringAudioEasingCoroutine = machineGunFiringAudioSource.DoVolume(volume, machineGunFiringSoundFadeInDuration);
            }

            while (time < machineGunAttackDuration)
            {
                time += Time.deltaTime;

                if (animator.speed > 0.1f)
                {
                    while (nextSpawn < time)
                    {
                        nextSpawn += machineGunAttackDelayBetweenProjectiles;

                        var projectile = StageController.ProjectilesManager.GetProjectile(machineGunAttackProjectilePrefab);

                        projectile.transform.position = machineGunProjectileSpawnPosition.position + Quaternion.FromToRotation(Vector3.forward, machineGunProjectileSpawnPosition.forward) * Random.insideUnitSphere.SetZ(0) * machineGunProjectileSpawnRadius;
                        projectile.transform.forward = Quaternion.Euler(0, Random.Range(-1f, 1f) * machineGunAttackSpread / 2f, 0) * machineGunProjectileSpawnPosition.forward;

                        projectile.onProjectileHidden += OnMachineGunProjectileHidden;
                        spawnedMachineGunProjectiles.Add(projectile);

                        projectile.Launch(MachineGunDamage);
                    }
                }
                else
                {
                    nextSpawn += Time.deltaTime;
                }

                yield return null;
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
            waitForAttackToEnd.Complete();

            if(machineGunParticle != null)
            {
                machineGunParticle.Stop();
            }

            if (machineGunFiringAudioSource != null)
            {
                machineGunFiringAudioEasingCoroutine = machineGunFiringAudioSource.DoVolume(0, machineGunFiringSoundFadeOutDuration).SetOnFinish(() =>
                {
                    machineGunFiringAudioSource.loop = false;
                    machineGunFiringAudioSource.Stop();
                    machineGunFiringAudioSource = null;
                });
            }
        }

        protected virtual void OnMachineGunProjectileHidden(AbstractProjectile projectile)
        {
            spawnedMachineGunProjectiles.Remove(projectile);
            projectile.onProjectileHidden -= OnMachineGunProjectileHidden;
        }

        public override void OnAttackEndedEventFired()
        {
            waitForAttackToEnd.Complete();
        }

        public override void OnSpawnEndedEventFired()
        {
            waitForSpawnToEnd.Complete();
        }

        public virtual void OnLeftHandInsertedEventFired()
        {
            if(leftHandInsertedParticle != null) leftHandInsertedParticle.Play();

            GameController.AudioManager.PlayAudio(handInsertedSound);
        }

        public virtual void OnRightHandInsertedEventFired()
        {
            if(rightHandInsertedParticle != null) rightHandInsertedParticle.Play();

            GameController.AudioManager.PlayAudio(handInsertedSound);
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();

            if (machineGunParticle.isPlaying)
            {
                machineGunParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            for(int i = 0; i < spawnedSpikes.Count; i++)
            {
                spawnedSpikes[i].onProjectileHidden -= OnSpikeHidden;
                spawnedSpikes[i].Clear();
            }
            spawnedSpikes.Clear();

            for (int i = 0; i < spawnedflexProjectiles.Count; i++)
            {
                spawnedflexProjectiles[i].onProjectileHidden -= OnFlexProjectileHidden;
                spawnedflexProjectiles[i].Clear();
            }
            spawnedflexProjectiles.Clear();

            for (int i = 0; i < spawnedMachineGunProjectiles.Count; i++)
            {
                spawnedMachineGunProjectiles[i].onProjectileHidden -= OnMachineGunProjectileHidden;
                spawnedMachineGunProjectiles[i].Clear();
            }
            spawnedMachineGunProjectiles.Clear();

            if(warningLinesParent.parent == null)
            {
                warningLinesParent.SetParent(transform);
                for(int i = 0; i < warningLines.Count; i++)
                {
                    warningLines[i].gameObject.SetActive(false);
                }
            }

            if (machineGunFiringAudioSource != null)
            {
                machineGunFiringAudioSource.loop = false;
                machineGunFiringAudioSource.Stop();
                machineGunFiringAudioSource = null;

                machineGunFiringAudioEasingCoroutine.StopIfExists();
            }
        }

        public override bool ApplyStatusEffect(StatusEffectType type, float multiplier)
        {
            var applied = base.ApplyStatusEffect(type, multiplier);

            if (applied && type == StatusEffectType.Freeze)
            {
                if (machineGunFiringAudioSource != null)
                {
                    machineGunFiringAudioSource.Pause();

                    if (machineGunParticle != null)
                    {
                        machineGunParticle.Stop();
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
                if (machineGunFiringAudioSource != null)
                {
                    machineGunFiringAudioSource.Play();

                    if (machineGunParticle != null)
                    {
                        machineGunParticle.Play();
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            waitForSpawnToEnd.Reset();

            StageController.onDefeat += OnPlayerDefeated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StageController.onDefeat -= OnPlayerDefeated;
        }

        protected virtual void OnPlayerDefeated()
        {
            if (machineGunFiringAudioSource != null)
            {
                machineGunFiringAudioSource.loop = false;
                machineGunFiringAudioSource.Stop();
                machineGunFiringAudioSource = null;

                machineGunFiringAudioEasingCoroutine.StopIfExists();
            }
        }

        public enum CactusAttackType
        {
            Flex = 0,
            Spikes = 1,
            MachineGun = 2,
        }
    }
}