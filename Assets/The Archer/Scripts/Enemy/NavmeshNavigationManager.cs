using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace OctoberStudio
{
    public class NavmeshNavigationManager : NavigationManager
    {
        protected Mesh navigationMesh;
        protected float[] triangleSizes;
        protected float[] tirangleCumulativeSizes;
        protected float totalSizes = 0;

        [SerializeField] protected List<NavMeshSurface> navMeshSurfaces;

        protected override void Awake()
        {
            base.Awake();

            StageController.DoAfterRoomLoaded(Init);
        }

        protected virtual void Init()
        {
            for(int i = 0; i < navMeshSurfaces.Count; i++)
            {
                var navMeshSurface = navMeshSurfaces[i];
                if (navMeshSurface == null) continue;
                navMeshSurface.BuildNavMesh();
            }

            Debug.Log("NavMesh built successfully.");

            var triangles = UnityEngine.AI.NavMesh.CalculateTriangulation();
            navigationMesh = new Mesh();
            navigationMesh.vertices = triangles.vertices;
            navigationMesh.triangles = triangles.indices;

            triangleSizes = GetTriSizes(navigationMesh.triangles, navigationMesh.vertices);
            tirangleCumulativeSizes = new float[triangleSizes.Length];

            for (int i = 0; i < triangleSizes.Length; i++)
            {
                totalSizes += triangleSizes[i];
                tirangleCumulativeSizes[i] = totalSizes;
            }
        }

        public override void Recalculate()
        {
            for (int i = 0; i < navMeshSurfaces.Count; i++)
            {
                var navMeshSurface = navMeshSurfaces[i];
                if (navMeshSurface == null) continue;
                navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
            }
        }

        public override bool IsPositionAvailable(Vector3 position)
        {
            return UnityEngine.AI.NavMesh.SamplePosition(position, out var hit, 0.01f, UnityEngine.AI.NavMesh.AllAreas);
        }

        public override bool IsStraightPathAvailable(Vector3 startPosition, Vector3 endPosition, out Vector3 hitPosition, out Vector3 hitNormal)
        {
            if(UnityEngine.AI.NavMesh.Raycast(startPosition, endPosition, out var hit, UnityEngine.AI.NavMesh.AllAreas))
            {
                hitPosition = hit.position;
                hitNormal = hit.normal;
                return false;
            }

            hitPosition = endPosition;
            hitNormal = Vector2.zero;

            return true;
            
        }

        public override Vector3 GetRandomPosition()
        {
            var randomSample = Random.value * totalSizes;

            var triIndex = -1;

            for (int i = 0; i < triangleSizes.Length; i++)
            {
                if (randomSample <= tirangleCumulativeSizes[i])
                {
                    triIndex = i;
                    break;
                }
            }

            if (triIndex == -1) return Vector3.zero;

            var pointA = navigationMesh.vertices[navigationMesh.triangles[triIndex * 3]];
            var pointB = navigationMesh.vertices[navigationMesh.triangles[triIndex * 3 + 1]];
            var pointC = navigationMesh.vertices[navigationMesh.triangles[triIndex * 3 + 2]];

            var barycentricR = Random.value;
            var barycentricS = Random.value;

            if (barycentricR + barycentricS >= 1)
            {
                barycentricR = 1 - barycentricR;
                barycentricS = 1 - barycentricS;
            }

            return pointA + barycentricR * (pointB - pointA) + barycentricS * (pointC - pointA);
        }

        protected virtual float[] GetTriSizes(int[] tris, Vector3[] verts)
        {
            int triCount = tris.Length / 3;
            float[] sizes = new float[triCount];
            for (int i = 0; i < triCount; i++)
            {
                sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
            }
            return sizes;
        }
    }
}