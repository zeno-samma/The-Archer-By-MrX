using OctoberStudio.UI;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class ComboAbilityBehavior : AbilityBehavior<ComboAbilityData, ComboAbilityLevel>
    {
        [SerializeField] protected string comboTextFormat = "COMBO x{0}";
        protected StatMultiplier attackDamageMultiplier = 1;

        protected float defeatTime;
        protected int comboCounter;

        protected ComboTextIndicatorBehavior comboIndicator;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);
            StageController.DoAfterRoomLoaded(() => StageController.Room.onEnemyDefeated += OnEnemyDefeated);
        }

        protected virtual void OnEnemyDefeated()
        {
            if (StageController.Room.AliveEnemiesCount == 0)
            {
                comboCounter = 0;
            } else
            {
                defeatTime = Time.time;
                comboCounter++;
            }
                
            RecalculateMultiplier();
        }

        protected virtual void RecalculateMultiplier()
        {
            var multiplier = Mathf.Pow(AbilityLevel.AttackDamageMultiplierPerEnemyKilled, comboCounter);
            if (multiplier > AbilityLevel.MaxAttackDamageMultiplier) multiplier = AbilityLevel.MaxAttackDamageMultiplier;

            attackDamageMultiplier.Value = multiplier;

            if(comboCounter > 0)
            {
                var comboText = string.Format(comboTextFormat, comboCounter);

                if (comboIndicator == null)
                {
                    comboIndicator = (ComboTextIndicatorBehavior) StageController.GameScreen.WorldSpaceTextManager.SpawnText(StageController.Player.transform, Vector2.zero, comboText, WorldSpaceTextType.Combo);
                } else
                {
                    comboIndicator.SetText(comboText);
                }
            
            } else
            {
                if(comboIndicator != null)
                {
                    comboIndicator.Hide();
                    comboIndicator = null;
                }
            }           
        }

        protected virtual void Update()
        {
            if(comboCounter > 0)
            {
                if(defeatTime + AbilityLevel.AbilityDuration <= Time.time)
                {
                    comboCounter--;
                    RecalculateMultiplier();
                }
            }
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);
            StageController.Room.onEnemyDefeated -= OnEnemyDefeated;

            base.Clear();
        }
    }
}