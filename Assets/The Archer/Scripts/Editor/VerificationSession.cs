using System;
using System.IO;
using System.Security.Cryptography;
using MrX.Save;
using UnityEditor;

namespace MrX.EditorTools
{
    // Invoked by verification tooling, with no changes to the authored scene or player profile.
    public static class VerificationSession
    {
        private const string OriginalHashKey = "MrX.Verification.OriginalHash";
        private static string OriginalPath => Path.Combine(SerializationHelper.persistentDataPath, SaveManager.SAVE_FILE_NAME_EDITOR);
        public static string Begin()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before isolating saves.");
            if (EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed)
                throw new InvalidOperationException("Fix compilation errors before isolating saves.");
            var manager = UnityEngine.Object.FindAnyObjectByType<SaveManager>();
            if (!manager || new SerializedObject(manager).FindProperty("saveType").intValue != (int)MrX.Save.SaveType.SaveFile)
                throw new InvalidOperationException("Verification isolation requires the Main Menu SaveManager in SaveFile mode.");
            if (!string.IsNullOrEmpty(SessionState.GetString(SerializationHelper.VerificationDirectoryKey, "")))
                throw new InvalidOperationException("Verification session already active.");
            var folder = Path.GetFullPath("Temp/Verification/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(folder);
            if (File.Exists(OriginalPath)) File.Copy(OriginalPath, Path.Combine(folder, "original-save.backup"), false);
            SessionState.SetString(OriginalHashKey, Hash(OriginalPath));
            SessionState.SetString(SerializationHelper.VerificationDirectoryKey, folder);
            SerializationHelper.SetVerificationDirectory(folder);
            return "Fresh isolated profile: " + folder;
        }
        public static string End()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before ending isolation.");
            string expected = SessionState.GetString(OriginalHashKey, "");
            string actual = Hash(OriginalPath);
            SessionState.EraseString(SerializationHelper.VerificationDirectoryKey);
            SerializationHelper.SetVerificationDirectory(null);
            SessionState.EraseString(OriginalHashKey);
            if (expected != actual) throw new Exception("Original player save changed during verification.");
            return "Isolation ended. Original player save hash unchanged.";
        }
        private static string Hash(string path)
        {
            if (!File.Exists(path)) return "absent";
            using (var hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(File.ReadAllBytes(path)));
        }
    }
}
