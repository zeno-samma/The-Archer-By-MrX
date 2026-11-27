using MrX.Audio;
using MrX.Currency;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MrX
{
    public class StageData : ScriptableObject
    {
        [SerializeField, HideInInspector] protected int dataVersion;
        public int DataVersion => dataVersion;

        [SerializeField, HideInInspector] protected string stageId;
        public string StageId => stageId;

        [SerializeField, HideInInspector] private int layoutRevision;
        [SerializeField, HideInInspector] private StageData previousRunLayout;

        public int LayoutRevision => layoutRevision;

        internal StageData GetRunLayout(int savedRevision)
        {
            return previousRunLayout != null && previousRunLayout.LayoutRevision == savedRevision
                ? previousRunLayout : this;
        }

        [SerializeField] protected Sprite stageImage;
        public Sprite StageImage => stageImage;

        [Header("Journey Encounter")]
        [SerializeField] private Texture2D journeyMap;
        [SerializeField] private GameObject journeyBossPrefab;
        public Texture2D JourneyMap => journeyMap;
        public GameObject JourneyBossPrefab => journeyBossPrefab;

        [SerializeField] protected string stageName;
        public string StageName => stageName;

        [Header("Run Energy")]
        [SerializeField, Min(0)] protected int energyCost = 5;
        public int EnergyCost => Mathf.Max(0, energyCost);

        [SerializeField] protected bool showStageObjective = true;
        public bool ShowStageObjective => showStageObjective;

        [SerializeField, TextArea] protected string stageObjective;
        public string StageObjective => stageObjective;

        [SerializeField] protected AudioData stageMusic;
        public AudioData StageMusic => stageMusic;

        [SerializeField] protected bool showAbilitySelector;
        public bool ShowAbilitySelector => showAbilitySelector;

        [Space]
        [SerializeField] protected float enemyDamageMulitplier = 1f;
        [SerializeField] protected float enemyDamageMultiplierRoomStep = 0.1f;
        [SerializeField] protected float enemyDamageMultiplierWaveStep = 0.02f;

        public float EnemyDamageMultiplier => enemyDamageMulitplier;
        public float EnemyDamageMultiplierRoomStep => enemyDamageMultiplierRoomStep;
        public float EnemyDamageMultiplierWaveStep => enemyDamageMultiplierWaveStep;

        [Space]
        [SerializeField] protected float enemyHPMulitplier = 1f;
        [SerializeField] protected float enemyHPMultiplierRoomStep = 0.1f;
        [SerializeField] protected float enemyHPMultiplierWaveStep = 0.02f;

        [Header("Difficulty")]
        [SerializeField, FormerlySerializedAs("enableDifficultyTrial")] private bool enableDifficulties;
        [SerializeField, Min(1f)] private float intermediateDamageFactor = 1.15f;
        [SerializeField, Min(1f)] private float intermediateHpFactor = 1.25f;
        [SerializeField, Min(1f)] private float hardDamageFactor = 1.3f;
        [SerializeField, Min(1f)] private float hardHpFactor = 1.5f;

        [Header("Encounter Tuning")]
        [SerializeField, Min(1f)] private float bossHpFactor = 1f;
        [SerializeField, Min(1f)] private float bossDamageFactor = 1f;
        [SerializeField, Range(0f, 1f)] private float waveRecoveryFactor = 1f;
        [SerializeField, Range(0f, 1f)] private float normalEnemyRageFactor = 1f;
        [SerializeField, Range(0f, 1f)] private float evilChestCurrentHpCost;
        [SerializeField] private bool useChallengeBossPatterns;
        [SerializeField] private bool showLowHealthWarning;

        public float BossHpFactor => bossHpFactor;
        public float BossDamageFactor => bossDamageFactor;
        public float WaveRecoveryFactor => waveRecoveryFactor;
        public float NormalEnemyRageFactor => normalEnemyRageFactor;
        public float EvilChestCurrentHpCost => evilChestCurrentHpCost;
        public bool UseChallengeBossPatterns => useChallengeBossPatterns;
        public bool ShowLowHealthWarning => showLowHealthWarning;

        public bool UsesDifficulties => enableDifficulties;

        public Vector2 GetEnemyMultipliers(StageDifficulty difficulty, int roomIndex, int waveIndex)
        {
            float damage = EnemyDamageMultiplier + EnemyDamageMultiplierRoomStep * roomIndex +
                EnemyDamageMultiplierWaveStep * waveIndex;
            float hp = EnemyHPMultiplier + EnemyHPMultiplierRoomStep * roomIndex +
                EnemyHPMultiplierWaveStep * waveIndex;

            switch (difficulty)
            {
                case StageDifficulty.Intermediate:
                    return new Vector2(damage * Mathf.Max(1f, intermediateDamageFactor), hp * Mathf.Max(1f, intermediateHpFactor));
                case StageDifficulty.Hard:
                    return new Vector2(damage * Mathf.Max(1f, intermediateDamageFactor, hardDamageFactor),
                        hp * Mathf.Max(1f, intermediateHpFactor, hardHpFactor));
                default:
                    return new Vector2(damage, hp);
            }
        }

        public ClearStageThroughDifficultyCondition GetDifficultyUnlock(int stageIndex)
        {
            if (!enableDifficulties || stageIndex <= 0) return null;
            foreach (var condition in unlockConditions)
            {
                if (condition is ClearStageThroughDifficultyCondition difficultyCondition)
                    return difficultyCondition;
            }

            return new ClearStageThroughDifficultyCondition();
        }

        [Space]
        [SerializeField, TextArea] protected string unlockDescription;
        public string UnlockDescription => unlockDescription;

        [SerializeReference] protected List<StageUnlockCondition> unlockConditions = new List<StageUnlockCondition>();
        public List<StageUnlockCondition> UnlockConditions => unlockConditions;

        public float EnemyHPMultiplier => enemyHPMulitplier;
        public float EnemyHPMultiplierRoomStep => enemyHPMultiplierRoomStep;
        public float EnemyHPMultiplierWaveStep => enemyHPMultiplierWaveStep;

        [SerializeField] protected Price abilityRerollPrice = new Price("gold", 200);
        [SerializeField] protected float nextAbilityRerollMultiplier = 2f;

        public Price AbilityRerollPrice => abilityRerollPrice;
        public float NextAbilityRerollMultiplier => nextAbilityRerollMultiplier;

        [Header("Rooms")]
        [SerializeField] protected List<RoomData> rooms = new List<RoomData>();
        public List<RoomData> Rooms => rooms;

        public int RoomsCount => rooms.Count;

        public virtual RoomData GetRoom(int index)
        {
            return rooms[index];
        }

        public virtual PurchasedCondition GetPurchaseUnlockCondition()
        {
            for(int i = 0; i < unlockConditions.Count; i++)
            {
                if (unlockConditions[i] is PurchasedCondition condition) return condition;
            }

            return null;
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(stageId))
            {
                stageId = System.Guid.NewGuid().ToString();
            }
        }
    }
}
