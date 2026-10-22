using MrX.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MrX.EditorTools
{
    [CustomEditor(typeof(UITextEffect)), CanEditMultipleObjects]
    public sealed class UITextEffectEditor : UnityEditor.Editor
    {
        private void Fields(params string[] names)
        { foreach (var name in names) EditorGUILayout.PropertyField(serializedObject.FindProperty(name), true); }
        public override void OnInspectorGUI()
        {
            serializedObject.Update(); EditorGUI.BeginChangeCheck();
            EditorGUILayout.HelpBox("Kéo script lên TextMeshPro trong Canvas, chọn Effect. Không đổi vị trí/scale của khung UI.", MessageType.Info);
            Fields("effect", "playOnEnable", "useUnscaledTime", "duration", "delay", "loop");
            if (serializedObject.FindProperty("loop").boolValue) Fields("loopPause");
            Fields("updatesPerSecond");
            var style = (UITextEffectStyle)serializedObject.FindProperty("effect").enumValueIndex;
            switch (style)
            {
                case UITextEffectStyle.CountNumber:
                    EditorGUILayout.LabelField("Đếm số", EditorStyles.boldLabel);
                    Fields("readNumberFromText", "numberFrom", "numberTo", "numberDisplay");
                    if (serializedObject.FindProperty("numberDisplay").enumValueIndex == 0) Fields("numberFormat");
                    Fields("prefix", "suffix", "easing");
                    EditorGUILayout.HelpBox("Nếu không đọc được số trên text, dùng Number To. Minutes Seconds nhận giá trị bằng giây. PlayNumber(from, to) dùng cho dữ liệu runtime.", MessageType.None); break;
                case UITextEffectStyle.Wave: Fields("amplitude", "frequency", "characterPhase"); break;
                case UITextEffectStyle.Bounce: case UITextEffectStyle.Float: case UITextEffectStyle.Shake:
                    Fields("amplitude", "frequency", "perCharacter", "characterPhase"); break;
                case UITextEffectStyle.Pulse: Fields("scaleAmount", "frequency", "perCharacter", "characterPhase"); break;
                case UITextEffectStyle.ColorPulse: Fields("accentColor", "frequency", "perCharacter", "characterPhase"); break;
                case UITextEffectStyle.Rainbow: Fields("frequency", "characterPhase"); break;
                case UITextEffectStyle.SlideIn: Fields("slideOffset", "easing"); break;
                case UITextEffectStyle.FadeIn: case UITextEffectStyle.Typewriter: Fields("easing"); break;
                case UITextEffectStyle.Pop: Fields("perCharacter"); break;
            }
            Fields("onComplete");
            if (EditorGUI.EndChangeCheck()) UITextEffectPreview.Stop();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
            if (!Application.isPlaying && EditorUtility.IsPersistent(target))
                EditorGUILayout.HelpBox("Mở prefab bằng Open để xem thử; Preview không sửa trực tiếp asset trong Project.", MessageType.Info);
            using (new EditorGUI.DisabledScope(targets.Length != 1 || EditorApplication.isCompiling))
            {
                var item = (UITextEffect)target;
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Application.isPlaying ? "Play / Replay" : "Preview"))
                    { if (Application.isPlaying) item.Play(); else UITextEffectPreview.Begin(item); }
                    if (GUILayout.Button(Application.isPlaying ? "Stop" : "Stop / Restore"))
                    { if (Application.isPlaying) item.Stop(); else UITextEffectPreview.Stop(); }
                    if (Application.isPlaying && GUILayout.Button("Complete")) item.Complete();
                }
            }
        }
        private void OnDisable() { if (!Application.isPlaying) UITextEffectPreview.Stop(); }
    }

    // Preview state is transient and restored before scene/prefab saves, reload, or entering Play.
    [InitializeOnLoad]
    public static class UITextEffectPreview
    {
        private static UITextEffect active;
        private static double last;
        static UITextEffectPreview()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            Undo.undoRedoPerformed += Stop;
            Undo.postprocessModifications += changes =>
            {
                if (active != null)
                {
                    var text = active.GetComponent<TMPro.TextMeshProUGUI>();
                    foreach (var change in changes)
                        if (change.currentValue.target == text || change.currentValue.target == active) { Stop(); break; }
                }
                return changes;
            };
            EditorApplication.quitting += Stop;
            EditorApplication.playModeStateChanged += _ => Stop();
            EditorSceneManager.sceneSaving += (scene, path) => Stop();
            PrefabStage.prefabSaving += root => Stop();
        }
        public static void Begin(UITextEffect item)
        {
            Stop();
            if (EditorApplication.isPlayingOrWillChangePlaymode || item == null || !item.isActiveAndEnabled || EditorUtility.IsPersistent(item)) return;
            active = item; last = EditorApplication.timeSinceStartup;
            active.Play(); EditorApplication.update += Tick;
        }
        private static void Tick()
        {
            if (active == null) { Stop(); return; }
            if (!active.IsPlaying) { EditorApplication.update -= Tick; return; }
            double now = EditorApplication.timeSinceStartup;
            active.EditorPreviewStep((float)(now - last)); last = now;
            EditorApplication.QueuePlayerLoopUpdate(); SceneView.RepaintAll();
        }
        public static void Stop()
        {
            EditorApplication.update -= Tick;
            if (active != null) active.EditorPreviewRestore();
            active = null;
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
