using MrX.Abilities;
using MrX.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace MrX
{
    public class HealthChestBehavior : ChestBehavior
    {
        [SerializeField] protected List<AbilityType> firstSlotAbilities;
        [SerializeField] protected List<AbilityType> secondSlotAbilities;

        protected override void OpenChestUI()
        {
            if (firstSlotAbilities == null || firstSlotAbilities.Count == 0) { Hide(); return; }
            var firstAbilityData = StageController.AbilitiesManager.AbilitiesDatabase.GetAbility(firstSlotAbilities.Random());
            if (firstAbilityData == null) { Hide(); return; }
            var secondAbilityData = StageController.AbilitiesManager.AbilitiesDatabase.GetAbility(SelectSecondAbility(firstAbilityData.AbilityType)) ?? firstAbilityData;

            StageController.GameScreen.HPChestUI.Show(firstAbilityData, secondAbilityData, Hide);
        }

        protected AbilityType SelectSecondAbility(AbilityType first)
        {
            if (secondSlotAbilities == null) return first;
            int alternatives = 0;
            foreach (var ability in secondSlotAbilities) if (ability != first) alternatives++;
            if (alternatives == 0) return first;
            int selected = Random.Range(0, alternatives);
            foreach (var ability in secondSlotAbilities)
                if (ability != first && selected-- == 0) return ability;
            return first;
        }
    }
}
