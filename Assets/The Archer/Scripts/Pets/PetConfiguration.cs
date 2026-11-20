using UnityEngine;

namespace MrX.Pets
{
    [DisallowMultipleComponent]
    public sealed class PetConfiguration : MonoBehaviour
    {
        [SerializeField] private PetDefinition definition;

        private void Awake()
        {
            PetSystem.Configure(definition);
        }
    }
}
