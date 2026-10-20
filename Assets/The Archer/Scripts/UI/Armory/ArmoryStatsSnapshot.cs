using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MrX.Armory;
using MrX.Player;
using MrX.Upgrades;

namespace MrX.UI
{
    public sealed class ArmoryStatsSnapshot
    {
        public PlayerStatsHandler Total { get; private set; }
        public PlayerStatsHandler HeroOnly { get; private set; }
        public float WaveEndHealing { get; private set; }
        private static readonly PropertyInfo[] Properties = typeof(PlayerStatsHandler).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        public static ArmoryStatsSnapshot Calculate(HeroLevel hero, IReadOnlyList<ItemLevel> equipment, IUpgradesManager upgrades)
        {
            var snapshot = new ArmoryStatsSnapshot { Total = new PlayerStatsHandler(), HeroOnly = new PlayerStatsHandler() };
            snapshot.HeroOnly.Init(hero, Array.Empty<ItemLevel>()); snapshot.Total.Init(hero, equipment);
            if (upgrades != null)
                foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
                {
                    var level = upgrades.GetUpgradeLevel(type); var data = upgrades.GetUpgradeData(type);
                    if (level < 0 || data == null || data.LevelsCount == 0) continue;
                    var value = data.GetLevel(Math.Min(level, data.LevelsCount - 1)).Value;
                    var stats = snapshot.Total;
                    // Same units and effects as the nine existing upgrade behaviors.
                    switch (type)
                    {
                        case UpgradeType.MorePower: stats.AttackDamageStat.AddMultiplier(value); break;
                        case UpgradeType.BaseHp: stats.MaxHPStat.AddMultiplier(value); break;
                        case UpgradeType.AttackSpeed: stats.AttackSpeedStat.AddMultiplier(value); break;
                        case UpgradeType.MoveSpeed: stats.MovementSpeedStat.AddMultiplier(value); break;
                        case UpgradeType.Shield: stats.DamageReduction.AddMultiplier(1 - value / 100f); break;
                        case UpgradeType.CritChance: stats.CritChanceStat.AddAdder((int)value); break;
                        case UpgradeType.DodgeChance: stats.DodgeChanceStat.AddAdder((int)value); break;
                        case UpgradeType.Revive: stats.RevivesStat.AddAdder((int)value); break;
                        case UpgradeType.HPRecovery: snapshot.WaveEndHealing = value; break;
                    }
                }
            return snapshot;
        }
        public static ArmoryStatsSnapshot Current(IUpgradesManager upgrades = null)
        {
            var armory = GameController.ArmoryManager;
            var items = new List<ItemLevel>(4);
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                var data = armory.GetEquippedItemData(type); var save = armory.GetEquippedItemSave(type);
                if (data != null && save != null && save.Level >= 0 && save.Level < data.ItemLevelCount) items.Add(data.GetItemLevel(save.Level));
            }
            return Calculate(armory.GetEquippedHeroLevel(), items, upgrades ?? GameController.UpgradesManager);
        }
        public string Details()
        {
            var text = new StringBuilder(3072);
            text.AppendLine("<b>LOADOUT & PERMANENT UPGRADES</b>");
            text.AppendLine("Loadout base stats plus permanent upgrades. Ability effects are resolved in battle. Green bonuses compare against the hero alone.\n");
            foreach (var property in Properties)
            {
                var value = property.GetValue(Total);
                float number;
                if (value is MultiplicativeStat multiplier) number = multiplier.Value;
                else if (value is AdditiveStat additive) number = additive.Value;
                else if (value is StatMultiplier modifier) number = modifier.Value;
                else continue;
                var title = property.Name.Replace("Stat", "");
                var label = new StringBuilder(title.Length + 10);
                for (int i = 0; i < title.Length; i++)
                { if (i > 0 && char.IsUpper(title[i]) && !char.IsUpper(title[i - 1])) label.Append(' '); label.Append(title[i]); }
                var percent = property.Name == "CritChanceStat" || property.Name == "DodgeChanceStat" || property.Name == "ChanceToIncreaseRarity";
                var absolute = property.Name == "MaxHPStat" || property.Name == "AttackDamageStat" || property.Name == "MovementSpeedStat" || property.Name == "FrontArrowDamageStat" || property.Name == "RearArrowDamageStat" || property.Name == "DiagonalArrowDamageStat";
                text.Append(label).Append(": <b>").Append(number.ToString("0.###")).Append(percent ? "%" : value is MultiplicativeStat && !absolute ? "x" : "").AppendLine("</b>");
            }
            text.Append("Wave end healing: <b>").Append(WaveEndHealing.ToString("0.##")).AppendLine("%</b>");
            return text.ToString();
        }
    }
}
