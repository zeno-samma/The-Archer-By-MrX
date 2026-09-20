using OctoberStudio.Armory;
using OctoberStudio.Player;
using OctoberStudio.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class HeroPreviewBehavior : MonoBehaviour
    {
        [SerializeField] protected RawImage rawImage;
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

            renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGBHalf);
            previewCamera.targetTexture = renderTexture;

            rawImage.texture = renderTexture;

            heroPreview.gameObject.SetActive(false);
        }

        public virtual void Show()
        {
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
            if (hero != null)
            {
                if (weapon != null)
                {
                    weapon.transform.SetParent(null);
                }

                Destroy(hero);
            }

            var heroData = GameController.ArmoryManager.EquippedHero;

            hero = Instantiate(heroData.Prefab, heroPreview.transform);

            if (weapon != null)
            {
                var animationsSet = GameController.ArmoryManager.GetWeaponAnimationsSet(heroData.Id, weaponData.Id);
                hero.GetComponent<IHeroBehavior>().PlaceWeapon(weapon.GetComponent<AbstractWeaponBehavior>(), animationsSet);
            }
        }

        public virtual void Clear()
        {
            weaponData = null;

            heroPreview.gameObject.SetActive(false);

            Destroy(hero);

            GameController.ArmoryManager.OnSelectedHeroChanged -= OnHeroChanged;
        }
    }
}