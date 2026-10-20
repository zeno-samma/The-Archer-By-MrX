# Rèn trang bị, loot boss và kho vật liệu

## Nâng level và ghép mới

- **Stage/Armory → popup trang bị → Upgrade:** tiếp tục dùng ItemData → Item Levels và giá từng mốc cũ trong Inspector.
- **Forge:** dùng 30 mảnh riêng theo ItemData ID cùng vật liệu để mở khóa món chưa sở hữu ở Lv.1. Không tự equip; món đã sở hữu bị khóa rèn.
- Mỗi công thức nằm ở `Assets/The Archer/Scriptables/Armory/Items`, mục **Crafting**. Số mảnh, loại vật liệu và số lượng đều chỉnh được. Yêu cầu các loại vật liệu không trùng nhau, số lượng dương.

Ba vật liệu hiện dùng: **Iron, Wood, Leather**. Icon lấy từ `Sprites/UI/Items/ui_iron.png`, `ui_wood.png`, `ui_leather.png`. Công thức cũ được chuyển một lần: giữ Iron và Leather; gộp chi phí Crystal + Scroll thành Wood. Các số lượng này có thể chỉnh lại trên từng ItemData.

ID enum cũ và số dư Crystal/Scroll cũ được giữ để tương thích save; hai loại này hiện không xuất hiện trong UI hoặc phần thưởng đang bật. Wood thêm ID mới, không chiếm ID cũ. Muốn thêm loại vật liệu sau này: thêm ID enum ở cuối, key save mới, định nghĩa/icon trong Crafting Database rồi thêm view prefab Forge và công thức tương ứng. Không đổi thứ tự ID cũ.

## Rương boss

Quái nhỏ chỉ giữ XP, hồi phục và buff vốn có. Chúng không rơi trang bị, mảnh ghép, bản vẽ hoặc vật liệu. Các rương ability Good/Health/Evil cũng không phát crafting loot. Profile Enemy/Boss/Chest crafting cũ đã ngừng hoạt động, kể cả khi dữ liệu cũ còn đặt chance > 0.

Boss có UseDrop tạo **một rương thưởng**. Rương hiện tái sử dụng hình hộp đồ và hiệu ứng từ source, prefab `Assets/The Archer/Prefabs/Drop/Boss Loot Chest.prefab`. Rương có collider nhặt và tự thu cuối wave/phòng qua DropManager hiện có. Không thêm cơ chế phát thưởng thứ hai khi kết thúc run.

Chọn `Assets/The Archer/Scriptables/Boss Chest Rewards/*.asset` để chỉnh:

| Trường | Ý nghĩa |
| --- | --- |
| Prizes / Enabled | Danh sách phần thưởng và bật/tắt từng dòng |
| Id | ID duy nhất trong bảng; giữ ổn định |
| Kind | Equipment hoặc Fragments; rương boss không nhận Material/Currency trực tiếp |
| Item | ItemData đã đăng ký trong Armory Database |
| Weight | Trọng số; xác suất = weight / tổng weight đang bật. Không bắt buộc tổng 100 |
| Minimum / Maximum Amount | Equipment luôn 1; Fragments chọn số nguyên trong khoảng, gồm hai đầu |
| Max Level Duplicate Gold | Bồi thường vàng khi trang bị trùng đã đạt cấp tối đa |

Ưu tiên bảng: **Stage Creator → Enemy Overrides → Boss Chest Rewards**, sau đó **Enemies Database → boss → Boss Chest Rewards**, cuối cùng **Crafting Database → Default Boss Rewards**. Bảng riêng từng wave được chuyển từ item pool override cũ; các bảng khởi tạo dùng trọng số tương đối của item cũ, chia 70% đồ nguyên chiếc / 30% mảnh (1–3). Đây là cấu hình ban đầu, chỉnh trực tiếp trong Inspector để cân bằng.

Mỗi rương chọn một phần thưởng. Đồ mới mở khóa Lv.1; đồ trùng chưa tối đa theo cơ chế tăng level cũ. Túi đầy thì đồ mới vào hàng chờ an toàn; mở rộng túi để nhận. Mảnh cộng vào kho riêng và không chiếm sức chứa trang bị.

Save **Boss Chests** giữ phần thưởng đã chọn, danh tính boss trong run và trạng thái nhận. Gọi lại callback hoặc tải lại cùng boss không roll/nhận thêm. Khi về menu hoặc khởi động lại, phần thưởng đã ghi nhưng chưa nhặt được phục hồi một lần. Lượt chơi mới có token riêng nên tiếp tục farm được. Đây là lưu offline theo cơ chế JSON hiện có, không bảo đảm giao dịch khi mất điện giữa lúc ghi file.

## Nguyên liệu và inventory

Vật liệu hiện nhận qua **Lucky Wheel / Gem Chest**; hai bảng đang dùng không có trang bị hoặc mảnh ghép. Chỉnh giá gem và trọng số tại `Scriptables/Lucky Rewards`. Xem [Lucky Rewards](Lucky%20Rewards.md).

Trong Armory, ALL hiển thị trang bị, mảnh riêng và ba nguyên liệu. Mỗi ItemData có một ô mảnh và số `× số lượng`; nhận nhiều lần cộng chung. Bấm ô stack để xem tên/số đang sở hữu. Bộ lọc loại trang bị chỉ hiển thị loại tương ứng. Chỉ trang bị tính vào sức chứa 40 ô; vật liệu/mảnh không làm mất ô trang bị.

Ô trống dùng PNG người dùng cung cấp, lưu tại `Sprites/UI/Armory/ui_inventory_empty.png`; không còn hình vũ khí mờ hoặc dấu cộng vẽ đè. Sửa hình ở Armory Dashboard Database và prefab `Prefabs/UI/Menu Pages/Armory Page.prefab`. Các ô được tái sử dụng khi cuộn; script chỉ binding dữ liệu, không dựng cây UI runtime.

Forge hiện có ba ô vật liệu trong prefab. Chỉnh bố cục/font/sprite trực tiếp trong Prefab Mode. Không chạy builder khi chỉ cần sửa giao diện thủ công.

## Công cụ và kiểm tra

**Tools → MrX Studio → Loot → Install Loot and Inventory Assets** là lệnh chuyển cấu hình/prefab của thay đổi này. Chạy ở Edit Mode; có thay đổi asset/prefab nên xem diff. Không gọi khi chỉ chỉnh trọng số. Bảng boss đã tồn tại không bị ghi lại; các công thức đã có Wood không bị chuyển lần nữa.

Các kiểm tra logic: `CraftingVerification.Verify()`, `LootInventoryVerification.Verify()`, `LuckyRewardsVerification.Verify()`. Kiểm tra Play: `VerifyLiveBoss()`, `CraftingVerification.VerifyLiveLoot()`, nút Forge và Armory. `VerifyLiveInventory()` cấp stack vào hồ sơ đang chạy để kiểm tra hiển thị: **chỉ dùng trong VerificationSession**, không dùng với save người chơi thật. Xem [báo cáo kiểm tra](Verification%202026-10-19.md).

## Quy trình hiện tại

Xem [Phần thưởng trong run và save](Run%20Rewards%20and%20Save.md): tỉ lệ Item trong Drop Overrides từng boss của stage được đọc trực tiếp; rương nhặt chỉ xếp thưởng chờ. Cấp vật phẩm và ghi save ở màn thắng/thua. Mô tả cấp ngay khi pickup và RecoverPending tự trả rương dưới đất ở bản trước không còn áp dụng.
