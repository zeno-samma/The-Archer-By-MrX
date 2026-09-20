using OctoberStudio.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class MiniAbilityCardBehavior : MonoBehaviour
    {
        [SerializeField] protected Image abilityIcon;

        public virtual void SetData(AbilityData data)
        {
            abilityIcon.sprite = data.Icon;
        }
    }
}