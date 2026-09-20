using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio
{
    public abstract class ShaderValue<T>
    {
        public bool IsExists { get; protected set; }

        public Material Material { get; protected set; }
        public int Hash { get; protected set; }

        public bool UseIntermidiateValue { get; protected set; }

        public T InitialValue { get; protected set; }
        public T IntermidiateValue { get; protected set; }

        protected IEasingCoroutine easingCoroutine;

        public ShaderValue(Material material, int hash)
        {
            Material = material;
            Hash = hash;

            InitialValue = GetValue();
        }

        public abstract T GetValue();
        public abstract void SetValue(T value);
        public abstract void DoValue(T value, float duration, EasingType easingType = EasingType.Linear);

        public void SetIntermidiateValue(T value)
        {
            UseIntermidiateValue = true;

            IntermidiateValue = value;
        }

        public void RemoveIntermidiateValue()
        {
            UseIntermidiateValue = false;
        }

        public void DoReset(float duration, EasingType easingType = EasingType.Linear)
        {
            var value = UseIntermidiateValue ? IntermidiateValue : InitialValue;
            DoValue(value, duration, easingType);
        }

        public void Reset()
        {
            SetValue(InitialValue);
        }
    }
}