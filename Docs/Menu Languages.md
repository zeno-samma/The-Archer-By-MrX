# Ngôn ngữ menu — 20/10/2026

Tại Stage main menu, bấm **LANGUAGE / NGÔN NGỮ** ở góc trên trái. Chọn English, Tiếng Việt, Español, Português hoặc Français. Tối đa năm lựa chọn; mặc định English. Chọn xong đổi nhãn ngay và đóng bảng; X hoặc Escape/gamepad Cancel đóng mà không đổi. Gamepad có điều hướng giữa năm nút. Bảng mở sẽ khóa dock và phím tắt của Stage để tránh mở popup khác cùng lúc.

Lựa chọn lưu qua SaveManager hiện có, khóa riêng `Language Settings`, trường `code`: en/vi/es/pt/fr. Không đổi ID item, dữ liệu stage, giá hoặc ví tiền. Code không hợp lệ quay về English khi nạp. Chọn lại ngôn ngữ hiện tại không ghi save thêm.

## Phạm vi dịch hiện tại

Hiện có 420 mục dịch cho menu và giao diện trong run, bao gồm tên/mô tả của 80 kỹ năng, độ hiếm, màn chọn kỹ năng, rương kỹ năng, tạm dừng và tên nhân vật/vũ khí Adventurers. Các format `{0}`, `{1}` giữ số liệu động; mô tả nhiều dòng được đối chiếu theo khoảng trắng. Nhãn do controller cập nhật được dịch lại trước khi dựng canvas; chỉ dịch lại khi nội dung/font đổi hoặc đổi ngôn ngữ.

Tên và mô tả kỹ năng hiện tại đã có đủ năm ngôn ngữ. Nội dung mới chưa có trong bảng vẫn giữ nguyên nguồn tiếng Anh; đây chưa phải xác nhận mọi tên quái hay mọi chuỗi gameplay đều đã được dịch. Quest hiện tại đã dịch tiêu đề, mô tả, tiến độ, thời gian và thông báo; xem [Responsive Menu and Daily Updates](Responsive%20Menu%20and%20Daily%20Updates.md). Hướng dẫn thêm kỹ năng và chỉnh Stage 4: [Run Languages and Halloween Crypt](Run%20Languages%20and%20Halloween%20Crypt.md).

## File cần tìm

| Nội dung | Đường dẫn từ gốc dự án |
| --- | --- |
| Bảng dịch chỉnh Inspector | `Assets/The Archer/Resources/Menu Translations.asset` |
| Font SDF Latin/Vietnamese | `Assets/The Archer/Resources/Menu Latin Font.asset` |
| Font nguồn sẵn có | `Assets/TextMesh Pro/Fonts/LiberationSans.ttf` |
| Nút và hộp thoại chọn | `Assets/The Archer/Prefabs/UI/Stage/Language Picker.prefab` |
| Trang chứa instance nút | `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` |
| Scope dịch trên trang/dock | `Assets/The Archer/Scripts/UI/Main Menu/LocalizedMenuScope.cs` |
| Logic hộp thoại | `Assets/The Archer/Scripts/UI/Main Menu/LanguagePicker.cs` |
| Service, save, dữ liệu dịch và fallback | `Assets/The Archer/Scripts/Localization/MenuTranslations.cs` |
| Authoring một lần trong Editor | `Assets/The Archer/Scripts/Editor/LanguageAuthoring.cs` → `Install()` |
| Kiểm tra tự động | `Assets/The Archer/Scripts/Editor/LanguageVerification.cs` → `Verify()` |

Đổi bố cục/màu/kích thước trong Prefab Mode. Không dựng UI runtime. Năm nút đã vẽ sẵn, script chỉ binding/chọn ngôn ngữ. Installer giữ bảng/prefab đã tồn tại, không dùng để cập nhật chỉnh tay thường ngày.

Thêm bản dịch: English là chuỗi nguồn chính xác; điền đủ bốn cột còn lại, giữ nguyên `{0}` nếu có. Code của hệ thống không dùng chuỗi hiển thị làm ID tiến trình. Nội dung thiếu bản dịch giữ nguyên nguồn. Khi thêm ký tự mới ngoài atlas đã tạo, cần cập nhật font atlas trong Editor; font hiện được đóng tĩnh từ các ký tự của bộ dịch để tránh phát sinh glyph runtime.

## Xác minh

Dùng VerificationSession để thử bằng save riêng. Bộ menu ban đầu đã vượt qua 293 điều kiện; sau mở rộng bảng, kiểm tra bản dịch/năm mã/placeholder/fallback đạt 1.158 điều kiện. Play Mode đã kiểm tra cả năm nút thật sau khi canvas cập nhật, mở lặp, đóng không đổi, khóa/khôi phục dock, chặn mở chồng với Lucky Wheel, đổi về English, cập nhật số màn và nạp lại lựa chọn Vietnamese qua lần Play mới. Các hàng upgrade có thêm 90 kiểm tra tiêu đề/mô tả trực tiếp qua năm ngôn ngữ. Kiểm tra prefab không có missing script; Console lượt Play cuối có 0 lỗi và 0 cảnh báo. Save thật được đối chiếu hash trước/sau để bảo đảm không thay đổi. Chưa kiểm tra build thiết bị.

Trong một lần reload trước đó có lỗi nội bộ Unity Hierarchy (ghi `UserSettings/HierarchyWindow.settings` và assertion main thread); lần biên dịch/Play cuối không tái hiện. Đây không phải lỗi được ghi nhận từ script chọn ngôn ngữ.
