using OctoberStudio.Easing;
using OctoberStudio.Enemy;
using OctoberStudio.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class RoomBehavior : MonoBehaviour
    {
        protected List<RoomExitBehavior> exits = new List<RoomExitBehavior>();
        protected List<EnemyBehavior> aliveEnemies = new List<EnemyBehavior>();
        protected List<ChestBehavior> chests = new List<ChestBehavior>();

        public UnityAction onEnemyDefeated;
        public UnityAction onAllEnemiesDefeated;

        public event UnityAction onWaveStarted;
        public event UnityAction onWaveEnded;

        public event UnityAction onRoomStarted;
        public event UnityAction onRoomEnded;
        public event UnityAction<EnemyData> onBossfightStarted;

        public RoomData RoomData { get; protected set; }
        public WaveData CurrentWave => StageController.RoomsShuffler.GetActiveWaveData();
        public int CurrentWaveIndex { get => ContinuePlayingSave.ActiveWaveId; protected set => ContinuePlayingSave.ActiveWaveId = value; }

        public int AliveEnemiesCount => aliveEnemies.Count;
        public bool IsLoaded { get; protected set; } = false;
        public bool WaitForLevelUpToEnd { get; protected set; } = false;

        public event UnityAction OnExitSpawned;
        protected WaitUntilTrue waitForAbilitySelectorClosed = new WaitUntilTrue();

        protected ContinuePlayingSave ContinuePlayingSave { get; set; }

        protected virtual void Awake()
        {
            StageController.RegisterRoom(this);

            ContinuePlayingSave = GameController.SaveManager.GetSave<ContinuePlayingSave>("Continue Playing");
        }

        #region Init

        public virtual void Init(RoomData roomData, int waveIndex, bool waitForAbilitySelector, bool loadShuffle, bool loadWithoutEnemies)
        {
            RoomData = roomData;

            SpawnRoomPrefabs();
            InitExits();

            if (waveIndex == -1)
            {
                CurrentWaveIndex = 0;
            }
            else
            {
                if (loadShuffle)
                {
                    CurrentWaveIndex = waveIndex;
                }
                else
                {
                    // We shuffeled waves, need to find the correct one
                    var startingWave = RoomData.Waves[waveIndex];

                    CurrentWaveIndex = StageController.RoomsShuffler.GetRealWaveIndex(RoomData, startingWave);
                }
            }

            StartCoroutine(LoadWaveCoroutine(waitForAbilitySelector, loadWithoutEnemies));

            IsLoaded = true;

            onRoomStarted?.Invoke();
        }

        protected virtual void OnAbilitySelectorClosed()
        {
            StageController.GameScreen.AbiltiesSelector.onClosed -= OnAbilitySelectorClosed;
            waitForAbilitySelectorClosed.Complete();
        }

        protected virtual IEnumerator LoadWaveCoroutine(bool waitForAbilitySelector, bool skipEnemies = false)
        {
            if (waitForAbilitySelector)
            {
                StageController.GameScreen.AbiltiesSelector.onClosed += OnAbilitySelectorClosed;
                waitForAbilitySelectorClosed.Reset();
                yield return waitForAbilitySelectorClosed;
            }

            yield return new WaitForSeconds(RoomData.FirstWaveStartDelay);

            LoadWave(skipEnemies);
        }

        protected virtual void SpawnRoomPrefabs()
        {
            for (int i = 0; i < RoomData.Prefabs.Length; i++)
            {
                var prefabData = RoomData.Prefabs[i];

                var prop = StageController.RoomBuilder.GetPooledObject(prefabData.Prefab);
                prefabData.TransformData.ApplyTo(prop.transform);
            }
        }

        protected virtual void InitExits()
        {
            exits.Clear();
            transform.GetComponentsInChildren(exits);

            for (int i = 0; i < exits.Count; i++)
            {
                exits[i].Init(false);
            }
        }

        #endregion

        public void LoadWave(bool skipEnemies = false)
        {
            StageController.RecalculateEnemyMultipliers();

            if (!skipEnemies)
            {
                WaitForLevelUpToEnd = StageController.ExperienceManager.WillLevelUp(CurrentWave.WaveDropExperience);

                EnemyData bossData = null;
                for (int i = 0; i < CurrentWave.EnemySpawns.Count; i++)
                {
                    var spawnData = CurrentWave.EnemySpawns[i];
                    var enemyType = spawnData.EnemyType;
                    var enemyData = StageController.EnemiesSpawner.EnemiesDatabase.GetEnemy(enemyType);

                    EnemySpawnPointBehavior spawn;
                    if (enemyData.IsBoss)
                    {
                        spawn = StageController.EnemiesSpawner.GetBossSpawn();
                        bossData = enemyData;
                    }
                    else
                    {
                        spawn = StageController.EnemiesSpawner.GetSpawn();
                    }

                    spawn.Init(spawnData);

                    spawn.onEnemySpawned += OnEnemySpawned;
                }

                if (bossData != null)
                {
                    onBossfightStarted?.Invoke(bossData);
                }
            }

            if (CurrentWave.EnemySpawns.Count == 0 || skipEnemies)
            {
                if (CurrentWave.ChestSpawns.Count == 0 || skipEnemies)
                {
                    StartCoroutine(EndWave());
                }
                else
                {
                    onWaveStarted?.Invoke();
                    ContinuePlayingSave.KilledAllEnemies = true;

                    SpawnChests();
                }
            }
            else
            {
                ContinuePlayingSave.KilledAllEnemies = false;

                onWaveStarted?.Invoke();
            }

            if (CurrentWave.WaveMusic != null)
            {
                GameController.AudioManager.PlayMusic(CurrentWave.WaveMusic, true);
            }
            else if (RoomData.RoomMusic != null)
            {
                GameController.AudioManager.PlayMusic(RoomData.RoomMusic, true);
            }
            else if (StageController.StageData.StageMusic != null)
            {
                GameController.AudioManager.PlayMusic(StageController.StageData.StageMusic, true);
            }
            else
            {
                GameController.AudioManager.PlayMainMusic();
            }
        }

        protected virtual IEnumerator EndWave()
        {
            onWaveEnded?.Invoke();

            if (CurrentWave.WaveMusic != null)
            {
                if (RoomData.RoomMusic != null)
                {
                    GameController.AudioManager.PlayMusic(RoomData.RoomMusic, true);
                }
                else if (StageController.StageData.StageMusic != null)
                {
                    GameController.AudioManager.PlayMusic(StageController.StageData.StageMusic, true);
                }
                else
                {
                    GameController.AudioManager.PlayMainMusic();
                }
            }

            for(int i = 0; i < CurrentWave.Rewards.Count; i++)
            {
                var reward = CurrentWave.Rewards[i];
                if (reward.Amount == 0) continue;

                var currency = GameController.CurrenciesManager.GetCurrency(reward.CurrencyId, true);

                if(currency != null)
                {
                    currency.Deposit(reward.Amount);
                }
                else
                {
                    Debug.LogWarning($"Trying to deposit {reward.Amount} to a {reward.CurrencyId} currency that is not in the Currencies Database");
                }
            }

            if (CurrentWave.EnemySpawns.Count == 0)
            {
                StageController.ExperienceManager.AddXP(CurrentWave.WaveDropExperience);
            }

            var xpLevel = StageController.ExperienceManager.XPLevel;
            if (!StageController.DropManager.AllWaveDropPickedUp())
            {
                StageController.DropManager.PickUpAllEndWaveDrop();
                yield return new WaitUntil(() => StageController.DropManager.AllWaveDropPickedUp());
            }

            if (StageController.Room.WaitForLevelUpToEnd && StageController.ExperienceManager.XPLevel == xpLevel)
            {
                var xpDifference = StageController.ExperienceManager.TargetXP - StageController.ExperienceManager.XP;
                if (xpDifference > 0)
                {
                    StageController.ExperienceManager.AddXP(xpDifference);
                }
            }

            if (StageController.DropManager.HasAliveIndicators)
            {
                yield return new WaitUntil(() => !StageController.DropManager.HasAliveIndicators);
            }

            var startWaitingTime = Time.time;

            if (WaitForLevelUpToEnd)
            {
                StageController.GameScreen.AbiltiesSelector.onClosed += OnAbilitiesSelectorClosed;
                yield return new WaitUntil(() => !WaitForLevelUpToEnd);
            }

            if (CurrentWaveIndex >= RoomData.Waves.Length - 1)
            {
                StartCoroutine(ShowExitsOrLoadNextRoom());
            }
            else
            {
                WaveData nextWave = null;
                bool hasContent = false;

                if (CurrentWaveIndex != RoomData.Waves.Length - 1)
                {
                    StageController.GameScreen.WavePanel.Animate();
                }

                do
                {
                    CurrentWaveIndex++;

                    nextWave = StageController.RoomsShuffler.GetActiveWaveData();

                    hasContent = nextWave.EnemySpawns.Count + nextWave.ChestSpawns.Count > 0;
                } while (!hasContent && CurrentWaveIndex != RoomData.Waves.Length - 1);

                if (nextWave != null)
                {
                    var delay = CurrentWave.CustomWaveEndDelay < 0 ? RoomData.DelayBetweenWaves : CurrentWave.CustomWaveEndDelay;
                    delay -= startWaitingTime;
                    if (delay < 0) delay = 0;

                    EasingManager.DoAfter(delay, () =>
                    {
                        LoadWave();
                    });
                }
                else
                {
                    StartCoroutine(ShowExitsOrLoadNextRoom());
                }
            }
        }

        protected virtual void OnAbilitiesSelectorClosed()
        {
            StageController.GameScreen.AbiltiesSelector.onClosed -= OnAbilitiesSelectorClosed;
            WaitForLevelUpToEnd = false;
        }

        protected virtual IEnumerator ShowExitsOrLoadNextRoom()
        {
            if (!StageController.IsLastRoom) StageController.GameScreen.WavePanel.Animate();

            var time = Time.time;
            if (!StageController.DropManager.AllRoomDropPickedUp())
            {
                StageController.DropManager.PickUpAllEndRoomDrop();

                yield return new WaitUntil(() => StageController.DropManager.AllRoomDropPickedUp());
            }

            if (StageController.DropManager.HasAliveIndicators)
            {
                yield return new WaitUntil(() => !StageController.DropManager.HasAliveIndicators);
            }

            var wait = RoomData.DelayBeforeExitSpawn - (Time.time - time);

            if (wait <= 0)
            {
                EndRoom();
            }
            else
            {
                EasingManager.DoAfter(wait, EndRoom);
            }
        }

        protected virtual void EndRoom()
        {
            onRoomEnded?.Invoke();

            if (exits.Count == 0 || StageController.IsLastRoom)
            {
                StageController.OnExitReached(null);
            }
            else
            {
                for (int i = 0; i < exits.Count; i++)
                {
                    exits[i].Show();
                }

                OnExitSpawned?.Invoke();
            }
        }

        protected virtual void SpawnChestOrEndWave()
        {
            if (CurrentWave.ChestSpawns.Count > 0)
            {
                var delay = CurrentWave.CustomWaveEndDelay < 0 ? RoomData.DelayBetweenWaves : CurrentWave.CustomWaveEndDelay;
                EasingManager.DoAfter(delay, SpawnChests);
            }
            else
            {
                StartCoroutine(EndWave());
            }
        }

        protected virtual void OnChestHidden(ChestBehavior chest)
        {
            chest.onHidden -= OnChestHidden;

            chests.Remove(chest);

            if (chests.Count == 0)
            {
                StartCoroutine(EndWave());
            }
        }

        protected virtual void SpawnChests()
        {
            foreach (var chestData in CurrentWave.ChestSpawns)
            {
                var chest = StageController.EnemiesSpawner.GetChest(chestData.ChestType);
                chest.Spawn(chestData);
                chest.onHidden += OnChestHidden;

                chests.Add(chest);
            }
        }

        #region Enemy

        protected virtual void ProcessDefeatedEnemy(EnemyBehavior enemy)
        {
            enemy.UnsubscribeOnDefeat(OnEnemyDefeated);

            aliveEnemies.Remove(enemy);

            var experiencePerWave = CurrentWave.WaveDropExperience;
            var experiencePerEnemy = experiencePerWave / CurrentWave.EnemySpawns.Count;
            enemy.Drop(experiencePerEnemy);
        }

        protected virtual void OnEnemyDefeated(IDefeatable defeatable)
        {
            if (defeatable is EnemyBehavior enemy)
            {
                ProcessDefeatedEnemy(enemy);
            }

            onEnemyDefeated?.Invoke();

            if (aliveEnemies.Count == 0)
            {
                onAllEnemiesDefeated?.Invoke();

                ContinuePlayingSave.KilledAllEnemies = true;

                SpawnChestOrEndWave();
            }
        }

        public virtual EnemyBehavior SpawnAdditionalEnemy(EnemyType enemyType, Vector3 position, Quaternion rotation)
        {
            var enemy = StageController.EnemiesSpawner.GetEnemy(enemyType);

            enemy.Spawn(position, rotation, false);
            enemy.SubscribeOnDefeat(OnEnemyDefeated);

            aliveEnemies.Add(enemy);

            return enemy;
        }

        protected virtual void OnEnemySpawned(EnemySpawnPointBehavior spawn, EnemyBehavior enemy)
        {
            spawn.onEnemySpawned -= OnEnemySpawned;
            spawn.Hide();
            enemy.SubscribeOnDefeat(OnEnemyDefeated);

            aliveEnemies.Add(enemy);
        }

        public virtual EnemyBehavior GetClosestEnemy(Vector3 position, Func<EnemyBehavior, bool> validateFunc = null)
        {
            EnemyBehavior closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in aliveEnemies)
            {
                var distance = (enemy.Position - position).sqrMagnitude;

                if (distance < closestDistance && (validateFunc == null || validateFunc(enemy)))
                {
                    closestEnemy = enemy;
                    closestDistance = distance;
                }
            }

            return closestEnemy;
        }

        public virtual EnemyBehavior GetRandomEnemy()
        {
            if (aliveEnemies.Count == 0) return null;

            var enemy = aliveEnemies.Random();
            return enemy;
        }

        public virtual bool IsCloseToOtherEnemyDestination(EnemyBehavior enemy, Vector3 position, float minDistance = 0.5f)
        {
            var minDistanceSqr = minDistance * minDistance;
            foreach (var otherEnemy in aliveEnemies)
            {
                if (otherEnemy == enemy) continue;
                var distance = (otherEnemy.Destination - position).sqrMagnitude;
                if (distance < minDistanceSqr)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}