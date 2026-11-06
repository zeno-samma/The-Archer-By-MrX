using MrX.Abilities;
using MrX.Drop;
using MrX.Easing;
using MrX.Enemy;
using MrX.Particles;
using MrX.Projectile;
using MrX.StatusEffects;
using MrX.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MrX.StageSave;

namespace MrX
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
        public static bool IsResultFinalized { get; private set; }
        private static bool settlingResult;

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
            IsResultFinalized = settlingResult = false;

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
            IsResultFinalized = settlingResult = false;
            (GameController.SaveManager as Save.SaveManager)?.BeginGameplayBatch();

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
                if (ContinuePlayingSave.HasUnfinishedStageData && !string.IsNullOrEmpty(ContinuePlayingSave.StageContentId))
                {
                    int savedIndex = System.Array.FindIndex(stageDatabase.Stages, s => s.StageId == ContinuePlayingSave.StageContentId);
                    if (savedIndex >= 0) StageIndex = savedIndex;
                }
                StageData = stageDatabase.GetStageData(StageIndex);
            }

            // Gameplay consumers (including DropManager) still read the selected index.
            StageSave.SetSelectedStageId(StageIndex);
            ContinuePlayingSave.RememberStage(StageData.StageId);

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
                InitStageFromTestingData();// Khởi tạo phòng chơi từ dữ liệu thử nghiệm
            }
            else if (ContinuePlayingSave.HasUnfinishedStageData)
            {
                InitStageFromUnfinishedSession();// Khởi tạo phòng chơi từ phiên chơi chưa hoàn thành
            }
            else
            {
                InitStageFromStart();// Quyết định khởi tạo phòng chơi từ đầu
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
            }
            else
            {
                EasingManager.DoNextFrame(() =>
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

        public static void OnExitReached(RoomExitBehavior roomExitBehavior)//người chơi đã đi đến cổng thoát
        {
            if (IsResultFinalized || settlingResult || CurrentRoomIndex >= StageData.RoomsCount) return;
            CurrentRoomIndex++;

            if (CurrentRoomIndex >= StageData.RoomsCount)
            {
                StageSave.CompleteStage(StageData);
                GameController.AchievementsManager?.RecordStageClear(StageData.StageId);
                GameController.DailyQuestsManager?.Record(Quests.DailyQuestType.ClearStages);

                FinalizeRunRewards();
                Debug.Log("[VICTORY] 1 - Before StageCompleteScreen.Show");
                GameScreen.StageCompleteScreen.Show();
                Debug.Log("[VICTORY] 2 - After StageCompleteScreen.Show");
                // Debug.Log("Người chơi đã hoàn thành tất cả các phòng trong giai đoạn");
                // 3. Hoặc đọc lại biến lưu trữ ở bất kỳ đâu/bất kỳ lúc nào về sau
                // float recordedTime = ConsoleTimer.Instance.GetElapsedTime("Wave_1");
                // ConsoleTimer.Instance.LogAllResults();//Phát event hoàn thành tất cả các phòng
                // ConsoleTimer.Instance.ReportSessionResult("FinishAllRoom");
            }
            else
            {
                // Debug.Log("Thoát khỏi phòng: " + CurrentRoomIndex);
                GameScreen.FadeInOut(LoadNextRoom, null);
                // 2. Dừng khi kết thúc phiên và lấy giá trị ra ngay
                // float finalTime = ConsoleTimer.Instance.StopTimer("Wave_1");
                // Debug.Log("Thời gian hoàn thành Wave 1: " + finalTime.ToString("F2") + "s");
            }
        }

        protected static void LoadNextRoom()
        {
            // ==========================
            // Debug.Log("Phòng tiếp theo: " + CurrentRoomIndex + " Loại phòng: " + Room.CurrentWave.Icon.name + " Bắt đầu đếm thời gian: ");
            // 1. Bắt đầu phiên đếm
            // ConsoleTimer.Instance.StartTimer("Wave_1");
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

        public static void OnHeroDied()// người chơi đã chết
        {
            if (IsResultFinalized || settlingResult) return;
            if (!Player.TryToRevive())
            {
                onDefeat?.Invoke();
                RunTelemetry.FailRoom();
                Debug.Log("[DEFEAT] 1 - Before StageFailedScreen.Show");
                FinalizeRunRewards();
                instance.gameScreen.StageFailedScreen.Show();
                Debug.Log("[DEFEAT] 2 - After StageFailedScreen.Show");
            }
        }

        public static void ForceFail()
        {
            if (IsResultFinalized || settlingResult) return;
            onDefeat?.Invoke();
            RunTelemetry.FailRoom();
            FinalizeRunRewards();
            instance.gameScreen.StageFailedScreen.Show();
        }

        public static void FinalizeRunRewards()
        {
            if (IsResultFinalized || settlingResult) return;
            settlingResult = true;
            try
            {
                Debug.Log("[RUN-END] Before GrantPickedUpItems");
                DropManager.GrantPickedUpItems();
                Debug.Log("[RUN-END] Before ApplyGameplayAmounts");
                GameController.CurrenciesManager.ApplyGameplayAmounts();
                ContinuePlayingSave.Disable();
                GameController.DailyQuestsManager?.FlushPending();
                GameController.AchievementsManager?.FlushPending();
                GameController.ArmoryManager.Crafting?.FlushPending();
                IsResultFinalized = true;
                Debug.Log("[RUN-END] Before PrepareForResult");
                GameScreen.PrepareForResult();
                Debug.Log("[RUN-END] Before CommitGameplayBatch");
                var saves = GameController.SaveManager as Save.SaveManager;
                if (saves != null) saves.CommitGameplayBatch(); else GameController.SaveManager.Save(false);
            }
            finally { settlingResult = false; }
            Debug.Log("[RUN-END] CommitGameplayBatch completed");
        }

        public static void RecalculateEnemyMultipliers()//tính toán lại các hệ số nhân sát thương và máu của kẻ thù
        {
            EnemyDamageMultiplier = StageData.EnemyDamageMultiplier +
                StageData.EnemyDamageMultiplierRoomStep * CurrentRoomIndex +
                StageData.EnemyDamageMultiplierWaveStep * Room.CurrentWaveIndex;

            EnemyHPMultiplier = StageData.EnemyHPMultiplier +
                StageData.EnemyHPMultiplierRoomStep * CurrentRoomIndex +
                StageData.EnemyHPMultiplierWaveStep * Room.CurrentWaveIndex;
        }

        public static void ReturnToMainMenu()//quay lại menu chính
        {
            Debug.Log("[RETURN] 1 - Start ReturnToMainMenu");

            Debug.Log("[RETURN] 2 - Before OnStageUnloaded");
            GameController.UpgradesManager.OnStageUnloaded();
            Debug.Log("[RETURN] 3 - After OnStageUnloaded");
            onDefeat = null;
            Debug.Log("[RETURN] 4 - Before LoadMainMenu");
            GameController.LoadMainMenu();
            Debug.Log("[RETURN] 5 - After LoadMainMenu");
        }
    }
}
