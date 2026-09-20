using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class RoomHeaderObjectBehavior : MonoBehaviour
    {
        private static Dictionary<Transform, string> lastState = new Dictionary<Transform, string>();

        protected virtual void Awake()
        {
            if(Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnEnable()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            CaptureHierarchyState();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        protected virtual void OnHierarchyChanged()
        {
            if (HasHierarchyChanged())
            {
                CaptureHierarchyState();
            }
        }

        protected virtual void CaptureHierarchyState()
        {
            lastState.Clear();
            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true))
            {
                lastState[t] = t.name + "|" + t.GetSiblingIndex() + "|" + t.parent?.name;
            }
        }

        protected virtual bool HasHierarchyChanged()
        {
            Transform[] current = gameObject.GetComponentsInChildren<Transform>(true);
            if (current.Length != lastState.Count)
                return true;

            foreach (Transform t in current)
            {
                if (!lastState.TryGetValue(t, out var prevData))
                    return true;

                string currentData = t.name + "|" + t.GetSiblingIndex() + "|" + t.parent?.name;
                if (currentData != prevData)
                    return true;
            }

            return false;
        }
    }
}