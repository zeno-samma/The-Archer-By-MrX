using System;
using System.Collections.Generic;
using MrX.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    [CustomEditor(typeof(StageEncounterPreview))]
    public sealed class StageEncounterPreviewEditor : Editor
    {
        private static readonly Dictionary<RawImage, StageEncounterPreviewEditor> PreviewOwners = new();

        private Scene previewScene;
        private RawImage fightersImage;
        private RenderTexture previewTexture;
        private Camera previewCamera;
        private Transform boss;
        private Transform hero;
        private GameObject bossPrefab;
        private GameObject heroPrefab;
        private bool inspectorActive;

        private void OnEnable()
        {
            inspectorActive = true;
            Undo.undoRedoPerformed += QueuePreview;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += ReleasePreview;
            QueuePreview();
        }

        private void OnDisable()
        {
            inspectorActive = false;
            Undo.undoRedoPerformed -= QueuePreview;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= ReleasePreview;
            ReleasePreview();
        }

        public override void OnInspectorGUI()
        {
            if (DrawDefaultInspector()) QueuePreview();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) ReleasePreview();
            if (state == PlayModeStateChange.EnteredEditMode) QueuePreview();
        }

        private void QueuePreview()
        {
            EditorApplication.delayCall -= RefreshPreview;
            if (this && inspectorActive && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.delayCall += RefreshPreview;
        }

        private void RefreshPreview()
        {
            if (!this || !inspectorActive || !target || EditorUtility.IsPersistent(target) || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            serializedObject.Update();
            var image = (RawImage)serializedObject.FindProperty("fightersImage").objectReferenceValue;
            var page = (MainMenuStagesPageBehavior)serializedObject.FindProperty("page").objectReferenceValue;
            var stage = page && page.StageDatabase && page.StageDatabase.StagesAmount > 0
                ? page.StageDatabase.GetStageData(0)
                : null;
            var catalogue = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            var defaultHero = catalogue && catalogue.armory ? catalogue.armory.GetDefaultHero() : null;
            if (!image || !image.isActiveAndEnabled || !stage || !stage.JourneyBossPrefab || !defaultHero || !defaultHero.Prefab)
            {
                ReleasePreview();
                return;
            }

            if (!previewCamera || fightersImage != image || bossPrefab != stage.JourneyBossPrefab || heroPrefab != defaultHero.Prefab)
            {
                ReleasePreview();
                BuildPreview(image, stage.JourneyBossPrefab, defaultHero.Prefab);
            }

            ApplyPlacement(boss, "bossPosition", "bossScale");
            ApplyPlacement(hero, "heroPosition", "heroScale");
            previewCamera.Render();
            Canvas.ForceUpdateCanvases();
            ShowPreview();
            EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private void BuildPreview(RawImage image, GameObject nextBoss, GameObject nextHero)
        {
            if (PreviewOwners.TryGetValue(image, out var previousOwner))
                previousOwner.ReleasePreview();

            fightersImage = image;
            PreviewOwners.Add(image, this);
            bossPrefab = nextBoss;
            heroPrefab = nextHero;
            previewScene = EditorSceneManager.NewPreviewScene();
            var rig = new GameObject("Journey Fighter Editor Preview");
            SceneManager.MoveGameObjectToScene(rig, previewScene);
            rig.SetActive(false);

            previewCamera = new GameObject("Camera").AddComponent<Camera>();
            previewCamera.transform.SetParent(rig.transform, false);
            previewCamera.scene = previewScene;
            previewCamera.enabled = false;
            var light = new GameObject("Light").AddComponent<Light>();
            light.transform.SetParent(rig.transform, false);
            StageEncounterPreview.ConfigurePreviewRig(previewCamera, light);
            previewTexture = StageEncounterPreview.CreatePreviewTexture();
            previewTexture.hideFlags = HideFlags.HideAndDontSave;
            previewCamera.targetTexture = previewTexture;

            boss = CreateModel(bossPrefab, rig.transform, Quaternion.Euler(0, 180, 0));
            hero = CreateModel(heroPrefab, rig.transform, Quaternion.identity);
            rig.SetActive(true);
            Canvas.willRenderCanvases += ShowPreview;
        }

        private static Transform CreateModel(GameObject prefab, Transform rig, Quaternion rotation)
        {
            var holder = new GameObject(prefab.name).transform;
            holder.SetParent(rig, false);
            holder.localRotation = rotation;
            var model = Object.Instantiate(prefab, holder);
            foreach (var script in model.GetComponentsInChildren<MonoBehaviour>(true)) DestroyImmediate(script);
            foreach (var part in model.GetComponentsInChildren<Transform>(true))
                part.gameObject.layer = StageEncounterPreview.FighterPreviewLayer;

            foreach (var animator in model.GetComponentsInChildren<Animator>(true))
            {
                animator.fireEvents = false;
                if (!animator.runtimeAnimatorController) continue;
                var idle = Array.Find(animator.runtimeAnimatorController.animationClips,
                    clip => clip.name.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0);
                if (idle) idle.SampleAnimation(animator.gameObject, idle.length * .25f);
            }

            return holder;
        }

        private void ApplyPlacement(Transform model, string position, string scale)
        {
            model.localPosition = serializedObject.FindProperty(position).vector3Value;
            model.localScale = Vector3.one * serializedObject.FindProperty(scale).floatValue;
        }

        private void ShowPreview()
        {
            if (!this || !inspectorActive || !fightersImage)
            {
                Canvas.willRenderCanvases -= ShowPreview;
                EditorApplication.delayCall += ReleasePreview;
                return;
            }

            // RawImage retains its authored texture so this preview never becomes a prefab override.
            if (previewTexture) fightersImage.canvasRenderer.SetTexture(previewTexture);
        }

        private void ReleasePreview()
        {
            EditorApplication.delayCall -= RefreshPreview;
            EditorApplication.delayCall -= ReleasePreview;
            Canvas.willRenderCanvases -= ShowPreview;
            if (!ReferenceEquals(fightersImage, null) && PreviewOwners.TryGetValue(fightersImage, out var owner) && ReferenceEquals(owner, this))
            {
                PreviewOwners.Remove(fightersImage);
                if (fightersImage) fightersImage.canvasRenderer.SetTexture(fightersImage.mainTexture);
            }
            if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
            previewScene = default;
            if (previewTexture) DestroyImmediate(previewTexture);
            fightersImage = null;
            previewCamera = null;
            previewTexture = null;
        }
    }
}
