using UnityEngine;
namespace MrX.Development
{
    // Attach to an always-active parent. Targets are authored scene/prefab objects.
    public sealed class GameFeatureGate : MonoBehaviour
    {
        [SerializeField] private GameFeature feature = GameFeature.Online;
        [Tooltip("Objects to show/hide. Keep this controller on an always-active parent, not inside these targets.")]
        [SerializeField] private GameObject[] targets;
        private bool? previous;
        private float nextCheck;
        private void OnEnable() { previous = null; nextCheck = 0; }
        private void LateUpdate()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + .25f;
            Refresh();
        }
        public void Refresh()
        {
            bool visible = GameFeatureSettings.IsEnabled(feature);
            if (previous == visible) return;
            previous = visible;
            if (targets == null) return;
            foreach (var target in targets) if (target != null && target != gameObject) target.SetActive(visible);
        }
    }
}
