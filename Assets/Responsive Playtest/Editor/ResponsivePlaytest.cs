using System;
using System.IO;
using MrX.Save;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MrX.EditorTools
{
    [InitializeOnLoad]
    public static class ResponsivePlaytest
    {
        private const string SessionKey = "MrX.ResponsivePlaytest.Active";
        private const string MenuPath = "Assets/The Archer/Scenes/Main Menu.unity";

        static ResponsivePlaytest()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Tools/MrX Studio/Responsive Playtest/Play with isolated save")]
        public static void StartTrial()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SceneManager.GetActiveScene().path != MenuPath)
                throw new InvalidOperationException("Open Main Menu before starting the responsive playtest.");

            VerificationSession.Begin();
            try
            {
                var file = SaveManager.SAVE_FILE_NAME_EDITOR;
                var original = Path.Combine(Application.persistentDataPath, file);
                var copy = SerializationHelper.GetPersistentFilePath(file);
                if (File.Exists(original)) File.Copy(original, copy);
                SessionState.SetBool(SessionKey, true);
                EditorApplication.isPlaying = true;
            }
            catch
            {
                SessionState.EraseBool(SessionKey);
                VerificationSession.End();
                throw;
            }
        }

        [MenuItem("Tools/MrX Studio/Responsive Playtest/Play with isolated save", true)]
        private static bool CanStartTrial() =>
            !EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().path == MenuPath;

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(SessionKey, false)) return;
            SessionState.EraseBool(SessionKey);
            Debug.Log(VerificationSession.End());
        }
    }
}
