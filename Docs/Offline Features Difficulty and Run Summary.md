# Công tắc tính năng, độ khó và thống kê cuối run

Cập nhật 21/10/2026. Đây là tính năng offline; không có backend hoặc kết nối mạng được bật thêm.

## Bật/tắt thể lực và công tắc online dự phòng

1. Chọn `Assets/The Archer/Scriptables/Developer Tools Settings.asset`.
2. Trong Inspector, tìm **Công tắc tính năng game**.
3. **Bật hệ thống thể lực**: tắt = Start không kiểm tra hoặc trừ năng lượng, ẩn ví năng lượng/thời gian hồi/chi phí run trên các trang. Bật = dùng lại chi phí trong từng StageData, hồi phục và hạn mức mua cũ.
4. Công tắc này độc lập với `Tools Enabled`: tắt nút DEV không thay đổi luật thể lực.
5. Các mục Online, Cloud Save, BXH Online hiện chỉ là công tắc dự phòng. Bật chúng không tự tạo dịch vụ online. Hai mục con chỉ được phép khi Online bật.

Để nối UI online trong tương lai: gắn `GameFeatureGate` lên một object cha luôn hoạt động, chọn `feature` và kéo các object UI đã vẽ vào `targets`. Component chỉ ẩn/hiện các target theo công tắc, không tạo UI và không khởi tạo dịch vụ online. Không đặt controller bên trong target sẽ bị ẩn.

Asset cấu hình thực sự: `Assets/The Archer/Resources/Game Feature Settings.asset`, được tham chiếu từ Developer Tools Settings. Mặc định bản offline mới tắt thể lực và tất cả cờ online. Nếu dự án thiếu asset này, code giữ hành vi thể lực cũ để tương thích. Tiền năng lượng cũ không bị xóa.

Source: `Scripts/Development/GameFeatureSettings.cs`, `Scripts/Development/DeveloperToolsSettings.cs`, `Scripts/Editor/DeveloperToolsSettingsEditor.cs`, `Scripts/Toolkit/Currency/MenuEconomyService.cs` (các đường dẫn Scripts ở dưới `Assets/The Archer/`).

## Chọn và cân bằng độ khó

- Bộ chọn ở trang Stage: **Thường → Khó → Ác mộng**.
- Khó mở khi vượt Thường của đúng stage; Ác mộng mở khi vượt Khó của đúng stage.
- Chuyển sang stage chưa mở mức đang chọn sẽ tự về Thường. Chỉ đổi lựa chọn ở menu; run đã bắt đầu giữ nguyên mức đã lưu, kể cả khi resume.
- Save cũ không có độ khó được hiểu là Thường. Stage đã hoàn thành trong save cũ vẫn mở được Khó; không tự suy ra đã hoàn thành Khó/Ác mộng.

Mỗi `Assets/The Archer/Scriptables/Stages/Stage Data 00X.asset` có **Difficulty — Normal / Hard / Nightmare**. Các phần tử 0/1/2 tương ứng Thường/Khó/Ác mộng. Tham số ban đầu:

| Mức | Hệ số HP | Hệ số sát thương |
| --- | ---: | ---: |
| Thường | 1 | 1 |
| Khó | 1,5 | 1,25 |
| Ác mộng | 2 | 1,5 |

Hệ số này nhân với kết quả tính theo room/wave đang có. Spawn có HP/damage override cũng được nhân hệ số độ khó, còn Thường giữ giá trị cũ. Không thay đổi số quái, rate rương hoặc loot. Đây là điểm khởi đầu để playtest, chưa khẳng định đã cân bằng cho mọi hero/build. Báo cáo Balance đã tách nhóm theo độ khó; dữ liệu cũ mặc định Thường.

Source: `Scripts/Stage/StageDifficulty.cs`, `StageData.cs`, `StageSave.cs`, `ContinuePlayingSave.cs`, `StageController.cs`.

## Thành tựu và thưởng gem một lần

Mỗi stage có 3 thành tựu mới trong tab **Thành tựu → Adventure**. Sau khi vượt ải, vào tab Thành tựu để bấm nhận. Chấm đỏ dùng hệ thống đã có. Không tự cộng gem khi vừa thắng.

Mặc định: Thường **20 gem**, Khó **50 gem**, Ác mộng **100 gem**, mỗi thành tựu nhận đúng một lần. Muốn chỉnh thưởng, mở `Assets/The Archer/Resources/Achievements Database.asset`, tìm các mục có ID `stage-difficulty:<StageId>:0/1/2`, sửa `reward` (Currency ID = gem, Amount = số thưởng). Giữ ID ổn định để không tạo lại quyền nhận thưởng.

Save cũ phải vượt lại Thường để nhận thành tựu mới này; việc mở Khó từ tiến độ cũ không tự cấp thưởng lịch sử. Luồng bắt đầu bằng TestingData của Stage Creator không ghi thành tựu độ khó. Các kiểm tra Play sử dụng save cô lập.

## Bảng thống kê khi thắng hoặc chết

Từ bản cập nhật mùa/EXP ngày 22/10, màn kết quả gọn hiện trước. Bấm **Thống kê** để mở bảng chi tiết, **Đóng** để trở lại màn gọn và **Xác nhận** để về menu. Xem [Mùa, EXP và kết quả](Seasons%20Hero%20Experience%20and%20Results.md). Có thể cuộn chuột/kéo trong mỗi khung. Escape hoặc nút B trên tay cầm đóng bảng. Bảng không tự thay đổi timeScale; màn kết quả điều khiển pause.

- Thời gian chơi: scaled time khi nhân vật đang chơi và còn sống; không tính pause, thời gian xem kết quả hoặc chờ hồi sinh.
- Sát thương: lượng HP thực bị trừ ở mục tiêu; không tính phần overkill. Có tổng, tổng lên boss, đòn cao nhất và chia theo DamageType.
- Quái đã hạ bao gồm boss; cột boss là số boss trong tổng đó.
- HP mất/hồi, số lần trúng đòn: HP thực tế sau né/giảm sát thương, không tính đòn bị bất tử chặn.
- Đòn chí mạng/đòn trúng được đếm theo lần gây damage, không phải số viên đạn bắn ra; tick status cũng là một lần gây damage.
- Build: chỉ số tổng ở cuối run, ability/level, hero, trang bị và nâng cấp lâu dài. Một số ability có hiệu ứng không thể hiện đầy đủ bằng ATK/HP.

Số liệu nằm trong `ContinuePlayingSave`, đi cùng cơ chế lưu/resume hiện tại; không thêm việc ghi file sau mỗi đòn hoặc mỗi quái. Run cũ chỉ có số liệu từ thời điểm cài tính năng. Kết thúc run giữ bản tham chiếu thống kê cho màn kết quả rồi xóa phần run đang dở khỏi save theo luồng kết toán cũ.

Prefab sửa trực tiếp:
- `Assets/The Archer/Prefabs/UI/Offline Progression/Stage Difficulty Selector.prefab`
- `Assets/The Archer/Prefabs/UI/Offline Progression/Run Summary.prefab`

Hai prefab thống kê/thất bại gốc `Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab` và `Stage Failed.prefab` chứa instance bảng thống kê. Bố cục ngang/dọc được lưu bằng `ResponsiveMenuLayout`; runtime chỉ binding dữ liệu/áp profile và không dựng UI.

Source: `Scripts/Stage/RunCombatStatistics.cs`, `Scripts/UI/RunSummaryPresenter.cs`, `Scripts/UI/Main Menu/StageDifficultySelector.cs`. Các điểm ghi nhận nằm trong `EnemyBehavior.TakeDamage`, `PlayerBehavior.TakeDamage/Heal`, `RoomBehavior.ProcessDefeatedEnemy` và `StageController.Update`.

Công cụ dựng các asset còn thiếu: **Tools → MrX Studio → Offline → Install Feature Switches Difficulty and Run Summary**. Có sẵn prefab thì giữ nguyên; không dùng lệnh này để đặt lại thiết kế thủ công. Số thưởng đã tồn tại và các hệ số stage đã đủ ba mức được giữ lại khi chạy lại lệnh.


## Xác minh đã thực hiện

- Compilation/Console; kiểm tra logic thể lực bật/tắt, mua, hồi phục và prefab bằng công cụ kiểm tra kinh tế hiện có.
- Play: năng lượng 0 vẫn bắt đầu khi tắt; bật trở lại thì bị chặn; ví không bị trừ khi tắt.
- Mở khóa tuần tự, lưu/reload trạng thái, thành tựu đúng stage/mức và chống nhận hai lần. Thử nhận thành tựu Khó cho 50 gem, lần hai thất bại.
- Play: hệ số Khó 1,5 HP/1,25 damage; kiểm tra HP/damage override vẫn áp hệ số; Normal giữ giá trị cũ.
- Play: chết thực tế và thắng được kích hoạt qua luồng kết thúc stage trong save thử; bảng mở/đóng/mở lại không làm mất trạng thái pause. Resume giữ Hard và thống kê.
- Sát thương/hồi máu nhỏ hơn 1 HP vẫn được ghi nhận; không lấy số nguyên dùng cho text damage làm dữ liệu thống kê.
- Xem trực tiếp bố cục ngang 3840×2160 và dọc 1080×1920; khôi phục cấu hình Game View sau kiểm tra.
- Hai phiên save cô lập kết thúc với hash save gốc không đổi. Không chạy build, chưa playtest cân bằng toàn bộ stage/hero ở cả ba độ khó.
