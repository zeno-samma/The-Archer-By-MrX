using OctoberStudio.Armory;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OctoberStudio.StageSave;

namespace OctoberStudio.Drop
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] protected DropDatabase database;

        protected Dictionary<DropType, PoolComponent<DropBehavior>> dropPools = new Dictionary<DropType, PoolComponent<DropBehavior>>();

        protected List<DropBehavior> dropList = new List<DropBehavior>();

        protected Queue<ItemData> itemsQueue = new Queue<ItemData>();
        protected List<TextIndicatorBehavior> aliveItemIndicators = new List<TextIndicatorBehavior>();

        public List<ItemDropSaveData> PickedUpItems { get; protected set; } = new List<ItemDropSaveData>();

        [SerializeField] protected AnimationCurve pickUpEasingCurve;

        protected int startIndex;

        protected StageSave StageSave { get; set; }
        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        public bool HasAliveIndicators => aliveItemIndicators.Count > 0;

        public virtual void Awake()
        {
            for (int i = 0; i < database.GemsCount; i++)
            {
                var data = database.GetGemData(i);

                var pool = new PoolComponent<DropBehavior>($"Drop_{data.DropType}", data.Prefab, data.InitialPoolSize);

                dropPools.Add(data.DropType, pool);
            }

            StageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                PickedUpItems.AddRange(ContinuePlayingSave.PickedUpItems);
            }
        }

        protected virtual void Start()
        {
            StageController.RegisterDropManager(this);
        }

        public virtual void GrantPickedUpItems()
        {
            for(int i = 0; i < PickedUpItems.Count; i++)
            {
                var itemData = PickedUpItems[i];

                if (GameController.ArmoryManager.IsItemUnlocked(itemData.ItemId))
                {
                    GameController.ArmoryManager.IncreaseItemLevel(itemData.ItemId);
                }
                else
                {
                    GameController.ArmoryManager.UnlockItem(itemData.ItemId);
                }
            }

            StageSave.AddDropData(PickedUpItems);
        }

        public virtual void PickUpAllEndWaveDrop()
        {
            StartCoroutine(PickUpAllDropCoroutine(true));
        }

        public virtual void PickUpAllEndRoomDrop()
        {
            StartCoroutine(PickUpAllDropCoroutine(false));
        }

        protected virtual IEnumerator PickUpAllDropCoroutine(bool endWave)
        {
            yield return new WaitUntil(() => !HasSpawningDrop());

            yield return new WaitForSeconds(0.5f);

            var magnetizedDropList = new List<DropBehavior>();

            for (int i = 0; i < dropList.Count; i++)
            {
                var drop = dropList[i];

                if (drop.IsFlyingToPlayer) continue;

                if (drop.DropData.WaveEndAutomaticPickUp && endWave ||
                    drop.DropData.RoomEndAutomaticPickUp && !endWave)
                {
                    magnetizedDropList.Add(dropList[i]);
                }
            }

            float delay = 0.3f / magnetizedDropList.Count;

            for (int i = 0; i < magnetizedDropList.Count; i++)
            {
                var drop = magnetizedDropList[i];
                drop.FlyToPlayer(StageController.Player.transform, pickUpEasingCurve);

                yield return new WaitForSeconds(delay);
            }

            magnetizedDropList.Clear();
        }

        protected virtual bool HasSpawningDrop()
        {
            for (int i = 0; i < dropList.Count; i++)
            {
                if (dropList[i].IsSpawning) return true;
            }

            return false;
        }

        protected virtual void OnDropPickedUp(DropBehavior drop)
        {
            drop.onPickedUp -= OnDropPickedUp;
            dropList.Remove(drop);

            if(drop is ItemDropBehavior itemDrop)
            {
                if (HasAliveIndicators)
                {
                    itemsQueue.Enqueue(itemDrop.ItemData);
                } else
                {
                    SpawnItemIndicator(itemDrop.ItemData);
                }

                var itemDropSave = CreateItemDropSaveData(itemDrop.ItemData);
                PickedUpItems.Add(itemDropSave);
            }

            ContinuePlayingSave.SavePickedUpItems(PickedUpItems.ToArray());
        }

        public virtual bool CanDropItem(EnemyDropData itemDropData)
        {
            var testSave = CreateItemDropSaveData(itemDropData.ItemData);

            return !StageSave.HasDroppedBefore(testSave);
        }

        public virtual ItemDropSaveData CreateItemDropSaveData(ItemData itemData)
        {
            var roomsShuffler = StageController.RoomsShuffler;
            var roomData = roomsShuffler.GetActiveRoomData();
            var realRoomIndex = StageController.StageData.Rooms.IndexOf(roomData);

            var waveData = roomsShuffler.GetActiveWaveData();
            var realWaveIndex = roomData.Waves.IndexOf(waveData);

            var stageIndex = StageSave.SelectedStageId;
            var itemId = itemData.Id;

            return new ItemDropSaveData(stageIndex, realRoomIndex, realWaveIndex, itemId);
        }

        protected virtual void SpawnItemIndicator(ItemData itemData)
        {
            var indicator = StageController.GameScreen.WorldSpaceTextManager.SpawnText(StageController.Player.transform, Vector2.zero, itemData.ItemName, WorldSpaceTextType.ItemPickUp);

            if (indicator is ItemTextIndicatorBehavior itemText)
            {
                itemText.SetItem(itemData);
                aliveItemIndicators.Add(itemText);

                itemText.onIndicatorHidden += OnItemTextIndicatorHidden;
            }
        }

        protected virtual void OnItemTextIndicatorHidden(TextIndicatorBehavior indicator)
        {
            indicator.onIndicatorHidden -= OnItemTextIndicatorHidden;

            if(itemsQueue.Count > 0)
            {
                SpawnItemIndicator(itemsQueue.Dequeue());
            }

            aliveItemIndicators.Remove(indicator);
        }

        public virtual DropBehavior Drop(DropType dropType, Vector3 position, float delay)
        {
            var drop = dropPools[dropType].GetEntity();
            var dropData = database.GetGemData(dropType);

            drop.transform.position = position;
            drop.onPickedUp += OnDropPickedUp;

            drop.Init(dropData, delay);

            dropList.Add(drop);

            return drop;
        }

        public virtual void HideAllDrop()
        {
            for (int i = 0; i < dropList.Count; i++)
            {
                var drop = dropList[i];
                HideDrop(drop);
            }

            dropList.Clear();
        }

        public virtual void HideDrop(DropBehavior drop)
        {
            dropList.Remove(drop);
            drop.onPickedUp -= OnDropPickedUp;
            drop.gameObject.SetActive(false);
        }

        public bool AllRoomDropPickedUp()
        {
            for (int i = 0; i < dropList.Count; i++)
            {
                if (dropList[i].DropData.RoomEndAutomaticPickUp) return false;
            }

            return true;
        }

        public bool AllWaveDropPickedUp()
        {
            for (int i = 0; i < dropList.Count; i++)
            {
                if (dropList[i].DropData.WaveEndAutomaticPickUp) return false;
            }

            return true;
        }
    }
}