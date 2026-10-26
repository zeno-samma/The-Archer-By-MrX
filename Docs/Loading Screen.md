# Màn hình tải game

## Nội dung và luồng hoạt động

Màn hình dùng `ui_background_main`, thanh vàng, phần trăm và `Icon_Booster` di chuyển theo đầu thanh. Nền giữ tỷ lệ và cắt phần dư để phủ kín màn hình; nội dung thanh nằm trong vùng an toàn. Giao diện được lưu sẵn bằng uGUI/TMP, không dựng cây UI khi chạy.

Màn hình được tích hợp vào luồng chuyển Main Menu → Game và Game → Main Menu hiện có. Main Menu vẫn là scene khởi động của dự án. Đây là tiến độ tải scene cục bộ; chưa phải tiến độ tải dữ liệu máy chủ hay tải file qua mạng.

Tiến độ gồm các bước: mở Loading Screen (0–8%), dỡ scene cũ (8–15%), tải scene đích (15–100%). Phần tải scene đích lấy từ `AsyncOperation.progress`; khi dữ liệu scene sẵn sàng, hiển thị 100% một frame trước khi kích hoạt scene. Máy tải nhanh có thể nhảy phần trăm qua nhiều mốc. Không có bộ đếm giả hoặc thời gian chờ nhân tạo. Việc khởi tạo gameplay sau khi scene kích hoạt không được đo như một tác vụ tải riêng.

## Chỉnh giao diện hoặc thay hình con trỏ

1. Mở prefab `Assets/The Archer/Prefabs/UI/Loading/Loading Screen.prefab` trong Project.
2. Chọn `Safe Content/Progress Bar/Fill Area/Progress Cursor`.
3. Trong component Image, thay **Source Image** bằng sprite mong muốn. Giữ **Preserve Aspect** để ảnh không bị méo.
4. Chỉnh Width/Height trong RectTransform nếu muốn thay kích thước hình; giữ anchor và vị trí do presenter điều khiển theo tiến độ.
5. Chọn object gốc `Loading Screen`, chỉnh **Preview Progress** trong LoadingScreenPresenter để xem trước 0–1 trong Edit Mode. Thay đổi này chỉ phục vụ xem trước; khi chơi sẽ dùng tiến độ thật.
6. Màu/ảnh thanh chỉnh tại Image của `Progress Bar` và `Fill Area/Fill`. Thanh dùng Sliced để co giãn; giữ border sprite và pixelsPerUnitMultiplier nếu không chủ động thay kiểu trình bày.
7. Chữ chỉnh tại `Loading Status`, `Percentage`; nền tại `Background`, lớp tăng tương phản tại `Bottom Shade`.
8. Lưu prefab. Scene `Assets/The Archer/Scenes/Loading Screen.unity` đã chứa instance của prefab này.

Không cần gắn lại script khi chỉ thay ảnh. Đừng xóa các object đang được tham chiếu trong LoadingScreenPresenter.

## Ngôn ngữ

Thông báo `Loading data...` có bản dịch Anh, Việt, Tây Ban Nha, Bồ Đào Nha và Pháp trong `Assets/The Archer/Resources/Menu Translations.asset`. Runtime theo ngôn ngữ người chơi đã chọn. Font lấy từ Latin Font của bảng dịch khi authoring.

## File chính

| Đường dẫn | Vai trò |
| --- | --- |
| `Assets/The Archer/Prefabs/UI/Loading/Loading Screen.prefab` | Nguồn chỉnh sửa giao diện |
| `Assets/The Archer/Scenes/Loading Screen.unity` | Scene chuyển tiếp chứa prefab |
| `Assets/The Archer/Scripts/UI/LoadingScreenPresenter.cs` | Binding thanh, phần trăm, con trỏ, safe area và ngôn ngữ |
| `Assets/The Archer/Scripts/UI/SceneLoadingProgress.cs` | Trạng thái tiến độ, reset mỗi lần chuyển scene |
| `Assets/The Archer/Scripts/Other/GameController.cs` | Báo tiến độ từ luồng tải scene thật |
| `Assets/The Archer/Scripts/Editor/LoadingScreenAuthoring.cs` | Công cụ dựng/lưu prefab ở Edit Mode |

`Tools → MrX Studio → UI → Install Loading Screen` tạo lại prefab và cập nhật Loading Screen scene. **Lệnh này ghi đè bố cục prefab**, chỉ chạy khi muốn dựng lại mẫu; chỉnh mỹ thuật trực tiếp trong prefab cho công việc thường ngày.

## Kiểm tra đã thực hiện

- Unity biên dịch thành công; kiểm tra Play Mode trên profile thử nghiệm riêng.
- Mốc -1/0/0.68/1/2/NaN lần lượt hiển thị 0/0/68/100/100/0%; con trỏ cùng giá trị đã clamp.
- Quan sát ảnh chụp Game View ở 1920×1080 và 1080×1920, chữ tiếng Việt đầy đủ dấu.
- Chuyển Main Menu → Game → Main Menu, tiến độ kết thúc 100%, trạng thái loading tắt.
- Sau khi xóa log cũ và chạy hai luồng chuyển scene: Console không ghi lỗi/cảnh báo mới.
- Hash save thật không thay đổi sau kiểm tra. Không chạy build.

Các cảnh báo API cũ và lỗi cửa sổ Editor/MCP có trước đợt kiểm tra không thuộc thay đổi màn hình tải; kết quả Console ở trên chỉ áp dụng phiên thử đã nêu.
