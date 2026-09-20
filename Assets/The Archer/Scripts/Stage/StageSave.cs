using OctoberStudio.Save;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace OctoberStudio
{
    public class StageSave : ISave
    {
        [SerializeField] protected StageSaveData[] stages;
        protected List<StageSaveData> Stages { get; set; }

        [Obsolete]
        [SerializeField] protected int maxReachedStageId;
        [SerializeField] protected int selectedStageId;

        [SerializeField] protected TestingData testingData;

        [SerializeField] protected ItemDropSaveData[] itemDrops;

        public event UnityAction<int> onSelectedStageChanged;

        public int SelectedStageId => selectedStageId;
        [Obsolete]
        public int MaxReachedStageId => maxReachedStageId;

        public bool IsFirstStageSelected => selectedStageId == 0;

        [Obsolete]
        public bool IsMaxReachedStageSelected => selectedStageId == maxReachedStageId;

        public virtual void Init()
        {
            if(stages == null) stages = new StageSaveData[0];
            Stages = new List<StageSaveData>(stages);
        }

        #region Stage

        public virtual bool IsStageUnlocked(StageData stageData)
        {
            return GetStageSave(stageData).IsUnlocked;
        }

        public virtual void UnlockStage(StageData stageData)
        {
            GetStageSave(stageData).Unlock();
        }

        public virtual bool IsStageCompleted(StageData stageData)
        {
            return GetStageSave(stageData).IsCompleted;
        }

        public virtual void CompleteStage(StageData stageData)
        {
            GetStageSave(stageData).Complete();
        }

        public virtual void IncrementStageAttempts(StageData stageData)
        {
            GetStageSave(stageData).IncrementAttempts();
        }

        public virtual int GetStageAttempts(StageData stageData)
        {
            return GetStageSave(stageData).AttemptsCount;
        }

        protected StageSaveData GetStageSave(StageData stageData)
        {
            for(int i = 0; i < Stages.Count; i++)
            {
                if(stageData.StageId == Stages[i].StageId) return Stages[i];
            }

            var stageSaveData = new StageSaveData(stageData.StageId);
            Stages.Add(stageSaveData);

            return stageSaveData;
        }

        public virtual void SetSelectedStageId(int selectedStageId)
        {
            this.selectedStageId = selectedStageId;

            onSelectedStageChanged?.Invoke(selectedStageId);
        }

        [Obsolete]
        public virtual void SetMaxReachedStageId(int maxReachedStageId)
        {
            this.maxReachedStageId = maxReachedStageId;
        }

        #endregion 

        public virtual void SetTestingData(int stageId, int roomId, int waveId)
        {
            testingData = new TestingData(stageId, roomId, waveId);
        }

        public virtual void ClearTestingData()
        {
            if (testingData != null) testingData.Clear();
        }

        public virtual TestingData GetTestingData()
        {
            if (testingData == null) return null;
            if (testingData.IsActive) return testingData;
            return null;
        }

        public virtual bool HasDroppedBefore(ItemDropSaveData itemSaveData)
        {
            if (itemDrops == null) return false;

            for (int i = 0; i < itemDrops.Length; i++)
            {
                if (itemDrops[i] == itemSaveData) return true;
            }

            return false;
        }

        public virtual void AddDropData(List<ItemDropSaveData> newItemDrops)
        {
            List<ItemDropSaveData> itemDropsList;
            if (itemDrops == null)
            {
                itemDropsList = new List<ItemDropSaveData>();
            }
            else
            {
                itemDropsList = new List<ItemDropSaveData>(itemDrops);
            }

            itemDropsList.AddRange(newItemDrops);

            itemDrops = itemDropsList.ToArray();
        }

        public virtual void Flush()
        {
            stages = Stages.ToArray();
        }

        [System.Serializable]
        public class TestingData
        {
            [SerializeField] protected int stageId = -1;
            [SerializeField] protected int roomId = -1;
            [SerializeField] protected int waveId = -1;
            [SerializeField] protected bool isActive = false;

            public int StageId => stageId;
            public int RoomId => roomId;
            public int WaveId => waveId;
            public bool IsActive => isActive;

            public TestingData(int stageId, int roomId, int waveId)
            {
                this.stageId = stageId;
                this.roomId = roomId;
                this.waveId = waveId;

                isActive = true;
            }

            public virtual void Clear()
            {
                stageId = -1;
                roomId = -1;
                waveId = -1;
                isActive = false;
            }
        }

        [System.Serializable]
        public class ItemDropSaveData
        {
            [SerializeField] protected int stageId;
            [SerializeField] protected int roomId;
            [SerializeField] protected int waveId;
            [SerializeField] protected string itemId;

            public string ItemId => itemId;

            public ItemDropSaveData(int stageId, int roomId, int waveId, string itemId)
            {
                this.itemId = itemId;
                this.stageId = stageId;
                this.roomId = roomId;
                this.waveId = waveId;
            }

            public static bool operator ==(ItemDropSaveData first, ItemDropSaveData second) => first.Equals(second);
            public static bool operator !=(ItemDropSaveData first, ItemDropSaveData second) => !first.Equals(second);

            public override bool Equals(object obj)
            {
                if (obj is ItemDropSaveData item)
                {
                    return item.itemId == itemId &&
                        item.stageId == stageId &&
                        item.waveId == waveId &&
                        item.roomId == roomId;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return $"{itemId}{stageId}{roomId}{waveId}".GetHashCode();
            }
        }

        [System.Serializable]
        public class StageSaveData
        {
            [SerializeField] protected string stageId;
            [SerializeField] protected bool isUnlocked = false;
            [SerializeField] protected bool isCompleted = false;
            [SerializeField] protected int attemptsCount = 0;

            public string StageId => stageId;
            public bool IsUnlocked => isUnlocked;
            public bool IsCompleted => isCompleted;
            public int AttemptsCount => attemptsCount;

            public StageSaveData(string stageId)
            {
                this.stageId = stageId;
            }

            public void Unlock()
            {
                isUnlocked = true;
            }

            public void Complete()
            {
                isCompleted = true;
            }
            
            public void IncrementAttempts()
            {
                attemptsCount++;
            }
        }
    }
}