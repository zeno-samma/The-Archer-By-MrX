using OctoberStudio.Armory;
using OctoberStudio.StatusEffects;
using System;
using System.Collections.Generic;

namespace OctoberStudio.Player
{
    public class PlayerStatsHandler
    {
        #region HP Stats

        public MultiplicativeStat MaxHPStat { get; protected set; } = 1;
        public MultiplicativeStat HealingStat { get; protected set; } = 1;

        #endregion

        #region Elemental Stats

        public MultiplicativeStat PoisonArrowSpeedMultiplierStat { get; protected set; } = 1;
        public MultiplicativeStat FreezeArrowSpeedMultiplierStat { get; protected set; } = 1;
        public MultiplicativeStat BurnArrowSpeedMultiplierStat { get; protected set; } = 1;
        public MultiplicativeStat ShockArrowSpeedMultiplierStat { get; protected set; } = 1;

        public MultiplicativeStat PoisonDamageMultiplierStat { get; protected set; } = 1;
        public MultiplicativeStat FreezeDamageMultiplierStat { get; protected set; } = 1;
        public MultiplicativeStat BurnDamageMultiplierStat { get; protected set; } = 1;
        public MultiplicativeStat ShockDamageMultiplierStat { get; protected set; } = 1;

        public MultiplicativeStat MeteorSpawnDelayStat { get; protected set; } = 1;
        public MultiplicativeStat StarSpawnIntervalMultiplierStat { get; protected set; } = 1;

        #endregion

        #region Arrows Stats

        public MultiplicativeStat FrontArrowDamageStat { get; protected set; } = 1;
        public MultiplicativeStat RearArrowDamageStat { get; protected set; } = 1;
        public MultiplicativeStat DiagonalArrowDamageStat { get; protected set; } = 1;

        public AdditiveStat FrontArrowsCountStat { get; protected set; } = 1;
        public AdditiveStat RearArrowsCountStat { get; protected set; } = 0;
        public AdditiveStat DiagonalArrowsCountStat { get; protected set; } = 0;

        public AdditiveStat ArrowBounceCountStat { get; protected set; } = 0;
        public MultiplicativeStat ArrowBounceDamageStat { get; protected set; } = 1;

        public AdditiveStat ArrowRicochetCountStat { get; protected set; } = 0;
        public MultiplicativeStat ArrowRicochetDamageStat { get; protected set; } = 1;

        public MultiplicativeStat ArrowSizeStat { get; protected set; } = 1;
        public AdditiveStat ArrowSpreadStat { get; protected set; } = 0;

        public MultiplicativeStat ArrowRangeStat { get; protected set; } = 1;

        public AdditiveStat MultishotStat { get; protected set; } = 0;

        public AdditiveStat SplitArrowCountStat { get; protected set; } = 0;
        public MultiplicativeStat SpliArrowDamageMultiplierStat { get; protected set; } = 1;

        public MultiplicativeStat MagnetismStat { get; protected set; } = 0;

        public StatMultiplier SlowDownMultiplier { get; protected set; } = 1;

        #endregion

        #region Character Stats

        public MultiplicativeStat MovementSpeedStat { get; protected set; } = 1;

        public MultiplicativeStat SizeStat { get; protected set; } = 1;

        public MultiplicativeStat AttackDamageStat { get; protected set; } = 1;
        public MultiplicativeStat AttackSpeedStat { get; protected set; } = 1;
        public MultiplicativeStat DamageReduction { get; protected set; } = 1;

        public MultiplicativeStat DamageToBossesMultiplierStat { get; protected set; } = 1;

        public AdditiveStat DodgeChanceStat { get; protected set; } = 0;
        public AdditiveStat CritChanceStat { get; protected set; } = 0;

        public AdditiveStat HPRecoveryStat { get; protected set; } = 0;

        public MultiplicativeStat CritDamageMultiplierStat { get; protected set; } = 1;

        public AdditiveStat RevivesStat { get; protected set; } = 0;

        public AdditiveStat ChanceToIncreaseRarity { get; protected set; } = 0;

        #endregion

        public virtual float GetDamageMultiplier(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Burning: return BurnDamageMultiplierStat;
                case DamageType.Freezing: return FreezeDamageMultiplierStat;
                case DamageType.Poison: return PoisonDamageMultiplierStat;
                case DamageType.Shock: return ShockDamageMultiplierStat;
                default: return 1;
            }
        }

        public virtual float GetArrowSpeedMultiplier(StatusEffectType statusEffectType)
        {
            switch (statusEffectType)
            {
                case StatusEffectType.Ignite: return BurnArrowSpeedMultiplierStat;
                case StatusEffectType.Freeze: return FreezeArrowSpeedMultiplierStat;
                case StatusEffectType.Poison: return PoisonArrowSpeedMultiplierStat;
                case StatusEffectType.Shock: return ShockArrowSpeedMultiplierStat;
                default: return 1;
            }
        }

        public virtual void Init()
        {
            var heroLevel = GameController.ArmoryManager.GetEquippedHeroLevel();

            var itemTypes = new List<ItemType>((ItemType[])Enum.GetValues(typeof(ItemType)));

            var maxHP = heroLevel.GetStatValue(StatType.HP);
            var damage = heroLevel.GetStatValue(StatType.Damage);

            var movementSpeed = heroLevel.GetStatValue(StatType.MovementSpeed);
            var attackSpeed = heroLevel.GetStatValue(StatType.AttackSpeed);
            var damageReduction = heroLevel.GetStatValue(StatType.DamageReduction);

            var criticalChance = heroLevel.GetStatValue(StatType.CriticalChance);
            var hpRecovery = heroLevel.GetStatValue(StatType.HPRecovery);

            for (int i = 0; i < itemTypes.Count; i++)
            {
                var itemType = itemTypes[i];
                var itemData = GameController.ArmoryManager.GetEquippedItemData(itemType);
                var itemSave = GameController.ArmoryManager.GetEquippedItemSave(itemType);

                if (itemData == null || itemSave == null) continue;

                var itemLevel = itemData.GetItemLevel(itemSave.Level);

                if (itemLevel == null) continue;

                maxHP += itemLevel.GetStatValue(StatType.HP);
                damage += itemLevel.GetStatValue(StatType.Damage);

                movementSpeed *= itemLevel.GetStatValue(StatType.MovementSpeed);
                attackSpeed *= itemLevel.GetStatValue(StatType.AttackSpeed);
                damageReduction *= itemLevel.GetStatValue(StatType.DamageReduction);

                criticalChance += itemLevel.GetStatValue(StatType.CriticalChance);
                hpRecovery += itemLevel.GetStatValue(StatType.HPRecovery);
            }

            MaxHPStat.ChangeInitialValue(maxHP);
            AttackDamageStat.ChangeInitialValue(damage);

            MovementSpeedStat.ChangeInitialValue(movementSpeed);
            AttackSpeedStat.ChangeInitialValue(attackSpeed);
            DamageReduction.ChangeInitialValue(damageReduction);

            CritChanceStat.AddAdder((int)criticalChance);
            HPRecoveryStat.AddAdder((int)hpRecovery);

            FrontArrowDamageStat.AddChildStat(AttackDamageStat);
            RearArrowDamageStat.AddChildStat(AttackDamageStat);
            DiagonalArrowDamageStat.AddChildStat(AttackDamageStat);

            CritDamageMultiplierStat.AddMultiplier(heroLevel.GetStatValue(StatType.DamageReduction));

            MovementSpeedStat.AddMultiplier(SlowDownMultiplier);
        }
    }
}