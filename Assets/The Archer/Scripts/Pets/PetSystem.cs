using System;
using UnityEngine;

namespace MrX.Pets
{
    public static class PetSystem
    {
        private const string SaveKey = "ArcBorn Pet";

        public static event Action Changed;

        public static PetDefinition Definition { get; private set; }

        internal static void Configure(PetDefinition definition)
        {
            if (definition == null || Definition == definition) return;

            Definition = definition;
            Changed?.Invoke();
        }

        public static PetProfileSave Profile
        {
            get
            {
                var config = Definition;
                if (config == null || GameController.SaveManager == null) return null;

                var profile = GameController.SaveManager.GetSave<PetProfileSave>(SaveKey);
                profile?.Initialize(config.StartingCakes);
                return profile;
            }
        }

        public static bool TryDeploy()
        {
            if (Definition == null || Definition.CombatAbility == null || Profile?.Deploy() != true)
                return false;

            SaveAndNotify();
            return true;
        }

        public static bool TryRecall()
        {
            if (Profile?.Recall() != true) return false;

            SaveAndNotify();
            return true;
        }

        public static bool TryFeed()
        {
            var profile = Profile;
            if (profile == null || !profile.Feed(Definition.CakesPerFeeding)) return false;

            SaveAndNotify();
            return true;
        }

        public static bool GrantCakes(int amount)
        {
            if (Profile?.AddCakes(amount) != true) return false;

            SaveAndNotify();
            return true;
        }

        public static bool AttachToRun()
        {
            if (Profile?.IsDeployed != true) return false;

            var ability = Definition.CombatAbility;
            var abilities = StageController.AbilitiesManager;
            if (ability == null || abilities == null) return false;

            if (!abilities.IsAbilityAquired(ability.AbilityType))
                abilities.AddAbility(ability);

            return abilities.IsAbilityAquired(ability.AbilityType);
        }

        // Call before FinalizeRunRewards so the stage save batch includes the HP loss.
        public static void RecordRunDefeat()
        {
            if (Profile?.ApplyDefeat() == true)
                Changed?.Invoke();
        }

        private static void SaveAndNotify()
        {
            GameController.SaveManager.Save(false);
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Definition = null;
            Changed = null;
        }
    }
}
