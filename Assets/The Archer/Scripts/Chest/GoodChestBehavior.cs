using OctoberStudio.Abilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio
{
    public class GoodChestBehavior : ChestBehavior
    {
        [SerializeField] protected List<AbilityChanceData> abilityChances;
        [SerializeField] protected string uiTitleText = "Chest Reward";

        protected override void OpenChestUI()
        {
            var rarity = GetRarity();
            StageController.GameScreen.AbiltiesSelector.Show(rarity, uiTitleText, Hide);
        }

        protected AbilityRarity GetRarity()
        {
            var chancesSum = abilityChances.Sum(r => r.Chance);

            var random = Random.value * chancesSum;

            AbilityRarity selectedRarity = AbilityRarity.Common;
            for (int i = 0; i < abilityChances.Count; i++)
            {
                var rarity = abilityChances[i];
                if (random <= rarity.Chance)
                {
                    selectedRarity = rarity.Rarity;
                    break;
                }
                random -= rarity.Chance;
            }

            if (selectedRarity != AbilityRarity.Legendary && Random.value * 100 < StageController.Player.Stats.ChanceToIncreaseRarity)
            {
                selectedRarity = selectedRarity++;
            }

            return selectedRarity;
        }

        [System.Serializable]
        protected class AbilityChanceData
        {
            [SerializeField] protected AbilityRarity rarity;
            [SerializeField] protected float chance;

            public AbilityRarity Rarity => rarity;
            public float Chance => chance;
        }
    }
}