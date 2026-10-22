# Cân bằng nhịp độ và đo từng lượt chơi

Cập nhật 21/10/2026. Các mục tiêu bên dưới là giả thuyết để playtest, không phải kết quả giữ chân người chơi đã được chứng minh.

## Đọc báo cáo nhanh — cập nhật giao diện 21/10/2026

1. Chọn **Tools → MrX Studio → Balance → Summarize Run Reports**. Công cụ tự mở báo cáo tiếng Việt trong trình duyệt, không cần đọc CSV.
2. Xem ba ô đầu: số phiên, số thắng/chết và thời gian điển hình của những run thắng.
3. Trong từng màn/nhân vật, đọc **Cần xem trước**: đợt lâu nhất, đợt mất nhiều HP nhất, số bản ghi chưa hoàn tất. Đây là gợi ý nơi cần kiểm tra, không tự thay đổi cân bằng.
4. Bấm **Xem từng đợt** để mở bảng. Tìm theo tên màn hoặc nhân vật nếu có nhiều nhóm. “Chưa có lần vượt qua” nghĩa là chưa tính được thời gian clear, không phải mất 0 giây.
5. Chơi thêm rồi chạy lại lệnh để cập nhật báo cáo.

File chính: `Temp/BalanceReport.html` (mở bằng trình duyệt). `Temp/BalanceSummary.csv` chỉ là bản số liệu chi tiết giữ lại để xuất sang công cụ khác. File trong Temp có thể bị Unity dọn; chạy lại lệnh để tạo lại từ JSON. Source trình bày: `Assets/The Archer/Scripts/Editor/BalanceReadableReport.cs`. Công cụ này là báo cáo offline dành cho developer, không phải UI runtime trong game.

## Những thay đổi đã áp dụng

- Mage/Rogue Skeleton: `attackRange = 40`, bằng bán kính detector gốc của Ranger. Không sao chép buff tăng tầm của người chơi. Đạn được kéo dài lifetime đủ đi 42 đơn vị, giữ tốc độ và va chạm vật cản; không biến thành đạn đuổi mục tiêu.
- Sửa nghỉ giữa wave: lấy delay của wave vừa kết thúc, trừ **thời gian đã chờ**, thay vì trừ timestamp tuyệt đối.
- Stage 5: nghỉ thường 0,8s; trước boss 2s; sau boss 3s. Đây là thời gian tối thiểu tính từ sau thu drop/indicator; thời gian chờ chọn ability được trừ vào nó. Không thêm 3 giây chờ thừa sau khi chọn xong.
- Giữ nguyên loot/rate boss, giá, HP và damage. Chưa áp dụng tự tăng độ khó theo sức mạnh người chơi.

## Đọc số liệu

`Assets/The Archer/Scripts/Other/ConsoleTimer.cs` gắn trong Game scene. Bật/tắt `Export Development Reports` trên component. Chỉ Editor hoặc development player xuất báo cáo, một file JSON khi kết thúc phiên (thắng, chết cuối cùng hoặc về menu), không ghi file mỗi lần giết quái.

Console in đường dẫn đầy đủ: `Application.persistentDataPath/BalanceReports/run-*.json`. File này độc lập với save người chơi. Bấm Stop Play trực tiếp không thay thế luồng kết thúc run và không tự xuất báo cáo.

- `activeSeconds`: thời gian scaled từ lúc Game scene khởi tạo bộ đo; pause bằng timeScale=0 không tính. Bao gồm chiến đấu, thu đồ và di chuyển; không phải thời gian thực trên đồng hồ.
- `waves[].seconds`: từ bắt đầu spawn wave đến quái cuối bị hạ, gồm cảnh báo spawn. Wave chỉ có rương đo đến lúc hoàn tất rương. Wave chết giữa chừng ghi `cleared=false`.
- `hitsTaken`, `damageTaken`: chỉ damage thực sự trừ HP sau dodge/giảm sát thương, bỏ invincible. Có tổng và theo wave.
- `room`, `wave`: chỉ số hiển thị bắt đầu 1; `boss`, `enemyCount`: loại wave và số spawn cấu hình, không tính summon.
- `hero`: tên prefab hero; `stage`: StageId ổn định; `resumed=true`: có khôi phục run. Báo cáo resumed chỉ đo đoạn vừa chơi, không được so với full run.
- Timer room dùng khóa số phòng ổn định, không phụ thuộc icon. Gọi Start trùng không tạo bộ đếm đua nhau. In lại tổng không cộng dồn. ResetSession xóa thật; không reset khi chuyển room.

## Quy trình cân bằng đề xuất

1. Chọn một bộ hero/level/trang bị/permanent upgrade cố định; ghi lại cấu hình cùng báo cáo. Thu ít nhất 10 run mỗi nhóm, tách người mới/người quen và run resumed. Không lấy tester bất tử làm dữ liệu cân bằng.
2. So sánh trung vị và nhóm 25% chậm nhất cho từng wave. Xem cả số hit/HP mất: wave lâu nhưng ít damage thường do HP, đường đi hoặc quái bị kẹt; wave nhanh mà nhiều hit thường do chồng đạn hoặc thiếu cảnh báo.
3. Mốc thử cho Stage 5: wave thường khoảng 12–20s, boss 35–60s. Với 29 wave thường, 4 boss và lựa chọn build, hướng thử một run khoảng 9–15 phút. Điều chỉnh theo dữ liệu; không kéo dài trận chỉ để đạt số phút.
4. Mỗi lượt chỉ đổi một nhóm: HP hoặc số quái hoặc tốc độ bắn. Nếu wave thường vượt mục tiêu vì HP, thử giảm 10% HP rồi đo lại; đừng đồng thời giảm cả damage và tăng loot.
5. Giữ nhịp lên–xuống: wave làm quen → kết hợp quái → wave áp lực → boss → khoảng thở. Tránh mọi wave đều đông hoặc mọi boss đều tăng HP mà không có cách đối phó khác.
6. Động lực chơi lại nên đến từ việc thử build khác, học được đòn boss và thấy tiến bộ. Kiểm tra lựa chọn ability có thực sự thay đổi cách chơi; các lựa chọn yếu cần được nâng tính hữu dụng trước khi tăng độ hiếm. Phần thưởng hiện tại giữ nguyên cho đến khi có dữ liệu tốc độ thu thập.
7. Khi chết, kiểm tra có thể chỉ ra sai lầm và cách tránh hay không. Nếu không, tăng độ rõ cảnh báo/đường đạn hoặc giảm chồng kỹ năng trước khi bù bằng thưởng.

Các gợi ý 3–7 là quy trình playtest và hướng cân bằng tiếp; bản này hiện thực hóa đo lường, sửa delay và nhịp nghỉ Stage 5. Chưa khẳng định đã tối ưu retention hoặc cân bằng mọi build.

## File cần sửa trong Inspector/source

| Nội dung | Đường dẫn |
| --- | --- |
| Tầm đánh boss | `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Rogue Boss.prefab`, `Skeleton Mage Boss.prefab` → SkeletonBossBehavior → Attack Range |
| Cadence/skill/cảnh báo | Cùng component: Shots Before Skill, Shot Recovery, Shot Warning, Warning Duration |
| Tốc độ đạn | `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Crossbow Bolt.prefab`, `Skeleton Arcane Bolt.prefab` |
| Wave/HP/damage | `Assets/The Archer/Scriptables/Stages/Stage Data 005.asset` → Rooms → Waves; dùng Stage Creator khi cần |
| Nghỉ | Room → Delay Between Waves; Wave → Custom Wave End Delay (âm = kế thừa room; 0 = không nghỉ) |
| Timer và xuất report | `Assets/The Archer/Scripts/Other/ConsoleTimer.cs` |
| Điểm đo wave và sửa delay | `Assets/The Archer/Scripts/Stage/Room/RoomBehavior.cs` |
| Damage thực nhận | `Assets/The Archer/Scripts/Player/PlayerBehavior.cs` → TakeDamage |
| Authoring boss | `Assets/The Archer/Scripts/Editor/SkeletonSurvivalRefinement.cs` |

Lệnh Apply Skeleton Boss Combat Revision đặt lại các giá trị preset của boss, gồm range 40. Sau khi cân bằng thủ công không chạy lại lệnh này nếu muốn giữ những chỉnh sửa riêng. Lệnh cài lại Stage 5 có thể ghi lại cấu hình wave, hãy xem diff trước khi lưu.

## Tổng hợp nhiều run

Chọn **Tools → MrX Studio → Balance → Summarize Run Reports**. Mở `Temp/BalanceSummary.csv`: nhóm theo StageId/hero/room/wave, số lần thử, số clear, median/P75 thời gian clear và HP mất trung bình. Wave thất bại không đi vào median clear. File source: `Assets/The Archer/Scripts/Editor/BalanceReportTools.cs`.

Công cụ loại run resumed và report có cờ `verification` của bộ kiểm tra cô lập. Các file thử từ trước khi có cờ này cần chuyển riêng bằng tay; CSV không tự biết level/trang bị hoặc kỹ năng của người điều khiển. Không diễn giải tỷ lệ clear wave thành tỷ lệ thắng full run, và chưa đủ mẫu thì chưa kết luận.

## Phạm vi xác minh 21/10

- Compilation, boss bắn thực tế tại khoảng cách 18–19 (lớn hơn ngưỡng cũ 11), lifetime đạn tương ứng 42/48 đơn vị.
- Timer start trùng, đọc/in tổng nhiều lần, pause, xuất JSON một lần, sinh CSV.
- Chưa có dữ liệu playtest người thật để kết luận retention hoặc khẳng định mọi build đã cân bằng. Không build player.
- Phiên kiểm tra ban đầu vào Play khi còn lỗi biên dịch đã làm kiểm tra hash save gốc không khớp. File save gốc quan sát sau đó có các cell JSON rỗng; không có snapshot trước phiên để đối chiếu/khôi phục, nên không xác nhận save gốc không đổi. Không tự thay bằng save thử. Bộ VerificationSession được bổ sung chặn compilation lỗi, sao `original-save.backup` trước phiên mới và phục hồi đường dẫn cô lập sau Editor domain reload. Save thử ở `Temp/Verification/20261021-043245` được giữ để tra cứu.

Kiểm tra bổ sung bảo vệ save: phiên mới `Temp/Verification/20261021-045740` có bản sao `original-save.backup`; sau compilation/domain reload đường dẫn vẫn trỏ vào thư mục thử, End xác nhận hash không đổi so với đầu phiên mới. Điều này không thay thế kết luận giới hạn của phiên thử ban đầu ở trên.
