using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class ComboTextIndicatorBehavior : TextIndicatorBehavior
    {
        [SerializeField] protected RectTransform gradientRect;

        protected bool canHide = false;

        protected float widthDifference;

        protected virtual void Awake()
        {
            textComponent.ForceMeshUpdate();
            widthDifference = textComponent.preferredWidth - gradientRect.sizeDelta.x;
        }

        protected override void Init(WorldSpaceIndicatorData data, Vector2 viewportPos, string text)
        {
            base.Init(data, viewportPos, text);

            canHide = false;
        }

        public virtual void Hide()
        {
            canHide = true;
            spawnTime = Time.time - Duration / 2;
        }

        public override void SetText(string text)
        {
            base.SetText(text);

            textComponent.ForceMeshUpdate();
            gradientRect.SetSizeDeltaX(textComponent.preferredWidth - widthDifference);
        }

        protected override void Update()
        {
            if (canHide && Time.time > spawnTime + Duration)
            {
                gameObject.SetActive(false);

                onIndicatorHidden?.Invoke(this);

                return;
            }

            var t = (Time.time - spawnTime) / Duration;
            if (!canHide && t > 0.5f)
            {
                t = 0.5f;
            }

            SetPosition(SpawnPostion + Vector2.up * Data.YPositionCurve.Evaluate(t) * MaxY + Vector2.right * Data.XPositionCurve.Evaluate(t) * MaxX);
            SetScale(Vector3.one * Data.ScaleCurve.Evaluate(t) * MaxScale);

            if (FixToTarget)
            {
                SetAnchors(Camera.main.WorldToViewportPoint(Target.position + TargetOffset));
            }
            else
            {
                SetAnchors(Camera.main.WorldToViewportPoint(WorldPosition));
            }

        }
    }
}