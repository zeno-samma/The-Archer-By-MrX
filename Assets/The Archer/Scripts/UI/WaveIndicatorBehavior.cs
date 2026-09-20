using OctoberStudio.Easing;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class WaveIndicatorBehavior : MonoBehaviour
    {
        [SerializeField] protected Image waveIconImage;
        [SerializeField] protected TMP_Text waveNumberText;
        [SerializeField] protected RectTransform numberRect;

        [SerializeField] protected float growScale = 1.2f;

        public RectTransform RectTransform { get; protected set; }

        List<MaskableGraphic> maskableGraphics = new List<MaskableGraphic>();

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            GetComponentsInChildren(maskableGraphics);
        }

        public virtual void Init(WaveData waveData, int number)
        {
            SetSprite(waveData.Icon);
            SetNumber(number);
        }

        public virtual void SetSprite(Sprite iconSprite)
        {
            waveIconImage.sprite = iconSprite;

        }

        public virtual void SetNumber(int waveNumber)
        {
            waveNumberText.text = waveNumber.ToString();
        }

        public virtual void ShowNumber(bool skipAnimation)
        {
            if (skipAnimation)
            {
                numberRect.localScale = Vector3.one;
            }
            else
            {
                numberRect.DoLocalScale(Vector3.one, 0.2f);
            }
        }

        public virtual void HideNumber(bool skipAnimation)
        {
            if (skipAnimation)
            {
                numberRect.localScale = Vector3.zero;
            }
            else
            {
                numberRect.DoLocalScale(Vector3.zero, 0.2f);
            }
        }

        public virtual void SetMaskable(bool isMaskable)
        {
            foreach (var graphic in maskableGraphics)
            {
                graphic.maskable = isMaskable;
            }
        }

        public virtual void Grow()
        {
            RectTransform.DoLocalScale(Vector3.one * 1.3f, 0.2f);
        }

        public virtual void Shrink()
        {

        }
    }
}