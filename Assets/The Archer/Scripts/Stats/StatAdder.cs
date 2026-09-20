using UnityEngine.Events;

namespace OctoberStudio
{
    public class StatAdder
    {
        protected int value;
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                onAdderChanged?.Invoke(this);
            }
        }

        public UnityAction<StatAdder> onAdderChanged;

        public static implicit operator int(StatAdder adder) => adder.Value;
        public static implicit operator StatAdder(int value) => new StatAdder() { Value = value };
    }
}