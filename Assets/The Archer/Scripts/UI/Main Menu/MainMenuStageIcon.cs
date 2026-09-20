using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MainMenuStageIcon : HorizontalSelectable
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected Image lockImage;

        public virtual void SetIcon(Sprite icon, bool isLocked)
        {
            iconImage.sprite = icon;
            lockImage.gameObject.SetActive(isLocked);
        }
    }
}