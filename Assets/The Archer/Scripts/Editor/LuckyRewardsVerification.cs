using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Armory;
using MrX.Crafting;
using MrX.Currency;
using MrX.Rewards;
using MrX.Save;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class LuckyRewardsVerification
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Func<string> finishAnimation;
        [MenuItem("Tools/MrX Studio/Lucky Rewards/Run Isolated Checks")]
        public static void Run() { Debug.Log(Verify()); }
        public static string Verify()
        {
            if (Application.isPlaying) throw new Exception("Run isolated delivery checks in Edit Mode; use live button checks in Play Mode.");
            int checks = 0; Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception(message); checks++; };
            var table = ScriptableObject.CreateInstance<LuckyRewardTable>(); table.id = "test"; table.price = new Price("gem", 10);
            var first = new LuckyPrize { id = "a", weight = 1, minimumAmount = 2, maximumAmount = 4 };
            var second = new LuckyPrize { id = "b", weight = 3 };
            table.prizes.Add(first); table.prizes.Add(second);
            try
            {
                var empty = JsonUtility.FromJson<LuckyRewardsSave>(JsonUtility.ToJson(new LuckyRewardsSave()));
                check(!empty.HasUnseen, "Empty inline receipt roundtrip cannot create a phantom reward");
                empty.lastReceipt = new LuckyReceipt { grant = new LuckyGrant() };
                check(!empty.HasUnseen, "Legacy empty receipt does not block purchases or show zero reward");
                check(table.Pick(0) == 0 && table.Pick(.249999) == 0 && table.Pick(.25) == 1 && table.Pick(1) == 1, "Weighted boundaries incl Unity endpoint");
                check(table.Pick(double.NaN) == -1 && table.Pick(-.01) == -1 && table.Pick(1.1) == -1, "Invalid random input rejected");
                second.weight = 0; check(table.Pick(1) == 0, "Zero weight excluded"); second.weight = 3;
                second.enabled = false; check(table.Pick(.99) == 0, "Disabled prize excluded"); second.enabled = true;
                second.id = "a"; check(!table.IsValid(out _), "Duplicate active IDs rejected"); second.id = "b";
                second.weight = float.NaN; check(!table.IsValid(out _), "NaN weights rejected"); second.weight = -1; check(!table.IsValid(out _), "Negative weights rejected"); second.weight = 3;
                first.maximumAmount = 0; check(!table.IsValid(out _), "Invalid amount range rejected"); first.maximumAmount = 4;
                var wallets = new Wallets(); var saves = new MemorySaves(); var delivery = new Delivery();
                var service = new LuckyRewardsService(wallets, saves, delivery);
                wallets.Gold.Deposit(10000);
                check(service.TryPurchase(table, 0, 0, out _) == LuckyPurchaseResult.InsufficientGems && wallets.Gold.Amount == 10000 && saves.Calls == 0, "Gold cannot fund a gem purchase");
                wallets.Gems.Deposit(100); LuckyPurchaseResult nested = LuckyPurchaseResult.Success;
                UnityEngine.Events.UnityAction<CurrencySave> observer = currency => { nested = service.TryPurchase(table, 0, 0, out _); };
                wallets.Gems.OnAmountChanged += observer;
                check(service.TryPurchase(table, 0, 0, out var receipt) == LuckyPurchaseResult.Success && receipt.grant.amount == 2 && delivery.Total == 2 && wallets.Gems.Amount == 90 && saves.Calls == 1 && nested == LuckyPurchaseResult.Busy, "One charge and grant committed before wallet observers");
                wallets.Gems.OnAmountChanged -= observer;
                check(service.TryPurchase(table, .9, 1, out _) == LuckyPurchaseResult.PendingReward && delivery.Total == 2, "Unseen receipt blocks another purchase");
                check(!service.Acknowledge(receipt.sequence + 1) && service.Acknowledge(receipt.sequence) && !service.Acknowledge(receipt.sequence), "Continue acknowledges only exact receipt once");
                check(service.TryPurchase(table, 0, 1, out receipt) == LuckyPurchaseResult.Success && receipt.grant.amount == 4 && delivery.Total == 6, "Inclusive amount maximum");
                var restored = JsonUtility.FromJson<LuckyRewardsSave>(JsonUtility.ToJson(service.Save)); saves.entries["Lucky Rewards"] = restored;
                var reopened = new LuckyRewardsService(wallets, saves, delivery);
                check(reopened.Save.HasUnseen && reopened.Save.lastReceipt.grant.amount == 4 && delivery.Total == 6 && reopened.State(table) == LuckyPurchaseResult.PendingReward, "Reload restores result without granting again");
                reopened.Acknowledge(receipt.sequence);
                int balance = wallets.Gems.Amount;
                check(reopened.TryPurchase(table, 0, double.NaN, out _) == LuckyPurchaseResult.InvalidConfiguration && wallets.Gems.Amount == balance, "Invalid quantity roll cannot charge");
                table.price = new Price("gold", 10); check(reopened.State(table) == LuckyPurchaseResult.InvalidConfiguration, "Payment restricted to gem");
                table.price = new Price("gem", 0); check(reopened.State(table) == LuckyPurchaseResult.InvalidConfiguration, "Zero or negative price invalid"); table.price = new Price("gem", 10);
                var errorWallets = new Wallets(); errorWallets.Gems.Deposit(20); var errorSaves = new MemorySaves(); var errorDelivery = new Delivery();
                var errorService = new LuckyRewardsService(errorWallets, errorSaves, errorDelivery);
                errorWallets.Gems.OnAmountChanged += _ => { throw new InvalidOperationException("test observer"); };
                try { errorService.TryPurchase(table, 0, 0, out _); } catch (InvalidOperationException) { }
                check(errorWallets.Gems.Amount == 10 && errorDelivery.Total == 2 && errorSaves.Calls == 1 && errorService.Save.HasUnseen, "Throwing notification preserves complete purchase and recovery receipt");
                delivery.Valid = false; check(reopened.State(table) == LuckyPurchaseResult.RewardUnavailable && wallets.Gems.Amount == balance, "Invalid delivery blocks before payment");
                return "Lucky Rewards: " + checks + " transaction/selection checks + " + VerifyDelivery() + " real delivery checks passed. Player save untouched.";
            }
            finally { UnityEngine.Object.DestroyImmediate(table); }
        }
        private static int VerifyDelivery()
        {
            int checks = 0; Action<bool, string> check = (ok, message) => { if (!ok) throw new Exception(message); checks++; };
            var achievements = typeof(GameController).GetProperty("AchievementsManager"); var previous = achievements.GetValue(null); achievements.SetValue(null, null);
            var data = ScriptableObject.CreateInstance<ArmoryDatabase>(); var a = UnityEngine.Object.Instantiate(Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath).armory.Items[0]);
            var b = UnityEngine.Object.Instantiate(a); var root = new GameObject("Isolated Reward Delivery"); var config = ScriptableObject.CreateInstance<ArmoryDashboardDatabase>();
            try
            {
                JsonUtility.FromJsonOverwrite("{\"guid\":\"reward-test-a\"}", a); JsonUtility.FromJsonOverwrite("{\"guid\":\"reward-test-b\"}", b);
                typeof(ArmoryDatabase).GetField("items", Flags).SetValue(data, new List<ItemData> { a, b });
                var armory = root.AddComponent<ArmoryManager>(); var save = new ArmorySave(); save.Init();
                typeof(ArmoryManager).GetField("database", Flags).SetValue(armory, data); typeof(ArmoryManager).GetField("save", Flags).SetValue(armory, save);
                var wallets = new Wallets(); var saves = new MemorySaves();
                typeof(ArmoryManager).GetProperty("Bag").SetValue(armory, new InventoryBagService(save, config, wallets, saves));
                var crafting = root.AddComponent<CraftingManager>(); crafting.Initialize(Resources.Load<CraftingDatabase>(CraftingDatabase.ResourcePath), data, save, saves, armory.UnlockItem);
                typeof(ArmoryManager).GetProperty("Crafting").SetValue(armory, crafting);
                var delivery = new LuckyRewardDelivery(armory, wallets);
                var prize = new LuckyPrize { kind = LuckyRewardKind.Currency, currencyId = "gold" };
                check(delivery.Resolve(prize, 50, 10, out var grant), "Gold resolves"); delivery.Grant(grant); check(wallets.Gold.Amount == 50 && wallets.Gems.Amount == 0, "Gold delivery uses permanent correct wallet");
                prize.currencyId = "missing"; check(!delivery.Resolve(prize, 1, 10, out _), "Unknown wallet rejected");
                prize.currencyId = "gem"; wallets.Gems.Deposit(int.MaxValue);
                check(delivery.Resolve(prize, 5, 10, out _) && !delivery.Resolve(prize, 11, 10, out _), "Same-wallet overflow accounts for gem payment");
                prize.kind = LuckyRewardKind.Equipment; prize.item = a;
                check(delivery.Resolve(prize, 1, 10, out grant), "Registered equipment resolves"); delivery.Grant(grant);
                check(save.GetItem(a.Id) != null && save.GetItem(a.Id).Level == 0 && !save.GetItem(a.Id).IsEquipped, "Equipment unlocks level one without auto-equip");
                delivery.Resolve(prize, 1, 10, out grant); delivery.Grant(grant); check(save.GetItem(a.Id).Level == 1, "Existing duplicate follows legacy level upgrade");
                JsonUtility.FromJsonOverwrite("{\"level\":" + (a.ItemLevelCount - 1) + "}", save.GetItem(a.Id));
                check(delivery.Resolve(prize, 1, 10, out grant) && grant.kind == LuckyRewardKind.Currency && grant.amount == 100, "Max-level duplicate converts to configurable gold");
                delivery.Grant(grant); check(wallets.Gold.Amount == 150, "Compensation granted once");
                prize.kind = LuckyRewardKind.Fragments; check(delivery.Resolve(prize, 8, 10, out grant), "Item-specific fragments resolve"); delivery.Grant(grant);
                check(crafting.Service.Inventory.Count(CraftingInventorySave.FragmentKey(a.Id)) == 8, "Fragments stored under equipment ID");
                prize.kind = LuckyRewardKind.Material; prize.material = CraftingMaterial.Wood; check(delivery.Resolve(prize, 7, 10, out grant), "Material resolves"); delivery.Grant(grant);
                check(crafting.Service.Inventory.Count(CraftingInventorySave.MaterialKey(CraftingMaterial.Wood)) == 7, "Crafting material uses existing storage");
                for (int i = save.UsedSlots; i < 40; i++) save.AddItem("full-" + i);
                prize.kind = LuckyRewardKind.Equipment; prize.item = b;
                check(delivery.Resolve(prize, 1, 10, out grant) && grant.queuedEquipment, "Full bag resolves equipment as pending"); delivery.Grant(grant);
                check(save.UsedSlots == 40 && save.GetItem(b.Id) == null && save.PendingEquipment.Count == 1 && save.PendingEquipment[0].id == b.Id, "Full bag never discards purchased equipment");
                return checks;
            }
            finally { achievements.SetValue(null, previous); UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(a); UnityEngine.Object.DestroyImmediate(b); UnityEngine.Object.DestroyImmediate(data); UnityEngine.Object.DestroyImmediate(config); }
        }
        public static string VerifyLiveButton(bool wheel)
        {
            var hub = UnityEngine.Object.FindAnyObjectByType<StageLuckyRewards>(); var original = hub.Service;
            var wallets = new Wallets(); wallets.Gems.Deposit(1000); var saves = new MemorySaves(); var delivery = new Delivery();
            var service = new LuckyRewardsService(wallets, saves, delivery); typeof(StageLuckyRewards).GetProperty("Service").SetValue(hub, service);
            LuckyRewardsPopup popup = null;
            try
            {
                var button = (Button)typeof(StageLuckyRewards).GetField(wheel ? "wheelButton" : "chestButton", Flags).GetValue(hub); button.onClick.Invoke();
                popup = (LuckyRewardsPopup)typeof(StageLuckyRewards).GetField(wheel ? "wheelPopup" : "chestPopup", Flags).GetValue(hub);
                var buy = (Button)typeof(LuckyRewardsPopup).GetField("purchaseButton", Flags).GetValue(popup); buy.onClick.Invoke(); buy.onClick.Invoke();
                var table = (LuckyRewardTable)typeof(LuckyRewardsPopup).GetField("table", Flags).GetValue(popup);
                if (wallets.Gems.Amount != 1000 - table.price.Amount || saves.Calls != 1 || delivery.Total < 1 || !service.Save.HasUnseen) throw new Exception("Authored purchase button failed or charged twice");
                popup.Hide(); popup.Show(service);
                var result = (GameObject)typeof(LuckyRewardsPopup).GetField("resultPanel", Flags).GetValue(popup);
                if (!result.activeSelf) throw new Exception("Closing during animation lost visual result");
                var next = (Button)typeof(LuckyRewardsPopup).GetField("continueButton", Flags).GetValue(popup); int granted = delivery.Total; next.onClick.Invoke(); next.onClick.Invoke();
                if (service.Save.HasUnseen || delivery.Total != granted) throw new Exception("Continue duplicated reward");
                return (wheel ? "Wheel" : "Chest") + ": live prefab button, double click, close/reopen animation recovery and Continue passed with isolated wallet/save.";
            }
            finally { if (popup != null) popup.Hide(); typeof(StageLuckyRewards).GetProperty("Service").SetValue(hub, original); }
        }
        public static string BeginAnimationCheck()
        {
            var hub = UnityEngine.Object.FindAnyObjectByType<StageLuckyRewards>(); var original = hub.Service;
            var popup = (LuckyRewardsPopup)typeof(StageLuckyRewards).GetField("wheelPopup", Flags).GetValue(hub);
            var tableField = typeof(LuckyRewardsPopup).GetField("table", Flags); var originalTable = (LuckyRewardTable)tableField.GetValue(popup);
            var table = UnityEngine.Object.Instantiate(originalTable); table.animationSeconds = .7f;
            table.prizes.Add(new LuckyPrize { id = "extra-test-a", title = "Extra prefab view A", weight = 5 });
            table.prizes.Add(new LuckyPrize { id = "extra-test-b", title = "Extra prefab view B", weight = 5 });
            var wallets = new Wallets(); wallets.Gems.Deposit(1000); var saves = new MemorySaves(); var service = new LuckyRewardsService(wallets, saves, new Delivery());
            float timeScale = Time.timeScale;
            tableField.SetValue(popup, table); typeof(StageLuckyRewards).GetProperty("Service").SetValue(hub, service);
            ((Button)typeof(StageLuckyRewards).GetField("wheelButton", Flags).GetValue(hub)).onClick.Invoke();
            Time.timeScale = 0;
            ((Button)typeof(LuckyRewardsPopup).GetField("purchaseButton", Flags).GetValue(popup)).onClick.Invoke();
            finishAnimation = () =>
            {
                try
                {
                    if ((bool)typeof(LuckyRewardsPopup).GetField("animating", Flags).GetValue(popup)) throw new Exception("Unscaled animation has not finished");
                    var result = (GameObject)typeof(LuckyRewardsPopup).GetField("resultPanel", Flags).GetValue(popup);
                    if (!result.activeSelf) throw new Exception("Animation did not reveal saved result");
                    var angles = (Dictionary<int, float>)typeof(LuckyRewardsPopup).GetField("winningAngles", Flags).GetValue(popup);
                    var disc = (RectTransform)typeof(LuckyRewardsPopup).GetField("wheelDisc", Flags).GetValue(popup);
                    if (Mathf.Abs(Mathf.DeltaAngle(disc.localEulerAngles.z, angles[service.Save.lastReceipt.prizeIndex])) > .02f) throw new Exception("Wheel pointer does not match actual reward");
                    int expected = table.prizes.FindAll(p => p != null && p.enabled && p.weight > 0).Count;
                    var slices = popup.GetComponentsInChildren<LuckyWheelSlice>(); if (slices.Length != expected) throw new Exception("Additional configured rewards did not reuse slice prefab");
                    for (int i = 0; i < slices.Length; i++)
                    {
                        if (Mathf.Abs(slices[i].sector.fillAmount - 1f / slices.Length) > .00001f || slices[i].label.gameObject.activeSelf)
                            throw new Exception("Visual sectors must be equal and percentages hidden");
                        float midpoint = (i + .5f) * 360 / slices.Length * Mathf.Deg2Rad;
                        var anchor = Vector2.one * .5f + new Vector2(Mathf.Sin(midpoint), Mathf.Cos(midpoint)) * .31f;
                        if (Vector2.Distance(slices[i].icon.rectTransform.anchorMin, anchor) > .0001f)
                            throw new Exception("Reward icons must be evenly spaced");
                    }
                    if (Mathf.Abs(disc.rect.width - disc.rect.height) > 1) throw new Exception("Wheel must remain circular");
                    return "Wheel animation: exact winning angle, unscaled time at timeScale=0, " + slices.Length + " configured prefab slices, circular layout and saved result passed.";
                }
                finally { Time.timeScale = timeScale; popup.Hide(); tableField.SetValue(popup, originalTable); typeof(StageLuckyRewards).GetProperty("Service").SetValue(hub, original); UnityEngine.Object.DestroyImmediate(table); }
            };
            return "Started isolated unscaled wheel animation with two extra configured rewards.";
        }
        public static string FinishAnimationCheck()
        { if (finishAnimation == null) throw new Exception("Begin animation check first"); var finish = finishAnimation; finishAnimation = null; return finish(); }
        private sealed class Delivery : ILuckyRewardDelivery
        {
            public int Total; public bool Valid = true;
            public bool Resolve(LuckyPrize prize, int amount, int gemCost, out LuckyGrant grant) { grant = new LuckyGrant { kind = prize.kind, amount = amount, title = "Isolated test reward" }; return Valid; }
            public void Grant(LuckyGrant grant) { Total += grant.amount; }
            public Sprite Icon(LuckyGrant grant) => null;
        }
        private sealed class MemorySaves : ISaveManager
        {
            public readonly Dictionary<string, ISave> entries = new Dictionary<string, ISave>(); public int Calls;
            public T GetSave<T>(string id) where T : ISave, new() { if (!entries.ContainsKey(id)) entries[id] = new T(); return (T)entries[id]; }
            public T GetSave<T>(int id) where T : ISave, new() => GetSave<T>(id.ToString());
            public void Save(bool multithreading = false) { Calls++; }
        }
        private sealed class Wallets : ICurrenciesManager
        {
            public readonly CurrencySave Gold = new CurrencySave(), Gems = new CurrencySave(), Gameplay = new CurrencySave();
            public Wallets() { Gold.Init(JsonUtility.FromJson<CurrencyData>("{\"id\":\"gold\",\"name\":\"Gold\"}")); Gems.Init(JsonUtility.FromJson<CurrencyData>("{\"id\":\"gem\",\"name\":\"Gem\"}")); }
            public CurrencySave GetCurrency(string id, bool gameplay) => gameplay ? Gameplay : id == "gold" ? Gold : id == "gem" ? Gems : null;
            public CurrencySave GetDefaultCurrency(bool gameplay) => gameplay ? Gameplay : Gold;
            public List<CurrencySave> GetAllCurrencies(bool gameplay) => new List<CurrencySave> { Gold, Gems };
            public void ApplyGameplayAmounts() { } public void EraseGameplayAmounts() { Gameplay.Clear(); }
        }
    }
}
