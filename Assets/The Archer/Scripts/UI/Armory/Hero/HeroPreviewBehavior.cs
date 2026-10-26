using MrX.Armory;
using MrX.Player;
using MrX.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public class HeroPreviewBehavior : MonoBehaviour
    {
        [SerializeField] protected RawImage rawImage;
        [SerializeField, Range(256, 1024)] private int renderHeight = 512;
        [Header("Editor preview only — does not change the player's hero")]
        [SerializeField] private HeroData editorHero;
        [SerializeField] private WeaponData editorWeapon;
        [SerializeField] private Vector3 editorKeyLightRotation = new Vector3(35, -15, 0);
        [SerializeField, Min(0)] private float editorKeyLightIntensity = 1.3f;
        [SerializeField] private Vector3 editorFillLightRotation = new Vector3(20, 150, 0);
        [SerializeField, Min(0)] private float editorFillLightIntensity = .7f;
        [SerializeField] private Color editorAmbientColor = new Color(.55f, .55f, .55f);
        [SerializeField] protected GameObject heroPreviewPrefab;
        [SerializeField] protected Vector3 previewPosition = new Vector3(100, 100, -100);
        [SerializeField] protected Vector3 previewRotation = new Vector3(0, 180, 0);

        protected GameObject heroPreview;
        protected Camera previewCamera;
        protected RenderTexture renderTexture;

        protected GameObject hero;
        protected GameObject weapon;

        protected HeroData heroData;
        protected WeaponData weaponData;

        protected virtual void Awake()
        {
            heroPreview = Instantiate(heroPreviewPrefab);
            heroPreview.transform.position = previewPosition;
            heroPreview.transform.rotation = Quaternion.Euler(previewRotation);

            previewCamera = heroPreview.GetComponentInChildren<Camera>();

            ResizeTexture();

            heroPreview.gameObject.SetActive(false);
        }

        public virtual void Show()
        {
            Clear();
            heroData = GameController.ArmoryManager.EquippedHero;

            hero = Instantiate(heroData.Prefab, heroPreview.transform);

            heroPreview.gameObject.SetActive(true);

            GameController.ArmoryManager.OnSelectedHeroChanged += OnHeroChanged;
        }

        public virtual void SetWeaponData(WeaponData weaponData)
        {
            if (this.weaponData == weaponData) return;

            this.weaponData = weaponData;

            if (weapon != null)
            {
                Destroy(weapon);
            }

            weapon = Instantiate(weaponData.WeaponPrefab);
            var weaponBehavior = weapon.GetComponent<AbstractWeaponBehavior>();
            weaponBehavior.Init(weaponData);

            var animationsSet = GameController.ArmoryManager.GetWeaponAnimationsSet(heroData.Id, weaponData.Id);
            hero.GetComponent<IHeroBehavior>().PlaceWeapon(weaponBehavior, animationsSet);
        }

        protected virtual void OnHeroChanged()
        {
            if (weapon != null) Destroy(weapon);
            if (hero != null) Destroy(hero);
            heroData = GameController.ArmoryManager.EquippedHero;
            hero = Instantiate(heroData.Prefab, heroPreview.transform);
            weaponData = null;
            SetWeaponData(GameController.ArmoryManager.GetEquippedItemData(ItemType.Weapon) as WeaponData);
        }

        public virtual void Clear()
        {
            weaponData = null;
            if (heroPreview != null) heroPreview.gameObject.SetActive(false);
            if (weapon != null) Destroy(weapon);
            if (hero != null) Destroy(hero);
            weapon = null;
            hero = null;
            if (GameController.ArmoryManager != null)
                GameController.ArmoryManager.OnSelectedHeroChanged -= OnHeroChanged;
        }

        private void LateUpdate() { if (previewCamera != null && heroPreview.activeSelf) ResizeTexture(); }

        private void ResizeTexture()
        {
            var rect = rawImage.rectTransform.rect;
            int height = Mathf.Clamp(renderHeight, 256, 1024);
            int width = Mathf.Clamp(Mathf.RoundToInt(height * rect.width / Mathf.Max(1, rect.height)), 64, 2048);
            if (renderTexture != null && renderTexture.width == width && renderTexture.height == height) return;
            ReleaseTexture();
            renderTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGBHalf);
            previewCamera.targetTexture = renderTexture;
            previewCamera.aspect = (float)width / height;
            rawImage.texture = renderTexture;
        }

        private void ReleaseTexture()
        {
            if (renderTexture == null) return;
            if (previewCamera != null) previewCamera.targetTexture = null;
            if (rawImage != null && rawImage.texture == renderTexture) rawImage.texture = null;
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        private void OnDestroy()
        {
            Clear();
            ReleaseTexture();
            if (heroPreview != null) Destroy(heroPreview);
        }
    }
}
