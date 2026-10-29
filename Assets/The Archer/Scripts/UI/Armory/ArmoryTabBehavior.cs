using MrX.Easing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MrX.UI
{
    public class ArmoryTabBehavior : MonoBehaviour
    {
        private readonly UIAnimationDefaults animationDefaults = new UIAnimationDefaults();
        [SerializeField] protected Button button;

        [Header("Background")]
        [SerializeField] protected Image topBackgroundImage;
        [SerializeField] protected Image bottomBackgroundImage;

        [Space]
        [SerializeField] protected Color selectedBackgroundColor;
        [SerializeField] protected Color deselectedBackgroundColor;

        [Header("Content")]
        [SerializeField] protected List<Graphic> contentGraphics;

        [Space]
        [SerializeField] protected Color selectedContentColor;
        [SerializeField] protected Color deselectedContentColor;

        public bool IsSelected { get; protected set; }

        public event UnityAction onTabSelected;

        protected List<IEasingCoroutine> contentEasingCoroutines = new List<IEasingCoroutine>();
        protected IEasingCoroutine scaleEasingCoroutine;

        protected virtual void Awake()
        {
            animationDefaults.Capture(topBackgroundImage.rectTransform);
            button.onClick.AddListener(OnClicked);
        }

        public virtual void Init(bool isSelected)
        {
            IsSelected = isSelected;

            for (int i = 0; i < contentGraphics.Count; i++)
            {
                var graphic = contentGraphics[i];
                graphic.color = IsSelected ? selectedContentColor : deselectedContentColor;
            }

            topBackgroundImage.color = IsSelected ? selectedBackgroundColor : deselectedBackgroundColor;
            bottomBackgroundImage.color = IsSelected ? deselectedBackgroundColor : selectedBackgroundColor;
            bottomBackgroundImage.enabled = false;

            button.enabled = !IsSelected;
        }

        public virtual void Select()
        {
            IsSelected = true;

            StopEasingCoroutines();

            for (int i = 0; i < contentGraphics.Count; i++)
            {
                var graphic = contentGraphics[i];
                var easingCoroutine = graphic.DoColor(selectedContentColor, 0.2f);

                contentEasingCoroutines.Add(easingCoroutine);
            }

            bottomBackgroundImage.enabled = true;
            bottomBackgroundImage.color = deselectedBackgroundColor;

            topBackgroundImage.color = selectedBackgroundColor;
            topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform, new Vector3(0, 0.7f, 1f));
            scaleEasingCoroutine = topBackgroundImage.rectTransform.DoLocalScale(animationDefaults.Scale(topBackgroundImage.rectTransform), 0.2f).SetEasing(EasingType.CubicOut).SetOnFinish(OnSelectFinished);

            button.enabled = false;
        }

        public virtual void Deselect()
        {
            IsSelected = false;

            StopEasingCoroutines();

            for (int i = 0; i < contentGraphics.Count; i++)
            {
                var graphic = contentGraphics[i];
                var easingCoroutine = graphic.DoColor(deselectedContentColor, 0.2f);

                contentEasingCoroutines.Add(easingCoroutine);
            }

            bottomBackgroundImage.enabled = true;
            bottomBackgroundImage.color = deselectedBackgroundColor;

            topBackgroundImage.color = selectedBackgroundColor;
            topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform);
            scaleEasingCoroutine = topBackgroundImage.rectTransform.DoLocalScale(animationDefaults.Scale(topBackgroundImage.rectTransform, new Vector3(0, 0.7f, 1f)), 0.2f).SetEasing(EasingType.CubicOut).SetOnFinish(OnDeselectFinished);

            button.enabled = true;
        }

        protected virtual void StopEasingCoroutines()
        {
            for (int i = 0; i < contentEasingCoroutines.Count; i++)
            {
                contentEasingCoroutines[i].StopIfExists();
            }
            contentEasingCoroutines.Clear();

            scaleEasingCoroutine.StopIfExists();
        }

        protected virtual void OnSelectFinished()
        {
            topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform);
            topBackgroundImage.color = selectedBackgroundColor;

            bottomBackgroundImage.enabled = false;
        }

        protected virtual void OnDeselectFinished()
        {
            topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform);
            topBackgroundImage.color = deselectedBackgroundColor;

            bottomBackgroundImage.enabled = false;
        }

        protected virtual void OnClicked()
        {
            onTabSelected?.Invoke();
        }
    }
}
