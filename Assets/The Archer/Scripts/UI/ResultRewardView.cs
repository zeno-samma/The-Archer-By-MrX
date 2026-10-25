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
        public void Bind(Sprite sprite,string name,int count)
        {icon.sprite=sprite;icon.enabled=sprite!=null;title.text=MenuLanguage.Text(name);amount.text="× "+count.ToString("#,0");}
    }
}
