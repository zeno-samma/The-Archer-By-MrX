using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class BalanceReportTools
    {
        [MenuItem("Tools/MrX Studio/Balance/Summarize Run Reports")]
        public static void Summarize()
        {
            Debug.Log(CreateSummary());
            if (File.Exists(ReadablePath)) Application.OpenURL(new Uri(ReadablePath).AbsoluteUri);
        }
        public static string ReadablePath => Path.GetFullPath("Temp/BalanceReport.html");

        public static string CreateSummary()
        {
            string folder = Path.Combine(Application.persistentDataPath, "BalanceReports");
            var reports = Directory.Exists(folder) ? Directory.GetFiles(folder, "run-*.json").Select(Read).Where(r => r != null).ToArray() : new ConsoleTimer.RunMeasurement[0];
            var runs = reports
                .Where(r => r != null && !r.resumed && !r.verification && (r.outcome == "Die" || r.outcome == "FinishAllRoom" || r.outcome == "BackToMenu" || r.outcome == "ForceFail")).ToArray();
            var csv = new StringBuilder("stage,hero,difficulty,room,wave,boss,attempts,clears,medianClearSeconds,p75ClearSeconds,meanDamageTaken\n");
            foreach (var group in runs.SelectMany(r => r.waves.Select(w => new { r.stage, r.hero, r.difficulty, w }))
                .GroupBy(x => new { x.stage, x.hero, x.difficulty, x.w.room, x.w.wave, x.w.boss })
                .OrderBy(g => g.Key.stage).ThenBy(g => g.Key.hero).ThenBy(g => g.Key.room).ThenBy(g => g.Key.wave))
            {
                var clears = group.Where(x => x.w.cleared).Select(x => x.w.seconds).OrderBy(x => x).ToArray();
                csv.Append(Quote(group.Key.stage)).Append(',').Append(Quote(group.Key.hero)).Append(',')
                    .Append(group.Key.difficulty).Append(',')
                    .Append(group.Key.room).Append(',').Append(group.Key.wave).Append(',').Append(group.Key.boss).Append(',')
                    .Append(group.Count()).Append(',').Append(clears.Length).Append(',')
                    .Append(Percentile(clears, .5f)).Append(',').Append(Percentile(clears, .75f)).Append(',')
                    .Append(group.Average(x => x.w.damageTaken).ToString("F2", CultureInfo.InvariantCulture)).AppendLine();
            }
            string output = Path.GetFullPath("Temp/BalanceSummary.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, csv.ToString(), new UTF8Encoding(true));
            File.WriteAllText(ReadablePath, BalanceReadableReport.Render(runs, reports.Length - runs.Length), new UTF8Encoding(true));
            return $"Báo cáo dễ đọc: {ReadablePath}; {runs.Length} phiên được tổng hợp. CSV chi tiết: {output}";
        }
        private static ConsoleTimer.RunMeasurement Read(string path)
        {
            try { var run = JsonUtility.FromJson<ConsoleTimer.RunMeasurement>(File.ReadAllText(path)); return run != null && run.waves != null ? run : null; }
            catch (Exception e) { Debug.LogWarning("Skipping unreadable balance report " + Path.GetFileName(path) + ": " + e.Message); return null; }
        }
        private static string Quote(string text) { return "\"" + (text ?? "").Replace("\"", "\"\"") + "\""; }
        private static string Percentile(float[] sorted, float percentile)
        {
            if (sorted.Length == 0) return "";
            float index = (sorted.Length - 1) * percentile;
            return Mathf.Lerp(sorted[Mathf.FloorToInt(index)], sorted[Mathf.CeilToInt(index)], index - Mathf.Floor(index)).ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}
