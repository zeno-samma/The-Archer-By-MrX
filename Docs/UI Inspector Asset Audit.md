# Kiểm kê prefab và khả năng chỉnh Inspector

Tạo lúc 2026-10-26 19:53. Chỉ đọc asset, không áp lại giao diện hoặc đổi save.

- Prefab đã đọc: **620**; prefab có RectTransform: **86**.
- Component script bị thiếu: **0**.
- Source C# đã quét: **674**; dấu hiệu dựng UI runtime cần xem lại: **0**.

Quét source là kiểm tra tĩnh, không chứng minh mọi callback hay mọi thiết bị hoạt động. Các hàm authoring trong #if UNITY_EDITOR không được tính là dựng UI runtime. Danh sách động, số tiến trình, nội dung bản dịch và hiệu ứng vẫn cần script binding.

## Dấu hiệu dựng UI runtime

Không thấy trong các mẫu API UI được kiểm tra.

## Danh sách prefab

| Prefab | Có UI | Component script bị thiếu |
| --- | --- | ---: |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Combo Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Invulnerability Star Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Painful Power Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Perfect Stance Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Rage Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Rage Star Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Cure Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Heal Enhancement Ability Behavior.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Kill To Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Power Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Protector Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Safety Bubble Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Second Chance Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Strong Aura Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Freeze Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Freezing Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Ignite Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Ignite Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Meteor Rain Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Meteor Strike Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Poison Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Poison Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Shock Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Shock Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Ultimate Meteor Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Common Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Common Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Legendary Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Legendary Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Mystic Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Mystic Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Rare Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Rare Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Fire Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Ice Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Laser Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Orbs Enhansment Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Poison Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Shock Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Ultimate Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Vampiric Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Fire Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Freeze Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Poison Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Shock Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Spirit Enhansment Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Spirit Rage Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Ultimate Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Boss Slayer Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Critical Strike Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Four Leaf Clover Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Gnome Form Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Lucky Charm Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Major Health Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Major Power Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Major Swiftness Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Medium Power Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Attack Speed Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Health Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Power Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Speed Increase Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Swiftness Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Starfall Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Titan Form Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Trifecta Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Bouncy Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Bullseye Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Close Combat Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Diagonal Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Flying Weapon Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Front Projectile Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Multi Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Piercing Strike Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Quick Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Rear Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Ricochet Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Sniper Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Split Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Traversing Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Audio/Audio Source.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Chests/Evil Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Chests/Good Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Chests/Health Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Boss Loot Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Heal.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Invulnerability Star.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Item.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Rage Star.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/XP Gem.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Boss Spawn Point.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Cactus Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Golem Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Ice Slime Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Mask Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Queen Wasp Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Mage Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Minion Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Rogue Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Warrior Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Totem Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Enemy Healthbar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Bat Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Bomb Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Candle Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Cauldron Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/B Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/C Attack 1 Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/C Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/Ca Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/E Attack Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/G Attack 1 Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/G Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/H Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/I Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/M Attack Finish.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/M Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/P HIt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/Pu Attack Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/Q Charge 1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/S Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/S Spawn.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/T Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Spawn Point.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Eye Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Hand Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Jelly Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Mole Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Plant Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Pumpkin Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Slime Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Smaller Slime Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Smallest Slime Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Spike Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Trap Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Wasp Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Flying Weapon Holder.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Barbarian.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Knight.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Mage.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Rogue.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Rogue_Hooded.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Archer Hero.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Ranger.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Thief Hero.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Player Behavior.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Player/Player Healthbar.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Bat Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cactus Flex Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cactus Machine Gun Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cactus Spike.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Candle Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cauldron Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cauldron Puddle Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Eye Laser Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Golem Line Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Golem Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Honey Mine.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Crystal.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Puddle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Shard.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Sphere.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Jellyfish Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Mask Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Mask Wave.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Plant Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Pumpkin Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Queen Wasp Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Shock Spread Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Arcane Bolt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Crossbow Bolt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Totem Fist.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Totem Laser.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Totem Tornado.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Wasp Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Meteors/Meteor Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Meteors/Ultimate Meteor Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Fire Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Ice Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Laser Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Orb Laser.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Poison Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Shock Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Ultimate Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Vampiric Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Arrow Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Split Arrow Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Split Wand Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Wand Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Fire Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Freeze Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Poison Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Shock Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Ultimate Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Big High Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Big Low Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Crack.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Env Frozen.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Env.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Exit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Small Env.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Icicles 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Icicles.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small High Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle Center.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle End L.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle End R.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/arch.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/arch_gate.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bench.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bench_decorated.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bone_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bone_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bone_C.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle_melted.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle_thin.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle_triple.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/coffin.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/coffin_decorated.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/crypt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_broken.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_gate.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_pillar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_pillar_broken.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_seperate.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_seperate_broken.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/floor_dirt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/floor_dirt_grave.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/floor_dirt_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/grave_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/grave_A_destroyed.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/grave_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/gravemarker_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/gravemarker_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/gravestone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/Halloween Crypt Arena.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/lantern_hanging.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/lantern_standing.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_C.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_D.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pillar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/plaque.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/plaque_candles.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/post.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/post_lantern.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/post_skull.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_orange.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_orange_jackolantern.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_orange_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_yellow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_yellow_jackolantern.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_yellow_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/ribcage.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/shrine.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/shrine_candles.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/skull.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/skull_candle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/Stage 5 Bone Courtyard.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_large.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_large_decorated.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_orange_large.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_orange_medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_orange_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_yellow_large.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_yellow_medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_yellow_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Archive Temple 1 Clone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Archive Temple 1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Archive Temple.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Beast Altar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Block.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Border Strips_1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Border Strips_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Border Strips_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Broken Pillar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert Annex.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert House Block.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert House Medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert Tower.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Flat Rock_02.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Flat Rock_03.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Flat Rock_04.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Floor Tile Modules.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Floor Tile Modules_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Floor Tile Modules_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Lily Pads_1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Lily Pads_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Lily Pads_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Obelisk Tower.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Patterned Floor.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Pillar Crystal.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Plain Floor.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Pool Rim.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Relic Block Clone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Relic Block.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Ruined House Front.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Ruined House Top.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Rune.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Sandstone Stairs.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Slab.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Small Corner Pieces_1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Small Corner Pieces_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Small Corner Pieces_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_01.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_02.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_03.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_04.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_05.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_06.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/High Big Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/High Small Obstacle 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Low Big Obstacle 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Low Small Obstacle 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Temple Env Clone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Temple Env.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Fire Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Freeze Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Poison Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Shock Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Ultimate Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Armory Ability Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Armory Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Armory Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Expanded Hero Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Expanded Item Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Hero Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Hero Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Hero Preview.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Info Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Item Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Rewards.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Stat Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Tabs Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Button Selection Arrow.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Selector Slot.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Selector.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Evil Chest UI.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Heal Chest UI.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Joystick.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause Menu.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause/Abilities Grid.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause/Abilities Row.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause/Mini Ability Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Run Loot Reward Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Small Item Reward Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Small Reward Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Stage Failed.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Ability Select Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Combo Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Dodge Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Burning Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Crit Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Freezing Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Poison Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Shock Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Heal Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Item Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Burning Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Freezing Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Physical Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Poison Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Shock Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Gold Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Loading/Loading Screen.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Gem Chest Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Lucky Wheel Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Prize Row.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Stage Lucky Rewards.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Wheel Slice.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Continue Playing Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Dock.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Info Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stage Icon.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stage Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stages Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Settings Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Expanded Upgrade Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Expanded Upgrades Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Selectable Upgrade.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Upgrades Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Upgrades Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Achievements Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Daily Quests Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Forge Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Gem Wallet.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Economy.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Responsive Menu Surface.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Reward Notification Dot.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Upgrades Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Offline Progression/Run Summary.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Offline Progression/Stage Difficulty Selector.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Compact Run Result.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Hero EXP Bar.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Result Reward Tile.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Run Season HUD.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Stage/Developer Tools.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Stage/Language Picker.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Text Effects/UI Text Effects Demo.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Wave Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Attack Speed Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Base HP Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Crit Chance Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Dodge Chance Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/HP Recovery Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Move Speed Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Power Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Revive Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Shield Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/Shade Sphere.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Blob.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cactus Leaf.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cactus Spike Small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Capsule Half.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cone Flat.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Disc.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Dome.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Gem.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Ice Shard.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Ice Sphere.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Lase Plane.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Mole Dirt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Ring.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Shine Square.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Stinger.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Target Ring Big.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Target Ring Solid.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Target Ring.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Tornado.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Arcane Spellbook.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Barbarian Axe and Shield.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Barbarian Battle Axe.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Knight Greatsword.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Knight Sword and Shield.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Mage Staff.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Mage Wand.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/mug_empty.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/mug_full.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_badge_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_round.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_round_barbarian.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_round_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_spikes.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_spikes_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_square.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_square_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/spellbook_closed.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Ranger Longbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Rogue Dagger.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Rogue Hand Crossbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Rogue Heavy Crossbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Royal Greatsword.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Bow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Crossbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Wand.prefab` | — | 0 |
| `Assets/The Archer/Resources/Crafting Drop.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_green.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_orange.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_teal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_trifecta.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_yellow.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_painful_power.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_perfect_stance.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_rage.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_strong_aura.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_ignite.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_crit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_default.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_freeze.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_poison.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_shock.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_fire.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_fireball.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_fireball_alt.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_freeze.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_ignite.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_meteor.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_poison.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_shock.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_shock_trail.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_slimy_trail.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_ultimate_meteor.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_crit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_default.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_fire.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_freeze.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_poison.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_shock.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_bubble.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_bubble_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_bubble_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_heal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_heal_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_fire_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_freeze_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_laser.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_laser_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_poison_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_shock_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_ultimate_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_vampiric_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_fire_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_freeze_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_poison_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_shock_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_ultimate_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_fire_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_freeze_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_poison_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_shock_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_ultimate_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_evil.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_evil_shine.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_good.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_good_shine.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_heal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_heal_shine.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_gem.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_gem_pickup.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_item_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_star_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_star_pickup.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bat/vfx_bat_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bat/vfx_bat_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bell/vfx_shade_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bomb/vfx_bomb_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bomb/vfx_bomb_fuse.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_leaf.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_leaf_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_shot.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_slam.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_spike.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_spike_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_spike_small.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Candle/vfx_candle_fire.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Candle/vfx_candle_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Candle/vfx_candle_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cauldron/vfx_cauldron_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cauldron/vfx_cauldron_puddle.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cauldron/vfx_cauldron_splash.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Eye/vfx_eye_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Eye/vfx_eye_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Eye/vfx_laser_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_eye.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_landing.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spin_circle.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spin_lightning.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spin_trail.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_take_off.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_lightning_strike.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_lightning_vertical.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_zap.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_zap_line.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_aura.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_circle.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_crystal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_shard.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_sphere.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_sphere_crack.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Jellyfish/vfx_jellyfish_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Jellyfish/vfx_jellyfish_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mask/vfx_mask_wave_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mask/vfx_shade_sphere.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mask/vfx_shade_splatter.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mole/vfx_mole_burst.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mole/vfx_mole_digging.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mole/vfx_mole_dirt.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Plant/vfx_plant_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Plant/vfx_plant_pop.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Plant/vfx_plant_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Pumpkin/vfx_pumpkin_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Queen Wasp/vfx_honey_splatter.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Queen Wasp/vfx_queen_wasp_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Queen Wasp/vfx_queen_wasp_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Slime/vfx_slime_split.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_laser_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_tornado.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_fist_slam.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_laser.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_monkey_slam.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Trap/vfx_trap_surprise.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Trap/vfx_trap_surprise_3D.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Wasp/vfx_wasp_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Wasp/vfx_wasp_stinger.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_boss_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_enemy_defeat.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_enemy_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_exit_sparks.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_field_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_fireflies.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_player_ring.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_running_dust.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_running_flame.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_target_ring.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_target_ring_big.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_warning.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_arrow_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_bow_shot.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_wand_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_wand_shot.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_weapon_cloud.prefab` | — | 0 |
