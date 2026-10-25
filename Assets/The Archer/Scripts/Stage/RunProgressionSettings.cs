using System;
using UnityEngine;
namespace MrX
{
    public enum RunSeason { None=-1, Spring=0, Summer=1, Autumn=2, Winter=3 }
    [Serializable] public sealed class SeasonDefinition
    {
        public RunSeason season;
        public string title, description;
        public Sprite icon;
        public Color color=Color.white;
        [Min(0)] public float weight=1;
        [Min(1)] public float damageMultiplier=1.15f, effectMultiplier=1.1f;
    }
    [CreateAssetMenu(menuName="MrX Studio/Run Progression and Seasons")]
    public sealed class RunProgressionSettings : ScriptableObject
    {
        public const string ResourcePath="Run Progression Settings";
        public bool seasonsEnabled=true;
        public SeasonDefinition[] seasons=new SeasonDefinition[0];
        [Min(0)] public int experiencePerEnemy=8, experiencePerBoss=80, clearExperience=120;
        [Range(0,1)] public float defeatExperienceMultiplier=.75f;
        public float[] difficultyExperience={1,1.5f,2};
        [Min(1)] public int baseLevelExperience=250;
        [Min(0)] public int extraExperiencePerLevel=150;
        public int RequiredExperience(Armory.HeroData hero,int level)
        {int configured=hero.GetHeroLevel(level).ExperienceToNextLevel;return configured>0?configured:(int)Math.Min(int.MaxValue,Math.Max(1L,(long)baseLevelExperience+(long)Math.Max(0,level)*extraExperiencePerLevel));}
        public int RunExperience(RunCombatStatistics stats,StageData stage,StageDifficulty mode,bool victory)
        {
            long regular=Math.Max(0L,(long)stats.enemiesKilled-stats.bossesKilled);
            double raw=regular*Math.Max(0,experiencePerEnemy)+(long)Math.Max(0,stats.bossesKilled)*Math.Max(0,experiencePerBoss)+(victory?Math.Max(0,clearExperience):0);
            float factor=difficultyExperience!=null&&(int)mode>=0&&(int)mode<difficultyExperience.Length?Mathf.Max(0,difficultyExperience[(int)mode]):1;
            double result=raw*Mathf.Max(0,stage.HeroExperienceMultiplier)*factor*(victory?1:Mathf.Clamp01(defeatExperienceMultiplier));
            return double.IsNaN(result)?0:(int)Math.Min(1000000,Math.Max(0,Math.Floor(result)));
        }
        public SeasonDefinition Find(RunSeason season)=>seasons==null?null:Array.Find(seasons,s=>s!=null&&s.season==season);
        public SeasonDefinition Roll(StageData stage)
        {
            if(!seasonsEnabled||seasons==null)return null;
            float total=0;foreach(var s in seasons)if(s!=null)total+=Mathf.Max(0,s.weight)*stage.SeasonWeight(s.season);
            if(total<=0)return null;float roll=UnityEngine.Random.value*total;SeasonDefinition last=null;
            foreach(var s in seasons)if(s!=null){float weight=Mathf.Max(0,s.weight)*stage.SeasonWeight(s.season);if(weight<=0)continue;last=s;if(roll<weight)return s;roll-=weight;}
            return last;
        }
    }
    public static class RunSeasons
    {
        public static bool Matches(StatusEffects.StatusEffectType type)
        {
            var s=StageController.RunStatistics;if(s==null)return false;
            return(s.seasonId==0&&type==StatusEffects.StatusEffectType.Shock)||(s.seasonId==1&&type==StatusEffects.StatusEffectType.Ignite)
                ||(s.seasonId==2&&type==StatusEffects.StatusEffectType.Poison)||(s.seasonId==3&&type==StatusEffects.StatusEffectType.Freeze);
        }
        public static float Damage(DamageType type)
        {
            var s=StageController.RunStatistics;if(s==null)return 1;
            bool match=(s.seasonId==0&&type==DamageType.Shock)||(s.seasonId==1&&type==DamageType.Burning)||(s.seasonId==2&&type==DamageType.Poison)||(s.seasonId==3&&type==DamageType.Freezing);
            return match?Mathf.Max(1,s.seasonDamageMultiplier):1;
        }
        public static float Duration(StatusEffects.StatusEffectType type)
        {var s=StageController.RunStatistics;return s!=null&&(s.seasonId==1||s.seasonId==3)&&Matches(type)?Mathf.Max(1,s.seasonEffectMultiplier):1;}
    }
}
