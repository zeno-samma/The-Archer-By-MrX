using OctoberStudio.Audio;
using OctoberStudio.Extensions;
using OctoberStudio.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class IceSlimeBossBehavior : EnemyBehavior
    {
        [SerializeField] protected List<IceSlimeAttackType> attacksSequence = new List<IceSlimeAttackType>
        {
            IceSlimeAttackType.IceSphere,
            IceSlimeAttackType.IceCrystal,
            IceSlimeAttackType.IcePuddles,
            IceSlimeAttackType.IceShards
        };

        [Space]
        [SerializeField] protected Float idleDuration = (1, 1.5f);
        [SerializeField] protected Float moveTowardsPlayerDuration = (2f, 3f);

        [Header("Ice Sphere Attack")]
        [SerializeField] protected GameObject iceSpherePrefab;
        [SerializeField] protected Transform iceSphereSpawnPosition;

        [Space]
        [SerializeField] protected Int iceSphereAttacksCount = 2;
        [SerializeField] protected Float iceSphereDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData iceSphereSpawnSound;

        [Header("Ice Crystal Attack")]
        [SerializeField] protected GameObject iceCrystalPrefab;

        [Space]
        [SerializeField] protected Int iceCrystalAttacksCount = 3;
        [SerializeField] protected Float iceCrystalAttackDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Int iceCrystalsCountPerAttack = 1;
        [SerializeField] protected Float timeBetweenIceCrystalSpawns = 0.5f;

        [Space]
        [SerializeField] protected AudioData iceCrystalSpawnSound;

        [Header("Ice Puddle Attack")]
        [SerializeField] protected GameObject icePuddlePrefab;

        [Space]
        [SerializeField] protected Int icePuddlesCount = (6, 8);
        [SerializeField] protected Float icePuddleDamageMultiplier = (0.9f, 1.1f);
        [SerializeField] protected Float timeBetweenIcePuddlesSpawn = 0.2f;

        [Space]
        [SerializeField] protected AudioData icePuddleAttackChargeSound;
        [SerializeField] protected AudioData icePuddleSpawnSound;

        [Header("Ice Shard Attack")]
        [SerializeField] protected GameObject iceShardPrefab;
        [SerializeField] protected Transform iceShardSpawnPosition;

        [Space]
        [SerializeField] protected Int iceShardAttacksCount = 8;
        [SerializeField] protected Float iceShardDamageMultiplier = (0.9f, 1.1f);

        [Space]
        [SerializeField] protected AudioData iceShardSpawnSound;

        protected List<AbstractProjectile> projectiles = new List<AbstractProjectile>();
        protected List<IceSphereBehavior> iceSpheres = new List<IceSphereBehavior>();

        protected float IceSphereDamage => Damage * iceSphereDamageMultiplier;
        protected float IceCrystalDamage => Damage * iceCrystalAttackDamageMultiplier;
        protected float IcePuddleDamage => Damage * icePuddleDamageMultiplier;
        protected float IceShardDamage => Damage * iceShardDamageMultiplier;

        protected override void Start()
        {
            base.Start();

            StageController.ProjectilesManager.RegisterProjectile(iceCrystalPrefab);
            StageController.ProjectilesManager.RegisterProjectile(iceSpherePrefab);
            StageController.ProjectilesManager.RegisterProjectile(icePuddlePrefab);
            StageController.ProjectilesManager.RegisterProjectile(iceShardPrefab);
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            enemyCollider.enabled = false;

            yield return null;

            TryFindTarget();
            LookAtPlayer = true;

            waitForSpawnToEnd.Reset();
            yield return waitForSpawnToEnd;

            enemyCollider.enabled = true;

            int i = 0;

            while (IsAlive)
            {
                yield return LocateTarget(0.3f);

                switch (attacksSequence[i % attacksSequence.Count])
                {
                    case IceSlimeAttackType.IceSphere:

                        yield return IdleCoroutine(idleDuration);
                        yield return RandomMoveCoroutine();

                        yield return IdleCoroutine(idleDuration);
                        yield return IceSphereAttackCoroutine();

                        break;
                    case IceSlimeAttackType.IceCrystal:

                        yield return IdleCoroutine(idleDuration);
                        yield return RandomMoveCoroutine();

                        yield return IdleCoroutine(idleDuration);
                        yield return IceCrystalAttackCoroutine();

                        break;
                    case IceSlimeAttackType.IcePuddles:
                        yield return IdleCoroutine(idleDuration);
                        yield return MoveToPlayerCoroutine();

                        yield return IdleCoroutine(idleDuration);
                        yield return IcePuddlesAttackCoroutine();

                        break;
                    case IceSlimeAttackType.IceShards:
                        yield return IceShardsAttackCoroutine();
                        break;
                }

                i++;
            }
        }

        protected virtual IEnumerator IdleCoroutine(float duration)
        {
            LookAtPlayer = true;
            yield return new WaitForSeconds(duration);
        }

        protected virtual IEnumerator RandomMoveCoroutine()
        {
            LookAtPlayer = false;
            
            TryMoveStraightInRadius(5, 1, 20);
            
            yield return navigationHandler.WaitUntilReachedDestination();
        }

        protected virtual IEnumerator MoveToPlayerCoroutine()
        {
            LookAtPlayer = false;

            if (TryFindTarget())
            {
                MoveTowardsTarget();

                yield return new WaitForSeconds(moveTowardsPlayerDuration);

                navigationHandler.Stop();
            } else
            {
                RandomMoveCoroutine();
            }
        }

        protected virtual IEnumerator IceSphereAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)IceSlimeAttackType.IceSphere);
            animator.SetTrigger(ATTACK_TRIGGER);

            for (int i = 0; i < iceSphereAttacksCount; i++)
            {
                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        protected virtual IEnumerator IceCrystalAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)IceSlimeAttackType.IceCrystal);
            animator.SetTrigger(ATTACK_TRIGGER);

            for(int i = 0; i < iceCrystalAttacksCount; i++)
            {
                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        protected virtual IEnumerator IcePuddlesAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)IceSlimeAttackType.IcePuddles);
            animator.SetTrigger(ATTACK_TRIGGER);

            GameController.AudioManager.PlayAudio(icePuddleAttackChargeSound);

            waitForAttackToEnd.Reset();
            yield return waitForAttackToEnd;

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        protected virtual IEnumerator IceShardsAttackCoroutine()
        {
            animator.SetInteger(ATTACK_TYPE_INT, (int)IceSlimeAttackType.IceShards);
            animator.SetTrigger(ATTACK_TRIGGER);

            for (int i = 0; i < iceShardAttacksCount; i++)
            {
                waitForAttackToEnd.Reset();
                yield return waitForAttackToEnd;
            }

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);
        }

        public override void OnAttackEventFired()
        {
            var attackType = (IceSlimeAttackType) animator.GetInteger(ATTACK_TYPE_INT);

            switch (attackType)
            {
                case IceSlimeAttackType.IceSphere:
                    SpawnIceSphere();
                    break;

                case IceSlimeAttackType.IceCrystal:
                    StartCoroutine(SpawnIceCrystal());
                    break;

                case IceSlimeAttackType.IcePuddles:
                    StartCoroutine(SpawnIcePuddles());
                    break;

                case IceSlimeAttackType.IceShards:
                    SpawnIceShards();
                    break;
            }
        }

        public override void OnAttackEndedEventFired()
        {
            waitForAttackToEnd.Complete();
        }

        protected virtual void SpawnIceSphere()
        {
            TryFindTarget();
            var direction = Target == null ? iceSphereSpawnPosition.forward.NormalizeXZ() : iceSphereSpawnPosition.position.DirectionToXZ(Target.Position);

            var startAngle = -45f;
            var step = 45f;

            for(int i = 0; i < 3; i++)
            {
                var iceSphere = StageController.ProjectilesManager.GetProjectile<IceSphereBehavior>(iceSpherePrefab);

                iceSphere.transform.position = iceSphereSpawnPosition.position;
                iceSphere.transform.forward = Quaternion.Euler(0, startAngle + step * i, 0) * direction;

                iceSphere.onProjectileHidden += OnIceSphereHidden;
                iceSphere.onSecondaryProjectileSpawned += OnIceSphereAdditionalProjectileSpawned;

                iceSphere.Target = Target?.transform;

                iceSphere.Launch(IceSphereDamage);
                iceSpheres.Add(iceSphere);
            }

            GameController.AudioManager.PlayAudio(iceSphereSpawnSound);
        }

        protected virtual IEnumerator SpawnIceCrystal()
        {
            for(int i = 0; i < iceCrystalsCountPerAttack; i++)
            {
                var projectile = StageController.ProjectilesManager.GetProjectile(iceCrystalPrefab);

                if (TryFindTarget())
                {
                    projectile.transform.position = GetRandomPositionAroundPointInRadius(Target.Position, 5, 2f, true, IceCrystalPositionValidation, 50);
                } else
                {
                    projectile.transform.position = GetRandomPositionInRadius(20f, 2f, true);
                }

                projectile.Launch(IceCrystalDamage);

                projectile.onProjectileHidden += OnProjectileHidden;
                projectiles.Add(projectile);

                GameController.AudioManager.PlayAudio(iceCrystalSpawnSound);

                yield return new WaitForSeconds(timeBetweenIceCrystalSpawns);
                if(animator.speed < 0.1f)
                {
                    yield return new WaitUntil(() => animator.speed > 0.1f);
                }
            }
        }

        protected virtual bool IceCrystalPositionValidation(Vector3 position)
        {
            for(int i = 0; i < projectiles.Count; i++)
            {
                if (Vector3.Distance(position, projectiles[i].transform.position) < 3) return false;
            }

            return true;
        }

        protected virtual IEnumerator SpawnIcePuddles()
        {
            for(int i = 0; i < icePuddlesCount; i++)
            {
                var position = GetRandomPositionAroundPointInRadius(Position, 20, 1f, true, IcePuddlesPositionValidation, 50);

                if (position == Position) continue;

                var puddle = StageController.ProjectilesManager.GetProjectile(icePuddlePrefab);

                puddle.transform.position = position;

                puddle.Launch(IcePuddleDamage);

                puddle.onProjectileHidden += OnProjectileHidden;
                projectiles.Add(puddle);

                GameController.AudioManager.PlayAudio(icePuddleSpawnSound);

                yield return new WaitForSeconds(timeBetweenIcePuddlesSpawn);

                if (animator.speed < 0.1f)
                {
                    yield return new WaitUntil(() => animator.speed > 0.1f);
                }
            }
        }

        protected virtual bool IcePuddlesPositionValidation(Vector3 position)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (Vector3.Distance(position, projectiles[i].transform.position) < 4) return false;
            }

            return true;
        }

        protected virtual void SpawnIceShards()
        {
            var shard = StageController.ProjectilesManager.GetProjectile(iceShardPrefab);
            shard.transform.position = iceShardSpawnPosition.position;
            shard.transform.forward = iceShardSpawnPosition.forward.NormalizeXZ();

            shard.onProjectileHidden += OnProjectileHidden;
            shard.Launch(IceShardDamage);

            GameController.AudioManager.PlayAudio(iceShardSpawnSound);
        }

        protected virtual void OnProjectileHidden(AbstractProjectile projectile)
        {
            projectile.onProjectileHidden -= OnProjectileHidden;
            projectiles.Remove(projectile);
        }

        protected virtual void OnIceSphereHidden(AbstractProjectile projectile)
        {
            projectile.onProjectileHidden -= OnIceSphereHidden;

            if (projectile is IceSphereBehavior iceSphere)
            {
                iceSphere.onSecondaryProjectileSpawned -= OnIceSphereAdditionalProjectileSpawned;

                iceSpheres.Remove(iceSphere);
            }
        }

        protected virtual void OnIceSphereAdditionalProjectileSpawned(AbstractProjectile additionalProjectile)
        {
            additionalProjectile.onProjectileHidden += OnProjectileHidden;
            projectiles.Add(additionalProjectile);
        }

        public override void OnSpawnEndedEventFired()
        {
            waitForSpawnToEnd.Complete();
        }

        public override void OnDefeatEndedEventFired()
        {
            base.OnDefeatEndedEventFired();

            for(int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].onProjectileHidden -= OnProjectileHidden;
                projectiles[i].Clear();
            }

            projectiles.Clear();

            for(int i = 0; i < iceSpheres.Count; i++)
            {
                iceSpheres[i].onProjectileHidden -= OnIceSphereHidden;
                iceSpheres[i].onSecondaryProjectileSpawned -= OnIceSphereAdditionalProjectileSpawned;
                iceSpheres[i].Clear();
            }

            iceSpheres.Clear();
        }

        public enum IceSlimeAttackType
        {
            IceSphere = 0,
            IceCrystal = 1,
            IcePuddles = 2,
            IceShards = 3
        }
    }
}