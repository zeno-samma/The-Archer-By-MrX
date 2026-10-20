using MrX.Armory;
using MrX.StatusEffects;
using MrX.Weapon;
using UnityEngine;

namespace MrX.Player
{
    public interface IHeroBehavior
    {
        Transform Transform { get; }
        float Speed { get; }

        AbstractWeaponBehavior Weapon { get; }
        HeroData HeroData { get; }

        void Init(HeroData data);
        void PlaceWeapon(AbstractWeaponBehavior weapon, WeaponAnimationsSet animationsSet);

        void OnShootAnimationEventFired();
        void SetMovementSpeed(float speed);

        void ClearTrails();

        void SetMovementBlend(float blend);

        void Revive();

        void AttachWeapon();
        void DetachWeapon();

        void OnDefeat();
        void OnDefeatAnimationEnded();
        void OnReviveAnimationEndedEventFired();
        void OnStepEventFired();

        void PlayHitEffect(bool disableSound);
        void PlayHealEffect(bool withSound);

        bool ApplyStatusEffect(StatusEffectType type, float multiplier);
        void RemoveStatusEffect(StatusEffectType type);
        float GetStatusEffectDurationMultiplier(StatusEffectType statusEffect);
    }
}