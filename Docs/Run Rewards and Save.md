# Phần thưởng trong run, save cuối run và tiền gem trên menu

Cập nhật 19/10/2026. Các đường dẫn dưới đây tính từ gốc dự án `The-Archer-By-MrX`.

## Người chơi nhận thưởng khi nào?

- Giết boss tạo rương. Nhặt rương hiện thông báo tên món/mảnh và số lượng trên màn chơi như hệ thống cũ.
- Trong run, vật phẩm chỉ vào danh sách **đã nhặt, chờ tổng kết**; không mở khóa trang bị hoặc cộng mảnh vào kho lâu dài ngay lúc đó.
- Khi thắng, chết hẳn hoặc chọn bỏ trận qua Pause, hệ thống gộp phần thưởng, cấp gold/trang bị/mảnh và ghi save cùng trạng thái kết thúc run. Màn kết quả hiện tiền và các món đã nhặt; cùng loại mảnh được gộp số lượng.
- Tiếp tục từ màn kết quả hoặc gọi kết thúc lặp không cấp lại thưởng. Hồi sinh chưa được xem là kết thúc run.
- Rương chưa nhặt không được tính thưởng. Túi đầy dùng hàng chờ trang bị hiện có; thẻ kết quả ghi Bag pending. Đồ trùng theo quy tắc tăng level cũ; món MAX có bồi thường vàng theo cấu hình.

## Ghi save và tiếp tục run

SaveManager gom các yêu cầu ghi thường trong màn chơi: tiến độ nhiệm vụ/thành tựu, tự lưu và pickup không ghi file riêng theo từng quái. Dữ liệu vẫn cập nhật trong bộ nhớ để tổng kết cuối run.

Cơ chế checkpoint khi ứng dụng mất focus trên thiết bị hoặc đóng ứng dụng được giữ nhằm tiếp tục run: checkpoint lưu trạng thái chờ, không biến loot thành đồ đã nhận. Nạp lại menu không tự trả thưởng của run chưa xong. Run được tiếp tục rồi kết thúc mới nhận. Đây khác với ghi file liên tục trong lúc chiến đấu.

Save cũ đã có cờ `claimed` từ phiên bản cấp ngay được giữ nguyên, không phát thưởng lần nữa. Không cần xóa save để dùng bản mới.

## Chỉnh tỉ lệ boss: tiếp tục dùng Stage Creator

Đường đang dùng: **Tools → MrX Studio → Stage Creator → stage → room/wave → boss → Enemy Overrides → Drop Overrides**.

- Các dòng Item ở đây được đọc trực tiếp khi boss chết. Không sao chép lại sang bảng khác và không tự chia thành 70% trang bị / 30% mảnh.
- Nhiều dòng Item dùng trọng số Chance theo HP Zone như logic boss cũ; tổng có thể khác 100. Dòng trùng cùng ItemData vẫn giữ trọng số cộng của từng dòng.
- Một dòng Item giữ điều kiện Chance của dòng đó. Chance 0 không cho thưởng; nhiều dòng không có trọng số dương cũng không có thưởng.
- Không có pool Item trong override thì dùng các dòng Item của EnemyData; chỉ khi không có pool Item cũ mới dùng BossChestRewards đã thêm trước đây cho trang bị/mảnh.
- Khi dùng BossChestRewards dự phòng: ưu tiên bảng trên override, rồi bảng EnemyData, cuối cùng Crafting Database. Không chạy lại công cụ chuyển loot chỉ để đổi tỉ lệ.

Không thay đổi các asset stage/rate người dùng đã cấu hình trong tác vụ này.

## Bản đồ file

| Chức năng | File chính |
| --- | --- |
| Trì hoãn ghi khi chơi, ghi tổng kết | `Assets/The Archer/Scripts/Toolkit/Save/SaveManager.cs` |
| Thắng/chết, kết toán một lần | `Assets/The Archer/Scripts/Stage/StageController.cs` → `FinalizeRunRewards()` |
| Thu tiền, về menu sau kết quả | `Assets/The Archer/Scripts/Other/GameController.cs` |
| Chọn thưởng theo pool từng boss | `Assets/The Archer/Scripts/Enemy/EnemyBehavior.cs` → `Drop()` |
| HP Zone, Chance trong stage | `Assets/The Archer/Scripts/Enemy/Data/EnemyDropData.cs` |
| Receipt: đã chọn / đã nhặt / đã cấp | `Assets/The Archer/Scripts/Lucky Rewards/BossChestService.cs` |
| Pickup vật lý, chặn callback lặp | `Assets/The Archer/Scripts/Drop/Behaviors/BossChestDropBehavior.cs` |
| Thông báo, gộp danh sách kết quả | `Assets/The Archer/Scripts/Drop/DropManager.cs` |
| Thông báo nhặt tên/icon/số lượng | `Assets/The Archer/Scripts/UI/Text Indicators/ItemTextIndicatorBehavior.cs` |
| Màn thắng / thua | `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs`, `StageFailedScreen.cs` |
| Thẻ item/mảnh/vàng bồi thường | `Assets/The Archer/Scripts/UI/SmallItemRewardCard.cs` |
| Gắn ví gem vào giao diện menu | `Assets/The Archer/Scripts/UI/Main Menu/MenuWalletView.cs` |
| Dựng prefab trong Editor | `Assets/The Archer/Scripts/Editor/RunSettlementAuthoring.cs` → `Install()` |
| Kiểm tra mới | `Assets/The Archer/Scripts/Editor/RunSettlementVerification.cs` |

## Sửa giao diện trong Prefab Mode

| Prefab | Sửa gì |
| --- | --- |
| `Assets/The Archer/Prefabs/UI/Game/Run Loot Reward Card.prefab` | Icon, tên, số lượng, viền và nhãn loại thưởng |
| `Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab` | Bố cục màn thắng, rewardsGrid, nút Continue |
| `Assets/The Archer/Prefabs/UI/Game/Stage Failed.prefab` | Bố cục màn thua, rewardsGrid, nút Continue |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Gem Wallet.prefab` | Nền, icon, số gem; dùng chung cho năm tab thêm mới |
| `Assets/The Archer/Prefabs/UI/Menu Pages/* Page.prefab` | Vị trí instance Gem Wallet trên Armory, Upgrades, Forge, Daily Quests, Achievements |
| `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` | Ví gem Stage đã có từ trước |

Các thẻ thưởng dùng instance từ prefab. Mọi graphic, text và bố cục đều có sẵn trong prefab; không dựng cây UI bằng code runtime.

Hàm `RunSettlementAuthoring.Install()` là authoring tường minh ở Edit Mode, có ghi prefab. Sửa giao diện hằng ngày trong Prefab Mode; không chạy lại installer nếu cần giữ chỉnh tay. Bố cục dựng một lần qua Unity MCP và lưu vào asset.

## Cách kiểm tra lại

Dùng `VerificationSession.Begin()` ở Edit Mode để tạo hồ sơ thử riêng trước Play; dừng Play rồi `VerificationSession.End()` để đối chiếu hash save thật.

- `RunSettlementVerification.Verify()` kiểm tra chọn thưởng, rate cũ, pickup không ghi/không cấp sớm, save roundtrip và kết toán một lần.
- Trong Game scene với hồ sơ thử mới: `BeginLive()` → chờ vài frame → `VerifyNoWrites()` → `FinishLive(true)` cho thắng hoặc `FinishLive(false)` cho thua.
- Kiểm tra thêm túi đầy, resume, nút Continue và ví gem khi chuyển tab. Không chạy hàm cấp tài nguyên thử trên hồ sơ thật.

Kết quả đã chạy được ghi ở cuối tài liệu; có file không đồng nghĩa mọi tổ hợp gameplay đều đã được kiểm tra.

Khi từ chối tiếp tục một checkpoint ở menu, các món đã nhặt của run đó được kết toán an toàn một lần cùng tiền theo hành vi kết thúc phiên cũ; không cấp rương chưa nhặt. Trường hợp giới hạn ví/số lượng hoặc dữ liệu không cho cấp ngay, receipt của run đã kết thúc được giữ Pending delivery để thử lại khi khởi động; không làm mất thưởng hoặc trả thưởng sớm của run chưa kết thúc.

## Kết quả đã xác minh

30 kiểm tra độc lập của kết toán run đạt; cùng các suite liên quan và gameplay regression là 230 kiểm tra đạt. Đã thử Play cho thắng, thua, kill/pickup thật, restore checkpoint, túi đầy, callback lặp và từ chối resume. Ví gem trên năm tab mới cập nhật đúng cùng ví lâu dài. Console lượt cuối không có error/warning; save thật giữ nguyên hash. Chưa kiểm tra build thiết bị/profiler. Xem chi tiết tại [Verification](Verification%202026-10-19.md).

Các bộ lọc và bố cục card/name/amount được xác minh bằng Game View; tên và số lượng có vị trí riêng, không còn đè lên nhau. Các biến thể tên dài/cân bằng giá và rate có thể tiếp tục chỉnh trong prefab/data nêu ở trên.

