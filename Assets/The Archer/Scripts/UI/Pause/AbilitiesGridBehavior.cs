using MrX.Abilities;
using MrX.Easing;
using MrX.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace MrX.UI
{
    public class AbilitiesGridBehavior : MonoBehaviour
    {
        private readonly UIAnimationDefaults animationDefaults = new UIAnimationDefaults();
        [SerializeField] protected GameObject abilitiesRowPrefab;
        [SerializeField] protected float scaleDuration = 0.3f;
        [SerializeField] protected float scaleDelay = 0.1f;
        [SerializeField] protected AnimationCurve scaleEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        protected PoolComponent<AbilitiesRowBehavior> abilitiesRowPool;

        protected RectTransform rectTransform;

        protected virtual void Awake()
        {
            animationDefaults.Capture(transform);
            rectTransform = GetComponent<RectTransform>();

            abilitiesRowPool = new PoolComponent<AbilitiesRowBehavior>(abilitiesRowPrefab, 3, transform);
        }

        public void Show()
        {
            rectTransform.localScale = Vector3.zero;
            rectTransform.DoLocalScale(animationDefaults.Scale(rectTransform), scaleDuration)
                .SetDelay(scaleDelay)
                .SetEasingCurve(scaleEasing)
                .SetUnscaledTime(true);

            abilitiesRowPool.DisableAllEntities();

            var abilities = StageController.AbilitiesManager.GetAcquiredAbilities();

            bool isEven = false;

            while(abilities.Count > 0)
            {
                var maxCount = isEven ? 6 : 5;

                if(abilities.Count < maxCount)
                {
                    maxCount = abilities.Count;
                }

                var dataList = new List<AbilityData>();

                for(int i = 0; i < maxCount; i++)
                {
                    dataList.Add(abilities[0]);
                    abilities.RemoveAt(0);
                }

                var row = abilitiesRowPool.GetEntity();
                row.Show(isEven, dataList);

                isEven = !isEven;
            }
        }
    }
}
