using OctoberStudio.Audio;
using OctoberStudio.Currency;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class WaveData
    {
        [SerializeField] protected List<EnemySpawnData> enemySpawns = new List<EnemySpawnData>();
        public List<EnemySpawnData> EnemySpawns => enemySpawns;

        [SerializeField] protected List<ChestSpawnData> chestSpawns = new List<ChestSpawnData>();
        public List<ChestSpawnData> ChestSpawns => chestSpawns;

        [SerializeField] protected int groupId = 0;
        public int GroupId => groupId;

        [SerializeField] protected float waveDropExpericence;
        public float WaveDropExperience => waveDropExpericence;

        [Obsolete("Migrating to Rewards")]
        [SerializeField, HideInInspector] protected int waveDropGold;
        [Obsolete("Migrating to Rewards")]
        public int WaveDropGold => waveDropGold;

        [SerializeField] protected List<Price> rewards = new List<Price>();
        public List<Price> Rewards => rewards;

        [SerializeField] protected float customWaveEndDelay = -1;
        public float CustomWaveEndDelay => customWaveEndDelay;

        [SerializeField] protected Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] protected AudioData waveMusic;
        public AudioData WaveMusic => waveMusic;

#if UNITY_EDITOR

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            if(enemySpawns == null) enemySpawns = new List<EnemySpawnData>();
            var enemySpawnsProperty = property.FindPropertyRelative("enemySpawns");
            enemySpawnsProperty.arraySize = enemySpawns.Count;
            for (int i = 0; i < enemySpawns.Count; i++)
            {
                enemySpawns[i].SaveToSerializedProperty(enemySpawnsProperty.GetArrayElementAtIndex(i));
            }

            if(chestSpawns == null) chestSpawns = new List<ChestSpawnData>();
            var chestSpawnsProperty = property.FindPropertyRelative("chestSpawns");
            chestSpawnsProperty.arraySize = chestSpawns.Count;
            for (int i = 0; i < chestSpawns.Count; i++)
            {
                chestSpawns[i].SaveToSerializedProperty(chestSpawnsProperty.GetArrayElementAtIndex(i));
            }

            var groupIdProperty = property.FindPropertyRelative("groupId");
            groupIdProperty.intValue = groupId;

            var waveDropExpericenceProperty = property.FindPropertyRelative("waveDropExpericence");
            waveDropExpericenceProperty.floatValue = waveDropExpericence;

            if(rewards == null) rewards = new List<Price>();
            var rewardsProperty = property.FindPropertyRelative("rewards");
            rewardsProperty.arraySize = rewards.Count;
            for (int i = 0; i < rewards.Count; i++)
            {
                rewards[i].SaveToSerializedProperty(rewardsProperty.GetArrayElementAtIndex(i));
            }

            var customWaveEndDelayProperty = property.FindPropertyRelative("customWaveEndDelay");
            customWaveEndDelayProperty.floatValue = customWaveEndDelay;

            var iconProperty = property.FindPropertyRelative("icon");
            iconProperty.objectReferenceValue = icon;

            var waveMusicProperty = property.FindPropertyRelative("waveMusic");
            waveMusicProperty.objectReferenceValue = waveMusic;
        }
#endif
    }
}