using MrX.Save;
using UnityEngine;

namespace MrX
{
    public class ContinuePlayingSave : ISave
    {
        [SerializeField] private string runToken;
        public string RunToken => string.IsNullOrEmpty(runToken) ? runToken = System.Guid.NewGuid().ToString("N") : runToken;
        [SerializeField] private string stageContentId;
        [SerializeField] private int difficulty;
        [SerializeField] private RunCombatStatistics runStatistics;
        public StageDifficulty Difficulty { get => (StageDifficulty)Mathf.Clamp(difficulty, 0, 2); set => difficulty = Mathf.Clamp((int)value, 0, 2); }
        public RunCombatStatistics RunStatistics => runStatistics ?? (runStatistics = new RunCombatStatistics());
        public void ResetRunStatistics() { runStatistics = new RunCombatStatistics(); }
        public string StageContentId => stageContentId;
        public void RememberStage(string contentId) { stageContentId = contentId; }
        [SerializeField] protected bool hasUnfinishedStageData;
        public bool HasUnfinishedStageData { get => hasUnfinishedStageData; protected set => hasUnfinishedStageData = value; }

        [SerializeField] protected int[] shuffledRoomIndices;
        [SerializeField] protected ArrayWrapper[] shuffledWavesIndices;

        [SerializeField] protected int activeRoomId;
        [SerializeField] protected int activeWaveId;
        [SerializeField] private int achievementRoomWatermark;
        public int AchievementRoomWatermark { get => achievementRoomWatermark; set => achievementRoomWatermark = value; }

        [SerializeField] protected int xpLevel;
        [SerializeField] protected float xp;

        [SerializeField] protected float hp;

        [SerializeField] protected bool killedAllEnemies;
        [SerializeField] protected int revivedTimes;

        [SerializeField] protected int rerollsCount;

        [SerializeField] protected StageSave.ItemDropSaveData[] pickedUpItems;

        public int[] ShuffledRoomIndices => shuffledRoomIndices;
        public ArrayWrapper[] ShuffledWaveIndices => shuffledWavesIndices;
        public StageSave.ItemDropSaveData[] PickedUpItems => pickedUpItems;

        public int ActiveRoomId { get => activeRoomId; set => activeRoomId = value; }
        public int ActiveWaveId { get => activeWaveId; set => activeWaveId = value; }

        public int XPLevel { get => xpLevel; set => xpLevel = value; }
        public float XP { get => xp; set => xp = value; }

        public float HP { get => hp; set => hp = value; }

        public bool KilledAllEnemies { get => killedAllEnemies; set => killedAllEnemies = value; }
        public int RevivedTimes { get => revivedTimes; set => revivedTimes = value; }

        public int RerollsCount { get => rerollsCount; set => rerollsCount = value; }

        public virtual void Flush()
        {

        }

        public virtual void Enable()
        {
            HasUnfinishedStageData = true;
        }

        public virtual void SaveShuffleData(int[] shuffledRoomIndices, ArrayWrapper[] shuffledWavesIndices)
        {
            this.shuffledRoomIndices = shuffledRoomIndices;
            this.shuffledWavesIndices = shuffledWavesIndices;
        }

        public virtual void SavePickedUpItems(StageSave.ItemDropSaveData[] pickedUpItems)
        {
            this.pickedUpItems = pickedUpItems;
        }

        public virtual void Disable()
        {
            runToken = null;
            stageContentId = null;
            difficulty = 0; runStatistics = null;
            HasUnfinishedStageData = false;

            ActiveRoomId = 0;
            ActiveWaveId = 0;
            achievementRoomWatermark = 0;

            XPLevel = 0;
            XP = 0;
            HP = 0;

            killedAllEnemies = false;
            revivedTimes = 0;
            rerollsCount = 0;
        }

        [System.Serializable]
        public class ArrayWrapper
        {
            [SerializeField] protected int[] array;
            public virtual int this[int index] => array[index];

            public virtual int Length => array.Length;

            public static implicit operator ArrayWrapper(int[] array)
            {
                return new ArrayWrapper { array = array };
            }
        }
    }
}
