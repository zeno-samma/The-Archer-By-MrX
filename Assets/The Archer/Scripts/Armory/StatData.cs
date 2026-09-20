using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class StatData
    {
        [SerializeField] protected StatType statType;
        [SerializeField] protected float value;
        [SerializeField] protected bool isVisibleOnUI = false;

        public StatType StatType => statType;
        public float Value => value;
        public bool IsVisibleOnUI => isVisibleOnUI;
    }
}