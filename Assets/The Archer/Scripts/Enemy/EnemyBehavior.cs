using OctoberStudio.Audio;
using OctoberStudio.Drop;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.StatusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Enemy
{
    public abstract class EnemyBehavior : MonoBehaviour, IDefeatable, IProjectileTarget
    {
        protected static readonly int MOVEMENT_BLEND_FLOAT = Animator.StringToHash("Movement Blend");
        protected static readonly int DEFEAT_TRIGGER = Animator.StringToHash("Defeat");
        protected static readonly int HIT_TRIGGER = Animator.StringToHash("Hit");
        protected static readonly int ATTACK_TRIGGER = Animator.StringToHash("Attack");
        protected static readonly int ATTACK_ENDED_TRIGGER = Animator.StringToHash("Attack Ended");

        protected static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");
        protected static readonly int SHOW_TRIGGER = Animator.StringToHash("Show");

        protected static readonly int ATTACK_TYPE_INT = Animator.StringToHash("Attack Type");

        [Header("References")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected Collider enemyCollider;
        [SerializeField] protected NavigationHandler navigationHandler;
        [SerializeField] protected RenderersHandler renderersHandler;
        [SerializeField] protected StatusEffectsHandler statusEffectsHandler;
        [SerializeField] protected HealthbarBehavior healthbar;
        [SerializeField] protected GameObject graphicsObject;
        [SerializeField] protected GameObject targetRingPrefab;

        [Header("Dying")]
        [SerializeField] protected GameObject deathParticlesPrefab;

        [Header("Hit Effect")]
        [SerializeField] protected bool useHitRimEffect = true;
        [SerializeField] protected Vector3 hitScale = new Vector3(0.8f, 1.6f, 0.8f);
        [SerializeField] protected Float hitScaleDuration = 0.1f;
        [SerializeField] protected EasingType hitScaleEasing = EasingType.SineIn;

        [Header("Knockback effect")]
        [SerializeField] protected bool useKnockback = true;
        [SerializeField] protected Float knockbackMagnitude = 8f;
        [SerializeField] protected Float knockbackCooldown = 1f;

        [Header("Settings")]
        [SerializeField] protected Float baseHealth = 100;
        [SerializeField] protected Float baseDamage = 1;
        [SerializeField] protected Float collisionDamageMultiplier = (5, 8);

        public Float Damage { get; protected set; }
        public Float PoisonDamageMultiplier { get; protected set; }
        public Float CollisionDamage { get; protected set; }

        public Float BaseDamage => baseDamage;
        public Float BaseHP => baseHealth;

        [Header("Sounds")]
        [SerializeField] protected AudioData spawnSound;
        [SerializeField] protected AudioData getHitSound;
        [SerializeField] protected AudioData deathSound;

        public EnemyData Data { get; protected set; }

        public virtual Transform Transform => transform;
        public virtual Vector3 Position => transform.position;
        public virtual Vector3 Destination => navigationHandler.Destination;

        public virtual int Layer => gameObject.layer;
        public virtual bool IsAlive => healthbar.HP > 0;
        public bool IsColliderEnabled => enemyCollider.enabled;

        public GameObject TargetRingPrefab => targetRingPrefab;

        protected virtual PlayerDetector TargetDetector { get; set; }
        protected PlayerBehavior Target { get; set; }

        public bool LookAtPlayer { get; set; }
        protected float LookAtPlayerMultiplier { get; set; } = 1;

        public MultiplicativeStat ReceivedDamageMultiplierStat { get; protected set; } = 1;

        protected WaitUntilTrue waitForSpawnToEnd = new WaitUntilTrue();
        protected WaitUntilTrue waitForAttackToEnd = new WaitUntilTrue();

        protected HashSet<int> animatorParameters;

        protected Coroutine knockbackCoroutine;
        protected Coroutine hitEffectCoroutine;
        protected Coroutine behaviorCoroutine;

        protected Vector3 cacheScale;
        protected float lastKnockbackTime = 0f;

        protected IEasingCoroutine scaleEasingCoroutine;

        protected event UnityAction<IDefeatable> onDefeated;

        protected abstract IEnumerator BehaviorCoroutine();

        public bool UseDrop { get; protected set; }
        public EnemyOverrideData OverrideData { get; protected set; }

        protected virtual void Awake()
        {
            animatorParameters = new HashSet<int>();
            for (int i = 0; i < animator.parameterCount; i++)
            {
                var parameter = animator.GetParameter(i);
                animatorParameters.Add(parameter.nameHash);
            }

            cacheScale = graphicsObject.transform.localScale;
        }

        protected virtual void Start()
        {
            if (deathParticlesPrefab != null)
            {
                StageController.ParticlesManager.RegisterParticle(deathParticlesPrefab);
            }
        }

        protected virtual void OnEnable()
        {
            OverrideData = null;
            graphicsObject.transform.localScale = cacheScale;
        }

        protected virtual void OnDisable()
        {

        }

        protected virtual IEnumerator LocateTarget(float interval)
        {
            var wait = new WaitForSeconds(interval);

            while (!TryFindTarget())
            {
                yield return wait;
            }
        }

        public virtual void SetData(EnemyData data)
        {
            Data = data;
        }

        public virtual void Spawn(EnemySpawnData spawn)
        {
            OverrideData = spawn.OverrideData;
            Spawn(spawn.TransformData.Position, spawn.TransformData.Rotation);
        }

        public virtual void Spawn(Vector3 position, Quaternion rotation, bool useDrop = true)
        {
            transform.position = position;
            transform.rotation = rotation;

            UseDrop = useDrop;

            enemyCollider.enabled = true;

            navigationHandler.Enable();

            Damage = baseDamage * StageController.EnemyDamageMultiplier;
            var hp = baseHealth * StageController.EnemyHPMultiplier;

            CollisionDamage = Damage * collisionDamageMultiplier;
            if (OverrideData != null)
            {
                Damage = OverrideData.ApplyDamageOverride(Damage);
                hp = OverrideData.ApplyHPOverride(hp);
            }

            healthbar.Init(hp);

            if (Data.IsBoss)
            {
                StageController.GameScreen.BossProgressbar.AddHealthbar(healthbar);

                healthbar.ForceHide();
            }
            else
            {
                healthbar.Show();
            }

            behaviorCoroutine = StartCoroutine(BehaviorCoroutine());

            GameController.AudioManager.PlayAudio(spawnSound);
        }

        public void RegisterPlayerDetector(PlayerDetector playerDetector)
        {
            TargetDetector = playerDetector;
        }

        /// <summary>
        /// Finds a closest target, or gets the player from the Stage Controller if the enemy does not have Player Detector Registered
        /// </summary>
        /// <returns>true if the target is detected</returns>
        protected bool TryFindTarget()
        {
            if (TargetDetector == null)
            {
                Target = StageController.Player;

                return true;
            }
            else
            {
                Target = TargetDetector.GetClosestPlayer();

                return Target != null;
            }
        }

        public virtual void TakeDamage(float damage, DamageType damageType, bool isCrit = false, bool disableSound = false)
        {
            if (healthbar.HP <= 0) return;
            if (!StageController.Player.IsAlive && !StageController.Player.CanBeRevived) return;

            var receivedDamage = damage * ReceivedDamageMultiplierStat * StageController.Player.Stats.GetDamageMultiplier(damageType);

            if (Data.IsBoss) receivedDamage *= StageController.Player.Stats.DamageToBossesMultiplierStat;

            var displayedDamage = healthbar.Subtract(receivedDamage);

            var textSpawnPosition = healthbar.transform.position;
            var text = displayedDamage.ToString();

            WorldSpaceTextType textType;
            if (isCrit)
            {
                textType = WorldSpaceTextType.CritDamage;
            }
            else
            {
                textType = DamageHelper.GetTextTypeFromDamageType(damageType);
            }

            StageController.GameScreen.WorldSpaceTextManager.SpawnText(textSpawnPosition, text, textType);

            if (healthbar.HP <= 0)
            {
                Defeat();
            }
            else
            {
                if (animatorParameters.Contains(HIT_TRIGGER))
                {
                    animator.SetTrigger(HIT_TRIGGER);
                }
            }

            if (!disableSound)
            {
                GameController.AudioManager.PlayAudio(getHitSound);
            }
        }

        public virtual void ShowHitEffect(Vector3 direction, bool useKnockBack)
        {
            if (healthbar.HP <= 0) return;
            if (!gameObject.activeSelf) return;

            if (useHitRimEffect)
            {
                renderersHandler.PlayHitEffect();

                if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);
                hitEffectCoroutine = StartCoroutine(HitEffectCoroutine());
            }

            if (useKnockBack && this.useKnockback && knockbackCoroutine == null && lastKnockbackTime + knockbackCooldown <= Time.time)
            {
                lastKnockbackTime = Time.time;
                knockbackCoroutine = StartCoroutine(HitKickBackCoroutine(direction, 0.1f, EasingType.QuintOut));
            }
        }

        protected virtual IEnumerator HitKickBackCoroutine(Vector3 direction, float duration, EasingType easingType)
        {
            var time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;

                var shift = direction * Time.deltaTime * EasingFunctions.ApplyEasing(time / duration, easingType) * knockbackMagnitude;
                transform.position += shift;

                yield return null;
            }

            knockbackCoroutine = null;
        }

        protected virtual IEnumerator HitEffectCoroutine()
        {
            float firstDuration = hitScaleDuration / 3;

            scaleEasingCoroutine.StopIfExists();
            scaleEasingCoroutine = graphicsObject.transform.DoLocalScale(new Vector3(cacheScale.x * hitScale.x, cacheScale.y * hitScale.y, cacheScale.z * hitScale.z), firstDuration);

            yield return new WaitForSeconds(firstDuration);

            float secondDuration = hitScaleDuration / 3 * 2;

            graphicsObject.transform.DoLocalScale(cacheScale, secondDuration).SetEasing(hitScaleEasing);

            yield return new WaitForSeconds(secondDuration);
        }

        protected virtual void Defeat()
        {
            StopCoroutine(behaviorCoroutine);

            animator.SetTrigger(DEFEAT_TRIGGER);

            enemyCollider.enabled = false;

            if (healthbar.HP > 0) healthbar.Subtract(healthbar.HP);
            healthbar.Hide();

            navigationHandler.Disable();

            GameController.AudioManager.PlayAudio(deathSound);

            onDefeated?.Invoke(this);

            ClearOnDefeatCallbacks();
        }

        protected virtual void Update()
        {
            if (!IsAlive) return;

            if (animatorParameters.Contains(MOVEMENT_BLEND_FLOAT))
            {
                animator.SetFloat(MOVEMENT_BLEND_FLOAT, navigationHandler.GetMovementMultiplier());
            }

            LookAtPlayerUpdate();
        }

        protected virtual void LookAtPlayerUpdate()
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
            }
        }

        public virtual bool ApplyStatusEffect(StatusEffectType type, float multiplier)
        {
            var applied = statusEffectsHandler.ApplyStatusEffect(type);

            if (applied && type == StatusEffectType.Freeze)
            {
                animator.speed = multiplier;
                navigationHandler.SetMovementSpeedMultiplier(multiplier);
                LookAtPlayerMultiplier = multiplier;
            }

            if (applied && type == StatusEffectType.Poison)
            {
                Damage *= multiplier;
                PoisonDamageMultiplier = multiplier;
            }

            return applied;
        }

        public virtual void RemoveStatusEffect(StatusEffectType type)
        {
            if (type == StatusEffectType.Freeze)
            {
                if (animator != null) animator.speed = 1;
                if (navigationHandler != null) navigationHandler.SetMovementSpeedMultiplier(1);
                LookAtPlayerMultiplier = 1f;
            }
            else if (type == StatusEffectType.Poison)
            {
                Damage /= PoisonDamageMultiplier;
                PoisonDamageMultiplier = 1f;
            }

            if (statusEffectsHandler != null) statusEffectsHandler.RemoveStatusEffect(type);
        }

        public virtual float GetStatusEffectDurationMultiplier(StatusEffectType statusEffect)
        {
            if (statusEffectsHandler == null) return 1;

            return statusEffectsHandler.GetStatusEffectDurationMultiplier(statusEffect);
        }

        public virtual void Kill()
        {
            TakeDamage(healthbar.HP + 1, DamageType.Physical);
        }

        public virtual void SubscribeOnDefeat(UnityAction<IDefeatable> action)
        {
            onDefeated += action;
        }

        public virtual void UnsubscribeOnDefeat(UnityAction<IDefeatable> action)
        {
            onDefeated -= action;
        }

        protected virtual void ClearOnDefeatCallbacks()
        {
            if (onDefeated == null) return;

            var callbacks = onDefeated.GetInvocationList();

            for (int i = 0; i < callbacks.Length; i++)
            {
                onDefeated -= (UnityAction<IDefeatable>)callbacks[i];
            }
        }

        #region Animator Events

        public virtual void OnDefeatEndedEventFired()
        {
            if (deathParticlesPrefab != null)
            {
                var particle = StageController.ParticlesManager.GetParticle(deathParticlesPrefab);

                particle.transform.position = Position;
                particle.Play();
            }

            gameObject.SetActive(false);
        }

        public virtual void OnAttackEventFired() { }

        public virtual void OnAttackEndedEventFired() { }

        public virtual void OnHideEndedEventFired() { }

        public virtual void OnSpawnEndedEventFired() { }

        #endregion

        protected virtual bool TryMove(Vector3 destination)
        {
            return navigationHandler.TryMove(destination);
        }

        protected virtual bool TryMoveStraight(Vector3 destination)
        {
            if (StageController.NavigationManager.IsPositionAvailable(destination))
            {
                if (StageController.NavigationManager.IsStraightPathAvailable(transform.position, destination, out var obstaclePosition, out var hitNormal))
                {
                    navigationHandler.Move(destination);
                    return true;
                }
                return false;
            }
            return false;
        }

        protected virtual bool TryMoveStraightInRadius(float radius, float minRadius, int maxNumberOfTries)
        {
            for (int counter = 0; counter < maxNumberOfTries; counter++)
            {
                var distance = Mathf.Lerp(radius, minRadius, (float)counter / maxNumberOfTries);
                var randomPosition = Position + Random.insideUnitSphere.SetY(0).normalized * distance;
                if (StageController.Room.IsCloseToOtherEnemyDestination(this, randomPosition, 1f)) continue;
                if (TryMoveStraight(randomPosition)) return true;
            }

            return false;
        }

        protected virtual void MoveTowardsTarget()
        {
            if (Target != null)
            {
                navigationHandler.Move(Target.Position);
            }

        }

        public virtual void SlowDown(float multiplier)
        {

        }

        public virtual void Drop(float experience)
        {
            if (!UseDrop) return;

            int counter = 0;
            bool droppedExperience = false;

            for (int i = 0; i < Data.Drops.Count; i++)
            {
                var data = Data.Drops[i];

                if (OverrideData != null)
                {
                    if (OverrideData.RemoveDrop.Contains(data.DropType)) continue;

                    var dropOverride = GetOverride(data.DropType);
                    if (dropOverride != null) data = dropOverride;
                }

                var chance = data.GetChance(StageController.Player.NormalizedHP);

                if (chance <= 0) continue;

                if (data.DropType == DropType.XPGem && droppedExperience) continue;

                if (chance >= 100 || Random.value * 100 <= chance)
                {
                    var amount = data.GetDropAmount(StageController.Player.NormalizedHP).Value;
                    for (int j = 0; j < amount; j++)
                    {
                        var delay = counter * 0.1f;
                        counter++;

                        var drop = StageController.DropManager.Drop(data.DropType, transform.position, delay);

                        if (drop is XPDropBehavior gem)
                        {
                            gem.XPAmount = experience / amount;

                            droppedExperience = true;
                        }
                        else if (drop is ItemDropBehavior item)
                        {
                            item.ItemData = data.ItemData;
                        }
                    }
                }
            }

            var assignAdditionalExperience = !droppedExperience;

            if (OverrideData != null)
            {
                for (int i = 0; i < OverrideData.AdditionalDrop.Count; i++)
                {
                    var data = OverrideData.AdditionalDrop[i];

                    var chance = data.GetChance(StageController.Player.NormalizedHP);

                    if (chance <= 0) continue;

                    if (data.DropType == DropType.XPGem && droppedExperience) continue;
                    if (data.DropType == DropType.Item && !StageController.DropManager.CanDropItem(data))
                    {
                        continue;
                    }

                    if (chance >= 100 || Random.value * 100 <= chance)
                    {
                        var amount = data.GetDropAmount(StageController.Player.NormalizedHP).Value;
                        for (int j = 0; j < amount; j++)
                        {
                            var delay = counter * 0.1f;
                            counter++;

                            var drop = StageController.DropManager.Drop(data.DropType, transform.position, delay);

                            if (drop is XPDropBehavior gem)
                            {
                                if (assignAdditionalExperience)
                                {
                                    gem.XPAmount = experience / amount;

                                    droppedExperience = true;
                                }
                            }
                            else if (drop is ItemDropBehavior item)
                            {
                                item.ItemData = data.ItemData;
                            }
                        }
                    }
                }
            }

            if (experience > 0 && !droppedExperience)
            {
                var gem = StageController.DropManager.Drop(DropType.XPGem, transform.position, counter * 0.1f) as XPDropBehavior;
                gem.XPAmount = experience;
            }
        }

        protected virtual EnemyDropData GetOverride(DropType dropType)
        {
            if (OverrideData == null) return null;

            for (int i = 0; i < OverrideData.DropOverrides.Count; i++)
            {
                var data = OverrideData.DropOverrides[i];
                if (data.DropType == dropType) return data;
            }
            return null;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerBehavior player))
            {
                player.TakeDamage(CollisionDamage, DamageType.Physical);
            }
        }

        /// <summary>
        /// If cannot find valid random position, returns the position of the enemy
        /// </summary>
        /// <param name="radius">Max distance from the enemy to a random position</param>
        /// <param name="minRadius">Min distance from the enemy to a random position</param>
        /// <param name="vaildate">Check if the position is available to navigate</param>
        /// <returns>Random Vector3 point around the enemy</returns>
        protected virtual Vector3 GetRandomPositionInRadius(float radius, float minRadius = 0, bool vaildate = true)
        {
            var randomPosition = Position + Random.insideUnitSphere.SetY(0).normalized * Random.Range(minRadius, radius);

            if (vaildate)
            {
                var counter = 0;

                while (!StageController.NavigationManager.IsPositionAvailable(randomPosition))
                {
                    randomPosition = Position + Random.insideUnitSphere.SetY(0).normalized * Random.Range(minRadius, radius);
                    // endless loop prevention
                    counter++;
                    if (counter >= 20) return Position;
                }
            }

            return randomPosition;
        }

        protected virtual Vector3 GetRandomPositionAroundPointInRadius(Vector3 position, float radius, float minRadius = 0, bool vaildate = true, System.Func<Vector3, bool> customValidationFunc = null, int triesCout = 20)
        {
            var randomPosition = position + Random.insideUnitSphere.SetY(0).normalized * Random.Range(minRadius, radius);

            if (vaildate)
            {
                var counter = 0;

                while (!StageController.NavigationManager.IsPositionAvailable(randomPosition) || (customValidationFunc != null && !customValidationFunc(randomPosition)))
                {
                    randomPosition = position + Random.insideUnitSphere.SetY(0).normalized * Random.Range(minRadius, radius);

                    // endless loop prevention
                    counter++;
                    if (counter >= triesCout) return position;
                }
            }

            return randomPosition;
        }

        protected virtual Vector3 GetRandomPositionAroundPointInRadius(Vector3 position, float radius, bool vaildate = true)
        {
            var randomPosition = position + Random.insideUnitSphere.SetY(0).normalized * Random.value * radius;

            if (vaildate)
            {
                var counter = 0;

                while (!StageController.NavigationManager.IsPositionAvailable(randomPosition))
                {
                    randomPosition = position + Random.insideUnitSphere.SetY(0).normalized * Random.value * radius;

                    // endless loop prevention
                    counter++;
                    if (counter >= 20) return Position;
                }
            }

            return randomPosition;
        }
    }
}