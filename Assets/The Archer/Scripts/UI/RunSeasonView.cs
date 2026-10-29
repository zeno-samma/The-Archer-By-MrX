using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MrX.UI
{
    public sealed class RunSeasonView:MonoBehaviour
    {
        [SerializeField] private Button badge,close;
        [SerializeField] private GameObject information;
        [SerializeField] private TMP_Text badgeText,title,description;
        [SerializeField] private Image icon,background;
        [Header("Introduction")]
        [SerializeField] private bool autoShowIntroduction;
        [SerializeField, Min(0)] private float introductionDuration = 4;
        private readonly InspectorVisibility visibility = new InspectorVisibility();
        private float introUntil;
        private bool initialized;
        private ResponsiveMenuLayout layout;
        private bool? portrait;
        private void Awake(){visibility.Capture(badge.gameObject);visibility.Capture(icon);badge.onClick.AddListener(Toggle);close.onClick.AddListener(CloseInformation);layout=GetComponent<ResponsiveMenuLayout>();}
        private void LateUpdate(){bool value=Screen.height>Screen.width;if(layout!=null&&portrait!=value){portrait=value;layout.Apply(value);}}
        private void Start(){Refresh();}
        private void Update(){if(StageController.IsResultFinalized){badge.gameObject.SetActive(false);information.SetActive(false);return;}if(!initialized){Refresh();return;}if(introUntil>0&&Time.unscaledTime>=introUntil){introUntil=0;information.SetActive(false);}}
        public static string SeasonName()
        {var s=StageController.RunStatistics;var config=Resources.Load<RunProgressionSettings>(RunProgressionSettings.ResourcePath);var definition=s!=null&&config!=null?config.Find((RunSeason)s.seasonId):null;return definition!=null?MenuLanguage.Text(definition.title):MenuLanguage.Text("No elemental season");}
        private void Refresh()
        {
            if (!isActiveAndEnabled) return;
            var stats=StageController.RunStatistics;if(stats==null)return;
            var config=Resources.Load<RunProgressionSettings>(RunProgressionSettings.ResourcePath);var season=config!=null?config.Find((RunSeason)stats.seasonId):null;
            initialized=true;if(season==null){badge.gameObject.SetActive(false);information.SetActive(false);return;}
            badgeText.text=title.text=MenuLanguage.Text(season.title);description.text=MenuLanguage.Text(season.description).Replace("{0}",((stats.seasonEffectMultiplier-1)*100).ToString("0"))+"\n"+MenuLanguage.Text("Elemental damage")+" +"+((stats.seasonDamageMultiplier-1)*100).ToString("0")+"%";
            icon.sprite=season.icon;visibility.SetVisible(icon,season.icon!=null);background.color=season.color;
            visibility.SetVisible(badge.gameObject,true);
            if(autoShowIntroduction){information.SetActive(true);introUntil=Time.unscaledTime+introductionDuration;}
        }
        private void Toggle(){if(!isActiveAndEnabled)return;introUntil=0;information.SetActive(!information.activeSelf);}
        private void CloseInformation(){information.SetActive(false);}
        private void OnDestroy(){if(badge!=null)badge.onClick.RemoveListener(Toggle);if(close!=null)close.onClick.RemoveListener(CloseInformation);}
    }
}
