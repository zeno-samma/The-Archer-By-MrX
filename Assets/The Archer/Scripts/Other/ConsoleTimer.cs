using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MrX
{
    // Session-local measurements: no per-kill writes and no changes to the player's save.
    public class ConsoleTimer : MonoBehaviour
    {
        public static ConsoleTimer Instance { get; private set; }
        [SerializeField] private bool exportDevelopmentReports = true;
        [Serializable] public class WaveMeasurement
        {
            public int room, wave, enemyCount;
            public bool boss, cleared;
            public float seconds, damageTaken;
            public int hitsTaken;
        }
        [Serializable] public class RunMeasurement
        {
            public string utc, stage, hero, outcome;
            public float damageTaken;
            public int hitsTaken;
            public float activeSeconds;
            public bool resumed;
            public bool verification;
            public int difficulty;
            public List<WaveMeasurement> waves = new List<WaveMeasurement>();
        }
        private readonly Dictionary<string, float> results = new Dictionary<string, float>();
        private readonly Dictionary<string, float> starts = new Dictionary<string, float>();
        private RunMeasurement report = new RunMeasurement();
        private WaveMeasurement activeWave;
        private float waveStart, runStart;
        private bool reported;
        public string LastReportPath { get; private set; }
        private void Awake() { if (Instance == null) { Instance = this; ResetSession(); } else Destroy(this); }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        public void StartTimer(string id) { if (string.IsNullOrEmpty(id) || starts.ContainsKey(id)) return; results[id] = 0; starts[id] = Time.time; }
        public float StopTimer(string id) { float value = GetElapsedTime(id); starts.Remove(id); results[id] = value; return value; }
        public float GetElapsedTime(string id) { return starts.TryGetValue(id, out var start) ? Mathf.Max(0, Time.time - start) : results.TryGetValue(id, out var value) ? value : 0; }
        public Dictionary<string, float> GetAllResults() { var copy = new Dictionary<string, float>(results); foreach (var pair in starts) copy[pair.Key] = GetElapsedTime(pair.Key); return copy; }
        public void LogAllResults() { float total = 0; foreach (var pair in GetAllResults()) { total += pair.Value; Debug.Log($"[Timing] {pair.Key}: {pair.Value:F2}s"); } Debug.Log($"[Timing] Room total: {total:F2}s"); }
        public void ResetSession() { results.Clear(); starts.Clear(); activeWave = null; reported = false; runStart = Time.time; LastReportPath = null; report = new RunMeasurement { utc = DateTime.UtcNow.ToString("o") }; }
        public void BeginWave(int room, int wave, int count, bool boss, bool resumed)
        {
            EndWave(false);
            report.stage = StageController.StageData != null ? StageController.StageData.StageId : "unknown";
            report.resumed |= resumed;
            report.difficulty = (int)StageController.ActiveDifficulty;
#if UNITY_EDITOR
            report.verification = !string.IsNullOrEmpty(UnityEditor.SessionState.GetString(Save.SerializationHelper.VerificationDirectoryKey, ""));
#endif
            var hero = StageController.Player != null ? StageController.Player.Hero as Component : null;
            report.hero = hero != null ? hero.name : "unknown";
            activeWave = new WaveMeasurement { room = room, wave = wave, enemyCount = count, boss = boss };
            waveStart = Time.time;
        }
        public void MarkResumed(bool resumed) { report.resumed |= resumed; }
        public void RecordDamage(float damage)
        {
            if (reported || damage <= 0) return;
            report.damageTaken += damage; report.hitsTaken++;
            if (activeWave != null) { activeWave.damageTaken += damage; activeWave.hitsTaken++; }
        }
        public void EndWave(bool cleared)
        {
            if (activeWave == null) return;
            activeWave.seconds = Mathf.Max(0, Time.time - waveStart); activeWave.cleared = cleared;
            report.waves.Add(activeWave); activeWave = null;
        }
        public void ReportSessionResult(string outcome)
        {
            if (reported) return;
            reported = true; EndWave(false);
            report.outcome = outcome; report.activeSeconds = Mathf.Max(0, Time.time - runStart);
            foreach (var id in new List<string>(starts.Keys)) StopTimer(id);
            LogAllResults();
            Debug.Log($"[Timing] {outcome}; active session {report.activeSeconds:F2}s; waves {report.waves.Count}");
            if (!exportDevelopmentReports || (!Application.isEditor && !Debug.isDebugBuild)) return;
            try
            {
                string folder = Path.Combine(Application.persistentDataPath, "BalanceReports");
                Directory.CreateDirectory(folder);
                LastReportPath = Path.Combine(folder, "run-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + ".json");
                File.WriteAllText(LastReportPath, JsonUtility.ToJson(report, true));
                Debug.Log("[Timing] Report: " + LastReportPath);
            }
            catch (Exception exception) { Debug.LogWarning("[Timing] Could not export report: " + exception.Message); }
        }
    }
}
