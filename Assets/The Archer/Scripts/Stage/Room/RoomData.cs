using OctoberStudio.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class RoomData
    {
        [SerializeField] protected PrefabData[] prefabs;
        public PrefabData[] Prefabs => prefabs;

        [SerializeField] protected WaveData[] waves;
        public WaveData[] Waves => waves;

        [SerializeField] protected Vector3 playerSpawnPoint;
        public Vector3 PlayerSpawnPoint => playerSpawnPoint;

        [SerializeField] protected int groupId = 0;
        public int GroupId => groupId;

        [SerializeField] protected AudioData roomMusic;
        public AudioData RoomMusic => roomMusic;

        [SerializeField] protected float firstWaveStartDelay;
        public float FirstWaveStartDelay => firstWaveStartDelay;

        [SerializeField] protected float delayBetweenWaves;
        public float DelayBetweenWaves => delayBetweenWaves;

        [SerializeField] protected float delayBeforeExitSpawn;
        public float DelayBeforeExitSpawn => delayBeforeExitSpawn;

        [SerializeField] protected CameraMovementType cameraMovementType;
        public CameraMovementType CameraMovementType => cameraMovementType;

        [SerializeField] protected Vector2[] cameraConfinementPolygon;
        public Vector2[] CameraConfinementPolygon => cameraConfinementPolygon;


#if UNITY_EDITOR
        public void SaveToSerializedProperty(SerializedProperty property)
        {
            var prefabsProperty = property.FindPropertyRelative("prefabs");
            prefabsProperty.arraySize = prefabs.Length;
            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].SaveToSerializedProperty(prefabsProperty.GetArrayElementAtIndex(i));
            }
            var wavesProperty = property.FindPropertyRelative("waves");
            wavesProperty.arraySize = waves.Length;
            for (int i = 0; i < waves.Length; i++)
            {
                waves[i].SaveToSerializedProperty(wavesProperty.GetArrayElementAtIndex(i));
            }

            var playerSpawnPointProperty = property.FindPropertyRelative("playerSpawnPoint");
            playerSpawnPointProperty.vector3Value = playerSpawnPoint;

            var groupIdProperty = property.FindPropertyRelative("groupId");
            groupIdProperty.intValue = groupId;

            var roomMusicProperty = property.FindPropertyRelative("roomMusic");
            roomMusicProperty.objectReferenceValue = roomMusic;

            var firstWaveStartDelayProperty = property.FindPropertyRelative("firstWaveStartDelay");
            firstWaveStartDelayProperty.floatValue = firstWaveStartDelay;

            var delayBetweenWavesProperty = property.FindPropertyRelative("delayBetweenWaves");
            delayBetweenWavesProperty.floatValue = delayBetweenWaves;

            var delayBeforeExitSpawnProperty = property.FindPropertyRelative("delayBeforeExitSpawn");
            delayBeforeExitSpawnProperty.floatValue = delayBeforeExitSpawn;

            var cameraMovementTypeProperty = property.FindPropertyRelative("cameraMovementType");
            cameraMovementTypeProperty.intValue = (int)cameraMovementType;

            var cameraConfinementPolygonProperty = property.FindPropertyRelative("cameraConfinementPolygon");
            cameraConfinementPolygonProperty.arraySize = cameraConfinementPolygon.Length;
            for (int i = 0; i < cameraConfinementPolygon.Length; i++)
            {
                cameraConfinementPolygonProperty.GetArrayElementAtIndex(i).vector2Value = cameraConfinementPolygon[i];
            }
        }
#endif
    }

    [System.Serializable]
    public class PrefabData
    {
        [SerializeField] protected GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] protected TransformData transformData;
        public TransformData TransformData => transformData;

        public PrefabData(GameObject prefab, Transform instanceTransform)
        {
            this.prefab = prefab;
            transformData = new TransformData(instanceTransform);
        }

#if UNITY_EDITOR
        public PrefabData(SerializedProperty property)
        {
            prefab = property.FindPropertyRelative("prefab").objectReferenceValue as GameObject;

            transformData = new TransformData(property.FindPropertyRelative("transformData"));
        }

        public static implicit operator PrefabData(SerializedProperty property)
        {
            return new PrefabData(property);
        }

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            transformData.SaveToSerializedProperty(property.FindPropertyRelative("transformData"));
        }
#endif
    }

    [System.Serializable]
    public class TransformData
    {
        [SerializeField] protected Vector3 position;
        public Vector3 Position => position;

        [SerializeField] protected Quaternion rotation;
        public Quaternion Rotation => rotation;

        [SerializeField] protected Vector3 localScale;
        public Vector3 LocalScale => localScale;

        public TransformData(Transform instanceTransform)
        {
            position = instanceTransform.position;
            rotation = instanceTransform.rotation;
            localScale = instanceTransform.localScale;
        }

        public void ApplyTo(Transform instanceTransform)
        {
            instanceTransform.position = position;
            instanceTransform.rotation = rotation;
            instanceTransform.localScale = localScale;
        }

        public void ApplyToLocal(Transform instanceTransform)
        {
            instanceTransform.localPosition = position;
            instanceTransform.localRotation = rotation;
            instanceTransform.localScale = localScale;
        }

#if UNITY_EDITOR
        public TransformData(SerializedProperty property)
        {
            position = property.FindPropertyRelative("position").vector3Value;
            rotation = property.FindPropertyRelative("rotation").quaternionValue;
            localScale = property.FindPropertyRelative("localScale").vector3Value;
        }

        public static implicit operator TransformData(SerializedProperty property)
        {
            return new TransformData(property);
        }

        public void SaveToSerializedProperty(SerializedProperty property)
        {
            property.FindPropertyRelative("position").vector3Value = position;
            property.FindPropertyRelative("rotation").quaternionValue = rotation;
            property.FindPropertyRelative("localScale").vector3Value = localScale;
        }
#endif
    }
}