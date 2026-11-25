using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    [DefaultExecutionOrder(100)]
    public sealed class StageEncounterPreview : MonoBehaviour
    {
        [SerializeField] private MainMenuStagesPageBehavior page;
        [SerializeField] private RawImage mapImage;
        [SerializeField] private GameObject encounterLayers;
        [SerializeField] private GameObject[] legacyVisuals;
        [SerializeField] private RawImage heroImage;
        [SerializeField] private RawImage fightersImage;
        [SerializeField] private Image selectorPlate;


        private Camera previewCamera;
        private GameObject rig;
        private GameObject hero;
        private GameObject heroPrefab;
        private GameObject boss;
        private GameObject displayedBossPrefab;
        private RenderTexture texture;
        private bool? showing;
        private bool initialPlateVisibility;
        private bool initialHeroVisibility;
        private StageData selectedStage;
        private HeroPreviewBehavior legacyPreview;

        private void Start()
        {
            legacyPreview = heroImage.GetComponentInParent<HeroPreviewBehavior>(true);
            initialPlateVisibility = selectorPlate.enabled;
            initialHeroVisibility = heroImage.enabled;
            if (page.StageSave == null) return;
            page.StageSave.onSelectedStageChanged += OnStageChanged;
            OnStageChanged(page.StageSave.SelectedStageId);
        }

        private void OnStageChanged(int index) => selectedStage = page.StageDatabase.GetStageData(index);

        private void LateUpdate()
        {
            bool active = selectedStage && selectedStage.JourneyMap && selectedStage.JourneyBossPrefab &&
                GameController.MainMenuScreenBehavior?.CurrentPage == page;
            if (active != showing) SetShowing(active);
            if (!active) return;

            if (mapImage.texture != selectedStage.JourneyMap)
                mapImage.texture = selectedStage.JourneyMap;

            var nextBossPrefab = selectedStage.JourneyBossPrefab;
            if (displayedBossPrefab != nextBossPrefab)
            {
                if (boss)
                {
                    boss.SetActive(false);
                    Destroy(boss);
                }
                boss = CreateVisual(nextBossPrefab, new Vector3(0f, 0, 35f), Quaternion.Euler(0, 180, 0), 1.215f);
                boss.transform.localScale = new Vector3(3f, 3f, 3f);
                displayedBossPrefab = nextBossPrefab;
            }

            var selectedHero = GameController.ArmoryManager.EquippedHero;
            if (selectedHero != null && heroPrefab != selectedHero.Prefab)
            {
                if (hero != null) Destroy(hero);
                heroPrefab = selectedHero.Prefab;
                hero = CreateVisual(heroPrefab, new Vector3(0f, 0f, -14f), Quaternion.identity, 1f);
            }

        }

        private void SetShowing(bool active)
        {
            showing = active;
            encounterLayers.SetActive(active);
            selectorPlate.enabled = !active && initialPlateVisibility;
            heroImage.enabled = !active && initialHeroVisibility;
            if (legacyPreview) legacyPreview.SetPreviewCameraEnabled(!active);
            foreach (var visual in legacyVisuals) visual.SetActive(!active);
            if (active)
            {
                if (!rig) BuildRig();
                rig.SetActive(true);
                return;
            }

            if (rig) rig.SetActive(false);
        }

        private void BuildRig()
        {
            rig = new GameObject("Journey Fighter Preview");
            rig.transform.position = new Vector3(240, 140, -180);
            var cameraObject = new GameObject("Camera");
            cameraObject.transform.SetParent(rig.transform, false);
            cameraObject.transform.localPosition = new Vector3(-.01f, 23.58f, -49.19f);
            cameraObject.transform.localRotation = Quaternion.Euler(27, 0, 0);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.cullingMask = 1 << 30;
            previewCamera.usePhysicalProperties = true;
            previewCamera.sensorSize = new Vector2(36, 24);
            previewCamera.gateFit = Camera.GateFitMode.Vertical;
            previewCamera.fieldOfView = 42;
            previewCamera.lensShift = new Vector2(0, .16f);
            texture = new RenderTexture(540, 1203, 24, RenderTextureFormat.ARGB32);
            previewCamera.targetTexture = texture;
            fightersImage.texture = texture;
            var lightObject = new GameObject("Light");
            lightObject.transform.SetParent(rig.transform, false);
            lightObject.transform.rotation = Quaternion.Euler(55, -30, 0);
            var keyLight = lightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.cullingMask = 1 << 30;
            keyLight.intensity = .76f;
        }

        private GameObject CreateVisual(GameObject prefab, Vector3 position, Quaternion rotation, float scale)
        {
            var holder = new GameObject(prefab.name + " Preview");
            holder.SetActive(false);
            holder.transform.SetParent(rig.transform, false);
            holder.transform.SetLocalPositionAndRotation(position, rotation);
            holder.transform.localScale = Vector3.one * scale;
            var model = Instantiate(prefab, holder.transform);
            foreach (var behaviour in model.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(behaviour);
            foreach (var collider in model.GetComponentsInChildren<Collider>(true)) Destroy(collider);
            foreach (var animator in model.GetComponentsInChildren<Animator>(true)) animator.fireEvents = false;
            foreach (var part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 30;
            StartCoroutine(ActivateVisual(holder));
            return holder;
        }

        private static System.Collections.IEnumerator ActivateVisual(GameObject holder)
        {
            yield return null;
            if (holder) holder.SetActive(true);
        }

        private void OnDestroy()
        {
            if (page && page.StageSave != null) page.StageSave.onSelectedStageChanged -= OnStageChanged;
            if (rig) Destroy(rig);
            if (texture) Destroy(texture);
        }
    }
}
