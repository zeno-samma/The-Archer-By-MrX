using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MrX.UI
{
    public sealed class ResultRewardView:MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amount,title;
        private readonly InspectorVisibility inspectorVisibility=new InspectorVisibility();
        private void Awake(){inspectorVisibility.Capture(icon);}
        public void Bind(Sprite sprite,string name,int count)
        {icon.sprite=sprite;inspectorVisibility.SetVisible(icon,sprite!=null);title.text=MenuLanguage.Text(name);amount.text="× "+count.ToString("#,0");}
    }
}
