# Lượt miễn phí hằng ngày — Lucky Wheel và Gem Chest

Cập nhật 20/10/2026. Chỉ sửa source/prefab và kiểm tra Editor; **không build**. Từ yêu cầu này, chỉ build khi người dùng yêu cầu rõ ràng. Các bản PC đã tạo trước đó chưa có cơ chế miễn phí mới.

## Cách hoạt động

- Mỗi ngày có **1 lượt Lucky Wheel miễn phí** và **1 lượt Gem Chest miễn phí**, độc lập nhau. Không cần có gem để dùng.
- Dùng miễn phí trước; nút hiện **FREE / MIỄN PHÍ**, ẩn icon giá gem. Sau khi dùng, trở về giá gem cũ trong Inspector.
- Dùng cùng bảng quà, số lượng và trọng số hiện tại. Không đổi tỉ lệ giữa miễn phí và trả phí.
- Shortcut nào còn lượt thì hiện chấm đỏ phía trên bên phải. Mở/đóng bảng không tiêu lượt; chỉ nhận thành công mới tắt chấm.
- Reset **00:00 UTC+7** mặc định. Lượt chưa dùng không cộng dồn; nghỉ nhiều ngày vẫn chỉ có một lượt hiện tại cho mỗi chức năng.
- Quota lưu cùng receipt/phần thưởng. Đóng giữa animation hoặc mở lại game không cấp thêm quà/lượt. Vẫn phải xác nhận receipt cũ trước khi quay/mua tiếp.
- Đồng hồ lùi về ngày đã dùng không mở lại lượt. Đây là giờ thiết bị offline, chưa có xác thực giờ máy chủ.

## Chỉnh Inspector và prefab

Hai asset đang bật miễn phí:

- `Assets/The Archer/Scriptables/Lucky Rewards/Lucky Wheel.asset`
- `Assets/The Archer/Scriptables/Lucky Rewards/Gem Chest.asset`

Trong **Daily Free Use**, chỉnh `dailyFreeUse` và `dailyResetUtcOffsetHours` (mặc định 7). Giá trả phí `price` vẫn phải là gem dương. Asset mới mặc định chưa bật miễn phí. ID bảng là danh tính quota: không đổi ID chỉ để đổi tên; nội dung clone độc lập cần ID riêng.

Chấm đỏ dùng `Assets/The Archer/Prefabs/UI/Menu Pages/Reward Notification Dot.prefab`. Hai instance nằm trong `Assets/The Archer/Prefabs/UI/Lucky Rewards/Stage Lucky Rewards.prefab`, dưới shortcut, tên `Daily Free Reward Dot`. Chỉnh màu/kích thước/vị trí trong Prefab Mode, giữ Raycast Target tắt.

`Lucky Wheel Popup.prefab` và `Gem Chest Popup.prefab` trong cùng thư mục có `costIcon` trỏ tới ảnh Gem trên nút mua; runtime chỉ ẩn nó ở lượt miễn phí. Nhãn miễn phí có đủ en/vi/es/pt/fr. Không tạo cây UI runtime.

## Đường dẫn logic và save

| Nội dung | File từ gốc repository |
| --- | --- |
| Cấu hình quota, múi giờ và quà | `Assets/The Archer/Scripts/Lucky Rewards/LuckyRewardTable.cs` |
| Quota, giá thực tế, receipt và commit | `Assets/The Archer/Scripts/Lucky Rewards/LuckyRewardsService.cs` |
| Nhãn miễn phí, icon giá, refresh bảng đang mở | `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs` |
| Chấm đỏ shortcut, refresh qua ngày | `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/StageLuckyRewards.cs` |
| Kiểm tra ví/save/đồng hồ giả | `Assets/The Archer/Scripts/Editor/DailyLuckyRewardsVerification.cs` |
| Bản dịch Inspector | `Assets/The Archer/Resources/Menu Translations.asset` |

Giữ save key **Lucky Rewards**. Thêm `dailyFreeUses`, mỗi entry có `tableId` và `usedDay`. Receipt mới có `dailyFreeUse` và `gemCost`; receipt cũ vẫn đọc được và không nhận lại quà. Save cũ thiếu quota nhận hai lượt miễn phí đầu tiên, không reset tiến trình.

Chỉ ghi quota khi nhận thành công. Quota/receipt/sequence được commit trước callback ví để chặn reentry. Resolve nhận giá 0 khi miễn phí để kiểm tra overflow đúng, không dựa vào khoản gem giả định sẽ trừ. Config, roll và phần thưởng lỗi không tiêu lượt.

Shortcut và popup kiểm tra thời gian mỗi giây bằng unscaled time để cập nhật qua ngày, kể cả khi đang mở bảng. Giao dịch cập nhật badge ngay qua event; không ghi save mỗi giây.

## Kiểm tra

Tools → MrX Studio → Lucky Rewards → **Run Daily Free Checks** chạy bằng save/ví/đồng hồ giả: hai quota độc lập, 0 gem, reset UTC+7, không cộng dồn, rollback, reload/receipt, reentry, lỗi giao dịch, tắt cấu hình và save cũ. Bộ Lucky Rewards cũ được chạy lại để bảo vệ giao dịch trả phí và delivery.

Play Mode dùng VerificationSession: mở/đóng, quay và mở rương thật với 0 gem, click lặp, đóng giữa animation rồi mở lại, xác nhận receipt, chấm đỏ độc lập và dịch năm ngôn ngữ. Không sửa save thật. Không chạy build trong tác vụ này.

Kết quả: 25 kiểm tra quota mới, 21 kiểm tra giao dịch/selection cũ và 15 kiểm tra delivery đạt; bảng dịch đạt 1.333 điều kiện. Đã nạp lại Play và xác nhận cả hai quota đã dùng vẫn bị khóa; mô phỏng quota từ ngày trước mà không phát Changed để kiểm tra shortcut và popup đang mở tự cập nhật theo timer. Console lượt Play cuối có 0 lỗi, 0 cảnh báo. Đã kết thúc VerificationSession và đối chiếu hash save thật không đổi.
