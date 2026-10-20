using MrX.Save;
using UnityEngine;

namespace MrX.Vibration
{
    public class VibrationSave : ISave
    {
        [SerializeField] bool isVibrationEnabled = true;
        public bool IsVibrationEnabled { get => isVibrationEnabled; set => isVibrationEnabled = value; }

        public void Flush()
        {

        }
    }
}