using MrX.Armory;
using MrX.Extensions;
using MrX.Pool;
using MrX.UI;
using MrX.Rewards;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MrX.StageSave;

namespace MrX.Drop
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] protected DropDatabase database;

        protected Dictionary<DropType, PoolComponent<DropBehavior>> dropPools = new Dictionary<DropType, PoolComponent<DropBehavior>>();

        protected List<DropBehavior> dropList = new List<DropBehavior>();

        protected Queue<LuckyGrant> itemsQueue = new Queue<LuckyGrant>();
        public List<LuckyGrant> ResultItems { get; } = new List<LuckyGrant>();
        public Dictionary<string, int> ResultCurrencies { get; } = new Dictionary<string, int>();
        private bool itemsGranted;
        protected List<TextIndicatorBehavior> aliveItemIndicators = new List<TextIndicatorBehavior>();

        public List<ItemDropSaveData> PickedUpItems { get; protected set; } = new List<ItemDropSaveData>();

        [SerializeField] protected AnimationCurve pickUpEasingCurve;

        protected int startIndex;

        protected StageSave StageSave { get; set; }
        protected ContinuePlayingSave ContinuePlayingSave { get; set; }
        private DropData craftingDropData;
        private DropData bossChestData;

        public bool HasAliveIndicators => aliveItemIndicators.Count > 0;

        public virtual void Awake()
        {
            for (int i = 0; i < database.GemsCount; i++)
            {
                var data = database.GetGemData(i);
                if (data.DropType == DropType.Item || data.DropType == DropType.CraftingResource || data.DropType == DropType.BossChest) continue;

                var pool = new PoolComponent<DropBehavior>($"Drop_{data.DropType}", data.Prefab, data.InitialPoolSize);

                dropPools.Add(data.DropType, pool);
            }
            var crafting = Resources.Load<Crafting.CraftingDatabase>(Crafting.CraftingDatabase.ResourcePath);
            if (crafting != null && crafting.bossChestPrefab != null)
            {
                bossChestData = new DropData(DropType.BossChest, crafting.bossChestPrefab, 2);
                dropPools[DropType.BossChest] = new PoolComponent<DropBehavior>("Boss Chest", crafting.bossChestPrefab, 2);
            }

            StageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                if (ContinuePlayingSave.PickedUpItems != null) PickedUpItems.AddRange(ContinuePlayingSave.PickedUpItems);
            }
            else ContinuePlayingSave.SavePickedUpItems(System.Array.Empty<ItemDropSaveData>());
        }

        protected virtual void Start()
        {
            StageController.RegisterDropManager(this);
        }

        public virtual void GrantPickedUpItems()
        {
            if (itemsGranted) return;
            itemsGranted = true;
            foreach (var currency in GameController.CurrenciesManager.GetAllCurrencies(true)) ResultCurrencies[currency.Data.ID] = currency.Amount;
            for (int i = 0; i < PickedUpItems.Count; i++)
            {
                var itemData = PickedUpItems[i];
                var item = GameController.ArmoryManager.GetItemData(itemData.ItemId);
                if (item == null) continue;
                AddResult(new LuckyGrant { kind = LuckyRewardKind.Equipment, itemId = item.Id, title = item.ItemName, amount = 1 });

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
            var token = ContinuePlayingSave.RunToken;
            GameController.ArmoryManager.BossChests.CommitRun(token);
            foreach (var grant in GameController.ArmoryManager.BossChests.GetCollected(token)) AddResult(grant);
        }

        private void AddResult(LuckyGrant grant)
        {
            var row = ResultItems.Find(g => g.kind == grant.kind && g.itemId == grant.itemId && g.currencyId == grant.currencyId && g.material == grant.material);
            if (row != null) { row.amount = (int)System.Math.Min(int.MaxValue, (long)row.amount + grant.amount); row.pendingDelivery |= grant.pendingDelivery; row.queuedEquipment |= grant.queuedEquipment; }
            else ResultItems.Add(new LuckyGrant { kind = grant.kind, itemId = grant.itemId, currencyId = grant.currencyId, material = grant.material, amount = grant.amount, title = grant.title, queuedEquipment = grant.queuedEquipment, pendingDelivery = grant.pendingDelivery });
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

            if (drop is ItemDropBehavior itemDrop)
            {
                if (HasAliveIndicators)
                {
                    itemsQueue.Enqueue(new LuckyGrant { kind = LuckyRewardKind.Equipment, itemId = itemDrop.ItemData.Id, title = itemDrop.ItemData.ItemName, amount = 1 });
                }
                else
                {
                    SpawnItemIndicator(itemDrop.ItemData);
                }

                var itemDropSave = CreateItemDropSaveData(itemDrop.ItemData);
                PickedUpItems.Add(itemDropSave);
            }

            if (drop is ItemDropBehavior)
            {
                ContinuePlayingSave.SavePickedUpItems(PickedUpItems.ToArray());
                GameController.DailyQuestsManager?.Record(Quests.DailyQuestType.CollectItems);
                GameController.AchievementsManager?.Record(Achievements.AchievementMetric.ItemsCollected);
            }
            if (drop is BossChestDropBehavior chest && chest.Reward != null)
            {
                if (HasAliveIndicators) itemsQueue.Enqueue(chest.Reward); else SpawnRewardIndicator(chest.Reward);
                GameController.DailyQuestsManager?.Record(Quests.DailyQuestType.CollectItems);
                GameController.AchievementsManager?.Record(Achievements.AchievementMetric.ItemsCollected);
            }
        }

        public virtual bool CanDropItem(EnemyDropData itemDropData)// Nếu muốn cho Boss rớt đồ farm nhiều lần thì bỏ qua check Save của template cũ
        {
            Debug.Log("Tắt cơ chế 1 lần rớt đồ của Boss để có thể rớt đồ farm nhiều lần");
            // Bỏ qua check Save của template cũ để Boss có thể rớt đồ farm được nhiều lần
            // var testSave = CreateItemDropSaveData(itemDropData.ItemData);
            // return !StageSave.HasDroppedBefore(testSave);
            ///Mới================================
            return true;
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

        protected virtual void SpawnItemIndicator(ItemData itemData) //sinh ra thông báo khi nhặt được item
        {
            var indicator = StageController.GameScreen.WorldSpaceTextManager.SpawnText(StageController.Player.transform, Vector2.zero, itemData.ItemName, WorldSpaceTextType.ItemPickUp);

            if (indicator is ItemTextIndicatorBehavior itemText)
            {
                itemText.SetItem(itemData);
                aliveItemIndicators.Add(itemText);

                itemText.onIndicatorHidden += OnItemTextIndicatorHidden;
            }
        }

        private void SpawnRewardIndicator(LuckyGrant grant)
        {
            if (!StageController.IsLoaded || StageController.Player == null) return;
            var item = GameController.ArmoryManager.GetItemData(grant.itemId); if (item == null) return;
            var label = grant.title + " x" + grant.amount;
            var indicator = StageController.GameScreen.WorldSpaceTextManager.SpawnText(StageController.Player.transform, Vector2.zero, label, WorldSpaceTextType.ItemPickUp);
            if (indicator is ItemTextIndicatorBehavior view)
            { view.SetReward(item, label); aliveItemIndicators.Add(view); view.onIndicatorHidden += OnItemTextIndicatorHidden; }
        }

        protected virtual void OnItemTextIndicatorHidden(TextIndicatorBehavior indicator) //on khi thông báo nhặt item biến mất
        {
            indicator.onIndicatorHidden -= OnItemTextIndicatorHidden;

            if (itemsQueue.Count > 0)
            {
                SpawnRewardIndicator(itemsQueue.Dequeue());
            }

            aliveItemIndicators.Remove(indicator);
        }

        public virtual DropBehavior Drop(DropType dropType, Vector3 position, float delay)
        {
            if (dropType == DropType.Item || dropType == DropType.BossChest) return null;
            if (dropType == DropType.CraftingResource)
            {
                Debug.LogWarning("Crafting resources require a payload: use crafting loot profiles/DropCrafting, not EnemyData.Drops.");
                return null;
            }
            var drop = dropPools[dropType].GetEntity();
            var dropData = database.GetGemData(dropType);

            drop.transform.position = position;
            drop.onPickedUp += OnDropPickedUp;

            drop.Init(dropData, delay);

            dropList.Add(drop);

            return drop;
        }

        public BossChestDropBehavior DropBossChest(Rewards.BossChestReceipt receipt, Vector3 position)
        {
            if (receipt == null || receipt.claimed || receipt.collected || bossChestData == null || !dropPools.TryGetValue(DropType.BossChest, out var pool)) return null;
            var drop = pool.GetEntity() as BossChestDropBehavior; if (drop == null) return null;
            drop.transform.position = position; drop.onPickedUp += OnDropPickedUp;
            drop.Configure(GameController.ArmoryManager.BossChests, receipt, bossChestData); dropList.Add(drop); return drop;
        }

        public CraftingDropBehavior DropCrafting(Crafting.CraftingLoot loot, Crafting.CraftingManager owner, Vector3 position)
        {
            if (craftingDropData == null || !dropPools.TryGetValue(DropType.CraftingResource, out var pool)) return null;
            var drop = pool.GetEntity() as CraftingDropBehavior;
            if (drop == null) return null;
            drop.transform.position = position;
            drop.onPickedUp += OnDropPickedUp;
            drop.Configure(loot, owner, craftingDropData); dropList.Add(drop);
            return drop;
        }

        public virtual void HideAllDrop()
        {
            for (int i = dropList.Count - 1; i >= 0; i--)
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
