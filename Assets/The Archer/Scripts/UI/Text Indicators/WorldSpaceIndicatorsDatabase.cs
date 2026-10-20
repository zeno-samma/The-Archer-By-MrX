using System.Collections.Generic;
using UnityEngine;

namespace MrX.UI
{
    [CreateAssetMenu(fileName = "World Space Indicators Database", menuName = "MrX Studio/UI/World Space Indicators Database")]
    public class WorldSpaceIndicatorsDatabase : ScriptableObject
    {
        [SerializeField] protected List<WorldSpaceIndicatorData> indicators;

        public List<WorldSpaceIndicatorData> Indicators => indicators;

        public virtual WorldSpaceIndicatorData GetIndicator(WorldSpaceTextType textType)
        {
            for (int i = 0; i < indicators.Count; i++)
            {
                if (indicators[i].TextType == textType)
                {
                    return indicators[i];
                }
            }

            return null;
        }
    }
}