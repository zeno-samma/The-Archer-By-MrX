using System;
using System.Collections.Generic;
using System.Reflection;
using MrX.Abilities;
using MrX.Player;
using MrX.Projectile;
using MrX.UI;
using MrX.Upgrades;
using MrX.Weapon;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MrX.EditorTools
{
    public static class GameplayRegressionVerification
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        public static string VerifyPlay()
        {
            if (!Application.isPlaying) throw new Exception("Run regression checks in isolated Play Mode.");
            int count = 0; Action<bool, string> check = (ok, name) => { if (!ok) throw new Exception(name); count++; };
            var originalPlayer = StageController.Player; var originalRoom = StageController.Room; var originalProjectiles = StageController.ProjectilesManager;
            var random = UnityEngine.Random.state;
            var root = new GameObject("Isolated Gameplay Regressions"); root.SetActive(false);
            var temporary = new List<Object>();
            try
            {
                var player = root.AddComponent<PlayerBehavior>();
                StageController.RegisterPlayerBehavior(player);
                StageController.RegisterRoom(root.AddComponent<RoomBehavior>());
                var stats = player.Stats;
                var baseline = Snapshot(stats);
                foreach (var data in GameController.UpgradesManager.GetAllUpgrades())
                {
                    var clone = Object.Instantiate(data); temporary.Add(clone);
                    var go = Object.Instantiate(data.Prefab, root.transform);
                    var behavior = go.GetComponent<UpgradeBehavior>(); behavior.Init(clone); behavior.SetUpgradeLevel(0);
                    behavior.Clear();
                    check(Equal(baseline, Snapshot(stats)), data.Title + " Clear must restore every stat and remove modifier subscriptions");
                    clone.SetLevel(0);
                    check(Equal(baseline, Snapshot(stats)), data.Title + " removed upgrade must no longer react to level events");
                }
                var trifectaGuids = AssetDatabase.FindAssets("t:TrifectaAbilityData");
                var trifectaData = Object.Instantiate(AssetDatabase.LoadAssetAtPath<TrifectaAbilityData>(AssetDatabase.GUIDToAssetPath(trifectaGuids[0]))); temporary.Add(trifectaData);
                var trifecta = Object.Instantiate(trifectaData.Prefab, root.transform).GetComponent<TrifectaAbilityBehavior>();
                trifecta.Init(trifectaData, 0); trifecta.Clear();
                check(Equal(baseline, Snapshot(stats)), "Trifecta Clear restores HP, damage and speed, including modifier counts");

                var chest = root.AddComponent<HealthChestBehavior>();
                var select = typeof(HealthChestBehavior).GetMethod("SelectSecondAbility", Flags);
                var types = (AbilityType[])Enum.GetValues(typeof(AbilityType));
                var first = types[0]; var other = types[1];
                var field = typeof(HealthChestBehavior).GetField("secondSlotAbilities", Flags);
                field.SetValue(chest, new List<AbilityType> { first });
                check((AbilityType)select.Invoke(chest, new object[] { first }) == first, "Single identical chest option terminates");
                field.SetValue(chest, new List<AbilityType>());
                check((AbilityType)select.Invoke(chest, new object[] { first }) == first, "Empty second chest slot falls back safely");
                field.SetValue(chest, new List<AbilityType> { first, other, first });
                bool distinct = true;
                for (int i = 0; i < 100; i++) distinct &= (AbilityType)select.Invoke(chest, new object[] { first }) == other;
                check(distinct, "Chest always chooses a distinct option when one exists");

                var good = root.AddComponent<GoodChestBehavior>();
                stats.ChanceToIncreaseRarity.AddAdder(1000);
                var rarity = typeof(GoodChestBehavior).GetMethod("GetRarity", Flags);
                JsonUtility.FromJsonOverwrite("{\"abilityChances\":[{\"rarity\":0,\"chance\":1}]}", good);
                check((AbilityRarity)rarity.Invoke(good, null) == AbilityRarity.Rare, "Guaranteed rarity boost promotes Common to Rare");
                JsonUtility.FromJsonOverwrite("{\"abilityChances\":[{\"rarity\":3,\"chance\":1}]}", good);
                check((AbilityRarity)rarity.Invoke(good, null) == AbilityRarity.Legendary, "Legendary rarity stays within enum bounds");

                var projectiles = new ProbeProjectiles(root.transform); StageController.RegisterProjectilesManager(projectiles);
                var weapon = root.AddComponent<WeaponBehavior>();
                var shoot = typeof(WeaponBehavior).GetMethod("Shoot", Flags, null,
                    new[] { typeof(Transform), typeof(Vector3), typeof(Vector3), typeof(int), typeof(float), typeof(float) }, null);
                StatAdder criticalChance = 1000; stats.CritChanceStat.AddAdder(criticalChance); stats.CritDamageMultiplierStat.ChangeInitialValue(2);
                shoot.Invoke(weapon, new object[] { null, Vector3.zero, Vector3.forward, 3, 1f, 10f });
                check(projectiles.Items.Count == 3 && projectiles.Items.TrueForAll(p => Mathf.Abs(p.GetDamage() - 20) < .001f && p.IsCriticalHit), "Every critical arrow uses base damage independently, not 20/40/80");
                foreach (var projectile in projectiles.Items)
                { projectile.Target = player.transform; projectile.TargetToIgnore = player; projectile.IsPiercingEnabled = true; projectile.IgnoresObstacles = true; projectile.ResetOverrides(); }
                check(projectiles.Items.TrueForAll(p => p.Target == null && p.TargetToIgnore == null && !p.IsPiercingEnabled && !p.IgnoresObstacles && !p.IsCriticalHit && !p.IsLaunched), "Pooled projectiles clear previous targeting, collision and crit flags");
                criticalChance.Value = 0; projectiles.Cursor = 0;
                shoot.Invoke(weapon, new object[] { null, Vector3.zero, Vector3.forward, 3, 1f, 10f });
                check(projectiles.Items.TrueForAll(p => Mathf.Abs(p.GetDamage() - 10) < .001f && !p.IsCriticalHit), "Reused non-critical arrows never inherit a previous critical hit");

                var hero = root.AddComponent<HeroBehavior>();
                typeof(HeroBehavior).GetField("boundPlayer", Flags).SetValue(hero, player);
                player.onInvincibilityActivated += hero.ShowInvincibilityVisuals;
                player.onInvincibilityDeactivated += hero.HideInvincibilityVisuals;
                player.onPerfectStanceActivated += hero.ShowPerfectStanceVisuals;
                player.onPerfectStanceDeactivated += hero.HidePerfectStanceVisuals;
                player.onHurryActivated += hero.ShowHurryVisuals; player.onHurryDeactivated += hero.HideHurryVisuals;
                var destroy = typeof(HeroBehavior).GetMethod("OnDestroy", Flags); destroy.Invoke(hero, null); destroy.Invoke(hero, null);
                check(player.onInvincibilityActivated == null && player.onInvincibilityDeactivated == null && player.onPerfectStanceActivated == null && player.onPerfectStanceDeactivated == null && player.onHurryActivated == null && player.onHurryDeactivated == null,
                    "Hero destruction unsubscribes all visual events and repeated cleanup is safe");
                return "Gameplay regressions: " + count + " checks passed (all nine upgrade cleanup paths, Trifecta, chest edge cases, rarity, critical arrows, pool reset and hero subscriptions).";
            }
            finally
            {
                Object.DestroyImmediate(root);
                foreach (var obj in temporary) Object.DestroyImmediate(obj);
                StageController.RegisterPlayerBehavior(originalPlayer); StageController.RegisterRoom(originalRoom);
                StageController.RegisterProjectilesManager(originalProjectiles); UnityEngine.Random.state = random;
            }
        }
        private static Dictionary<string, double> Snapshot(PlayerStatsHandler stats)
        {
            var result = new Dictionary<string, double>();
            foreach (var p in typeof(PlayerStatsHandler).GetProperties())
            {
                if (p.PropertyType == typeof(MultiplicativeStat))
                {
                    var stat = (MultiplicativeStat)p.GetValue(stats); result[p.Name] = stat.Value;
                    result[p.Name + ":modifiers"] = ((System.Collections.ICollection)typeof(MultiplicativeStat).GetField("multipliers", Flags).GetValue(stat)).Count;
                }
                else if (p.PropertyType == typeof(AdditiveStat))
                {
                    var stat = (AdditiveStat)p.GetValue(stats); result[p.Name] = stat.Value;
                    result[p.Name + ":modifiers"] = ((System.Collections.ICollection)typeof(AdditiveStat).GetField("adders", Flags).GetValue(stat)).Count;
                }
            }
            return result;
        }
        private static bool Equal(Dictionary<string, double> a, Dictionary<string, double> b)
        { foreach (var entry in a) if (Math.Abs(entry.Value - b[entry.Key]) > .0001) return false; return true; }
        private sealed class ProbeProjectiles : IProjectilesManager
        {
            private readonly Transform root;
            public readonly List<SimpleProjectile> Items = new List<SimpleProjectile>();
            public int Cursor;
            public ProbeProjectiles(Transform root) { this.root = root; }
            public AbstractProjectile GetProjectile(GameObject prefab) => GetProjectile<SimpleProjectile>(prefab);
            public T GetProjectile<T>(GameObject prefab) where T : AbstractProjectile
            {
                if (Cursor == Items.Count)
                {
                    var go = new GameObject("Isolated Arrow"); go.SetActive(false); go.transform.SetParent(root);
                    var projectile = go.AddComponent<SimpleProjectile>();
                    typeof(SimpleProjectile).GetField("trailsToDisableOnHit", Flags).SetValue(projectile, new List<TrailRenderer>());
                    projectile.ResetOverrides(); Items.Add(projectile);
                }
                return (T)(AbstractProjectile)Items[Cursor++];
            }
            public void RegisterProjectile(GameObject prefab) { }
        }
    }
}
