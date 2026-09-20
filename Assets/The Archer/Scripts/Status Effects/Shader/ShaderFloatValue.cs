using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio
{
    public class ShaderFloatValue : ShaderValue<float>
    {
        public ShaderFloatValue(Material material, int hash) : base(material, hash)
        {
            Material = material;
            Hash = hash;
            IsExists = material.HasFloat(hash);
        }

        public override float GetValue()
        {
            if (IsExists) return Material.GetFloat(Hash);

            return 0;
        }

        public override void SetValue(float value)
        {
            if (IsExists) Material.SetFloat(Hash, value);
        }

        public override void DoValue(float value, float duration, EasingType easingType = EasingType.Linear)
        {
            if (IsExists)
            {
                easingCoroutine.StopIfExists();
                easingCoroutine = Material.DoFloat(Hash, value, duration).SetEasing(easingType);
            }
        }
    }
}