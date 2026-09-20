using OctoberStudio.StatusEffects;

namespace OctoberStudio
{
    public static class DamageHelper
    {
        public static WorldSpaceTextType GetTextTypeFromStatusEffect(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Ignite: return WorldSpaceTextType.BurningDamage;
                case StatusEffectType.Freeze: return WorldSpaceTextType.FreezingDamage;
                case StatusEffectType.Poison: return WorldSpaceTextType.PoisonDamage;
                case StatusEffectType.Shock: return WorldSpaceTextType.ShockDamage;
                default: return WorldSpaceTextType.PhysicalDamage;
            }
        }

        public static DamageType GetDamageTypeFromStatusEffect(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Ignite: return DamageType.Burning;
                case StatusEffectType.Freeze: return DamageType.Freezing;
                case StatusEffectType.Poison: return DamageType.Poison;
                case StatusEffectType.Shock: return DamageType.Shock;
                default: return DamageType.Physical;
            }
        }

        public static WorldSpaceTextType GetTextTypeFromDamageType(DamageType effectType)
        {
            switch (effectType)
            {
                case DamageType.Burning: return WorldSpaceTextType.BurningDamage;
                case DamageType.Freezing: return WorldSpaceTextType.FreezingDamage;
                case DamageType.Poison: return WorldSpaceTextType.PoisonDamage;
                case DamageType.Shock: return WorldSpaceTextType.ShockDamage;
                default: return WorldSpaceTextType.PhysicalDamage;
            }
        }

        public static WorldSpaceTextType GetTextTypeFromDamageToPlayerType(DamageType effectType)
        {
            switch (effectType)
            {
                case DamageType.Burning: return WorldSpaceTextType.PlayerBurningDamage;
                case DamageType.Freezing: return WorldSpaceTextType.PlayerFreezingDamage;
                case DamageType.Poison: return WorldSpaceTextType.PlayerPoisonDamage;
                case DamageType.Shock: return WorldSpaceTextType.PlayerShockDamage;
                default: return WorldSpaceTextType.PlayerPhysicalDamage;
            }
        }
    }
}