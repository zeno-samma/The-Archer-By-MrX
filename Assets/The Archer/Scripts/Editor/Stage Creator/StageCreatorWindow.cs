using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class StageCreatorWindow : EditorWindow
    {
        public static StageCreatorWindow Instance { get; private set; }

        [SerializeField] protected Texture2D dragHandleTexture;
        public Texture2D DragHandleTexture => dragHandleTexture;
        [SerializeField] protected Texture2D circleTexture;
        public Texture2D CircleTexture => circleTexture;

        [Space]
        [SerializeField] protected Texture2D defaultEnemyTexture;
        [SerializeField] protected Texture2D defaultChestTexture;
        [SerializeField] protected Texture2D defaultPropTexture;

        public Texture2D DefaultEnemyTexture => defaultEnemyTexture;
        public Texture2D DefaultChestTexture => defaultChestTexture;
        public Texture2D DefaultPropTexture => defaultPropTexture;

        public PageType CurrentPage { get; protected set; } = PageType.StageDatabase;

        public StageCreatorContextManager ContextManager { get; protected set; }

        public StageDatabasePage StageDatabasePage { get; protected set; }
        public StagePage StagePage { get; protected set; }

        public static GUIStyle HeaderTextStyle { get; protected set; }

        public bool IsTesting { get; set; }

        #region Properties

        protected StageController stageController;
        protected StageController StageController
        {
            get
            {
                if (stageController == null)
                {
#if UNITY_6000_4_OR_NEWER
                    stageController = FindAnyObjectByType<StageController>();
#elif UNITY_6000_0_OR_NEWER
                    stageController = FindFirstObjectByType<StageController>();
#else
                    stageController = FindObjectOfType<StageController>();
#endif
                }

                return stageController;
            }
        }

        public StageDatabase StageDatabase
        {
            get
            {
                if (StageController == null) return null;
                return StageController.StageDatabase;
            }
        }

        protected SerializedObject stageDatabaseObject;
        public SerializedObject StageDatabaseObject
        {
            get
            {
                if (stageDatabaseObject == null)
                {
                    if (StageDatabase == null) return null;
                    stageDatabaseObject = new SerializedObject(StageDatabase);
                }

                return stageDatabaseObject;
            }
        }

        public ExperienceData ExperienceData
        {
            get
            {
                if (StageDatabase == null) return null;
                return StageDatabase.ExperienceData;
            }
        }

        public int LastSelectedStageId
        {
            get => StageDatabaseObject.FindProperty("lastSelectedStageId").intValue;
            set => StageDatabaseObject.FindProperty("lastSelectedStageId").intValue = value;
        }

        public int LastSelectedRoomId
        {
            get => StageDatabaseObject.FindProperty("lastSelectedRoomId").intValue;
            set => StageDatabaseObject.FindProperty("lastSelectedRoomId").intValue = value;
        }

        public int LastSelectedWaveId
        {
            get => StageDatabaseObject.FindProperty("lastSelectedWaveId").intValue;
            set => StageDatabaseObject.FindProperty("lastSelectedWaveId").intValue = value;
        }

        #endregion

        [MenuItem("Tools/October/Stage Creator #s")]
        public static void OpenWindow()
        {
            StageCreatorWindow wnd = GetWindow<StageCreatorWindow>();
            var content = new GUIContent("Stage Creator");

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/The Archer/Sprites/Editor/editor_stage_creator.png");
            if (icon != null)
            {
                content.image = icon;
            }

            wnd.titleContent = content;
            wnd.minSize = new Vector2(760, 800);
        }

        bool shouldOpenStage = false;

        protected virtual void OnEnable()
        {
            Instance = this;

            if (StageDatabaseObject != null)
            {
                StageDatabasePage = new StageDatabasePage(StageDatabaseObject);
                StagePage = new StagePage();

                if (LastSelectedRoomId >= 0)
                {
                    shouldOpenStage = true;
                }
            }

            CurrentPage = PageType.StageDatabase;

            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;

            EditorSceneManager.sceneOpening -= OnSceneOpening;
            EditorSceneManager.sceneOpening += OnSceneOpening;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        protected virtual void OnSceneOpening(string path, OpenSceneMode mode)
        {
            if (StagePage != null && StagePage.IsOpened)
            {
                OpenPage(PageType.StageDatabase);
            }
        }

        protected virtual void PlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (StagePage != null && StagePage.IsOpened)
                {
                    if (!IsTesting)
                    {
                        StagePage.ApplyTestingData();
                    }

                    IsTesting = false;
                }
                Disable();
            }
        }

        protected virtual void OnGUI()
        {
            if (HeaderTextStyle == null) HeaderTextStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15, alignment = TextAnchor.MiddleCenter };

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Disabled when playmod is active.");
                return;
            }

            if (StageController == null)
            {
                EditorGUILayout.LabelField("Stage Controller not found in the scene.");

                string[] guiID = AssetDatabase.FindAssets("t:SceneSettings");

                if (guiID != null)
                {
                    var sceneSettings = AssetDatabase.LoadAssetAtPath<SceneSettings>(AssetDatabase.GUIDToAssetPath(guiID[0]));

                    if (sceneSettings != null && sceneSettings.GameScene != null)
                    {
                        if (GUILayout.Button("Open Game Scene"))
                        {
                            var path = AssetDatabase.GetAssetPath(sceneSettings.GameScene.Scene);
                            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        }
                    }
                }

                return;
            }

            if (StageDatabase == null)
            {
                EditorGUILayout.LabelField("Stage Database not assigned in Stage Controller.");
                return;
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("You cannot edit stages while the game is running.");
                return;
            }

            if (StageDatabasePage == null || StagePage == null)
            {
                StageDatabasePage = new StageDatabasePage(StageDatabaseObject);
                StagePage = new StagePage();

                if (LastSelectedRoomId >= 0)
                {
                    shouldOpenStage = true;
                }
            }

            if (ContextManager == null) ContextManager = new StageCreatorContextManager(this);

            if (CurrentPage == PageType.StageDatabase)
            {
                StageDatabasePage.Draw();

            }
            else
            {
                StagePage.Draw();
            }

            if (shouldOpenStage)
            {
                CurrentPage = PageType.Stage;
                StagePage.Open(StageDatabasePage.GetStageObject(LastSelectedStageId), LastSelectedStageId);

                shouldOpenStage = false;
            }

            StageDatabaseObject.ApplyModifiedProperties();
        }

        public virtual void OpenPage(PageType pageType)
        {
            if (CurrentPage == PageType.Stage)
            {
                StagePage.OnDisable();
            }

            CurrentPage = pageType;
            switch (pageType)
            {
                case PageType.StageDatabase:
                    LastSelectedRoomId = -1;
                    LastSelectedWaveId = -1;
                    StageDatabasePage.Open();
                    break;
                case PageType.Stage:
                    LastSelectedStageId = StageDatabasePage.DoubleClickedStageIndex;
                    StagePage.Open(StageDatabasePage.DoubleClickedStageObject, StageDatabasePage.DoubleClickedStageIndex);
                    break;
            }
        }

        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            SceneView.duringSceneGui -= OnSceneGUI;

            EditorSceneManager.sceneOpening -= OnSceneOpening;

            Disable();

            Instance = null;
        }

        protected virtual void OnSceneGUI(SceneView sceneView)
        {
            if (StageController == null)
            {
                return;
            }

            if (StageDatabase == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                return;
            }

            switch (CurrentPage)
            {
                case PageType.StageDatabase:
                    if (StageDatabasePage != null) StageDatabasePage.OnSceneGUI(sceneView);
                    break;
                case PageType.Stage:
                    if (StagePage != null) StagePage.OnSceneGUI(sceneView);
                    break;
            }
        }

        protected virtual void Disable()
        {
            if (StagePage != null) StagePage.OnDisable();
        }

        protected CameraDataBuffer cameraDataBuffer;

        public virtual CameraDataBuffer GetCameraDataBuffer()
        {
            return cameraDataBuffer;
        }

        public virtual void SetCameraDataBuffer(CameraDataBuffer cameraDataBuffer)
        {
            this.cameraDataBuffer = cameraDataBuffer;
        }

        public class CameraDataBuffer
        {
            protected CameraMovementType cameraMovementType;
            protected Vector2[] cameraConfinementPolygon;

            public CameraDataBuffer(SerializedProperty roomProperty)
            {
                cameraMovementType = (CameraMovementType)roomProperty.FindPropertyRelative("cameraMovementType").intValue;

                var polygonProperty = roomProperty.FindPropertyRelative("cameraConfinementPolygon");

                cameraConfinementPolygon = new Vector2[polygonProperty.arraySize];
                for (int i = 0; i < polygonProperty.arraySize; i++)
                {
                    cameraConfinementPolygon[i] = polygonProperty.GetArrayElementAtIndex(i).vector2Value;
                }
            }

            public virtual void Apply(SerializedProperty roomProperty)
            {
                roomProperty.FindPropertyRelative("cameraMovementType").intValue = (int)cameraMovementType;

                var polygonProperty = roomProperty.FindPropertyRelative("cameraConfinementPolygon");
                polygonProperty.arraySize = cameraConfinementPolygon.Length;

                for (int i = 0; i < cameraConfinementPolygon.Length; i++)
                {
                    polygonProperty.GetArrayElementAtIndex(i).vector2Value = cameraConfinementPolygon[i];
                }

                Undo.RecordObject(roomProperty.serializedObject.targetObject, "Paste Camera Data");
                roomProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        public enum PageType
        {
            StageDatabase,
            Stage,
        }
    }
}