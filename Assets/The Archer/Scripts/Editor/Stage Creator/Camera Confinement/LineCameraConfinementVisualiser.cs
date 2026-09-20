using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class LineCameraConfinementVisualiser : CameraConfinementVisualiser
    {
        protected Vector2 mousePosition;

        private int lastHotControl = 0;
        protected int pressedPointId = -1;

        protected virtual SerializedProperty GetPointsProperty(SerializedProperty roomProperty)
        {
            var pointsProperty = roomProperty.FindPropertyRelative("cameraConfinementPolygon");

            if (pointsProperty.arraySize == 0)
            {
                pointsProperty.arraySize = 2;
                pointsProperty.GetArrayElementAtIndex(1).vector2Value = Vector2.up;
            }
            else if (pointsProperty.arraySize == 1)
            {
                pointsProperty.arraySize = 2;
                pointsProperty.GetArrayElementAtIndex(1).vector2Value = pointsProperty.GetArrayElementAtIndex(0).vector2Value + Vector2.up;
            }

            return pointsProperty;
        }

        protected virtual bool InitMousePosition()
        {
            if (!GetMousePositionXZPlane(out var intersection))
            {
                mousePosition = Vector2.zero;
                return false;
            }
            else
            {
                mousePosition = intersection.XZ();
                return true;
            }
        }

        protected virtual int GetClosestPointId(SerializedProperty pointsProperty)
        {
            var closest = Vector2.zero;
            var minDistSq = float.MaxValue;
            var closestId = -1;

            for (int i = 0; i < pointsProperty.arraySize; i++)
            {
                var point = pointsProperty.GetArrayElementAtIndex(i).vector2Value;

                var distSqr = (point - mousePosition).sqrMagnitude;
                if (distSqr < minDistSq)
                {
                    closest = point;
                    minDistSq = distSqr;
                    closestId = i;
                }
            }

            return closestId;

        }

        protected virtual void DrawLine(SerializedProperty pointsProperty)
        {
            Handles.color = Color.yellow;
            for (int i = 0; i < pointsProperty.arraySize; i++)
            {
                var pointProperty = pointsProperty.GetArrayElementAtIndex(i);

                var position = pointProperty.vector2Value.X0Y();

                if (i != pointsProperty.arraySize - 1)
                {
                    var nextPointProperty = pointsProperty.GetArrayElementAtIndex(i + 1);
                    Handles.DrawLine(pointProperty.vector2Value.X0Y(), nextPointProperty.vector2Value.X0Y(), 2);
                }

                Handles.color = Color.yellow;
                Handles.DrawSolidDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward, 0.5f);
            }
        }

        public override void OnSceneGUI(SerializedProperty roomProperty)
        {
            var pointsProperty = GetPointsProperty(roomProperty);

            var closestPointId = -1;

            if (InitMousePosition())
            {
                closestPointId = GetClosestPointId(pointsProperty);
            }

            DrawLine(pointsProperty);

            if (pressedPointId != -1)
            {
                DrawHandle(pressedPointId, pointsProperty.GetArrayElementAtIndex(pressedPointId));
            }
            else if (closestPointId != -1)
            {
                var closestPointProperty = pointsProperty.GetArrayElementAtIndex(closestPointId);
                var position = closestPointProperty.vector2Value;
                var distance = (position - mousePosition).magnitude;

                if (distance < 0.6f)
                {
                    if (DrawHandle(closestPointId, closestPointProperty) && pointsProperty.arraySize > 2)
                    {
                        pointsProperty.DeleteArrayElementAtIndex(closestPointId);
                    }
                }
                else
                {
                    var closestSegmentId = GetClosestSegment(pointsProperty, out var closestPosition);

                    if (closestSegmentId != -1)
                    {
                        distance = (closestPosition - mousePosition).magnitude;

                        if (distance < 0.3f)
                        {
                            Handles.color = Color.green.SetAlpha(0.5f);
                            Handles.DrawSolidDisc(closestPosition.X0Y(), SceneView.currentDrawingSceneView.camera.transform.forward, 0.3f);

                            EditorGUI.BeginChangeCheck();

                            closestPosition = DoXZHandle(closestPosition.X0Y()).XZ();

                            if (EditorGUI.EndChangeCheck())
                            {
                                pointsProperty.InsertArrayElementAtIndex(closestSegmentId + 1);
                                pointsProperty.GetArrayElementAtIndex(closestSegmentId + 1).vector2Value = closestPosition;
                            }
                        }
                    }
                }
            }
        }

        protected virtual int GetClosestSegment(SerializedProperty pointsProperty, out Vector2 closestPosition)
        {
            closestPosition = Vector2.zero;
            var minDistSq = float.MaxValue;
            var closestId = -1;

            for (int i = 0; i < pointsProperty.arraySize - 1; i++)
            {
                var a = pointsProperty.GetArrayElementAtIndex(i).vector2Value;
                var b = pointsProperty.GetArrayElementAtIndex(i + 1).vector2Value;

                var value = ClosestPointOnSegment(a, b, mousePosition);
                var distSq = (value - mousePosition).sqrMagnitude;

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closestPosition = value;
                    closestId = i;
                }
            }

            return closestId;
        }

        protected virtual bool DrawHandle(int id, SerializedProperty pointProperty)
        {
            EditorGUI.BeginChangeCheck();

            var position = pointProperty.vector2Value.X0Y();
            position = DoXZHandle(position);

            if (EditorGUI.EndChangeCheck())
            {
                if (pressedPointId == -1)
                {
                    pressedPointId = id;
                }
            }

            int hot = GUIUtility.hotControl;

            if (lastHotControl != hot)
            {
                if (lastHotControl != 0 && hot == 0)
                {
                    pressedPointId = -1;
                }
                lastHotControl = hot;
            }

            pointProperty.vector2Value = position.XZ();

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual Vector2 ClosestPointOnLine(Vector2 point, List<Vector2> line)
        {
            var closest = Vector2.zero;
            var minDistSq = float.MaxValue;

            for (int i = 0; i < line.Count - 1; i++)
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

        protected virtual Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            var ab = b - a;
            var t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return a + t * ab;
        }
    }
}