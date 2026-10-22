using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    // Editor-only recorder: never included in a player and never writes during sampling.
    [InitializeOnLoad]
    public static class PerformanceCapture
    {
        private static ProfilerRecorder cpu, gc;
        private static readonly List<double> milliseconds = new List<double>(600);
        private static readonly List<long> allocations = new List<long>(600);
        private static int lastFrame, warmup;
        private static bool running;
        private static string context;
        static PerformanceCapture()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.ExitingPlayMode) Stop(); };
        }
        [MenuItem("Tools/MrX Studio/Performance/Capture 600 Gameplay Frames")]
        public static void Begin()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Start Play and load a gameplay room first.");
            if (StageController.StageData == null || StageController.Room == null || !StageController.Room.IsLoaded)
                throw new InvalidOperationException("Wait for a gameplay room to load before sampling.");
            Stop();
            context = "Stage: " + StageController.StageData.StageName + "; difficulty: " + StageController.ActiveDifficulty
                + "; room: " + (StageController.CurrentRoomIndex + 1) + "; wave at start: " + (StageController.Room.CurrentWaveIndex + 1)
                + "; live enemies at start: " + StageController.Room.AliveEnemiesCount + ".\n\n";
            milliseconds.Clear(); allocations.Clear();
            cpu = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            lastFrame = -1; warmup = 60; running = true;
            EditorApplication.update += Sample;
        }
        private static void Sample()
        {
            if (!EditorApplication.isPlaying) { Stop(); return; }
            if (Time.frameCount == lastFrame || EditorApplication.isPaused || Time.timeScale == 0) return;
            lastFrame = Time.frameCount;
            if (warmup-- > 0) return;
            if (!cpu.Valid || !gc.Valid) { Stop(); Debug.LogWarning("Performance counters unavailable on this Editor."); return; }
            milliseconds.Add(cpu.LastValue / 1000000.0);
            allocations.Add(gc.LastValue);
            if (milliseconds.Count >= 600) Stop();
        }
        [MenuItem("Tools/MrX Studio/Performance/Stop and Save Capture")]
        public static void Stop()
        {
            EditorApplication.update -= Sample;
            cpu.Dispose(); gc.Dispose();
            if (!running) return;
            running = false;
            if (milliseconds.Count == 0) return;
            var ordered = milliseconds.OrderBy(value => value).ToArray();
            string folder = "Logs/Performance";
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md");
            string report = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "# Performance capture (Editor)\n\nFrames: {0}\n\nMain Thread mean: {1:F2} ms; median: {2:F2} ms; P95: {3:F2} ms; max: {4:F2} ms.\n\nGC allocated/frame mean: {5:F0} bytes; max: {6} bytes.\n\nResolution: {7} x {8}; quality: {9}.\n\nEditor counters include Editor/MCP overhead and waiting; these are not device FPS or GPU measurements. Paused frames are skipped. Use the same stage, wave, hero, abilities, resolution and focus for comparisons.\n",
                ordered.Length, ordered.Average(), ordered[ordered.Length / 2], ordered[Math.Min(ordered.Length - 1, (int)(ordered.Length * .95))], ordered.Last(),
                allocations.Average(value => (double)value), allocations.Max(), Screen.width, Screen.height, QualitySettings.names[QualitySettings.GetQualityLevel()]);
            File.WriteAllText(path, context + report);
            Debug.Log("Performance capture saved: " + Path.GetFullPath(path));
        }
    }
}
