using UnityEditor.Rendering;

namespace MrX.PipelineSwitcher
{
    public class MrXMaterialUpgrader : MaterialUpgrader
    {
        public MrXMaterialUpgrader()
        {
            RenameShader("MrX/Character", "MrX/Character URP");
        }
    }
}