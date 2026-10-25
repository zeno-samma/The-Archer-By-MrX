# Mùa nguyên tố, EXP hero và kết quả lượt chơi

Cập nhật 22/10/2026. Tính năng offline, không cần backend. Không build trong đợt này.

## Người chơi sẽ thấy gì?

- Mỗi run mới chọn một mùa. Thông báo xuất hiện trong 4 giây; bấm huy hiệu mùa để xem lại. Không dừng trận khi đọc thông báo.
- Xuân tăng Điện; Hạ tăng Lửa; Thu tăng Độc; Đông tăng Băng. Các đòn vật lý và sát thương của quái không nhận buff này.
- Hero đang tham gia nhận EXP khi thắng hoặc kết thúc thất bại. EXP không phải XP chọn kỹ năng trong trận. Đổi hero sẽ xem EXP riêng của hero đó.
- Thanh EXP nằm trên nhân vật trong Stage và Armory. Hero đã mở khóa không còn mua level bằng gold. Mở khóa Mage/Barbarian bằng gem vẫn giữ nguyên.
- Màn kết quả gọn hiển thị thời gian, kỷ lục, quái/boss, DPS cao nhất, mùa, phần thưởng và EXP. Nút **Thống kê** mở damage/build chi tiết; đóng lại vẫn giữ pause. Nút **Xác nhận** về menu.

## Chỉnh mùa và EXP trong Inspector

1. Thoát Play Mode, mở `Assets/The Archer/Resources/Run Progression Settings.asset`.
2. **Seasons Enabled** bật/tắt chọn mùa cho run mới. Run đang dở giữ mùa và hệ số đã lưu, kể cả khi đổi cấu hình giữa chừng.
3. Trong **Seasons**, chỉnh tên, mô tả, icon, màu, Weight, Damage Multiplier và Effect Multiplier. Weight là trọng số tương đối, không cần tổng bằng 100. Giữ giá trị enum mùa ổn định.
4. Mỗi `Assets/The Archer/Scriptables/Stages/Stage Data 00N.asset` có **Season Weights** theo thứ tự Xuân/Hạ/Thu/Đông. Trọng số thực = Weight toàn cục × trọng số stage. Tất cả bằng 0 sẽ không có mùa.
5. Hệ số mặc định: sát thương nguyên tố tương ứng ×1,15; hiệu ứng phụ ×1,10. Xuân tăng damage điện lan, Hạ tăng thời gian cháy, Thu tăng mức suy yếu do độc, Đông tăng thời gian đóng băng. Vẫn giữ kháng hiệu ứng/cooldown của mục tiêu; không tự tăng số lần stack hay tạo thêm VFX.
6. `AbilityData` có **Season Affinity**. Trong ba level XP đầu của run (`XPLevel <= 3`), kỹ năng hợp mùa có trọng số chọn ×1,5 trong pool cùng rarity. Đây là tăng cơ hội, không đảm bảo xuất hiện; không thay rarity, prerequisite hay bỏ giới hạn max. Đường thưởng đơn từ rương vẫn dùng lựa chọn cũ.

Thông báo dùng icon nguyên tố và nền pastel có sẵn. Chưa thêm hệ thời tiết vật lý như mưa/tuyết hoặc đổi ánh sáng toàn map.

### Công thức EXP hero

```
quái thường = max(0, tổng quái đã hạ - boss đã hạ)
EXP = floor((quái thường × 8 + boss × 80 + thưởng clear)
            × hệ số stage × hệ số độ khó × hệ số kết quả)
```

- Thưởng clear mặc định 120 khi thắng, 0 khi thua.
- Hệ số stage ban đầu: stage 1–5 lần lượt 1 / 1,25 / 1,5 / 1,75 / 2; chỉnh **Hero Experience Multiplier** ở từng StageData.
- Thường/Khó/Ác mộng: 1 / 1,5 / 2 trong **Difficulty Experience**.
- Thắng ×1; thua ×0,75 trong **Defeat Experience Multiplier**. Không có kill và không clear thì nhận 0.
- Ví dụ stage 1, 12 quái gồm 2 boss: Thường thắng = 360 EXP, Khó thắng = 540 EXP, Thường thua = 180 EXP. Boss không bị cộng hai lần như quái thường.
- Các số 8/80/120 đều chỉnh được ở cấu hình chung. Giới hạn bảo vệ mỗi run là 1.000.000 EXP.

Ngưỡng từ Lv.1 lên Lv.2 mặc định 250 EXP, mỗi level sau thêm 150. Trong từng `HeroData → Hero Levels`, **Experience To Next Level** lớn hơn 0 sẽ ghi đè ngưỡng chung cho mốc đó; 0 dùng công thức chung. Một lần nhận có thể lên nhiều level. Đạt level cuối của danh sách thì hiện MAX và không tích EXP dư. Muốn tăng cap, thêm mốc HeroLevel với stat/ability như trước.

Giữ các level đã nâng bằng gold trước đây; save cũ bắt đầu với EXP dư bằng 0 tại level hiện có. Không hoàn gold và không xóa save. `HeroLevel.Price` vẫn tồn tại để đọc dữ liệu cũ và dùng giá mở khóa ở mốc đầu; các giá nâng level hero phía sau không còn được UI sử dụng. Gold nâng trang bị/permanent upgrades không bị thay đổi.

## Kỷ lục và lưu dữ liệu

- Kỷ lục theo `stageId + độ khó`, dùng chung giữa các hero/build.
- Thắng: thời gian clear **thấp nhất**. Thua: thời gian sống **dài nhất**, lưu riêng nên chết sớm không phá kỷ lục clear.
- DPS cao nhất là **DPS trung bình cả run** = tổng HP thực gây sát thương / thời gian hoạt động. Không phải đỉnh DPS trong một giây. Hiện thêm DPS lượt hiện tại để so sánh.
- Thời gian và DPS đếm từ 0 trong 2,4 giây bằng unscaled time. Nhãn kỷ lục hiện sau khi đếm; không dùng giá trị đang chạy trên UI để tính record.
- Bảng chi tiết giữ tổng damage, damage boss, damage theo loại, HP mất/hồi, số hit/crit và build. DPS cao nhất nằm ở màn gọn, không lặp trong bảng chi tiết.
- `Armory`: level và EXP dư theo Hero ID. `Continue Playing`: hero ID của run, mùa, hệ số mùa và thống kê để resume. `Run Records`: clear, survival, DPS và mùa của từng record.
- EXP và loot được kết toán qua `StageController.FinalizeRunRewards`, cùng batch lưu cuối run. Gọi kết toán lần nữa trong cùng run không cấp lại EXP. Không thêm ghi đĩa theo từng kill hoặc từng frame đếm số.
- Lượt TestingData từ Stage Creator không cấp EXP/kỷ lục thật. Hồ sơ verification dùng save riêng.

## Prefab và source để chỉnh lại

Tất cả đường dẫn dưới đây tính từ thư mục dự án.

| File | Vai trò |
| --- | --- |
| `Assets/The Archer/Prefabs/UI/Run Progression/Hero EXP Bar.prefab` | Portrait, tên/level, track/fill và chữ EXP; dùng chung menu/Armory/kết quả |
| `Assets/The Archer/Prefabs/UI/Run Progression/Compact Run Result.prefab` | Màn kết quả gọn, CountNumber/Pop trên TMP, profile ngang/dọc |
| `Assets/The Archer/Prefabs/UI/Run Progression/Result Reward Tile.prefab` | Thẻ phần thưởng được instance vào ScrollRect |
| `Assets/The Archer/Prefabs/UI/Run Progression/Run Season HUD.prefab` | Huy hiệu, thông tin mùa và nút đóng |
| `Assets/The Archer/Prefabs/UI/Offline Progression/Run Summary.prefab` | Bảng chi tiết; Record Highlights cũ bị tắt trong hai màn kết quả |
| `Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab`, `Stage Failed.prefab` | Liên kết màn gọn với nút thống kê |
| `Assets/The Archer/Resources/Menu Translations.asset` | Chuỗi của năm ngôn ngữ hiện có |
| `Assets/The Archer/Scripts/Stage/RunProgressionSettings.cs` | Công thức EXP, mùa, buff và chọn trọng số |
| `Assets/The Archer/Scripts/Armory/HeroSaveData.cs` | EXP dư và lên level |
| `Assets/The Archer/Scripts/Player/Hero/Data/HeroData.cs` | Mốc chỉ số/ability hero và ngưỡng EXP riêng từng level |
| `Assets/The Archer/Scripts/Stage/StageController.cs` | Gắn mùa/hero vào run và kết toán EXP |
| `Assets/The Archer/Scripts/UI/HeroExperienceBar.cs`, `CompactRunResult.cs`, `RunSeasonView.cs` | Binding view, tương tác và animation |
| `Assets/The Archer/Scripts/Editor/SeasonalProgressionAuthoring.cs` | Lệnh cài/đấu nối prefab trong Edit Mode |

Chạy **Tools → MrX Studio → Offline → Install Seasons Hero EXP and Compact Results** khi Main Menu đang mở trong Edit Mode. Lệnh tạo prefab/config còn thiếu, đấu nối scene/prefab và thêm bản dịch. Prefab đã tồn tại được giữ thiết kế; không dùng lệnh để reset layout thủ công. Script runtime chỉ binding và instance thẻ prefab, không tạo cây UI mới.

## Kiểm tra

Đã kiểm tra qua Unity MCP bằng hồ sơ save cô lập: công thức EXP/boss không đếm trùng, ngưỡng/multi-level/MAX/serialize, chặn nâng gold và mở Mage bằng gem, resume giữ mùa/hero/số liệu, kết toán thắng/thua và chống cấp lặp, mở/đóng thống kê giữ pause. Đã xem Armory và kết quả ở màn dọc 1080×1920, cùng kết quả/menu ngang 3840×2160. Các lượt kết quả dùng số liệu kiểm soát để kiểm tra tích hợp; chưa phải playtest cân bằng đầy đủ mọi stage/hero/độ khó. Đã kiểm tra thêm 72 kỹ năng max vẫn bị loại và 40 lượt lựa chọn fallback không trùng, 10.000 lần chọn mùa, tắt mùa/trọng số 0 và chỉ một mùa có trọng số. Đã sửa lớp fade che kết quả và kiểm tra khi kết thúc ngay giữa chuyển cảnh. Console cuối đợt không có Error; save thật giữ nguyên hash sau các phiên thử. Không build và không dùng số đo Editor làm bảo đảm hiệu suất thiết bị.
