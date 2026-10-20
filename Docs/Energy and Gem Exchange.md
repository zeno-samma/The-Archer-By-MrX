# Năng lượng và đổi gem — 20/10/2026

## Cách hoạt động

Ba ví Năng lượng, Gold, Gem xuất hiện trên Stage, Armory, Upgrades, Forge, Daily Quests và Achievements. Nút **+** sau năng lượng/gold mở bảng giá; chỉ bấm **Mua** trong bảng mới trừ gem. X, Escape hoặc gamepad Cancel đóng bảng. Bảng mua khóa điều hướng menu và phím tắt Stage trong lúc mở.

- Hồ sơ chưa từng có năng lượng được khởi tạo đầy một lần. Mặc định **30 năng lượng**.
- Start một run mới trừ `Energy Cost` của stage. Thiếu năng lượng không tăng số lượt thử, không chuyển scene và mở bảng mua. Nhấn Start lặp trong lúc chuyển màn không trừ thêm.
- Tiếp tục run dang dở không trừ năng lượng lại. Công cụ vào stage thử nghiệm trực tiếp của Editor giữ đường đi thử nghiệm cũ.
- Mặc định hồi **5 năng lượng mỗi giờ**, tính theo giờ đã trôi qua từ khi bắt đầu thiếu năng lượng. Không phải mỗi lần đồng hồ chuyển sang giờ mới.
- Khi đóng game, giờ đã trôi qua vẫn được tính lúc mở lại; phần thời gian lẻ được giữ. Khi đầy, dừng hồi và không tích trữ các giờ chờ. Khi tiêu từ mức đầy, bắt đầu đồng hồ mới.
- Gói mua mặc định: **10 năng lượng / 20 gem**, **1.000 gold / 10 gem**. Giá và số lượng thay được trong Inspector. Gói năng lượng vượt sức chứa bị từ chối toàn bộ, không trừ gem và không bán một phần gói với nguyên giá.
- Mua năng lượng tối đa **5 lần/ngày** mặc định, reset **00:00 UTC+7**. Chỉnh `dailyEnergyPurchaseLimit` và `purchaseResetUtcOffsetHours` trong cấu hình; 0 lượt tắt mua năng lượng. Chỉ mua thành công mới tính lượt; giữ số lượt qua reload, đồng hồ lùi không reset. Gold không dùng hạn mức này. Bảng mua có số lượt còn lại và khóa nút khi hết.
- Tiền và đồng hồ hồi dùng UTC của thiết bị. Nếu đồng hồ lùi, không cấp năng lượng cho khoảng thời gian lùi đó.

Ví cập nhật qua event; đồng hồ hiển thị được kiểm tra mỗi giây, không ghi save mỗi giây. Năng lượng đã trừ được lưu trước khi tải run. Mua thành công và hồi đủ một giờ mới yêu cầu lưu; trong Game scene, yêu cầu ghi vẫn đi qua cơ chế gom save/checkpoint đang có.

## Chỉnh cấu hình

| Muốn chỉnh | File / nơi chỉnh |
| --- | --- |
| Trần năng lượng, lượng hồi mỗi giờ, lượng mua và giá gem | `Assets/The Archer/Resources/Menu Economy Settings.asset` |
| Chi phí của mỗi màn | Chọn asset tại `Assets/The Archer/Scriptables/Stages/Stage Data 00X.asset` → **Run Energy / Energy Cost**; hoặc Tools → MrX Studio → Stage Creator → **Run Energy Cost** |
| Bố cục, màu, chữ, nút và bảng mua | `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Economy.prefab` |
| Đổi icon ví | Currency `energy` trong `Assets/The Archer/Scriptables/Currencies Database.asset` và Image trong prefab ví |
| Bản dịch | `Assets/The Archer/Resources/Menu Translations.asset` |

Giữ giá và lượng mua dương, trần và lượng hồi lớn hơn 0. Chi phí run bằng 0 cho phép màn miễn phí. Đặt chi phí cao hơn trần có thể khiến màn không vào được với sức chứa cấu hình hiện tại.

## Đường dẫn logic và save

| Chức năng | File |
| --- | --- |
| Cấu hình Inspector | `Assets/The Archer/Scripts/Toolkit/Currency/MenuEconomySettings.cs` |
| Hồi năng lượng, kiểm tra số dư, đổi gem, chặn overflow/reentry | `Assets/The Archer/Scripts/Toolkit/Currency/MenuEconomyService.cs` |
| Khởi tạo sau ví cũ, tick và lấy lại focus | `Assets/The Archer/Scripts/Toolkit/Currency/CurrenciesManager.cs` |
| Vào run mới và chặn chuyển scene lặp | `Assets/The Archer/Scripts/Other/GameController.cs` → `TryStartNewRun()` |
| Chi phí từng stage | `Assets/The Archer/Scripts/Stage/StageData.cs` → `energyCost` |
| Binding tiền, bảng mua và nút | `Assets/The Archer/Scripts/UI/Main Menu/MenuEconomyPanel.cs` |
| Tạo asset/prefab một lần trong Editor | `Assets/The Archer/Scripts/Editor/MenuEconomyAuthoring.cs` |
| Kiểm tra tự động bằng ví/save/đồng hồ giả | `Assets/The Archer/Scripts/Editor/MenuEconomyVerification.cs` |

Save giữ currency ID `energy` theo hệ ví cũ, cell **Energy Recovery** chứa cờ khởi tạo, timestamp UTC, `purchaseDay` và `energyPurchases`. Giữ nguyên ID `gold`, `gem`, các ID stage/item và những khóa save cũ. UI là prefab lưu sẵn, không dựng GameObject/component UI bằng runtime code.

Tools → MrX Studio → Economy → **Install Energy and Exchange Prefab** dành cho thiết lập ban đầu. Lệnh giữ prefab/cấu hình/bản dịch đã có; không dùng để ghi đè chỉnh layout thường ngày. **Run Energy Logic Checks** chạy kiểm tra với dữ liệu thử riêng.

## Bản dịch bổ sung

Ba trang Upgrades, Achievements và Forge đã được bổ sung tiêu đề, mô tả, trạng thái, thông báo và các mẫu có số liệu động. Tên trang bị của các công thức hiện có cũng được bổ sung. Chuỗi nhiều dòng được đối chiếu theo khoảng trắng; `{0}` và `{1}` giữ số liệu riêng và ưu tiên mẫu cụ thể trước mẫu tổng quát.

`Assets/The Archer/Resources/Menu Translation Additions.txt` là dữ liệu nhập ban đầu gồm năm cột Anh/Việt/Tây Ban Nha/Bồ Đào Nha/Pháp. Sau khi nhập, chỉnh bảng `.asset` trong Inspector. Tools → MrX Studio → Localization → **Import Additional Menu Translations** chỉ thêm khóa mới, giữ nội dung đã chỉnh của khóa cũ và bổ sung glyph font cần thiết. Nội dung mới do bạn thêm về sau cần thêm bản dịch tương ứng; không tự dịch tên dữ liệu hoặc đổi ID gameplay.

## Xác minh trong Unity Editor ngày 20/10/2026

- Logic năng lượng/đổi gem và reference prefab: 1.334 điều kiện đạt (bao gồm kiểm tra missing script trong cây sáu trang). Đã thử ranh giới một giờ, nhiều giờ offline, thời gian lẻ, đầy trần, lùi đồng hồ, reload, thiếu gem, giá không hợp lệ, overflow và reentry qua event ví.
- Bảng 187 mục dịch: 1.158 điều kiện đạt; đối chiếu độ phủ 128 tên/mô tả hiện tại của thành tựu, upgrade và item. Đổi qua năm ngôn ngữ trên chín hàng upgrade thực tế: 90 kiểm tra tiêu đề/mô tả đạt.
- Bấm nút thật trong Play: mua Gold, mua năng lượng, thiếu gem, thiếu năng lượng khi Start, khóa dock và đóng bảng. Sáu trang nhận đúng số dư chung.
- Vào Game bằng nút Start: 20 → 15 năng lượng với stage giá 5; yêu cầu Start lặp không trừ thêm. Dừng/mở Play lại, bấm tiếp tục run: giữ 15, không trừ lại. Từ chối tiếp tục cũng không trừ năng lượng.
- Đã xem giao diện Stage, Upgrades, Achievements, Forge và bảng mua; sửa ví gem cũ hiển thị trùng và vị trí Settings đè lên ví.
- Console lượt Play cuối: **0 lỗi, 0 cảnh báo**. Trong các lần domain reload trước đó có cảnh báo WebSocket của plugin MCP; lỗi biên dịch tạm thời trong công cụ cập nhật font đã được sửa và biên dịch lại thành công.
- Kiểm tra dùng hồ sơ riêng trong Temp/Verification. Hash save thật trước/sau giữ nguyên; đã kết thúc chế độ kiểm tra và trở về Edit Mode.

Chưa xác nhận build thiết bị hoặc mọi cấu hình nội dung bổ sung trong tương lai.

Cập nhật hạn mức, Quest, Stage 1 mặc định và bố cục ngang/dọc: [Responsive Menu and Daily Updates](Responsive%20Menu%20and%20Daily%20Updates.md). Các số kiểm tra phía trên mô tả vòng xác minh trước khi bổ sung hạn mức.
