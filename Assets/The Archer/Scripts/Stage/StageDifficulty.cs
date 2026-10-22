using System;
using UnityEngine;
namespace MrX
{
    public enum StageDifficulty { Normal = 0, Hard = 1, Nightmare = 2 }
    [Serializable] public sealed class StageDifficultyTuning
    {
        [Min(.1f)] public float enemyHp = 1;
        [Min(.1f)] public float enemyDamage = 1;
        public StageDifficultyTuning(float hp, float damage) { enemyHp = hp; enemyDamage = damage; }
    }
}
