using OctoberStudio.Abilities;
using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class HealthChestBehavior : ChestBehavior
    {
        [SerializeField] protected List<AbilityType> firstSlotAbilities;
        [SerializeField] protected List<AbilityType> secondSlotAbilities;

        protected override void OpenChestUI()
        {
            var firstAbilityData = StageController.AbilitiesManager.AbilitiesDatabase.GetAbility(firstSlotAbilities.Random());

            var secondAbilityType = firstAbilityData.AbilityType;
            var counter = 0;
            while(counter < 20 && secondAbilityType == firstAbilityData.AbilityType)
            {
                secondAbilityType = secondSlotAbilities.Random();
            }

            var secondAbilityData = StageController.AbilitiesManager.AbilitiesDatabase.GetAbility(secondAbilityType);

            StageController.GameScreen.HPChestUI.Show(firstAbilityData, secondAbilityData, Hide);
        }
    }
}