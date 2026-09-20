using OctoberStudio.Abilities;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.UI;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class AbilitiesRowBehavior : MonoBehaviour
    {
        [SerializeField] protected GameObject miniAbilityCardPrefab;

        [SerializeField] protected float evenWidth = 900;
        [SerializeField] protected float oddWidth = 740;

        protected PoolComponent<MiniAbilityCardBehavior> miniAbilityCardPool;

        protected RectTransform rectTransform;

        protected virtual void Awake()
        {
            miniAbilityCardPool = new PoolComponent<MiniAbilityCardBehavior>(miniAbilityCardPrefab, 6, transform);
            rectTransform = GetComponent<RectTransform>();
        }

        public void Show(bool isEven, List<AbilityData> dataList)
        {
            var width = isEven ? evenWidth : oddWidth;
            rectTransform.SetSizeDeltaX(width);

            miniAbilityCardPool.DisableAllEntities();

            for(int i = 0; i < dataList.Count; i++)
            {
                var card = miniAbilityCardPool.GetEntity();

                card.SetData(dataList[i]);
            }
        }
    }
}