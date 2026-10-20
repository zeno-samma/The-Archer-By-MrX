# Xác minh logic và loot — 19/10/2026

Kiểm tra bằng Unity MCP trong Unity 6000.6.2f1. Có kiểm tra Play Mode và prefab thực; chưa build hoặc chạy trên thiết bị. Kết quả áp dụng cho các tình huống dưới đây, không phải bảo đảm tuyệt đối mọi tổ hợp gameplay.

## Kiểm tra tự động

| Nhóm | Kết quả |
| --- | --- |
| Daily Quests | 22 kiểm tra độc lập đạt |
| Achievements | 32 đạt |
| Nâng level Stage/Armory | 19 đạt |
| Crafting, công thức, kho, túi đầy | 29 đạt |
| Armory stats, mở túi, permanent upgrades | 24 đạt |
| Lucky Rewards transaction/delivery | 36 đạt |
| Developer Tools | 10 đạt |
| Bảng rương boss, receipt, recovery, stack/save | 25 đạt |
| Combat/cleanup/pool regressions | 28 đạt |

Tổng: **225 kiểm tra đạt** trên dữ liệu cô lập. Các kiểm tra delivery gồm thiếu tiền, tràn số, item đã sở hữu/MAX, bồi thường vàng, túi đầy và hàng chờ, mảnh riêng theo ItemData ID. Các kiểm tra bảng boss gồm biên random 0/1, trọng số NaN, loại thưởng không hợp lệ, cùng kill key không roll lại, claim một lần, save roundtrip/recovery và token run mới.

## Kiểm tra Play Mode

- Quái nhỏ với Item/CraftingResource trong cấu hình không tạo rare loot. Các profile Enemy/Boss/Chest legacy đặt 100% cũng không phát tài nguyên; callback mở rương ability lặp và dùng lại không phát loot.
- Đường Drop của boss tạo đúng một pooled chest khi gọi lặp; pickup lặp và replay cùng boss không grant thêm. Đã dùng prefab Queen Wasp thật trong Game scene, gọi đường Drop, chờ hiệu ứng, nhặt Boss Loot Chest và xác nhận phần thưởng được lưu. Lần cuối đã kiểm tra thêm TakeDamage → boss chết → Room callback → một rương, rồi nhặt/lưu. Không phải chơi tự nhiên hết một trận boss.
- Armory: nhận 12 rồi 18 mảnh tạo một ô ×30; ba stack Iron/Wood/Leather; ô trống dùng ảnh xanh đầy đủ, không hình vũ khí mờ. Đã xem ảnh chụp Game View. Nút mở rộng túi: modal, khóa dock, +10 ô, trừ vàng/lưu một lần.
- Forge: nút thật mở khóa Lv.1, trừ chi phí một lần, OWNED khóa lần tiếp; pooled payload không lẫn giữa hai lần dùng.
- Nút CLAIM nhiệm vụ/mốc ngày và thành tựu: không nhận lặp, trạng thái đã nhận/khóa; bộ lọc danh mục và cuộn gamepad.
- Permanent Upgrade: chọn hàng, preview, mua một lần và lưu.
- Lucky Wheel/Gem Chest: nút thật, bấm đúp, đóng/mở giữa hiệu ứng, Continue không cấp lại. Vòng quay với hai prize bổ sung có 7 ô đều nhau, đúng góc trúng, chạy khi timeScale=0.
- DEV: chọn lượng, cấp gold/gem/item, chuyển item, khóa dock, tắt ẩn ngay.
- Save: hai lần ghi đồng bộ liên tiếp; thêm yêu cầu khi đang ghi nền; dừng Play giữa ghi nền vẫn giữ số dư cuối.
- Prefab UI và drop: không có missing script.

## Các lỗi đã sửa trong đợt kiểm tra

- Rarity không tăng vì phép gán lại giá trị hậu tăng.
- Damage crit bị nhân dồn giữa nhiều mũi tên và cờ crit còn sót khi dùng lại projectile.
- Năm upgrade và Trifecta thêm modifier trong Clear; listener hình ảnh hero chưa được hủy đúng.
- Health Chest có thể lặp vô hạn khi chỉ có một lựa chọn.
- Projectile còn giữ target bị bỏ qua, piercing/ignore-obstacles khi tái dùng.
- Callback spawn drop trễ chạy sau khi dọn đối tượng; hủy delay, dừng coroutine khi tắt, kiểm tra đối tượng còn tồn tại/đang bật; chờ kết thúc spawn trước khi bay về player.
- Save đồng bộ trước đây giữ handle coroutine đã kết thúc và bỏ yêu cầu sau; ghi nền có thể dùng dữ liệu thay đổi hoặc ghi đè snapshot cuối khi thoát. Nay gộp yêu cầu chờ, chụp JSON trên main thread, ghi snapshot riêng và chờ worker trước bản ghi cuối.

## Save và Console

Dùng VerificationSession với file mới trong Temp/Verification. So sánh SHA-256 file save người chơi trước/sau: **không thay đổi**. Không xóa/reset hồ sơ thực.

Đã phát hiện và sửa MissingReferenceException ở callback spawn drop khi kiểm tra dọn pool; chạy lại không còn lỗi này. Unity MCP có cảnh báo kết nối WebSocket khi domain reload ở một lần biên dịch; đây là cảnh báo kết nối công cụ. Lượt Play cuối không có error/warning từ game. Không xóa Console để che lỗi còn tồn tại.

## Giới hạn và hiệu suất

Inventory tái sử dụng 36 view đã vẽ trong prefab, chỉ binding ô thấy được khi cuộn. Stack cập nhật theo sự kiện; không tạo ô cho từng đơn vị mảnh. Rương dùng pool; các pool item/nguyên liệu chiến đấu cũ không còn được khởi tạo; bảng loot chỉ roll một lần mỗi boss, receipt lưu theo run và dọn lịch sử đã claim khi bắt đầu run khác. Không dựng UI runtime.

Chưa đo profiler trên thiết bị, chưa kiểm tra toàn bộ ability/enemy trong mọi tổ hợp, chưa kiểm tra mất điện giữa ghi file hoặc build player. Không mở rộng thay đổi sang cân bằng toàn game, shader hoặc package. Các đầu mối còn lại trong AGENTS.md cần được kiểm tra riêng khi chạm đến hệ liên quan.


## Xác minh bản kết toán cuối run

Đã biên dịch và chạy lại sau thay đổi này:

- **230 kiểm tra**: Forge 19, Crafting 29, nhiệm vụ 22, thành tựu 32, Armory/Upgrades 24, Lucky Rewards 36, DEV 10, kết toán run mới 30 và gameplay regression 28 — đạt.
- Thắng và thua đều thử với gold + trang bị mới + hai rương mảnh cùng món: trong run không cấp kho/không đổi file; cuối run cấp đúng một lần và hiển thị hai stack vật phẩm cùng gold. Gọi Finalize/ForceFail lặp không cấp/lưu thêm.
- Đã thử đường chết thật của ba quái nhỏ và một boss qua TakeDamage → Room callback. Không ghi file khi giết; boss dùng đúng Item pool đã cấu hình trong stage; pickup có thông báo và vẫn chưa cấp vào kho. Màn thua cấp và hiện đúng món.
- Đã đóng/mở Play giữa run để thử checkpoint: ba receipt đã nhặt phục hồi nhưng menu không trả thưởng sớm. Tiếp tục rồi kết thúc với túi đầy đưa đúng một món vào PendingEquipment, cộng đủ 8 mảnh và hiển thị nhãn chờ, không nhân đôi.
- Đã thử từ chối tiếp tục checkpoint: kết toán một lần các món đã nhặt, xóa trạng thái run dở; callback lần hai không tăng level.
- Năm Gem Wallet mới đều cập nhật từ cùng ví gem khi số dư đổi. Đã xem Game View trên Armory/Forge; UI được lưu trong prefab. Thẻ thưởng được sửa tách tên và số lượng; màn kết quả đóng selector đang mở để không còn giao diện tương tác phía sau.
- Tất cả phiên xác minh dùng save riêng; hash save thật trước/sau không đổi. Console lượt cuối **0 error / 0 warning**.

Trong lúc reload Editor/đổi Play nhanh có gặp lỗi nội bộ Unity.Hierarchy khi ghi HierarchyWindow.settings và assertion Unity.PlayMode.Editor.DefaultScenario; không có stack từ game. Lượt Play cuối và kiểm tra runtime sau đó không còn các mục này. Không dùng thao tác xóa Console để thay thế sửa lỗi game.

Chưa build thiết bị hoặc đo profiler thực. Không khẳng định đã thử mọi tổ hợp ability/enemy; phạm vi tập trung kinh tế, vòng đời loot, save/checkpoint, màn kết quả và các tab menu. Hướng dẫn mới và bản đồ file: [Run Rewards and Save](Run%20Rewards%20and%20Save.md).
