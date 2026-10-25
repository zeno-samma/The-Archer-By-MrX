# Kỹ năng đã max và kỷ lục cuối run

Cập nhật 22/10/2026. Không build trong tác vụ này.

## Quy tắc chọn kỹ năng

- Kỹ năng thường có level đã lưu **lớn hơn hoặc bằng `LevelsCount - 1`** sẽ không được đề xuất nữa. Level nội bộ bắt đầu từ 0.
- Quy tắc này áp dụng cả khi asset có cờ `IsRepeatedAbility`, khi resume, và với kỹ năng do trang bị/hero cấp mà đã được lưu vào Abilities Save.
- Không đổi level/stat của kỹ năng đã sở hữu. API lặp lại phục vụ behavior hiện hữu vẫn còn; danh sách đề xuất tuân theo giới hạn level.
- Không đổi trọng số độ hiếm, yêu cầu tiên quyết hoặc điều kiện chỉ dành cho vũ khí tầm xa.
- Các mục cùng `AbilityType` trong database không tạo ra lựa chọn trùng trong cùng bảng.
- Khi ít kỹ năng phù hợp, dùng phần thưởng Endgame hồi máu/vàng hiện có. Đây là phần thưởng có tác dụng tức thời, không phải nâng tiếp một kỹ năng đã max. Không nhân bản thẻ để ép đủ ba lựa chọn.

File: `Assets/The Archer/Scripts/Abilities/Data/AbilitiesManager.cs`. Cả selector, reroll và đường lấy ability theo rarity dùng chung các phương thức này.

## Màn kết quả hiện tại

Từ cập nhật mùa/EXP ngày 22/10, thời gian và DPS cao nhất chuyển ra **màn kết quả gọn**, còn bảng chi tiết chỉ mở bằng nút Thống kê. Thời gian clear thấp nhất dành cho lượt thắng; thời gian sinh tồn dài nhất dành cho lượt thua được lưu riêng. DPS là trung bình toàn run, không phải đỉnh một giây. Các kỷ lục tách theo stageId và độ khó.

Hai số dùng UITextEffect, đếm 2,4 giây bằng unscaled time, rồi hiển thị nhãn kỷ lục mới. Sửa tại `Assets/The Archer/Prefabs/UI/Run Progression/Compact Run Result.prefab`. Nhóm Record Highlights cũ được giữ để tương thích công cụ nhưng bị tắt trong các màn kết quả. Không dùng lệnh Upgrade Run Summary Records để thay bố cục mới.

Hướng dẫn hiện hành và công thức: [Mùa, EXP hero và kết quả](Seasons%20Hero%20Experience%20and%20Results.md).

## File xử lý và lưu dữ liệu

| Đường dẫn trong `Assets/The Archer/Scripts/` | Trách nhiệm |
| --- | --- |
| `Stage/RunRecordsSave.cs` | Save key mới `Run Records`, danh sách kỷ lục; so sánh thời gian/DPS và giữ giá trị trước đó cho UI. Không có file save riêng. |
| `Stage/RunCombatStatistics.cs` | Công thức DPS và kết quả so sánh tạm thời của lượt; không dùng giá trị UI để tính damage. |
| `Stage/StageController.cs` | Ghi kỷ lục tại FinalizeRunRewards trước lần commit cuối lượt, chỉ một lần; lượt testing qua Stage Creator không lập kỷ lục. |
| `UI/RunSummaryPresenter.cs` | Binding bảng thống kê chi tiết theo yêu cầu. |
| `UI/RunRecordHighlights.cs` | Thẻ kỷ lục bố cục cũ; màn gọn mới dùng CompactRunResult + UITextEffect. |
| `Editor/OfflineProgressionAuthoring.cs` | Dựng/nâng cấp prefab trong Edit Mode và thêm bản dịch. |

Giữ save key và stageId ổn định để không mất liên kết kỷ lục. Save cũ tự bắt đầu với danh sách kỷ lục rỗng. Kỷ lục mới được ghi cùng batch kết thúc lượt có sẵn; không phát sinh thao tác ghi đĩa mỗi hit hoặc mỗi frame đếm số.

## Kiểm tra đã thực hiện bằng MCP

- 72 kỹ năng thường đều bị loại khi đã max; 40 lượt lấy danh sách cho mỗi độ hiếm (160 lượt), không có kỹ năng max hoặc AbilityType trùng. Kiểm tra thêm đường lấy một phần thưởng theo rarity.
- Kỷ lục đầu tiên, lượt chậm hơn, DPS bằng nhau, chết sớm, vượt kỷ lục thời gian, tách độ khó/stage, thời gian 0 và serialize/deserialize đều qua kiểm tra.
- Luồng kết quả thắng/thua được kích hoạt trong save thử riêng với số liệu đặt trước: thắng cập nhật hai kỷ lục; thua không đổi thời gian nhanh nhất nhưng cập nhật DPS khi cao hơn. Đây là kiểm tra tích hợp kết quả, không phải hai lượt chơi tay đầy đủ.
- Quan sát animation bắt đầu từ 0 và kết thúc đúng khi `timeScale = 0`; đóng/mở lại không đổi cờ kỷ lục.
- Xem giao diện ở 3840×2160 và 1080×1920; kiểm tra reference trong cả hai prefab kết quả và chuỗi dịch của bốn ngôn ngữ ngoài Anh.
- Định dạng mốc 59,999 giây chuyển đúng thành 01:00.00, không xuất hiện 00:60.00.
- Save thật giữ nguyên hash sau phiên thử. Đã khôi phục Game View và xóa độ phân giải thử nghiệm tạm.

Trong lúc thử, thao tác dọn preset Game View qua MCP dùng sai loại chỉ số một lần và ghi lỗi Editor `Invalid index`; đã sửa thao tác và xóa đúng preset. Đây không phải lỗi từ gameplay. Không build player trong phạm vi kiểm tra.
