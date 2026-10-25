using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public class ToggleBehavior : MonoBehaviour
    {
        [SerializeField] protected Image toggleImage;
        [SerializeField] protected Sprite onSprite;
        [SerializeField] protected Sprite offSprite;

        [Space]
        [SerializeField] Button toggleButton;

        public event UnityAction<bool> onChanged;

        public bool IsOn { get; private set; }

        protected virtual void Awake()
        {
            toggleButton.onClick.AddListener(OnToggleClicked);
        }

        public virtual void SetToggle(bool value)
        {
            IsOn = value;
            toggleImage.sprite = IsOn ? onSprite : offSprite;

            onChanged?.Invoke(value);
        }

        protected virtual void OnToggleClicked()
        {
            SetToggle(!IsOn);


        }

        public virtual void Select()
        {
            EventSystem.current.SetSelectedGameObject(toggleButton.gameObject);
        }
    }
}