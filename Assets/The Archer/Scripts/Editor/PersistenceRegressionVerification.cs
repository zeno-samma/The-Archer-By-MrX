using System;
using System.IO;
using System.Reflection;
using MrX.Save;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class PersistenceRegressionVerification
    {
        private const string ExpectedKey = "MrX.Verification.ExpectedGold";
        public static string Begin()
        {
            RequireIsolation();
            var manager = (SaveManager)GameController.SaveManager;
            var busy = (bool)typeof(SaveManager).GetField("isSaving", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
            if (busy) return "WAIT: previous save still in progress; call Begin again after a frame.";
            var gold = GameController.CurrenciesManager.GetCurrency("gold", false);
            gold.Deposit(11); manager.Save(false);
            gold.Deposit(13); manager.Save(false);
            if (ReadGold() != gold.Amount) throw new Exception("Sequential synchronous saves lost the second update.");
            manager.Save(true);
            gold.Deposit(17); manager.Save(false);
            SessionState.SetInt(ExpectedKey, gold.Amount);
            return "Two synchronous saves persisted. Started background save with a queued newer balance.";
        }
        public static string Finish()
        {
            RequireIsolation();
            var manager = (SaveManager)GameController.SaveManager;
            if ((bool)typeof(SaveManager).GetField("isSaving", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager))
                return "WAIT: background save still completing.";
            int expected = SessionState.GetInt(ExpectedKey, -1);
            if (ReadGold() != expected) throw new Exception("Coalesced background save dropped the latest update.");
            return "Persistence passed: sequential synchronous writes and queued writes during a background save retain the latest balance.";
        }
        public static string BeginExitCheck()
        {
            RequireIsolation();
            var manager = (SaveManager)GameController.SaveManager;
            var gold = GameController.CurrenciesManager.GetCurrency("gold", false);
            gold.Deposit(19); manager.Save(true); gold.Deposit(23); manager.Save(false);
            SessionState.SetInt(ExpectedKey, gold.Amount);
            return "Started older background write and newer queued balance; stop Play immediately, then VerifyStoppedSave.";
        }
        public static string VerifyStoppedSave()
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop Play first.");
            int expected = SessionState.GetInt(ExpectedKey, -1);
            if (expected < 0 || ReadGold() != expected) throw new Exception("Final save was overwritten by an older worker snapshot.");
            return "Exit persistence passed: final balance survived stopping Play during an in-flight background save.";
        }
        private static void RequireIsolation()
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(SessionState.GetString(SerializationHelper.VerificationDirectoryKey, "")))
                throw new Exception("An isolated verification profile is required. Never run this against the player profile.");
        }
        private static int ReadGold()
        {
            string folder = SessionState.GetString(SerializationHelper.VerificationDirectoryKey, "");
            if (string.IsNullOrEmpty(folder)) throw new Exception("Missing isolated save directory.");
            var database = JsonUtility.FromJson<SaveDatabase>(File.ReadAllText(Path.Combine(folder, SaveManager.SAVE_FILE_NAME_EDITOR)));
            database.Init(); return database.GetSave<CurrencySave>("gold".GetHashCode()).Amount;
        }
    }
}
