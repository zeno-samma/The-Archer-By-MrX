using MrX.Currency;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MrX
{
    using MrX.Audio;
    using MrX.Easing;
    using MrX.Input;
    using MrX.UI;
    using MrX.Upgrades;
    using Save;
    using Vibration;

    [DefaultExecutionOrder(-100)]
    public class GameController : MonoBehaviour
    {
        private static GameController instance;
        public static bool IsInitialized => instance != null;

        public static ISaveManager SaveManager { get; protected set; }
        public static ICurrenciesManager CurrenciesManager { get; protected set; }
        public static MenuEconomyService MenuEconomy { get; private set; }
        private static bool loadingStage;
        public static IVibrationManager VibrationManager { get; protected set; }
        public static IAudioManager AudioManager { get; protected set; }
        public static IInputManager InputManager { get; protected set; }
        public static IUpgradesManager UpgradesManager { get; protected set; }
        public static ArmoryManager ArmoryManager { get; protected set; }
        public static Quests.DailyQuestsManager DailyQuestsManager { get; private set; }
        public static Achievements.AchievementsManager AchievementsManager { get; private set; }

        public static MainMenuScreenBehavior MainMenuScreenBehavior { get; protected set; }

        [SerializeField] protected SceneSettings sceneSettings;
        [Header("Frame pacing (desktop VSync can override this limit)")]
        [SerializeField, Range(30, 240)] private int desktopFrameRate = 120;
        [SerializeField, Range(30, 120)] private int mobileFrameRate = 60;
        public static SceneSettings SceneSettings => instance.sceneSettings;

        // Indicates that the main menu is just loaded, and not exited from the game scene
        public static bool FirstTimeLoaded { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void ResetStatics()
        {
            instance = null;

            FirstTimeLoaded = true;

            SaveManager = null;
            CurrenciesManager = null;
            MenuEconomy = null;
            loadingStage = false;
            VibrationManager = null;
            AudioManager = null;
            InputManager = null;
            UpgradesManager = null;
            ArmoryManager = null;
            DailyQuestsManager = null;
            AchievementsManager = null;

            MainMenuScreenBehavior = null;
        }

        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);

                FirstTimeLoaded = false;

                return;
            }

            instance = this;

            FirstTimeLoaded = true;

            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = Application.isMobilePlatform
                ? Mathf.Clamp(mobileFrameRate, 30, 120)
                : Mathf.Clamp(desktopFrameRate, 30, 240);
        }

        #region Registering Managers
        public static bool RegisterSaveManager(ISaveManager saveManager)
        {
            if (SaveManager != null) return false;
            SaveManager = saveManager;

            return true;
        }

        public static bool RegisterArmoryManager(ArmoryManager armoryManager)
        {
            if (ArmoryManager != null) return false;
            ArmoryManager = armoryManager;
            return true;
        }

        public static bool RegisterCurrenciesManager(ICurrenciesManager currenciesManager)
        {
            if (CurrenciesManager != null) return false;
            CurrenciesManager = currenciesManager;
            return true;
        }

        public static bool RegisterVibrationManager(IVibrationManager vibrationManager)
        {
            if (VibrationManager != null) return false;
            VibrationManager = vibrationManager;
            return true;
        }

        public static bool RegisterAudioManager(IAudioManager audioManager)
        {
            if (AudioManager != null) return false;
            AudioManager = audioManager;
            return true;
        }

        public static bool RegisterInputManager(IInputManager inputManager)
        {
            if (InputManager != null) return false;
            InputManager = inputManager;
            return true;
        }

        public static bool RegisterUpgradesManager(IUpgradesManager upgradesManager)
        {
            if (UpgradesManager != null) return false;
            UpgradesManager = upgradesManager;
            return true;
        }

        public static bool RegisterMainMenuScreenBehavior(MainMenuScreenBehavior mainMenuScreenBehavior)
        {
            MainMenuScreenBehavior = mainMenuScreenBehavior;
            return true;
        }

        #endregion

        private void Start()
        {
            SaveManager.GetSave<StageSave>("Stage").Init();

            var dailyQuestsDatabase = Resources.Load<Quests.DailyQuestsDatabase>(Quests.DailyQuestsDatabase.ResourcePath);
            if (dailyQuestsDatabase != null)
            {
                DailyQuestsManager = gameObject.AddComponent<Quests.DailyQuestsManager>();
                DailyQuestsManager.Initialize(dailyQuestsDatabase);
            }

            EasingManager.DoNextFrame(ChechForTesting);
            var achievementsDatabase = Resources.Load<Achievements.AchievementsDatabase>(Achievements.AchievementsDatabase.ResourcePath);
            if (achievementsDatabase != null)
            {
                AchievementsManager = gameObject.AddComponent<Achievements.AchievementsManager>();
                AchievementsManager.Initialize(achievementsDatabase);
                EasingManager.DoNextFrame(() => AchievementsManager.RestoreKnownProgress(
                    SaveManager.GetSave<StageSave>("Stage"), SaveManager.GetSave<ArmorySave>("Armory"),
                    SaveManager.GetSave<UpgradesSave>("Upgrades Save")));
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            InputManager.InputAsset.UI.Click.performed += MusicStartWebGL;
#else
            EasingManager.DoNextFrame(AudioManager.PlayMainMusic);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        protected virtual void MusicStartWebGL(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            InputManager.InputAsset.UI.Click.performed -= MusicStartWebGL;

            AudioManager.PlayMainMusic();
        }
#endif

        protected virtual void ChechForTesting()
        {
            var stageSave = SaveManager.GetSave<StageSave>("Stage");

            var testingData = stageSave.GetTestingData();

            if (testingData != null)
            {
                var save = SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
                save.Disable();

                LoadStage();
            }
        }

        public static void InitializeMenuEconomy(ICurrenciesManager wallets)
        {
            MenuEconomy = new MenuEconomyService(Resources.Load<MenuEconomySettings>(MenuEconomySettings.ResourcePath), wallets, SaveManager);
        }

        public static bool TryStartNewRun(StageData stage)
        {
            if (instance == null || loadingStage || stage == null || MenuEconomy == null) return false;
            var stages = SaveManager.GetSave<StageSave>("Stage");
            if (!stages.IsStageUnlocked(stage)) return false;
            if (!stages.CanPlayDifficulty(stage, stages.SelectedDifficulty)) return false;
            // Set the transition guard before wallet events can reenter the start path.
            loadingStage = true;
            if (!MenuEconomy.TrySpendForNewRun(stage.EnergyCost)) { loadingStage = false; return false; }
            SaveManager.GetSave<ContinuePlayingSave>("Continue Playing").Disable();
            CurrenciesManager.ApplyGameplayAmounts();
            stages.IncrementStageAttempts(stage);
            instance.StartCoroutine(StageLoadingCoroutine());
            SaveManager.Save(false);
            return true;
        }

        // Resume and explicit Editor testing keep their existing prepaid entry path.
        public static void LoadStage()
        {
            if (instance == null || loadingStage) return;
            loadingStage = true;
            instance.StartCoroutine(StageLoadingCoroutine());

            SaveManager.Save(false);
        }

        public static void LoadMainMenu()
        {
            bool settled = StageController.IsLoaded && StageController.IsResultFinalized;
            if (!settled) CurrenciesManager.ApplyGameplayAmounts();

            if (instance != null) instance.StartCoroutine(MainMenuLoadingCoroutine());

            if (!settled) SaveManager.Save(false);
        }

        private static IEnumerator StageLoadingCoroutine()
        {
            // Debug.Log("Loading Stage...");
            yield return LoadAsyncScene(SceneSettings.LoadingScene.SceneName, LoadSceneMode.Additive);//Load loading scene
            yield return UnloadAsyncScene(SceneSettings.MainMenuScene.SceneName);//Unload main menu scene
            yield return LoadAsyncScene(SceneSettings.GameScene.SceneName, LoadSceneMode.Single);//Load game scene
        }

        private static IEnumerator MainMenuLoadingCoroutine()
        {
            yield return LoadAsyncScene(SceneSettings.LoadingScene.SceneName, LoadSceneMode.Additive);
            yield return UnloadAsyncScene(SceneSettings.GameScene.SceneName);
            yield return LoadAsyncScene(SceneSettings.MainMenuScene.SceneName, LoadSceneMode.Single);

            loadingStage = false;

            AudioManager.PlayMainMusic();
        }

        private static IEnumerator UnloadAsyncScene(string sceneName)
        {
            var asyncLoad = SceneManager.UnloadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            //wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        private static IEnumerator LoadAsyncScene(string sceneName, LoadSceneMode loadSceneMode)
        {
            // Debug.Log($"Loading scene {sceneName}...");
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            asyncLoad.allowSceneActivation = false;
            //wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                //scene has loaded as much as possible,
                // the last 10% can't be multi-threaded
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
        }
    }
}
