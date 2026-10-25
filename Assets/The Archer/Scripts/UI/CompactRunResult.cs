using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace MrX.UI
{
    public sealed class CompactRunResult:MonoBehaviour
    {
        [SerializeField] private TMP_Text title,stage,record,kills,dps,season,experience;
        [SerializeField] private UITextEffect timeEffect;
        [SerializeField] private UITextEffect dpsEffect;
        [SerializeField] private Button confirm;
        [SerializeField] private Transform rewards;
        [SerializeField] private ResultRewardView rewardPrefab;
        [SerializeField] private HeroExperienceBar heroBar;
        [SerializeField] private ResponsiveMenuLayout layout;
        [SerializeField] private RunSummaryPresenter summary;
        private GridLayoutGroup rewardLayout;
        private bool shown;
        private bool? portrait;
        private string finalRecordText,finalDpsText;
        private void Awake(){confirm.onClick.AddListener(()=>{Time.timeScale=1;StageController.ReturnToMainMenu();});rewardLayout=rewards.GetComponent<GridLayoutGroup>();timeEffect.Completed+=RevealRecord;if(dpsEffect!=null)dpsEffect.Completed+=RevealDps;}
        private void OnDestroy(){if(timeEffect!=null)timeEffect.Completed-=RevealRecord;if(dpsEffect!=null)dpsEffect.Completed-=RevealDps;}
        private void RevealRecord(){record.text=finalRecordText;record.GetComponent<UITextEffect>()?.Play();}
        private void RevealDps(){dps.text=finalDpsText;}
        public void Show(bool victory)
        {
            if(shown)return;shown=true;gameObject.SetActive(true);var s=StageController.RunStatistics;if(s==null)return;
            title.text=MenuLanguage.Text(victory?"Victory":"Defeat");stage.text=MenuLanguage.Text(StageController.StageData.StageName)+" · "+StageDifficultySelector.Name(StageController.ActiveDifficulty);
            timeEffect.PlayNumber(0,s.seconds);
            bool newRecord=victory?s.newTimeRecord:s.newSurvivalRecord;
            double best=victory?(s.newTimeRecord?s.seconds:s.previousBestTime):(s.newSurvivalRecord?s.seconds:s.previousBestSurvival);
            finalRecordText=(newRecord?MenuLanguage.Text("NEW RECORD!")+" · ":"")+MenuLanguage.Text(victory?"Fastest clear":"Longest survival")+": "+UITextEffect.FormatTime(best);
            double previous=victory?s.previousBestTime:s.previousBestSurvival;
            record.text=MenuLanguage.Text(victory?"Fastest clear":"Longest survival")+": "+(previous>0?UITextEffect.FormatTime(previous):"--:--");
            kills.text=MenuLanguage.Text("Enemies / Bosses defeated")+": "+s.enemiesKilled+" / "+s.bossesKilled;
            double highest=System.Math.Max(s.previousBestDps,s.Dps);
            finalDpsText=MenuLanguage.Text("Highest DPS")+": "+highest.ToString("#,0.##")+(s.newDpsRecord?" · "+MenuLanguage.Text("NEW RECORD!"):"")+"\n"+MenuLanguage.Text("This run DPS")+": "+s.Dps.ToString("#,0.##");
            if(dpsEffect!=null){dpsEffect.Prefix=MenuLanguage.Text("Highest DPS")+": ";dpsEffect.Suffix="";dpsEffect.PlayNumber(0,highest);}else dps.text=finalDpsText;
            season.text=RunSeasonView.SeasonName();experience.text="+"+s.heroExperienceGained.ToString("#,0")+" EXP"+(s.heroLevelAfter>s.heroLevelBefore?" · Lv."+(s.heroLevelBefore+1)+" → "+(s.heroLevelAfter+1):"");
            var delivery=new Rewards.LuckyRewardDelivery(GameController.ArmoryManager,GameController.CurrenciesManager);
            foreach(var currency in GameController.CurrenciesManager.GetAllCurrencies(true))
            {int earned;StageController.DropManager.ResultCurrencies.TryGetValue(currency.Id,out earned);if(earned>0)Add(currency.Data.Icon,currency.Data.Name,earned);}
            Add(GameController.ArmoryManager.EquippedHero.Icon,"Hero EXP",s.heroExperienceGained);
            foreach(var reward in StageController.DropManager.ResultItems)Add(delivery.Icon(reward),reward.title,reward.amount);
            heroBar.Refresh();if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(confirm.gameObject);
        }
        private void Add(Sprite icon,string name,int amount){if(amount<=0)return;var row=Instantiate(rewardPrefab,rewards);row.gameObject.SetActive(true);row.Bind(icon,name,amount);}
        private void LateUpdate()
        {
            bool value=Screen.height>Screen.width;if(layout!=null&&portrait!=value){portrait=value;layout.Apply(value);}
            if(rewardLayout!=null){int columns=Mathf.Clamp(Mathf.FloorToInt((((RectTransform)rewards).rect.width+rewardLayout.spacing.x)/(rewardLayout.cellSize.x+rewardLayout.spacing.x)),1,5);if(rewardLayout.constraintCount!=columns)rewardLayout.constraintCount=columns;}
            if(shown&&(summary==null||!summary.IsDetailsOpen)&&EventSystem.current!=null)
            {var selected=EventSystem.current.currentSelectedGameObject;if(selected==null||!selected.transform.IsChildOf(transform))EventSystem.current.SetSelectedGameObject(confirm.gameObject);}
        }
    }
}
