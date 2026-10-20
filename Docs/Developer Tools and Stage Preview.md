# Công cụ DEV và giao diện Stage

## Sử dụng

Trong Play Mode, mở trang Stage rồi bấm **DEV** bên trái. Bấm **AMOUNT** để đổi mức cấp, sau đó **+ GOLD** hoặc **+ GEMS**. Mặc định có 100 / 1.000 / 10.000 / 100.000. Dùng hai mũi tên chọn trang bị từ Armory Database và bấm **+ 1 ITEM**. Nút X hoặc Cancel đóng bảng.

Thao tác trên bảng thật lưu vào hồ sơ người chơi ngay. Tiền vào ví lâu dài, không vào ví trong trận. Trang bị đi qua đường cấp thưởng đang có: mở khóa Lv.1, không tự equip; đồ trùng tăng level; đồ MAX bù 100 gold theo mặc định delivery; đầy túi lưu vào hàng chờ để nhận khi có chỗ. Không có chức năng xóa/reset save.

## Bật/tắt và build

Chọn `Assets/The Archer/Scriptables/Developer Tools Settings.asset` trong Inspector:

- **Tools Enabled**: công tắc chung. Tắt thì ẩn nút DEV, đóng bảng đang mở và từ chối cấp tiếp.
- **Allow In Development Builds**: cho phép khi build có đánh dấu Development Build, mặc định bật.
- **Allow In Release Builds**: cho phép trong build phát hành, mặc định **tắt**.
- **Currency Amounts**: các mức tiền để bấm luân phiên.
- **Armory**: database trang bị dùng để chọn và kiểm tra ID.

Editor được phép khi công tắc chung bật. Để tắt hoàn toàn trước khi xuất bản, bỏ chọn **Tools Enabled**; bản release bình thường cũng tự ẩn theo cấu hình mặc định. Có thể tắt GameObject **Developer Tools** trong Stage prefab/scene nếu không muốn dùng công cụ này. Việc ẩn trong build được thực hiện lúc khởi tạo; không cần tháo script hoặc sửa code.

## Chỉnh giao diện

`Assets/The Archer/Prefabs/UI/Stage/Developer Tools.prefab` chứa toàn bộ nút, bảng, icon, nhãn và hai mũi tên. Có thể mở Prefab Mode để sửa và clone. Nền là ui_slot Sliced pastel. Runtime chỉ gán dữ liệu/event lên UI đã có.

`Stage Page.prefab` trong cùng thư mục là variant của Stage page cũ, lưu trực tiếp trong Main Menu scene. Nó giữ lại Hero Panel và Stage Panel lồng sẵn; không sửa prefab Hero Panel chung của Armory. Trong Editor, tên mặc định là Ranger Lv.1, ảnh nhân vật tĩnh giữ đúng tỉ lệ 256×512, ảnh Stage lấy từ phần tử đầu Stage Database. Các giá trị này chỉ là mẫu xem trước. Khi Play, tên/level cập nhật theo hero đang chọn và event level; hình 3D dùng render texture vuông; ảnh Stage theo màn được chọn. Ảnh mẫu Stage được tắt trước khi selector tạo icon thật để không có hai lớp hình.

`Tools/MrX Studio/Stage/Install Developer Tools and Editor Preview` là công cụ thiết lập ban đầu; dùng Prefab Mode cho các chỉnh sửa hằng ngày, không chạy lại để ghi đè bố cục đã chỉnh tay.

## Kiểm tra đã chạy

- 36 kiểm tra giao dịch/chọn thưởng/delivery và 10 kiểm tra công cụ DEV đạt trong Editor bằng dữ liệu tách riêng.
- Play Mode: nút chọn mức tiền, Gold/Gem/item, chuyển món, khóa dock và tắt/ẩn khi bảng đang mở; kiểm tra bằng service/save riêng.
- Vòng quay: kiểm tra bấm đúp, đóng/mở, kết quả lưu; tám ô đều nhau, icon cách đều, không phần trăm trên vòng, đúng góc trúng và animation khi timeScale=0.
- Quan sát Stage trước Play và popup vòng quay/DEV trong Game View. Chưa chạy bản build thiết bị.
- Đóng/mở lại scene xác nhận tên/cấp, tỉ lệ ảnh và ảnh mẫu Stage được lưu. Kiểm tra raycast xác nhận popup vòng quay chặn nút DEV phía sau; Canvas popup có sorting riêng khi mở.
- Đã sửa phụ thuộc thứ tự Start giữa Stage và ArmoryHeroPanel: pool/khung stat khởi tạo một lần trước Init, không chờ Start riêng của panel. Lần Play mới xác nhận selector sẵn sàng, bốn ô trang bị, ảnh mẫu tắt và ảnh 3D vuông; Console không lỗi lúc khởi động/thoát Play.

Ảnh đối chiếu nằm trong `Docs/Images/Stage Editor Preview.png`, `Lucky Wheel Equal Sectors.png` và `Developer Tools.png`.
