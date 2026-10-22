# Hướng dẫn build Windows PC — The Archer By MrX

Cập nhật **20/10/2026**. Đây là bản Windows chạy độc lập để chơi thử; chưa tích hợp Steam SDK hoặc tạo gói phát hành lên Steam.

Bản mới hơn có chấm đỏ nhận thưởng và sửa chuyển thiết bị điều khiển nằm trong `Builds/Windows/PC-Playtest-20261020-02/`. Xem [Reward Notifications and Input](Reward%20Notifications%20and%20Input.md); các số liệu bản `01` bên dưới được giữ làm lịch sử lần build đầu.

## 1. Bản đã build và cách chơi ngay

Từ thư mục gốc dự án, mở:

`Builds/Windows/PC-Playtest-20261020-01/The Archer By MrX.exe`

Bản nén để sao chép sang máy khác:

`Builds/Windows/The-Archer-By-MrX-PC-Playtest-20261020.zip`

1. Nếu dùng ZIP, giải nén toàn bộ vào một thư mục.
2. Mở `The Archer By MrX.exe` trong thư mục đã giải nén.
3. Chọn ngôn ngữ ở Stage main menu nếu cần; vào Stage 1 và bấm Start. Mỗi run mới trừ năng lượng theo StageData; tiếp tục run dở không tính phí lần nữa.
4. Di chuyển bằng **W/A/S/D**, phím mũi tên hoặc cần analog gamepad. Đứng yên để nhân vật tự bắn theo gameplay hiện tại. Menu hỗ trợ chuột, con lăn Inventory/Upgrades và gamepad.
5. Dùng **Alt+F4** để đóng game. Có thể dùng **Alt+Enter** để chuyển cửa sổ/toàn màn hình theo hỗ trợ của player.

Giữ `.exe`, `UnityPlayer.dll`, `MonoBleedingEdge`, thư mục `_Data` và các thành phần Unity xuất ra ở cạnh nhau. Không chỉ gửi mỗi file `.exe`. Thư mục `The Archer By MrX_BackUpThisFolder_ButDontShipItWithYourGame` là bản sao phục vụ khôi phục/debug của Unity, được giữ tại máy build nhưng **không đưa vào ZIP chơi thử**.

## 2. Kết quả build đã xác minh

| Mục | Giá trị |
| --- | --- |
| Unity | 6000.6.2f1 |
| Platform | Windows, Intel 64-bit / StandaloneWindows64, Player |
| Backend | Mono |
| Product / Version | The Archer By MrX / 1.2.2 |
| Development Build / Script Debugging | Tắt |
| Compression | LZ4 |
| Kiểm tra build | Strict Mode + Detailed Build Report |
| Kết quả cuối | **Succeeded, 0 errors, 0 warnings** |
| Hoàn tất | 15:12:16 ngày 20/10/2026, giờ Việt Nam |
| Thời gian build lại | Khoảng 52 giây; lần đầu khoảng 106 giây |
| Dung lượng theo Unity BuildReport | 141.950.546 byte, khoảng 135,37 MiB |

Báo cáo máy đọc được: [Windows Build Report 2026-10-20.json](Windows%20Build%20Report%202026-10-20.json). Đã kiểm tra executable có định dạng PE Windows x64 và đủ runtime Mono/assembly game. Console sau build cuối không có lỗi/cảnh báo. Chưa mở bản player để chơi trọn một run, chưa xác minh GPU/Windows trên máy khác; kết quả build không thay thế kiểm tra gameplay sau khi mở game.

Lần build đầu có hai cảnh báo và đã sửa trước lần build cuối:

- `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader`: đổi pragma cũ `enable_d3d11_debug_symbols` sang `enable_debug_symbols` theo cảnh báo compiler Unity hiện tại.
- Bật **Read/Write** bằng ModelImporter cho đúng hai file đang được Particle System Shape tham chiếu: `Assets/The Archer/Models/Enemies/Cauldron/enemy_cauldron_attack.fbx` và `Assets/The Archer/Models/Enemies/Mole/enemy_mole.fbx`. Giữ GUID, mesh và animation event. Không bật Read/Write toàn bộ model vì cần thêm bộ nhớ CPU.

## 3. Chuẩn bị trước mỗi lần build

1. Mở project này bằng **Unity 6000.6.2f1** trong Unity Hub. Xem phiên bản chuẩn ở `ProjectSettings/ProjectVersion.txt`; không nâng Unity/package chỉ để build lại.
2. Trong Unity Hub → **Installs** → menu của phiên bản Unity → **Add modules**, kiểm tra hỗ trợ Windows. Máy đã tạo bản này có module Windows và hỗ trợ Mono. Nếu chọn IL2CPP sau này, cần thêm Windows Build Support (IL2CPP) và bộ C++/Windows SDK tương ứng; đây là cấu hình khác, chưa được xác minh trong bản hướng dẫn này.
3. Trong Editor, thoát Play Mode, đợi import và biên dịch kết thúc.
4. Mở **Window → General → Console**. Sửa lỗi đỏ hiện tại trước. Không coi thông báo cũ từ lần build trước là kết quả của lần build mới; đọc timestamp và báo cáo cuối.
5. Lưu scene/prefab đang chỉnh bằng **Ctrl+S**. Đảm bảo những thay đổi muốn đưa vào game đã được lưu vào asset, không chỉ tồn tại trong Play Mode.
6. Kiểm tra `Assets/The Archer/Scriptables/Testing Database.asset`: các preset có **Enabled In Build = false** nếu không chủ ý dùng preset thử nghiệm.
7. Kiểm tra `Assets/The Archer/Scriptables/Developer Tools Settings.asset`: bản hiện tại có `allowInDevelopmentBuilds = false`, `allowInReleaseBuilds = false`. Nút DEV vì vậy ẩn và dịch vụ cấp tài nguyên bị chặn trong bản này. Muốn bản QA có DEV, bật đúng quyền cho loại build; thao tác DEV cấp tài nguyên thật vào hồ sơ của bản game.

## 4. Chọn platform và thứ tự scene

1. Mở **File → Build Profiles**.
2. Trong **Platforms**, chọn **Windows**. Nếu đang ở Android/iOS, bấm **Switch Platform** và đợi import xong. Không chọn Dedicated Server.
3. Chọn kiến trúc **Intel 64-bit / x86_64**; target cần là `StandaloneWindows64`.
4. Dùng **Scene List** dùng chung của dự án. Bật đúng ba scene theo thứ tự sau:

| Index | Scene |
| --- | --- |
| 0 | `Assets/The Archer/Scenes/Main Menu.unity` |
| 1 | `Assets/The Archer/Scenes/Loading Screen.unity` |
| 2 | `Assets/The Archer/Scenes/Game.unity` |

5. Nếu tạo Build Profile có override scene list, kiểm tra danh sách override cũng đúng thứ tự này. Dự án tại thời điểm build chưa có Build Profile riêng; bản vừa tạo dùng scene list chung trong `ProjectSettings/EditorBuildSettings.asset`.

Main Menu là bootstrap tạo Save/Currency/Armory và các dịch vụ. Không chỉ đưa Game scene vào bản build. `Default Scene Loader` trong Tools chỉ phục vụ Play của Editor, không thay scene list của bản player.

## 5. Cấu hình Player Settings

Trong cửa sổ Build Profiles, mở **Player Settings** của Windows:

1. Giữ **Product Name = The Archer By MrX**, **Company Name = DefaultCompany** cho bản đang dùng. Đổi tên công ty có thể làm đổi thư mục save; nếu muốn đổi thương hiệu ứng dụng, xử lý migration save riêng trước.
2. **Version** hiện là `1.2.2`. Tăng có chủ ý khi phát hành phiên bản mới; không đổi ID nội dung stage/item để đánh phiên bản build.
3. **Other Settings → Configuration → Scripting Backend: Mono**. API và stripping giữ cấu hình hiện tại. Đổi sang IL2CPP cần build/kiểm tra riêng, không mặc định coi kết quả Mono áp dụng cho IL2CPP.
4. **Resolution and Presentation**: mặc định dự án là 1920×1080, Fullscreen Window. Có thể bật **Resizable Window** khi muốn người chơi thay kích thước cửa sổ; bản vừa tạo giữ giá trị đang có là tắt. UI vẫn chọn bố cục theo kích thước màn hình thực.
5. **Active Input Handling** giữ New Input System của dự án. Không chuyển về input cũ để khắc phục phím điều khiển.
6. Giữ URP hiện tại trong Graphics/Quality. Không chạy Pipeline Switcher để build Windows.

## 6. Xuất bản game ra thư mục

1. Quay về **File → Build Profiles → Windows**.
2. Bản chơi thông thường: tắt **Development Build**, **Script Debugging**, **Autoconnect Profiler** và **Deep Profiling**. Khi cần điều tra lỗi bằng Profiler mới bật Development và các tùy chọn cần thiết.
3. Nếu có lựa chọn **Compression Method**, dùng **LZ4** như bản đã kiểm tra.
4. Bấm **Build**. Dùng **Build And Run** nếu muốn Unity tự mở game sau khi build.
5. Chọn thư mục đầu ra **ngoài Assets**, ví dụ `Builds/Windows/PC-Playtest-20261021-01/`, và tên executable `The Archer By MrX.exe`. Dùng thư mục mới cho mỗi bản muốn giữ để so sánh.
6. Đợi Unity hoàn tất. Lần đầu có thể lâu vì phải biên dịch shader/đóng gói tài nguyên. Không khởi động một build khác khi tiến trình còn chạy.
7. Kiểm tra Console và dòng kết quả **Build Succeeded**. Nếu thất bại, chưa dùng file `.exe` còn sót từ lần trước để kết luận bản mới chạy được.
8. Mở executable, thử menu, ngôn ngữ, các tab, cuộn túi/nâng cấp, Start, chết/clear, phần thưởng cuối run, thoát và mở lại. Khi phát hiện lỗi, giữ log của lần đó để đối chiếu.
9. Muốn gửi bản chơi: nén các file player trong thư mục đầu ra vào ZIP, loại thư mục `*_BackUpThisFolder_ButDontShipItWithYourGame`. Không gửi source/Library và không gửi mỗi `.exe`.

Strict Mode được dùng qua MCP ở lần xác minh này để cảnh báo cũng làm build thất bại; không phải mọi giao diện Build Profiles đều có checkbox tương ứng. Build bằng nút thông thường vẫn cần đọc và xử lý cảnh báo Console.

## 7. Tái hiện đúng lệnh MCP đã dùng

Khi Unity MCP đang kết nối, tool `manage_build` có thể nhận cấu hình sau. Đổi `output_path` sang thư mục bản mới nếu muốn giữ bản cũ:

```json
{
  "action": "build",
  "target": "windows64",
  "subtarget": "player",
  "output_path": "Builds/Windows/PC-Playtest-20261020-01/The Archer By MrX.exe",
  "development": "false",
  "options": "[\"compress_lz4\",\"detailed_report\",\"strict_mode\"]",
  "scenes": "[\"Assets/The Archer/Scenes/Main Menu.unity\",\"Assets/The Archer/Scenes/Loading Screen.unity\",\"Assets/The Archer/Scenes/Game.unity\"]"
}
```

Lưu asset trước; đợi job hoàn tất rồi đọc `manage_build` với `action = status`, `job_id` nhận được. MCP có thể timeout khi Editor bận build; timeout kết nối chưa có nghĩa build thất bại. Kết nối lại và đọc trạng thái job/BuildReport, không bấm build lặp ngay. Không cần MCP để build thủ công theo các bước phía trên.

## 8. Save và log khi chơi bản PC

Hiện tại Windows player dùng thư mục:

`C:/Users/Admin/AppData/LocalLow/DefaultCompany/The Archer By MrX/`

Trên máy khác, thay `Admin` bằng tài khoản Windows tương ứng.

- `game_save`: tiến trình của bản game độc lập.
- `game_save_editor`: tiến trình Play trong Unity Editor, tách tên với player.
- `Player.log`: log mặc định của bản player. Sau khi gặp lỗi, sao chép file này trước lần mở game tiếp theo để giữ log cần điều tra.
- Log build chi tiết của Editor trong phiên cấu hình hiện tại: `Logs/Editor.log`; khi Editor không chuyển log về project, xem `%LOCALAPPDATA%/Unity/Editor/Editor.log`.

Không xóa save người chơi để thử lỗi mặc định. Build lại vào thư mục mới không tự reset save vì save nằm ở LocalLow. Không tự đổi Company/Product Name để vô tình tạo một hồ sơ khác rồi tưởng save đã mất. Bản build này không sao chép save Editor vào player và chưa được chạy để sửa dữ liệu người chơi.

## 9. Lỗi thường gặp và nơi kiểm tra

| Hiện tượng | Kiểm tra / cách xử lý |
| --- | --- |
| Platform/module không được hỗ trợ | Cài module đúng cho Unity 6000.6.2f1 trong Hub; kiểm tra Mono hay IL2CPP đang chọn. |
| Lỗi `UnityEditor` trong player compilation | Source runtime chỉ được import/gọi API Editor bên trong `#if UNITY_EDITOR`; công cụ đặt trong thư mục `Editor`. Đừng thêm package Editor vào player để chữa tạm. |
| Không tìm thấy UnityPlayer.dll, Data hoặc runtime | Giải nén toàn bộ ZIP và giữ nguyên cấu trúc thư mục cạnh `.exe`. |
| Khởi động sai scene, thiếu manager | Scene 0 phải là Main Menu; kiểm tra cả scene list override của Build Profile. |
| Shader chữ báo pragma deprecated | Kiểm tra TMP_SDF-Mobile.shader đang dùng bản pragma đã sửa; tránh import gói TMP cũ đè lại. |
| Particle System báo mesh Read/Write | Tìm mesh được Shape Module tham chiếu, bật Read/Write đúng ModelImporter và Apply; xem hai asset đã sửa ở mục 2. |
| DEV vẫn hiện ở build phát hành | Kiểm tra `allowInReleaseBuilds`, Development Build và Developer Tools Settings thực sự được scene/prefab tham chiếu. |
| MCP timeout trong lúc build | Đọc lại status/BuildReport sau khi Editor rảnh; không coi lỗi HTTP là kết quả BuildPipeline. |
| Game mở rồi lỗi hoặc mất hiển thị | Giữ Player.log, ghi bước tái hiện và cấu hình máy; build thành công không bảo đảm mọi đường gameplay/GPU đều đã được thử. |

Hướng dẫn các hệ thống liên quan: [Menu ngang/dọc](Responsive%20Menu%20and%20Daily%20Updates.md), [Năng lượng](Energy%20and%20Gem%20Exchange.md), [Công cụ DEV](Developer%20Tools%20and%20Stage%20Preview.md), [Phần thưởng và save](Run%20Rewards%20and%20Save.md).
