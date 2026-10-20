# Giao diện menu và cách chỉnh prefab

Các trang mới được lưu thành GameObject/uGUI/TMP trong prefab và được đặt trực tiếp trong scene `Main Menu`. Script runtime chỉ nối sự kiện, hiển thị dữ liệu và thay đổi trạng thái tương tác; không dựng cây giao diện khi mở trang.

## Nơi chỉnh giao diện

Tất cả nằm trong `Assets/The Archer/Prefabs/UI/Menu Pages/`:

| Prefab | Nội dung |
| --- | --- |
| `Upgrades Page.prefab` | Chi tiết nâng cấp bên trái, bảng so sánh chỉ số, danh sách 9 nâng cấp bên phải |
| `Armory Page.prefab` | Bảng Stats có sẵn, hero thật, inventory, bộ lọc, chọn hero, hộp thoại Details/mở rộng túi |
| `Forge Page.prefab` | Chọn công thức, mảnh riêng, bốn nguyên liệu và trang bị đầu ra |
| `Achievements Page.prefab` | Danh mục, tiến độ và các hàng thành tựu |
| `Daily Quests Page.prefab` | Nhiệm vụ, mốc thưởng và thời gian reset |
| `Menu Dock.prefab` | Sáu tab đang được game hỗ trợ |

Double-click prefab trong Project để mở Prefab Mode. Các Canvas trong asset được bật và có alpha 1 để xem ngay trong Editor. Instance trong Main Menu được ẩn/hiện theo trang; khi chạy, `MainMenuScreenBehavior` thiết lập trạng thái ban đầu, không tạo thêm trang hay tab.

Chỉnh RectTransform, font, cỡ chữ, màu, sprite và material trực tiếp tại GameObject tương ứng. Nền dùng `ui_slot` với Image Type **Sliced**. Script không ghi đè cỡ chữ đã chỉnh trong ba trang Forge/Achievements/Daily Quests. Script vẫn cập nhật nội dung động, thanh tiến độ và trạng thái nút.

Các công cụ authoring cũ chỉ dùng để dựng dữ liệu ban đầu trong Editor, không phải quy trình chỉnh giao diện hằng ngày. Không chạy lệnh dựng lại prefab nếu muốn giữ các chỉnh sửa trực tiếp trong prefab.

## Upgrades và bảng Stats

- Giữ nguyên 9 UpgradeData, ID, giá và từng mốc cấu hình trong Inspector. Không thêm Multishot giả theo ảnh minh họa vì database nâng cấp lâu dài hiện không có mục đó.
- Chọn một hàng để xem mô tả, cấp hiện tại → cấp kế tiếp và hiệu ứng. Nút mua ở hàng và nút lớn bên trái dùng chung dịch vụ giao dịch.
- Bảng Stats hiển thị ATK, HP, CRIT, tốc độ di chuyển và ASPD (hệ số tốc độ tấn công), so sánh bộ trang bị hiện tại với trạng thái sau đúng một lần nâng cấp đã chọn. Preview không sửa save hoặc stat đang dùng trong trận.
- HP/damage cộng từ hero và trang bị, các hệ số tốc độ nhân theo công thức `PlayerStatsHandler`. Sau đó áp nâng cấp lâu dài. Ability và buff của lượt chơi được giải quyết trong trận; đây là bảng chuẩn bị giữa các lượt.
- Không tạo chỉ số Combat Power khi dự án chưa có công thức được xác định. Shield, hồi máu cuối wave và revive thể hiện hiệu ứng tại khung nâng cấp đã chọn; không giả lập chúng thành ATK/HP.
- Mua kiểm tra giá của level kế tiếp, ví lâu dài, số dư, cấp tối đa và cấp đang được hiển thị. Chặn lời gọi lồng nhau, commit cấp trước callback ví và lưu sau giao dịch.

## Túi trang bị

Cấu hình tại `Assets/The Archer/Resources/Armory Dashboard Database.asset`:

- Khởi đầu **40 ô**. Đồ đang mặc cũng chiếm ô; mỗi ItemData chỉ có một bản sở hữu theo cấu trúc save hiện có. Bản trùng tiếp tục dùng cơ chế tăng level cũ.
- Nút **+** mở hộp thoại mua **10 ô bằng vàng**. Giá ban đầu **100 gold/lần**, chỉnh bằng `Expansion Price`; số ô và giới hạn ban đầu 200 cũng chỉnh được trong Inspector.
- Inventory tái sử dụng 36 ô đã vẽ trong prefab khi cuộn, không tạo GameObject mới theo dung lượng túi. Ô trống có ảnh và không mở popup trang bị. Bộ lọc không làm thay đổi số ô thực tế.
- Save Armory lưu thêm `bagCapacity` và `pendingEquipment`. Save cũ thiếu field bắt đầu ở 40; save đã có hơn 40 món giữ đầy đủ và nâng sức chứa tối thiểu cho phù hợp.
- Trang bị mới nhận khi đầy túi được giữ trong danh sách chờ, giao sau khi mở rộng. Rèn bị chặn khi đầy trước khi trừ mảnh/nguyên liệu. Nguyên liệu và mảnh có kho riêng, không chiếm ô trang bị.
- Nâng level trang bị vẫn sử dụng ItemLevels và popup Stage/Armory cũ. Không chuyển nâng level sang rèn.

## Kiểm tra

`Tools/MrX Studio/UI/Verify Bag Stats and Upgrades` kiểm tra dữ liệu thử riêng: save cũ, giao dịch túi, bấm lặp, cấp tối đa, các phép tổng hợp chỉ số và giá nâng cấp theo từng mốc. Các kiểm tra Crafting/Stage-Armory cũ tiếp tục áp dụng.

`ArmoryUpgradeVerification.VerifyLiveBagButton()` và `VerifyLiveUpgradeButton()` chạy khi trang tương ứng đang mở trong Play Mode, thử nút prefab bằng ví/save riêng rồi khôi phục dịch vụ thật. Không dùng vàng hoặc tăng level thực tế của người chơi.
