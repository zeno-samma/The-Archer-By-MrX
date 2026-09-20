using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class ScalableAnimatedTitleUI : AnimatedTitleUI
    {
        [SerializeField] protected List<RectTransform> scalablePosition;
        [SerializeField] protected List<RectTransform> scalableSize;

        protected Dictionary<RectTransform, Vector2> initialScalablePosition = new Dictionary<RectTransform, Vector2>();
        protected Dictionary<RectTransform, Vector2> initialScalableSize = new Dictionary<RectTransform, Vector2>();

        protected float initialPrefferedWidth;
        protected Vector2 initialTitleLinePosition;

        protected override void Awake()
        {
            base.Awake();

            initialPrefferedWidth = titleText.preferredWidth;
            initialTitleLinePosition = titleLinePosition;

            for (int i = 0; i < scalablePosition.Count; i++)
            {
                initialScalablePosition.Add(scalablePosition[i], scalablePosition[i].anchoredPosition);
            }

            for (int i = 0; i < scalableSize.Count; i++)
            {
                initialScalableSize.Add(scalableSize[i], scalableSize[i].sizeDelta);
            }
        }

        public override void SetText(string text)
        {
            base.SetText(text);

            EasingManager.DoNextFrame(RecalculateSizesAndScales);
        }

        public override void RestoreText()
        {
            base.RestoreText();

            EasingManager.DoNextFrame(RecalculateSizesAndScales);
        }

        protected virtual void RecalculateSizesAndScales()
        {
            var preferredWidth = titleText.preferredWidth;

            var difference = preferredWidth - initialPrefferedWidth;

            foreach (var scalable in scalablePosition)
            {
                if (initialScalablePosition.TryGetValue(scalable, out var initialPosition))
                {
                    if (initialPosition.x > 0)
                    {
                        scalable.anchoredPosition = initialPosition + Vector2.right * difference / 2;
                    }
                    else
                    {
                        scalable.anchoredPosition = initialPosition + Vector2.left * difference / 2;
                    }
                }
            }

            foreach (var scalable in scalableSize)
            {
                if (initialScalableSize.TryGetValue(scalable, out var initialSize))
                {
                    scalable.sizeDelta = initialSize + Vector2.right * difference;
                }
            }

            if (initialTitleLinePosition.x > 0)
            {
                titleLinePosition = initialTitleLinePosition + Vector2.right * difference / 2;
            }
            else
            {
                titleLinePosition = initialTitleLinePosition + Vector2.left * difference / 2;
            }

            if (lineEasingCoroutine.ExistsAndActive())
            {
                ShowAnimatedLine();
            }
        }
    }
}