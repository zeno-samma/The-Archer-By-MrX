using MrX.Armory;
using UnityEngine;

namespace MrX.Development
{
    [CreateAssetMenu(menuName = "MrX Studio/Hero Shard")]
    public sealed class HeroShardDefinition : ScriptableObject
    {
        [SerializeField] private HeroData hero;
        [SerializeField] private Sprite icon;
        [SerializeField, Min(1)] private int grantAmount = 5;

        public HeroData Hero => hero;
        public Sprite Icon => icon;
        public int GrantAmount => grantAmount;
    }
}
