using OctoberStudio.Armory;
using OctoberStudio.Easing;
using OctoberStudio.Enemy;
using OctoberStudio.Extensions;
using OctoberStudio.Player;
using OctoberStudio.StatusEffects;
using OctoberStudio.Weapon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class PlayerBehavior : MonoBehaviour, IProjectileTarget
    {
        public virtual Vector3 Position => transform.position;
        public virtual int Layer => gameObject.layer;
        public virtual Transform Transform => transform;

        [SerializeField] protected HealthbarBehavior healthbar;
        [SerializeField] protected NavigationHandler navigationHandler;
        [SerializeField] protected PlayerOrbsManager orbsManager;
        [SerializeField] protected PlayerSpiritsManager spiritsManager;

        [Space]
        [SerializeField] protected Collider trigger;

        protected EnemiesDetector EnemiesDetector { get; set; }
        public PlayerOrbsManager OrbsManager => orbsManager;
        public PlayerSpiritsManager SpiritsManager => spiritsManager;
        public PlayerStatsHandler Stats { get; protected set; } = new PlayerStatsHandler();

        public IHeroBehavior Hero { get; protected set; }
        public AbstractWeaponBehavior Weapon { get; protected set; }
        protected HeroData HeroData { get; set; }
        protected WeaponData WeaponData { get; set; }

        protected event UnityAction<IDefeatable> onDefeated;
        public UnityAction<float> onTakenDamage;
        public UnityAction onHPChanged;

        public bool IsInvincible { get; protected set; }
        public bool IsMovingAlowed { get; set; }
        public bool IsPiercingArrowEnabled { get; set; }
        public bool ProjectilesIgnoreObstacles { get; set; }
        public bool MovedThisFrame { get; protected set; }

        public float Damage => Stats.AttackDamageStat.Value;
        public float HP => healthbar.HP;
        public float MaxHP => healthbar.MaxHP;
        public float NormalizedHP => healthbar.HP / healthbar.MaxHP;

        public bool IsAlive => healthbar.HP > 0;
        public bool CanBeRevived => Stats.RevivesStat.Value > RevivedTimes;

        protected virtual float DodgeChance => Stats.DodgeChanceStat.Value / 100f;

        protected int RevivedTimes { get => ContinuePlayingSave.RevivedTimes; set => ContinuePlayingSave.RevivedTimes = value; }

        public UnityAction<EnemyBehavior> onClosestEnemyChanged;
        public UnityAction onStartedMoving;
        public UnityAction onStoppedMoving;

        public UnityAction onPerfectStanceActivated;
        public UnityAction onPerfectStanceDeactivated;

        public UnityAction onInvincibilityActivated;
        public UnityAction<bool> onInvincibilityDeactivated;

        public UnityAction onHurryActivated;
        public UnityAction onHurryDeactivated;

        protected EnemyBehavior closestEnemy;
        public EnemyBehavior ClosestEnemy
        {
            get => closestEnemy;
            protected set
            {
                var prevClosestEnemy = closestEnemy;
                closestEnemy = value;
                if (closestEnemy != prevClosestEnemy)
                {
                    onClosestEnemyChanged?.Invoke(closestEnemy);
                }
            }
        }

        protected IEasingCoroutine slowDownEasingCoroutine;
        public AnimationCurve DistanceDamageMultiplierCurve { get; set; }
        protected StatMultiplier hurryMovementStatMultiplier = 1;

        protected List<StatusEffect> projectileStatusEffects = new List<StatusEffect>();
        protected List<InvincibilityData> invincibilities = new List<InvincibilityData>();

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        public bool IsPlaying { get; protected set; } = false;

        protected StatMultiplier PoisonedDamageMultiplier { get; set; } = 1;

        protected bool isWaitingForRevive = false;

        protected virtual void Awake()
        {
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            StageController.RegisterPlayerBehavior(this);

            Hero = GameController.ArmoryManager.SpawnHero();
            Hero.Transform.SetParent(transform);
            Hero.Transform.ResetLocal();

            Weapon = GameController.ArmoryManager.SpawnWeapon();

            var animationsSet = GameController.ArmoryManager.GetWeaponAnimationSet(Hero, Weapon);
            Hero.PlaceWeapon(Weapon, animationsSet);

            healthbar.Init(1);

            Stats.MaxHPStat.onStatChanged += OnMaxHPStatChanged;
            Stats.MovementSpeedStat.onStatChanged += OnMovementSpeedStatChanged;
            Stats.SizeStat.onStatChanged += OnSizeChanged;
            Stats.MovementSpeedStat.AddMultiplier(hurryMovementStatMultiplier);

            Stats.Init();
            Stats.AttackDamageStat.AddMultiplier(PoisonedDamageMultiplier);


            healthbar.ResetHP();

            Hero.SetMovementSpeed(Stats.MovementSpeedStat);
        }

        protected virtual void SaveHP()
        {
            ContinuePlayingSave.HP = HP;
        }

        protected virtual void Start()
        {
            StageController.DoAfterRoomLoaded(() => StageController.Room.OnExitSpawned += OnExitSpawned);
        }

        protected virtual void OnExitSpawned()
        {
            hurryMovementStatMultiplier.Value = 1.5f;
            onHurryActivated?.Invoke();
        }

        protected virtual void OnSizeChanged(MultiplicativeStat sizeStat)
        {
            transform.localScale = Vector3.one * sizeStat.Value;
        }

        protected virtual void OnArrowRangeChanged(MultiplicativeStat stat)
        {
            if (EnemiesDetector.InitialRadius != stat.Value)
            {
                EnemiesDetector.ShowRangeIndicator();
            }
            else
            {
                EnemiesDetector.HideRangeIndicator();
            }
        }

        protected virtual void OnMaxHPStatChanged(MultiplicativeStat maxHPStat)
        {
            var addedHP = healthbar.ChangeMaxHP(maxHPStat);

            if (addedHP > 0 && StageController.GameScreen.WorldSpaceTextManager.IsInitialized)
            {
                StageController.GameScreen.WorldSpaceTextManager.SpawnText(healthbar.transform.position, $"+{addedHP}", WorldSpaceTextType.PlayerHeal);

                Hero.PlayHealEffect(true);
            }

            onHPChanged?.Invoke();
        }

        protected virtual void OnMovementSpeedStatChanged(MultiplicativeStat movementSpeedStat)
        {
            Hero.SetMovementSpeed(movementSpeedStat);
        }

        public virtual void StartPlaying(Vector3 spawnPoint)
        {
            hurryMovementStatMultiplier.Value = 1f;
            onHurryDeactivated?.Invoke();

            IsMovingAlowed = true;

            navigationHandler.Enable();
            navigationHandler.Teleport(spawnPoint);

            SpiritsManager.ResetPosition();

            EasingManager.DoNextFrame(() =>
            {
                Hero.ClearTrails();
                OrbsManager.ResetTrails();
            });

            onHPChanged += SaveHP;

            IsPlaying = true;

            if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                if (ContinuePlayingSave.HP != HP)
                {
                    healthbar.Subtract(HP - ContinuePlayingSave.HP);
                }
            }
            else
            {

                ContinuePlayingSave.HP = HP;
            }
        }

        public virtual void RegisterEnemiesDetector(EnemiesDetector detector)
        {
            EnemiesDetector = detector;

            Stats.ArrowRangeStat.ChangeInitialValue(EnemiesDetector.InitialRadius);

            Stats.ArrowRangeStat.onStatChanged += OnArrowRangeChanged;
        }

        public virtual void SlowDown(float multiplier)
        {
            Stats.SlowDownMultiplier.Value = multiplier;

            slowDownEasingCoroutine.StopIfExists();
            slowDownEasingCoroutine = EasingManager.DoAfter(1f, () => Stats.SlowDownMultiplier.Value = 1);
        }

        public virtual void ShowPerfectStance()
        {
            onPerfectStanceActivated?.Invoke();
        }

        public virtual void HidePerfectStance()
        {
            onPerfectStanceDeactivated?.Invoke();
        }

        protected virtual void Update()
        {
            if (healthbar.IsZero) return;

            if (EnemiesDetector != null)
            {
                EnemiesDetector.GetClosestEnemy(out var newClosestEnemy, !ProjectilesIgnoreObstacles);

                if (newClosestEnemy != ClosestEnemy)
                {
                    ClosestEnemy = newClosestEnemy;
                }
            }

            if (!IsMovingAlowed) return;

            var input = GameController.InputManager.MovementValue.X0Y();
            var joysticPower = input.magnitude;

            // Preventing player position change during navigation in pause menu
            if (Time.timeScale > 0.1f) Hero.SetMovementBlend(joysticPower);

            if (!Mathf.Approximately(joysticPower, 0) && Time.timeScale > 0)
            {
                var frameMovement = input * Time.deltaTime * Hero.Speed;

                transform.position += frameMovement;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(input.normalized, Vector3.up), Time.deltaTime * 10);

                if (!MovedThisFrame) onStartedMoving?.Invoke();
                MovedThisFrame = true;
            }
            else
            {
                if (MovedThisFrame) onStoppedMoving?.Invoke();
                MovedThisFrame = false;

                if (ClosestEnemy != null)
                {
                    var lookDirection = (ClosestEnemy.Position - transform.position).SetY(0).normalized;

                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection, Vector3.up), Time.deltaTime * 20);
                }
            }

            for (int i = 0; i < invincibilities.Count; i++)
            {
                var invincibility = invincibilities[i];

                if (invincibility.duration > 0)
                {
                    if (Time.time - invincibility.startTime >= invincibility.duration)
                    {
                        RemoveInvincibility(invincibility);
                        i--;
                    }
                }
            }
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

        public virtual void OnDefeatEndedEventFired()
        {
            gameObject.SetActive(false);
        }

        public virtual void TakeDamage(float damage, DamageType damageType, bool isCrit = false, bool disableSound = false)
        {
            if (HP <= 0) return;
            if (StageController.Room.AliveEnemiesCount == 0) return;

            onTakenDamage?.Invoke(IsInvincible ? 0 : damage);

            if (IsInvincible) return;

            Hero.PlayHitEffect(disableSound);

            if (Random.value < DodgeChance)
            {
                var dodgeTextSpawnPosition = healthbar.transform.position;
                StageController.GameScreen.WorldSpaceTextManager.SpawnText(healthbar.transform.position, "Dodge", WorldSpaceTextType.Dodge);

                return;
            }

            damage *= Stats.DamageReduction.Value;

            var subtractedHP = healthbar.Subtract(damage);

            onHPChanged?.Invoke();

            var damageTextSpawnPosition = healthbar.transform.position;
            var textType = DamageHelper.GetTextTypeFromDamageToPlayerType(damageType);
            StageController.GameScreen.WorldSpaceTextManager.SpawnText(damageTextSpawnPosition, $"-{subtractedHP}", textType);

            if (healthbar.HP <= 0)
            {
                healthbar.Hide();

                Hero.OnDefeat();
                trigger.enabled = false;

                onDefeated?.Invoke(this);
            }
        }

        public virtual void HealProportion(float maxProportionHP, bool withSound = true)
        {
            if (!IsPlaying) return;

            var addedHP = healthbar.AddPercentage(maxProportionHP * 100 * Stats.HealingStat);

            if (addedHP > 0)
            {
                StageController.GameScreen.WorldSpaceTextManager.SpawnText(healthbar.transform.position, $"+{addedHP}", WorldSpaceTextType.PlayerHeal);

                Hero.PlayHealEffect(withSound);
            }

            onHPChanged?.Invoke();
        }

        public virtual void AddHP(float hp, bool withSound = true)
        {
            if (!IsPlaying) return;

            var hpToAdd = hp * Stats.HealingStat;

            var addedHP = healthbar.AddHP(hpToAdd);

            if (addedHP > 0)
            {
                StageController.GameScreen.WorldSpaceTextManager.SpawnText(healthbar.transform.position, $"+{addedHP}", WorldSpaceTextType.PlayerHeal);

                Hero.PlayHealEffect(withSound);
            }

            onHPChanged?.Invoke();
        }

        public virtual void ShowHitEffect(Vector3 direction, bool useKnockBack)
        {

        }

        public virtual bool ApplyStatusEffect(StatusEffectType type, float multiplier)
        {
            var applied = Hero.ApplyStatusEffect(type, multiplier);

            if (applied && type == StatusEffectType.Freeze)
            {
                navigationHandler.SetMovementSpeedMultiplier(multiplier);
            }

            if (applied && type == StatusEffectType.Poison)
            {
                PoisonedDamageMultiplier.Value = multiplier;
            }

            return applied;
        }

        public virtual void RemoveStatusEffect(StatusEffectType type)
        {
            Hero.RemoveStatusEffect(type);

            if (type == StatusEffectType.Freeze)
            {
                if (navigationHandler != null) navigationHandler.SetMovementSpeedMultiplier(1);
            }
            else if (type == StatusEffectType.Poison)
            {
                PoisonedDamageMultiplier.Value = 1;
            }
        }

        public virtual float GetStatusEffectDurationMultiplier(StatusEffectType statusEffect)
        {
            return Hero.GetStatusEffectDurationMultiplier(statusEffect);
        }

        public virtual InvincibilityData MakeInvincible(float duration, bool explode)
        {
            var data = new InvincibilityData()
            {
                startTime = Time.time,
                duration = duration,
                explode = explode,
            };

            invincibilities.Add(data);

            RecalculateInvincibilityBar();

            if (!IsInvincible)
            {
                IsInvincible = true;

                onInvincibilityActivated?.Invoke();
            }
            return data;
        }

        public virtual void RemoveInvincibility(InvincibilityData data)
        {
            invincibilities.Remove(data);
            if (invincibilities.Count == 0)
            {
                IsInvincible = false;

                onInvincibilityDeactivated?.Invoke(data.explode);
            }
            RecalculateInvincibilityBar();
        }

        protected virtual void RecalculateInvincibilityBar()
        {
            var furthestEndTime = 0f;
            InvincibilityData furthestInvinsibility = null;

            for (int i = 0; i < invincibilities.Count; i++)
            {
                var invincibility = invincibilities[i];

                if (invincibility.duration <= 0)
                {
                    furthestInvinsibility = invincibility;
                    break;
                }

                var endTime = invincibility.startTime + invincibility.duration;
                if (endTime > furthestEndTime)
                {
                    furthestEndTime = endTime;
                    furthestInvinsibility = invincibility;
                }
            }

            healthbar.SetInvincibilityData(furthestInvinsibility);
        }

        public virtual void DetachWeapon()
        {
            Hero.DetachWeapon();
            Hero.Weapon.OnDetachedFromHero();
        }

        public virtual void AttachWeapon()
        {
            Hero.AttachWeapon();
            Hero.Weapon.OnAttachedToHero();
        }

        public virtual bool TryToRevive()
        {
            if (Stats.RevivesStat.Value > RevivedTimes)
            {
                IsMovingAlowed = false;

                RevivedTimes++;

                isWaitingForRevive = true;

                Hero.Revive();
                MakeInvincible(4f, false);
                EasingManager.DoAfter(1.1f, () =>
                {
                    onHPChanged?.Invoke();
                });

                healthbar.Show();
                healthbar.ResetHP(1);

                return true;
            }

            return false;
        }

        public virtual void OnReviveFinished()
        {
            if (isWaitingForRevive)
            {
                trigger.enabled = true;
                IsMovingAlowed = true;

                isWaitingForRevive = false;
            }
        }

        public virtual void SetProjectileEffect(StatusEffect effect)
        {
            projectileStatusEffects.Add(effect);
            Hero.Weapon.ApplyStatusEffect(effect);
        }

        public virtual void RemoveProjectileEffect(StatusEffect effect)
        {
            projectileStatusEffects.Remove(effect);
            Hero.Weapon.RemoveStatusEffect(effect);
        }

        protected virtual void OnDestroy()
        {
            Stats.ArrowRangeStat.onStatChanged -= OnArrowRangeChanged;

            onHPChanged -= SaveHP;
        }
    }

    public class InvincibilityData
    {
        public float startTime;
        public float duration;
        public bool explode = false;
    }
}