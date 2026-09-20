using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public abstract class CameraConfinementVisualiser
    {
        public abstract void OnSceneGUI(SerializedProperty roomProperty);

        protected virtual bool IsMouseNearSphere(Vector3 center, float radius)
        {
            var guiPos = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(guiPos);

            var closest = ray.origin + ray.direction * Vector3.Dot(center - ray.origin, ray.direction);

            var dist = Vector3.Distance(closest, center);
            return dist <= radius;
        }

        public static bool GetMousePositionXZPlane(out Vector3 point)
        {
            point = Vector3.zero;

            var guiPos = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(guiPos);

            // If ray is parallel to XZ plane (D.y == 0), no intersection
            if (Mathf.Abs(ray.direction.y) < 1e-6f)
                return false;

            float t = -ray.origin.y / ray.direction.y;

            // Only accept intersections in front of the ray
            if (t < 0)
                return false;

            point = ray.origin + ray.direction * t;
            return true;
        }

        protected virtual Vector3 DoXZHandle(Vector3 pos)
        {
            Handles.color = new Color(1, 0.5f, 0, 0.5f);
            pos = Handles.Slider2D(
                pos,
                Vector3.up,           // plane normal XZ plane
                Vector3.right,        // axis 1
                Vector3.forward,      // axis 2
                0.5f,                 // handle size
                Handles.RectangleHandleCap,
                0f                    // snap
            );

            return pos;
        }
    }
}