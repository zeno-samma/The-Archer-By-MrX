using System.Collections.Generic;
using UnityEngine;

namespace MrX
{
    [CreateAssetMenu(menuName = "MrX Studio/Testing/Database", fileName = "Testing Database")]
    public class TestingDatabase : ScriptableObject
    {
        [SerializeField] protected List<TestingPreset> presets;
        public List<TestingPreset> Presets => presets;
    }
}