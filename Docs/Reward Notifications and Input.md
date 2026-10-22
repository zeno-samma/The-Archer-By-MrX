# Chấm đỏ nhận thưởng và thiết bị điều khiển — 20/10/2026

## Chấm đỏ trên dock

Icon **Nhiệm vụ ngày** và **Thành tựu** có một chấm đỏ nhỏ ở phía trên bên phải khi còn phần thưởng đủ điều kiện nhận. Không hiển thị số. Mở tab không làm mất chấm; phải nhận hết các phần thưởng đang chờ thì chấm mới tắt.

- Nhiệm vụ: gồm cả thưởng từng nhiệm vụ và rương mốc tiến độ. Nhận hết thưởng nhiệm vụ mà còn rương mốc chưa nhận thì vẫn báo đỏ.
- Thành tựu: dùng số thành tựu đã hoàn thành, chưa nhận thưởng từ AchievementsManager hiện có.
- Khi tiến độ đạt yêu cầu, nhận thưởng hoặc reset ngày, badge cập nhật qua event. Khi tải lại menu hoặc bật lại dock, trạng thái được lấy lại từ manager/save.
- Không tạo UI runtime, không thêm khóa save, không ghi save chỉ để bật/tắt chấm; không quét nhiệm vụ mỗi frame. Image chấm đỏ không nhận raycast, không cản nút bên dưới.

### Chỉnh hình thức trong Inspector

1. Mở `Assets/The Archer/Prefabs/UI/Menu Pages/Reward Notification Dot.prefab` để chỉnh màu, sprite và kích thước. Sprite đang dùng `Assets/The Archer/Sprites/UI/Generic/ui_circle.png`, màu đỏ, kích thước 34×34 theo đơn vị thiết kế.
2. Mở `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab`. Hai instance nằm ở `QUESTS Dock Item/Icon/Reward Notification Dot` và `ACHIEVEMENTS Dock Item/Icon/Reward Notification Dot`.
3. Trên object gốc dock, component `MenuRewardBadges` giữ hai reference. Không gắn component lên chính chấm đỏ vì chấm sẽ được tắt khi hết thưởng.
4. Dot là con của Icon, nên đi theo vị trí/kích thước icon khi chọn tab và khi đổi bố cục ngang/dọc. Chỉnh vị trí instance nếu muốn thay khoảng cách; giữ `Raycast Target` tắt.

## Chuột, bàn phím và tay cầm

Dự án **đã có hỗ trợ gamepad** qua New Input System (`Active Input Handling = Input System Package`). Không cần bật một plugin tay cầm riêng. Input Asset hiện có binding cho hai cần analog, WASD, phím mũi tên và điều hướng menu bằng gamepad.

Bản cập nhật sửa chuyển thiết bị ở `InputManager`:

- PC mặc định dùng bàn phím khi không có gamepad; mobile vẫn giữ joystick màn hình.
- Cần analog nhẹ sau khi dùng chuột vẫn nhận di chuyển, không cần đẩy quá ngưỡng của một nút bấm.
- Click chuột khi đang giữ WASD không làm mất input di chuyển.
- Kéo joystick bằng chuột/cảm ứng vẫn hoạt động; thả nút/cần/joystick trả chuyển động về 0.
- Ngắt tay cầm duy nhất chuyển về thiết bị phù hợp; dùng lại tay cầm sẽ chuyển lại chế độ gamepad.
- Input action được hủy đăng ký/dispose khi InputManager bị hủy. ID enum và cấu trúc save Input giữ nguyên.

Di chuyển bằng WASD/phím mũi tên hoặc analog. Chuột dùng menu và joystick kéo như trước; không thêm cơ chế click để nhân vật tự tìm đường. Menu dùng nút South (thường A/Cross) để xác nhận, East (thường B/Circle) để quay lại theo binding hiện có. Đây là tên vị trí nút trong Input System; ký hiệu in trên tay cầm tùy thiết bị.

Thêm/sửa binding trong `Assets/The Archer/Scriptables/Input/Input Asset.inputactions`, rồi để importer sinh lại lớp C#. Không chỉnh tay `Scripts/Toolkit/Input/Input Asset.cs`. Bản cập nhật này giữ nguyên inputactions và file C# được sinh.

## File logic

| Chức năng | Đường dẫn từ gốc repository |
| --- | --- |
| Binding hai badge và event lifecycle | `Assets/The Archer/Scripts/UI/Main Menu/MenuRewardBadges.cs` |
| Kiểm tra thưởng nhiệm vụ/rương mốc còn lại | `Assets/The Archer/Scripts/Quests/DailyQuestsManager.cs` → `HasClaimableRewards` |
| Cache thưởng thành tựu | `Assets/The Archer/Scripts/Achievements/AchievementsManager.cs` → `ClaimableCount` |
| Chuyển thiết bị và lấy di chuyển | `Assets/The Archer/Scripts/Toolkit/Input/InputManager.cs` |
| Joystick màn hình hiện có | `Assets/The Archer/Scripts/Toolkit/Input/JoystickBehavior.cs` |
| Kiểm tra bằng save và input mô phỏng | `Assets/The Archer/Scripts/Editor/RewardBadgeInputVerification.cs` |

## Kiểm tra đã thực hiện

- 14 đối chiếu badge trong Play: trạng thái ban đầu, chỉ còn rương mốc, nhận hết, hoàn thành mới, thành tựu, reset ngày, bật/tắt component và tránh trùng listener.
- 12 kiểm tra input với Gamepad/Keyboard/Mouse mô phỏng của Input System và joystick gameplay thực: analog nhẹ, hai cần, WASD, mũi tên, giữ phím khi click, thả input, joystick và ngắt tay cầm.
- Chạy scene Game: player đã đổi vị trí khi giữ analog nhẹ; menu Submit/Cancel mở/đóng bảng mua và khóa/khôi phục dock đúng, không mua tài nguyên.
- Đã xem chấm đỏ ở ngang và dọc, kiểm tra khôi phục từ save sau tải lại scene và dừng/mở Play lại. Bộ DailyQuestsVerification cũ cũng đạt 22 điều kiện.
- Dùng VerificationSession với hồ sơ riêng; hash save thật trước/sau không đổi. Lượt Play cuối: Console 0 lỗi, 0 cảnh báo.
- Trong một lần khởi động lại Play nhanh, Unity Editor ghi assertion `Unity.PlayMode.Editor.DefaultScenario.StateSyncedSanityCheck` (Idle/Starting). Lượt kiểm tra cuối khi chờ Editor về trạng thái idle không tái hiện; không sửa package nội bộ Unity để che log này.

Máy kiểm tra chỉ có Keyboard/Mouse thật; gamepad được mô phỏng qua Input System, chưa thử tay cầm USB/Bluetooth thực hoặc mọi driver. Không khẳng định mọi tay cầm đều tương thích chỉ từ kết quả mô phỏng. Khi thử thiết bị thật, kiểm tra analog, xác nhận/quay lại, rút/cắm lại và chuyển xen kẽ với chuột/bàn phím.

## Bản PC cập nhật

Bản sau khi thêm tính năng nằm ở `Builds/Windows/PC-Playtest-20261020-02/The Archer By MrX.exe`; tách khỏi bản `01` trước đó. Hướng dẫn quy trình build: [Windows PC Build Guide](Windows%20PC%20Build%20Guide.md).

Build Windows x64/Mono không Development, LZ4 + StrictMode đã thành công lúc 16:06 ngày 20/10/2026 (giờ Việt Nam): **0 errors, 0 warnings**, khoảng 58 giây. Báo cáo: [Windows Build Report — Rewards and Input](Windows%20Build%20Report%202026-10-20%20-%20Rewards%20and%20Input.json). ZIP bản cập nhật: `Builds/Windows/The-Archer-By-MrX-PC-Playtest-20261020-02.zip`. Chưa chạy bản player cập nhật với tay cầm vật lý; kiểm tra di chuyển/menu được thực hiện trong Editor bằng Input System mô phỏng.
