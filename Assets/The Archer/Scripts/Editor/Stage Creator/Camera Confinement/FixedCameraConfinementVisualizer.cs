using OctoberStudio.Extensions;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class FixedCameraConfinementVisualizer : CameraConfinementVisualiser
    {
        public override void OnSceneGUI(SerializedProperty roomProperty)
        {
            var pointsProperty = roomProperty.FindPropertyRelative("cameraConfinementPolygon");

            if (pointsProperty.arraySize == 0) pointsProperty.arraySize = 1;

            var firstPointProperty = pointsProperty.GetArrayElementAtIndex(0);

            DrawCameraPoint(firstPointProperty, 0);
        }

        protected virtual void DrawCameraPoint(SerializedProperty pointProperty, int id)
        {
            var position = pointProperty.vector2Value.X0Y();

            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(position, SceneView.currentDrawingSceneView.camera.transform.forward, 0.5f);

            if (IsMouseNearSphere(position, 2))
            {
                EditorGUI.BeginChangeCheck();

                position = DoXZHandle(position);

                pointProperty.vector2Value = position.XZ();
            }
        }
    }
}