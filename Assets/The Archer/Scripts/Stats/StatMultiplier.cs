using UnityEngine.Events;

namespace OctoberStudio
{
    public class StatMultiplier
    {
        protected float value;
        public float Value
        {
            get => value;

            set
            {
                var prevValue = this.value;
                this.value = value;

                if (prevValue != value)
                {
                    onMultiplierChanged?.Invoke(this);
                }
            }
        }

        public UnityAction<StatMultiplier> onMultiplierChanged;

        public static implicit operator float(StatMultiplier multiplier) => multiplier.Value;
        public static implicit operator StatMultiplier(float value) => new StatMultiplier() { Value = value };
    }
}