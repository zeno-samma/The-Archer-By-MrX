# The Archer Roguelike — Mô tả dự án và quy tắc phát triển

> UI Inspector 26/10/2026: chọn Edit Landscape/Portrait trên ResponsiveMenuLayout rồi chỉnh RectTransform/TMP/Image trực tiếp; Auto Capture Inspector lưu vào profile đang chọn, có Undo. HeroPreviewBehavior có Refresh Editor Preview và trường hero/weapon/ánh sáng mẫu; không dùng save thật. Không thêm runtime reset mỹ thuật hoặc dựng UI; trạng thái/animation lấy cấu hình Inspector. Hướng dẫn: [Inspector UI Authoring](Docs/Inspector%20UI%20Authoring.md). Kiểm kê prefab/source: Tools/MrX Studio/UI/Audit Inspector Authoring. Không build.

> Rep menu input: mỗi ô dock phải có Image nhận raycast trên chính Button; icon/TMP chỉ trang trí. Không kiểm tra chuyển tab bằng onClick.Invoke đơn thuần: dùng Tools/MrX Studio/UI/Verify Menu Pointer Targets (Play Mode) để kiểm tra 18 vùng bấm thật qua GraphicRaycaster. Bốn tab còn lại dùng RepMenuPagesAuthoring, chi tiết tại Docs/Rep Menu UI.md.

> UI Rep 23/10/2026: Stage Page, Armory Page, Menu Dock và Item Card đã áp giao diện Rep được duyệt; scene Main Menu có override trình bày tương ứng. Chỉnh cả Landscape/Portrait trong ResponsiveMenuLayout (gồm pixelsPerUnitMultiplier của Image), không dựng UI runtime. Tài liệu: [Rep Menu UI](Docs/Rep%20Menu%20UI.md). Apply Approved Rep Menu là công cụ áp lại bố cục, có thể ghi đè chỉnh mỹ thuật thủ công. Không build.

> Cập nhật UI audio 22/10/2026: Button dùng `UI/UIButtonAudio.cs` (Default/Custom/Silent), gắn sẵn trong prefab/scene. Không gọi thêm PlayButtonClick trong handler đã có component. Dùng helper Click cho shortcut thông thường; selector có mũi tên ẩn khi gamepad dùng PlayShortcut và hành động cũ. Khi thay callback, chỉ RemoveListener callback mình sở hữu. Hướng dẫn: [UI Button Audio](Docs/UI%20Button%20Audio.md). Không build.

> Áp dụng cho toàn bộ repository, dành cho người phát triển và trợ lý AI. Hiện trạng bên dưới được đối chiếu với source, scene, prefab, ScriptableObject và các phần liên quan của tài liệu đi kèm vào ngày **18/10/2026**. Đây là kết quả đọc tĩnh, chưa phải xác nhận Play Mode hoặc build thành công.

## 1. Mục đích và cách dùng

Hướng dẫn thao tác menu **Tools → MrX Studio**: [MrX Studio Tools](Docs/MrX%20Studio%20Tools.md). Tài liệu liệt kê từng lệnh, điều kiện Play/Edit, file cấu hình và tác động tới prefab/save.

Tài liệu này là bản đồ để hiểu dự án, chọn đúng nơi thay đổi và giữ các hệ thống liên quan hoạt động nhất quán. Phần **hiện trạng** mô tả source đang có; phần **quy tắc** định hướng công việc tiếp theo. Các điểm bất nhất đã biết không được xem là thiết kế cần giữ nguyên.

- Yêu cầu trực tiếp của người dùng quyết định mục tiêu và phạm vi công việc. Nếu khác tài liệu này, thực hiện theo yêu cầu đó và cập nhật tài liệu khi phù hợp.
- Trước khi sửa, đọc thay đổi đang có và các file liên quan. Giữ công việc của người dùng; không hoàn tác thay đổi ngoài phạm vi.
- Chủ động hoàn thành các bước cần thiết trong phạm vi đã được giao. Không đặt thêm bước xin xác nhận cho thao tác đọc, sửa hoặc kiểm tra thông thường đã được cho phép.
- **Quy tắc build từ 20/10/2026:** chỉ chạy build khi người dùng yêu cầu build rõ ràng. Không tự build để kiểm tra hoặc bàn giao một tác vụ tính năng; dùng compilation/Play Mode và ghi giới hạn kiểm tra tương ứng.
- Khi thiếu thông tin, kiểm tra source, dữ liệu và reference trước. Chỉ hỏi những điểm thực sự ảnh hưởng đến kết quả mà không thể suy ra hợp lý.
- Giữ thay đổi tập trung; không gộp nâng phiên bản, đổi kiến trúc diện rộng hoặc cân bằng lại toàn game vào một tác vụ nhỏ.
- Khi hiện trạng khác tài liệu, ưu tiên bằng chứng từ source và asset đang được sử dụng. Không suy ra tính năng đã hoạt động chỉ vì có file hoặc tên tương ứng.

## 2. Bản chất và định hướng gameplay

The Archer là game hành động roguelike 3D có tiến trình lâu dài giữa các lượt chơi, phát triển từ template **October Studio**. Namespace chủ đạo là `MrX`. Người chơi điều khiển nhân vật qua các phòng, vượt qua từng đợt quái, chọn ability trong trận, thu thập vật phẩm và cải thiện trang bị/nâng cấp giữa các lượt.

Vòng chơi cốt lõi:

**Chuẩn bị hero/trang bị → chọn stage → di chuyển và né đòn → đứng yên để tự bắn → diệt quái → nhận XP/phần thưởng → chọn ability → chuyển wave/phòng → thắng hoặc thua → trở về menu.**

Nguyên tắc thiết kế khi phát triển:

- Giữ trọng tâm vào quyết định di chuyển, thời điểm dừng để bắn và lựa chọn build trong trận. Nếu đổi cơ chế này, coi đó là thay đổi gameplay rõ ràng và kiểm tra các hệ thống phụ thuộc.
- Đòn đánh nguy hiểm cần có dấu hiệu nhận biết và thời gian phản ứng phù hợp; animation, vùng cảnh báo, collider và thời điểm gây damage phải khớp nhau.
- Ability nên tạo lựa chọn hoặc tương tác có ý nghĩa với hệ stat, projectile, spirit/orb và các hiệu ứng sẵn có.
- Tách sức mạnh tạm thời của lượt chơi khỏi tiến trình lâu dài. Mô tả rõ tài nguyên nào được giữ lại và điều kiện nhận thưởng.
- Ưu tiên cấu hình qua dữ liệu hiện có trước khi thêm điều kiện đặc biệt vào manager hoặc UI.
- Những điều trên là nguyên tắc phát triển; không khẳng định mọi nội dung hiện tại đã được cân bằng theo chúng.

## 3. Nền tảng kỹ thuật và cấu trúc thư mục

### 3.1. Cấu hình đã xác minh

| Thành phần | Hiện trạng | Nguồn đối chiếu |
| --- | --- | --- |
| Unity Editor | `6000.6.2f1` | `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | URP `17.6.0`, đang được gán trong Graphics Settings | `Packages/manifest.json`, `ProjectSettings/GraphicsSettings.asset` |
| Input | New Input System `1.20.0` | `Packages/manifest.json`, `ProjectSettings/ProjectSettings.asset` |
| Camera | Cinemachine `6.6.0` | `Packages/manifest.json` |
| Navigation | AI Navigation `2.0.14` | `Packages/manifest.json` |
| UI | uGUI và TextMesh Pro; công cụ Editor có dùng UI Toolkit | Source và package hiện có |
| Assembly | Chưa có `.asmdef` riêng trong Assets | Cấu trúc source hiện tại |

URP asset đang được Graphics Settings tham chiếu nằm trong `Assets/The Archer/Scriptables/URP`. Các asset PC/Mobile ở `Assets/Settings` không tự chứng minh chúng là pipeline đang dùng. Cấu hình Android tồn tại, nhưng chưa xác nhận build target đang active trong Editor.

### 3.2. Bản đồ thư mục

| Đường dẫn | Vai trò |
| --- | --- |
| `Assets/The Archer/Scripts` | Gameplay, UI, các dịch vụ nền tảng và công cụ Editor |
| `Assets/The Archer/Scriptables` | Database, cấu hình stage, hero, item, ability, upgrade, audio và effects |
| `Assets/The Archer/Scenes` | `Main Menu`, `Loading Screen`, `Game` |
| `Assets/The Archer/Prefabs` | Player, hero, weapon, enemy, môi trường, UI và behavior nội dung |
| `Assets/The Archer/Animations` | Animator controller, clip và animation event |
| `Assets/The Archer/Models`, `Materials`, `Shaders`, `Sprites`, `Textures`, `VFX`, `Audio` | Tài nguyên trình bày và hiệu ứng |
| `Assets/The Archer/Scripts/Toolkit` | Save, currency, input, audio, vibration, pool, easing và tiện ích |
| `Assets/The Archer/Scripts/Editor` | Stage Creator, khởi động scene, Pipeline Switcher và các công cụ khác |
| `Assets/The Archer/The Archer Documentation.pdf` | Tài liệu template gốc; cần đối chiếu với bản đã tùy biến |
| `Packages`, `ProjectSettings` | Package và cấu hình dự án cần quản lý cùng source |
| `Library`, `Temp`, `Logs`, `obj`, các file project IDE sinh tự động | Dữ liệu phát sinh; không chỉnh trực tiếp để sửa gameplay |
| `UserSettings` | Trạng thái cá nhân của Editor; tránh đưa vào thay đổi tính năng nếu không liên quan |

Source gameplay được chia thành `Abilities`, `Armory`, `Chest`, `Drop`, `Enemy`, `Experience`, `Player`, `Projectiles`, `Quests`, `Stage`, `Stats`, `Status Effects`, `UI`, `Upgrades`, cùng `Other`, `Particles`, `Toolkit` và `Editor`.

## 4. Nội dung đang được sử dụng

Các số dưới đây là ảnh chụp tại ngày khảo sát, không phải giới hạn thiết kế. Đếm theo database đang được scene tham chiếu, không theo toàn bộ asset tồn tại trên đĩa.

| Nội dung | Đang đăng ký |
| --- | --- |
| Stage | 4 |
| Room / Wave | 58 / 134 |
| Enemy | 22 mục: 16 thường, gồm hai cấp slime con, và 6 boss |
| Ability | 80 |
| Rarity ability | Common, Rare, Mystic, Legendary; trọng số hiện tại `50 / 30 / 15 / 5` |
| Item | 14; bốn slot Weapon, Armor, Helmet, Ring |
| Hero | 1: Ranger |
| Permanent upgrade | 9 |
| Chest | 3 loại: Good, Health, Evil |
| Currency | 1 ID: `gold` |

| Stage | Asset trong `Scriptables/Stages` | Room | Wave |
| --- | --- | ---: | ---: |
| Secret Chambers | `Stage Data 001.asset` | 28 | 34 |
| Ancient Temple | `Stage Data 002.asset` | 1 | 33 |
| Secrets Pyramid | `Stage Data 003.asset` | 28 | 34 |
| Forgotten Tombs | `Stage Data 004.asset` | 1 | 33 |

`Armory Database` hiện chỉ đăng ký Ranger qua `Hero Data 003.asset`. Archer và Thief vẫn có asset nhưng không nằm trong danh sách hero đang dùng. Loadout mặc định từ dữ liệu hiện tại là Ranger, Signature Bow và Dragon's Breath Plate. Không dùng mô tả hero/stage của tài liệu template gốc để thay thế dữ liệu hiện tại.

## 5. Kiến trúc và vòng đời

### 5.1. Hai cấp điều phối

| Cấp | Đầu mối | Trách nhiệm |
| --- | --- | --- |
| Xuyên scene | `Scripts/Other/GameController.cs` | Giữ dịch vụ save, currency, input, audio, vibration, armory, upgrades; chuyển scene |
| Trong lượt chơi | `Scripts/Stage/StageController.cs` | Chọn stage, tạo player, khôi phục run, phối hợp room, enemy, projectile, drop, XP, ability, camera và kết quả |
| Trong phòng | `Scripts/Stage/Room/RoomBehavior.cs` | Spawn wave, theo dõi quái/rương, thu drop, chờ chọn ability và mở lối ra |
| Dữ liệu nội dung | Các `*Database`, `*Data`, `*Level` | Cấu hình và reference tới prefab; không thay vai trò của save |

`GameController` tồn tại xuyên scene bằng `DontDestroyOnLoad`. Các manager đăng ký vào `GameController` hoặc `StageController` bằng phương thức `Register...`. Đây là mô hình truy cập dịch vụ qua static hiện hữu; chưa có hệ dependency injection tách rời.

Quy tắc vòng đời:

- Dùng đầu mối đã có; không tự tạo thêm bản sao manager để giải quyết lỗi chưa khởi tạo.
- Kiểm tra nơi đăng ký và thời điểm sử dụng trước khi gọi manager trong `Awake`, `OnEnable` hoặc `Start`. Không dựa vào thứ tự tên file/component.
- Tôn trọng `DefaultExecutionOrder` hiện có. Dùng cơ chế như `StageController.DoAfterRoomLoaded` khi công việc phụ thuộc phòng đã sẵn sàng.
- Static state mới phải có chiến lược reset phù hợp khi bắt đầu Play hoặc tải lại lượt chơi; kiểm tra trường hợp không reload domain.
- Mỗi event, modifier, coroutine và easing phải có chủ sở hữu, nơi khởi tạo và nơi dọn. Đăng ký/hủy đăng ký phải đối xứng, kể cả khi panel mở nhiều lần hoặc object được pool tái sử dụng.

### 5.2. Khởi động và chuyển scene

Build Settings đăng ký theo thứ tự `Main Menu` (0), `Loading Screen` (1), `Game` (2). `Main Menu` là bootstrap của luồng chuẩn, chứa các dịch vụ dùng chung và dữ liệu menu. `GameController.LoadStage()` chuyển qua loading rồi tải Game; chiều về menu chuyển tiền gameplay sang ví lâu dài và lưu dữ liệu.

`DefaultSceneLoader` trong Editor mặc định đặt scene bắt đầu Play là Main Menu; có toggle tại `Tools/MrX Studio/Default Scene Loader Enabled`. Đọc trạng thái toggle thực tế nếu Play bắt đầu khác scene đang mở.

- Khi đổi scene, cập nhật đồng bộ Build Settings, `Scriptables/Scene Settings.asset` và các reference liên quan.
- Không coi việc mở trực tiếp Game scene là tương đương luồng bootstrap đầy đủ.
- Testing data trong save có thể đưa người chơi vào stage/room/wave thử nghiệm. Kiểm tra dữ liệu này khi hành vi khởi động khác dự kiến.

### 5.3. Stage, room và wave

`StageData` là ScriptableObject. `RoomData` và `WaveData` là dữ liệu serializable nằm bên trong stage, không phải mỗi phòng/đợt quái là một scene riêng. `RoomBuilder` lấy môi trường từ pool và áp transform; `RoomsShuffler` xáo trộn room/wave theo `GroupId`, rồi lưu thứ tự để resume.

Một wave có enemy spawn, chest spawn, XP, reward, âm nhạc và tùy chọn delay. Room quản lý quái sống, kết thúc wave, nhặt drop, chờ UI ability và chuyển sang wave tiếp theo. Cuối phòng sẽ mở exit hoặc tự chuyển nếu cấu hình không có exit; phòng cuối dẫn tới hoàn thành stage.

Hệ số HP/damage enemy hiện tính riêng theo công thức: **hệ số gốc + bước theo phòng × chỉ số phòng + bước theo wave × chỉ số wave**. Không hiểu nhầm đây là phép nhân lũy tiến giữa các phòng.

- Giữ cấu trúc spawn, reward, transform, camera confinement và group khi chỉnh stage data.
- Không để wave chờ vô hạn vì enemy/chest đã biến mất nhưng chưa báo kết thúc vòng đời.
- Khi summon enemy, dùng `Room.SpawnAdditionalEnemy` hoặc tích hợp tương đương để room theo dõi đúng số quái.
- Khi đổi thứ tự hoặc số lượng room/wave, kiểm tra dữ liệu shuffle và chỉ số đã lưu trong run cũ.

## 6. Player, chiến đấu và AI

`PlayerBehavior` điều phối HP, stat, input, mục tiêu và liên kết hero. Hero/weapon thực tế được tạo theo Armory. `EnemiesDetector` chọn mục tiêu; `HeroBehavior` quản lý Animator; animation event truyền lệnh bắn tới weapon. Weapon thiết lập projectile, các hàng mũi, multishot, bounce, ricochet, piercing, split và status effect.

Player di chuyển trực tiếp trên XZ; hệ navigation vẫn có vai trò ở các thao tác liên quan. Enemy dùng coroutine và animation event, kết hợp `NavigationHandler`/NavMeshAgent; charge và knockback có thể tự thay đổi transform. `EnemyStateBehavior` có tồn tại nhưng không phải hệ AI trung tâm của các enemy chính.

Quy tắc combat:

- Gây sát thương qua `IProjectileTarget.TakeDamage` hoặc đường đi hiện có, không tự trừ healthbar để bỏ qua dodge, invincibility, damage reduction hoặc callback chết.
- Giữ đồng bộ Animator, animation event, weapon và thời điểm tạo đạn. Không phát thêm đạn trong `Update` nếu đường bắn tương ứng đã được event điều khiển.
- Enemy dùng vòng đời spawn → hành vi → nhận damage → defeat → animation kết thúc → tắt object. Không bỏ callback defeat hoặc bước hoàn tất animation.
- Với movement mới, kiểm tra cả NavMesh lẫn những đoạn dịch chuyển transform. Không mặc định mọi tốc độ chạy đều lấy từ NavMeshAgent.
- Khi đổi tầm bắn, kiểm tra đồng thời quãng đường đạn, detector collider, quy tắc chọn mục tiêu và vòng chỉ thị tầm bắn.
- Giá trị `Float`/`Int` có thể random lại mỗi lần đọc. Nếu một attack cần số lượng/thời lượng nhất quán, lấy giá trị một lần vào biến local rồi dùng lại.

## 7. Stat, status effect và object pool

### 7.1. Stat và đơn vị

`MultiplicativeStat` kết hợp initial value, các multiplier và child stat bằng phép nhân. `AdditiveStat` cộng initial value, các adder và child stat. Stat có event và có thể được nhiều hệ thống giữ reference.

- Thêm buff bằng `StatMultiplier`/`StatAdder`, lưu reference và gỡ đúng đối tượng đã thêm. Không thêm modifier trong `Clear` và không thay toàn bộ đối tượng stat đang được hệ khác theo dõi.
- HP/damage cơ sở được tổng hợp từ hero và item; các stat khác có quy tắc cộng/nhân riêng trong `PlayerStatsHandler`. Kiểm tra cách tổng hợp trước khi thêm stat mới.
- Crit/dodge của player dùng phần trăm `0–100`; resistance của status effect dùng tỷ lệ `0–1`.
- `DamageReduction` hiện là hệ số nhân sát thương nhận, không tự diễn giải thành phần trăm giảm damage.
- Phân biệt level nội bộ bắt đầu từ `0` với level hiển thị. Ability/upgrade chưa sở hữu có thể dùng `-1`.

### 7.2. Status effect

Các effect hiện có: Ignite, Freeze, Poison, Shock. `StatusEffect` quản lý mục tiêu và tick damage; `StatusEffectsHandler` trên mục tiêu kiểm tra khả năng áp dụng, resistance, cooldown, duration và hiệu ứng hình ảnh. `IProjectileTarget` thống nhất giao diện target cho player/enemy.

- Apply/remove phải đối xứng; trả animator/navigation/modifier về trạng thái phù hợp khi effect kết thúc, target chết hoặc object trở về pool.
- Không mặc định một effect được stack nhiều lần; kiểm tra quy tắc chặn và cooldown hiện tại.
- Nếu thêm damage type, cập nhật mapping damage, text indicator, resistance, dữ liệu effect và shader/VFX liên quan.

### 7.3. Pool và trạng thái tái sử dụng

Enemy, projectile, drop, particle, audio và môi trường sử dụng các pool hiện hữu. `PoolComponent.GetEntity()` bật GameObject trước khi caller cấu hình dữ liệu lần sử dụng.

- Đăng ký prefab với manager trước khi lấy projectile/particle. Không giả định `GetProjectile` tự tạo pool khi chưa đăng ký.
- `OnEnable` không được giả định target, damage hoặc vị trí mới đã được caller gán.
- Mỗi field mutable mới cần có nơi khởi tạo và reset. Đặc biệt kiểm tra target, target bỏ qua, piercing, ignore obstacles, scale, speed, lifetime, effect list và cờ crit.
- Dùng vòng đời trả về pool đang có; không `Destroy` để trả một object về pool.
- Phân biệt `Hide` có thể có hiệu ứng/delay với `Clear` cần dọn ngay. Không giả định hai đường đi luôn phát cùng callback.
- Dừng hoặc vô hiệu hóa callback trễ của lần dùng trước để chúng không tác động lần dùng mới.

## 8. Ability, XP, Armory và nâng cấp lâu dài

### 8.1. Tiến trình trong lượt chơi

`AbilitiesManager` tạo behavior từ prefab và lưu type/level vào `AbilitiesSave`. Run mới xóa ability run cũ; resume phục hồi chúng, áp testing preset phù hợp rồi nạp ability của hero/trang bị. Các behavior có thể sửa stat, cấu hình projectile, theo dõi event hoặc điều khiển spirit/orb/meteor.

XP đi qua `ExperienceManager`; khi đủ ngưỡng, UI chờ drop/thông báo cần thiết rồi mở selector. Selector chọn tối đa ba ability cùng rarity, dùng Endgame ability khi thiếu lựa chọn. Prerequisite hiện là **có ít nhất một** ability trong danh sách, không phải có tất cả.

- Giữ quy tắc prerequisite/rarity/repeat/Endgame trừ khi tác vụ yêu cầu thay thiết kế tương ứng.
- Rarity được chọn theo trọng số cấu hình; không hardcode tổng luôn bằng 100.
- Curve XP hiện có 31 ngưỡng; hàm lấy XP clamp vào ngưỡng cuối. Đây không tự động là giới hạn level.
- Khi đổi XP/level, xét việc nhận nhiều level cùng lúc, popup pause và save đang có XPLevel vượt chiều dài cấu hình mới.
- Ability tức thời cho gold/hồi máu và state tích lũy cần kiểm tra resume để không nhận thưởng lại ngoài ý đồ. Save type/level không tự lưu mọi timer hoặc stack riêng trong behavior.

### 8.2. Tiến trình lâu dài

`UpgradesManager` giữ level nâng cấp lâu dài, tạo behavior khi vào stage và dọn khi thoát. `ArmoryManager` quản lý item/hero mở khóa, level và trang bị. Vật phẩm nhặt lần đầu được mở khóa; bản trùng hiện tăng level đến giới hạn.

- Mỗi `ItemType` chỉ có một item được trang bị; một hero được chọn; loadout phải có weapon hợp lệ.
- Ability gắn với hero/item được lấy ở **level hiện tại**, không tự cộng dồn mọi level trước. Muốn giữ ability qua level sau phải cấu hình tiếp ở level đó.
- Vũ khí cần mapping animation theo cặp hero và weapon; thêm prefab/data mà thiếu mapping chưa đủ để dùng được.
- Không xóa permanent upgrade hoặc Armory khi bắt đầu run mới.
- Testing preset có `EnabledInEditor` và `EnabledInBuild` riêng; tránh để cấu hình thử nghiệm cấp ability ngoài ý muốn trong bản phát hành.

## 9. Save, danh tính nội dung và kinh tế

`SaveManager` quản lý `SaveDatabase` gồm các cell serialize. Với phiên bản Unity hiện tại, nhánh lưu file dùng JSON; Editor dùng tên `game_save_editor`, player dùng `game_save`. Source còn hỗ trợ PlayerPrefs và WebGL localStorage. Các nhánh binary dành cho Unity cũ không mô tả cách lưu chính của bản hiện tại.

| Khóa/nhóm save | Trạng thái |
| --- | --- |
| `Stage` | Stage đã chọn, mở khóa, hoàn thành, số lần thử và thông tin liên quan |
| `Continue Playing` | Run đang dở: room/wave, thứ tự shuffle, HP, XP, reroll, revive và item đã nhặt |
| `Abilities Save` | Ability type/level của run để phục hồi |
| `Armory` | Hero/item, level và trạng thái trang bị lâu dài |
| `Upgrades Save` | Level nâng cấp lâu dài |
| `Crafting Inventory` | Mảnh riêng theo ItemData ID và bốn nguyên liệu rèn lâu dài |
| `Daily Quests` | Tiến độ nhiệm vụ, ngày reset và cờ nhận thưởng nhiệm vụ/mốc hằng ngày |
| Currency ID, ví dụ `gold` | Ví lâu dài |
| Currency ID + `:___GAMEPLAY___` | Ví tạm của lượt chơi |

Quy tắc tương thích dữ liệu:

- Giữ giá trị số đã serialize của enum như `EnemyType`, `AbilityType`, `UpgradeType`, `ItemType`, `StatType`. Không đổi thứ tự hoặc tái sử dụng ID cũ làm thay đổi ý nghĩa dữ liệu.
- Giữ `.meta` đi cùng asset. Phân biệt GUID asset của Unity với ID riêng của stage/item/hero được lưu bên trong dữ liệu.
- Không tái tạo ID chỉ để đổi tên nội dung. Đổi khóa save, ID, enum hoặc schema cần xử lý dữ liệu cũ và mô tả cách migration/reset liên quan.
- Khóa chuỗi hiện được tra qua `GetHashCode`; không giả định đổi tên khóa hoặc đổi cơ chế hash là tương thích ngược.
- Kiểm tra giới hạn level/index từ save trước khi truy cập data. Khi giảm số level hoặc xóa nội dung, xử lý save cũ thay vì để truy cập ngoài mảng.
- Run đang dở cần giữ nhất quán HP, XP, room/wave, shuffle, ability, reroll, item và tiền gameplay. Việc resume không được tự nhân đôi phần thưởng.
- Không xóa save thực tế của người dùng để làm bước kiểm tra mặc định. Dùng dữ liệu thử nghiệm riêng khi cần.

Quy tắc kinh tế:

- Phân biệt ví lâu dài với ví gameplay; không chuyển tiền giữa hai ví ở nhiều nơi mà thiếu kiểm soát.
- Dùng currency ID và `Price` hiện có. Theo luồng hiện tại, reroll dùng ví gameplay; mua/nâng cấp ở menu dùng ví lâu dài.
- `ApplyGameplayAmounts` chuyển số tạm sang ví lâu dài rồi xóa số tạm. Kiểm tra cả thoát, thất bại, hoàn thành và từ chối tiếp tục run để tránh grant hai lần.
- Item nhặt trong trận được ghi nhận trước và grant ở bước kết thúc phù hợp. Giữ sự nhất quán giữa UI phần thưởng, Armory và save.

## 10. Quy tắc source, asset, UI và rendering

- **Quy tắc UI từ 19/10/2026:** mọi giao diện mới phải được vẽ/lưu bằng uGUI/TMP trong scene hoặc prefab có thể sửa trực tiếp trong Editor. Không dựng GameObject, thêm component UI hay tạo cây giao diện bằng code runtime. Danh sách động chỉ tái sử dụng view đã vẽ hoặc instance từ prefab. Bố cục/font/sprite trong prefab là nguồn chỉnh sửa chính; script chỉ binding dữ liệu, tương tác và tiến độ.

- Theo convention C# hiện có: namespace `MrX` phù hợp hệ thống, PascalCase cho type/member công khai, field serialized theo mẫu file lân cận. Giữ API `virtual`/kế thừa khi nó là điểm mở rộng đang dùng.
- Ưu tiên subclass, prefab variant hoặc asset cấu hình mới khi phù hợp. Không tạo lại một hệ thống đã có để xử lý cùng trách nhiệm.
- Đổi tên field serialized phải giữ khả năng đọc dữ liệu cũ, ví dụ dùng `FormerlySerializedAs` khi phù hợp. Không xóa reference Inspector mà không kiểm tra scene/prefab liên quan.
- Không đưa dependency `UnityEditor` mới vào runtime. Đặt công cụ trong thư mục `Editor` hoặc bọc điều kiện biên dịch đúng phạm vi.
- Không tự nâng Unity/package, thêm package hay đổi pipeline khi tác vụ không cần đến chúng.
- Không sửa trực tiếp file input C# được sinh tự động; tìm đúng `.inputactions` mà lớp runtime sử dụng rồi tái sinh theo workflow hiện có.
- UI gameplay dùng uGUI/TMP. Giữ điều hướng keyboard/gamepad, safe area và trạng thái tương tác khi thêm panel hoặc popup.
- Popup/animation vẫn cần hoạt động khi game pause; dùng unscaled time phù hợp. Phối hợp trạng thái mở page và quyền khôi phục `Time.timeScale`, tránh đóng một popup làm chạy game khi popup khác còn mở.
- Dùng AudioManager, ParticlesManager, EasingManager và các tiện ích hiện có khi phù hợp. Không thêm một bộ quản lý song song cho cùng nhu cầu nhỏ.
- Dự án đang dùng URP; không chạy Pipeline Switcher hoặc convert toàn bộ material chỉ để sửa một hiệu ứng. Giữ tương thích shader/rim/status effect hiện có.
- Physics phụ thuộc tên layer/tag và collision matrix. Tags hiện có `Wall`, `Ground`, `Obstacle`; layers gồm `Player`, `Enemy`, `Player Projectile`, `Enemy Projectile`, `Enemies Detector`, `Player Detector`, `Obstacle`, `Room Exit`, `Drop`, `Ground`, `Chest`, `Weapon Holder`.
- Khi chỉnh collider/môi trường, kiểm tra layer, tag, NavMeshObstacle và hai loại navigation surface hiện có trong Game scene.
- Khi chỉnh YAML bằng tay, xác minh GUID/fileID và asset đích qua `.meta`. Không sửa reference bằng phỏng đoán; không tạo thay đổi định dạng hàng loạt ngoài phạm vi.

## 11. Quy trình thêm hoặc sửa nội dung

| Loại công việc | Các phần cần đồng bộ |
| --- | --- |
| Stage / Room / Wave | `StageData`, Stage Database, prefab môi trường, spawn, reward, group shuffle, camera confinement, music, unlock condition và save cũ |
| Enemy / Boss | Enum ID, subclass `EnemyBehavior`, prefab, Animator/events handler, collider, navigation, status/renderers/healthbar, Enemies Database, drop và dữ liệu wave |
| Ability | Enum ID, data/level, behavior và prefab đúng kiểu, Abilities Database, icon/text, rarity, prerequisite, repeat/Endgame, apply/cleanup và resume |
| Projectile | Lớp nền phù hợp, prefab, registration/pool, target/layer/collision, damage/effect, reset và Hide/Clear |
| Status effect | Enum/mapping, logic tick, target handler, resistance/duration/cooldown, VFX/audio/shader, apply/remove và damage/text type nếu có |
| Item | ID ổn định, ItemData/levels, stat/ability, rarity/icon/price, Armory Database, unlock/drop và UI |
| Hero | ID ổn định, HeroData/levels, prefab, Animator/events, model/shader, animation mapping với weapon hỗ trợ, Armory Database và UI |
| Weapon | WeaponData/prefab, projectile registration, animation mapping hero–weapon, spawn point, range, stat và hiệu ứng |
| Permanent upgrade | Enum ID, UpgradeData/levels/price, behavior prefab, Upgrades Database, UI và cleanup khi thoát stage |
| UI / Popup | Controller hiện có, reference, event lifecycle, input navigation, pause/unscaled time, safe area và trình bày dữ liệu |

Dùng `Tools/MrX Studio/Stage Creator` cho authoring stage khi Editor sẵn sàng; công cụ có thể dựng scene tạm và lưu trạng thái authoring. `Tools/MrX Studio/Abilities Tester` hỗ trợ preset và kiểm tra ability. Sau thao tác công cụ, xem diff để phát hiện asset/scene bị đổi ngoài ý muốn.

Một nội dung chỉ hoàn thành khi dữ liệu đã được đăng ký đúng database, prefab có đủ reference và đường đi thực tế tới gameplay/UI đã được kiểm tra theo phạm vi. Có file trên đĩa chưa đồng nghĩa nội dung đã được tích hợp.

## 12. Các tùy biến và điểm cần lưu ý trong bản hiện tại

### 12.1. Tùy biến có chủ đích cần nhận diện

- `Scripts/Drop/DropManager.cs`, `CanDropItem`: luôn trả `true`; comment nêu mục đích cho boss rơi item lặp để farm. Không tự khôi phục khóa rơi một lần của template khi chỉnh loot khác.
- `Scripts/Other/ConsoleTimer.cs`: đo thời gian phòng; được gắn trong Game scene. `RoomBehavior` và `StageController` gọi trực tiếp khi bắt đầu/kết thúc phòng hoặc kết thúc run.
- `Scripts/Armory/ArmoryManager.cs`, `UnlockItem`: đăng ký listener equip ngay sau mở khóa item.
- `Scripts/UI/Main Menu/MainMenuStagesPageBehavior.cs`: trang stage đã tích hợp hero panel.

### 12.2. Bất nhất quan sát được qua đọc source

Các mục sau là đầu mối cần kiểm tra khi tác vụ chạm đến phần liên quan, không phải kết quả xác nhận tác động trong Play Mode. Không coi hành vi lỗi là yêu cầu phải bảo tồn; sửa trong phạm vi tác vụ và mô tả hành vi trước/sau.

| Vị trí | Quan sát |
| --- | --- |
| `AbilitiesManager.GetAbilitiesForSelector`, `GoodChestBehavior` | `selectedRarity = selectedRarity++` không tăng rarity như ý đồ nhánh code |
| `Armory/WeaponBehavior`, vòng lặp tạo nhiều mũi | Nhân trực tiếp biến damage khi crit có thể ảnh hưởng mũi phía sau cùng hàng |
| `Player/PlayerStatsHandler.Init` | Crit damage multiplier lấy từ `StatType.DamageReduction`; cần phân biệt ý nghĩa dữ liệu và công thức |
| `Player/Hero/HeroBehavior.OnDestroy` | Một số listener visuals dùng `+=` thay vì hủy đăng ký |
| Một số `UpgradeBehavior.Clear`, `TrifectaAbilityBehavior.Clear` | Có lệnh thêm modifier tại nhánh dọn thay vì gỡ modifier |
| `Chest/HealthChestBehavior` | Vòng chọn ability có điều kiện counter nhưng không tăng counter |
| `Player/EnemiesDetector` và weapon range | Chỉ thị range, bán kính detector và lifetime đạn chưa dùng chung một cơ chế cập nhật |
| `Projectiles/SimpleProjectile.ResetOverrides` | Một số field như target bỏ qua, piercing, ignore obstacles không được reset ở đây; cần kiểm tra caller khi tái dùng |
| `Other/ConsoleTimer`, `Room/RoomBehavior` | Đã sửa 21/10: khóa số phòng ổn định, reset thật, tổng không cộng dồn; thêm report wave/run ở `Docs/Combat Pacing and Balance.md` |
| `RoomBehavior.EndWave` | Đã sửa 21/10: dùng delay wave vừa kết thúc và trừ khoảng thời gian đã chờ; Stage 5 có nhịp nghỉ 0,8/2/3 giây |
| Một số file runtime, ví dụ `HeroData`, `Int`, `Price` | Có import `UnityEditor` ngoài điều kiện biên dịch; cần kiểm tra khi build player |

Danh sách này không thay thế review toàn diện. Đọc lại code hiện tại trước khi xử lý vì các mục có thể đã được sửa sau ngày khảo sát; cập nhật hoặc bỏ mục đã hết hiệu lực.

## 13. Xác minh thay đổi và cách bàn giao

Chọn kiểm tra theo tác động thực tế; không chạy toàn bộ mọi lượt và không tạo test chỉ để lặp lại cách triển khai. Dùng kiểm tra tự động khi có điều kiện và chúng bảo vệ hành vi có ý nghĩa.

| Loại thay đổi | Xác minh phù hợp |
| --- | --- |
| Chỉ tài liệu | Đọc diff, kiểm tra Markdown, đường dẫn, tính đúng của khẳng định; không cần Play Mode hoặc Unity test |
| Script runtime | Kiểm tra compilation/Console; xác minh đường đi chịu ảnh hưởng và vòng đời cleanup |
| Combat / Enemy / Projectile | Spawn lần đầu, tái sử dụng pool, chết giữa đòn, hết wave, pause/resume, revive hoặc thoát màn nếu liên quan |
| Ability / Stat / Upgrade | Áp dụng, tăng/giảm level, gỡ modifier, tương tác build và resume nếu liên quan |
| Save / Progression / Economy | Run mới, save cũ, tiếp tục run, nhận thưởng một lần, tiền tạm/lâu dài và kết thúc run |
| Stage / Prefab / Content | Database/reference, spawn/animation, kết thúc wave/room, exit, NavMesh/camera và lưu/mở lại dữ liệu authoring |
| UI | Chuột/touch/keyboard/gamepad phù hợp, focus, đóng/mở lặp, pause và safe area |
| Rendering | Shader/material/VFX trong URP và hiển thị trên đường đi đã thay đổi |
| Editor tool | Thao tác công cụ, lưu/mở lại và kiểm tra diff của các asset liên quan |

Khi dùng Unity MCP, đọc trạng thái Editor và tài nguyên liên quan trước khi thao tác. Sau sửa script, đợi compilation kết thúc và xem Console; chỉ báo runtime đã được kiểm tra nếu thực sự chạy. Không cần làm phát sinh thay đổi scene/save để xác minh một tác vụ chỉ có tài liệu.

Bàn giao ngắn gọn bằng tiếng Việt: nêu điều đã thay đổi, mục đích, file chính, kiểm tra thực sự đã chạy và giới hạn còn lại có liên quan. Phân biệt kết luận từ source với kết luận từ runtime/build. Cập nhật tài liệu này khi thay đổi kiến trúc, ID/save, đường chạy, quy tắc nội dung hoặc tùy biến làm các mô tả trên không còn đúng.

## 14. Nhiệm vụ hằng ngày — bổ sung ngày 19/10/2026

- `Scripts/Quests` chứa database, save và manager; `Scripts/UI/Main Menu/Quests` chứa page. `GameController` khởi tạo manager sau khi save sẵn sàng; menu đăng ký `MainMenuPageType.DailyQuests = 3`, giữ nguyên các giá trị enum cũ.
- Cấu hình/icon ở `Assets/The Archer/Resources/Daily Quests Database.asset`; prefab chỉnh được ở cùng thư mục, tên `Daily Quests Page.prefab`. Không đổi tên hoặc di chuyển hai asset này ra khỏi Resources nếu chưa sửa đường dẫn tải tương ứng.
- Menu dùng prefab có sẵn; nếu chưa có prefab thì tạo view từ cấu hình. Dock thay ô hình cuộn giấy `ui_5` bằng tab QUESTS có chức năng; không thêm một hàng dock mới.
- Năm nhiệm vụ mặc định là hoàn thành stage 3 lần, hạ 50 quái, nhặt 3 item, đăng nhập và nâng hero một lần. Source chưa có resource node/gem; không trình bày chúng như tài nguyên đã được triển khai. Phần thưởng hiện dùng ví gold lâu dài.
- Hook tiến độ nằm trong `StageController.OnExitReached`, `RoomBehavior.ProcessDefeatedEnemy`, `DropManager.OnDropPickedUp` và setter `HeroSaveData.Level`. Không cộng lại các sự kiện này khi chỉ phục hồi save.
- Ngày reset mặc định là 00:00 UTC+7, đổi được trong database. Đồng hồ và tiến độ dùng cùng ranh giới ngày; ngày lùi không được mở lại thưởng đã nhận. Chưa có thời gian máy chủ để chống người dùng chỉnh đồng hồ tiến lên.
- Mốc rương mở theo số nhiệm vụ hoàn thành, không đợi nhận thưởng từng nhiệm vụ. Cả thưởng nhiệm vụ và thưởng rương đều nhận một lần trong ngày. Reward được manager đọc từ database, kiểm tra ví trước khi đánh dấu đã nhận.
- GO của nhiệm vụ chiến đấu/nhặt item đưa về trang stage; GO nâng hero mở Armory với tab Heroes. CLAIM chuyển thành CLAIMED và khóa nút sau khi nhận. Cấu hình và save dùng ID chuỗi ổn định; đổi ID sẽ tạo danh tính nhiệm vụ mới.
- Nền sheet, hàng nhiệm vụ và vùng phụ dùng `ui_slot` với `Image.Type.Sliced`. Placeholder art thay được trong database; cần rebuild prefab để cập nhật các reference/typography đã được tạo trong hierarchy.
- `Tools/MrX Studio/Daily Quests/Build Editable Page Prefab` dựng lại prefab. Lệnh này thay hierarchy prefab theo builder; không chạy nếu cần giữ chỉnh tay chưa đưa lại vào builder. `Run Logic Checks` kiểm tra bằng save và ví tạm, không ghi dữ liệu người chơi.

## 15. Thành tựu — bổ sung ngày 19/10/2026

- `Scripts/Achievements` chứa database, save và manager. Trang nằm trong `Scripts/UI/Main Menu/Achievements`; dữ liệu và prefab chỉnh được tại `Resources/Achievements Database.asset` và `Prefabs/UI/Menu Pages/Achievements Page.prefab`. Giữ tên/đường dẫn Resources nếu chưa sửa nơi tải.
- `GameController` tạo manager xuyên scene; đối chiếu tiến trình cũ sau khi các manager hoàn tất Start. Trang và tab được đặt trực tiếp bằng prefab trong Main Menu scene; enum `MainMenuPageType.Achievements = 4`. Không còn tự cài trang/tab bằng code runtime.
- Có 48 định nghĩa mặc định chia đều Adventure, Battle, Collection, Upgrade. Tiến trình gồm tổng/loại stage hoàn thành, phòng hoàn thành, quái/boss hạ, item nhặt/loại sở hữu, rương mở, level hero/item và permanent upgrade. Nội dung dùng cơ chế có thật trong source; phần thưởng hiện là gold lâu dài.
- Save riêng khóa `Achievements`, bộ đếm `long` tăng đến giới hạn an toàn, ID chuỗi ổn định cho định nghĩa/stage/item, cờ nhận thưởng lâu dài. Không reset theo ngày. Không đổi ID để đổi tên hiển thị; không đổi giá trị số enum đã lưu. Điều chỉnh mục tiêu/reward phải cân nhắc các cờ thưởng đã nhận từ cấu hình cũ.
- Chỉ nhận thưởng khi đủ mục tiêu, ID tồn tại, ví hợp lệ và số dư không tràn. Đánh dấu trước callback ví, chặn lời gọi lồng nhau và lưu sau khi cộng vàng. Nhận lại không cấp thêm thưởng; mức hoàn thành đếm mục tiêu đạt được, không phụ thuộc đã nhận thưởng hay chưa.
- Hook đặt ở hoàn thành stage, callback defeat được room theo dõi, nhặt item, mở khóa item, setter level hero/item, mua permanent upgrade và animation mở rương. Rương reset cờ chống lặp mỗi Spawn. `ContinuePlayingSave.achievementRoomWatermark` tránh cộng phòng lần nữa khi resume tại phòng đã hoàn thành; reset khi Disable run. Các lần hạ quái/mở rương thực tế trong phần wave phải chơi lại được tính là hành động mới.
- Save cũ chỉ được bổ sung mức tối thiểu từ stage đã hoàn thành, item sở hữu và level nâng cấp còn trong save. Đối chiếu nhiều lần không cộng thêm, không giảm tổng đã ghi. Không suy đoán số quái, phòng, rương hay item drop trong lịch sử chưa được lưu.
- Manager cập nhật theo sự kiện, cache chỉ số theo metric/category và tra ID bằng dictionary/hash set. Gộp ghi tiến trình tối đa một lần mỗi hai giây khi đang phát sinh sự kiện, nhận thưởng lưu ngay; manager tự tắt khi không còn ghi chờ. Dữ liệu vẫn tham gia cơ chế lưu chung khi mất focus/thoát ứng dụng.
- Trang dùng pool bằng số hàng của nhóm lớn nhất (mặc định 12) thay vì tạo 48 hàng. Chỉ cập nhật nội dung khi dirty; không tạo lại UI khi đổi nhóm. Nhóm mới mở xếp thưởng nhận được trước, chưa đạt tiếp theo, đã nhận cuối cùng. Giữ nguyên vị trí hàng trong lúc nhận thưởng để nhấp đúp không vô tình nhận hàng khác. Bàn phím/gamepad có điều hướng rõ ràng, tự cuộn tới hàng nhận thưởng được chọn.
- Sheet, hàng, ô nhóm và nền phụ dùng `ui_slot` Sliced với material dùng chung `Resources/Achievements Slot.mat` (shader UI/Default, bù sắc ấm của sprite gốc để đạt tông trắng/xanh). Hình minh họa/icon là tài nguyên sẵn có có thể thay trong database hoặc prefab. `Tools/MrX Studio/Achievements/Create Database If Missing` chỉ tạo khi chưa tồn tại. `Build Editable Page Prefab` thay hierarchy theo builder, vì vậy cần đưa chỉnh sửa tay quan trọng trở lại builder trước khi dựng lại. Thay số định nghĩa, category hoặc reference art cần dựng lại prefab tương ứng.
- `Tools/MrX Studio/Achievements/Run Logic Checks` và `AchievementsVerification.VerifyLiveButtons()` dùng save/ví tạm; không xóa hoặc cấp vàng vào dữ liệu người chơi để kiểm thử. Xác minh runtime và build player là hai mức kiểm tra riêng.

## 16. Nâng level và ghép trang bị — cập nhật ngày 19/10/2026

- Trang `UI/Main Menu/Forge/ForgePageBehavior` đăng ký qua ô búa `ui_4` trên dock, enum mới `MainMenuPageType.Forge = 5`. `Resources/Forge Page Database.asset` giữ reference art/Armory và `Resources/Forge Page.prefab` là giao diện chỉnh được. Menu tự cài trang khi chạy, không cần lưu lại Main Menu scene.
- Nâng level vẫn nằm ở popup Stage/Armory, dùng nguyên `ItemData.ItemLevels`: giá, stat, rarity và ability chỉnh trực tiếp trong Inspector. `Armory/EquipmentForge.cs` xử lý mua level qua `ArmoryManager.Forge`; tên API được giữ để tương thích nhưng không còn là cơ chế của trang Forge. `IncreaseItemLevel` vẫn là đường grant level khi nhặt đồ trùng. Không chuyển mốc level vào công thức rèn.
- `TryUpgrade(id, expectedLevel)` kiểm tra sở hữu, index save, MAX, giá và ví lâu dài, từ chối yêu cầu cũ/lồng nhau. `CurrencySave.TryWithdrawAndCommit` commit số dư/level nhất quán rồi phát sự kiện, lưu ngay sau mua; popup có chặn nhấp liên tiếp bằng unscaled time. Save level cũ và equip state được giữ.
- Trang Forge dùng `Scripts/Crafting/EquipmentCrafting` để mở khóa trang bị chưa sở hữu ở index level 0 (Lv.1). Mỗi ItemData có phần `Crafting` độc lập trong Inspector: bật/tắt, số mảnh riêng theo ID của chính món đó (mặc định 30), bốn material costs (mặc định 10/5/3/2). Đã khởi tạo công thức cho 14 item đang đăng ký, chỉ thêm field crafting, không sửa ItemLevels/GUID/prefab. Món đã sở hữu hiển thị OWNED và không thể rèn bản trùng hoặc tăng level qua Forge.
- Save `Crafting Inventory` lưu kho lâu dài, key `fragment:<ItemData.Id>` và `material:iron/crystal/scroll/leather`, bộ đếm long chống tràn. Không đổi ID hoặc enum đã serialize. Ghép kiểm tra đầy đủ mọi điều kiện trước khi trừ, commit cả chi phí/sở hữu trước callback, không tự equip. Save ngay sau rèn, chống lặp/lồng nhau; item mới vẫn đi qua UnlockItem để gắn listener và ghi thành tựu sở hữu.
- `ArmoryManager` tạo CraftingManager sau InitItems/InitHeroes. Manager nhận sự kiện nhặt đồ/ghép, gộp ghi kho tối đa một lần mỗi hai giây khi có tiến độ, tự tắt khi idle. Kho tham gia cơ chế save chung khi mất focus/thoát. Nguyên liệu đã nhặt được giữ khi thua/bỏ run; loot chưa nhặt không được cấp. Không chuyển chúng lần nữa qua GrantPickedUpItems hoặc ví gameplay.
- `Resources/Crafting Database.asset` cấu hình bốn material/icon và profile loot quái thường, boss, rương. Mặc định thử nghiệm: quái thường 12% rơi một đơn vị, boss/rương 100% rơi 2–4; mỗi lần rơi là một stack. Đây là cấu hình ban đầu, chưa phải cân bằng cuối. Fragment target chỉ chọn món chưa sở hữu có công thức hợp lệ; hết target thì rơi nguyên liệu. Đồ nguyên chiếc và phần thưởng ability của rương hiện có vẫn được giữ.
- Hook quái nằm trong nhánh aliveEnemies.Remove thành công của RoomBehavior và tôn trọng UseDrop. Hook rương nằm trong animation mở hoàn tất, dùng cờ chống lặp/reset mỗi Spawn. DropType.CraftingResource = 5 được nối cuối enum, prefab `Resources/Crafting Drop.prefab` dùng pool riêng trong DropManager. Gọi DropCrafting với payload, không cấu hình loại này vào EnemyData.Drops/gọi generic Drop. Payload/cờ pickup/callback spawn trễ được dọn khi tắt object; HideAllDrop đã đổi sang duyệt ngược để dọn đủ.
- Forge hiển thị công thức gồm cả món chưa sở hữu, mảnh X/30, bốn ô vật liệu, stat đầu ra từ ItemLevels[0] và trạng thái FORGE/OWNED. Không dùng giá vàng hoặc preview level kế tiếp ở trang này. Weapon hiển thị ATK/CRIT/ATK Speed theo dữ liệu/default thực tế; không tạo chỉ số Power giả. UI uGUI/TMP dùng ui_slot Sliced, material pastel dùng chung, bảy hàng stat tái sử dụng, dirty/event updates, tắt component khi ẩn, điều hướng gamepad và khóa dock cho Settings.
- `Tools/MrX Studio/Forge/Install Crafting Assets` chỉ tạo cấu hình/prefab còn thiếu. `Enable Default Recipes For Registered Items` là thao tác authoring tường minh để bật công thức mặc định cho các item chưa bật; không chạy tự động khi vào game. `Build Editable Page Prefab` thay hierarchy theo builder: đưa chỉnh tay cần giữ trở lại builder trước khi chạy. Đổi icon/hình đe trong database cần dựng lại prefab.
- Kiểm tra: `Run Crafting Checks` cho công thức/kho/ghép; `Run Level Upgrade Checks` cho đường nâng level cũ. `CraftingVerification.VerifyLiveButtons/VerifyLiveLoot` dùng save, kho và pool tạm để thử UI/loot/rương; không cấp tài nguyên hay xóa save người chơi. Hướng dẫn authoring và giới hạn kiểm tra ở `Docs/Crafting.md`.


## 17. UI prefab và Armory/Upgrades — cập nhật 19/10/2026

- Các mục UI tự dựng hoặc rebuild theo builder ở phần 14–16 mô tả cách triển khai trước đây. Quy trình hiện tại ưu tiên prefab chỉnh trực tiếp; xem `Docs/Menu UI Prefabs.md`. Không rebuild để cập nhật chỉnh tay.
- Năm trang và dock nằm trong `Assets/The Archer/Prefabs/UI/Menu Pages`. Scene Main Menu tham chiếu instance trực tiếp trong danh sách pages/dockItems. Không còn runtime Install/fallback dựng UI. Ba prefab chuyển từ Resources giữ nguyên GUID. Canvas asset bật để xem trong Prefab Mode; visibility runtime được MainMenuScreenBehavior thiết lập.
- Upgrades dùng UpgradeDashboardPage và PermanentUpgradeService: giữ 9 UpgradeData/ID/mốc giá; chọn hàng xem trước hiệu ứng và chỉ số sau một cấp; kiểm tra ví, giá, stale level, maximum và reentry trước khi mua, lưu ngay sau commit. Không thêm Combat Power hoặc upgrade không tồn tại trong database.
- ArmoryStatsSnapshot sử dụng overload PlayerStatsHandler.Init(heroLevel, itemLevels) dùng chung với gameplay, rồi áp 9 permanent upgrades theo đơn vị behavior hiện có. Đây là chỉ số loadout giữa các lượt, chưa áp ability/buff trong trận. Crit/dodge theo 0–100; DamageReduction là hệ số nhận damage. Không thay công thức crit damage cũ ngoài phạm vi.
- ArmorySave bổ sung sức chứa mặc định 40 và pendingEquipment; không đổi ID/key cũ. Túi tính toàn bộ ItemData đã sở hữu gồm đồ đang mặc. Mua thêm 10 ô bằng vàng, giá Inspector mặc định 100, giới hạn mặc định 200 tại Resources/Armory Dashboard Database. Save cũ quá 40 giữ nguyên đồ. Trang bị nhận khi đầy được lưu chờ, rèn chặn trước khi tiêu tài nguyên.
- Armory dùng ô inventory đã vẽ trong prefab, tái sử dụng theo vùng cuộn; hộp thoại và footer hero cũng có sẵn trong prefab. Compact stats của HeroPanel chỉ tắt riêng tại Armory để tránh hiển thị đồng thời hai bộ số khác cách tổng hợp; Stage giữ thiết lập cũ.
- Kiểm tra authoring/logic ở Editor/ArmoryUpgradeVerification; kiểm tra live dùng ví/save thử và khôi phục dịch vụ thật. Không xóa hoặc cấp tài nguyên vào save thật để kiểm tra.


## 18. Vòng quay, rương gem — bổ sung 19/10/2026

- Currency mới `gem` được nối sau `gold` trong Currencies Database, dùng manager/save/indicator cũ, không đổi gold/default currency. Gem mới mặc định 0, không tự cấp tiền để test.
- Scripts/Lucky Rewards có LuckyRewardTable, LuckyRewardsService và delivery sang currency/Armory/kho crafting. Hai bảng tại Scriptables/Lucky Rewards; giá thử nghiệm 10 gem/vòng quay và 30 gem/rương, tất cả quà/quantity/weights chỉnh Inspector. Weight chuẩn hóa theo tổng, không hardcode 100. Equipment quantity 1, đồ trùng tăng level, MAX bù gold cấu hình, đầy túi dùng pendingEquipment.
- Stage Lucky Rewards prefab đặt trực tiếp vào Stage Page. Popup, row và wheel slice đều là prefab trong Prefabs/UI/Lucky Rewards; runtime chỉ binding và instance prefab view khi tăng reward entries, không dựng component/UI. Hai ảnh đính kèm đã import Sprite. Wheel sector/icon chia đều theo số quà đang bật; weight chỉ quyết định chọn thưởng và phần trăm bên phải. Kim dừng đúng midpoint ô của prize đã chọn, không suy xác suất từ diện tích ô.
- Save `Lucky Rewards` giữ sequence/receipt ID và giá trị. Trừ gem và cấp/lưu quà trước animation. Chưa xác nhận receipt thì không mua tiếp. Đóng/thoát giữa animation, mở lại chỉ hiển thị receipt, Continue không grant lại. Không đồng nhất receipt với hàng đợi grant; quà đã được cấp. Giữ save key/ID.
- Service từ chối cấu hình/ví/quà không hợp lệ, overflow và số dư thiếu trước thanh toán; có guard reentry. Popup khóa dock, hỗ trợ Cancel và unscaled animation. Chỉ là kinh tế offline theo save hiện có, không phải giao dịch máy chủ.
- Docs/Lucky Rewards.md ghi cách chỉnh/clone và các giới hạn. Editor/LuckyRewardsVerification dùng ví/save tạm; không sửa hoặc xóa dữ liệu người chơi để kiểm tra.

## 19. Công cụ DEV và Stage preview — bổ sung 19/10/2026

- Prefabs/UI/Stage/Developer Tools.prefab chứa bảng cấp Gold/Gem và trang bị từ Armory Database, không dựng UI bằng runtime code. Scriptables/Developer Tools Settings.asset có công tắc chung, quyền dùng ở development/release và các mức tiền. Mặc định cho Editor/development, ẩn và chặn cấp ở release. Tắt Tools Enabled hoặc GameObject để bỏ công cụ khỏi màn hình.
- DeveloperGrantService kiểm tra quyền mỗi lần cấp, số lượng dương, currency gold/gem, ItemData đã đăng ký, overflow và reentry. Dùng LuckyRewardDelivery hiện có để giữ quy tắc đồ trùng/MAX/pendingEquipment; lưu ngay, không thay save key/schema hay tự cấp khi khởi động. DEV thao tác thật ghi vào hồ sơ, còn kiểm tra dùng dữ liệu riêng.
- Stage Page.prefab là variant của trang Stage cũ, giữ nested Hero Panel/Stage Panel và được scene tham chiếu trực tiếp. HeroPreviewBehavior có AspectRatioFitter tùy chọn: ảnh tĩnh đúng aspect trước Play, render texture vuông khi chạy. StageHeroIdentity cập nhật tên/cấp qua event. Ảnh mẫu từ StageDatabase[0] có sẵn trong prefab và được ẩn khi selector khởi tạo icon thật.
- Hướng dẫn tại Docs/Developer Tools and Stage Preview.md. Không chạy công cụ thiết lập ban đầu để ghi đè chỉnh sửa Prefab Mode. Các popup bật sorting override cho Canvas lồng sẵn khi mở để chắn đúng UI phía sau.
- ArmoryHeroPanel.EnsureInitialized khởi tạo pool/spacing một lần từ Start hoặc Init, để Stage.Start không phụ thuộc thứ tự Start của panel. MainMenuStageSelector cleanup chịu được trường hợp Init chưa chạy.

## 20. Thương hiệu công cụ và namespace — cập nhật 19/10/2026

- Namespace gốc hiện là `MrX`; menu phát triển và Create Asset là `MrX Studio`. Dùng các tên này khi thêm source/công cụ. Các cửa sổ Stage Creator, Abilities Tester, Pipeline Switcher và Readme có tiêu đề MrX Studio. `Tools/MrX Studio/Tools Guide` mở hướng dẫn ở Docs.
- Cập nhật đồng bộ type metadata của scene/prefab/ScriptableObject, managed references điều kiện mở Stage, reflection và custom shader editor. Input Asset.cs được sinh lại từ `.inputactions` bằng importer với namespace `MrX.Input`, không sửa tay file sinh tự động.
- Các điều kiện mở Stage giữ `MovedFrom` cho namespace gốc để nhận asset cũ. Giữ GUID/.meta, ID nội dung, enum, save keys và cấu hình định danh ứng dụng/vị trí save. Không tìm/thay chuỗi lịch sử hoặc khóa tương thích để xóa dấu vết tên cũ.
- Shader đang hoạt động dùng nhánh `MrX/...`, hai shader nhân vật được đổi tên file qua AssetDatabase và giữ GUID. Gói `.unitypackage`, credit và AssetOrigin gốc giữ làm tài liệu/nguồn nhập cũ; không import lại gói cũ chỉ để áp dụng đổi namespace. Không chạy Pipeline Switcher để thực hiện đổi tên.

## Loot boss và inventory — cập nhật 19/10/2026

Mục này thay thế mô tả profile loot và bốn nguyên liệu trước đây ở mục 16. Hướng dẫn chi tiết: [Crafting](Docs/Crafting.md). Báo cáo kiểm tra: [Verification](Docs/Verification%202026-10-19.md).

- Quái nhỏ và rương ability không còn phát item/mảnh/nguyên liệu. EnemyBehavior bỏ qua generic Item/CraftingResource; CraftingManager.DropLoot là API legacy không phát thưởng.
- Boss có UseDrop tạo một Boss Loot Chest dùng pool. Bảng BossChestRewards trên override stage ưu tiên hơn EnemyData, sau cùng fallback Crafting Database. Chỉ Equipment/Fragments; trọng số và số lượng chỉnh Inspector. Claim có receipt theo run token/phòng/wave/spawn, chặn nhận lặp và phục hồi thưởng chưa nhận ở menu.
- Vật liệu đang bật là Iron (ID 0), Leather (ID 3), Wood (ID 4 mới). ID Crystal/Scroll và số dư cũ được giữ nhưng không dùng trong cấu hình hiện tại. Công thức cũ chuyển chi phí Crystal + Scroll thành Wood; không đổi ItemLevels, ID hoặc save sở hữu.
- Lucky Wheel/Gem Chest đang cấu hình currency và ba nguyên liệu; không có item/mảnh đang bật. Mỗi mảnh theo item ID hoặc vật liệu là một stack trong Armory ALL, hiển thị số lượng và xem chi tiết khi bấm. Chỉ equipment chiếm sức chứa túi.
- Ô trống dùng ui_inventory_empty.png từ ảnh người dùng; icon trắng đầy đủ, không vũ khí mờ/text cộng đè. Forge có ba material view được lưu trong prefab. Không tạo UI runtime.
- Công cụ chuyển cấu hình: Tools/MrX Studio/Loot/Install Loot and Inventory Assets. Chỉ dùng authoring tường minh ở Edit Mode, xem diff sau khi chạy.
- Kiểm tra 19/10 đã sửa rarity increment, crit damage bị nhân dồn giữa nhiều mũi, đăng ký event hero khi hủy, cleanup modifier upgrade/Trifecta, chọn ability Health Chest khi ít lựa chọn, reset projectile pool, callback drop trễ và nhiều yêu cầu save liên tiếp. Các dòng mô tả các lỗi này ở bảng khảo sát 12.2 là lịch sử trước bản sửa; xem báo cáo xác minh. Những mục còn lại chưa mặc nhiên được xem là đã sửa.

## Kết toán phần thưởng — cập nhật 19/10/2026 sau kiểm tra run

Mục này thay thế mô tả cấp/lưu ngay khi tạo/nhặt rương boss ở phần cập nhật loot trước. Hướng dẫn và đường dẫn đầy đủ: [Run Rewards and Save](Docs/Run%20Rewards%20and%20Save.md).

- Item pool trong Stage Creator → boss → Drop Overrides là nguồn rate ưu tiên trực tiếp. Không tự chuyển trọng số hoặc chia 70/30 từ cấu hình này. Bảng BossChestRewards chỉ dự phòng khi không có pool Item cũ.
- Boss receipt tách collected (đã nhặt) và claimed (đã kết toán). Nhặt chỉ xếp thưởng và thông báo; cuối run cấp cùng tiền, gộp vật phẩm/mảnh trên màn thắng/thua và lưu một lần. Cờ claimed cũ không được grant lại.
- SaveManager gom yêu cầu ghi thường trong Game scene đến FinalizeRunRewards; checkpoint lúc mất focus/đóng app vẫn giữ để resume, chưa kết toán loot. Nhiệm vụ/thành tựu vẫn cập nhật RAM khi giết quái.
- Kết quả dùng Run Loot Reward Card.prefab; không tạo graphic runtime. Gem Wallet.prefab được đặt trong Armory/Upgrades/Forge/Daily Quests/Achievements; Stage giữ ví gem đã có.

## Ngôn ngữ menu — 20/10/2026

Stage có Language Picker prefab với tối đa năm mã en/vi/es/pt/fr, save riêng Language Settings. Bảng Menu Translations trong Resources giữ nội dung; font Latin/Vietnamese SDF đóng tĩnh từ font có sẵn. LocalizedMenuScope trên các trang/dock chỉ binding nhãn và giữ dữ liệu động, không tạo UI runtime. Phạm vi hiện tại là Stage và nhãn menu chung; nội dung chưa có bản dịch giữ tiếng Anh. Xem Docs/Menu Languages.md.

## Năng lượng, đổi gem và bản dịch mở rộng — 20/10/2026

- Currency `energy` được thêm sau gold/gem, không thay ID cũ. MenuEconomyService dùng ví hiện có và save `Energy Recovery` để hồi theo giờ thực UTC, kể cả offline, dừng tại trần và không tích trữ hồi khi đầy. Hồ sơ cũ được khởi tạo đầy một lần. Cấu hình tại Resources/Menu Economy Settings.
- GameController.TryStartNewRun kiểm tra stage mở khóa, số năng lượng và guard chuyển màn trước khi tăng attempts/chuyển scene/lưu. Resume và đường testing Editor trực tiếp không tính phí lần nữa. StageData.energyCost chỉnh Inspector từng stage và Stage Creator.
- Menu Economy.prefab đặt trong sáu trang; các ví cũ được ẩn có giữ reference. Nút + sau gold/energy mở bảng xác nhận dùng gem, chặn số âm/overflow/thiếu tiền, đầy năng lượng và callback reentry. Bố cục/sprite/font chỉnh prefab; runtime chỉ binding.
- Menu Translations mở rộng cho Upgrades/Achievements/Forge và các tên trang bị đang dùng. Normalize khoảng trắng xử lý mô tả nhiều dòng; template nhiều placeholder ưu tiên mẫu cụ thể, giữ số liệu động. Nguồn nhập mới Menu Translation Additions.txt không ghi đè bản dịch Inspector đã sửa.
- Xem Docs/Energy and Gem Exchange.md và Editor/MenuEconomyVerification.cs. Kiểm tra dùng save/đồng hồ/ví riêng; không cấp tài nguyên vào save thật.

## Menu ngang/dọc, Quest và hạn mức — cập nhật 20/10/2026

- Sáu trang và dock có ResponsiveMenuLayout lưu profile ngang/dọc trong prefab; ResponsiveMenuSurface chỉ chọn profile và fit đồng nhất vào safe area bằng UnityEngine.Device.Screen để hỗ trợ Device Simulator. Ngang 3840×2160, dọc 1080×2160; không dựng UI runtime. Preview/Capture trong Inspector hỗ trợ chỉnh trực tiếp; giữ profile khi chạy lại installer.
- UI/ScrollWheel đã nối InputSystemUIInputModule, viewport nhận raycast. Inventory 6 cột ngang/4 cột dọc, tiếp tục tái sử dụng view. Touch drag vẫn giữ.
- Menu luôn chọn Stage 1 mỗi lần tải scene. ContinuePlayingSave.stageContentId giữ stage của run và migration từ lựa chọn cũ; StageController khôi phục index gameplay để DropManager vẫn đọc đúng stage.
- Mua năng lượng mặc định 5 lần/ngày, reset 00:00 UTC+7 cấu hình Inspector; chỉ thành công mới tính lượt. Save Energy Recovery giữ purchaseDay/energyPurchases; gold không giới hạn theo quota này.
- Bảng dịch 210 mục gồm Quest hiện tại. Xem Docs/Responsive Menu and Daily Updates.md để sửa profile, cấu hình và tra đường dẫn.

## Build Windows PC — xác minh 20/10/2026

- Đã build Windows x64 Player bằng Unity 6000.6.2f1, Mono, không Development, LZ4, StrictMode và DetailedBuildReport: Succeeded, 0 errors, 0 warnings. Không suy kết quả này sang IL2CPP/mobile hoặc mọi đường gameplay.
- Giữ ba scene Main Menu → Loading Screen → Game. Hướng dẫn từng bước và vị trí bản chơi thử: Docs/Windows PC Build Guide.md; số liệu máy đọc được: Docs/Windows Build Report 2026-10-20.json.
- TMP_SDF-Mobile.shader dùng enable_debug_symbols. Hai ModelImporter enemy_cauldron_attack.fbx và enemy_mole.fbx bật Read/Write vì Particle System Shape tham chiếu mesh; không bật hàng loạt model.
- Build nằm ngoài Assets trong Builds/Windows; ZIP gửi người chơi giữ runtime/Data và loại thư mục sao lưu *_BackUpThisFolder_ButDontShipItWithYourGame. Không đổi Company/Product Name chỉ để thử build vì ảnh hưởng vị trí save.

## Badge thưởng và input — cập nhật 20/10/2026

- Menu Dock.prefab có hai instance Reward Notification Dot.prefab dưới icon Quest/Achievements. MenuRewardBadges cập nhật qua Changed, bind lại khi bật, hủy listener khi tắt. Chỉ tắt dot khi hết thưởng đủ điều kiện; Quest tính cả milestone, không xóa thông báo chỉ vì mở trang. Không tạo UI runtime hoặc save flag mới.
- Gamepad đã có binding trong Input Asset. InputManager lấy di chuyển vật lý dù thao tác UI gần nhất là chuột, nhận analog dưới ngưỡng Button Detection, fallback khi ngắt pad; joystick chuột/cảm ứng giữ nguyên. Không sửa lớp Input Asset.cs được sinh tự động để đổi binding.
- Hướng dẫn, đường dẫn và giới hạn kiểm tra tay cầm mô phỏng: Docs/Reward Notifications and Input.md. RewardBadgeInputVerification chỉ chạy Play bằng VerificationSession, không thao tác hồ sơ thật.

## Lucky Rewards miễn phí mỗi ngày — 20/10/2026

- Lucky Wheel và Gem Chest đang bật dailyFreeUse, mỗi bảng một lượt/ngày, reset mặc định 00:00 UTC+7, không cộng dồn. Bảng mới mặc định tắt tính năng; chỉnh trong LuckyRewardTable. Giữ giá gem và weight cũ.
- Lucky Rewards save giữ dailyFreeUses theo tableId/usedDay. Quota chỉ tiêu khi commit cùng receipt, không khi mở popup; reload/ack không grant lại. Receipt mới ghi dailyFreeUse/gemCost. Save cũ không bị reset.
- Stage Lucky Rewards.prefab có hai chấm đỏ dùng Reward Notification Dot.prefab. Shortcut/popup kiểm tra qua ngày mỗi giây unscaled, không ghi save định kỳ. FREE có đủ năm ngôn ngữ; icon giá gem ẩn khi miễn phí.
- Hướng dẫn: Docs/Daily Free Lucky Rewards.md. Chỉ kiểm tra compilation/Play và logic giả lập; không tự build. Từ yêu cầu này chỉ build khi người dùng yêu cầu build rõ ràng.

## Ngôn ngữ kỹ năng và Stage 4 Halloween — 20/10/2026

- Bảng Menu Translations hiện có 397 entry, bao gồm tên/mô tả của 80 kỹ năng ở năm ngôn ngữ. AbilityData giữ nguồn tiếng Anh; không đổi enum/ID để dịch. File bổ sung là `Assets/The Archer/Resources/Ability Translation Additions.txt`; importer đọc cùng file menu và giữ bản dịch Inspector đã có.
- LocalizedMenuScope được gắn sẵn vào Canvas Game, prefab selector/thẻ/rương/thông báo. Scope gần nhất sở hữu nhãn; RefreshNow áp chữ/font trước khi đo TMP; scope tắt không giữ listener. Không dựng UI runtime.
- Stage 4 có tên Halloween Crypt và dùng `Prefabs/Prop/Halloween/Halloween Crypt Arena.prefab`. Giữ ID stage, một phòng/33 wave, toàn bộ boss/loot override và spawn/camera. Material và ảnh preview riêng; không sửa prefab đền dùng chung. Props trang trí tắt collider/NavMeshObstacle, bốn tường biên vẫn chặn và sàn vẫn là Ground.
- Hướng dẫn và đường dẫn: `Docs/Run Languages and Halloween Crypt.md`. Đã kiểm tra 800 binding tên/mô tả và 105 vị trí sinh quái có đường đi; chưa chơi thắng trọn 33 wave, không build trong tác vụ này.

## Hero roster và vũ khí theo lớp — 21/10/2026

- Armory Database đăng ký Ranger cũ và năm prefab mới: Mage, Barbarian, Knight, Rogue, Rogue Hooded. Ba hero đầu hiện/mở mặc định; ba hero sau ẩn/chưa mở. HeroData có HeroClass, HideInRoster, StarterWeapon. Không xóa database entry để ẩn hoặc đổi ID để reset tiến trình.
- Có 12 WeaponData/prefab mới, tổng 15 weapon, 23 mapping hợp lệ. AllowedClasses kiểm tra equip; đổi lớp giữ món phù hợp hoặc dùng starter. Starter chỉ cấp khi thiếu và có chỗ; không nhân bản qua các lần đổi. Khiên/offhand là hình phụ của weapon, chưa thêm loại slot mới.
- Prefab mới tái sử dụng skeleton Generic Ranger, remap mesh/bones và giữ controller AC_Hero_Ranger qua override. Clip mới có event Shoot; normalize tốc độ chỉ bật cho set mới. Không thay ModelImporter hàng loạt. Material mới hỗ trợ MrX Character URP/rim.
- MeleeWeaponBehavior dùng damage/status pipeline cũ, kiểm tra góc/tầm/tường/collider trùng. RangedWeaponsOnly lọc sáu ability thuần đạn khỏi lựa chọn cận chiến; preset/attach cần cấu hình hợp lý. UI roster sáu nút đã lưu trong Armory Page.prefab.
- Hướng dẫn/công cụ/file: Docs/Adventurer Heroes and Weapons.md. Chỉ compilation/Play Mode với profile riêng; không build khi chưa được yêu cầu.

### Stage Showcase, gem và clip di chuyển (21/10/2026)

Mage/Barbarian hiện cần 500/750 `gem` để mở trên profile mới, giá ở Hero Levels[0].Price; save đã mở vẫn giữ. Barbarian gốc 220 HP/55 Damage. Stage Page đã lưu bố cục showcase landscape/portrait trong prefab và ResponsiveMenuLayout, ảnh stage là banner bo góc. Clip Idle/Walking/Running Adventurers dùng các bản sao Loop *.anim; không sửa FBX gốc. Hướng dẫn chi tiết: [Adventurer Heroes and Weapons](Docs/Adventurer%20Heroes%20and%20Weapons.md).

## Stage 5 Skeleton Survival — 21/10/2026

- Stage Data 005 giữ stageId người dùng đã tạo, một room/33 wave như Stage 2; prefab riêng `Stage 5 Bone Courtyard`, preview riêng. Mốc 8/16/24/33 lần lượt SkeletonMinion/Warrior/Rogue/Mage (enum mới 106–109, không đổi enum cũ). Group Id riêng giữ thứ tự wave.
- SkeletonBossBehavior dùng event animation và marker mesh đã lưu trong prefab: chém/quật đất, charge/vòng đạn, quạt đạn/sidestep, ba rune/vòng đạn. Dùng damage, navigation, projectile pool, status, boss healthbar và rương của hệ cũ. Tạm dừng/chết/pool reset có guard và cleanup.
- General/MovementBasic lấy từ Boss/Animation; thư mục đó hiện chưa có combat nên tái sử dụng clip Generic khớp rig từ Ranger/Animation_Pack, không sửa FBX importer. Prefab/controller/material riêng có thể sửa Inspector.
- Ba cấu hình boss spawn cũ được giữ cho ba boss đầu, Mage kế thừa mốc cuối; không đổi rate trên các stage khác. Nhặt rương và kết toán cuối run dùng BossChestService cũ.
- Chi tiết file, cấu hình, kiểm tra và giới hạn: [Stage 5 Skeleton Survival](Docs/Stage%205%20Skeleton%20Survival.md). Chỉ compilation/Play Mode, không build.

### Stage 5 — chỉnh combat và geometry lần tiếp theo

Minion/Warrior có gap closer theo khoảng cách + cooldown; Rogue bắn thường liên tục rồi mới fan/sidestep, Mage ba chưởng thường rồi xen kẽ rune/ring. Attack Cadence nằm ở controller; damage/shot vẫn do event, charge chặn contact damage trùng. Môi trường Necropolis thay nội dung prefab cùng GUID, 7.056 tam giác mesh thay 25.252; mẫu UnityStats wave 8 khoảng 41,8k thay 85,2k, không coi là FPS hoặc trung bình mọi wave. Công cụ riêng SkeletonSurvivalRefinement và hướng dẫn mới ở Docs/Stage 5 Skeleton Survival.md. Không đổi Stage 4/loot/save thật; không build.


## Bổ sung offline ngày 21/10/2026

- Công tắc gameplay tại `Assets/The Archer/Resources/Game Feature Settings.asset`, tham chiếu và chỉnh trực tiếp trong Inspector của Developer Tools Settings. Thể lực và cờ online dự phòng mặc định tắt; `toolsEnabled` chỉ kiểm soát công cụ DEV.
- Stage có enum độ khó ổn định Normal=0, Hard=1, Nightmare=2, mở tuần tự theo từng StageId. Hệ số HP/damage trong StageData, kể cả spawn override. Mức Thường giữ cân bằng trước đó.
- Thành tựu `StageDifficultyClear=11` lưu đúng StageId/mức; reward gem cấu hình trong Achievements Database, nhận một lần qua UI thành tựu. Không tái sinh ID thành tựu.
- ContinuePlayingSave giữ độ khó và RunCombatStatistics; kết quả giữ bản tham chiếu sau kết toán. Thống kê ghi nhận HP thực, không ghi save sau mỗi đòn. Bảng thống kê và bộ chọn độ khó nằm trong prefab `UI/Offline Progression`.
- Đọc `Docs/Offline Features Difficulty and Run Summary.md` để tra cấu hình, lưu/resume và giới hạn xác minh. Không bật lại hoặc triển khai backend theo các cờ dự phòng nếu người dùng chưa yêu cầu.

## Bổ sung hiệu suất ngày 22/10/2026

Đã tối ưu đường lấy mẫu NavMesh, chọn mục tiêu, ricochet/Shock, redraw HP và cập nhật UI; giới hạn FPS PC/mobile cấu hình ở GameController. Công cụ Tools → MrX Studio → Performance thu số đo Editor vào Logs/Performance. Đọc [Performance Optimization](Docs/Performance%20Optimization.md) để biết phạm vi, kết quả và giới hạn; không diễn giải số đo Editor thành bảo đảm FPS trên mọi thiết bị. Không build trong đợt này.


## Bổ sung kỹ năng và kỷ lục ngày 22/10/2026

Danh sách lựa chọn không đề xuất kỹ năng thường đã max, kể cả cờ IsRepeatedAbility; giữ phần thưởng Endgame hồi máu/vàng có tác dụng ngay. Kỷ lục offline theo stageId + độ khó lưu ở save key Run Records: thời gian thấp nhất chỉ khi clear, DPS trung bình cao nhất khi kết thúc run. Hai thẻ số chạy bằng unscaled time được author trong Run Summary.prefab. Xem [Run Records and Max Abilities](Docs/Run%20Records%20and%20Max%20Abilities.md) trước khi đổi quy tắc.


## Bổ sung UI Text Effect ngày 22/10/2026

Component UI/UITextEffect.cs kéo thả lên TextMeshProUGUI có 12 kiểu hiệu ứng, dùng lại EasingFunctions; không dựng UI runtime và không đổi RectTransform. Inspector có Preview khôi phục trước khi lưu scene/prefab; prefab mẫu ở Prefabs/UI/Text Effects. Đọc [UI Text Effects](Docs/UI%20Text%20Effects.md) về API đếm số, pause, callback và tránh hai script cùng ghi text. Bộ đếm bảng kết quả hiện hữu vẫn do RunRecordHighlights quản lý, dùng chung helper FormatTime.



## Bổ sung mùa, EXP hero và kết quả ngày 22/10/2026

Run Progression Settings.asset trong Resources quản lý mùa nguyên tố và công thức EXP; StageData có trọng số mùa/hệ số EXP, HeroLevel có ngưỡng EXP tùy chọn. EXP riêng theo hero của run, giữ level cũ, nhận khi kết toán thắng/thua cùng batch; không còn mua level hero bằng gold, mở khóa gem giữ nguyên. ContinuePlayingSave giữ mùa/hệ số/hero khi resume. Màn gọn mới ở Prefabs/UI/Run Progression hiển thị thời gian + DPS cao nhất bằng UITextEffect; bảng Run Summary chỉ mở bằng nút Thống kê, không lặp thẻ DPS. Thua có record thời gian sống dài nhất riêng với fastest clear. Source và hướng dẫn: [Seasons Hero Experience and Results](Docs/Seasons%20Hero%20Experience%20and%20Results.md). Các mô tả RunRecordHighlights phía trên là bố cục cũ. Không build.
