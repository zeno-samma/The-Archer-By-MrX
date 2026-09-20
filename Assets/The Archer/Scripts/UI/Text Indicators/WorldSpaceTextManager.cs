using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class WorldSpaceTextManager : MonoBehaviour
    {
        [SerializeField] protected WorldSpaceIndicatorsDatabase database;
        [SerializeField] protected RectTransform canvasRect;

        protected Dictionary<WorldSpaceTextType, PoolComponent<TextIndicatorBehavior>> damageIndicatorsPools;

        public bool IsInitialized { get; protected set; }

        protected virtual void Init()
        {
            damageIndicatorsPools = new Dictionary<WorldSpaceTextType, PoolComponent<TextIndicatorBehavior>>();

            for (int i = 0; i < database.Indicators.Count; i++)
            {
                var data = database.Indicators[i];
                var pool = new PoolComponent<TextIndicatorBehavior>(data.TextPrefab, 10, canvasRect);
                damageIndicatorsPools.Add(data.TextType, pool);
            }

            IsInitialized = true;
        }

        protected virtual void Start()
        {
            if (!IsInitialized)
            {
                Init();
            }
        }

        public virtual TextIndicatorBehavior SpawnText(Vector3 worldPos, string text, WorldSpaceTextType textType)
        {
            if (!IsInitialized)
            {
                Init();
            }

            var indicator = damageIndicatorsPools[textType].GetEntity();
            var data = database.GetIndicator(textType);

            indicator.Init(data, text, worldPos);

            return indicator;
        }

        public virtual TextIndicatorBehavior SpawnText(Transform target, Vector3 targetOffset, string text, WorldSpaceTextType textType)
        {
            if (!IsInitialized)
            {
                Init();
            }

            var indicator = damageIndicatorsPools[textType].GetEntity();
            var data = database.GetIndicator(textType);

            indicator.Init(data, text, target, targetOffset);

            return indicator;
        }
    }
}