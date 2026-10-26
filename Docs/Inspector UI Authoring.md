# Chỉnh giao diện trực tiếp trong Inspector

Cập nhật 26/10/2026. Phạm vi: source chính trong `Assets/The Archer`, các prefab UI và scene Main Menu. Không build.

## Chỉnh một trang menu

1. Dừng Play Mode. Mở `Assets/The Archer/Scenes/Main Menu.unity` hoặc double click prefab cần sửa.
2. Chọn object gốc của trang, tìm component **Responsive Menu Layout**.
3. Bấm **Edit Landscape** để chỉnh ngang hoặc **Edit Portrait** để chỉnh dọc. Bật **Auto Capture Inspector**.
4. Chọn phần tử con trong Hierarchy, sửa RectTransform, font TMP hoặc Pixels Per Unit Multiplier của Image như bình thường. Thay đổi được ghi vào profile đang chọn. Không cần sửa tọa độ trong source.
5. Ctrl+Z hoàn tác cả phần tử và giá trị tương ứng trong profile. Thử cả hai hướng trước khi lưu.
6. Lưu prefab/scene. Khi sửa scene instance, thay đổi là override của instance đó; Apply override về prefab nếu muốn tái sử dụng ở nơi khác.

Inspector của object con có dòng **Ngang/Dọc · Auto Capture** và nút **Layout Settings** để quay về trang quản lý. Nếu tắt Auto Capture, phải bấm **Capture Current Layout** sau khi sửa. Các phần tử mới muốn có bố cục riêng theo hướng cần được thêm vào danh sách tương ứng của Responsive Menu Layout trong Inspector.

Nếu RectTransform ghi “driven by LayoutGroup”, chỉnh Padding/Spacing/Child Control/Preferred Height trong LayoutGroup hoặc LayoutElement đang quản lý nó. Đây là cơ chế bố cục có sẵn của Unity. Riêng khung Hero Raw Image đã gỡ AspectRatioFitter để có thể sửa rộng/cao trực tiếp.

Các thay đổi trên object đang chạy trong Play Mode là tạm thời theo cơ chế Unity, không tự lưu về prefab. Chỉnh asset ở Edit Mode để giữ kết quả lâu dài.

## Xem nhân vật khi chưa Play

1. Mở Stage Page hoặc Armory Page, chọn `Hero Panel/Hero` có component **Hero Preview Behavior**.
2. Chọn **Editor Hero**, **Editor Weapon**; để trống sẽ dùng hero/vũ khí mặc định tương thích. Những trường này không chọn hero hoặc mở khóa trong save người chơi.
3. Sửa kích thước **Hero Raw Image**. Chọn **Render Height** để đổi độ phân giải hình render; kích thước khung nằm ở RectTransform.
4. Bấm **Refresh Editor Preview**. Ảnh nền trong suốt được tạo từ model, vũ khí và tư thế idle thật. Tên, chân dung, EXP ban đầu, chỉ số mẫu và nhãn Stage 1 cũng được cập nhật mà không chạy game.
5. Đổi ánh sáng preview bằng **Editor Key/Fill Light Rotation**, **Intensity**, **Editor Ambient Color** rồi Refresh.
6. Muốn đổi góc camera, bấm **Open Preview Camera Prefab**, sửa Camera trong `Assets/The Archer/Prefabs/UI/Armory/Hero Preview.prefab`, lưu rồi Refresh trên trang.

Lệnh **Tools → MrX Studio → UI → Refresh Menu Editor Previews** làm mới các preview trong scene đang mở. Khi thay đổi tỷ lệ khung, hãy Refresh lại ảnh Editor. Trong Play, render texture tự bám tỷ lệ khung để tránh kéo méo model.

Ảnh sinh tại `Assets/The Archer/Sprites/UI/Editor Previews`. Preview có tên theo trang/hero/vũ khí/kích thước; chỉnh preview một trang không dùng chung file với trang có tên khác. Ảnh Editor chỉ là mẫu cấp 1; dữ liệu thật được bind khi Play.

## Chỗ chỉnh từng phần

| Phần | Prefab / asset | Chỗ chỉnh |
| --- | --- | --- |
| Stage | `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` | Image/TMP/RectTransform và ResponsiveMenuLayout |
| Armory | `Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab` | Image/TMP/RectTransform, ArmoryDashboard và ResponsiveMenuLayout |
| Upgrades | `Assets/The Archer/Prefabs/UI/Menu Pages/Upgrades Page.prefab` | UpgradeDashboardPage: Selection And Level Presentation; nội dung card được vẽ sẵn |
| Nhiệm vụ | `Assets/The Archer/Prefabs/UI/Menu Pages/Daily Quests Page.prefab` | DailyQuestsPageBehavior: Reward State Presentation |
| Thành tựu | `Assets/The Archer/Prefabs/UI/Menu Pages/Achievements Page.prefab` | AchievementsPageBehavior: State Presentation |
| Rèn | `Assets/The Archer/Prefabs/UI/Menu Pages/Forge Page.prefab` | ForgePageBehavior: Crafting State Presentation; Stat Comparison dùng VerticalLayoutGroup |
| Dock | `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab` | Button/Image nhận raycast; icon và TMP chỉ trang trí |
| Túi đồ | `Assets/The Archer/Resources/Armory Dashboard Database.asset` và Armory Page | Database quản lý sprite theo trạng thái/rarity; ArmoryDashboard quản lý Cell Gap, Empty Icon/Plus Bounds, Empty Plus Color |
| Vòng quay | `Assets/The Archer/Prefabs/UI/Lucky Rewards/Wheel Slice.prefab` | LuckyWheelSlice: Even/Odd Sector Color, Icon Radius; icon, font, kích thước trong prefab |
| Rương / vòng quay | `Assets/The Archer/Prefabs/UI/Lucky Rewards/Gem Chest Popup.prefab`, `Lucky Wheel Popup.prefab` | LuckyRewardsPopup: Chest Pulse Amplitude/Cycles; giữ scale gốc của ảnh rương sau hiệu ứng và khi đóng giữa chừng |
| Chọn độ khó | `Assets/The Archer/Prefabs/UI/Offline Progression/Stage Difficulty Selector.prefab` | StageDifficultySelector: Difficulty State Colors |
| Chọn ngôn ngữ | Stage Page → Language Picker | LanguagePicker: Selected Color, Idle Color |

Màu đổi theo trạng thái phải chỉnh ở trường trạng thái của component tương ứng; đổi riêng màu Image sẽ bị trạng thái hiện tại bind lại. Icon trang bị, chữ bản dịch, số tiền, XP, thanh tiến trình, danh sách vật phẩm và animation vẫn do dữ liệu gameplay điều khiển. Thông số mỹ thuật không cần sửa source để đổi.

Túi dùng các ô đã vẽ sẵn và tái sử dụng khi cuộn. Runtime vẫn tính vị trí ô theo hàng/cột và độ rộng viewport để tránh dựng hàng trăm ô. Khung icon và vị trí nhãn của ô có vật phẩm lấy từ prefab; không đặt lại về tọa độ cố định mỗi lần cuộn. Ô trống dùng các bounds Inspector riêng.

## Công cụ authoring và kiểm kê

- **Apply Approved Rep Menu** / **Apply Rep Remaining Pages and Repair Dock** là lệnh áp lại thiết kế mẫu. Không chạy chúng sau mỗi lần sửa tay: chúng có thể ghi đè mỹ thuật. Runtime không gọi chúng để dựng giao diện.
- Các hàm Build/Create trong bốn trang chỉ thuộc Editor và chặn chạy khi Play. Chúng dùng để tạo asset ban đầu; prefab đã lưu là nguồn chỉnh sửa.
- **Tools → MrX Studio → UI → Audit Inspector Authoring** chỉ đọc prefab/source, ghi `Docs/UI Inspector Asset Audit.md`. Không reset save hoặc build.
- Source công cụ: `Assets/The Archer/Scripts/Editor/UIInspectorAuthoringAudit.cs`, `HeroMenuPreviewAuthoring.cs`, `ResponsiveMenuLayoutEditor.cs`.

## Kết quả kiểm tra của đợt sửa

Đã quét 674 file C# và đọc 620 prefab trong `Assets/The Archer`; 86 prefab có RectTransform, không thấy component script bị thiếu. Không thấy dấu hiệu dựng UI runtime theo nhóm API được công cụ kiểm tra. Đây là kiểm tra tĩnh, không thay thế chạy từng nội dung của toàn bộ game.

Đã kiểm tra chỉnh khung ngang/dọc, profile hướng còn lại không đổi và Undo khôi phục profile. Trong Play đã mở sáu tab, cuộn túi, chọn Mage/Barbarian trên profile thử, kiểm tra preview khi đổi khung từ 1260×1260 sang 1500×1260: render texture đổi từ 512×512 sang 610×512. Đã kiểm tra Game View 3840×2160 và 1080×1920; vùng bấm dock đạt 18/18 ở cả hai hướng. Console của các lượt thử này không có error/warning. Save thật giữ nguyên hash sau khi kết thúc phiên thử.

Các thông báo Unity/MCP hoặc cảnh báo source cũ trong giai đoạn reload được phân biệt với Console Play. Không cam kết toàn bộ Editor hoặc mọi thiết bị không bao giờ phát sinh lỗi. Không chạy build.

Kiểm tra bổ sung sau đợt rà soát: TMP font và Image PPU chỉnh qua Undo/Inspector được profile giữ lại khi Apply; ô chỉ số rèn dùng LayoutGroup và ba hàng đang hiển thị có chiều cao bằng nhau; vòng quay hiển thị năm sector được instance từ prefab đã stretch; chạy hiệu ứng rương với scale mẫu 1.25×0.8 rồi đóng vẫn trả về đúng scale đó. Phiên thử bổ sung không có error/warning Console và hash save thật không đổi.
