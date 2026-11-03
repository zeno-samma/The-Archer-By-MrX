using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace MrX
{
    /// <summary>
    /// Thu thap so lieu room. Khong dieu khien gameplay.
    /// Ghi CSV vao Application.persistentDataPath/room_telemetry.csv.
    /// </summary>
    public static class RunTelemetry
    {
        private sealed class RoomSample
        {
            public string RunId;
            public int RoomNumber;
            public string RoomType;
            public float StartedAt;
            public float HPAtStart;
            public float LastHP;
            public float MaxHP;
            public float DamageTaken;
            public float HPAdded;
            public int DamageEvents;
        }

        private static readonly CultureInfo CsvCulture = CultureInfo.InvariantCulture;
        private static HealthbarBehavior playerHealthbar;
        private static RoomSample activeRoom;
        private static string runId;

        public static string CsvPath => Path.Combine(Application.persistentDataPath, "room_telemetry.csv");

        /// <summary>Goi sau khi khoi tao Healthbar cua PLAYER, khong phai Enemy.</summary>
        public static void BindPlayerHealthbar(HealthbarBehavior healthbar)
        {
            if (playerHealthbar == healthbar) return;

            UnbindPlayerHealthbar(playerHealthbar);
            playerHealthbar = healthbar;

            if (playerHealthbar == null) return;

            playerHealthbar.OnHPDamaged += OnPlayerDamaged;
            playerHealthbar.OnHPAdded += OnPlayerHPAdded;

            // Ho tro truong hop Player Healthbar duoc tao sau BeginRoom.
            if (activeRoom != null)
            {
                activeRoom.HPAtStart = playerHealthbar.HP;
                activeRoom.LastHP = playerHealthbar.HP;
                activeRoom.MaxHP = playerHealthbar.MaxHP;
            }
        }

        public static void UnbindPlayerHealthbar(HealthbarBehavior healthbar)
        {
            if (playerHealthbar != healthbar) return;

            if (playerHealthbar != null)
            {
                playerHealthbar.OnHPDamaged -= OnPlayerDamaged;
                playerHealthbar.OnHPAdded -= OnPlayerHPAdded;
            }

            playerHealthbar = null;
        }

        public static void BeginRoom(int roomNumber, string roomType)
        {
            // Tu dong tao ma Run moi tai Room 1.
            if (activeRoom != null)
            {
                CompleteRoom(Time.time - activeRoom.StartedAt, "Interrupted");
            }

            if (roomNumber == 1 || string.IsNullOrEmpty(runId))
                runId = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CsvCulture);

            activeRoom = new RoomSample
            {
                RunId = runId,
                RoomNumber = roomNumber,
                RoomType = roomType ?? "Unknown",
                StartedAt = Time.time,
                HPAtStart = playerHealthbar != null ? playerHealthbar.HP : -1f,
                LastHP = playerHealthbar != null ? playerHealthbar.HP : -1f,
                MaxHP = playerHealthbar != null ? playerHealthbar.MaxHP : -1f
            };

            if (playerHealthbar == null)
                Debug.LogWarning("[RunTelemetry] Chua BindPlayerHealthbar: HP cua Room nay se khong duoc ghi day du.");
        }

        /// <summary>Ket thuc room va tu tinh thoi gian gameplay da troi qua.</summary>
        public static void EndRoom()
        {
            if (activeRoom == null) return;
            CompleteRoom(Time.time - activeRoom.StartedAt, "Cleared");
        }

        /// <summary>Goi khi Player chet / Run ket thuc truoc khi clear Room.</summary>
        public static void FailRoom()
        {
            if (activeRoom == null) return;
            CompleteRoom(Time.time - activeRoom.StartedAt, "Failed");
        }

        private static void OnPlayerDamaged(float actualHPRemoved)
        {
            if (activeRoom == null || actualHPRemoved <= 0f) return;

            activeRoom.DamageTaken += actualHPRemoved;
            activeRoom.DamageEvents++;
            if (playerHealthbar != null) activeRoom.LastHP = playerHealthbar.HP;
        }

        private static void OnPlayerHPAdded(float actualHPAdded)
        {
            if (activeRoom == null || actualHPAdded <= 0f) return;

            // Bao gom ca HP duoc cong khi ChangeMaxHP goi AddHP.
            activeRoom.HPAdded += actualHPAdded;
            if (playerHealthbar != null) activeRoom.LastHP = playerHealthbar.HP;
        }

        private static void CompleteRoom(float elapsedSeconds, string result)
        {
            if (activeRoom == null) return;

            var sample = activeRoom;
            activeRoom = null; // Chan ghi trung neu callback duoc goi nhieu lan.

            if (playerHealthbar != null)
            {
                sample.LastHP = playerHealthbar.HP;
                sample.MaxHP = playerHealthbar.MaxHP;
            }

            string number(float value) => value.ToString("0.##", CsvCulture);
            string line = string.Join(";", new[]
            {
                sample.RunId,
                sample.RoomNumber.ToString(CsvCulture),
                sample.RoomType.Replace(';', ','),
                number(Mathf.Max(0f, elapsedSeconds)),
                number(sample.HPAtStart),
                number(sample.LastHP),
                number(sample.MaxHP),
                number(sample.DamageTaken),
                number(sample.HPAdded),
                sample.DamageEvents.ToString(CsvCulture),
                result
            });

            try
            {
                if (!File.Exists(CsvPath))
                    File.AppendAllText(CsvPath, "RunId;Room;Type;TimeSec;HPStart;HPEnd;MaxHP;DamageTaken;HPAdded;DamageEvents;Result\n");

                File.AppendAllText(CsvPath, line + "\n");

                // Chi in link sau khi ghi file thanh cong.
                string summary = $"[RunTelemetry] Room {sample.RoomNumber} ({sample.RoomType}) | {number(elapsedSeconds)}s | " +
                                 $"HP {number(sample.HPAtStart)} -> {number(sample.LastHP)}/{number(sample.MaxHP)} | " +
                                 $"Damage {number(sample.DamageTaken)} | Heal {number(sample.HPAdded)} | " +
                                 $"Hits {sample.DamageEvents} | {result}";
#if UNITY_EDITOR
                // Debug.Log($"{summary}\n{RunTelemetryEditorLinks.OpenCsvHyperlink}\n{CsvPath}");
#else
                // Debug.Log($"{summary}\nCSV: {CsvPath}");
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[RunTelemetry] Khong ghi duoc CSV: {e.Message}\n{CsvPath}");
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Link trong Unity Console: click de mo File Explorer va chon file CSV.
    /// Khong chay trong Android/iOS build.
    /// </summary>
    [UnityEditor.InitializeOnLoad]
    internal static class RunTelemetryEditorLinks
    {
        private const string LinkId = "runtelemetry-open-csv";

#if UNITY_6000_0_OR_NEWER
        internal static string OpenCsvHyperlink =>
            "<color=#4FA9FF><link=\"href='" + LinkId + "'\">[Mo file CSV]</link></color>";
#else
        internal static string OpenCsvHyperlink =>
            "<a href=\"" + LinkId + "\">[Mo file CSV]</a>";
#endif

        static RunTelemetryEditorLinks()
        {
            UnityEditor.EditorGUI.hyperLinkClicked += OnHyperlinkClicked;
        }

        private static void OnHyperlinkClicked(UnityEditor.EditorWindow window,
            UnityEditor.HyperLinkClickedEventArgs args)
        {
            if (args.hyperLinkData == null) return;
            if (!args.hyperLinkData.TryGetValue("href", out var href)) return;
            if (href != LinkId) return;

            RevealCsv();
        }

        [UnityEditor.MenuItem("Tools/Run Telemetry/Open CSV")]
        private static void RevealCsv()
        {
            var path = RunTelemetry.CsvPath;
            if (!File.Exists(path))
            {
                Debug.LogWarning("[RunTelemetry] Chua tim thay CSV: " + path);
                return;
            }

            UnityEditor.EditorUtility.RevealInFinder(path);
        }
    }
#endif
}
