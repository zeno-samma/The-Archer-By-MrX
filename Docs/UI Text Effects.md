# UI Text Effect — hiệu ứng chữ kéo thả

Cập nhật 22/10/2026. Component dùng cho **TextMeshProUGUI trong Canvas**, không cần Animator hoặc manager riêng.

## Dùng ngay, không cần viết code

1. Tạo **UI → Text - TextMeshPro** trong Canvas, nhập chữ và chọn font phù hợp như bình thường.
2. Kéo `Assets/The Archer/Scripts/UI/UITextEffect.cs` vào GameObject text, hoặc chọn **Add Component → MrX Studio → UI → UI Text Effect**.
3. Trong component, chọn **Effect**. Mặc định là Bounce (nảy).
4. Chỉnh **Duration** theo giây. Bật **Play On Enable** để tự chạy mỗi khi text được bật trong game; bật **Loop** nếu muốn lặp.
5. Bấm **Preview** ngay trong Inspector để xem ở Edit Mode. Nếu text thuộc prefab, mở prefab bằng **Open**, chọn text bên trong rồi Preview.
6. Bấm **Stop / Restore** để trả text về trạng thái trước Preview. Đổi lựa chọn trong Inspector sẽ dừng Preview để chỉnh lại.

Preview tự khôi phục trước khi lưu scene/prefab, khi reload script, chuyển Play Mode hoặc rời Inspector. Chữ do người thiết kế sửa trong lúc Preview được giữ lại khi dừng. Không lưu giá trị đếm tạm vào prefab.

## Các kiểu có sẵn

| Effect | Hiển thị | Thông số chính |
| --- | --- | --- |
| Count Number | Số tăng/giảm tới giá trị đích; có định dạng thời gian | Number From/To, Number Display, Number Format, Prefix/Suffix, Easing |
| Bounce | Chữ nảy lên, giảm biên độ rồi trở về chỗ cũ | Amplitude, Frequency, Per Character |
| Pulse | Phóng to/thu nhỏ theo nhịp | Scale Amount, Frequency |
| Shake | Rung, không thay đổi trạng thái random của gameplay | Amplitude, Frequency |
| Fade In | Chữ xuất hiện từ trong suốt tới màu gốc | Easing |
| Typewriter | Hiện lần lượt từng ký tự, giữ tag rich text | Duration, Easing; chọn Linear nếu muốn nhịp đều |
| Wave | Từng ký tự chuyển động thành sóng | Amplitude, Frequency, Character Phase |
| Rainbow | Màu cầu vồng chạy qua ký tự | Frequency, Character Phase |
| Pop | Chữ lớn dần, nảy vượt kích thước một chút rồi ổn định | Duration, Per Character; dùng Back Out có sẵn |
| Slide In | Chữ trượt từ một khoảng lệch vào vị trí gốc | Slide Offset, Easing |
| Float | Chữ lơ lửng theo nhịp | Amplitude, Frequency |
| Color Pulse | Nhấp nháy nhẹ về một màu nhấn | Accent Color, Frequency |

Hiệu ứng hình học/màu trở về trạng thái gốc sau mỗi lần chạy; Count giữ số cuối và Typewriter hiện đủ chữ theo giới hạn hiển thị ban đầu.

## Các tùy chọn chung

- **Use Unscaled Time:** mặc định bật, vẫn chạy khi màn kết quả/popup đặt `Time.timeScale = 0`. Tắt nếu muốn hiệu ứng dừng theo gameplay.
- **Delay:** thời gian chờ ban đầu. Fade/Pop/Typewriter có thể ở trạng thái ẩn lúc chờ.
- **Loop Pause:** khoảng giữ trạng thái cuối giữa hai lần lặp, không phải delay đầu.
- **Updates Per Second:** mặc định 30, giới hạn số lần cập nhật hiệu ứng. Chỉ tăng khi cần chuyển động mịn hơn.
- **Per Character:** áp dụng chuyển động/điểm co giãn cho từng chữ ở các kiểu hỗ trợ. Wave luôn tạo lệch pha giữa ký tự.
- **Character Phase:** chênh lệch pha giữa các chữ; tăng để chuyển động rõ hơn.
- **On Complete:** sự kiện có thể nối qua Inspector khi hiệu ứng một lần kết thúc hoặc khi gọi Complete. Loop không tự phát sự kiện này mỗi chu kỳ. Stop/hủy giữa chừng không phát On Complete. Preview Edit Mode không gọi sự kiện gameplay.

Script không thay đổi RectTransform, vị trí hoặc scale của khung text; vì vậy không kéo các phần tử khác trong LayoutGroup. Các chuyển động được áp dụng lên hình học chữ. Không tạo GameObject/UI/material mới trong runtime.

## Đếm chỉ số

Chọn Count Number. Ví dụ text ban đầu là `12,345`, **Read Number From Text** bật và **Number From = 0**: hiệu ứng chạy từ 0 đến 12.345. Nếu không đọc được số, dùng **Number To** trong Inspector. Cách đọc/hiển thị số theo culture hiện tại của ứng dụng.

Một số Number Format:

| Format | Kết quả ví dụ |
| --- | --- |
| `0` | `12345` |
| `#,0` | `12,345` |
| `#,0.##` | `12,345.67` |
| `0.0` | `12345.7` |

**Prefix/Suffix** thêm chữ trước/sau số, chẳng hạn `+` và ` DPS`. Nếu text đã có các phần này, script loại đúng prefix/suffix đã cấu hình khi đọc số đích.

Chọn **Number Display → Minutes Seconds** cho thời gian. Đầu vào Number To hoặc API là **giây**: `125.5` sẽ hiển thị `02:05.50`. Khi đọc từ text, dạng hỗ trợ là `phút:giây`, không phải giờ:phút:giây. Số giây âm hiển thị 00:00.00; chế độ Number hỗ trợ số âm bình thường.

Khi tự bật lại text sau một lần đếm bị ngắt do ẩn GameObject, số đích được giữ đúng. Khi Stop thủ công, số đang hiển thị được giữ; nếu muốn dùng một đích cụ thể cho lần sau, truyền giá trị qua PlayNumber.

## Điều khiển từ nút bấm hoặc script

Trong UnityEvent của button, kéo text vào và chọn `UITextEffect.Play`, `Stop`, `Complete`, `PlayText(string)` hoặc `CountTo(float)`.

Khi có dữ liệu thật, gọi sau bước binding text:

```csharp
[SerializeField] private MrX.UI.UITextEffect damageEffect;

// Số từ 0 tới sát thương thật; định dạng lấy từ Inspector.
damageEffect.PlayNumber(0, totalDamage);
```

| API | Hành vi |
| --- | --- |
| `Play()` | Chạy lại kiểu đang chọn |
| `PlayText(string)` | Gán chữ mới rồi chạy kiểu đang chọn |
| `PlayNumber(double from, double to)` | Chạy đếm số với dữ liệu truyền vào, không đổi lựa chọn Effect đã serialize |
| `CountTo(float)` | Tiện nối UnityEvent; bắt đầu từ Number From |
| `Complete()` | Nhảy tới cuối và phát On Complete một lần nếu đang chạy |
| `Stop()` | Dừng; trả hình học/giới hạn ký tự về gốc, giữ số hiện tại nếu đang đếm |
| `StopAndRestore()` | Dừng và khôi phục cả nội dung text đã chụp khi bắt đầu |
| `IsPlaying` | Bao gồm trạng thái đang chờ tự chạy |
| `Effect` | Đổi kiểu bằng code; dừng kiểu cũ trước khi đổi |

Mỗi text dùng một component hiệu ứng. Với chỉ số đang có script khác ghi text liên tục, dùng một đường binding/đếm duy nhất để tránh hai nơi ghi cùng nội dung. Bộ đếm cũ trên Run Summary vẫn do `RunRecordHighlights` quản lý; tác vụ này không tự thêm component đếm thứ hai vào các chỉ số đó. Hàm định dạng thời gian của bảng cũ đã dùng chung helper mới.

## Prefab mẫu và vị trí file

Mở `Assets/The Archer/Prefabs/UI/Text Effects/UI Text Effects Demo.prefab` để thấy 12 ô mẫu. Chọn `Animated Text` trong một ô, Preview và sửa các tham số. Có thể sao chép riêng text đó vào Canvas của bạn. Toàn prefab mẫu có Canvas riêng để thử; chưa được chèn vào menu/game đang dùng.

Lệnh **Tools → MrX Studio → UI → Create Text Effects Demo Prefab** tạo prefab mẫu nếu chưa có; nếu có rồi chỉ chọn asset, giữ những chỉnh sửa của bạn.

| Đường dẫn | Nội dung |
| --- | --- |
| `Assets/The Archer/Scripts/UI/UITextEffect.cs` | Component runtime, 12 kiểu, bộ đếm, callback và buffer hình học |
| `Assets/The Archer/Scripts/Editor/UITextEffectEditor.cs` | Inspector theo từng kiểu và Preview có khôi phục |
| `Assets/The Archer/Scripts/Editor/UITextEffectAuthoring.cs` | Tạo prefab mẫu trong Edit Mode |
| `Assets/The Archer/Scripts/Toolkit/Easing/EasingFunctions.cs` | Các hàm easing đang có được tái sử dụng |

Buffer vertex/color được giữ lại để tái sử dụng và chỉ tăng khi nội dung cần thêm chỗ. Đếm số vẫn tạo chuỗi ở mỗi lần cập nhật; không nên coi đây là zero-allocation cho mọi kiểu. Nên dùng Loop cho những điểm nhấn cần thiết thay vì toàn bộ chữ trong một màn hình.

## Kiểm tra qua Unity MCP

- Cả 12 kiểu thay đổi đúng ở giữa tiến trình, kết thúc được và khôi phục hình học/màu/text gốc khi StopAndRestore.
- Chữ tiếng Việt, rich text, chuỗi trống và thay nội dung dài hơn trong lúc Wave đang chạy.
- Play Mode khi `timeScale = 0`: 11 component dùng unscaled kết thúc, component Shake dùng scaled giữ trạng thái chờ; callback tương ứng chỉ phát cho các component kết thúc.
- Số âm, phần thập phân, thời gian, format sai có fallback, dừng/bật lại giữa đếm và Complete gọi hai lần chỉ phát callback một lần.
- Chạy/dừng Wave 32 lần không làm lệch RectTransform/scale của UI.
- Preview dừng không làm mất text mới do người thiết kế sửa; lưu prefab khi đang Preview giữ nguyên text gốc.
- Console được kiểm tra không có lỗi gameplay/biên dịch. Save thật giữ nguyên hash sau các phiên thử riêng. Không build player.

## Tham khảo kỹ thuật

Đã đối chiếu API TMP qua MCP và tài liệu Unity: [OnPreRenderText](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_Text.OnPreRenderText.html) là điểm can thiệp hình học trước khi render; [TMP_Text](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_Text.html) mô tả maxVisibleCharacters và UpdateVertexData. Script triển khai các chuyển động riêng, giữ font/material và cấu trúc uGUI hiện có.


### Binding màn kết quả gọn

`Completed` là event C# bổ sung bên cạnh UnityEvent trong Inspector; đăng ký/hủy đối xứng theo vòng đời view. `Prefix` và `Suffix` cho phép đổi nhãn bản địa hóa trước khi gọi `PlayNumber`. Màn kết quả gọn dùng event này để hiện nhãn kỷ lục sau khi đếm; không để script khác đồng thời ghi vào số đang chạy.
