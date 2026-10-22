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

## Hai thẻ nổi bật đầu bảng kết quả

**Thời gian:** hiện thời gian vượt ải khi thắng; hiện thời gian sinh tồn khi thua. Chỉ lượt thắng mới cạnh tranh kỷ lục vượt ải nhanh nhất. Chết sớm không được xem là phá kỷ lục thời gian.

**DPS trung bình:** tổng sát thương thực gây ra chia cho `RunCombatStatistics.seconds`. Bao gồm sát thương thường, crit và các loại damage đã được hệ thống thống kê ghi nhận. Đây là DPS trung bình toàn lượt theo thời gian hoạt động hiện hữu, không phải DPS đỉnh hay thời gian chỉ đứng bắn. Tổng sát thương được hiển thị riêng ngay dưới số DPS để tránh nhầm hai khái niệm.

Kỷ lục được so riêng theo **stageId + độ khó**, dùng chung giữa các hero/build của người chơi. Thời gian thấp hơn là tốt hơn; DPS cao hơn là tốt hơn. Lượt thua vẫn có thể lập kỷ lục DPS. Trường hợp bằng kỷ lục không đánh dấu mới; lần hợp lệ đầu tiên được coi là kỷ lục đầu tiên. Lượt có thời gian bằng 0/không hợp lệ không lập kỷ lục. Kỷ lục không được suy ngược từ các run cũ chưa từng được ghi.

Mỗi số chạy từ 0 tới giá trị thực bằng thời gian không phụ thuộc pause, mặc định **2,4 giây**, chậm dần ở cuối. Hai nhãn **KỶ LỤC MỚI!** xuất hiện khi đếm xong nếu đạt điều kiện. Đóng/mở lại bảng hiển thị ngay giá trị cuối, không ghi thêm kỷ lục. So sánh vẫn dùng giá trị gốc, không dùng số đang chạy hoặc số đã làm tròn trên UI.

## Chỉnh giao diện và hiệu ứng trong Inspector

1. Mở prefab `Assets/The Archer/Prefabs/UI/Offline Progression/Run Summary.prefab`.
2. Mở `Statistics Panel → Record Highlights`.
3. `Time Record Card` và `DPS Record Card` chứa Heading, Value, Previous Best, New Record Badge; thẻ DPS còn có Total Damage.
4. Chỉnh RectTransform, TMP, màu nền sliced trực tiếp tại đây. Không cần sửa code runtime để đổi hình thức thẻ.
5. Component `RunRecordHighlights` trên Record Highlights có **Count Duration** và **Record Color**, cùng chín reference TMP.
6. Component `ResponsiveMenuLayout` trên root quản lý bố cục hai cột ngang và hai vùng cuộn xếp dọc. Thẻ kỷ lục nằm đầu bảng ở cả hai hướng.

Prefab trên được lồng trong cả `Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab` và `Stage Failed.prefab`.

Lệnh **Tools → MrX Studio → Offline → Upgrade Run Summary Records** thêm các thẻ còn thiếu và các chuỗi dịch vào prefab hiện tại. Có thể chạy lại mà không tạo thêm thẻ; khi Record Highlights đã tồn tại, lệnh giữ layout thẻ đã chỉnh tay. Lệnh không reset save/kỷ lục. Giao diện chỉ được dựng lúc authoring trong Editor và lưu vào prefab, không dựng bằng code trong player.

Hỗ trợ tiếng Anh, Việt, Tây Ban Nha, Bồ Đào Nha và Pháp qua `Assets/The Archer/Resources/Menu Translations.asset`.

## File xử lý và lưu dữ liệu

| Đường dẫn trong `Assets/The Archer/Scripts/` | Trách nhiệm |
| --- | --- |
| `Stage/RunRecordsSave.cs` | Save key mới `Run Records`, danh sách kỷ lục; so sánh thời gian/DPS và giữ giá trị trước đó cho UI. Không có file save riêng. |
| `Stage/RunCombatStatistics.cs` | Công thức DPS và kết quả so sánh tạm thời của lượt; không dùng giá trị UI để tính damage. |
| `Stage/StageController.cs` | Ghi kỷ lục tại FinalizeRunRewards trước lần commit cuối lượt, chỉ một lần; lượt testing qua Stage Creator không lập kỷ lục. |
| `UI/RunSummaryPresenter.cs` | Binding bảng kết quả và hai thẻ mới. |
| `UI/RunRecordHighlights.cs` | Hiệu ứng số dùng unscaled time, nhãn kỷ lục và định dạng thời gian. |
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
