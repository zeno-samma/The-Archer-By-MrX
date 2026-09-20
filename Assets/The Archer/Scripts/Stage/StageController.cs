using OctoberStudio.Abilities;
using OctoberStudio.Drop;
using OctoberStudio.Easing;
using OctoberStudio.Enemy;
using OctoberStudio.Particles;
using OctoberStudio.Projectile;
using OctoberStudio.StatusEffects;
using OctoberStudio.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static OctoberStudio.StageSave;

namespace OctoberStudio
{
    [DefaultExecutionOrder(-10)]
    public class StageController : MonoBehaviour
    {
        private static StageController instance;

        [SerializeField] protected StageDatabase stageDatabase;
#if UNITY_EDITOR
        public StageDatabase StageDatabase => stageDatabase;
#endif

        [Space]
        [SerializeField] protected GameScreenBehavior gameScreen;
        [SerializeField] protected GameObject playerPrefab;

        public static bool IsLoaded => instance != null;

        public static GameScreenBehavior GameScreen => instance.gameScreen;

        public static StageData StageData { get; private set; }
        public static IProjectilesManager ProjectilesManager { get; protected set; }
        public static PlayerBehavior Player { get; protected set; }
        public static EnemiesSpawner EnemiesSpawner { get; protected set; }
        public static RoomBehavior Room { get; protected set; }
        public static NavigationManager NavigationManager { get; protected set; }
        public static AbilitiesManager AbilitiesManager { get; protected set; }
        public static ParticlesManager ParticlesManager { get; protected set; }
        public static DropManager DropManager { get; protected set; }
        public static RoomBuilder RoomBuilder { get; protected set; }
        public static ExperienceManager ExperienceManager { get; protected set; }
        public static CameraManager CameraManager { get; protected set; }
        public static EnemyStatusEffectsManager EnemyStatusEffectsManager { get; protected set; }

        public static int CurrentRoomIndex
        {
            get => ContinuePlayingSave.ActiveRoomId;
            set => ContinuePlayingSave.ActiveRoomId = value;
        }

        protected TestingData TestingData { get; set; }

        protected static StageSave StageSave { get; set; }
        protected static ContinuePlayingSave ContinuePlayingSave { get; set; }

        public static float EnemyDamageMultiplier { get; protected set; }
        public static float EnemyHPMultiplier { get; protected set; }

        public static int StageIndex { get; protected set; }
        public static bool IsLastRoom => CurrentRoomIndex == StageData.RoomsCount - 1;

        public static RoomsShuffler RoomsShuffler { get; protected set; }

        public static UnityAction onDefeat;

        protected static List<UnityAction> doAfterRoomLoaded = new List<UnityAction>();
        public static void DoAfterRoomLoaded(UnityAction action)
        {
            if (Room == null || !Room.IsLoaded)
            {
                doAfterRoomLoaded.Add(action);
            }
            else
            {
                action?.Invoke();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;

            StageData = null;
            ProjectilesManager = null;
            Player = null;
            EnemiesSpawner = null;
            Room = null;
            NavigationManager = null;
            AbilitiesManager = null;
            ParticlesManager = null;
            DropManager = null;
            RoomBuilder = null;
            ExperienceManager = null;
            CameraManager = null;
            EnemyStatusEffectsManager = null;

            StageSave = null;
            ContinuePlayingSave = null;

            EnemyDamageMultiplier = 1f;
            EnemyHPMultiplier = 1f;

            StageIndex = 0;

            RoomsShuffler = null;

            onDefeat = null;

            doAfterRoomLoaded = new List<UnityAction>();
        }

        protected virtual void Awake()
        {
            instance = this;

            StageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");

            TestingData = StageSave.GetTestingData();

            if (TestingData != null)
            {
                StageIndex = TestingData.StageId;
                StageData = stageDatabase.GetStageData(StageIndex);

                if (!StageSave.IsStageUnlocked(StageData))
                {
                    StageSave.UnlockStage(StageData);
                }

                StageSave.SetSelectedStageId(StageIndex);
                StageSave.IncrementStageAttempts(StageData);
            }
            else
            {
                StageIndex = StageSave.SelectedStageId;
                StageData = stageDatabase.GetStageData(StageIndex);
            }

            RoomsShuffler = new RoomsShuffler(StageData);

            Instantiate(playerPrefab);
        }

        protected virtual void InitStageFromTestingData()
        {
            var testingRoomData = StageData.GetRoom(TestingData.RoomId);
            CurrentRoomIndex = RoomsShuffler.GetRoomIndex(testingRoomData);

            var roomData = RoomsShuffler.GetActiveRoomData();
            var waveIndex = TestingData.WaveId;

            Room.Init(roomData, waveIndex, StageData.ShowAbilitySelector, false, false);
        }

        protected virtual void InitStageFromUnfinishedSession()
        {
            CurrentRoomIndex = ContinuePlayingSave.ActiveRoomId;

            var roomData = RoomsShuffler.GetActiveRoomData();
            var waveIndex = ContinuePlayingSave.ActiveWaveId;

            Room.Init(roomData, waveIndex, false, true, ContinuePlayingSave.HasUnfinishedStageData && ContinuePlayingSave.KilledAllEnemies);
        }

        protected virtual void InitStageFromStart()
        {
            CurrentRoomIndex = 0;

            var roomData = RoomsShuffler.GetActiveRoomData();
            var waveIndex = -1;

            Room.Init(roomData, waveIndex, StageData.ShowAbilitySelector, false, false);
        }

        protected virtual void Start()
        {
            GameController.UpgradesManager.OnStageLoaded();

            RoomBuilder = new RoomBuilder(Room);
            RoomBuilder.Init(StageData);

            if (TestingData != null && TestingData.IsActive)
            {
                InitStageFromTestingData();
            }
            else if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                InitStageFromUnfinishedSession();
            }
            else
            {
                InitStageFromStart();
            }

            if (Room.CurrentWave.WaveMusic != null)
            {
                GameController.AudioManager.PlayMusic(Room.CurrentWave.WaveMusic, true);
            }
            else if (Room.RoomData.RoomMusic != null)
            {
                GameController.AudioManager.PlayMusic(Room.RoomData.RoomMusic, true);
            }
            else if (StageData.StageMusic != null)
            {
                GameController.AudioManager.PlayMusic(StageData.StageMusic, true);
            }
            else
            {
                GameController.AudioManager.PlayMainMusic();
            }

            if (doAfterRoomLoaded != null)
            {
                foreach (var action in doAfterRoomLoaded)
                {
                    action?.Invoke();
                }

                doAfterRoomLoaded.Clear();
            }

            CameraManager.Init(Room.RoomData);
            EasingManager.DoNextFrame(() =>
            {
                Player.StartPlaying(Room.RoomData.PlayerSpawnPoint);
                CameraManager.TeleportCamera(Player.transform);

                ContinuePlayingSave.Enable();
            });

            StageSave.ClearTestingData();

            var shouldShowAbilitySelector = StageData.ShowAbilitySelector && !ContinuePlayingSave.HasUnfinishedStageData;

            if (StageData.ShowStageObjective)
            {
                gameScreen.StageObjectiveUI.Show(StageData, StageIndex, () =>
                {
                    if (shouldShowAbilitySelector)
                    {
                        gameScreen.AbiltiesSelector.Open();
                    }
                    else
                    {
                        gameScreen.ShowSideUI();
                    }
                });
            } else
            {
                EasingManager.DoNextFrame(() => {
                    if (shouldShowAbilitySelector)
                    {
                        gameScreen.AbiltiesSelector.Open();
                    }
                    else
                    {
                        gameScreen.ShowSideUI();
                    }
                });
            }

            gameScreen.FadeOut();
        }

        #region Registers

        public static void RegisterEnemyStatusEffectsManager(EnemyStatusEffectsManager manager)
        {
            EnemyStatusEffectsManager = manager;
        }

        public static void RegisterDropManager(DropManager dropManager)
        {
            DropManager = dropManager;
        }

        public static void RegisterCameraManager(CameraManager cameraManager)
        {
            CameraManager = cameraManager;
        }

        public static void RegisterProjectilesManager(IProjectilesManager manager)
        {
            ProjectilesManager = manager;
        }

        public static void RegisterParticlesManager(ParticlesManager manager)
        {
            ParticlesManager = manager;
        }

        public static void RegisterExperienceManager(ExperienceManager experienceManager)
        {
            ExperienceManager = experienceManager;
            ExperienceManager.Init();
        }

        public static void RegisterPlayerBehavior(PlayerBehavior player)
        {
            Player = player;
        }

        public static void RegisterEnemiesSpawner(EnemiesSpawner spawner)
        {
            EnemiesSpawner = spawner;
            EnemiesSpawner.Init(StageData);
        }

        public static void RegisterRoom(RoomBehavior room)
        {
            Room = room;
        }

        public static void RegisterNavigationManager(NavigationManager navigationManager)
        {
            NavigationManager = navigationManager;
        }

        public static void RegisterAbilitiesManager(AbilitiesManager abilitiesManager)
        {
            AbilitiesManager = abilitiesManager;
            AbilitiesManager.Init();
        }

        #endregion

        public static void OnExitReached(RoomExitBehavior roomExitBehavior)
        {
            CurrentRoomIndex++;

            if (CurrentRoomIndex >= StageData.RoomsCount)
            {
                StageSave.CompleteStage(StageData);

                DropManager.GrantPickedUpItems();
                GameScreen.StageCompleteScreen.Show();
            }
            else
            {
                GameScreen.FadeInOut(LoadNextRoom, null);
            }
        }

        protected static void LoadNextRoom()
        {
            RoomBuilder.ClearRoom();

            DropManager.HideAllDrop();

            // We've already incremented CurrentRoomIndex in OnExitReached method
            var nextRoomData = RoomsShuffler.GetActiveRoomData();
            Room.Init(nextRoomData, -1, false, false, false);

            CameraManager.Init(Room.RoomData);
            Player.StartPlaying(Room.RoomData.PlayerSpawnPoint);
            CameraManager.TeleportCamera(Player.transform);
            NavigationManager.Recalculate();

            if (Room.CurrentWave.WaveMusic != null)
            {
                GameController.AudioManager.PlayMusic(Room.CurrentWave.WaveMusic, true);
            }
            else if (Room.RoomData.RoomMusic != null)
            {
                GameController.AudioManager.PlayMusic(Room.RoomData.RoomMusic, true);
            }
            else if (StageData.StageMusic != null)
            {
                GameController.AudioManager.PlayMusic(StageData.StageMusic, true);
            }
            else
            {
                GameController.AudioManager.PlayMainMusic();
            }
        }

        public static void OnHeroDied()
        {
            if (!Player.TryToRevive())
            {
                onDefeat?.Invoke();

                DropManager.GrantPickedUpItems();
                instance.gameScreen.StageFailedScreen.Show();
            }
        }

        public static void ForceFail()
        {
            onDefeat?.Invoke();

            DropManager.GrantPickedUpItems();
            instance.gameScreen.StageFailedScreen.Show();
        }

        public static void RecalculateEnemyMultipliers()
        {
            EnemyDamageMultiplier = StageData.EnemyDamageMultiplier +
                StageData.EnemyDamageMultiplierRoomStep * CurrentRoomIndex +
                StageData.EnemyDamageMultiplierWaveStep * Room.CurrentWaveIndex;

            EnemyHPMultiplier = StageData.EnemyHPMultiplier +
                StageData.EnemyHPMultiplierRoomStep * CurrentRoomIndex +
                StageData.EnemyHPMultiplierWaveStep * Room.CurrentWaveIndex;
        }

        public static void ReturnToMainMenu()
        {
            GameController.UpgradesManager.OnStageUnloaded();

            onDefeat = null;

            GameController.LoadMainMenu();
        }
    }
}