using UnityEditor.Rendering;

namespace OctoberStudio.PipelineSwitcher
{
    public class OctoberMaterialUpgrader : MaterialUpgrader
    {
        public OctoberMaterialUpgrader()
        {
            RenameShader("October/October", "October/October URP");
        }
    }
}