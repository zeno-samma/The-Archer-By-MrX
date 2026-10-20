using TMPro;
using UnityEngine;

namespace MrX.UI
{
    [DefaultExecutionOrder(60)]
    public sealed class StageHeroIdentity : MonoBehaviour
    {
        [SerializeField] private TMP_Text heroLabel;
        private ArmoryManager armory;
        private HeroSaveData heroSave;
        private void Start()
        {
            armory = GameController.ArmoryManager;
            if (armory == null) return;
            armory.OnSelectedHeroChanged += Bind;
            Bind();
        }
        private void Bind()
        {
            if (heroSave != null) heroSave.OnHeroLevelChanged -= OnLevel;
            var hero = armory.EquippedHero;
            heroSave = hero != null ? armory.GetHeroSave(hero.Id) : null;
            if (heroSave != null) heroSave.OnHeroLevelChanged += OnLevel;
            Refresh();
        }
        private void OnLevel(HeroSaveData save, int level) => Refresh();
        private void Refresh()
        {
            if (heroLabel && armory.EquippedHero != null)
                heroLabel.text = armory.EquippedHero.Name + "  Lv." + ((heroSave?.Level ?? 0) + 1);
        }
        private void OnDestroy()
        {
            if (armory != null) armory.OnSelectedHeroChanged -= Bind;
            if (heroSave != null) heroSave.OnHeroLevelChanged -= OnLevel;
        }
    }
}
