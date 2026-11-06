using UnityEditor;
using MrX;

public static class RunExitDebugMenu
{
    [MenuItem("Tools/Debug/Force Fail Run")]
    private static void ForceFail()
    {
        if (!EditorApplication.isPlaying ||
            !StageController.IsLoaded ||
            StageController.IsResultFinalized)
            return;

        StageController.ForceFail();
    }
}