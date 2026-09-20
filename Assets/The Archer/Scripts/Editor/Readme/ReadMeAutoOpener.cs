using UnityEditor;

namespace OctoberStudio.ReadMe
{
    [InitializeOnLoad]
    public static class ReadMeAutoOpener
    {
        static ReadMeAutoOpener()
        {
            // Delay call until after scripts reload
            EditorApplication.delayCall += TryShowReadMe;
        }

        static void TryShowReadMe()
        {
            // Use EditorPrefs so it shows only once
            if (EditorPrefs.GetBool("October_The_Archer_Readme_Shown", false))
                return;

            ReadMeWindow.OpenWindow();
            EditorPrefs.SetBool("October_The_Archer_Readme_Shown", true);
        }
    }
}