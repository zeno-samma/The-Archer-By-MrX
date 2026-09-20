using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Rim Data", menuName = "October/Effects/Rim Data")]
    public class RimData : ScriptableObject
    {
        [SerializeField] protected Color rimColor;
        public Color RimColor => rimColor;

        [SerializeField] protected float rimStrength;
        public float RimStrength => rimStrength;

        [SerializeField] protected float rimGradient;
        public float RimGradient => rimGradient;

        [SerializeField] protected float rimDirectional;
        public float RimDirectional => rimDirectional;
    }
}
