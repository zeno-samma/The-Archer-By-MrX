# Menu ngang/dọc và cập nhật hằng ngày — 20/10/2026

## Sửa bố cục

Menu có hai bố cục lưu trong prefab: ngang **3840 × 2160**, dọc **1080 × 2160**. Khi đổi hướng, script áp vị trí/kích thước đã lưu; không tạo cây UI runtime. Khung thiết kế thu phóng đồng nhất trong vùng an toàn, hình nền phủ màn hình và giữ tỉ lệ. Tablet/màn hình rộng có thể có thêm khoảng nền quanh khung để giữ tỉ lệ các phần tử.

Ở dọc, Stage xếp hero, preview, phần thưởng và Start theo chiều trên xuống; Armory đặt chỉ số/túi dưới hero; Upgrades đặt lựa chọn/chỉ số phía trên danh sách. Quest/Achievements đặt danh sách dưới tiêu đề; Forge đặt nguyên liệu trên công thức. Inventory có 4 cột ở dọc, 6 cột ở ngang. Vòng quay, bảng mua và chọn ngôn ngữ có vị trí dọc riêng.

1. Mở prefab trang trong **Prefab Mode**, chọn object gốc có `ResponsiveMenuLayout`.
2. Bấm **Preview Portrait** hoặc **Preview Landscape**. Công cụ đặt kích thước khung prefab trang tương ứng.
3. Chỉnh RectTransform, font, chiều cao hàng/grid trực tiếp; bấm **Capture Portrait/Landscape** để lưu đúng profile. Phần tử mới muốn có vị trí theo hướng cần thêm vào danh sách tham chiếu tương ứng trong component.
4. Trở lại **Preview Landscape** trước khi lưu để giữ bản xem trước mặc định ngang. Kiểm tra cả hai hướng trong Play; thay đổi trong Play không tự lưu vào asset.

Dock có profile riêng ở `Menu Dock.prefab`. Công cụ **Tools → MrX Studio → UI → Install Portrait and Landscape Layouts** dùng cho thiết lập ban đầu, giữ profile đã tồn tại; không xóa profile để cập nhật bố cục thường ngày.

## Cuộn chuột

Inventory và Upgrades hỗ trợ con lăn; kéo cảm ứng vẫn hoạt động. `InputSystemUIInputModule.scrollWheel` trong Main Menu nối action **UI/ScrollWheel** của Input Asset cũ. Viewport có Image trong suốt nhận raycast để chuột ở khoảng trống cũng cuộn được. Độ nhạy mặc định `Scroll Sensitivity = 90`, chỉnh trên ScrollRect prefab. Inventory tiếp tục tái sử dụng các ô đã vẽ theo vùng nhìn thấy.

## Quest và hạn mức mua

Bảng dịch hiện có **210 mục**, gồm tiêu đề/mô tả năm quest, tiến độ, thời gian reset, trạng thái và thông báo nhận thưởng bằng en/vi/es/pt/fr. Quest mới cần thêm bản dịch trong `Menu Translations.asset`; xem [Menu Languages](Menu%20Languages.md).

Mua năng lượng mặc định tối đa **5 lần/ngày**. Trong `Resources/Menu Economy Settings.asset`, chỉnh **Daily Energy Purchase Limit** và **Purchase Reset Utc Offset Hours**. Reset mặc định **00:00 UTC+7**; giới hạn 0 tắt mua năng lượng. Bảng mua hiển thị số lượt còn lại, khóa nút khi hết. Chỉ giao dịch thành công tính lượt; thiếu gem, đầy năng lượng, cấu hình sai không tính lượt. Gold không dùng hạn mức này. Lượt đã dùng lưu cùng giao dịch, giữ qua reload. Đồng hồ lùi không reset lượt; đây là giờ thiết bị, chưa xác thực thời gian máy chủ.

## Stage mặc định và resume

Mỗi lần Main Menu tải lại, lựa chọn hiển thị về **Stage 1**. Run đang dở giữ ID nội dung stage trong `Continue Playing.stageContentId`, nên resume vẫn vào đúng stage cũ, không trừ năng lượng lần nữa. Save cũ chưa có ID lấy stage đã chọn trước khi menu reset. Không đổi ID stage chỉ để đổi tên.

## File cần tìm

Các đường dẫn sau bắt đầu từ gốc repository.

| Nội dung | Đường dẫn |
| --- | --- |
| Scene và nối action cuộn | `Assets/The Archer/Scenes/Main Menu.unity` |
| Khung safe area | `Assets/The Archer/Prefabs/UI/Menu Pages/Responsive Menu Surface.prefab` |
| Stage | `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` |
| Năm trang khác và dock | `Assets/The Archer/Prefabs/UI/Menu Pages/` |
| Safe area và hướng | `Assets/The Archer/Scripts/UI/ResponsiveMenuSurface.cs` |
| Profile vị trí/chữ/hàng/grid | `Assets/The Archer/Scripts/UI/ResponsiveMenuLayout.cs` |
| Preview/Capture Inspector | `Assets/The Archer/Scripts/Editor/ResponsiveMenuLayoutEditor.cs` |
| Thiết lập asset/scene Editor | `Assets/The Archer/Scripts/Editor/ResponsiveMenuAuthoring.cs` |
| Cột và tái sử dụng ô inventory | `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs` |
| Hạn mức và giao dịch | `Assets/The Archer/Scripts/Toolkit/Currency/MenuEconomyService.cs` |
| Reset lựa chọn menu | `Assets/The Archer/Scripts/UI/Main Menu/MainMenuStagesPageBehavior.cs` |
| ID stage đang chơi | `Assets/The Archer/Scripts/Stage/ContinuePlayingSave.cs` |
| Resume đúng stage | `Assets/The Archer/Scripts/Stage/StageController.cs` |

## Xác minh

Kiểm tra Unity Editor qua MCP dùng hồ sơ riêng: mua đủ 5 lượt, chặn lượt 6 không mất gem, giữ hạn mức sau reload; reset UTC+7, đồng hồ lùi, thiếu tiền và đầy năng lượng. Logic/reference đạt 1.355 điều kiện; bảng dịch đạt 1.323 điều kiện (bao gồm reference prefab, không phải số kịch bản gameplay độc lập). Quest có 50 đối chiếu tiêu đề/mô tả trực tiếp qua năm ngôn ngữ.

Đã kiểm tra cuộn ScrollRect Inventory/Upgrades và run Stage 3 → menu Stage 1 → tiếp tục Stage 3 không mất năng lượng lại. Đã xem Game View 1920×1080, 1080×2160, 1536×2048, 2560×1080; khung menu nằm trong safe area. Đã kiểm tra API safe area Device Simulator; runtime dùng `UnityEngine.Device.Screen` để nhận kích thước mô phỏng. Chưa xác nhận build hoặc thao tác trên điện thoại thật, chưa bao quát mọi mẫu thiết bị/nội dung tương lai.

Lượt Play cuối sau biên dịch: Console **0 lỗi, 0 cảnh báo**, sáu ví và khóa dock khi mở bảng mua hoạt động. Trong một lần reload trước đó, Editor ghi IOException khi lưu `UserSettings/HierarchyWindow.settings` và assertion nội bộ `CurrentThread::IsMainThread()`; plugin MCP cũng có cảnh báo WebSocket lúc reload. Không tái hiện ở lượt Play cuối; đây vẫn là giới hạn của kết quả kiểm tra, không khẳng định toàn bộ phiên Editor chưa từng có log lỗi.
