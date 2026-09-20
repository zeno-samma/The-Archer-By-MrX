using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace OctoberStudio
{
    public static class StageUnlockConditionTypeCache
    {
        private static List<(Type type, UnlockConditionAttribute meta)> cachedTypes;

        public static List<(Type, UnlockConditionAttribute)> GetTypes()
        {
            if (cachedTypes != null)
                return cachedTypes;

            cachedTypes = TypeCache.GetTypesDerivedFrom<StageUnlockCondition>()
                .Where(type => !type.IsAbstract)
                .Select(type => (type, type.GetCustomAttribute<UnlockConditionAttribute>()))
                .Where(item => item.Item2 != null)
                .OrderBy(item => item.Item2.Order)
                .ThenBy(item => item.Item2.MenuName)
                .ToList();

            return cachedTypes;
        }
    }
}