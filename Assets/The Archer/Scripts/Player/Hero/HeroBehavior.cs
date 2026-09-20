using OctoberStudio.Armory;
using OctoberStudio.Audio;
using OctoberStudio.Enemy;
using OctoberStudio.Extensions;
using OctoberStudio.StatusEffects;
using OctoberStudio.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Player
{
    public class HeroBehavior : MonoBehaviour, IHeroBehavior
    {
        protected static readonly int MOVEMENT_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Movement Speed Multiplier");
        protected static readonly int ATTACK_SPEED_MULTIPLIER_FLOAT = Animator.StringToHash("Attack Speed Multiplier");

        protected static readonly int MOVEMENT_BLEND_FLOAT = Animator.StringToHash("Movement Blend");
        protected static readonly int CAN_ATTACK_BOOL = Animator.StringToHash("Can Attack");
        protected static readonly int WEAPON_ATTACHED_BOOL = Animator.StringToHash("Weapon Attached");

        protected static readonly int DEFEAT_TRIGGER = Animator.StringToHash("Defeat");
        protected static readonly int REVIVE_TRIGGER = Animator.StringToHash("Revive");

        [SerializeField] protected Animator animator;
        [SerializeField] protected Transform leftWeaponParent;
        [SerializeField] protected Transform rightWeaponParent;
        [SerializeField] protected FlyingWeaponHolder flyingWeaponHolder;
        [SerializeField] protected AnimationCurve flyingWeaponSpeedDistanceCurve;

        [Space]
        [SerializeField] protected RenderersHandler renderersHandler;
        [SerializeField] protected StatusEffectsHandler statusEffectsHandler;

        [Space]
        [SerializeField] protected AudioData getHitSound;
        [SerializeField] protected AudioData healSound;
        [SerializeField] protected AudioData stepSound;

        protected float movementAnimationSpeed = 5;
        protected float attackAnimationLength = 1;

        [Space]
        [SerializeField] protected ProtectiveBubbleBehavior protectiveBubble;
        [SerializeField] protected GameObject perfectStanceParticleObject;
        [SerializeField] protected GameObject hurryParticleObject;
        [SerializeField] protected ParticleSystem runParticle;
        [SerializeField] protected ParticleSystem healParticle;

        protected AnimatorOverrideController overrideController;

        public Transform Transform => transform;

        public float Speed { get; protected set; }

        public AbstractWeaponBehavior Weapon { get; protected set; }
        public HeroData HeroData { get; protected set; }

        protected List<KeyValuePair<AnimationClip, AnimationClip>> animationOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        protected float movementBlend;

        protected virtual void Awake()
        {
            if (animator != null)
            {
                overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

                overrideController.GetOverrides(animationOverrides);
            }
        }

        public virtual void Init(HeroData hero)
        {
            HeroData = hero;

            animator.SetBool(CAN_ATTACK_BOOL, false);

            StageController.Player.Stats.AttackSpeedStat.onStatChanged += OnAttackSpeedChanged;

            StageController.Player.onStartedMoving += OnPlayerStartedMoving;
            StageController.Player.onStoppedMoving += OnPlayerStoppedMoving;

            StageController.Player.onClosestEnemyChanged += OnClosestEnemyChanged;

            StageController.Player.onInvincibilityActivated += ShowInvincibilityVisuals;
            StageController.Player.onInvincibilityDeactivated += HideInvincibilityVisuals;

            StageController.Player.onPerfectStanceActivated += ShowPerfectStanceVisuals;
            StageController.Player.onPerfectStanceDeactivated += HidePerfectStanceVisuals;

            StageController.Player.onHurryActivated += ShowHurryVisuals;
            StageController.Player.onHurryDeactivated += HideHurryVisuals;
        }

        public virtual void PlayHitEffect(bool disableSound)
        {
            renderersHandler.PlayHitEffect();

            if (!disableSound) GameController.AudioManager.PlayAudio(getHitSound);
        }

        public virtual void PlayHealEffect(bool withSound)
        {
            if (healParticle != null) healParticle.Play();
            if (withSound) GameController.AudioManager.PlayAudio(healSound);
        }

        protected virtual void OnPlayerStartedMoving()
        {
            animator.SetBool(CAN_ATTACK_BOOL, false);
        }

        protected virtual void OnPlayerStoppedMoving()
        {
            animator.SetBool(CAN_ATTACK_BOOL, StageController.Player.ClosestEnemy != null && !flyingWeaponHolder.IsDetached);
        }

        protected virtual void OnClosestEnemyChanged(EnemyBehavior closestEnemy)
        {
            if (StageController.Player.MovedThisFrame || flyingWeaponHolder.IsDetached)
            {
                animator.SetBool(CAN_ATTACK_BOOL, false);
            }
            else
            {
                animator.SetBool(CAN_ATTACK_BOOL, closestEnemy != null);
            }
        }

        public virtual void ClearTrails()
        {
            runParticle.Clear();

            if (Weapon != null && Weapon.IsDetached)
            {
                flyingWeaponHolder.ResetPosition();
            }
        }

        public virtual void PlaceWeapon(AbstractWeaponBehavior weapon, WeaponAnimationsSet animationsSet)
        {
            Weapon = weapon;

            if (weapon.IsRightHandWeapon)
            {
                Weapon.transform.SetParent(rightWeaponParent);
            }
            else
            {
                Weapon.transform.SetParent(leftWeaponParent);
            }

            Weapon.transform.ResetLocal();

            if(animationsSet.WeaponAnimationsSetType == WeaponAnimationsSetType.Separate_Animations)
            {
                for (int i = 0; i < animationsSet.AnimationsCount; i++)
                {
                    var animation = animationsSet.Animations[i];

                    for (int j = 0; j < animationOverrides.Count; j++)
                    {
                        if (animationOverrides[j].Key.name == animation.Name)
                        {
                            if (animation.Name == "Hero Shoot")
                            {
                                attackAnimationLength = animation.AnimationClip.length;
                                weapon.SetAnimationLength(attackAnimationLength);
                            }

                            animationOverrides[j] = new KeyValuePair<AnimationClip, AnimationClip>(animationOverrides[j].Key, animation.AnimationClip);
                            break;
                        }
                    }
                }

                overrideController.ApplyOverrides(animationOverrides);

                animator.runtimeAnimatorController = overrideController;
            } else
            {
                animator.runtimeAnimatorController = animationsSet.RuntimeAnimatorController;
                attackAnimationLength = animationsSet.AttackAnimationLength;
                weapon.SetAnimationLength(attackAnimationLength);
            }

            movementAnimationSpeed = animationsSet.MovementAnimationSpeed;
        }

        public virtual void SetMovementBlend(float blend)
        {
            movementBlend = blend;

            animator.SetFloat(MOVEMENT_BLEND_FLOAT, movementBlend);
        }

        protected virtual void OnAttackSpeedChanged(MultiplicativeStat attackSpeedStat)
        {
            animator.SetFloat(ATTACK_SPEED_MULTIPLIER_FLOAT, attackSpeedStat / attackAnimationLength);
        }

        public virtual void SetMovementSpeed(float speed)
        {
            Speed = speed;
            animator.SetFloat(MOVEMENT_SPEED_MULTIPLIER_FLOAT, speed / movementAnimationSpeed);
        }

        public virtual void OnShootAnimationEventFired()
        {
            if (movementBlend < 0.01f)
            {
                var multishotCount = StageController.Player.Stats.MultishotStat + 1;
                StartCoroutine(ShootCoroutine(multishotCount));
            }
        }

        protected virtual IEnumerator ShootCoroutine(int multiShotCount)
        {
            for (int i = 0; i < multiShotCount; i++)
            {
                Shoot();

                if (i != multiShotCount - 1)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        protected virtual void Shoot()
        {
            var closestEnemy = StageController.Player.ClosestEnemy;
            if (closestEnemy == null) return;

            var direction = (closestEnemy.Position - Weapon.transform.position).SetY(0).normalized;
            Weapon.Shoot(closestEnemy.transform, direction);
        }

        public virtual void ShowInvincibilityVisuals()
        {
            protectiveBubble.Show();
        }

        public virtual void HideInvincibilityVisuals(bool explosion)
        {
            protectiveBubble.Hide(explosion);
        }

        public virtual void ShowPerfectStanceVisuals()
        {
            perfectStanceParticleObject.SetActive(true);
        }

        public virtual void HidePerfectStanceVisuals()
        {
            perfectStanceParticleObject.SetActive(false);
        }

        public virtual void ShowHurryVisuals()
        {
            if (hurryParticleObject != null) hurryParticleObject.SetActive(true);
        }

        public virtual void HideHurryVisuals()
        {
            if (hurryParticleObject != null) hurryParticleObject.SetActive(false);
        }

        public virtual void OnDefeat()
        {
            animator.SetTrigger(DEFEAT_TRIGGER);
        }

        public virtual void OnDefeatAnimationEnded()
        {

        }

        public virtual void OnStepEventFired()
        {
            GameController.AudioManager.PlayAudio(stepSound);
        }

        public virtual void DetachWeapon()
        {
            flyingWeaponHolder.Detach(Weapon);

            animator.SetBool(CAN_ATTACK_BOOL, false);
            animator.SetBool(WEAPON_ATTACHED_BOOL, false);
        }

        public virtual void AttachWeapon()
        {
            if (Weapon.IsRightHandWeapon)
            {
                Weapon.transform.SetParent(rightWeaponParent);
            }
            else
            {
                Weapon.transform.SetParent(leftWeaponParent);
            }

            Weapon.transform.ResetLocal();

            flyingWeaponHolder.Attach();
            animator.SetBool(WEAPON_ATTACHED_BOOL, true);
        }

        public virtual void Revive()
        {
            animator.SetTrigger(REVIVE_TRIGGER);
        }

        public virtual void OnReviveAnimationEndedEventFired()
        {
            StageController.Player.OnReviveFinished();
        }

        public bool ApplyStatusEffect(StatusEffectType type, float multiplier)
        {
            var applied = statusEffectsHandler.ApplyStatusEffect(type);

            if (applied && type == StatusEffectType.Freeze)
            {
                animator.speed = multiplier;
            }

            return applied;
        }

        public void RemoveStatusEffect(StatusEffectType type)
        {
            if (type == StatusEffectType.Freeze)
            {
                if (animator != null) animator.speed = 1;
            }

            if (statusEffectsHandler != null) statusEffectsHandler.RemoveStatusEffect(type);
        }

        public float GetStatusEffectDurationMultiplier(StatusEffectType statusEffect)
        {
            if (statusEffectsHandler == null) return 1;

            return statusEffectsHandler.GetStatusEffectDurationMultiplier(statusEffect);
        }

        protected virtual void OnDestroy()
        {
            if (!StageController.IsLoaded) return;

            StageController.Player.Stats.AttackSpeedStat.onStatChanged -= OnAttackSpeedChanged;

            StageController.Player.onStartedMoving -= OnPlayerStartedMoving;
            StageController.Player.onStoppedMoving -= OnPlayerStoppedMoving;

            StageController.Player.onClosestEnemyChanged -= OnClosestEnemyChanged;

            StageController.Player.onInvincibilityActivated += ShowInvincibilityVisuals;
            StageController.Player.onInvincibilityDeactivated += HideInvincibilityVisuals;

            StageController.Player.onPerfectStanceActivated += ShowPerfectStanceVisuals;
            StageController.Player.onPerfectStanceDeactivated += HidePerfectStanceVisuals;

            StageController.Player.onHurryActivated += ShowHurryVisuals;
            StageController.Player.onHurryDeactivated += HideHurryVisuals;
        }
    }
}