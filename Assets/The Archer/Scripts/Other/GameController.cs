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
        private static bool changingScene;
        public static IVibrationManager VibrationManager { get; protected set; }
        public static IAudioManager AudioManager { get; protected set; }
        public static IInputManager InputManager { get; protected set; }
        public static IUpgradesManager UpgradesManager { get; protected set; }
        public static ArmoryManager ArmoryManager { get; protected set; }
        public static Quests.DailyQuestsManager DailyQuestsManager { get; private set; }
        public static Achievements.AchievementsManager AchievementsManager { get; private set; }

        public static MainMenuScreenBehavior MainMenuScreenBehavior { get; protected set; }

        [SerializeField] protected SceneSettings sceneSettings;
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
            changingScene = false;
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

            Application.targetFrameRate = 120; // Giới hạn ổn định ở 120 FPS
            QualitySettings.vSyncCount = 0;   // Tắt VSync để tránh kẹt khung hình
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

        public static bool TryStartNewRun(StageData stage) => TryStartNewRun(stage, StageDifficulty.Basic);

        public static bool TryStartNewRun(StageData stage, StageDifficulty difficulty)
        {
            if (instance == null || stage == null || MenuEconomy == null ||
                !CanChangeScene(SceneSettings.GameScene.SceneName)) return false;
            var stages = SaveManager.GetSave<StageSave>("Stage");
            if (!stages.CanPlayDifficulty(stage, difficulty)) return false;
            // Set the transition guard before wallet events can reenter the start path.
            changingScene = true;
            if (!MenuEconomy.TrySpendForNewRun(stage.EnergyCost)) { changingScene = false; return false; }
            var run = SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
            run.Disable();
            run.RememberRun(stage.StageId, difficulty);
            CurrenciesManager.ApplyGameplayAmounts();
            stages.IncrementStageAttempts(stage);
            instance.StartCoroutine(ChangeScene(SceneSettings.MainMenuScene.SceneName, SceneSettings.GameScene.SceneName));
            SaveManager.Save(false);
            return true;
        }

        // Resume and explicit Editor testing keep their existing prepaid entry path.
        public static void LoadStage()
        {
            if (instance == null || !CanChangeScene(SceneSettings.GameScene.SceneName)) return;
            changingScene = true;
            instance.StartCoroutine(ChangeScene(SceneSettings.MainMenuScene.SceneName, SceneSettings.GameScene.SceneName));

            SaveManager.Save(false);
        }

        public static void LoadMainMenu()
        {
            if (instance == null || !CanChangeScene(SceneSettings.MainMenuScene.SceneName)) return;
            changingScene = true;

            bool settled = StageController.IsLoaded && StageController.IsResultFinalized;
            if (!settled) CurrenciesManager.ApplyGameplayAmounts();

            instance.StartCoroutine(ChangeScene(SceneSettings.GameScene.SceneName, SceneSettings.MainMenuScene.SceneName));

            if (!settled) SaveManager.Save(false);
        }

        private static bool CanChangeScene(string destination)
        {
            if (changingScene || SceneManager.GetActiveScene().name == destination) return false;

            string loadingScene = SceneSettings.LoadingScene.SceneName;
            if (Application.CanStreamedLevelBeLoaded(loadingScene) && Application.CanStreamedLevelBeLoaded(destination))
                return true;

            Debug.LogError($"Cannot change scene to '{destination}'. Check the scene list for '{loadingScene}' and '{destination}'.");
            return false;
        }

        private static IEnumerator ChangeScene(string outgoing, string destination)
        {
            LoadingScreenProgress progressView = null;
            try
            {
                string loadingSceneName = SceneSettings.LoadingScene.SceneName;
                var loadingScene = SceneManager.GetSceneByName(loadingSceneName);
                if (!loadingScene.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
                    loadingScene = SceneManager.GetSceneByName(loadingSceneName);
                }

                foreach (var root in loadingScene.GetRootGameObjects())
                {
                    if (root.TryGetComponent(out progressView)) break;
                }

                if (progressView != null)
                {
                    // Keep the overlay through single-scene activation so 100% means the destination is loaded.
                    DontDestroyOnLoad(progressView.gameObject);
                    yield return null;
                }

                var outgoingScene = SceneManager.GetSceneByName(outgoing);
                if (outgoingScene.isLoaded) yield return SceneManager.UnloadSceneAsync(outgoingScene);

                var load = SceneManager.LoadSceneAsync(destination, LoadSceneMode.Single);
                while (!load.isDone)
                {
                    if (progressView != null)
                        progressView.SetProgress(Mathf.Clamp01(load.progress / 0.9f) * 0.99f);
                    yield return null;
                }

                if (progressView != null)
                {
                    progressView.SetProgress(1f);
                    yield return null;
                }

                if (destination == SceneSettings.MainMenuScene.SceneName)
                    AudioManager.PlayMainMusic();
            }
            finally
            {
                if (progressView != null) Destroy(progressView.gameObject);
                changingScene = false;
            }
        }
    }
}
