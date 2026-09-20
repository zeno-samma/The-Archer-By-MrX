using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio
{
    public class ShaderColorValue : ShaderValue<Color>
    {
        public ShaderColorValue(Material material, int hash) : base(material, hash)
        {
            Material = material;
            Hash = hash;
            IsExists = material.HasColor(hash);
        }

        public override Color GetValue()
        {
            if (IsExists) return Material.GetColor(Hash);

            return Color.clear;
        }

        public override void SetValue(Color value)
        {
            if (IsExists) Material.SetColor(Hash, value);
        }

        public override void DoValue(Color value, float duration, EasingType easingType = EasingType.Linear)
        {
            if (IsExists)
            {
                easingCoroutine.StopIfExists();
                easingCoroutine = Material.DoColor(Hash, value, duration).SetEasing(easingType);
            }

        }
    }
}