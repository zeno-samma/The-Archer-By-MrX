namespace OctoberStudio.Abilities
{
    public class FlyingWeaponAbilityBehavior : AbilityBehavior<FlyingWeaponAbilityData, FlyingWeaponAbilityLevel>
    {
        protected StatMultiplier attackSpeedMultiplier = 1;
        protected StatMultiplier attackDamageMultiplier = 1;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);
            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.AttackDamageStat.AddMultiplier(attackDamageMultiplier);

            StageController.Player.DetachWeapon();
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
            attackDamageMultiplier.Value = AbilityLevel.AttackDamageMultiplier;
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.AttackDamageStat.RemoveMultiplier(attackDamageMultiplier);

            StageController.Player.AttachWeapon();
            base.Clear();
        }
    }
}