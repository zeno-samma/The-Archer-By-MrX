using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.UI
{
    public class TextIndicatorBehavior : MonoBehaviour
    {
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected TMP_Text textComponent;

        protected float spawnTime;
        public WorldSpaceIndicatorData Data { get; protected set; }

        public bool FixToTarget { get; set; }

        public Vector3 WorldPosition { get; set; }
        public Transform Target { get; set; }
        public Vector3 TargetOffset { get; set; }

        protected float MaxX { get; set; }
        protected float MaxY { get; set; }
        protected float MaxScale { get; set; }

        protected float Duration { get; set; }

        protected Vector2 SpawnPostion { get; set; }

        public UnityAction<TextIndicatorBehavior> onIndicatorHidden;

        public virtual void Init(WorldSpaceIndicatorData data, string text, Vector3 worldPos)
        {
            FixToTarget = false;

            Target = null;
            TargetOffset = Vector3.zero;
            WorldPosition = worldPos;

            var viewportPos = Camera.main.WorldToViewportPoint(worldPos);
            Init(data, viewportPos, text);
        }

        public virtual void Init(WorldSpaceIndicatorData data, string text, Transform target, Vector3 offset)
        {
            FixToTarget = true;

            Target = target;
            TargetOffset = offset;
            WorldPosition = Vector3.zero;

            var viewportPos = Camera.main.WorldToViewportPoint(target.position + offset);
            Init(data, viewportPos, text);
        }

        protected virtual void Init(WorldSpaceIndicatorData data, Vector2 viewportPos, string text)
        {
            Data = data;

            Duration = data.Duration;
            MaxX = data.MaxX;
            MaxY = data.MaxY;
            MaxScale = data.MaxScale;

            SpawnPostion = new Vector2(Data.SpawnPostionOffsetX, Data.SpawnPositionOffsetY);

            SetAnchors(viewportPos);
            SetPosition(SpawnPostion);

            spawnTime = Time.time;
            textComponent.text = text;
        }

        public virtual void SetText(string text)
        {
            textComponent.text = text;
        }

        public virtual void SetAnchors(Vector2 viewportPosition)
        {
            rectTransform.anchorMin = viewportPosition;
            rectTransform.anchorMax = viewportPosition;
        }

        public virtual void SetPosition(Vector2 position)
        {
            rectTransform.anchoredPosition = position;
        }

        public virtual void SetScale(Vector3 scale)
        {
            rectTransform.localScale = scale;
        }

        protected virtual void Update()
        {
            if (Time.time > spawnTime + Duration)
            {
                gameObject.SetActive(false);

                onIndicatorHidden?.Invoke(this);

                return;
            }

            var t = (Time.time - spawnTime) / Duration;

            SetPosition(SpawnPostion + Vector2.up * Data.YPositionCurve.Evaluate(t) * MaxY + Vector2.right * Data.XPositionCurve.Evaluate(t) * MaxX);
            SetScale(Vector3.one * Data.ScaleCurve.Evaluate(t) * MaxScale);

            if (FixToTarget)
            {
                SetAnchors(Camera.main.WorldToViewportPoint(Target.position + TargetOffset));
            } else
            {
                SetAnchors(Camera.main.WorldToViewportPoint(WorldPosition));
            }
        }
    }
}