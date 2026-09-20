using OctoberStudio.Armory;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class HeroCardBehavior : ArmoryCardBehavior
    {
        [SerializeField] protected bool showCheckmark;
        [SerializeField] protected GameObject selectedCheckmark;

        [Space]
        [SerializeField] protected GameObject lockedStateObject;
        [SerializeField] protected Image lockedStateBackgroundImage;

        public HeroData HeroData { get; protected set; }
        public HeroSaveData SaveData { get; protected set; }

        public HeroLevel HeroLevel { get; protected set; }

        public RectTransform RectTransform { get; protected set; }

        protected override void Awake()
        {
            base.Awake();

            RectTransform = GetComponent<RectTransform>();
        }

        public virtual void SetData(HeroData data, HeroSaveData saveData)
        {
            HeroData = data;
            SaveData = saveData;

            iconImage.sprite = data.Icon;

            var level = saveData != null ? saveData.Level : 0;

            HeroLevel = data.GetHeroLevel(level);

            levelText.text = (level + 1).ToString();

            InitRarityVisuals(HeroLevel.HeroRarity);

            selectedCheckmark.SetActive(showCheckmark && saveData.IsEquipped);

            saveData.OnHeroSelected += OnHeroSelected;
            saveData.OnHeroLevelChanged += OnHeroLevelChanged;
        }

        protected virtual void OnHeroSelected(HeroSaveData saveData)
        {
            selectedCheckmark.SetActive(showCheckmark && saveData.IsEquipped);
        }

        protected virtual void OnHeroLevelChanged(HeroSaveData saveData, int level)
        {
            HeroLevel = HeroData.GetHeroLevel(level);
            levelText.text = (level + 1).ToString();
            InitRarityVisuals(HeroLevel.HeroRarity);
        }

        protected override void InitRarityVisuals(ItemRarityType rarityType)
        {
            base.InitRarityVisuals(rarityType);

            lockedStateObject.gameObject.SetActive(!SaveData.IsUnlocked);
            rarityBackgroundImage.gameObject.SetActive(SaveData.IsUnlocked);
            lockedStateBackgroundImage.color = GameController.ArmoryManager.GetItemRarityData(rarityType).MainColor;
            lockedStateBackgroundImage.SetAlpha(0.6f);
        }

        public override void Clear()
        {
            base.Clear();

            if(SaveData != null)
            {
                SaveData.OnHeroSelected -= OnHeroSelected;
                SaveData.OnHeroLevelChanged -= OnHeroLevelChanged;
            }
        }

        protected override void OnButtonClicked()
        {
            GameController.MainMenuScreenBehavior.ExpandedHeroPopup.Show(HeroData, SaveData);
        }
    }
}