using System;
using System.Collections.Generic;
namespace MrX
{
    [Serializable] public sealed class RunDamageBucket { public DamageType type; public double amount; }
    [Serializable] public sealed class RunCombatStatistics
    {
        public float seconds;
        public double damageDealt, bossDamage, damageTaken, healing;
        public float highestHit;
        public int hitsLanded, criticalHits, hitsTaken, enemiesKilled, bossesKilled;
        public List<RunDamageBucket> damageTypes = new List<RunDamageBucket>();
        [NonSerialized] public bool newTimeRecord, newDpsRecord;
        [NonSerialized] public double previousBestTime, previousBestDps;
        public double Dps => seconds > 0 && !float.IsInfinity(seconds) && !float.IsNaN(seconds)
            && !double.IsInfinity(damageDealt) && !double.IsNaN(damageDealt) ? Math.Max(0, damageDealt) / seconds : 0;
        public void RecordDamage(float damage, DamageType type, bool critical, bool boss)
        {
            if (damage <= 0 || float.IsNaN(damage) || float.IsInfinity(damage)) return;
            damageDealt += damage; hitsLanded++; if (critical) criticalHits++; if (boss) bossDamage += damage;
            highestHit = Math.Max(highestHit, damage);
            if (damageTypes == null) damageTypes = new List<RunDamageBucket>();
            var bucket = damageTypes.Find(b => b.type == type);
            if (bucket == null) { bucket = new RunDamageBucket { type = type }; damageTypes.Add(bucket); }
            bucket.amount += damage;
        }
    }
}
