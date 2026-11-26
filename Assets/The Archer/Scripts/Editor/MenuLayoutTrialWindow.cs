using System;
using System.Collections.Generic;
using MrX.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.EditorTools
{
    public sealed class MenuLayoutTrialWindow : MenuLayoutPreviewWindow
    {
        private static readonly string[] ArmoryGroups = { "Parents", "Stats", "Equipment", "Inventory", "All children" };
        private static readonly string[] MenuGroups = { "Parents", "All children" };
        private const string MenuPath = "GameObject/MrX Studio/Open UI Layout Trial";
        private const string ToolsPath = "Tools/MrX Studio/Main Menu Layout Trial";
        private const int InventoryGroup = 3;
        private static readonly Color Outline = new Color(.24f, .89f, 1f, 1f);

        [Serializable]
        private struct Adjustment
        {
            public int element;
            public Vector2 position;
            public Vector2 size;
            public float scale;
        }

        private readonly struct RectState
        {
            private readonly Vector2 min, max, pivot, size, position;
            private readonly Vector3 scale;

            public RectState(RectTransform rect)
            {
                min = rect.anchorMin;
                max = rect.anchorMax;
                pivot = rect.pivot;
                size = rect.sizeDelta;
                position = rect.anchoredPosition;
                scale = rect.localScale;
            }

            public void Restore(RectTransform rect)
            {
                rect.anchorMin = min;
                rect.anchorMax = max;
                rect.pivot = pivot;
                rect.sizeDelta = size;
                rect.anchoredPosition = position;
                rect.localScale = scale;
            }
        }

        [SerializeField] private int group;
        [SerializeField] private int selected;
        [SerializeField] private List<Adjustment> adjustments = new List<Adjustment>();
        [SerializeField] private bool tryGrid;
        [SerializeField] private int columns = 6;
        [SerializeField] private float gap = 10f;
        [SerializeField] private float inventoryScroll;

        private RectTransform indexedSource;
        private RectTransform indexedPreview;
        private RectTransform[] sourceNodes;
        private Transform[] sourceParents;
        private RectTransform[] previewNodes;
        private string[] labels;
        private int[] choices;
        private int indexedGroup = -1;
        private readonly Dictionary<RectTransform, RectState> adjustedRects = new Dictionary<RectTransform, RectState>();
        private readonly HashSet<Behaviour> suspendedLayouts = new HashSet<Behaviour>();
        private readonly Vector3[] corners = new Vector3[4];
        private int trialDragControl;
        private Vector2 dragMouse;
        private Adjustment dragStart;
        private bool editingBackdrop;

        [MenuItem(MenuPath, false, 30)]
        [MenuItem(ToolsPath)]
        private static void OpenSelection() => Open(SelectedPanel);

        [MenuItem(MenuPath, true)]
        [MenuItem(ToolsPath, true)]
        private static bool CanOpenSelection() =>
            !EditorApplication.isPlayingOrWillChangePlaymode && SelectedPanel;

        internal static RectTransform SelectedPanel =>
            Selection.activeTransform as RectTransform ?? (Selection.activeObject as GameObject)?.transform as RectTransform;

        internal static void Open(RectTransform panel)
        {
            var window = GetWindow<MenuLayoutTrialWindow>();
            window.titleContent = new GUIContent("Main Menu Layout Trial");
            window.Preview(panel);
        }

        protected override void OnScopeChanged()
        {
            selected = 0;
            group = 0;
            indexedGroup = -1;
            editingBackdrop = false;
            ResetAll();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.hierarchyChanged += RefreshHierarchy;
        }

        protected override void OnDisable()
        {
            EditorApplication.hierarchyChanged -= RefreshHierarchy;
            base.OnDisable();
        }

        protected override void DrawPreviewControls(SerializedObject settings)
        {
            bool entireArmory = Source.GetComponent<ArmoryResponsiveLayout>();
            var groups = entireArmory ? ArmoryGroups : MenuGroups;
            group = GUILayout.Toolbar(Mathf.Clamp(group, 0, groups.Length - 1), groups);
            IndexSource(settings, entireArmory);
            if (choices.Length == 0)
            {
                editingBackdrop = false;
                return;
            }

            int choice = Mathf.Max(0, Array.IndexOf(choices, selected));
            int next = choices[EditorGUILayout.Popup("Element", choice, labels)];
            if (selected != next)
            {
                selected = next;
                InvalidatePreview();
            }
            var backdrop = settings?.FindProperty("backdrop").objectReferenceValue as Image;
            editingBackdrop = backdrop && sourceNodes[selected] == backdrop.rectTransform;
            if (editingBackdrop)
            {
                EditorGUILayout.LabelField("Backdrop settings · stored on Armory Page", EditorStyles.miniLabel);
                base.DrawPreviewControls(settings);
                return;
            }

            EditorGUILayout.LabelField("Trial adjustments · preview only", EditorStyles.miniLabel);
            var edit = GetAdjustment(selected);
            EditorGUI.BeginChangeCheck();
            edit.position = EditorGUILayout.Vector2Field("Position offset", edit.position);
            edit.size = EditorGUILayout.Vector2Field("Size offset", edit.size);
            edit.scale = EditorGUILayout.Slider("Scale", edit.scale, .25f, 2f);
            if (EditorGUI.EndChangeCheck()) SetAdjustment(edit);

            if (entireArmory && group == InventoryGroup)
            {
                EditorGUI.BeginChangeCheck();
                tryGrid = EditorGUILayout.Toggle("Try grid settings", tryGrid);
                using (new EditorGUI.DisabledScope(!tryGrid))
                {
                    columns = EditorGUILayout.IntSlider("Columns", columns, 1, 6);
                    gap = EditorGUILayout.Slider("Cell gap", gap, 0, 40);
                }
                inventoryScroll = EditorGUILayout.Slider("Scroll to bottom", inventoryScroll, 0, 1);
                if (EditorGUI.EndChangeCheck()) InvalidatePreview();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset Element")) SetAdjustment(new Adjustment { element = selected, scale = 1 });
                if (GUILayout.Button("Reset All")) ResetAll();
            }
            EditorGUILayout.LabelField("Drag the preview to move the selected element.", EditorStyles.miniLabel);
        }

        private void IndexSource(SerializedObject settings, bool entireArmory)
        {
            if (indexedSource != Source)
            {
                indexedSource = Source;
                sourceNodes = Source.GetComponentsInChildren<RectTransform>(true);
                CaptureSourceParents();
                indexedGroup = -1;
            }
            if (indexedGroup == group) return;

            indexedGroup = group;
            Transform root = !entireArmory ? Source : group switch
            {
                1 => settings.FindProperty("stats").objectReferenceValue as Transform,
                2 => Source.GetComponent<ArmoryResponsiveLayout>().SlotGroup,
                InventoryGroup => settings.FindProperty("inventory").objectReferenceValue as Transform,
                _ => Source.transform,
            };
            var indices = new List<int>();
            var names = new List<string>();
            for (int i = entireArmory ? 1 : 0; i < sourceNodes.Length; i++)
            {
                var node = sourceNodes[i];
                if (!root || !node.IsChildOf(root) || (group == 0 && i != 0 && node.parent != root)) continue;
                string path = i == 0 ? node.name + " (panel)" :
                    node == root ? node.name : AnimationUtility.CalculateTransformPath(node, root);
                indices.Add(i);
                names.Add(path);
            }
            choices = indices.ToArray();
            labels = names.ToArray();
        }

        private void RefreshHierarchy()
        {
            if (!Source || indexedSource != Source || sourceNodes == null || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var next = Source.GetComponentsInChildren<RectTransform>(true);
            bool changed = next.Length != sourceNodes.Length;
            for (int i = 0; !changed && i < next.Length; i++)
                changed = next[i] != sourceNodes[i] || next[i].parent != sourceParents[i];

            indexedGroup = -1;
            if (!changed) return;

            selected = RemapIndex(next, selected);
            for (int i = adjustments.Count - 1; i >= 0; i--)
            {
                var edit = adjustments[i];
                edit.element = RemapIndex(next, edit.element);
                if (edit.element >= 0) adjustments[i] = edit;
                else adjustments.RemoveAt(i);
            }
            sourceNodes = next;
            CaptureSourceParents();
            RebuildPreview();
        }

        private int RemapIndex(RectTransform[] next, int index) =>
            index >= 0 && index < sourceNodes.Length ? Array.IndexOf(next, sourceNodes[index]) : -1;

        private void CaptureSourceParents()
        {
            sourceParents = new Transform[sourceNodes.Length];
            for (int i = 0; i < sourceNodes.Length; i++) sourceParents[i] = sourceNodes[i].parent;
        }

        private Adjustment GetAdjustment(int element)
        {
            int index = adjustments.FindIndex(edit => edit.element == element);
            return index >= 0 ? adjustments[index] : new Adjustment { element = element, scale = 1 };
        }

        private void SetAdjustment(Adjustment edit)
        {
            int index = adjustments.FindIndex(item => item.element == edit.element);
            if (edit.position == Vector2.zero && edit.size == Vector2.zero && Mathf.Approximately(edit.scale, 1))
            {
                if (index >= 0) adjustments.RemoveAt(index);
            }
            else if (index >= 0) adjustments[index] = edit;
            else adjustments.Add(edit);
            InvalidatePreview();
        }

        private void ResetAll()
        {
            adjustments.Clear();
            tryGrid = false;
            columns = 6;
            gap = 10;
            inventoryScroll = 0;
            InvalidatePreview();
        }

        protected override void HandlePreviewInput(int control, Rect rect, float scale, float spriteHeight, SerializedObject settings)
        {
            if (editingBackdrop)
            {
                base.HandlePreviewInput(control, rect, scale, spriteHeight, settings);
                return;
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.MoveArrow);
            var input = Event.current;
            switch (input.GetTypeForControl(control))
            {
                case EventType.MouseDown when input.button == 0 && rect.Contains(input.mousePosition) && selected >= 0:
                    trialDragControl = GUIUtility.hotControl = control;
                    dragMouse = input.mousePosition;
                    dragStart = GetAdjustment(selected);
                    input.Use();
                    break;
                case EventType.MouseDrag when trialDragControl == control:
                    var edit = dragStart;
                    var delta = (input.mousePosition - dragMouse) / scale;
                    var element = FindPreviewElement(edit.element);
                    if (element)
                    {
                        var world = PreviewFrame.TransformVector(new Vector3(delta.x, -delta.y, 0));
                        edit.position += (Vector2)element.parent.InverseTransformVector(world);
                        SetAdjustment(edit);
                    }
                    input.Use();
                    break;
                case EventType.MouseUp when trialDragControl == control:
                    GUIUtility.hotControl = trialDragControl = 0;
                    input.Use();
                    break;
            }
        }

        protected override void BeforeLayout()
        {
            foreach (var pair in adjustedRects)
                if (pair.Key) pair.Value.Restore(pair.Key);
            adjustedRects.Clear();
            foreach (var layoutGroup in suspendedLayouts)
                if (layoutGroup) layoutGroup.enabled = true;
            suspendedLayouts.Clear();

            if (indexedPreview == PreviewScope) return;
            indexedPreview = PreviewScope;
            previewNodes = indexedPreview ? indexedPreview.GetComponentsInChildren<RectTransform>(true) : null;
        }

        protected override Transform FocusedElement =>
            sourceNodes != null && selected >= 0 && selected < sourceNodes.Length ? sourceNodes[selected] : Source;

        protected override void AfterLayout()
        {
            var layout = PreviewScope ? PreviewScope.GetComponent<ArmoryResponsiveLayout>() : null;
            var scroll = layout ? layout.InventoryScroll : null;
            var content = scroll ? scroll.content : null;
            Canvas.ForceUpdateCanvases();
            foreach (var edit in adjustments)
            {
                var target = FindPreviewElement(edit.element);
                if (target && (!content || !target.IsChildOf(content) || target == content))
                    Adjust(target, edit);
            }

            if (content && tryGrid) LayoutGrid(content);
            foreach (var edit in adjustments)
            {
                var target = FindPreviewElement(edit.element);
                if (target && content && target != content && target.IsChildOf(content))
                    Adjust(target, edit);
            }

            if (content && (inventoryScroll > 0 || tryGrid))
            {
                Record(content);
                Canvas.ForceUpdateCanvases();
                scroll.StopMovement();
                scroll.verticalNormalizedPosition = 1 - inventoryScroll;
            }
        }

        private RectTransform FindPreviewElement(int index) =>
            previewNodes != null && index >= 0 && index < previewNodes.Length ? previewNodes[index] : null;

        private void Adjust(RectTransform rect, Adjustment edit)
        {
            Record(rect);
            Suspend(rect.parent.GetComponent<LayoutGroup>());
            Suspend(rect.GetComponent<ContentSizeFitter>());
            Suspend(rect.GetComponent<AspectRatioFitter>());
            rect.anchoredPosition += edit.position;
            var size = rect.rect.size + edit.size;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(1, size.x));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1, size.y));
            rect.localScale *= edit.scale;
        }

        private void LayoutGrid(RectTransform content)
        {
            Record(content);
            Suspend(content.GetComponent<LayoutGroup>());
            Suspend(content.GetComponent<ContentSizeFitter>());
            float spacing = Mathf.Min(gap, Mathf.Max(0, (content.rect.width - columns) / columns));
            float cellSize = Mathf.Max(1, (content.rect.width - (columns - 1) * spacing) / columns);
            int count = 0;
            foreach (Transform item in content)
            {
                if (!(item is RectTransform cell) || !cell.gameObject.activeSelf) continue;
                Record(cell);
                cell.anchorMin = cell.anchorMax = cell.pivot = Vector2.up;
                cell.anchoredPosition = new Vector2(count % columns, -(count / columns)) * (cellSize + spacing);
                cell.sizeDelta = new Vector2(cellSize, cellSize);
                count++;
            }
            float rows = Mathf.Ceil(count / (float)columns);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1, rows * (cellSize + spacing) - spacing));
        }

        private void Record(RectTransform rect)
        {
            if (!adjustedRects.ContainsKey(rect)) adjustedRects.Add(rect, new RectState(rect));
        }

        private void Suspend(Behaviour layout)
        {
            if (!layout || !layout.enabled || !suspendedLayouts.Add(layout)) return;
            layout.enabled = false;
        }

        protected override void DrawPreviewOverlay(Rect rect, Vector2 size)
        {
            var target = FindPreviewElement(selected);
            if (!target) return;
            var root = PreviewFrame;
            target.GetWorldCorners(corners);
            Vector2 min = root.InverseTransformPoint(corners[0]);
            Vector2 max = min;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 corner = root.InverseTransformPoint(corners[i]);
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }
            var bounds = Rect.MinMaxRect(
                (min.x - root.rect.xMin) / size.x * rect.width, (root.rect.yMax - max.y) / size.y * rect.height,
                (max.x - root.rect.xMin) / size.x * rect.width, (root.rect.yMax - min.y) / size.y * rect.height);
            GUI.BeginGroup(rect);
            EditorGUI.DrawRect(new Rect(bounds.xMin, bounds.yMin, bounds.width, 2), Outline);
            EditorGUI.DrawRect(new Rect(bounds.xMin, bounds.yMax - 2, bounds.width, 2), Outline);
            EditorGUI.DrawRect(new Rect(bounds.xMin, bounds.yMin, 2, bounds.height), Outline);
            EditorGUI.DrawRect(new Rect(bounds.xMax - 2, bounds.yMin, 2, bounds.height), Outline);
            GUI.EndGroup();
        }
    }
}
