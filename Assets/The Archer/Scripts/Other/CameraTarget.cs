using UnityEngine;

namespace OctoberStudio
{
    public class CameraTarget : MonoBehaviour
    {
        private void LateUpdate()
        {
            var position = StageController.CameraManager.ValidatePosition(StageController.Player.transform.position);
            //position.x = 0;
            transform.position = position;
        }
    }
}