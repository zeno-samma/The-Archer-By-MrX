using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    /// <summary>
    /// An owner's authored visibility is an upper bound for conditional decorations.
    /// Capture targets in Awake, before any gameplay condition hides them. Popup and
    /// pooled-view roots have a separate show/hide lifecycle and must not use this gate.
    /// </summary>
    public sealed class InspectorVisibility
    {
        private readonly Dictionary<GameObject, bool> objects = new Dictionary<GameObject, bool>();
        private readonly Dictionary<Behaviour, bool> behaviours = new Dictionary<Behaviour, bool>();

        public void Capture(GameObject target)
        {
            if (target != null && !objects.ContainsKey(target)) objects.Add(target, target.activeSelf);
        }

        public void Capture(Graphic target)
        {
            Capture((Behaviour)target);
        }

        public void Capture(Behaviour target)
        {
            if (target != null && !behaviours.ContainsKey(target)) behaviours.Add(target, target.enabled);
        }

        public void SetVisible(GameObject target, bool condition)
        {
            if (target == null) return;
            Capture(target);
            bool visible = objects[target] && condition;
            if (target.activeSelf != visible) target.SetActive(visible);
        }

        public void SetVisible(Graphic target, bool condition)
        {
            SetEnabled(target, condition);
        }

        public void SetEnabled(Behaviour target, bool condition)
        {
            if (target == null) return;
            Capture(target);
            bool visible = behaviours[target] && condition;
            if (target.enabled != visible) target.enabled = visible;
        }
    }
}
