using MrX.Abilities;
using MrX.Armory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class HeroPageBehavior : MainMenuPageBehavior
    {
        [SerializeField] private Button[] slots;
        [SerializeField] private RectTransform details;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private AbilitiesDatabase abilitiesDatabase;

        private ArmoryManager Armory => GameController.ArmoryManager;
        private HeroData selected;

        protected override void Start()
        {
            base.Start();
            closeButton.onClick.AddListener(Close);
            selectButton.onClick.AddListener(SelectHero);
            upgradeButton.onClick.AddListener(UpgradeHero);
            Close();
            RefreshSlots();
        }

        public override void MoveCenter()
        {
            RefreshSlots();
            base.MoveCenter();
        }

        public override void MoveLeft()
        {
            Close();
            base.MoveLeft();
        }

        public override void MoveRight()
        {
            Close();
            base.MoveRight();
        }

        private void RefreshSlots()
        {
            var heroes = Armory.GetAllHeroDataList();
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                slot.gameObject.SetActive(i < heroes.Count);
                if (i >= heroes.Count) continue;

                var hero = heroes[i];
                slot.onClick.RemoveAllListeners();
                slot.onClick.AddListener(() => Show(hero));
                SetText(slot.transform, "Text_Name", hero.Name);
                SetText(slot.transform, "Text_Level", $"{GetLevel(hero) + 1}");
                SetText(slot.transform, "Text_Slider", GetShardProgress(hero));
                SetPortrait(slot.transform, "Character", hero.Icon);
            }
        }

        private int GetLevel(HeroData hero) =>
            Mathf.Clamp(Armory.GetHeroSave(hero.Id)?.Level ?? 0, 0, hero.HeroLevelCount - 1);

        private void Show(HeroData hero)
        {
            selected = hero;
            int index = GetLevel(hero);
            var level = hero.GetHeroLevel(index);
            var left = details.Find("Group_Left");
            SetText(left, "Text_Lv", (index + 1).ToString());
            SetText(left, "Text_Exp", GetShardProgress(hero));
            upgradeButton.interactable = Armory.GetHeroSave(hero.Id)?.CanUpgradeWithShards(hero.HeroLevelCount) == true;
            SetText(left, "Text_Rank", level.HeroRarity.ToString());
            SetText(left, "Text_Info", hero.Name);
            SetPortrait(details.Find("Group_Center/Character_Panel"), "Character", hero.Icon);

            var stats = details.Find("Group_Center/Stats");
            SetStat(stats.Find("Attack"), "Attack", "ATK", level.GetStatValue(StatType.Damage));
            SetStat(stats.Find("Health"), "Text_Value", "HP", level.GetStatValue(StatType.HP));
            SetStat(stats.Find("Damage"), "Text_Value", "CRIT", level.GetStatValue(StatType.CriticalChance), "%");
            SetStat(stats.Find("MoveSpeed"), "Text_Value", "SPD", level.GetStatValue(StatType.MovementSpeed));
            RefreshAbilities(hero, index, level);

            bool unlocked = Armory.GetHeroSave(hero.Id)?.IsUnlocked == true;
            bool equipped = Armory.EquippedHero == hero;
            selectButton.interactable = unlocked && !equipped;
            SetText(selectButton.transform, "Text_Select", equipped ? "Selected" : unlocked ? "Select" : "Locked");
            details.gameObject.SetActive(true);
            details.SetAsLastSibling();
            GameController.MainMenuScreenBehavior.DisableDock();
        }

        private void RefreshAbilities(HeroData hero, int index, HeroLevel level)
        {
            var parent = details.Find("Group_Right/Ability");
            var seen = new System.Collections.Generic.HashSet<AbilityType>();
            int rowIndex = 0;
            for (int i = 0; i < hero.HeroLevelCount && rowIndex < parent.childCount; i++)
            {
                var skills = hero.GetHeroLevel(i).AttachedAbilities;
                if (skills == null) continue;
                foreach (var skill in skills)
                {
                    if (rowIndex >= parent.childCount || !seen.Add(skill)) continue;
                    var row = parent.GetChild(rowIndex++);
                    var ability = abilitiesDatabase.GetAbility(skill);
                    row.gameObject.SetActive(true);
                    SetText(row, "Text_Stats", ability != null ? ability.Title : skill.ToString());
                    SetText(row, "Text_Value", level.AttachedAbilities?.Contains(skill) == true
                        ? "Active" : i > index ? $"Lv. {i + 1} · Locked" : $"Lv. {i + 1}");
                    var icon = row.Find("Icon");
                    if (icon != null) icon.gameObject.SetActive(ability != null && ability.Icon != null);
                    if (ability != null) SetPortrait(row, "Icon", ability.Icon);
                }
            }

            for (int i = rowIndex; i < parent.childCount; i++)
                parent.GetChild(i).gameObject.SetActive(false);
        }

        private string GetShardProgress(HeroData hero)
        {
            var save = Armory.GetHeroSave(hero.Id);
            if (save?.IsUnlocked != true) return "Locked";
            return GetLevel(hero) >= hero.HeroLevelCount - 1
                ? "MAX" : $"{save.Shards} / {save.ShardsToUpgrade}";
        }

        private void UpgradeHero()
        {
            if (selected == null ||
                Armory.GetHeroSave(selected.Id)?.TryUpgradeWithShards(selected.HeroLevelCount) != true) return;
            GameController.SaveManager.Save(false);
            RefreshSlots();
            Show(selected);
        }

        private void SelectHero()
        {
            if (selected == null) return;
            var save = Armory.GetHeroSave(selected.Id);
            if (save?.IsUnlocked != true || Armory.EquippedHero == selected) return;
            save.IsEquipped = true;
            GameController.SaveManager.Save(false);
            Show(selected);
        }

        private void Close()
        {
            if (!details.gameObject.activeSelf) return;
            details.gameObject.SetActive(false);
            GameController.MainMenuScreenBehavior.EnableDock();
        }

        private static void SetStat(Transform row, string valueName, string label, float value, string suffix = "")
        {
            SetText(row, "Text_Stats", label);
            SetText(row, valueName, value.ToString("0.##") + suffix);
        }

        private static void SetText(Transform root, string name, string value)
        {
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                if (text.name == name) text.text = value;
        }

        private static void SetPortrait(Transform root, string name, Sprite sprite)
        {
            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.name != name || sprite == null) continue;
                image.sprite = sprite;
                image.preserveAspect = true;
                break;
            }
        }
    }
}
