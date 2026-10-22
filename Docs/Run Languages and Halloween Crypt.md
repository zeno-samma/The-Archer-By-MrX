# Ngôn ngữ trong run và Stage 4 — Halloween Crypt

Cập nhật: 20/10/2026. Các đường dẫn bên dưới tính từ thư mục gốc dự án.

## 1. Sử dụng ngôn ngữ trong run

1. Mở Main Menu, vào trang Stage và bấm nút chọn ngôn ngữ ở góc trên trái.
2. Chọn English, Tiếng Việt, Español, Português hoặc Français.
3. Bắt đầu run. Tên/mô tả kỹ năng, độ hiếm, tiêu đề lên cấp, nút đổi lựa chọn, giao kèo/phần thưởng rương và các nhãn tạm dừng dùng lựa chọn đã lưu.
4. Thẻ kỹ năng từ rương và thông báo tên kỹ năng trên nhân vật cũng dùng chung bảng dịch. Font tiếng Việt/Latin được đổi cùng chữ; chuyển lại English khôi phục font gốc.

`Language Settings` vẫn là khóa save hiện có. Dữ liệu kỹ năng giữ nguyên tên nguồn tiếng Anh, enum, level và giá trị gameplay. Phần dịch chỉ tác động nhãn hiển thị. Tên quái hoặc nội dung mới chưa có trong bảng có thể còn tiếng Anh.

## 2. Thêm hoặc chỉnh bản dịch

| Cần chỉnh | File |
| --- | --- |
| Bảng dịch đang dùng, chỉnh trực tiếp Inspector | `Assets/The Archer/Resources/Menu Translations.asset` |
| Nguồn bổ sung tên/mô tả 80 kỹ năng và nhãn trong run | `Assets/The Archer/Resources/Ability Translation Additions.txt` |
| Nguồn bổ sung các nhãn menu trước đây | `Assets/The Archer/Resources/Menu Translation Additions.txt` |
| Font SDF dùng cho chữ dịch | `Assets/The Archer/Resources/Menu Latin Font.asset` |
| Dịch, mã ngôn ngữ và lưu lựa chọn | `Assets/The Archer/Scripts/Localization/MenuTranslations.cs` |
| Binding nhãn/font và vòng đời sự kiện | `Assets/The Archer/Scripts/UI/Main Menu/LocalizedMenuScope.cs` |
| Lệnh import và bổ sung glyph vào atlas | `Assets/The Archer/Scripts/Editor/MenuEconomyAuthoring.cs` → `ImportTranslations()` |

Để thêm kỹ năng mới:

1. Cấu hình `AbilityData` trong hệ thống kỹ năng như bình thường. Giữ `Title` và `Description` bằng tiếng Anh làm nguồn.
2. Thêm hai dòng vào `Ability Translation Additions.txt`, một dòng cho tên và một dòng cho mô tả. Mỗi dòng có đúng năm cột: `English|Vietnamese|Spanish|Portuguese|French`.
3. Giữ nguyên số liệu, placeholder `{0}` và thẻ rich text nếu có. Không dùng dấu `|` bên trong nội dung; ký hiệu `\n` trong file được import thành xuống dòng.
4. Trong Edit Mode, chạy **Tools → MrX Studio → Localization → Import Additional Menu Translations**. Lệnh đọc cả hai file bổ sung, thêm entry còn thiếu và cập nhật glyph của font tĩnh.
5. Chạy Play, chọn ngôn ngữ và xem thẻ thật. Thử cả tên dài, mô tả dài, đổi lựa chọn và nhận lại thẻ từ rương.

Import giữ nguyên các entry đã có để bảo vệ chỉnh sửa trong Inspector. Vì vậy, sửa câu dịch hiện hữu phải sửa trong `Menu Translations.asset`; cập nhật cả dòng tương ứng trong file `.txt` để lần tạo bảng mới sau này cũng có cùng nội dung. Sửa `.txt` rồi import không ghi đè entry hiện hữu.

## 3. Prefab và các điểm binding

| Vai trò | File |
| --- | --- |
| Màn chọn kỹ năng, nút đổi lựa chọn | `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Selector.prefab` |
| Thẻ kỹ năng tái sử dụng | `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Card.prefab` |
| Rương giao kèo / hồi máu | `Assets/The Archer/Prefabs/UI/Game/Evil Chest UI.prefab`, `Assets/The Archer/Prefabs/UI/Game/Heal Chest UI.prefab` |
| Thẻ kỹ năng trong Armory | `Assets/The Archer/Prefabs/UI/Armory/Armory Ability Card.prefab` |
| Thông báo kỹ năng trên nhân vật | `Assets/The Archer/Prefabs/UI/Game/World Space Text/Ability Select Indicator.prefab` |
| Canvas gameplay, nhãn tạm dừng/mục tiêu/kết quả | `Assets/The Archer/Scenes/Game.unity` |
| Gán nội dung thẻ | `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs` |
| Độ hiếm và bề rộng nền độ hiếm | `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs` |
| Nội dung thẻ Armory | `Assets/The Archer/Scripts/UI/Armory/ArmoryAbilityCard.cs` |
| Đo thông báo theo chữ/font đã dịch | `Assets/The Archer/Scripts/UI/Text Indicators/AbilitySelectedIndicatorBehavior.cs` |

`LocalizedMenuScope` đã được gắn sẵn trong scene/prefab; runtime không tạo cây UI. Khi các scope lồng nhau, nhãn thuộc scope gần nhất để tránh dịch hai lần hoặc ghi nhớ câu đã dịch làm nguồn. Thẻ pool gán lại chữ nguồn trước khi gọi `RefreshNow()`. Thông báo kỹ năng dịch trước khi đo TMP để nền vừa với tên mới. Nút đổi lựa chọn tự thu nhỏ chữ và không xuống dòng vào vùng giá.

## 4. Chỉnh Stage 4 Halloween

Stage 4 đổi tên hiển thị thành **Halloween Crypt** (tiếng Việt: **Hầm mộ Halloween**). Khu giữa dành cho chiến đấu; lối đá dẫn tới nhà mộ phía bắc. Hai biên có hàng rào, cột đá, quan tài, bia mộ, nến đầu lâu, bí ngô và cây chết. Model và mesh dùng từ tài nguyên hiện có.

| Nội dung | File |
| --- | --- |
| Dữ liệu Stage 4 | `Assets/The Archer/Scriptables/Stages/Stage Data 004.asset` |
| Môi trường riêng, mở Prefab Mode để chỉnh | `Assets/The Archer/Prefabs/Prop/Halloween/Halloween Crypt Arena.prefab` |
| Các prefab Halloween nguồn | `Assets/The Archer/Prefabs/Prop/Halloween/` |
| Material sàn riêng | `Assets/The Archer/Materials/Environment/Halloween Crypt Stone.mat` |
| Material nền ngoài biên riêng | `Assets/The Archer/Materials/Environment/Halloween Burial Ground.mat` |
| Preview Stage 4 ở menu | `Assets/The Archer/Sprites/UI/Main Menu/ui_stage_4_preview.png` |

Prefab môi trường chia thành các nhóm có tên trong Hierarchy:

- `01 - Crypt Stone Floor`: sàn đi được, đường lát đá và nền ngoài biên.
- `02 - Arena Boundaries`: bốn collider tường biên.
- `03 - Crypt and Colonnade`: nhà mộ, cổng, cột và hàng rào.
- `04 - Burial Chambers`: quan tài, bia mộ và miếu.
- `05 - Pumpkins Candles and Lanterns`: bí ngô, nến, đèn và hai point light không đổ bóng.
- `06 - Dead Grove`: cây chết ngoài khu chiến đấu.

Để sửa bố cục:

1. Dừng Play. Mở prefab `Halloween Crypt Arena` trong Prefab Mode.
2. Di chuyển/thay model trong nhóm tương ứng rồi lưu prefab. Đây là prefab độc lập; không cần sửa `Temple Env Clone` đang dùng ở môi trường khác.
3. Giữ khu sinh quái hiện tại thông thoáng: X khoảng −5,11 đến 5,15; Z khoảng −8,81 đến 11,01. Điểm bắt đầu player là `(−0,13; 0; −5,52)`.
4. Collider sàn dùng layer/tag `Ground`; tường biên dùng layer `Obstacle`, tag `Wall`. Props trang trí đã tắt collider và `NavMeshObstacle`, đặc biệt các viên `path_A`, để không cắt đường đi ở giữa. Không bật lại chúng trên lối chiến đấu nếu chưa kiểm tra NavMesh.
5. Nếu cần sửa wave hoặc phần thưởng, mở scene Game và **Tools → MrX Studio → Stage Creator**, chọn Stage 4/phòng đầu. Chỉnh đúng dữ liệu cần thay đổi và lưu qua công cụ. Đóng Stage Creator trước khi kiểm thử luồng Main Menu thông thường; công cụ có cơ chế ghi testing stage/room/wave khi Play.
6. Thử run Stage 4, đi tới bốn biên, kiểm tra enemy/boss/rương và camera. Nếu bố cục đổi nhiều, chụp lại preview và thay PNG trên, giữ file `.meta` để giữ reference.

Stage 4 vẫn có **một phòng, 33 wave**, cùng ID `98f43066-55f0-4c56-aa7e-661ad6981943`. Lần thay môi trường này giữ nguyên toàn bộ dữ liệu wave, boss/loot override, vị trí sinh, camera confinement, spawn player và điều kiện mở khóa. Prefab được đưa vào Stage bằng handler của Stage Creator. Nền ngoài biên chỉ để hiển thị, không mở rộng NavMesh đi được.

## 5. Xác minh và giới hạn

- Compilation và Play Mode qua Unity MCP; không chạy build.
- 80 kỹ năng × 5 ngôn ngữ × tên/mô tả = **800 lần kiểm tra binding** trên thẻ thật được tái sử dụng; kiểm tra entry, ký tự font và tràn khung TMP.
- **105/105 điểm sinh quái** của 33 wave có NavMesh và đường đi đầy đủ tới điểm bắt đầu player; các điểm rương cấu hình cũng nằm trên sàn đi được.
- Đã chọn kỹ năng thật, kiểm tra thông báo tiếng Pháp, mở lại selector và xem trực quan tiếng Việt/Pháp.
- Kiểm tra vòng đời scope: binding khi tắt không đăng ký listener, bật lại nhận đúng tiếng Pháp, chỉ một subscription và tắt lại hủy sạch. Chuyển về English được kiểm tra cùng lượt.
- Prefab môi trường có 126 renderer dùng mesh có sẵn; chỉ 5 collider hoạt động (sàn và bốn biên), không có NavMeshObstacle trang trí đang bật. Hai point light không đổ bóng. Đây là cấu hình hiện tại, không phải số đo FPS trên phần cứng đích.
- So sánh dữ liệu trước/sau: toàn bộ wave và thiết lập phòng từ khối `waves` trở xuống không đổi.
- Dùng profile thử riêng qua `VerificationSession`; khi kết thúc, hash save thật không đổi. Không xóa save người chơi.
- Console ở lượt Play kiểm tra cuối không có error hoặc warning. Trong một số lần reconnect trước đó có warning từ WebSocket của plugin MCP; không phát hiện exception gameplay trong các lượt thử này.

Đây là kiểm tra đường đi, UI và smoke test gameplay, chưa phải chơi thắng đủ 33 wave, đo FPS trên mọi thiết bị hoặc xác nhận mọi chuỗi trong game đã được dịch. Khi thêm kỹ năng/wave/đồ trang trí mới, cần kiểm tra lại phần tương ứng. Chỉ build khi người dùng yêu cầu rõ ràng.
