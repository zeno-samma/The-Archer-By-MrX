using System.Runtime.CompilerServices;
using UnityEngine;

namespace OctoberStudio.Extensions
{
    public static class TransformExtensions
    {
        public static Transform ResetLocal(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            transform.localScale = Vector3.one;

            return transform;
        }

        public static Transform ResetGlobal(this Transform transform)
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            transform.localScale = Vector3.one;

            return transform;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DirectionToXZ(this Transform from, Transform to)
        {
            return (to.position - from.position).NormalizeXZ();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DirectionToXZ(this Transform from, Vector3 to)
        {
            return (to - from.position).NormalizeXZ();
        }
    }
}