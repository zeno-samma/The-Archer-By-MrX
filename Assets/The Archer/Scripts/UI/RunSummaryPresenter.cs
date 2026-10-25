using System.Text;
using MrX.Abilities;
using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace MrX.UI
{
    // References only authored prefab children. The result screen owns pause/timeScale.
    public sealed class RunSummaryPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject details;
        [SerializeField] private Button openButton, closeButton;
        [SerializeField] private TMP_Text title, metrics, build;
        [SerializeField] private RunRecordHighlights highlights;
        private bool bound;
        private ResponsiveMenuLayout layout;
        private bool? portrait;
        public bool IsDetailsOpen=>details!=null&&details.activeInHierarchy;
        private void Awake()
        { openButton.onClick.AddListener(Open); closeButton.onClick.AddListener(Close); layout = GetComponent<ResponsiveMenuLayout>(); bound = true; }
        public void OpenDetails(){Open();}
        private static string T(string source) => MenuLanguage.Text(source);
        private static string Number(double value) => value.ToString("#,0.##");
        private void Open()
        {
            var stats = StageController.RunStatistics;
            if (stats == null) return;
            details.SetActive(true);
            title.text = T(GetComponentInParent<StageCompleteScreen>() != null ? "Victory" : "Defeat") + " · " + T("RUN SUMMARY") + " · " + StageDifficultySelector.Name(StageController.ActiveDifficulty);
            if (highlights != null) highlights.Play(stats, GetComponentInParent<StageCompleteScreen>() != null);
            var text = new StringBuilder();
            text.AppendLine(MenuLanguage.Text(StageController.StageData.StageName));
            Row(text, "Time played", ((int)stats.seconds / 60).ToString("00") + ":" + ((int)stats.seconds % 60).ToString("00"));
            Row(text, "Enemies / Bosses defeated", stats.enemiesKilled + " / " + stats.bossesKilled);
            Row(text, "Damage dealt", Number(stats.damageDealt)); Row(text, "Damage to bosses", Number(stats.bossDamage));
            Row(text, "HP lost / restored", Number(stats.damageTaken) + " / " + Number(stats.healing));
            Row(text, "Hits taken", stats.hitsTaken.ToString()); Row(text, "Critical hits / Hits landed", stats.criticalHits + " / " + stats.hitsLanded);
            Row(text, "Highest hit", Number(stats.highestHit));
            text.AppendLine().AppendLine(T("Damage by type"));
            if (stats.damageTypes != null) foreach (var bucket in stats.damageTypes)
                Row(text, bucket.type.ToString(), Number(bucket.amount) + " (" + (stats.damageDealt > 0 ? bucket.amount * 100 / stats.damageDealt : 0).ToString("0.#") + "%)");
            metrics.text = text.ToString();
            var loadout = new StringBuilder(T("FINAL BUILD")).AppendLine().AppendLine(T("Includes equipment and permanent upgrades."));
            if (StageController.Player != null)
            {
                var s = StageController.Player.Stats;
                Row(loadout, "ATK / HP", s.AttackDamageStat.Value.ToString("0.#") + " / " + s.MaxHPStat.Value.ToString("0.#"));
                Row(loadout, "Attack speed / Critical chance", s.AttackSpeedStat.Value.ToString("0.##") + " / " + s.CritChanceStat.Value.ToString("0.#") + "%");
            }
            loadout.AppendLine().AppendLine(T("Abilities this run"));
            var saved = GameController.SaveManager.GetSave<AbilitiesSave>("Abilities Save");
            foreach (var type in saved.GetSavedAbilities())
            {
                var data = StageController.AbilitiesManager.AbilitiesDatabase.GetAbility(type);
                if (data != null) loadout.Append(T(data.Title)).Append(" · Lv. ").Append(saved.GetAbilityLevel(type) + 1).AppendLine();
            }
            var armory = GameController.ArmoryManager;
            loadout.AppendLine().AppendLine(T("Equipment"));
            loadout.Append(T(armory.EquippedHero.Name)).Append(" · Lv. ").Append(armory.GetHeroSave(armory.EquippedHero.Id).Level + 1).AppendLine();
            foreach (MrX.Armory.ItemType slot in System.Enum.GetValues(typeof(MrX.Armory.ItemType)))
            {
                var item = armory.GetEquippedItemData(slot); var itemSave = armory.GetEquippedItemSave(slot);
                if (item != null && itemSave != null) loadout.Append(T(item.ItemName)).Append(" · Lv. ").Append(itemSave.Level + 1).AppendLine();
            }
            loadout.AppendLine().AppendLine(T("Permanent upgrades"));
            foreach (var upgrade in GameController.UpgradesManager.GetAllUpgrades())
            {
                int level = GameController.UpgradesManager.GetUpgradeLevel(upgrade.UpgradeType);
                if (level >= 0) loadout.Append(T(upgrade.Title)).Append(" · Lv. ").Append(level + 1).AppendLine();
            }
            build.text = loadout.ToString();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
        }
        private static void Row(StringBuilder text, string label, string value) => text.Append(T(label)).Append(": ").Append(value).AppendLine();
        private void Close()
        { details.SetActive(false); if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(openButton.gameObject); }
        private void LateUpdate()
        {
            if (!details.activeSelf) return;
            var rect = (RectTransform)details.transform;
            bool isPortrait = rect.rect.height > rect.rect.width;
            if (layout != null && portrait != isPortrait) { portrait = isPortrait; layout.Apply(isPortrait); }
            if (EventSystem.current != null)
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                if (selected == null || !selected.transform.IsChildOf(details.transform)) EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
            }
            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)) Close();
        }
    }
}
