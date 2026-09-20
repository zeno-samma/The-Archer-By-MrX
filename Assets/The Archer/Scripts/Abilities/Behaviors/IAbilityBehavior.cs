namespace OctoberStudio.Abilities
{
    public interface IAbilityBehavior
    {
        AbilityType AbilityType { get; }
        AbilityData AbilityData { get; }
        void Init(AbilityData data, int stageId);
        void Clear();
    }
}