# Giao diện Rep — các tab menu

## Chỉnh Inspector — cập nhật 26/10/2026

Quy trình mới: chọn **Edit Landscape/Edit Portrait** và bật **Auto Capture Inspector** trên ResponsiveMenuLayout, rồi chỉnh trực tiếp object con; profile tương ứng được cập nhật tự động, có Undo. Hero Raw Image đã bỏ AspectRatioFitter, có nút **Refresh Editor Preview** để xem model/vũ khí thật trước Play. Hướng dẫn đầy đủ, nơi chỉnh màu trạng thái và bản đồ prefab tại [Inspector UI Authoring](Inspector%20UI%20Authoring.md). Các lệnh Apply Rep vẫn là áp lại mẫu và có thể ghi đè chỉnh tay.

## Sửa co giãn sprite — 23/10/2026

Các khung/nút Rep trước đây dùng đường chia viền tự động theo 22% cạnh ngắn; đường chia cắt qua góc trang trí nên kéo lớn làm mép vàng/xanh bị giãn. Đã đặt lại 9-slice riêng theo từng họ ảnh, bảo vệ trọn góc và mép. Các surface được import tối đa 2048, không nén texture để tránh vết khối ở cạnh; icon giữ cấu hình riêng. Không sửa pixel ảnh gốc hoặc ID/GUID của sprite.

- Khung trang: độ dày góc/viền được giảm, cấu hình trong `ResponsiveMenuLayout.images` (ngang 8, dọc 12).
- Dock: nền và ô đang chọn dùng `CardFrame04_White1` có tint, không kéo ảnh nút dài thành toàn thanh dock. Image nhận bấm trên Button vẫn giữ nguyên.
- Scrollbar: thay hình nút ngang bằng khung bo góc đơn giản, phù hợp thanh dọc.
- Tái áp riêng sửa co giãn, giữ bố cục hiện tại: **Tools → MrX Studio → UI → Repair Rep Sprite Scaling**. Công cụ lưu prefab UI đang dùng Rep và scene hiện tại, chạy trong Edit Mode với Main Menu mở. Source: `Assets/The Archer/Scripts/Editor/RepSpriteScalingAuthoring.cs`.
- Công cụ Apply Rep hiện có gọi lại chính sách độ dày này khi hoàn tất, tránh tái tạo cấu hình lỗi cũ. Không dùng Simple cho khung cần giãn; icon vẫn dùng Simple + Preserve Aspect.

Đã xem ảnh chụp Game View 3840×2160, 1920×1080 và 1080×1920; kiểm tra raycast đủ 18/18 ở cả ba kích thước và chuyển sáu tab qua pointer. Lượt Play kiểm tra cuối không có warning/error, save thật giữ nguyên hash. Có lỗi vòng đời khi Editor reload ở lượt Play đầu; dừng và khởi động lại sau import/compile đã hoàn tất thì không tái hiện. Chưa kiểm tra thiết bị thật, không build.

Ảnh so sánh sau sửa: `Docs/Design Previews/Rep UI/Scaling Review.png`; ảnh chi tiết có tên `Scaling Upgrades 4K.png`, `Scaling Forge 4K.png`, `Scaling Quests 4K.png`, `Scaling Achievements 4K.png`, `Scaling Armory 4K.png`, `Scaling Stage 1080p.png`, `Scaling Upgrades Portrait.png`. Giá trị ví trong ảnh là profile kiểm tra riêng.

## Bổ sung: bốn tab còn lại và sửa vùng bấm

Ngày 23/10/2026 đã áp tiếp giao diện Rep cho **Upgrades, Forge, Daily Quests, Achievements**, gồm bố cục ngang/dọc, header tiền tệ, bảng chỉ số trước/sau, nguyên liệu rèn, mốc thưởng ngày, nhóm thành tựu và danh sách cuộn.

Các prefab tương ứng nằm ở `Assets/The Archer/Prefabs/UI/Menu Pages/`: `Upgrades Page.prefab`, `Forge Page.prefab`, `Daily Quests Page.prefab`, `Achievements Page.prefab`. Công cụ authoring: **Tools → MrX Studio → UI → Apply Rep Remaining Pages and Repair Dock**, source `Assets/The Archer/Scripts/Editor/RepMenuPagesAuthoring.cs`. Công cụ lưu bốn prefab cùng override trình bày trong Main Menu, cập nhật sprite nút ở các database và sửa vùng bấm của dock; không reset tiến trình hoặc build.

Lỗi chỉ bấm được Upgrades là do bước thay icon đặt `raycastTarget=false` trên năm icon; các ô tab chưa có Graphic riêng để nhận chuột. Upgrades còn icon cũ nên vẫn nhận. Hiện mỗi ô tab có **Image trong suốt trên chính GameObject chứa Button**, nhận bấm trên toàn ô. Icon, nhãn và viền là trang trí; không cần bật raycast lên chúng. Button.targetGraphic trỏ về Image này. Tránh xóa Image vì màu alpha 0 không có nghĩa nó không cần thiết.

Kiểm tra lại trong Play Mode: đóng popup rồi chạy **Tools → MrX Studio → UI → Verify Menu Pointer Targets (Play Mode)**. Source ở `Scripts/Editor/RepMenuInputVerification.cs`; công cụ đọc GraphicRaycaster tại 18 tọa độ (icon, nhãn và phần trống của cả sáu ô), báo phần tử nào đang chắn nếu sai. Nó không tự nhận thưởng hoặc ghi save. Kiểm tra qua `onClick.Invoke` đơn thuần không phát hiện được lỗi vùng bấm này.

Đã kiểm tra thực tế qua EventSystem theo tọa độ màn hình: chuyển sáu tab; mua nâng cấp trừ 100 gold; nhận nhiệm vụ +100 gold rồi chuyển Thành tựu; nhận thành tựu +100 gold và bấm lại không cộng trùng; rèn bị khóa khi thiếu nguyên liệu, rèn đủ điều kiện mở một trang bị; nút đổi công thức hoạt động. Các giao dịch này chỉ ở profile thử. Font dùng Truncate thay vì Ellipsis ở các nhãn không có glyph dấu ba chấm. Thẻ thành tựu được clone giữ listener do UIButtonAudio sở hữu.

Ảnh chụp mới: `Docs/Design Previews/Rep UI/Upgrades Updated Runtime.png`, `Forge Updated Runtime.png`, `DailyQuests Updated Runtime.png`, `Achievements Updated Runtime.png`; bản dọc có hậu tố `Portrait.png` tương ứng. Các lần kiểm tra Game View dùng 1920×1080 và 1080×1920; chưa thay thế kiểm tra trên thiết bị thật.

---

Cập nhật: 23/10/2026. Triển khai theo hai ảnh người dùng đã duyệt trong `Docs/Design Previews/Rep UI/Stage Concept v1.png` và `Armory Concept v1.png`.

## Những phần đã thay

- Stage: nhân vật lớn bên trái, bốn ô trang bị hai bên, ATK/HP bên dưới; vòng quay và rương miễn phí phía dưới. Bảng hành trình bên phải có ảnh màn, chọn độ khó, phần thưởng và nút bắt đầu.
- Armory: giữ nhân vật lớn, dãy đổi hero, bốn chỉ số và thưởng chỉ số; túi đồ có số ô, mở rộng, lọc, thanh cuộn, cấp trang bị và số lượng nguyên liệu/mảnh.
- Header: chân dung, cấp hero, EXP, tiền tệ thật. Thể lực vẫn tuân theo công tắc tính năng hiện có; khi tắt sẽ không ép hiện lại.
- Dock: sáu tab, nhãn luôn hiện, nền xanh cho tab đang chọn. Chấm đỏ vẫn lấy trạng thái nhận thưởng từ manager.
- Bố cục dọc riêng: hồ sơ/tiền ở trên, hero ở giữa, nội dung xếp dưới. Túi dọc bốn cột, ngang sáu cột. Giao diện giữ safe area qua `ResponsiveMenuSurface`.

Tất cả hình, TMP, nút, mask và scrollbar nằm trong prefab/scene. Script runtime chỉ bind dữ liệu, chọn sprite, điều khiển trạng thái và áp profile bố cục; không tạo cây UI mới.

## Mở và sửa trong Unity

1. Mở scene `Assets/The Archer/Scenes/Main Menu.unity`.
2. Mở prefab cần sửa trong Project (double click):

| Thành phần | Đường dẫn |
| --- | --- |
| Trang Stage | `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` |
| Trang Armory | `Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab` |
| Dock sáu tab | `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab` |
| Thẻ trang bị trên hero | `Assets/The Archer/Prefabs/UI/Armory/Item Card.prefab` |

3. Các node quan trọng: `Player Profile`, `Stage Showcase Card`, `Journey Heading`, `Stage Panel`, `Rewards`, `Stage Difficulty Selector`, `Info Panel`, `Armory Panel`, `Hero Selector` và `Settings Shortcut`.
4. Chọn Edit Landscape/Portrait trên `ResponsiveMenuLayout`, bật Auto Capture Inspector rồi chỉnh ảnh/màu trong Image, chữ trong TMP và RectTransform trực tiếp. Chỉnh lần lượt cả hai hướng. Khi tắt Auto Capture, cần Capture Current Layout trước khi Play để profile không ghi đè thay đổi chưa lưu.
5. Có thể dùng `ResponsiveMenuLayout.Apply(false)` để xem ngang và `Apply(true)` để xem dọc qua công cụ Editor. Trong Play, Game View quyết định hướng; không chỉnh prefab khi đang Play.
6. Khi sửa scene instance, chú ý prefab override. Hai trang đã được đồng bộ cả prefab và override trình bày trong Main Menu.

## Sprite, khung và dữ liệu

- Nguồn ảnh giữ tại `Assets/The Archer/Sprites/UI/Rep`. Chỉ các ảnh được dùng được nhập thành Sprite; các ảnh khác không bị đổi hàng loạt.
- Sprite dùng một vùng cắt alpha trong metadata, không ghi đè pixel PNG gốc. Frame dùng border để co giãn, tắt mipmap, giới hạn kích thước nhập. Không đổi tên hoặc làm mất file `.meta`.
- Khung nền xanh/kem và vàng được ghép từ sprite Rep có sẵn. Hero/model/animation và icon vật phẩm dùng nguồn thật của game, vì vậy không phải hình vẽ nhân vật giả lập trong concept.
- Stage 1 dùng tạm tranh nền phong cảnh có sẵn cho banner rộng. Đổi ảnh thật về sau tại **Stage Data 001 → Stage Image** (`Assets/The Archer/Scriptables/Stages/Stage Data 001.asset`). Ảnh cũ vẫn còn tại `Sprites/UI/Main Menu/ui_stage_1_preview.png`.
- Chọn asset `Assets/The Archer/Resources/Armory Dashboard Database.asset`: `Equipment Frame`, `Stack Frame`, `Empty Frame`, `Empty Plus`, `Rarity Frames` quản lý hình túi. Thứ tự rarity: Common, Uncommon, Rare, Mystic, Legendary, Mythic. Giá và số ô mở rộng vẫn dùng cấu hình cũ, không hardcode trong view.
- `HeroExperienceBar` có tùy chọn `Use Face Crop` cùng bounds cho sprite hero cao và vuông. Bounds này chỉ đổi khung hiển thị chân dung, không sửa asset gốc.
- Nhãn FREE và chấm đỏ của shortcut chỉ hiện khi còn lượt miễn phí. Đã thêm bản dịch FREE cho EN/VI/ES/PT/FR trong Menu Translations.

## Công cụ khôi phục bố cục đã duyệt

**Tools → MrX Studio → UI → Apply Approved Rep Menu** gọi:

`Assets/The Archer/Scripts/Editor/RepMenuAuthoring.cs`.

Đây là công cụ authoring trong Edit Mode, không phải script dựng UI lúc chạy. Nó lưu prefab, cập nhật override trình bày của scene đang mở, gán sprite khung và ảnh banner Stage 1. Chỉ chạy lại khi muốn áp lại bố cục Rep đã duyệt; nếu đang tự chỉnh mỹ thuật, hãy lưu thay đổi trước vì công cụ sẽ áp lại các giá trị bố cục nó quản lý. Nó không reset save, mở hero, cấp tiền hoặc build game.

Các điểm runtime liên quan:

| File trong `Assets/The Archer/Scripts` | Trách nhiệm |
| --- | --- |
| `UI/ResponsiveMenuLayout.cs` | Rect/font/độ co viền Image theo ngang và dọc |
| `UI/HeroExperienceBar.cs` | Hero, cấp, EXP và khuôn chân dung |
| `UI/Armory/ArmoryDashboard.cs` | Bind chỉ số, virtualized inventory, stack, lọc, mở túi và badge hero |
| `UI/Armory/ArmoryDashboardDatabase.cs` | Art và cấu hình túi |
| `UI/Armory/Items/ItemCardBehavior.cs` | Tiền tố cấp của thẻ trang bị |
| `UI/Main Menu/MainMenuDockItemBehavior.cs` | Highlight tab và nhãn tab chưa chọn |
| `UI/Main Menu/StageDifficultySelector.cs` | Sprite độ khó đã chọn/chưa chọn và nhãn gọn |
| `UI/Main Menu/Lucky Rewards/StageLuckyRewards.cs` | Chấm đỏ và nhãn lượt miễn phí |
| `UI/Main Menu/MenuSettingsShortcut.cs` | Mở cài đặt từ Armory, khóa/khôi phục dock và focus |
| `UI/UIButtonAudio.cs` | Âm nút Default/Custom/Silent, dùng lại hệ thống hiện có |

## Xác minh

Dùng profile riêng trong `Temp/Verification`; không reset hoặc cấp vật phẩm vào save thật. Đã chạy kiểm tra:

- Compilation; hiển thị Game View 1920×1080 và 1080×1920.
- Chuyển Stage/Armory, đổi EN/VI/FR/PT/ES không phát sinh cảnh báo thiếu glyph trong các view đã chạy.
- Scroll wheel làm vị trí túi đổi từ 0 lên 160; mở lại túi bắt đầu ở hàng đầu. Mảnh/wood/iron/leather giữ một stack cùng số lượng.
- Chi tiết chỉ số; mở rộng túi từ 40 lên 50 và trừ đúng 100 gold theo cấu hình profile thử.
- Mở khóa Mage trừ 500 gem, chọn Mage cập nhật mô hình, chỉ số, chân dung và dấu chọn roster.
- Mở/đóng Settings khóa và khôi phục dock.
- Quay miễn phí không trừ gem; nhãn FREE và chấm đỏ của vòng quay cùng tắt sau khi dùng.
- Console trong các lượt Play kiểm tra không có error/warning. Kiểm tra hash xác nhận save thật không thay đổi khi kết thúc phiên.

Ảnh chụp thực tế nằm cạnh hai ảnh concept trong `Docs/Design Previews/Rep UI`: `Stage Runtime.png`, `Armory Runtime.png`, `Stage Portrait.png`, `Armory Portrait.png`. Số liệu trong ảnh kiểm tra có thể là dữ liệu tạm của profile thử.

Không build. Kiểm tra Game View không thay thế thử trên máy điện thoại hoặc tay cầm thật; không cam kết mọi tỉ lệ thiết bị chỉ từ hai độ phân giải đã kiểm tra.

### Thông báo Editor khi nạp lại script

Compilation cuối thành công (`scriptCompilationFailed=false`), scene đã lưu và không còn dirty. Trong lúc Unity nạp lại script có ghi nhận cảnh báo cũ CS0414 ở `RunSummaryPresenter.bound`, thông báo kết nối WebSocket của MCP và assert nội bộ `BeginImportAsyncTask called while OndemandScheduler was stopped`. Các thông báo này thuộc giai đoạn Editor/reload, không xuất hiện trong các luồng Play UI được kiểm tra ở trên. Chưa sửa package MCP hay Unity để xử lý chúng; không diễn giải kết quả UI thành cam kết toàn bộ Editor không bao giờ có thông báo.
