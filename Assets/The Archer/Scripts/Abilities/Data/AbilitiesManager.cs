using OctoberStudio.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class AbilitiesManager : MonoBehaviour
    {
        [SerializeField] protected AbilitiesDatabase abilitiesDatabase;
        public AbilitiesDatabase AbilitiesDatabase => abilitiesDatabase;

        [SerializeField] protected TestingDatabase testingDatabase;

        protected List<IAbilityBehavior> acquiredAbilities = new List<IAbilityBehavior>();

        protected AbilitiesSave save;
        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        protected virtual void Awake()
        {
            save = GameController.SaveManager.GetSave<AbilitiesSave>("Abilities Save");
            save.Init();

            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            // Usualy the data isn't getting reset only if the Player continues the game after they've closed it without dying
            if (!ContinuePlayingSave.HasUnfinishedStageData) save.Clear();
        }

        protected virtual void Start()
        {
            StageController.RegisterAbilitiesManager(this);
        }

        #region Initialization

        public virtual void Init()
        {
            if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                LoadAbilitiesFromSave();
            }

            if (testingDatabase != null)
            {
                for (int i = 0; i < testingDatabase.Presets.Count; i++)
                {
                    var preset = testingDatabase.Presets[i];

#if UNITY_EDITOR
                    if (preset.EnabledInEditor)
#else
                    if (preset.EnabledInBuild)
#endif
                    {
                        AddTestingPresetAbilities(preset);
                    }
                }
            }
            LoadArmoryAbilities();
        }

        protected virtual void LoadAbilitiesFromSave()
        {
            var savedAbilities = save.GetSavedAbilities();
            for (int i = 0; i < savedAbilities.Count; i++)
            {
                AbilityType type = savedAbilities[i];

                AbilityData data = abilitiesDatabase.GetAbility(type);
                AddAbility(data, save.GetAbilityLevel(type));
            }
        }

        protected virtual void AddTestingPresetAbilities(TestingPreset testingPreset)
        {
            for (int i = 0; testingPreset.Abilities.Count > i; i++)
            {
                AbilityType abilityType = testingPreset.Abilities[i].abilityType;

                if (IsAbilityAquired(abilityType)) continue;

                var data = abilitiesDatabase.GetAbility(abilityType);
                var level = testingPreset.Abilities[i].level;
                if (level >= data.LevelsCount) level = data.LevelsCount - 1;
                AddAbility(data, level);
            }
        }

        protected virtual void LoadArmoryAbilities()
        {
            var armoryAbilities = GameController.ArmoryManager.GetEquippedItemAndHeroAbilities();

            for (int i = 0; i < armoryAbilities.Count; i++)
            {
                var abilityType = armoryAbilities[i];
                if (IsAbilityAquired(abilityType)) continue;

                var data = abilitiesDatabase.GetAbility(abilityType);
                AddAbility(data);
            }
        }

        #endregion

        public virtual AbilityData GetAvailableAbility(AbilityRarity rarity)
        {
            var abilities = GetAllAbilitiesOfRarity(rarity);

            abilities = abilities.Filter(IsAbilityAvailable);
            if (abilities.Count == 0)
            {
                abilities = GetAllEndgameAbilitiesOfRarity(rarity);
            }

            return abilities.Random();
        }

        public virtual List<AbilityData> GetAcquiredAbilities()
        {
            return acquiredAbilities.ConvertAll((ability) => ability.AbilityData);
        }

        public virtual List<AbilityData> GetAllAbilitiesOfRarity(AbilityRarity rarity)
        {
            var abilities = new List<AbilityData>();
            var rarityData = abilitiesDatabase.GetRarityData(rarity);

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var ability = abilitiesDatabase.GetAbility(i);
                if (ability.Rarity == rarity)
                {
                    abilities.Add(ability);
                }
            }

            return abilities;
        }

        public virtual List<AbilityData> GetAllEndgameAbilitiesOfRarity(AbilityRarity rarity)
        {
            var abilities = new List<AbilityData>();
            var rarityData = abilitiesDatabase.GetRarityData(rarity);
            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var ability = abilitiesDatabase.GetAbility(i);
                if (ability.Rarity == rarity && ability.IsEndgameAbility)
                {
                    abilities.Add(ability);
                }
            }
            return abilities;
        }

        public virtual List<AbilityData> GetAbilitiesForSelector()
        {
            var rarities = abilitiesDatabase.GetRarities();
            var chancesSum = rarities.Sum(r => r.Chance);

            var random = Random.value * chancesSum;

            AbilityRarity selectedRarity = AbilityRarity.Common;
            for (int i = 0; i < rarities.Count; i++)
            {
                var rarity = rarities[i];
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

            return GetAbilitiesForSelector(selectedRarity);
        }

        public virtual List<AbilityData> GetAbilitiesForSelector(AbilityRarity rarity)
        {
            var abilities = GetAllAbilitiesOfRarity(rarity);
            abilities = abilities.Filter(IsAbilityAvailable);

            var selectedAbilities = new List<AbilityData>();

            while (selectedAbilities.Count < 3 && abilities.Count > 0)
            {
                selectedAbilities.Add(abilities.PopRandom());
            }

            if (selectedAbilities.Count < 3)
            {
                var endgameAbilities = GetAllEndgameAbilitiesOfRarity(rarity);

                while (selectedAbilities.Count < 3 && endgameAbilities.Count > 0)
                {
                    selectedAbilities.Add(endgameAbilities.PopRandom());
                }
            }

            if (selectedAbilities.Count == 0)
            {
                Debug.LogWarning($"No abilities available for selector. Check endgame abilities for {rarity} rarity");
            }

            return selectedAbilities;
        }

        public virtual bool IsAbilityAquired(AbilityType ability)
        {
            for (int i = 0; i < acquiredAbilities.Count; i++)
            {
                if (acquiredAbilities[i].AbilityType == ability) return true;
            }

            return false;
        }

        public virtual bool IsAbilityAvailable(AbilityData ability)
        {
            if (ability.IsEndgameAbility) return false;

            var isAquired = IsAbilityAquired(ability.AbilityType);

            if (!isAquired)
            {
                if (ability.Prerequisites.Count == 0) return true;

                var hasPrerequisite = false;
                for (int i = 0; i < ability.Prerequisites.Count; i++)
                {
                    var prerequisite = ability.Prerequisites[i];

                    if (IsAbilityAquired(prerequisite))
                    {
                        hasPrerequisite = true;
                        break;
                    }
                }

                return hasPrerequisite;
            }

            return (save.GetAbilityLevel(ability.AbilityType) < ability.LevelsCount - 1 || ability.IsRepeatedAbility) && !ability.IsEndgameAbility;
        }

        public virtual void AddAbility(AbilityData abilityData, int level = 0)
        {
            IAbilityBehavior ability = Instantiate(abilityData.Prefab).GetComponent<IAbilityBehavior>();
            ability.Init(abilityData, level);

            save.SetAbilityLevel(abilityData.AbilityType, level);
            acquiredAbilities.Add(ability);
        }

        public virtual void RemoveAbility(AbilityData abilityData)
        {
            for (int i = 0; i < acquiredAbilities.Count; i++)
            {
                var ability = acquiredAbilities[i];

                if (ability.AbilityData == abilityData)
                {
                    ability.Clear();

                    acquiredAbilities.RemoveAt(i);

                    save.RemoveAbility(abilityData.AbilityType);

                    break;
                }
            }
        }

        public virtual void DecreaseAbilityLevel(AbilityData abilityData)
        {
            var level = save.GetAbilityLevel(abilityData.AbilityType);
            if (level > 0)
            {
                save.SetAbilityLevel(abilityData.AbilityType, level - 1);

                for (int i = 0; i < acquiredAbilities.Count; i++)
                {
                    var ability = acquiredAbilities[i];

                    if (ability.AbilityData == abilityData)
                    {
                        abilityData.Upgrade(level - 1);

                        break;
                    }
                }
            }
        }

        public virtual void IncreaseAbilityLevel(AbilityData abilityData)
        {
            var level = save.GetAbilityLevel(abilityData.AbilityType);
            if (level < abilityData.LevelsCount - 1)
            {
                save.SetAbilityLevel(abilityData.AbilityType, level + 1);

                abilityData.Upgrade(level + 1);

            }
            else if (abilityData.IsRepeatedAbility)
            {
                abilityData.Upgrade(level);
            }
        }

        public virtual IAbilityBehavior GetAcquiredAbility(AbilityType abilityType)
        {
            for (int i = 0; i < acquiredAbilities.Count; i++)
            {
                if (acquiredAbilities[i].AbilityType == abilityType)
                {
                    return acquiredAbilities[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public virtual List<AbilityData> GetAllAbilitiesDev()
        {
            var abilities = new List<AbilityData>();

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                abilities.Add(abilitiesDatabase.GetAbility(i));
            }

            return abilities;
        }

        public virtual int GetAbilityLevelDev(AbilityType type)
        {
            return save.GetAbilityLevel(type);
        }
#endif
    }
}