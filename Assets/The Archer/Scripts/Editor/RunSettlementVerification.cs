using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MrX.Armory;
using MrX.Crafting;
using MrX.Rewards;
using MrX.Save;
using MrX.UI;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class RunSettlementVerification
    {
        private static string path, before, itemId;
        private static long fragments;
        private static int gold, gameplayGold;
        public static string Verify()
        {
            int n=0; Action<bool,string> check=(ok,s)=>{if(!ok)throw new Exception(s);n++;};
            var item=Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath).armory.Items[0];
            var table=ScriptableObject.CreateInstance<BossChestRewards>();
            try
            {
                table.prizes.Add(new LuckyPrize { id="a",kind=LuckyRewardKind.Equipment,item=item,weight=1 });
                table.prizes.Add(new LuckyPrize { id="b",kind=LuckyRewardKind.Fragments,item=item,weight=3,minimumAmount=2,maximumAmount=4 });
                check(table.Pick(0)==table.prizes[0]&&table.Pick(.25)==table.prizes[1]&&table.Pick(1)==table.prizes[1],"Weighted boundaries");
                check(table.Pick(double.NaN)==null&&table.Pick(-1)==null&&table.Pick(double.PositiveInfinity)==null,"Invalid rolls");
                var saves=new Saves();var delivery=new Delivery();var service=new BossChestService(null,delivery,saves);
                var a=service.Prepare("run","a",table,0,0);var ground=service.Prepare("run","ground",table,1,1);
                check(a!=null&&ground!=null&&saves.Calls==0&&delivery.Total==0,"Prepare must not write or grant");
                check(ReferenceEquals(a,service.Prepare("run","a",table,1,1)),"Kill key stable");
                check(service.Claim(a)&&!service.Claim(a)&&delivery.Total==0&&saves.Calls==0,"Pickup queues once without grant/save");
                check(!service.Claim(new BossChestReceipt{grant=a.grant}),"Foreign receipt rejected");
                check(service.GetCollected("run").Count==1&&service.GetCollected("other").Count==0,"Only collected run loot listed");
                var json=JsonUtility.ToJson(saves.GetSave<BossChestSave>("Boss Chests"));
                var restored=new Saves();restored.entries["Boss Chests"]=JsonUtility.FromJson<BossChestSave>(json);
                var restoredDelivery=new Delivery();var loaded=new BossChestService(null,restoredDelivery,restored);
                loaded.RecoverPending();check(restoredDelivery.Total==0,"Reload cannot pay unfinished run or ground loot");
                loaded.CommitRun("run");loaded.CommitRun("run");
                check(restoredDelivery.Total==1&&restored.Calls==0,"Settlement pays picked loot once; owner controls save");
                check(!restored.GetSave<BossChestSave>("Boss Chests").receipts[1].claimed,"Ground chest never awarded");
                var old=restored.GetSave<BossChestSave>("Boss Chests").receipts[0];
                old.collected=false;old.claimed=true;loaded.CommitRun("run");check(restoredDelivery.Total==1,"Legacy claimed flag never regrants");
                var other=Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath).armory.Items[1];
                var d1=Drop(item,20);var d2=Drop(other,30);var d3=Drop(item,50);var drops=new List<EnemyDropData>{d1,d2,d3};
                var rateSaves=new Saves();var rate=new BossChestService(null,new Delivery(),rateSaves);
                var r0=rate.PrepareFromDrops("rates","r0",drops,1,0,0);var r1=rate.PrepareFromDrops("rates","r1",drops,1,1,0);
                check(r0!=null&&r1!=null&&r0.grant.kind==LuckyRewardKind.Equipment&&rateSaves.Calls==0,"Stage pool read directly, no 70/30 conversion");
                check(rate.PrepareFromDrops("rates","boundary",drops,1,.2,0).grant.itemId==other.Id&&rate.PrepareFromDrops("rates","repeat-item",drops,1,.5,0).grant.itemId==item.Id,"Exact stage weight boundaries and repeated item rows preserved");
                check(rate.PrepareFromDrops("rates","miss",new List<EnemyDropData>{Drop(item,20)},1,.9,0)==null,"Single-entry chance gate preserved");
                check(rate.PrepareFromDrops("rates","off",new List<EnemyDropData>{Drop(item,0)},1,0,0)==null,"Disabled pool gives no reward");
                var delayed=service.Prepare("run","delayed",table,0,0);service.Claim(delayed);delivery.Valid=false;service.CommitRun("run");
                check(!delayed.claimed&&delayed.grant.pendingDelivery&&saves.Calls==0,"Unresolvable settlement retained pending without extra writes");
                delivery.Valid=true;service.RecoverPending();int totalAwarded=delivery.Total;service.RecoverPending();
                check(delayed.claimed&&delivery.Total==totalAwarded&&saves.Calls==1,"Only finalized pending delivery recovers and saves once");
                foreach(var guid in AssetDatabase.FindAssets("t:BossChestRewards"))check(AssetDatabase.LoadAssetAtPath<BossChestRewards>(AssetDatabase.GUIDToAssetPath(guid)).IsValid(out _),"Fallback table valid");
                return "Run settlement: "+n+" isolated selection, no-save pickup, stage-rate, migration and settlement checks passed.";
            }
            finally { UnityEngine.Object.DestroyImmediate(table); }
        }
        private static EnemyDropData Drop(ItemData item,float chance)
        {
            var d=JsonUtility.FromJson<EnemyDropData>("{\"dropType\":4,\"hpZones\":[{\"hp\":1,\"chance\":"+chance.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}]}");
            typeof(EnemyDropData).GetField("itemData",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(d,item);return d;
        }
        public static string BeginLive()
        {
            if(!Application.isPlaying||!StageController.IsLoaded||string.IsNullOrEmpty(SessionState.GetString(SerializationHelper.VerificationDirectoryKey,"")))throw new Exception("Isolated gameplay required");
            Time.timeScale=0;
            path=SerializationHelper.GetPersistentFilePath(SaveManager.SAVE_FILE_NAME_EDITOR); before=File.ReadAllText(path);
            var armory=GameController.ArmoryManager;var item=Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath).armory.Items.Find(i=>!armory.IsItemUnlocked(i.Id));
            if(item==null)throw new Exception("Use fresh isolated save");itemId=item.Id;
            fragments=armory.Crafting.Service.Inventory.Count(CraftingInventorySave.FragmentKey(itemId));
            gold=GameController.CurrenciesManager.GetCurrency("gold",false).Amount;
            var temp=GameController.CurrenciesManager.GetCurrency("gold",true);temp.Deposit(123);gameplayGold=temp.Amount;
            var table=ScriptableObject.CreateInstance<BossChestRewards>();
            try
            {
                table.prizes.Add(new LuckyPrize{id="equipment",kind=LuckyRewardKind.Equipment,item=item});
                var token=GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing").RunToken;
                var a=armory.BossChests.Prepare(token,"verify-equipment",table,0,0);var drop=StageController.DropManager.DropBossChest(a,StageController.Player.transform.position);
                drop.OnPickedUp();drop.OnPickedUp();
                table.prizes[0].kind=LuckyRewardKind.Fragments;table.prizes[0].minimumAmount=table.prizes[0].maximumAmount=4;
                for(int i=0;i<2;i++){var r=armory.BossChests.Prepare(token,"verify-fragments-"+i,table,0,0);StageController.DropManager.DropBossChest(r,StageController.Player.transform.position).OnPickedUp();}
                if(!StageController.DropManager.HasAliveIndicators)throw new Exception("Pickup notification not spawned");
            }
            finally { UnityEngine.Object.DestroyImmediate(table); }
            GameController.DailyQuestsManager.Record(Quests.DailyQuestType.DefeatEnemies,3);
            GameController.AchievementsManager.Record(Achievements.AchievementMetric.EnemiesDefeated,3);
            GameController.SaveManager.Save(false);GameController.SaveManager.Save(true);
            return "Queued equipment and two fragment chests with real pickup notification; kill progress recorded. Wait frames, then VerifyNoWrites.";
        }
        public static string VerifyNoWrites()
        {
            if(File.ReadAllText(path)!=before)throw new Exception("Run wrote file before result");
            if(GameController.ArmoryManager.IsItemUnlocked(itemId)||GameController.ArmoryManager.Crafting.Service.Inventory.Count(CraftingInventorySave.FragmentKey(itemId))!=fragments)throw new Exception("Loot granted before result");
            if(!((SaveManager)GameController.SaveManager).IsGameplayWriteDeferred)throw new Exception("Run write batch not active");
            return "No file writes after kills/pickups and delayed progress updates; permanent item/fragment inventory unchanged.";
        }
        public static string FinishLive(bool victory)
        {
            VerifyNoWrites();
            if(victory){StageController.CurrentRoomIndex=StageController.StageData.RoomsCount-1;StageController.OnExitReached(null);}else StageController.ForceFail();
            var armory=GameController.ArmoryManager;
            if(!armory.IsItemUnlocked(itemId)||armory.Crafting.Service.Inventory.Count(CraftingInventorySave.FragmentKey(itemId))!=fragments+8)throw new Exception("Settlement missing rewards");
            if(GameController.CurrenciesManager.GetCurrency("gold",false).Amount!=gold+gameplayGold||GameController.CurrenciesManager.GetCurrency("gold",true).Amount!=0)throw new Exception("Gold settlement incorrect");
            if(StageController.DropManager.ResultItems.Count!=2||StageController.DropManager.ResultItems.Find(g=>g.kind==LuckyRewardKind.Fragments).amount!=8)throw new Exception("Result loot not aggregated");
            if(StageController.DropManager.ResultCurrencies["gold"]!=gameplayGold)throw new Exception("Result currency snapshot missing");
            var after=File.ReadAllText(path);if(after==before)throw new Exception("Final result not persisted");
            StageController.FinalizeRunRewards();StageController.ForceFail();
            if(File.ReadAllText(path)!=after||armory.Crafting.Service.Inventory.Count(CraftingInventorySave.FragmentKey(itemId))!=fragments+8)throw new Exception("Repeated result grants/writes twice");
            if(victory&&!StageController.GameScreen.StageCompleteScreen.IsOpen||!victory&&!StageController.GameScreen.StageFailedScreen.IsOpen)throw new Exception("Result screen missing");
            return (victory?"Victory":"Defeat")+": gold, equipment and stacked fragments granted/saved once; result screen populated; repeated finalization ignored.";
        }
        private sealed class Delivery:ILuckyRewardDelivery
        {
            public int Total; public bool Valid=true;
            public bool Resolve(LuckyPrize p,int amount,int cost,out LuckyGrant g){g=new LuckyGrant{kind=p.kind,amount=amount};return Valid;}
            public void Grant(LuckyGrant g){Total+=g.amount;}public Sprite Icon(LuckyGrant g)=>null;
        }
        private sealed class Saves:ISaveManager
        {
            public Dictionary<string,ISave> entries=new Dictionary<string,ISave>();public int Calls;
            public T GetSave<T>(string key)where T:ISave,new(){if(!entries.ContainsKey(key))entries[key]=new T();return (T)entries[key];}
            public T GetSave<T>(int key)where T:ISave,new()=>GetSave<T>(key.ToString());public void Save(bool multithreading=false){Calls++;}
        }
    }
}
