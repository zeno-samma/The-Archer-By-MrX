using Unity.Cinemachine;
using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio
{
    public class CameraManager : MonoBehaviour
    {
        protected RoomData RoomData { get; set; }
        public CameraMovementType CameraMovementType => RoomData.CameraMovementType;

        protected Vector2[] Polygon => RoomData.CameraConfinementPolygon;

        [SerializeField] protected CinemachineBrain cinemachineBrain;
        [SerializeField] protected CameraTarget target;

        protected virtual void Awake()
        {
            StageController.RegisterCameraManager(this);

        }

        public virtual void Init(RoomData roomData)
        {
            RoomData = roomData;
        }

        public virtual void TeleportCamera(Transform cameraTarget)
        {
            if (cinemachineBrain.ActiveVirtualCamera is CinemachineVirtualCameraBase camera)
            {
                var oldPosition = target.transform.position;
                var position = ValidatePosition(cameraTarget.position);

                camera.OnTargetObjectWarped(target.transform, position - oldPosition);
            } else
            {
                Debug.LogWarning("Active virtual camera is not a CinemachineCamera.");
            }
        }

        public virtual Vector3 ValidatePosition(Vector3 position)
        {
            switch (CameraMovementType)
            {
                case CameraMovementType.Free: return position;
                case CameraMovementType.Fixed: return Polygon[0].X0Y();
                case CameraMovementType.Line: return ClosestPointOnLine(position.XZ(), Polygon).X0Y();
                case CameraMovementType.Polygon: return ValidatePositionPolygon(position);
            }

            return position;
        }

        protected virtual Vector3 ValidatePositionPolygon(Vector3 position)
        {
            var position2D = position.XZ();
            if (IsPointInPolygon(position2D, Polygon))
            {
                return position;
            }
            else
            {
                position2D = ClosestPointOnPolygon(position2D, Polygon);

                return position2D.X0Y();
            }
        }

        protected virtual bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            var inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                    (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y)
                     / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        protected virtual Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            var ab = b - a;
            var t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }

        protected virtual Vector2 ClosestPointOnPolygon(Vector2 point, Vector2[] polygon)
        {
            var closest = Vector2.zero;
            var minDistSq = float.MaxValue;

            for (int i = 0; i < polygon.Length; i++)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % polygon.Length]; // wrap around
                var candidate = ClosestPointOnSegment(a, b, point);
                var distSq = (candidate - point).sqrMagnitude;

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = candidate;
                }
            }

            return closest;
        }

        protected virtual Vector2 ClosestPointOnLine(Vector2 point, Vector2[] line)
        {
            var closest = Vector2.zero;
            var minDistSq = float.MaxValue;

            for (int i = 0; i < line.Length - 1; i++)
            {
                var a = line[i];
                var b = line[i + 1];
                var candidate = ClosestPointOnSegment(a, b, point);
                var distSq = (candidate - point).sqrMagnitude;

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = candidate;
                }
            }

            return closest;
        }
    }
}