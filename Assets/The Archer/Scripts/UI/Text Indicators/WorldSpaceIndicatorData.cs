using UnityEngine;

namespace OctoberStudio.UI
{
    [System.Serializable]
    public class WorldSpaceIndicatorData
    {
        [SerializeField] protected WorldSpaceTextType textType;
        [SerializeField] protected GameObject textPrefab;

        [Header("Animation")]
        [SerializeField] protected Float duration;

        [Space]
        [SerializeField] protected Float maxScale;
        [SerializeField] protected Float maxY;
        [SerializeField] protected Float maxX;

        [Space]
        [SerializeField] protected Float spawnPostionOffsetX;
        [SerializeField] protected Float spawnPositionOffsetY;

        [Space]
        [SerializeField] protected AnimationCurve scaleCurve;
        [SerializeField] protected AnimationCurve xPositionCurve;
        [SerializeField] protected AnimationCurve yPositionCurve;

        public WorldSpaceTextType TextType => textType;
        public GameObject TextPrefab => textPrefab;

        public Float Duration => duration;

        public Float MaxScale => maxScale;
        public Float MaxX => maxX;
        public Float MaxY => maxY;

        public Float SpawnPostionOffsetX => spawnPostionOffsetX;
        public Float SpawnPositionOffsetY => spawnPositionOffsetY;

        public AnimationCurve ScaleCurve => scaleCurve;
        public AnimationCurve XPositionCurve => xPositionCurve;
        public AnimationCurve YPositionCurve => yPositionCurve;
    }
}