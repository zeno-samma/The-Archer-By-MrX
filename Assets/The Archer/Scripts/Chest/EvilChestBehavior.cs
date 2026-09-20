using OctoberStudio.Abilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio
{
    public class EvilChestBehavior : ChestBehavior
    {
        [SerializeField] protected List<AbilityChanceData> abilityChances;

        protected override void OpenChestUI()
        {
            var abilityData = SelectAbilityChanceData();
            StageController.GameScreen.EvilChestUI.Show(abilityData, Hide);
        }

        public virtual AbilityData SelectAbilityChanceData()
        {
            var chancesSum = abilityChances.Sum(r => r.Chance);

            var random = Random.value * chancesSum;

            var data = abilityChances[0];
            for (int i = 0; i < abilityChances.Count; i++)
            {
                var chanceData = abilityChances[i];
                if (random <= chanceData.Chance)
                {
                    data = chanceData;
                    break;
                }
                random -= chanceData.Chance;
            }

            return StageController.AbilitiesManager.GetAvailableAbility(data.Rarity);
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