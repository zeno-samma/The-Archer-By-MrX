using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Currency;
using MrX.Development;
using MrX.Rewards;
using MrX.Save;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public static class DeveloperToolsVerification
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        public static string Verify()
        {
            int checks = 0; Action<bool, string> check = (ok, name) => { if (!ok) throw new Exception(name); checks++; };
            var config = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<DeveloperToolsSettings>(StagePresentationAuthoring.SettingsPath));
            try
            {
                config.toolsEnabled = true; config.allowInDevelopmentBuilds = true; config.allowInReleaseBuilds = false;
                check(config.Allows(true, false) && config.Allows(false, true) && !config.Allows(false, false), "Editor/dev allowed; release hidden");
                var wallets = new Wallets(); var saves = new Saves(); var delivery = new LuckyRewardDelivery(null, wallets);
                var service = new DeveloperGrantService(config, delivery, saves, true, false);
                check(service.GrantCurrency("gold", 1000, out _) && wallets.Gold.Amount == 1000 && wallets.Gems.Amount == 0 && saves.Calls == 1, "Gold saved to permanent wallet");
                check(service.GrantCurrency("gem", 50, out _) && wallets.Gems.Amount == 50 && wallets.Gameplay.Amount == 0 && saves.Calls == 2, "Gem grant independent of gold/gameplay");
                check(!service.GrantCurrency("gold", 0, out _) && !service.GrantCurrency("gold", -1, out _) && !service.GrantCurrency("unknown", 100, out _) && saves.Calls == 2, "Bad amounts/currency rejected");
                check(!service.GrantCurrency("gold", int.MaxValue, out _) && wallets.Gold.Amount == 1000 && saves.Calls == 2, "Overflow rejected");
                var release = new DeveloperGrantService(config, delivery, saves, false, false);
                check(!release.GrantCurrency("gold", 1, out _) && saves.Calls == 2, "Hidden release tools cannot grant");
                config.toolsEnabled = false;
                check(!config.Allows(true, true) && !service.GrantCurrency("gem", 1, out _) && saves.Calls == 2, "Master switch disables even an existing service");
                config.toolsEnabled = true; config.allowInDevelopmentBuilds = false;
                check(!config.Allows(false, true), "Development build switch works");
                var fake = new Delivery(); var items = new DeveloperGrantService(config, fake, saves, true, false);
                check(!items.GrantItem(null, out _) && fake.Grants == 0, "Missing item rejected");
                check(items.GrantItem(config.armory.Items[0], out _) && fake.Grants == 1 && fake.Last.kind == LuckyRewardKind.Equipment && fake.Last.amount == 1 && saves.Calls == 3, "Registered item uses shared delivery and saves once");
                return "Developer tools: " + checks + " policy, wallet, overflow and item routing checks passed; real player save untouched.";
            }
            finally { UnityEngine.Object.DestroyImmediate(config); }
        }
        public static string VerifyLiveButtons()
        {
            var panel = UnityEngine.Object.FindAnyObjectByType<DeveloperToolsPanel>();
            var serviceField = typeof(DeveloperToolsPanel).GetField("service", Flags); var original = serviceField.GetValue(panel);
            var settingsField = typeof(DeveloperToolsPanel).GetField("settings", Flags); var originalSettings = (DeveloperToolsSettings)settingsField.GetValue(panel);
            var config = UnityEngine.Object.Instantiate(originalSettings); config.toolsEnabled = true;
            var saves = new Saves(); var delivery = new Delivery(); var service = new DeveloperGrantService(config, delivery, saves, true, false);
            try
            {
                settingsField.SetValue(panel, config); serviceField.SetValue(panel, service);
                Click(panel, "openButton"); var body = (GameObject)typeof(DeveloperToolsPanel).GetField("panel", Flags).GetValue(panel);
                if (!body.activeSelf || GameController.MainMenuScreenBehavior.IsDockEnabled) throw new Exception("DEV modal/focus lock failed");
                Click(panel, "amountButton"); Click(panel, "goldButton");
                if (delivery.Last.currencyId != "gold" || delivery.Last.amount != 10000) throw new Exception("Amount selection/gold button failed");
                Click(panel, "gemButton"); if (delivery.Last.currencyId != "gem") throw new Exception("Gem button failed");
                Click(panel, "nextItem"); Click(panel, "grantItem"); if (delivery.Last.itemId != config.armory.Items[1].Id || saves.Calls != 3) throw new Exception("Item selector/grant button failed");
                config.toolsEnabled = false; typeof(DeveloperToolsPanel).GetMethod("Availability", Flags).Invoke(panel, null);
                var shortcut = (GameObject)typeof(DeveloperToolsPanel).GetField("shortcut", Flags).GetValue(panel);
                if (body.activeSelf || shortcut.activeSelf || !GameController.MainMenuScreenBehavior.IsDockEnabled || service.GrantCurrency("gold", 1, out _)) throw new Exception("Disable must hide tools, close modal and prevent grant");
                return "DEV live prefab: amount selector, gold/gem/item buttons, item navigation, dock lock, live disable/hide and save calls passed with isolated delivery.";
            }
            finally
            {
                panel.Close(); settingsField.SetValue(panel, originalSettings); serviceField.SetValue(panel, original);
                typeof(DeveloperToolsPanel).GetMethod("Availability", Flags).Invoke(panel, null); UnityEngine.Object.DestroyImmediate(config);
            }
        }
        private static void Click(DeveloperToolsPanel panel, string name) => ((Button)typeof(DeveloperToolsPanel).GetField(name, Flags).GetValue(panel)).onClick.Invoke();
        private sealed class Delivery : ILuckyRewardDelivery
        {
            public int Grants; public LuckyGrant Last;
            public bool Resolve(LuckyPrize prize, int amount, int cost, out LuckyGrant grant)
            { grant = new LuckyGrant { kind = prize.kind, currencyId = prize.currencyId, itemId = prize.item ? prize.item.Id : null, amount = amount, title = "Isolated grant" }; return true; }
            public void Grant(LuckyGrant grant) { Grants++; Last = grant; }
            public Sprite Icon(LuckyGrant grant) => null;
        }
        private sealed class Saves : ISaveManager
        {
            public int Calls;
            public T GetSave<T>(string key) where T : ISave, new() => new T();
            public T GetSave<T>(int key) where T : ISave, new() => new T();
            public void Save(bool multithreading = false) { Calls++; }
        }
        private sealed class Wallets : ICurrenciesManager
        {
            public readonly CurrencySave Gold = new CurrencySave(), Gems = new CurrencySave(), Gameplay = new CurrencySave();
            public Wallets() { Gold.Init(JsonUtility.FromJson<CurrencyData>("{\"id\":\"gold\",\"name\":\"Gold\"}")); Gems.Init(JsonUtility.FromJson<CurrencyData>("{\"id\":\"gem\",\"name\":\"Gem\"}")); }
            public CurrencySave GetCurrency(string id, bool gameplay) => gameplay ? Gameplay : id == "gold" ? Gold : id == "gem" ? Gems : null;
            public CurrencySave GetDefaultCurrency(bool gameplay) => gameplay ? Gameplay : Gold;
            public List<CurrencySave> GetAllCurrencies(bool gameplay) => new List<CurrencySave> { Gold, Gems };
            public void ApplyGameplayAmounts() { } public void EraseGameplayAmounts() { }
        }
    }
}
