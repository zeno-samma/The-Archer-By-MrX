using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(menuName = "October/Stage/Stage Database", fileName = "Stage Database")]
    public class StageDatabase : ScriptableObject
    {
        [SerializeField] protected StageData[] stages;
        public int StagesAmount => stages.Length;
        public StageData[] Stages => stages;

#if UNITY_EDITOR
        [Header("Editor Settings")]

        [SerializeField] protected ExperienceData experienceData;
        public ExperienceData ExperienceData => experienceData;

        [Space]
        [SerializeField] protected List<Color> randomGroupColor = new List<Color>();
        [SerializeField] protected List<Sprite> waveIcons = new List<Sprite>();

        [SerializeField, HideInInspector] protected int lastSelectedStageId = -1;
        [SerializeField, HideInInspector] protected int lastSelectedRoomId = -1;
        [SerializeField, HideInInspector] protected int lastSelectedWaveId = -1;

        public virtual Color GetRandomGroupColor(int index)
        {
            if (index < 0 || index >= randomGroupColor.Count)
            {
                return Color.white;
            }
            return randomGroupColor[index];
        }

        public virtual List<Sprite> GetWaveIcons()
        {
            return waveIcons;
        }
#endif

        public virtual StageData GetStageData(int index)
        {
            return stages[index % StagesAmount];
        }
    }
}