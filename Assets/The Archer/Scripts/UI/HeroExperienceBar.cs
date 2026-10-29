using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MrX.UI
{
    public sealed class HeroExperienceBar:MonoBehaviour
    {
        [SerializeField] private TMP_Text heroName,experience;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Image portrait,fill;
        [SerializeField] private bool useFaceCrop;
        [SerializeField] private Vector4 tallPortraitBounds=new Vector4(-.38f,-1.15f,1.38f,1.57f);
        [SerializeField] private Vector4 squarePortraitBounds=new Vector4(-.1f,-.25f,1.1f,1.2f);
        private RunProgressionSettings config;
        private float next;
        private readonly InspectorVisibility inspectorVisibility=new InspectorVisibility();
        private void Awake(){inspectorVisibility.Capture(portrait);}
        private void OnEnable(){next=0;}
        private void LateUpdate(){if(Time.unscaledTime<next)return;next=Time.unscaledTime+.25f;Refresh();}
        public void Refresh()
        {
            var armory=GameController.ArmoryManager;if(armory==null||armory.EquippedHero==null)return;
            if(config==null)config=Resources.Load<RunProgressionSettings>(RunProgressionSettings.ResourcePath);
            var hero=armory.EquippedHero;var save=armory.GetHeroSave(hero.Id);
            heroName.text=MenuLanguage.Text(hero.Name);
            levelText.text=(save.Level+1).ToString();
            if(portrait.sprite!=hero.Icon)
            {
                portrait.sprite=hero.Icon;
                if(useFaceCrop&&hero.Icon!=null)
                {
                    var b=hero.Icon.rect.height>hero.Icon.rect.width*1.5f?tallPortraitBounds:squarePortraitBounds;
                    portrait.rectTransform.anchorMin=new Vector2(b.x,b.y);portrait.rectTransform.anchorMax=new Vector2(b.z,b.w);
                    portrait.rectTransform.offsetMin=portrait.rectTransform.offsetMax=Vector2.zero;
                }
            }
            inspectorVisibility.SetVisible(portrait,hero.Icon!=null);
            bool max=save.Level>=hero.HeroLevelCount-1;int required=config!=null?config.RequiredExperience(hero,save.Level):1;
            string value=max?"MAX":save.Experience+" / "+required+" EXP";
            if(experience.text!=value)experience.text=value;
            float amount=max?1:Mathf.Clamp01((float)save.Experience/required);if(!Mathf.Approximately(fill.fillAmount,amount))fill.fillAmount=amount;
        }
    }
}
