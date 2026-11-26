using MrX.Abilities;
using MrX.Armory;
using MrX.Currency;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        [SerializeField] private RectTransform upgradePopupRoot;

        private ArmoryManager Armory => GameController.ArmoryManager;
        private HeroData selected;
        private HeroUpgradePopup upgradePopup;
        private int upgradeFromLevel = -1;
        private CurrencySave observedUpgradeCurrency;
        private InputAction cancelUpgradeAction;
        private bool wasPageInteractable;

        protected override void Start()
        {
            base.Start();
            closeButton.onClick.AddListener(Close);
            selectButton.onClick.AddListener(SelectHero);
            upgradeButton.onClick.AddListener(OpenUpgradePopup);
            if (upgradePopupRoot)
            {
                upgradePopup = new HeroUpgradePopup(upgradePopupRoot, UpgradeHero, () => CloseUpgradePopup());
                upgradePopup.Hide();
            }
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
                var save = Armory.GetHeroSave(hero.Id);
                slot.onClick.RemoveAllListeners();
                slot.onClick.AddListener(() => Show(hero));
                SetText(slot.transform, "Text_Name", hero.Name);
                SetText(slot.transform, "Text_Level", $"{GetLevel(hero, save) + 1}");
                BindShardProgress(slot.transform, "Text_Slider", hero, save);
                SetPortrait(slot.transform, "Character", hero.Icon);
            }
        }

        private static int GetLevel(HeroData hero, HeroSaveData save) =>
            Mathf.Clamp(save?.Level ?? 0, 0, hero.HeroLevelCount - 1);

        private void Show(HeroData hero)
        {
            selected = hero;
            var save = Armory.GetHeroSave(hero.Id);
            int index = GetLevel(hero, save);
            var level = hero.GetHeroLevel(index);
            bool unlocked = save?.IsUnlocked == true;
            var left = details.Find("Group_Left");
            SetText(left, "Text_Lv", (index + 1).ToString());
            BindShardProgress(left, "Text_Exp", hero, save, " shards");
            RefreshUpgradeButton(hero, save);
            SetText(left, "Text_Rank", level.HeroRarity.ToString());
            SetText(left, "Text_Info", hero.Name);
            SetPortrait(details.Find("Group_Center/Character_Panel"), "Character", hero.Icon);

            var stats = details.Find("Group_Center/Stats");
            SetStat(stats.Find("Attack"), "Attack", "ATK", level.GetStatValue(StatType.Damage));
            SetStat(stats.Find("Health"), "Text_Value", "HP", level.GetStatValue(StatType.HP));
            SetStat(stats.Find("Damage"), "Text_Value", "CRIT", level.GetStatValue(StatType.CriticalChance), "%");
            SetStat(stats.Find("MoveSpeed"), "Text_Value", "SPD", level.GetStatValue(StatType.MovementSpeed));
            RefreshAbilities(hero, index, level);

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

        private static void BindShardProgress(Transform root, string label, HeroData hero, HeroSaveData save, string suffix = "")
        {
            float progress = 0f;
            string text = "Locked";
            if (save?.IsUnlocked == true)
            {
                if (GetLevel(hero, save) >= hero.HeroLevelCount - 1)
                {
                    progress = 1f;
                    text = "MAX";
                }
                else
                {
                    progress = Mathf.Clamp01((float)save.Shards / save.ShardsToUpgrade);
                    text = $"{save.Shards} / {save.ShardsToUpgrade}{suffix}";
                }
            }

            SetText(root, label, text);
            var slider = root.GetComponentInChildren<Slider>(true);
            if (slider == null) return;

            slider.interactable = false;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(Mathf.Lerp(slider.minValue, slider.maxValue, progress));
        }

        private static bool HasNextLevel(HeroData hero, HeroSaveData save) =>
            save?.IsUnlocked == true && save.Level >= 0 && save.Level < hero.HeroLevelCount - 1;

        private static Price GetUpgradePrice(HeroData hero, HeroSaveData save) =>
            HasNextLevel(hero, save) ? hero.GetHeroLevel(save.Level + 1).Price : null;

        private static CurrencySave GetUpgradeCurrency(Price price) =>
            price != null && price.Amount >= 0 && !string.IsNullOrEmpty(price.CurrencyId)
                ? GameController.CurrenciesManager.GetCurrency(price.CurrencyId, false)
                : null;

        private void RefreshUpgradeButton(HeroData hero, HeroSaveData save)
        {
            var price = GetUpgradePrice(hero, save);
            var currency = GetUpgradeCurrency(price);
            bool canPreview = upgradePopup != null && HasNextLevel(hero, save);
            var label = save?.IsUnlocked != true ? "Locked"
                : GetLevel(hero, save) >= hero.HeroLevelCount - 1 ? "MAX" : "Upgrade";
            if (currency != null)
                label = $"{price.Amount:N0} {currency.Data.Name}";

            SetText(upgradeButton.transform, "Text_Gold", label);
            upgradeButton.interactable = canPreview;
        }

        private void OpenUpgradePopup()
        {
            if (upgradePopup == null || upgradeFromLevel >= 0 || !selected || !details.gameObject.activeInHierarchy) return;
            var save = Armory.GetHeroSave(selected.Id);
            if (!HasNextLevel(selected, save)) return;

            var price = GetUpgradePrice(selected, save);
            observedUpgradeCurrency = GetUpgradeCurrency(price);
            upgradeFromLevel = save.Level;
            wasPageInteractable = canvasGroup.interactable;
            canvasGroup.interactable = false;
            upgradePopup.Show(selected, upgradeFromLevel, save, price, observedUpgradeCurrency);
            if (observedUpgradeCurrency != null)
                observedUpgradeCurrency.OnAmountChanged += OnUpgradeBalanceChanged;
            if (GameController.InputManager?.InputAsset != null)
            {
                cancelUpgradeAction = GameController.InputManager.InputAsset.UI.Cancel;
                cancelUpgradeAction.performed += OnUpgradeCancel;
            }
        }

        private void OnUpgradeBalanceChanged(CurrencySave currency)
        {
            var save = Armory.GetHeroSave(selected.Id);
            if (save?.Level == upgradeFromLevel)
                upgradePopup.BindCosts(GetUpgradePrice(selected, save), currency, save, selected.HeroLevelCount);
        }

        private void OnUpgradeCancel(InputAction.CallbackContext context) => CloseUpgradePopup();

        private void CloseUpgradePopup(bool restoreSelection = true)
        {
            if (upgradeFromLevel < 0) return;
            upgradeFromLevel = -1;
            if (observedUpgradeCurrency != null) observedUpgradeCurrency.OnAmountChanged -= OnUpgradeBalanceChanged;
            observedUpgradeCurrency = null;
            if (cancelUpgradeAction != null) cancelUpgradeAction.performed -= OnUpgradeCancel;
            cancelUpgradeAction = null;
            upgradePopup.Hide();
            canvasGroup.interactable = wasPageInteractable;
            if (restoreSelection && EventSystem.current)
                EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        private void OnDisable() => CloseUpgradePopup(false);

        private void UpgradeHero()
        {
            var hero = selected;
            if (upgradeFromLevel < 0 || !hero || !details.gameObject.activeInHierarchy) return;

            var save = Armory.GetHeroSave(hero.Id);
            if (save?.Level != upgradeFromLevel)
            {
                CloseUpgradePopup(false);
                Show(hero);
                return;
            }
            var price = GetUpgradePrice(hero, save);
            var currency = GetUpgradeCurrency(price);
            if (currency == null || !currency.CanAfford(price.Amount) || !save.CanUpgradeWithShards(hero.HeroLevelCount))
            {
                upgradePopup.BindCosts(price, currency, save, hero.HeroLevelCount);
                return;
            }

            bool upgraded = false;
            if (!currency.TryWithdrawAndCommit(price.Amount, () =>
            {
                upgraded = save.TryUpgradeWithShards(hero.HeroLevelCount);
                if (!upgraded) currency.Deposit(price.Amount);
            }) || !upgraded)
            {
                upgradePopup.BindCosts(price, currency, save, hero.HeroLevelCount);
                return;
            }

            GameController.SaveManager.Save(false);
            CloseUpgradePopup(false);
            RefreshSlots();
            Show(hero);
            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject((upgradeButton.interactable ? upgradeButton : closeButton).gameObject);
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
            CloseUpgradePopup(false);
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
