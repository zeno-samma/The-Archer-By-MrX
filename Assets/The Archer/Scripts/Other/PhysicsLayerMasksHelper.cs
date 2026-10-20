using MrX.Audio;
using MrX.Currency;
using MrX.Input;
using MrX.Save;
using MrX.UI;
using MrX.Upgrades;
using MrX.Vibration;
using System.Collections.Generic;
using UnityEngine;

namespace MrX
{
    public static class PhysicsLayerMasksHelper
    {
        private static Dictionary<int, int> masksByLayer;
        private static bool isInited = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            isInited = false;
            masksByLayer = null;
        }

        private static void Init()
        {
            masksByLayer = new Dictionary<int, int>();
            for (int i = 0; i < 32; i++)
            {
                var mask = 0;
                for (int j = 0; j < 32; j++)
                {
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        mask |= 1 << j;
                    }
                }
                masksByLayer.Add(i, mask);
            }
        }

        public static int GetMaskForLayer(int layerId)
        {
            if (!isInited) Init();

            return masksByLayer[layerId];
        }

        public static int GetMaskForLayer(string layerName)
        {
            var layerId = LayerMask.NameToLayer(layerName);
            return GetMaskForLayer(layerId);
        }
    }
}