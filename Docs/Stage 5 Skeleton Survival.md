# Stage 5 — Skeleton Survival

Cập nhật 21/10/2026. Các đường dẫn bên dưới tính từ gốc dự án. Nội dung được dựng và chạy thử qua Unity MCP, không build.

## Cấu trúc và nhịp chơi

Stage 5 giữ nguyên `stageId` của asset người dùng đã tạo, một phòng và 33 wave theo cấu trúc Stage 2. Môi trường riêng là sân hầm mộ: tái sử dụng nền, tường, confinement và navigation của Temple, thay phần trang trí bằng crypt, bia mộ, quan tài và cổng Halloween. Vùng chơi giữa sân được để thoáng; vật trang trí nằm ngoài biên chiến đấu.

| Wave hiển thị | Boss | HP / Damage gốc | Cơ chế |
| --- | --- | --- | --- |
| 8 | Skeleton Minion | 1800 / 12 | Chém hình quạt; luân phiên quật đất hình tròn |
| 16 | Skeleton Warrior | 3000 / 16 | Khóa hướng, báo hành lang rồi lao; luân phiên đập đất và bắn vòng 12 đạn |
| 24 | Skeleton Rogue | 4200 / 18 | Bắn quạt 5/7 đạn, khóa hướng trước khi bắn, di chuyển ngang sau đòn |
| 33 | Skeleton Mage | 5500 / 21 | Ba vùng nổ khóa vị trí; luân phiên vòng 14 đạn |

HP/Damage thực tế còn nhân hệ số wave của StageData và OverrideData nếu bật. Mặc định, HP ở wave boss tương ứng là khoảng 2430 / 5250 / 9030 / 14300. Đây là cấu hình khởi đầu để cân bằng bằng Inspector, chưa phải xác nhận độ khó cuối cùng.

Mỗi wave có Group Id riêng để giữ thứ tự boss. Không sửa ID enum cũ: các boss mới dùng 106–109, theo Minion/Warrior/Rogue/Mage. Các stage khác không bị thay boss hoặc loot.

## File cần mở

| Mục | Đường dẫn |
| --- | --- |
| Stage 5 | `Assets/The Archer/Scriptables/Stages/Stage Data 005.asset` |
| Danh sách stage | `StageDatabase` hiện hữu đang được menu tham chiếu; Stage 5 đã đăng ký |
| Database enemy | `Assets/The Archer/Scriptables/Enemies Database.asset` |
| Bốn prefab boss | `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/` |
| Môi trường riêng | `Assets/The Archer/Prefabs/Prop/Halloween/Stage 5 Bone Courtyard.prefab` |
| Ảnh preview | `Assets/The Archer/Sprites/UI/Main Menu/ui_stage_5_preview.png` |
| Model và vũ khí gốc | `Assets/The Archer/Models/Heroes/Boss/` và `Weapons/` |
| Animation gốc mới | `Assets/The Archer/Models/Heroes/Boss/Animation/` |
| Controller, clip và mesh cảnh báo | `Assets/The Archer/Animations/Enemies/Skeletons/` |
| Material riêng | `Assets/The Archer/Materials/Enemies/Skeletons/` |
| Logic boss | `Assets/The Archer/Scripts/Enemy/Bosses/Skeletons/SkeletonBossBehavior.cs` |
| ID enemy | `Assets/The Archer/Scripts/Enemy/EnemyType.cs` |
| Công cụ authoring | `Assets/The Archer/Scripts/Editor/SkeletonSurvivalAuthoring.cs` |

## Chỉnh boss trong Inspector

1. Mở prefab boss tương ứng. Component `SkeletonBossBehavior` giữ base Health/Damage và các cấu hình đặc biệt.
2. `Recovery`: nghỉ sau đòn; `Warning Duration`: thời gian báo trước khi bắt đầu animation tấn công. Damage phát đúng sự kiện giữa clip, nên tổng thời gian phản ứng còn có phần đầu animation.
3. `Attack Range`: khoảng cách tiếp cận và tầm chém Minion; `Ground Radius`: bán kính vòng đập đất/vùng nổ Mage; `Cleave Angle`: góc kiểm tra chém Minion.
4. `Special Damage Multiplier`: hệ số damage của đòn luân phiên; mặc định 1.5.
5. Warrior: `Charge Distance`, `Charge Duration`, `Charge Half Width`. Đích charge được chặn theo NavMesh; chỉ trúng một lần trong một lượt lao.
6. Rogue/Mage: `Projectile Count`, `Fan Angle`, `Projectile Prefab`, `Projectile Origin`. Rogue thêm hai đạn vào lượt đặc biệt; Mage dùng số lượng này cho vòng đạn. Warrior hiện dùng vòng 12 đạn cố định trong logic.
7. Giữ reference `Circle Marker`, `Cone Marker`, `Line Marker`, ba `Rune Markers`. Đây là mesh/view đã lưu dưới `Attack Telegraphs` trong prefab; runtime chỉ bật/tắt, đặt vị trí và scale, không tạo UI.
8. Nếu đổi góc chém/quạt, chỉnh đồng thời mesh cảnh báo tương ứng để vùng hiển thị khớp damage. Các mesh mặc định nằm trong thư mục animation nêu trên.

Vũ khí được lưu dưới hai bone `handslot.r` / `handslot.l` của model trong prefab. Minion dùng blade + khiên nhỏ, Warrior dùng axe + khiên lớn, Rogue dùng crossbow, Mage dùng staff. Chỉnh transform model vũ khí trong prefab nếu đổi model; giữ các bone và component sự kiện animation.

## Animation

Rig mới khớp đường dẫn Generic `Rig_Medium`. Idle, Run, Spawn và Death được sao chép từ hai FBX mới trong thư mục Boss/Animation. Hai FBX này chưa có clip combat, nên đòn chém/bắn/phép tái sử dụng `CombatMelee.fbx` / `CombatRanged.fbx` đang có trong Ranger/Animation_Pack. FBX gốc không bị sửa importer.

Clip Idle/Run bật loop; Attack không loop. Mỗi Attack có một `OnAttack` và một `OnAttackEnded`; Death có `OnDefeatAnimationEnded`. Logic chỉ nhận một impact khi đòn đang chờ; không gây damage nếu pause, boss đã chết hoặc callback bị gọi lại. Timeout chỉ thoát đòn lỗi cấu hình, không tự phát damage không có animation.

Nếu tự thay clip, giữ event và đường dẫn bone hợp lệ. Tốc độ freeze ảnh hưởng cả animation, thời gian cảnh báo và tiến độ hành vi. Khi chết/tắt object, coroutine và marker được dọn; projectile dùng pool/lifetime cũ.

## Mốc boss và rương thưởng

Mở Stage Creator hoặc StageData → Rooms[0] → Waves. Chỉ số mảng nội bộ là 7, 15, 23, 32; trên UI là wave 8, 16, 24, 33.

Công cụ lần đầu đã sao chép toàn bộ EnemySpawnData của ba mốc boss cũ (11/22/33) sang ba boss đầu ở mốc mới, rồi đổi EnemyType/vị trí. Mage kế thừa cấu hình mốc boss cuối cũ. Các Drop Overrides và bảng rương trong spawn được giữ; không cân bằng lại trọng số cũ. Sau tích hợp, chỉnh loot riêng từng boss tại mốc mới bằng Inspector/Stage Creator như trước.

Boss chết đi qua `EnemyBehavior.Drop` và `BossChestService` cũ: một rương, nhặt vào phần thưởng của run, kết toán khi thắng/chết. Không thêm drop nguyên liệu/item vào quái thường. Boss được triệu hồi bằng `SpawnAdditionalEnemy` trong test không có loot (`useDrop=false`), không dùng các đợt test đó để kiểm chứng rate thật.

## Công cụ và lưu ý dữ liệu

`Tools → MrX Studio → Stage → Install Stage 5 Skeleton Survival` chỉ chạy ở Edit Mode. Tạo các asset còn thiếu, đăng ký enemy và Stage 5; lần đầu thay mốc boss/khóa thứ tự wave. Khi chạy lại, giữ prefab/controller đã có và không viết lại mốc đã mang ID skeleton. Tuy nhiên, lệnh vẫn gán lại prefab môi trường mặc định của room; sau khi tự thêm môi trường vào room thì không cần chạy lại installer.

StageId/GUID cũ được giữ. Với một run Stage 5 bắt đầu trước lần chỉnh wave này, nên hoàn tất/bỏ run cũ rồi bắt đầu run mới để dùng thứ tự shuffle mới; không xóa save thật như một bước cập nhật mặc định.

Tên boss và tên Stage 5 được thêm vào nguồn dịch năm ngôn ngữ `Assets/The Archer/Resources/Menu Translation Additions.txt` và đã import bằng công cụ hiện có.

## Xác minh đã thực hiện

- Compilation thành công; kiểm tra đủ reference và binding animation trên cả bốn boss.
- Stage 5: một room, 33 wave, đúng bốn mốc; 106 điểm spawn đều sample được trên NavMesh trong run Stage 5.
- Minion thực hiện hơn 30 lượt đánh; Warrior/Rogue/Mage thực hiện nhiều chu kỳ thường/đặc biệt; kiểm tra lấy lại Minion từ pool sau khi chết.
- Cả bốn boss chặn callback tấn công khi pause và dọn marker khi chết.
- Kết liễu hai lần chỉ tạo một rương. Hạ Mage thật ở wave 33 dẫn tới Victory, hiển thị gold + mảnh ghép; receipt `collected=true, claimed=true`.
- Không ghi nhận error/warning gameplay trong lượt Play cuối; plugin MCP có thể phát warning WebSocket sau domain reload, trong khi kết nối HTTP vẫn hoạt động.
- Profile test riêng; kiểm tra hash save thật không đổi sau mỗi phiên. Editor đã được trả về Edit Mode. Không chạy build.

Chưa phải đánh giá cân bằng đầy đủ bằng cách chơi cả 33 wave với mọi hero, hoặc đo hiệu năng trên mọi cấu hình/mobile. Cần playtest thực tế để chốt HP, damage và recovery cho phát hành.

## Chỉnh nhịp đánh và tối ưu môi trường — cập nhật tiếp 21/10/2026

Phần này thay thế mô tả nhịp đánh và bố cục cũ phía trên.

### Áp sát và bắn thường

- Minion và Warrior dùng gap closer khi người chơi ở xa và đã hết hồi chiêu. Khóa điểm gần người chơi, báo hành lang khoảng 0,48 giây rồi lao tới; Minion lao trong 0,32 giây, Warrior 0,42 giây. Đích bị chặn bởi NavMesh. Khi ở gần, hai boss vẫn chém/đập đất như trước. Mỗi lần lao chỉ gây một hit; va chạm tiếp xúc của cùng lượt lao bị chặn để không cộng damage lần thứ hai.
- Rogue bắn nỏ từng mũi liên tục, sau **6 phát thường** mới bắn quạt đặc biệt và đổi vị trí. Không sidestep/nghỉ dài sau từng mũi. Projectile riêng có model Skeleton_Arrow; giữ collision và pool của SimpleProjectile.
- Mage bắn **3 chưởng thường** rồi dùng một kỹ năng: lần lượt ba rune và vòng đạn. Chưởng có projectile tím/lõi sáng riêng, dùng tổng 16 tam giác cho phần mesh, không thêm các hệ particle nặng vào từng viên. Animation bắn nhanh riêng; animation phép dài dùng cho skill.
- Mọi phát đạn vẫn lấy thời điểm từ animation event. Chỉ tăng nhịp playback qua parameter `Attack Cadence`, không phát đạn ở Update. Freeze vẫn nhân tốc độ Animator và clock hành vi.

Trong prefab boss, nhóm **Ranged cadence and gap closer** có:

| Field | Tác dụng |
| --- | --- |
| Shots Before Skill | Số phát thường trước khi dùng kỹ năng |
| Shot Recovery / Shot Warning | Nhịp nghỉ và chuẩn bị của phát thường |
| Attack Playback Speed | Tốc độ clip khi bắn thường hoặc chuẩn bị lao |
| Gap Close Min Distance | Khoảng cách tối thiểu để lao áp sát |
| Gap Close Cooldown | Hồi chiêu áp sát |
| Gap Close Warning | Thời gian cảnh báo hành lang |
| Charge Distance / Duration / Half Width | Giới hạn quãng lao, thời gian lao và bề rộng trúng đòn |

Projectile mới:

- `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Crossbow Bolt.prefab`
- `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Arcane Bolt.prefab`
- Mesh chưởng: `Assets/The Archer/Animations/Enemies/Skeletons/Arcane Octahedron.asset`
- Clip bắn nhanh Mage: `Assets/The Archer/Animations/Enemies/Skeletons/Skeleton Mage Bolt Attack.anim`

### Môi trường Necropolis thay thế

Prefab/GUID môi trường cũ vẫn giữ để StageData không mất reference, nhưng nội dung nay chia nhóm `Necropolis Architecture`: cổng vòm, nhà mộ phía bắc, colonnade, các gian mộ có bệ và vòm hai bên, đèn, nền lát đá và cây chết ngoài biên. Đã bỏ hàng 10 quan tài mở lặp lại, nền cỏ sáng và mây che cảnh. Khu trung tâm, collider/NavMesh và 33 wave/loot không bị đổi.

Trang trí không có collider/NavMeshObstacle bật; phần này tắt đổ bóng để giảm lượt render phụ. Chỉ hai point light nhấn màu, không đổ bóng. Model nguyên bản không bị giảm chất lượng hàng loạt; thay bằng prefab/mesh nhẹ và hình học đơn giản tại Stage 5. Stage 4 giữ nguyên.

| Chỉ số | Trước | Sau cuối |
| --- | ---: | ---: |
| Tam giác mesh môi trường đang bật, đếm một lượt | 25.252 | 7.056 |
| Mẫu UnityStats trong run Stage 5 wave 8, một boss + Ranger | 85.154 | 41.834 |
| Shadow casters ở hai mẫu trên | 49 | 24 |

Tam giác môi trường giảm khoảng 72%; hai mẫu toàn cảnh giảm khoảng 51%. UnityStats gồm rendering thực tế, nhân vật, hiệu ứng và các pass; số mesh một lượt không phải tổng tam giác GPU. Các mẫu là snapshot tại cấu hình kiểm tra, không phải cam kết FPS hay mức trung bình cho mọi wave/máy. Khi spawn nhiều quái/projectile, số triangles thay đổi.

Ảnh preview Stage 5 đã được chụp lại. Các vật thể vẫn chỉnh trực tiếp trong prefab, không dựng môi trường hoặc UI bằng code runtime.

### Công cụ mới và kiểm tra

Source authoring: `Assets/The Archer/Scripts/Editor/SkeletonSurvivalRefinement.cs`.

- **Tools → MrX Studio → Stage → Apply Skeleton Boss Combat Revision**: đặt lại các giá trị nhịp đánh mặc định kể trên, gán projectile mới và cấu hình controller. Chỉ Edit Mode; có ghi đè các giá trị này, không cần chạy lại sau khi đã cân bằng riêng trong Inspector.
- **Tools → MrX Studio → Stage → Rebuild Stage 5 Optimized Necropolis**: dựng lại nhóm Necropolis Architecture theo bố cục mặc định; ghi đè nhóm trang trí, không sửa wave/loot. Không chạy nếu muốn giữ chỉnh sửa tay của nhóm này.

Kiểm tra bằng profile riêng: Rogue ghi nhận 107 phát thường/17 loạt đặc biệt, Mage 57 phát thường/19 kỹ năng; Minion/Warrior áp sát lần nữa sau khi người chơi lùi xa; callback khi pause bị chặn. Thử lao xuyên qua player ghi nhận một callback damage. 106 điểm spawn vẫn có NavMesh sau thay map. Console cuối không có error/warning gameplay; save thật giữ nguyên hash. Không build. Vẫn cần playtest cân bằng và profiling trên phần cứng đích.
