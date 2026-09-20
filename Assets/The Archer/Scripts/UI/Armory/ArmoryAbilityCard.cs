using OctoberStudio.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Armory
{
    public class ArmoryAbilityCard : MonoBehaviour
    {
        [SerializeField] protected Image abilityIconImage;
        [SerializeField] protected TMP_Text abilityNamaText;
        [SerializeField] protected TMP_Text abilityDescriptionText;

        [Space]
        [SerializeField] protected GameObject lockImage;
        [SerializeField] protected TMP_Text lockText;

        protected RectTransform rectTransform;

        public float Height => rectTransform.sizeDelta.y;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void Init(AbilityData data, bool isLocked, int level)
        {
            abilityIconImage.sprite = data.Icon;
            abilityNamaText.text = data.Title;
            abilityDescriptionText.text = data.Description;

            lockImage.SetActive(isLocked);
            if (isLocked)
            {
                lockText.text = $"Lvl {level}";
            }
        }
    }
}