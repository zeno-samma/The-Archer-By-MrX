using System;
using System.Collections.Generic;
using MrX.Currency;
using MrX.Save;
using MrX.Localization;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class MenuEconomyVerification
    {
        [MenuItem("Tools/MrX Studio/Economy/Run Energy Logic Checks")]
        public static void Run() { Debug.Log(Verify()); }
        public static string Verify()
        {
            int count=0;
            Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);count++;};
            var config=ScriptableObject.CreateInstance<MenuEconomySettings>();
            var features = MrX.Development.GameFeatureSettings.Current;
            bool originalEnergy = features != null && features.energyEnabled;
            if (features != null) features.energyEnabled = true;
            try
            {
                var wallets=new Wallets();var saves=new Saves();long now=100000;
                var service=new MenuEconomyService(config,wallets,saves,()=>now);
                check(service.Ready&&wallets.Energy.Amount==30,"Initial energy cap");
                check(saves.Writes==1,"Initialization saved once");
                now+=72000;service.Refresh();
                check(wallets.Energy.Amount==30&&saves.Writes==1,"No banked recovery or per-tick saves at cap");
                check(service.TrySpendForNewRun(5)&&wallets.Energy.Amount==25,"Spend once");
                check(!service.TrySpendForNewRun(-1)&&!service.TrySpendForNewRun(26),"Invalid/insufficient entry blocked");
                now+=3599;check(!service.Refresh()&&service.SecondsUntilRecovery==1,"Hourly boundary");
                now++;check(service.Refresh()&&wallets.Energy.Amount==30,"Hour grant capped");
                check(!service.Refresh(),"No duplicate grant");
                check(service.TrySpendForNewRun(30),"Spend full balance");
                now+=3600*3+1800;service.Refresh();
                check(wallets.Energy.Amount==15&&service.SecondsUntilRecovery==1800,"Offline hours and remainder");
                var reloaded=new MenuEconomyService(config,wallets,saves,()=>now);
                check(wallets.Energy.Amount==15&&reloaded.SecondsUntilRecovery==1800,"Reload preserves recovery");
                now-=100000;reloaded.Refresh();check(wallets.Energy.Amount==15,"Clock rollback gives no energy");
                now+=100000;now+=3600*24*365;reloaded.Refresh();check(wallets.Energy.Amount==30,"Long offline clamp");
                int writes=saves.Writes;
                check(reloaded.Buy("gold")==ExchangeResult.NotEnoughGems&&saves.Writes==writes,"No gem means no payment/save");
                wallets.Gems.Deposit(100);
                check(reloaded.Buy("energy")==ExchangeResult.EnergyFull&&wallets.Gems.Amount==100,"Full energy no payment");
                check(reloaded.TrySpendForNewRun(15),"Prepare test energy space");
                check(reloaded.Buy("energy")==ExchangeResult.Success&&wallets.Energy.Amount==25&&wallets.Gems.Amount==80,"Energy pack exact cost/grant");
                check(reloaded.Buy("energy")==ExchangeResult.EnergyFull&&wallets.Gems.Amount==80,"Reject partial full-price pack");
                check(reloaded.Buy("gold")==ExchangeResult.Success&&wallets.Gold.Amount==1000&&wallets.Gems.Amount==70,"Gold exchange");
                ExchangeResult nested=ExchangeResult.Success;
                wallets.Gold.OnAmountChanged+=_=>{nested=reloaded.Buy("gold");};
                check(reloaded.Buy("gold")==ExchangeResult.Success&&nested==ExchangeResult.Busy&&wallets.Gold.Amount==2000&&wallets.Gems.Amount==60,"Wallet observer reentry blocked");
                config.goldGemPrice=-1;check(reloaded.Buy("gold")==ExchangeResult.InvalidConfiguration,"Invalid negative price");
                config.goldGemPrice=10;config.goldPurchaseAmount=int.MaxValue;check(reloaded.Buy("gold")==ExchangeResult.InvalidConfiguration,"Overflow rejected");
                check(reloaded.Buy("gem")==ExchangeResult.InvalidConfiguration,"Unsupported exchange rejected");
                config.energyPerHour=0;check(!reloaded.TrySpendForNewRun(1)&&!reloaded.Refresh(),"Invalid recovery config fails closed");
                config.energyPerHour=5;
                var limitedWallets=new Wallets();var limitedSaves=new Saves();long dayTime=new DateTimeOffset(2026,10,20,16,0,0,TimeSpan.Zero).ToUnixTimeSeconds();
                var limited=new MenuEconomyService(config,limitedWallets,limitedSaves,()=>dayTime);limitedWallets.Gems.Deposit(1000);limited.TrySpendForNewRun(30);
                for(int i=0;i<5;i++)
                {
                    check(limited.Buy("energy")==ExchangeResult.Success,"Daily purchase accepted "+i);limited.TrySpendForNewRun(10);
                    limited=new MenuEconomyService(config,limitedWallets,limitedSaves,()=>dayTime);
                    check(limited.RemainingEnergyPurchases==4-i,"Quota persists through reload");
                }
                int gems=limitedWallets.Gems.Amount;check(limited.Buy("energy")==ExchangeResult.DailyLimit&&limitedWallets.Gems.Amount==gems,"Sixth purchase rejected without charge");
                check(limited.Buy("gold")==ExchangeResult.Success,"Gold is not limited by energy quota");
                dayTime+=3600;check(limited.RemainingEnergyPurchases==5,"Midnight UTC+7 quota reset");
                check(limited.Buy("energy")==ExchangeResult.Success&&limited.RemainingEnergyPurchases==4,"First purchase after reset");
                dayTime-=86400;check(limited.RemainingEnergyPurchases==4,"Rollback cannot reset quota");
                var table=Resources.Load<MenuTranslations>("Menu Translations");
                foreach(var code in MenuLanguage.Codes)
                {
                    string result=table.Translate("FRAGMENTS: 12 / 30",code);
                    check(result.Contains("12")&&result.Contains("30"),"Two placeholders preserved");
                    check(table.Translate("Lv. 2  >  Lv. 3",code).Contains("3"),"Specific level template precedence");
                    if(code!="en")check(table.Translate("Increases your\n\nmaximum HP",code)!="Increases your\n\nmaximum HP","Multiline descriptions localized");
                }
                foreach(var path in MenuEconomyAuthoring.Pages)
                {
                    var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    check(root!=null&&root.GetComponentsInChildren<MrX.UI.MenuEconomyPanel>(true).Length==1,"Exactly one economy prefab per page");
                    foreach(var tr in root.GetComponentsInChildren<Transform>(true))check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tr.gameObject)==0,"Missing script in "+path);
                }
                if (features != null)
                {
                    features.energyEnabled = false;
                    int disabledEnergy = wallets.Energy.Amount, disabledGems = wallets.Gems.Amount, writesBefore = saves.Writes;
                    check(service.TrySpendForNewRun(int.MaxValue) && wallets.Energy.Amount == disabledEnergy && saves.Writes == writesBefore, "Energy disabled: run never charges or blocks");
                    check(service.Buy("energy") == ExchangeResult.InvalidConfiguration && wallets.Gems.Amount == disabledGems, "Energy disabled: no paid purchase");
                    now += 36000;
                    check(!service.Refresh() && wallets.Energy.Amount == disabledEnergy, "Energy disabled: no recovery grant");
                }
                return "Energy, exchange, localization and prefab checks passed: "+count;
            }
            finally { if (features != null) features.energyEnabled = originalEnergy; UnityEngine.Object.DestroyImmediate(config); }
        }
        private sealed class Saves:ISaveManager
        {
            private readonly Dictionary<int,ISave> cells=new Dictionary<int,ISave>();public int Writes;
            public T GetSave<T>(string id)where T:ISave,new(){return GetSave<T>(id.GetHashCode());}
            public T GetSave<T>(int id)where T:ISave,new(){if(!cells.ContainsKey(id))cells[id]=new T();return (T)cells[id];}
            public void Save(bool multithreading=false){Writes++;}
        }
        private sealed class Wallets:ICurrenciesManager
        {
            public readonly CurrencySave Energy=new CurrencySave(),Gold=new CurrencySave(),Gems=new CurrencySave();
            public CurrencySave GetCurrency(string id,bool gameplay){return gameplay?null:id=="energy"?Energy:id=="gold"?Gold:id=="gem"?Gems:null;}
            public CurrencySave GetDefaultCurrency(bool gameplay){return Gold;}
            public List<CurrencySave> GetAllCurrencies(bool gameplay){return new List<CurrencySave>{Gold,Gems,Energy};}
            public void ApplyGameplayAmounts(){} public void EraseGameplayAmounts(){}
        }
    }
}
