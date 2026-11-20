using UnityEngine;
using UnityEngine.UI;

namespace MrX.Pets
{
    public sealed class PetMenuPreview : MonoBehaviour
    {
        [SerializeField] private GameObject petVisual;
        [SerializeField] private Image petImage;

        private void OnEnable()
        {
            PetSystem.Changed += Refresh;
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            PetSystem.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (petImage != null && PetSystem.Definition?.Portrait != null)
                petImage.sprite = PetSystem.Definition.Portrait;

            var visual = petVisual != null ? petVisual : petImage != null ? petImage.gameObject : null;
            if (visual != null && visual != gameObject)
                visual.SetActive(PetSystem.Profile?.IsDeployed == true);
        }
    }
}
