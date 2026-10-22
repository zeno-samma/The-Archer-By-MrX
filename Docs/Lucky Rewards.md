# Vòng quay may mắn và rương gem

Cập nhật 20/10/2026: mỗi chức năng có một lượt miễn phí mỗi ngày và chấm đỏ khi chưa dùng; sau đó mới trả giá gem cũ. Cấu hình/reset/save và kiểm tra: [Daily Free Lucky Rewards](Daily%20Free%20Lucky%20Rewards.md).

Hai nút xuất hiện bên phải trang chọn Stage, dùng hai ảnh người dùng cung cấp. Gem được thêm vào **Currencies Database** hiện có với ID `gem`, icon riêng, save riêng và ví gameplay riêng theo cơ chế cũ. Gold vẫn ở vị trí đầu database và vẫn là default currency.

## Cấu hình trong Inspector

Tại `Assets/The Archer/Scriptables/Lucky Rewards/`:

- `Lucky Wheel.asset`: giá ban đầu **10 gem/lượt**, hiệu ứng 4 giây.
- `Gem Chest.asset`: giá ban đầu **30 gem/lần**, hiệu ứng 1,8 giây.

Đây là mức thử nghiệm có thể đổi, chưa phải cân bằng cuối. Gem khởi đầu 0 trong save mới; tính năng không tự cấp gem vào ví người chơi. Dùng cơ chế cấp currency hiện có để đưa gem vào nhiệm vụ/phần thưởng theo thiết kế kinh tế sau này.

Mỗi asset có ID ổn định, title, Price, thời gian hiệu ứng và danh sách Prizes. Mỗi prize có ID riêng, bật/tắt, weight, icon/title tùy chọn, loại thưởng, số lượng nhỏ nhất/lớn nhất. Inspector hiển thị tỉ lệ đã chuẩn hóa: **weight / tổng weight đang bật**. Tổng không bắt buộc bằng 100; weight 0 bị loại. Không đổi ID chỉ để đổi tên hiển thị.

Các loại quà:

| Loại | Cấu hình và cách nhận |
| --- | --- |
| Currency | `currencyId` gold/gem hoặc currency đã đăng ký; cộng ví lâu dài |
| Equipment | Kéo ItemData đã đăng ký vào `item`, số lượng 1; đồ mới Lv.1, không tự equip |
| Fragments | Kéo đúng ItemData có công thức rèn; cộng mảnh riêng theo ID |
| Material | Chọn Iron, Wood, Leather; dùng kho rèn hiện có |

Trang bị trùng tiếp tục tăng level theo cơ chế hiện hữu. Nếu đã MAX, nhận gold bằng `Max Level Duplicate Gold` của prize. Đồ mới khi đầy túi được lưu vào danh sách chờ và nhận sau khi mở rộng; không mất đồ đã mua. Quà không tự thay đổi tỉ lệ theo trạng thái sở hữu. Khi tặng mảnh của món đã sở hữu, mảnh vẫn được giữ trong kho, vì vậy cần chọn danh sách quà phù hợp khi cân bằng.

Mỗi lượt nhận **một** prize. Số lượng trong khoảng min/max được chọn đều. Vòng quay chia ô và đặt icon **đều nhau**, không ghi phần trăm trên vòng. Diện tích ô không đại diện cho xác suất. Danh sách bên phải hiển thị tỉ lệ thật theo weight; việc chọn thưởng vẫn dùng weight, rồi kim dừng đúng tâm ô của món đã chọn.

## Chỉnh giao diện và clone

Tại `Assets/The Archer/Prefabs/UI/Lucky Rewards/`:

- `Stage Lucky Rewards.prefab`: hai nút và hai popup lồng sẵn, được đặt trong Stage Page của Main Menu.
- `Lucky Wheel Popup.prefab`: nền, vòng quay, kim cố định, ví gem, danh sách quà, nút và màn kết quả.
- `Gem Chest Popup.prefab`: hình rương, danh sách quà và màn kết quả.
- `Prize Row.prefab`: ô quà dùng lại cho danh sách.
- `Wheel Slice.prefab`: miếng vòng quay và icon; nhãn tỉ lệ cũ được tắt.

Mở Prefab Mode để chỉnh RectTransform, font, màu, hình và nút. Background dùng ui_slot Sliced pastel. Script không dựng GameObject/component UI; khi danh sách quà tăng, chỉ instance các prefab view đã vẽ. Các ô mới tự chia đều theo số prize đang bật có weight dương. Có thể clone popup và table, gán table mới tại component LuckyRewardsPopup; table clone cần ID riêng nếu là một loại sản phẩm độc lập. Không dùng công cụ authoring để ghi đè chỉnh sửa thủ công.

## Giao dịch và save

LuckyRewardsService kiểm tra cấu hình, ví gem, số dư, phần thưởng và giới hạn số trước khi trừ. Không dùng gold thay gem. Chặn thao tác lồng nhau và lần mua tiếp theo khi kết quả trước chưa được xem xong.

Kết quả được chọn, trừ gem, cấp quà và lưu trước hiệu ứng. Save `Lucky Rewards` giữ sequence và receipt gần nhất; receipt chỉ chứa ID/giá trị, không giữ reference Sprite/ScriptableObject. Đóng cửa sổ hoặc khởi động lại giữa hiệu ứng không quay lại hoặc cấp quà lần nữa. Khi mở lại, màn kết quả chưa xác nhận xuất hiện. **Continue** chỉ xác nhận đã xem, không cấp lại quà. Hiệu ứng dùng unscaled time; Escape/gamepad Cancel đóng cửa sổ và trả quyền dock.

Receipt chỉ hợp lệ khi có sequence dương khớp bộ đếm, ID và lượng quà dương. Cách Unity deserialize một inline class rỗng không được xem là một giao dịch; save cũ/trống không hiện thưởng x0 hoặc khóa chức năng.

Save dùng cơ chế JSON hiện tại của dự án. Không có backend, giao dịch máy chủ hay bảo đảm chống sửa save/atomic file write khi mất điện; các đặc tính này nằm ngoài phạm vi hệ thống offline hiện có.

## Kiểm tra

`Tools/MrX Studio/Lucky Rewards/Run Isolated Checks` kiểm tra trọng số/biên random, số lượng, loại tiền, thiếu gem, lồng nhau, receipt/save, đủ loại quà, đồ trùng và túi đầy trên dữ liệu riêng. Chạy khi không ở Play Mode.

Trong Play Mode tại Stage, `LuckyRewardsVerification.VerifyLiveButton(true/false)` thử nút prefab bằng ví/save riêng, gồm bấm đúp và đóng/mở giữa hiệu ứng. `BeginAnimationCheck` → `FinishAnimationCheck` kiểm tra góc trúng, timeScale=0 và thêm reward bằng prefab. Các kiểm tra này không cấp gem hoặc thay đổi thưởng thật của người chơi.

Xác minh ngày 19/10/2026: 36 kiểm tra logic/delivery đạt; hai nút prefab được thử trong Play Mode bằng ví riêng; kiểm tra vòng quay với tám prize, đúng góc trúng và timeScale=0 đạt. Save trống đã được nạp lại để kiểm tra không có receipt giả. Console không có lỗi sau kiểm tra; chưa kiểm tra build thiết bị.


## Chính sách loot hiện tại — 19/10/2026

Hai bảng đang sử dụng chỉ bật currency và nguyên liệu **Iron/Wood/Leather**. Quà Equipment/Fragments của bảng Gem Chest cũ đã chuyển sang nguyên liệu. Crystal và Scroll cũ được gộp trọng số sang Wood, giữ tổng trọng số; dòng Wood trùng tắt đi. Tổng weight đang bật vẫn được chuẩn hóa, hình vòng quay vẫn chia đều.

Equipment/Fragments vẫn là loại thưởng mà delivery dùng chung hỗ trợ để tương thích và kiểm thử; nguồn đang sử dụng cho hai loại này là **rương boss**. Không bật lại chúng trong Lucky tables nếu muốn giữ chính sách hiện tại. Xem [Crafting](Crafting.md).
