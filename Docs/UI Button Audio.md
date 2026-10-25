# Âm thanh nút bấm

Cập nhật 22/10/2026. Hệ thống dùng Button uGUI, AudioManager/AudioData và pool AudioSource hiện có; không thêm AudioSource riêng cho từng nút.

## Gắn vào nút mới

1. Mở scene hoặc prefab trong Editor, chọn GameObject chứa component **Button**.
2. Chọn **Add Component → MrX Studio → UI → Button Audio**, hoặc kéo `Assets/The Archer/Scripts/UI/UIButtonAudio.cs` vào nút.
3. Chọn **Mode**:

| Mode | Kết quả |
| --- | --- |
| Default | Dùng âm thanh Button Click chung trong AudioManager |
| Custom | Dùng AudioData ở trường Custom Sound; không phát thêm tiếng mặc định |
| Silent | Tắt tiếng click cho nút này |

Custom không có AudioData sẽ im lặng. Tắt component cũng tắt âm thanh của nó. Không cần gắn thủ công một lời gọi phát click vào On Click trong Inspector.

Gắn script vào prefab nút gốc để các instance kế thừa. Muốn nút riêng có tiếng khác, chọn Custom trên instance hoặc prefab variant rồi gán Custom Sound; đây là override dữ liệu Inspector, không cần subclass C#.

## Đổi tiếng mặc định và tạo tiếng riêng

- Asset mặc định hiện dùng: `Assets/The Archer/Scriptables/Audio/UI/Button Click.asset`. Sửa Samples/volume/pitch/cooldown tại asset này để đổi tiếng chung cho các nút Default.
- Có thể thay reference **Button Click** trên AudioManager trong Main Menu nếu muốn dùng asset khác.
- Để tạo tiếng riêng, dùng **Create → MrX Studio → Audio → Data**; cấu hình sample có clip và **Audio Group = UI**, rồi gán vào Custom Sound. Asset có cooldown sẽ tuân theo cooldown đó, kể cả khi nhiều nút cùng sử dụng asset.
- Âm lượng/mute SFX vẫn theo cài đặt chung. UI click hoạt động khi `Time.timeScale = 0` qua AudioManager hiện hữu.

## Áp cho UI đang có

Đã gắn component và lưu vào các prefab/scene của dự án, bao gồm các mẫu hàng/thẻ được nhân bản trong danh sách. Nếu sau này dùng công cụ dựng lại trang UI, chạy **Tools → MrX Studio → UI → Install Button Audio** trong Edit Mode.

Lệnh bổ sung component còn thiếu trên Button trong `Assets/The Archer/Prefabs` và `Assets/The Archer/Scenes`. Nút đã có component sẽ giữ Mode/Custom Sound. Lệnh không reset save, không đổi bố cục và không build. Prefab ngoài các thư mục này cần gắn script thủ công. Giao diện vẫn nằm trong prefab/scene; runtime chỉ nghe sự kiện nút và phát âm thanh.

## Quy tắc khi sửa code

- Component nghe `Button.onClick`, vì vậy pointer chuột/cảm ứng và Submit của keyboard/gamepad cùng dùng một đường. Button bị khóa hoặc bị CanvasGroup chặn tương tác sẽ không phát qua luồng input chuẩn.
- Các phím tắt trực tiếp của dự án đã chuyển sang `UIButtonAudio.Click(button)`. Hàm kiểm tra nút đang bật và tương tác được rồi gọi đúng sự kiện Button, giữ lựa chọn Default/Custom/Silent.
- Riêng mũi tên selector vốn bị ẩn khi dùng tay cầm: controller kiểm tra khả năng chuyển, gọi `PlayShortcut(button)` để phát cấu hình của nút ẩn rồi chạy hành động chuyển cũ. Helper này chỉ phát âm thanh, không chạy onClick và không thay điều hướng.
- Không gọi thêm `GameController.AudioManager.PlayButtonClick()` trong handler của nút đã có component. Các lệnh click cũ ở UI đã được chuyển sang component để tránh phát hai tiếng.
- Âm báo giao dịch thành công/thất bại, mở rương hoặc rèn là âm của **kết quả gameplay**. Giữ chúng tại logic kết quả bằng `PlayAudio`; component chỉ phụ trách click.
- Khi thay callback nghiệp vụ, dùng `RemoveListener` đúng callback thay vì `RemoveAllListeners` trên nút đang hoạt động. Lệnh xóa tất cả sẽ xóa cả listener âm thanh. Popup Armory đã được sửa để chỉ thay callback do nó sở hữu.
- Không kiểm tra lại `interactable` sau khi transaction đã chạy trong callback âm thanh: chính transaction có thể vừa khóa/ẩn nút. Luồng Button đã kiểm tra đầu vào trước khi gọi các listener. Nếu gọi bằng code, dùng helper Click thay vì Invoke trực tiếp trên nút bị khóa.

## File xử lý

| Đường dẫn | Vai trò |
| --- | --- |
| `Assets/The Archer/Scripts/UI/UIButtonAudio.cs` | Component, ba chế độ và helper Click cho phím tắt |
| `Assets/The Archer/Scripts/Toolkit/Audio/AudioManager.cs` | Phát âm qua pool/mixer, âm mặc định và trạng thái đã khởi tạo |
| `Assets/The Archer/Scripts/Toolkit/Audio/AudioData.cs` | Sample, volume/pitch/cooldown và mixer group |
| `Assets/The Archer/Scripts/Editor/UIButtonAudioAuthoring.cs` | Lệnh bổ sung vào prefab/scene trong Edit Mode |

## Kiểm tra qua Unity MCP

- Default và Custom tạo đúng một audio voice với đúng AudioData; Silent/Custom thiếu data không tạo voice.
- Nút disabled không phát với pointer hoặc gamepad Submit; Submit hợp lệ phát một lần. Bật/tắt component 20 lần không nhân listener.
- Pause vẫn phát; mute SFX làm voice có volume 0. Callback tự ẩn nút/popup vẫn phát đúng một lần.
- Nút Settings thật và shortcut Cancel đóng Settings: một voice mỗi thao tác.
- Popup Armory mở/đổi callback xác nhận năm lần vẫn giữ listener âm thanh, một voice mỗi click.
- Hàng thành tựu bổ sung từ pool kế thừa component và phát một lần.
- Kiểm tra mũi tên bị ẩn vẫn chuyển stage khi chạy nhánh shortcut, phát âm theo cấu hình và tuân theo Silent/không khả dụng.
- Audit 38 prefab chứa Button trực tiếp (238 Button tính cả nội dung lồng), 186 Button của Main Menu và 25 Button của Game: mỗi nút có đúng một component. Chạy lại installer không thêm trùng.
- Hồ sơ thử được cô lập; save thật giữ nguyên hash. Không build trong tác vụ này. Đây là kiểm tra dispatch/voice và mixer trong Editor, chưa phải kiểm thử bằng mọi mẫu tay cầm hoặc thiết bị âm thanh thực tế.
