using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(menuName = "October/Testing/Database", fileName = "Testing Database")]
    public class TestingDatabase : ScriptableObject
    {
        [SerializeField] protected List<TestingPreset> presets;
        public List<TestingPreset> Presets => presets;
    }
}