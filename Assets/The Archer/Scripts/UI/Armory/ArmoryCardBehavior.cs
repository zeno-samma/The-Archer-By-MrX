using OctoberStudio.Armory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public abstract class ArmoryCardBehavior : MonoBehaviour
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Image rarityImage;
        [SerializeField] protected Image rarityBackgroundImage;

        [Space]
        [SerializeField] protected TMP_Text levelText;

        [Space]
        [SerializeField] protected Button button;

        [Space]
        [SerializeField] protected List<RarityData> rarityDataList;

        public Selectable Selectable => button;

        protected virtual void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        protected abstract void OnButtonClicked();

        public virtual void SetNavigation(Selectable selectOnLeft, Selectable selectOnRight, Selectable selectOnUp, Selectable selectOnDown)
        {
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;

            navigation.selectOnRight = selectOnRight;
            navigation.selectOnLeft = selectOnLeft;
            navigation.selectOnUp = selectOnUp;
            navigation.selectOnDown = selectOnDown;

            button.navigation = navigation;
        }

        protected virtual void InitRarityVisuals(ItemRarityType rarityType)
        {
            var rarityData = GetRarityData(rarityType);

            rarityImage.sprite = rarityData.backgroundSprite;
            rarityBackgroundImage.color = GameController.ArmoryManager.GetItemRarityData(rarityType).SecondaryColor;
        }

        protected virtual RarityData GetRarityData(ItemRarityType rarityType)
        {
            for (int i = 0; i < rarityDataList.Count; i++)
            {
                if (rarityDataList[i].rarityType == rarityType)
                {
                    return rarityDataList[i];
                }
            }

            return null;
        }

        public virtual void Clear()
        {

        }

        [System.Serializable]
        protected class RarityData
        {
            public ItemRarityType rarityType;
            public Sprite backgroundSprite;
        }
    }
}