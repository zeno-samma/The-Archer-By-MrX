using System.Collections.Generic;
using MrX.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Screen = UnityEngine.Device.Screen;

namespace MrX.EditorTools
{
    [CustomEditor(typeof(ArmoryResponsiveLayout))]
    public sealed class ArmoryResponsiveLayoutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("Open Edit Mode Layout Trial"))
                    MenuLayoutTrialWindow.Open((RectTransform)((ArmoryResponsiveLayout)target).transform);
            }
            DrawDefaultInspector();
        }
    }

    public abstract class MenuLayoutPreviewWindow : EditorWindow
    {
        [SerializeField] private RectTransform source;
        [SerializeField] private Vector2 lastGameViewPixels;
        [SerializeField] private Vector2 lastSafeViewPixels;
        [SerializeField] private Vector2 lastAuthoredSize;
        private RectTransform context;
        private Scene previewScene;
        private RectTransform clonedRoot;
        private Canvas canvas;
        private Camera camera;
        private RenderTexture image;
        private readonly List<RenderTexture> heroImages = new List<RenderTexture>();
        private readonly List<(ArmoryResponsiveLayout source, ArmoryResponsiveLayout clone)> armories = new();
        private readonly List<MainMenuPageBehavior> pages = new();
        private readonly Dictionary<GameObject, bool> visibility = new();
        private readonly Dictionary<Canvas, bool> canvasVisibility = new();
        private readonly Dictionary<CanvasGroup, float> alpha = new();
        private Transform visibleFocus;
        private Vector2 layoutSize;
        private bool needsRender;
        private bool rebuildQueued;
        private int dragControl;
        private int undoGroup;
        private float dragStartY;
        private float dragStartFloor;

        protected RectTransform Source => source;
        protected RectTransform PreviewScope => FindClone(source) as RectTransform;
        protected RectTransform PreviewFrame => canvas ? (RectTransform)canvas.transform : null;
        protected Vector2 PreviewPixels { get; private set; }
        protected RectTransform FindPreviewRect(RectTransform original) => FindClone(original) as RectTransform;
        protected virtual bool UseAuthoredCanvas => false;
        protected virtual RectTransform IsolatedPanel => null;

        protected void Preview(RectTransform panel)
        {
            var menuContent = panel && panel.name == "MainMenu_Android" ? panel.Find("MainMenu_Android") as RectTransform : null;
            if (menuContent) panel = menuContent;
            if (source != panel)
            {
                source = panel;
                OnScopeChanged();
            }
            RebuildPreview();
            minSize = new Vector2(350f, 460f);
            Show();
            Focus();
        }

        protected void InvalidatePreview()
        {
            needsRender = true;
            Repaint();
        }

        protected void RebuildPreview()
        {
            ReleasePreview();
            InvalidatePreview();
        }

        protected virtual void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload += ReleasePreview;
            Undo.postprocessModifications += WatchAuthoringChanges;
            Undo.undoRedoPerformed += QueueRebuild;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleasePreview;
            Undo.postprocessModifications -= WatchAuthoringChanges;
            Undo.undoRedoPerformed -= QueueRebuild;
            ReleasePreview();
        }

        protected void OnInspectorUpdate() => Repaint();

        private UndoPropertyModification[] WatchAuthoringChanges(UndoPropertyModification[] changes)
        {
            if (!context) return changes;
            foreach (var change in changes)
            {
                var component = change.currentValue.target as Component;
                if (!component || !component.transform.IsChildOf(context)) continue;
                if (component is ArmoryResponsiveLayout) Repaint();
                else QueueRebuild();
                break;
            }
            return changes;
        }

        private void QueueRebuild()
        {
            rebuildQueued = true;
            Repaint();
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) ReleasePreview();
            Repaint();
        }

        protected void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("Open the layout trial in Edit Mode.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var selected = EditorGUILayout.ObjectField("Panel", source, typeof(RectTransform), true) as RectTransform;
                if (selected != source) Preview(selected);
                using (new EditorGUI.DisabledScope(!MenuLayoutTrialWindow.SelectedPanel))
                    if (GUILayout.Button("Use Selection", GUILayout.Width(105))) Preview(MenuLayoutTrialWindow.SelectedPanel);
            }

            if (!source)
            {
                if (previewScene.IsValid()) ReleasePreview();
                EditorGUILayout.HelpBox("Select a UI panel or prefab.", MessageType.Info);
                return;
            }

            var armory = source.GetComponentInParent<ArmoryResponsiveLayout>(true);
            var settings = armory ? new SerializedObject(armory) : null;
            DrawPreviewControls(settings);
            Vector2 size = GetLayoutSize(source, UseAuthoredCanvas);
            Rect space = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Layout || space.width <= 0 || space.height <= 0) return;
            float scale = Mathf.Min(space.width / size.x, space.height / size.y);
            var previewRect = new Rect(space.center - size * scale * .5f, size * scale);
            int control = GUIUtility.GetControlID(FocusType.Passive);
            var backdrop = settings?.FindProperty("backdrop").objectReferenceValue as Image;
            var sprite = backdrop ? backdrop.sprite : null;
            float spriteHeight = sprite ? size.x * sprite.rect.height / sprite.rect.width : 0f;
            HandlePreviewInput(control, previewRect, scale, spriteHeight, settings);

            if (Event.current.type != EventType.Repaint) return;
            if (rebuildQueued) RebuildPreview();
            if (!clonedRoot) BuildPreview();
            if (!clonedRoot) return;
            RenderPreview(size);
            if (image) GUI.DrawTexture(previewRect, image, ScaleMode.StretchToFill, false);
            DrawPreviewOverlay(previewRect, size);
        }

        protected virtual void DrawPreviewControls(SerializedObject settings)
        {
            EditorGUILayout.PropertyField(settings.FindProperty("floorOnSprite"));
            EditorGUILayout.PropertyField(settings.FindProperty("floorHeight"));
            settings.ApplyModifiedProperties();
            EditorGUILayout.LabelField("Drag the image vertically to align the floor.", EditorStyles.miniLabel);
        }

        protected virtual void HandlePreviewInput(int control, Rect rect, float scale, float spriteHeight, SerializedObject settings) =>
            EditBackdrop(control, rect, spriteHeight * scale, settings, settings.FindProperty("floorOnSprite"));

        protected virtual void OnScopeChanged() { }
        protected virtual void BeforeLayout() { }
        protected virtual void AfterLayout() { }
        protected virtual Transform FocusedElement => source;
        protected virtual void DrawPreviewOverlay(Rect rect, Vector2 size) { }

        protected static Vector2 GetGameViewPixels(bool useSafeArea)
        {
            Vector2 available = Handles.GetMainGameViewSize();
            if (available.x <= 0 || available.y <= 0) available = new Vector2(720, 1604);
            var safe = Screen.safeArea;
            if (useSafeArea && Mathf.Abs(available.x - Screen.width) <= 1f && Mathf.Abs(available.y - Screen.height) <= 1f
                && safe.width > 0 && safe.height > 0)
                available = safe.size;
            return available;
        }

        private Vector2 GetLayoutSize(RectTransform content, bool useAuthoredCanvas)
        {
            // Unity can report the maximized editor pane's size after it hides the Game View.
            if (!IsolatedPanel || !maximized || lastGameViewPixels == Vector2.zero)
            {
                lastGameViewPixels = GetGameViewPixels(false);
                lastSafeViewPixels = GetGameViewPixels(true);
                if (IsolatedPanel && FindContext(content).parent is RectTransform parent)
                    lastAuthoredSize = parent.rect.size;
            }
            if (useAuthoredCanvas && lastAuthoredSize.x > 0f && lastAuthoredSize.y > 0f)
            {
                PreviewPixels = lastGameViewPixels;
                return lastAuthoredSize;
            }
            PreviewPixels = lastSafeViewPixels;

            var frame = content.GetComponentInParent<ResponsiveFrame>();
            if (frame && frame.TryGetPreviewSize(content, PreviewPixels, out var responsiveSize)) return responsiveSize;
            return new Vector2(1080f, Mathf.Max(1760f, 1080f * PreviewPixels.y / PreviewPixels.x));
        }

        private static RectTransform FindContext(RectTransform selected)
        {
            var current = selected;
            while (current.parent is RectTransform parent)
            {
                if (current.name == "MainMenu_Android" || parent.name == "MainMenu_Android")
                    return current;
                current = parent;
            }
            return current;
        }

        private Transform FindClone(Transform original)
        {
            if (!original || !clonedRoot) return null;
            if (original == context) return clonedRoot;
            var parent = FindClone(original.parent);
            int index = original.GetSiblingIndex();
            return parent && index < parent.childCount ? parent.GetChild(index) : null;
        }

        private void EditBackdrop(int control, Rect rect, float imageHeight, SerializedObject settings, SerializedProperty floor)
        {
            if (imageHeight <= 0) return;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
            var current = Event.current;
            switch (current.GetTypeForControl(control))
            {
                case EventType.MouseDown when current.button == 0 && rect.Contains(current.mousePosition):
                    Undo.IncrementCurrentGroup();
                    undoGroup = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Align Armory Background");
                    dragControl = GUIUtility.hotControl = control;
                    dragStartY = current.mousePosition.y;
                    dragStartFloor = floor.floatValue;
                    current.Use();
                    break;
                case EventType.MouseDrag when dragControl == control:
                    floor.floatValue = Mathf.Clamp01(dragStartFloor + (current.mousePosition.y - dragStartY) / imageHeight);
                    settings.ApplyModifiedProperties();
                    current.Use();
                    Repaint();
                    break;
                case EventType.MouseUp when dragControl == control:
                    GUIUtility.hotControl = dragControl = 0;
                    Undo.CollapseUndoOperations(undoGroup);
                    current.Use();
                    break;
            }
        }

        private void BuildPreview()
        {
            context = FindContext(source);
            previewScene = EditorSceneManager.NewPreviewScene();
            var root = CreateRoot("Menu Canvas Preview", typeof(RectTransform), typeof(Canvas));
            root.SetActive(false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var clone = Instantiate(context.gameObject, root.transform, false);
            clonedRoot = (RectTransform)clone.transform;
            root.layer = clone.layer;

            if (context.GetComponent<MainMenuPageBehavior>() || context.name == "MainMenu_Android")
            {
                clonedRoot.anchorMin = Vector2.zero;
                clonedRoot.anchorMax = Vector2.one;
                clonedRoot.offsetMin = clonedRoot.offsetMax = Vector2.zero;
                clonedRoot.localScale = Vector3.one;
            }

            pages.AddRange(context.GetComponentsInChildren<MainMenuPageBehavior>(true));
            foreach (var original in context.GetComponentsInChildren<ArmoryResponsiveLayout>(true))
            {
                var copied = FindClone(original.transform).GetComponent<ArmoryResponsiveLayout>();
                _ = copied.SlotGroup;
                copied.enabled = false;
                armories.Add((original, copied));
            }

            foreach (var heroPreview in clone.GetComponentsInChildren<HeroPreviewBehavior>(true))
                CreateHeroPreview(new SerializedObject(heroPreview));
            foreach (var behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour && behaviour is not UIBehaviour) behaviour.enabled = false;
            }
            foreach (var childCamera in clone.GetComponentsInChildren<Camera>(true)) childCamera.enabled = false;
            foreach (var audioSource in clone.GetComponentsInChildren<AudioSource>(true)) audioSource.enabled = false;
            clone.SetActive(true);
            root.SetActive(true);

            camera = CreateRoot("Menu UI Preview Camera", typeof(Camera)).GetComponent<Camera>();
            camera.enabled = false;
            camera.scene = previewScene;
            camera.cullingMask = 1 << clone.layer;
            foreach (var child in clone.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = clone.layer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.04f, .11f, .20f, 1);
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -1000);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 2000;
            needsRender = true;
        }

        private void CreateHeroPreview(SerializedObject settings)
        {
            var raw = (RawImage)settings.FindProperty("rawImage").objectReferenceValue;
            var rigPrefab = (GameObject)settings.FindProperty("heroPreviewPrefab").objectReferenceValue;
            var catalogue = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var hero = catalogue && catalogue.armory ? catalogue.armory.GetDefaultHero() : null;
            if (!raw || !raw.transform.IsChildOf(clonedRoot) || !rigPrefab || !hero || !hero.Prefab) return;

            var holder = CreateRoot("Hero Preview");
            holder.SetActive(false);
            var rig = Instantiate(rigPrefab, holder.transform, false);
            rig.transform.SetPositionAndRotation(settings.FindProperty("previewPosition").vector3Value,
                Quaternion.Euler(settings.FindProperty("previewRotation").vector3Value));
            var heroCamera = rig.GetComponentInChildren<Camera>(true);
            if (!heroCamera) return;
            heroCamera.enabled = false;
            heroCamera.scene = previewScene;
            var model = Instantiate(hero.Prefab, rig.transform, false);
            foreach (var behaviour in holder.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour) behaviour.enabled = false;
            foreach (var animator in model.GetComponentsInChildren<Animator>(true)) animator.fireEvents = false;
            var light = CreateRoot("Hero Preview Light", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.cullingMask = heroCamera.cullingMask;
            light.transform.rotation = heroCamera.transform.rotation * Quaternion.Euler(25, -25, 0);
            var texture = new RenderTexture(384, 384, 16, RenderTextureFormat.ARGBHalf) { hideFlags = HideFlags.HideAndDontSave };
            heroImages.Add(texture);
            heroCamera.targetTexture = texture;
            holder.SetActive(true);
            heroCamera.Render();
            raw.texture = texture;
        }

        private GameObject CreateRoot(string objectName, params System.Type[] components)
        {
            var root = new GameObject(objectName, components) { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(root, previewScene);
            return root;
        }

        private void ApplyVisibility()
        {
            var focus = FocusedElement ? FocusedElement : source;
            if (IsolatedPanel && visibleFocus == focus) return;
            visibleFocus = focus;
            foreach (var entry in visibility) if (entry.Key) entry.Key.SetActive(entry.Value);
            foreach (var entry in canvasVisibility) if (entry.Key) entry.Key.enabled = entry.Value;
            foreach (var entry in alpha) if (entry.Key) entry.Key.alpha = entry.Value;
            visibility.Clear();
            canvasVisibility.Clear();
            alpha.Clear();

            var panel = FindPreviewRect(IsolatedPanel);
            if (panel)
            {
                foreach (Transform sibling in panel.parent)
                {
                    if (sibling == panel || !sibling.gameObject.activeSelf) continue;
                    visibility.TryAdd(sibling.gameObject, true);
                    sibling.gameObject.SetActive(false);
                }
            }
            var page = focus.GetComponentInParent<MainMenuPageBehavior>(true);
            if (page && FindClone(page.transform))
            {
                foreach (var candidate in pages)
                {
                    if (!candidate || candidate == page) continue;
                    var other = FindClone(candidate.transform);
                    if (other && other.TryGetComponent<CanvasGroup>(out var group))
                    {
                        alpha.TryAdd(group, group.alpha);
                        group.alpha = 0;
                    }
                }
            }

            for (var node = FindClone(focus); node && node != PreviewFrame; node = node.parent)
            {
                visibility.TryAdd(node.gameObject, node.gameObject.activeSelf);
                node.gameObject.SetActive(true);
                if (node.TryGetComponent<Canvas>(out var targetCanvas))
                {
                    canvasVisibility.TryAdd(targetCanvas, targetCanvas.enabled);
                    targetCanvas.enabled = true;
                }
                if (node.TryGetComponent<CanvasGroup>(out var group))
                {
                    alpha.TryAdd(group, group.alpha);
                    group.alpha = 1;
                }
            }
        }

        private void RenderPreview(Vector2 size)
        {
            foreach (var pair in armories)
            {
                var settings = new SerializedObject(pair.source);
                var copy = new SerializedObject(pair.clone);
                var property = settings.GetIterator();
                for (bool visible = property.NextVisible(true); visible; visible = property.NextVisible(false))
                    if (property.propertyType == SerializedPropertyType.Float || property.propertyType == SerializedPropertyType.Vector2)
                        copy.CopyFromSerializedPropertyIfDifferent(property);
                needsRender |= copy.ApplyModifiedPropertiesWithoutUndo();
            }
            if (!needsRender && layoutSize == size) return;

            layoutSize = size;
            BeforeLayout();
            PreviewFrame.sizeDelta = size;
            ApplyVisibility();
            if (!UseAuthoredCanvas)
            {
                var frame = context.GetComponentInParent<ResponsiveFrame>();
                if (frame) frame.ApplyPreviewPins(FindPreviewRect);
                foreach (var pair in armories)
                    pair.clone.ApplyLayout(((RectTransform)pair.clone.transform).rect.size);
            }
            AfterLayout();

            const int width = 512;
            int height = Mathf.Clamp(Mathf.RoundToInt(width * size.y / size.x), 1, 2048);
            if (!image || image.height != height)
            {
                if (image) DestroyImmediate(image);
                image = new RenderTexture(width, height, 24) { hideFlags = HideFlags.HideAndDontSave };
                camera.targetTexture = image;
            }

            camera.aspect = size.x / size.y;
            camera.orthographicSize = size.y * .5f;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            needsRender = false;
        }

        private void ReleasePreview()
        {
            if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
            previewScene = default;
            if (image) DestroyImmediate(image);
            foreach (var texture in heroImages)
                if (texture) DestroyImmediate(texture);
            heroImages.Clear();
            armories.Clear();
            pages.Clear();
            visibility.Clear();
            canvasVisibility.Clear();
            alpha.Clear();
            visibleFocus = null;
            context = null;
            clonedRoot = null;
            image = null;
            rebuildQueued = false;
        }
    }
}
