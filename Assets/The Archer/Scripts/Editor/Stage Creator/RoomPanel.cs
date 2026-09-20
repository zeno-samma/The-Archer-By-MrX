using OctoberStudio.Save;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.StageCreator
{
    public class RoomPanel
    {
        protected EditorGridView<GameObject, RoomPropGridItem> gridView = new EditorGridView<GameObject, RoomPropGridItem>(StageCreatorWindow.Instance.DefaultPropTexture);
        protected GUIStyle textStyle;
        protected GUIStyle gridTextStyle;
        public StagePage StagePage { get; protected set; }

        public bool IsCameraModEnabled { get; protected set; } = false;
        protected CameraConfinementVisualiser CameraConfinementVisualiser { get; set; }

        protected RoomPrefabsOverridesIndicator RoomPrefabsOverridesIndicator { get; set; }

        public RoomPanel(StagePage stagePage, RoomPrefabsHandler roomPrefabsHandler)
        {
            StagePage = stagePage;

            IsCameraModEnabled = false;
            CameraConfinementVisualiser = null;

            RoomPrefabsOverridesIndicator = new RoomPrefabsOverridesIndicator(roomPrefabsHandler);
        }

        protected virtual void Update()
        {
            SceneView.RepaintAll();
        }

        #region GUI

        public virtual void Draw(SerializedProperty roomProperty, UnityAction<GameObject> spawnPrefabGridAction)
        {
            if (textStyle == null) textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = Color.black }
            };

            if (gridTextStyle == null) gridTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Room Settings", StageCreatorWindow.HeaderTextStyle, GUILayout.ExpandWidth(false), GUILayout.Width(170));

            DrawGroupDropDown(roomProperty);
            DrawRoomMusicProperty(roomProperty);

            GUI.enabled = DefaultSceneLoaderActions.Enabled;
            var tooltip = GUI.enabled ?
                "Start playmode with and automatically enter selected room" :
                "Default Scene Loader is disabled. Starting play mode will not load necessary systems and will break the game. \n" +
                "To enable Default Scene Loader, use 'Tools -> October -> Default Scene Loader Enabled'";

            if (GUILayout.Button(new GUIContent("Test Room", tooltip), GUILayout.Width(172)))
            {
                var saveDatabase = SaveManager.GetSaveDatabaseEditor();

                saveDatabase.Init();
                var save = saveDatabase.GetSave<StageSave>("Stage");
                save.Init();
                save.SetTestingData(StageCreatorWindow.Instance.StagePage.StageIndex, StageCreatorWindow.Instance.StagePage.RoomIndex, 0);
                saveDatabase.Flush();

                SaveManager.SaveSaveDatabaseEditor(saveDatabase);

                StageCreatorWindow.Instance.StagePage.SaveScene();
                StageCreatorWindow.Instance.StagePage.ClearScene(true);

                StageCreatorWindow.Instance.IsTesting = true;

                EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
            }
            GUI.enabled = true;

            RoomPrefabsOverridesIndicator.Draw();

            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            SerializedPropertyExtensions.DrawVerticalSeparator(1);
            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Timings", StageCreatorWindow.HeaderTextStyle, GUILayout.ExpandWidth(false), GUILayout.Width(120));
            DrawSpawnDelayProperty(roomProperty);
            DrawDelayBetweenWavesProperty(roomProperty);
            DrawExitDelayProperty(roomProperty);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            SerializedPropertyExtensions.DrawVerticalSeparator(1);
            GUILayout.Space(5f);

            DrawCameraProperties(roomProperty);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);

            gridView.Draw(130, new Rect(0, 0, 55, 55), 3, 3, spawnPrefabGridAction);

            EditorGUILayout.EndVertical();

            roomProperty.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawGroupDropDown(SerializedProperty roomProperty)
        {
            var groupIdProperty = roomProperty.FindPropertyRelative("groupId");

            var groups = new List<string> { "None", "Group 1", "Group 2", "Group 3", "Group 4", "Group 5", "Group 6", "Group 7", "Group 8", "Group 9", "Group 10" };

            if (groupIdProperty.intValue < 0) groupIdProperty.intValue = 0;

            int selectedIndex = groupIdProperty.intValue;

            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Group Id", "Rooms with the same group id will be randomly shuffeled when playing");
            EditorGUILayout.LabelField(label, GUILayout.Width(75));
            selectedIndex = EditorGUILayout.Popup(selectedIndex, groups.ToArray(), GUILayout.Width(95));
            groupIdProperty.intValue = selectedIndex;
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawRoomMusicProperty(SerializedProperty roomProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Room Music", "If assigned, this music start playing at the start of this room");
            EditorGUILayout.LabelField(label, GUILayout.Width(75));
            var roomMusicProperty = roomProperty.FindPropertyRelative("roomMusic");
            EditorGUILayout.PropertyField(roomMusicProperty, new GUIContent(""), GUILayout.Width(95));
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawSpawnDelayProperty(SerializedProperty roomProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Start Delay", "Delay between entering the room and starting the first wave");
            EditorGUILayout.LabelField(label, GUILayout.Width(70));
            var spawnDelayProperty = roomProperty.FindPropertyRelative("firstWaveStartDelay");
            EditorGUILayout.PropertyField(spawnDelayProperty, new GUIContent(""), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        public virtual void DrawDelayBetweenWavesProperty(SerializedProperty roomProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Wave Delay", "Delay between waves");
            EditorGUILayout.LabelField(label, GUILayout.Width(70));
            var waveDelayProperty = roomProperty.FindPropertyRelative("delayBetweenWaves");
            EditorGUILayout.PropertyField(waveDelayProperty, new GUIContent(""), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        public virtual void DrawExitDelayProperty(SerializedProperty roomProperty)
        {
            EditorGUILayout.BeginHorizontal();
            var label = new GUIContent("Exit Delay", "Delay between the end of the last wave end exit spawn");
            EditorGUILayout.LabelField(label, GUILayout.Width(70));
            var waveDelayProperty = roomProperty.FindPropertyRelative("delayBeforeExitSpawn");
            EditorGUILayout.PropertyField(waveDelayProperty, new GUIContent(""), GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawCameraProperties(SerializedProperty roomProperty)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Camera", StageCreatorWindow.HeaderTextStyle, GUILayout.ExpandWidth(false), GUILayout.Width(178));

            EditorGUI.BeginChangeCheck();

            var cameraMovementTypeProperty = roomProperty.FindPropertyRelative("cameraMovementType");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Camera Type", GUILayout.Width(90));
            cameraMovementTypeProperty.intValue = (int)(CameraMovementType)EditorGUILayout.EnumPopup((CameraMovementType)cameraMovementTypeProperty.intValue, GUILayout.Width(85));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                InitCameraVisualizer(roomProperty);
            }

            if (cameraMovementTypeProperty.intValue != (int)CameraMovementType.Free)
            {
                var text = IsCameraModEnabled ? "Disable Camera Edit Mode" : "Enable Camera Edit Mode";
                if (GUILayout.Button(text, GUILayout.Width(178)))
                {
                    IsCameraModEnabled = !IsCameraModEnabled;
                    SceneView.RepaintAll();

                    InitCameraVisualizer(roomProperty);

                    if (IsCameraModEnabled)
                    {
                        EditorApplication.update -= Update;
                        EditorApplication.update += Update;
                    }
                    else
                    {
                        EditorApplication.update -= Update;
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy", GUILayout.Width(85)))
            {
                StageCreatorWindow.Instance.SetCameraDataBuffer(new StageCreatorWindow.CameraDataBuffer(roomProperty));
            }

            GUILayout.Space(5);

            var cameraDataBuffer = StageCreatorWindow.Instance.GetCameraDataBuffer();
            GUI.enabled = cameraDataBuffer != null;
            if (GUILayout.Button("Paste", GUILayout.Width(85)))
            {
                cameraDataBuffer.Apply(roomProperty);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        protected virtual void InitCameraVisualizer(SerializedProperty roomProperty)
        {
            var cameraMovementTypeProperty = roomProperty.FindPropertyRelative("cameraMovementType");

            switch ((CameraMovementType)cameraMovementTypeProperty.intValue)
            {
                case CameraMovementType.Free:
                    CameraConfinementVisualiser = null;
                    break;
                case CameraMovementType.Fixed:
                    CameraConfinementVisualiser = new FixedCameraConfinementVisualizer();
                    break;
                case CameraMovementType.Line:
                    CameraConfinementVisualiser = new LineCameraConfinementVisualiser();
                    break;
                case CameraMovementType.Polygon:
                    CameraConfinementVisualiser = new PolygonCameraConfinementVisualiser();
                    break;
                default:
                    CameraConfinementVisualiser = null;
                    break;
            }
        }

        public virtual void SetGridPrefabs(List<GameObject> prefabs)
        {
            gridView.SetItems(prefabs);
        }

        #endregion

        public virtual void Clear()
        {
            IsCameraModEnabled = false;
            CameraConfinementVisualiser = null;

            EditorApplication.update -= Update;
        }

        public virtual bool OnSceneGUI(SerializedProperty roomProperty)
        {
            if (textStyle == null) textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = Color.black }
            };

            if (IsCameraModEnabled)
            {
                if (CameraConfinementVisualiser != null)
                {
                    CameraConfinementVisualiser.OnSceneGUI(roomProperty);
                }

                roomProperty.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var playerSpawnPointProperty = roomProperty.FindPropertyRelative("playerSpawnPoint");
                var playerSpawnPoint = playerSpawnPointProperty.vector3Value;
                Handles.color = Color.green;
                Handles.DrawSolidDisc(playerSpawnPoint, SceneView.currentDrawingSceneView.camera.transform.forward, 0.5f);

                Handles.Label(playerSpawnPoint, "Player Spawn Point", textStyle);

                if (IsMouseNearSphere(playerSpawnPoint, 0.6f))
                {
                    playerSpawnPoint = DoXZHandle(playerSpawnPoint);

                    playerSpawnPointProperty.vector3Value = playerSpawnPoint;
                }
            }

            roomProperty.serializedObject.ApplyModifiedProperties();

            return !IsCameraModEnabled;
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

        protected virtual bool IsMouseNearSphere(Vector3 center, float radius)
        {
            var guiPos = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(guiPos);

            var closest = ray.origin + ray.direction * Vector3.Dot(center - ray.origin, ray.direction);

            var dist = Vector3.Distance(closest, center);
            return dist <= radius;
        }
    }
}