using OctoberStudio.Save;
using UnityEngine;

namespace OctoberStudio
{
    public class ContinuePlayingSave : ISave
    {
        [SerializeField] protected bool hasUnfinishedStageData;
        public bool HasUnfinishedStageData { get => hasUnfinishedStageData; protected set => hasUnfinishedStageData = value; }

        [SerializeField] protected int[] shuffledRoomIndices;
        [SerializeField] protected ArrayWrapper[] shuffledWavesIndices;

        [SerializeField] protected int activeRoomId;
        [SerializeField] protected int activeWaveId;

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
            HasUnfinishedStageData = false;

            ActiveRoomId = 0;
            ActiveWaveId = 0;

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