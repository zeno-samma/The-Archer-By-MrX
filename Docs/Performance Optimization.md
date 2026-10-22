# Hiệu suất và cách đo lại

Đợt tối ưu ngày 21–22/10/2026. Không build player trong tác vụ này. Các kiểm tra dùng Unity MCP, Editor và save thử nghiệm riêng; chưa phải chứng nhận tốc độ trên mọi thiết bị.

## Những phần đã thay đổi

Các đường dẫn dưới đây bắt đầu từ gốc repository.

| File trong `Assets/The Archer/Scripts/` | Thay đổi và tác dụng |
| --- | --- |
| `Projectiles/SimpleProjectile.cs` | Bỏ `Physics.OverlapSphere` có kết quả không được dùng trong ricochet. Giữ nguyên đường chọn mục tiêu của Room. |
| `Status Effects/ShockStatusEffect.cs` | Dùng buffer collider tái sử dụng. Tự tăng kích thước khi đầy, tránh bỏ sót mục tiêu ở wave đông; giải phóng reference collider sau khi đọc. Lần tăng buffer vẫn cấp phát. |
| `Player/EnemiesDetector.cs` | Tìm quái gần nhất còn hoạt động/có đường bắn trong một lượt duyệt thay cho sắp xếp danh sách mỗi lần ngắm; cache obstacle layer mask. Khi tất cả bị chắn, trả quái hợp lệ gần nhất. Số raycast còn phụ thuộc thứ tự và vật cản; không khẳng định mọi tình huống nhanh hơn. |
| `Enemy/NavmeshNavigationManager.cs` | Cache mảng đỉnh/chỉ số thay vì đọc các bản sao từ Mesh mỗi lần lấy điểm ngẫu nhiên. Reset tổng diện tích khi khởi tạo lại và dọn Mesh native khi hủy manager. Không đổi phân phối lấy mẫu theo diện tích. |
| `Player/HealthbarBehavior.cs` | Chỉ tạo chuỗi HP khi số HP hiển thị đổi; animation thanh máu vẫn cập nhật bình thường. |
| `UI/Main Menu/LocalizedMenuScope.cs` | Kiểm tra thay đổi tối đa một lần mỗi frame dù Canvas gọi nhiều lần; tránh gán lại text/font không đổi; font thiếu dùng fallback nhất quán. |
| `UI/ResponsiveMenuLayout.cs` | Tìm surface khi enable/đổi parent thay vì tìm mỗi frame ở panel không có surface. Bố cục ngang/dọc vẫn cập nhật khi hướng của surface đổi. |
| `Other/GameController.cs` | Inspector có Desktop Frame Rate mặc định 120 và Mobile Frame Rate mặc định 60. Trước đây cố định 120 cho cả hai. VSync có thể quyết định FPS thực tế trên desktop; giới hạn FPS không làm thiết bị yếu tự đạt được mức đó. |
| `Editor/PerformanceCapture.cs` | Công cụ lấy mẫu Editor, không đưa vào player. |

## Ghi mẫu trong Unity

1. Mở Main Menu và Play theo luồng chuẩn. Chọn stage/hero/build cần kiểm tra.
2. Đợi phòng tải xong và đóng bảng chọn kỹ năng. Giữ cùng độ phân giải, focus Editor và wave khi so sánh.
3. Chọn **Tools → MrX Studio → Performance → Capture 600 Gameplay Frames**.
4. Công cụ bỏ qua 60 frame đầu, sau đó thu 600 frame không pause. Không ghi file trong mỗi frame.
5. Báo cáo tự lưu vào `Logs/Performance/yyyyMMdd-HHmmss.md`. Có thể kết thúc sớm bằng **Stop and Save Capture**; thoát Play cũng dừng và giải phóng recorder.
6. Đọc mean/median/P95/max Main Thread theo ms và số byte GC trung bình/tối đa. Main Thread có thể gồm chờ đồng bộ, chi phí Editor và MCP; không quy đổi trực tiếp thành FPS gameplay hay thời gian GPU.

Ngân sách tham khảo: 60 FPS có khoảng 16,67 ms/frame, 30 FPS khoảng 33,33 ms/frame. Cần cả CPU và GPU đáp ứng, đồng thời xem spike và nhiệt độ sau khi chơi lâu; FPS trung bình không đủ để đánh giá độ mượt.

## Kiểm tra đã thực hiện

- Compilation không có lỗi ở các lần kiểm tra sau sửa.
- Stage 5 Normal, 3840 × 2160, Very High, hero được bảo vệ và tắt animation bắn để giữ tình huống ổn định. Hai mẫu 600 frame lần lượt có Main Thread mean 8,33/8,33 ms, P95 9,24/9,42 ms; GC trung bình khoảng 58,6/57,7 KB/frame. Đây là hai mẫu ở các bước triển khai, không phải benchmark trước/sau toàn bộ tác vụ. Không suy ra phần trăm tăng FPS từ chúng.
- Lấy vị trí ngẫu nhiên NavMesh: 200 lần khớp cách cũ khi phục hồi cùng trạng thái random. Bộ đếm `GC.GetAllocatedBytesForCurrentThread` trả 0 cho cả cách cũ và mới trên runtime này nên không dùng nó để khẳng định số byte tiết kiệm.
- Chọn mục tiêu thực trong phòng: kết quả raycast/không raycast đúng; danh sách không bị sắp xếp; trường hợp rỗng trả false.
- Shock trong tình huống thử 82 collider: buffer tăng lên 128, không bị cắt ở 16 và các reference đã đọc được xóa.
- Stress test nhân tạo Stage 5 với 26 quái, gồm cả bốn boss xương cùng lúc: 600 frame, Main Thread mean 8,28 ms, P95 9,65 ms, GC trung bình khoảng 36,8 KB/frame. Snapshot cuối có 115.078 triangles; đây là cảnh stress thêm quái, không phải geometry mặc định của map. Không so phần trăm cải thiện với hai mẫu ít quái ở trên.
- Các lượt thử đã kết thúc với hash save thật không đổi.

## Giới hạn và điểm cần đo tiếp trên thiết bị

Đã rà các đường combat, projectile, pool, navigation, UI, save và cấu hình rendering liên quan. Chưa đọc/benchmark từng asset, từng ability, mọi phối hợp hero, toàn bộ wave và mọi thiết bị. Không coi tác vụ này là xác nhận toàn dự án đã tối ưu tuyệt đối.

- URP đang dùng shadow distance 100, render scale 1, MSAA 1; các mức Quality không gán pipeline riêng nên không nên mặc định đổi tên Quality sẽ giảm toàn bộ chi phí URP. Chưa tự giảm chất lượng hình ảnh toàn game khi chưa có số đo GPU trên thiết bị đích.
- Editor 4K và các counter toàn Editor không đại diện điện thoại. Khi có yêu cầu build tiếp theo, đo player trên PC cấu hình tối thiểu, Android/iOS thực, màn hình ngang/dọc và phiên chơi dài.
- Kiểm tra wave đông nhất, boss, multishot + ricochet + Shock, số lượng damage text, particle/overdraw, lần mở rộng pool và lúc chuyển phòng/build NavMesh. Các thao tác khởi tạo vẫn có thể tạo spike.
- Đặt mục tiêu riêng cho cấu hình thấp/trung/cao sau số đo thiết bị; không giảm số quái, damage, loot hoặc nhịp trận chỉ để cải thiện thống kê.

Không có thay đổi save schema, drop rate hay quyền mở khóa; công cụ đo không gửi dữ liệu online.
