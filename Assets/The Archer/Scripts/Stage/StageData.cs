using OctoberStudio.Audio;
using OctoberStudio.Currency;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class StageData : ScriptableObject
    {
        [SerializeField, HideInInspector] protected int dataVersion;
        public int DataVersion => dataVersion;

        [SerializeField, HideInInspector] protected string stageId;
        public string StageId => stageId;

        [SerializeField] protected Sprite stageImage;
        public Sprite StageImage => stageImage;

        [SerializeField] protected string stageName;
        public string StageName => stageName;

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

        [Space]
        [SerializeField, TextArea] protected string unlockDescription;
        public string UnlockDescription => unlockDescription;

        [SerializeReference] protected List<StageUnlockCondition> unlockConditions;
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