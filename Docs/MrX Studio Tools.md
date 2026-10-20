# MrX Studio — Hướng dẫn bộ công cụ The Archer

Menu của dự án nằm tại **Tools → MrX Studio**. Chọn **Tools Guide** để mở tài liệu này; **Readme** cũng có nút mở hướng dẫn. Tên namespace hiện tại là `MrX`, ví dụ `MrX.Armory`, `MrX.UI`, `MrX.EditorTools`.

Tài liệu đối chiếu source ngày 19/10/2026. Các tên lệnh dưới đây giữ đúng cách viết trong menu. Lệnh màu xám thường do đang ở sai chế độ Play/Edit; xem cột điều kiện. Console hiển thị kết quả các lệnh kiểm tra.

## 1. Chọn công cụ theo công việc

| Muốn làm gì | Công cụ nên dùng |
| --- | --- |
| Thử mua, quay, mở rương, nâng cấp bằng tiền | Nút **DEV** trên Stage, chọn mức rồi cấp Gold/Gems |
| Cấp một trang bị để thử | **DEV → EQUIPMENT**, chọn món rồi **+ 1 ITEM** |
| Chỉnh bố cục một trang menu | Mở prefab trong `Assets/The Archer/Prefabs/UI/Menu Pages` hoặc `UI/Stage`, chỉnh trong Prefab Mode |
| Chỉnh nhiệm vụ, thưởng, ngày reset | **Daily Quests → Select Database** |
| Chỉnh phần quà/xác suất vòng quay và rương | Inspector của `Scriptables/Lucky Rewards/Lucky Wheel.asset` hoặc `Gem Chest.asset` |
| Chỉnh mốc level và giá trang bị | Inspector của ItemData trong `Scriptables/Armory/Items` |
| Chỉnh mảnh và ba nguyên liệu ghép | Mục Crafting trên từng ItemData; rương boss ở `Scriptables/Boss Chest Rewards` |
| Chỉnh stage, phòng, wave, spawn | **Stage Creator** |
| Thử ability trong trận | **Abilities Tester** |
| Kiểm tra logic sau thay đổi | Các lệnh **Run ... Checks** hoặc **UI → Verify Bag Stats and Upgrades** khi không Play |

Các đường dẫn rút gọn trong tài liệu đều nằm dưới `Assets/The Archer`, trừ thư mục `Docs` ở gốc repository.

## 2. Các lệnh nhanh ở đầu menu

| Lệnh | Điều kiện và cách dùng | Tác động |
| --- | --- | --- |
| **Get Currencies (1K)** | Đang Play, chọn một lần | Cộng **1.000 cho từng currency đã đăng ký**, gồm gold và gem. Ở menu cấp vào ví lâu dài; khi StageController đang loaded cấp vào ví gameplay. Không phải chỉ cấp gold. |
| **Get Currencies (10K)** | Đang Play, chọn một lần | Tương tự trên, mỗi loại cộng 10.000. Muốn chọn riêng Gold/Gem hoặc item thì dùng DEV. |
| **Open All Stages** | Đang Play | Mở khóa tất cả stage trong Stage Database, sau đó tự thoát Play. Không đánh dấu đã hoàn thành tất cả stage. |
| **Delete Save File** | Đã thoát Play | Xóa save theo cơ chế SaveManager **và gọi PlayerPrefs.DeleteAll()**. Đây là lệnh xóa tiến trình, không phải dọn cache giao diện; không dùng trong luồng test thông thường. Sao lưu hồ sơ cần giữ trước khi chủ động dùng. |

Các lệnh cấp tiền/mở stage làm thay đổi tiến trình đang dùng. Lệnh cấp tiền nhanh không tự gọi Save ngay tại chỗ; dựa vào luồng save chung. Nút DEV lưu sau mỗi lần cấp và kiểm tra số lượng/tràn số nên phù hợp hơn khi cần cấp chính xác.

## 3. Achievements — thành tựu

| Lệnh | Cách dùng và kết quả |
| --- | --- |
| **Create Database If Missing** | Khi không Play. Tạo `Resources/Achievements Database.asset` nếu chưa có, gồm bộ định nghĩa ban đầu; nếu đã có thì chỉ chọn asset. Không ghi đè database đã cân bằng. |
| **Build Editable Page Prefab** | Khi không Play, database hợp lệ. Dựng và **ghi lại** `Prefabs/UI/Menu Pages/Achievements Page.prefab` từ builder. Chỉ dùng khi chủ động dựng lại bố cục; chỉnh giao diện hằng ngày bằng Prefab Mode. |
| **Run Logic Checks** | Khi không Play. Thử tiến độ, mốc nhận và nhận thưởng bằng dữ liệu kiểm tra riêng; xem Console. |

Để đổi danh mục, chỉ tiêu, icon và quà: chọn `Resources/Achievements Database.asset` trong Project. Giữ ID của thành tựu đã phát hành để save nhận đúng tiến trình.

## 4. Armory — trang bị, túi và stat

**Create Dashboard Settings If Missing** tạo `Resources/Armory Dashboard Database.asset` nếu thiếu; nếu đã có thì chọn asset đó. Chạy khi không Play. Trong Inspector có ảnh ô trống, font/màu, sức chứa ban đầu, số ô tăng thêm, giá vàng và giới hạn túi.

Để chỉnh giao diện, mở `Prefabs/UI/Menu Pages/Armory Page.prefab`. Để chỉnh số liệu trang bị, mở ItemData trong `Scriptables/Armory/Items`: các level/stat/price cũ vẫn là nguồn dữ liệu nâng cấp. Rèn tại Forge là ghép món chưa sở hữu từ mảnh và nguyên liệu; nâng level đồ đang có tại Stage/Armory.

Thử việc đầy túi bằng hồ sơ test hoặc DEV; trang bị mới nhận khi đầy túi được lưu chờ, không tự bỏ. Mặc định túi 40 ô, mở thêm 10 ô bằng gold theo giá Inspector.

## 5. Daily Quests — nhiệm vụ hằng ngày

| Lệnh | Cách dùng và kết quả |
| --- | --- |
| **Select Database** | Chọn `Resources/Daily Quests Database.asset` trong Inspector để chỉnh nhiệm vụ, mục tiêu, quà, các mốc thưởng và cấu hình reset. |
| **Build Editable Page Prefab** | Khi không Play, database hợp lệ. Ghi lại `Prefabs/UI/Menu Pages/Daily Quests Page.prefab` từ builder; có thể thay chỉnh sửa bố cục thủ công. |
| **Run Logic Checks** | Khi không Play. Kiểm tra tiến độ, reset và nhận thưởng một lần bằng save riêng; xem Console. |

Sau khi đổi dữ liệu, mở trang trong Play để xem số liệu thật. Muốn chỉnh kích thước, vị trí, hình nền hoặc nút thì sửa prefab, không dựng lại trang để thử từng thay đổi nhỏ. Giữ ID nhiệm vụ/mốc thưởng đang được save sử dụng.

## 6. Forge — ghép và nâng cấp trang bị

| Lệnh | Cách dùng và kết quả |
| --- | --- |
| **Create Database If Missing** | Tạo/chọn `Resources/Forge Page Database.asset`: reference Armory, sprite nền/nút, icon, font và material giao diện. Không thay ItemLevels. |
| **Build Editable Page Prefab** | Khi không Play, database có đủ reference. Ghi lại `Prefabs/UI/Menu Pages/Forge Page.prefab` từ builder; dùng Prefab Mode cho chỉnh sửa thường ngày. |
| **Install Crafting Assets** | Tạo cấu hình kho/loot crafting và prefab drop còn thiếu. Nếu database/drop đã có thì tái sử dụng. Không tự bật lại công thức của toàn bộ item. |
| **Enable Default Recipes For Registered Items** | Thao tác cân bằng có chủ đích: bật công thức cho các item đăng ký chưa bật. Mặc định 30 mảnh riêng và Iron/Wood/Leather lần lượt 10/5/2. Các công thức đã bật được giữ. Không chạy lệnh này để sửa một món riêng. |
| **Run Crafting Checks** | Khi không Play. Thử kho, công thức, điều kiện ghép, trạng thái sở hữu và phần tiêu tài nguyên bằng dữ liệu riêng. |
| **Run Level Upgrade Checks** | Khi không Play. Thử đường nâng level trang bị cũ, giá, giới hạn và tính nhất quán giao dịch. |

Xem [hướng dẫn crafting](Crafting.md). Muốn thay số nguyên liệu, mở từng ItemData và chỉnh mục **Crafting**; muốn thay xác suất rơi, chỉnh `Resources/Crafting Database.asset`. Hình và tên nguyên liệu có thể dùng tạm; không đổi enum/ID chỉ để thay icon.

## 7. Migration — chuyển dữ liệu giá cũ

**Migrate from Cost to Price** chỉ khả dụng khi không Play. Nó chuyển dữ liệu Cost kiểu cũ sang Price có currency cho HeroData, ItemData, UpgradeData và điều kiện mua Stage. Currency mặc định lấy từ Currencies Database, fallback là gold.

Đây là công cụ chuyển cấu trúc dữ liệu, không phải tăng giá hay reset level. Dùng khi nhập dữ liệu template cũ còn Cost; kiểm tra diff các asset sau khi chạy. Dự án hiện tại đã dùng Price nên không cần chạy mỗi lần sửa item. Cơ chế tự kiểm tra một lần theo EditorPrefs vẫn giữ nguyên sau đổi namespace.

## 8. UI — cài prefab và kiểm tra stat/túi/upgrades

| Lệnh | Cách dùng và kết quả |
| --- | --- |
| **Install Authored Menu Prefabs** | Mở scene **Main Menu**, thoát Play. Gắn các trang prefab đã vẽ, reference điều hướng và dock còn thiếu. Có thể thay trang cũ và lưu scene/prefab. Đây là lệnh tích hợp ban đầu, không dùng để refresh dữ liệu sau mỗi lần chỉnh Inspector. |
| **Verify Bag Stats and Upgrades** | Khi không Play. Kiểm tra sức chứa/mở rộng túi, snapshot stat và nâng cấp lâu dài bằng dữ liệu kiểm tra riêng; xem Console. |

Các prefab đang dùng là nguồn bố cục chính. Không tạo UI gameplay bằng `new GameObject`/`AddComponent` trong runtime. Có thể instance prefab hàng/ô đã author khi dữ liệu cần thêm; controller chỉ gán dữ liệu, sự kiện và trạng thái.

Xem [hướng dẫn prefab menu](Menu%20UI%20Prefabs.md).

## 9. Lucky Rewards — vòng quay và rương gem

| Lệnh | Cách dùng và kết quả |
| --- | --- |
| **Install Missing Assets and Stage Shortcuts** | Mở **Main Menu**, thoát Play. Tạo table/prefab còn thiếu, đăng ký gem nếu thiếu, gắn hai nút vào Stage và sửa indicator Gem theo currency gem. Tái sử dụng asset có sẵn; không phải lệnh rebuild bố cục popup đã chỉnh tay. |
| **Run Isolated Checks** | Khi không Play. Thử xác suất, biên random, thanh toán, receipt, các loại quà, đồ trùng và túi đầy bằng ví/save riêng. |

Chỉnh trong `Scriptables/Lucky Rewards/Lucky Wheel.asset` và `Gem Chest.asset`: giá gem, thời gian hiệu ứng, prize, loại quà, min/max và weight. Phần trăm bằng weight chia tổng weight đang bật. Các ô vòng quay **đều nhau để trình bày**; tỉ lệ thật ở bảng bên phải và quyết định kết quả theo trọng số. Icon/title có thể đổi mà không đổi ID prize/table.

Giá thử nghiệm hiện tại là 10 gem/vòng quay và 30 gem/rương, có thể đổi Inspector. Dùng DEV cấp gem rồi thử nút thật. Quà đã cấp/lưu trước animation; mở lại kết quả không cấp lần nữa. Xem [hướng dẫn Lucky Rewards](Lucky%20Rewards.md).

## 10. Stage — công cụ DEV và hình xem trước

**Install Developer Tools and Editor Preview** yêu cầu mở **Main Menu** và không Play. Lệnh thiết lập Developer Tools prefab/settings nếu thiếu, cập nhật tên mẫu nhân vật, aspect ảnh tĩnh và preview màn đầu. Stage Page được lưu thành variant riêng để không sửa Hero Panel dùng chung của Armory. Đây là thiết lập ban đầu; dùng `Prefabs/UI/Stage/Stage Page.prefab` cho chỉnh tay về sau.

Trong Play Mode, nút **DEV** ở bên trái Stage mở bảng cấp Gold/Gem/item. Mức tiền mặc định 100 / 1.000 / 10.000 / 100.000, bấm **AMOUNT** để chuyển mức. Mũi tên chọn trang bị; **+ 1 ITEM** cấp một món. Các lần cấp này ghi vào hồ sơ thật ngay.

Tắt tại `Scriptables/Developer Tools Settings.asset` bằng **Tools Enabled**, hoặc tắt GameObject Developer Tools. Mặc định chỉ cho Editor và Development Build; **Allow In Release Builds** tắt. Khi chuẩn bị phát hành, bỏ **Tools Enabled** nếu muốn loại công cụ khỏi màn hình trong mọi chế độ. Xem [hướng dẫn DEV/Stage](Developer%20Tools%20and%20Stage%20Preview.md).

## 11. Abilities Tester — Shift+A

Mở **Abilities Tester** hoặc nhấn **Shift+A**. Chọn Testing Database đang dùng và preset cần thử. Cấu hình ability/level trong preset, bật **Enabled In Editor** khi muốn áp vào lượt test. Giữ **Enabled In Build** tắt cho preset chỉ dành thử nghiệm.

Chạy qua Main Menu để các manager được khởi tạo, vào stage và kiểm tra ability/stat/hiệu ứng. Khi dùng các thao tác test trong cửa sổ, chú ý preset đang được chọn và chế độ áp dụng của source hiện hành. Thay preset có thể ảnh hưởng build nếu bật cờ dành cho build; không cần đổi script để thử một danh sách ability khác.

## 12. Default Scene Loader Enabled

Đây là công tắc có dấu tích. Bật: nút Play dùng Main Menu làm scene khởi động theo Scene Settings, kể cả khi bạn đang xem scene khác. Điều này cấp đầy đủ Save, Armory, Currency và các dịch vụ xuyên scene.

Tắt: Play bắt đầu từ scene đang mở. Dùng cho nội dung thử độc lập đã có bootstrap phù hợp. Cửa sổ xác nhận khi tắt là hành vi hiện có của công cụ; không phải yêu cầu hỏi lại mỗi lần Play. Trạng thái được lưu qua EditorPrefs.

## 13. Pipeline Switcher

Mở cửa sổ **Pipeline Switcher** để xem luồng chuyển Built-in sang URP của template. Các nút có thể cài package URP, import gói shader và chuyển material toàn dự án. Dự án hiện đã dùng URP; không chạy chuyển pipeline để sửa UI, nhân vật hay một material riêng.

Đổi namespace không cần chuyển pipeline. Shader đang dùng đã mang tên `MrX/Character`, `MrX/Character URP` và `MrX/Particles/...`; reference material vẫn theo GUID. Gói `.unitypackage` template cũ là tài nguyên gốc lưu trữ, chưa được đóng lại dưới thương hiệu mới; nếu chủ động import lại gói cũ thì phải kiểm tra namespace/shader và diff trước khi tiếp tục.

## 14. Stage Creator — Shift+S

Mở **Stage Creator** hoặc nhấn **Shift+S**. Chọn Stage Database, stage rồi room/wave cần sửa. Công cụ dùng scene authoring tạm để xem môi trường, vị trí enemy/chest/prop và thông số wave; không phải mỗi room là một scene trong game.

Quy trình: chọn stage → room/wave → chỉnh prefab/transform/thông số spawn/reward → lưu bằng thao tác Save của công cụ → mở lại để đối chiếu → test từ luồng bootstrap. Giữ ID, group shuffle, unlock conditions và prefab reference. Chức năng test có thể ghi vị trí stage/room/wave thử vào save; nhớ kiểm tra testing data nếu lượt chơi bắt đầu khác dự kiến.

Khi cửa sổ yêu cầu Default Scene Loader, kiểm tra công tắc ở mục 12. Sau chỉnh nội dung, kiểm tra hoàn tất wave/room, exit, camera và NavMesh phù hợp; xem diff để nhận ra scene/asset ngoài ý muốn.

## 15. Tools Guide và Readme

**Tools Guide** mở file Markdown này bằng ứng dụng mặc định của máy. **Readme** mở cửa sổ có tiêu đề MrX Studio, nút mở hướng dẫn dự án và phần tài liệu template gốc bên dưới. Link hướng dẫn mẫu, email và nguồn asset cũ chỉ là tài liệu tham khảo gốc.

## 16. Namespace MrX và tương thích dữ liệu

Đã chuyển namespace C#, tên type dạng chuỗi/reflection, UnityEvent/type metadata và managed reference Stage từ `OctoberStudio` sang `MrX`; menu Editor và Create Asset dùng **MrX Studio**. Lớp Input được sinh lại từ Input Actions với `wrapperCodeNamespace = MrX.Input`.

GUID script/asset, ItemData/Stage IDs, enum đã serialize, tên save và currency `gold`/`gem` giữ nguyên. Các điều kiện mở Stage có `MovedFrom` để Unity nhận diện type cũ khi nhập lại asset chưa chuyển. Không xóa save để áp dụng đổi namespace.

Tên thư mục `Assets/The Archer`, company/application identifiers và vị trí save hiện có được giữ để tránh tách hồ sơ người chơi. Các đoạn nhắc tác giả gốc, AssetOrigin của gói mua, EditorPrefs cũ và chuỗi `MovedFrom` là dấu vết nguồn/tương thích; chúng không phải namespace hay menu đang hoạt động.

Việc đổi tên không tự bảo đảm các save binary từ Unity cũ có assembly/type name sẽ đọc được; dự án Unity 6000.6 hiện dùng JSON. Nếu cần hỗ trợ dữ liệu binary từ một bản phát hành cũ, phải làm migration riêng và kiểm tra bằng dữ liệu của bản đó.

## 17. Đối chiếu sau chuyển tên

Ngày 19/10/2026 đã biên dịch trong Unity, đối chiếu 577 script theo GUID/type cũ, kiểm tra 569 prefab và cả ba scene Main Menu/Loading Screen/Game không có missing script. Bốn Stage đọc được điều kiện mở khóa; 275 material và bốn shader MrX hợp lệ. Menu đăng ký 29 lệnh MrX Studio, các tên lệnh có trong hướng dẫn này.

171 kiểm tra logic hiện có đạt bằng dữ liệu riêng. Play Mode nạp được JSON save ở vị trí cũ, Input dùng `MrX.Input.InputAsset`; các nút DEV, vòng quay và rương được thử bằng service/save riêng. Chưa kiểm tra bản build thiết bị hoặc toàn bộ nội dung combat.


## Loot — rương boss và ô túi

**Install Loot and Inventory Assets** (Edit Mode) chuyển cấu hình sang ba nguyên liệu Iron/Wood/Leather, tạo bảng boss/prefab rương, cập nhật icon và ô inventory/Forge/Lucky trong prefab. Có ghi asset và prefab; không dùng lệnh này để chỉ đổi một tỉ lệ. Bảng boss đã tồn tại được giữ nguyên. Đọc [Crafting](Crafting.md) để biết ưu tiên override và cách chỉnh từng trường thưởng.

Sau cập nhật loot có thêm lệnh này ngoài 29 lệnh của lần khảo sát namespace trước. Chỉnh bảng thưởng boss ở `Scriptables/Boss Chest Rewards`; Inspector có phần **Normalized chances**. Thưởng nguyên liệu chỉnh tại hai bảng Lucky Rewards. Quái nhỏ không phát các vật phẩm này.

## Kết toán run và ví gem

Bản đồ đường dẫn source/prefab và quy trình mới ở [Run Rewards and Save](Run%20Rewards%20and%20Save.md). Chỉnh rate từng boss trong Stage Creator như trước; không cần chạy Loot Install hoặc đổi Boss Chest Rewards để cập nhật Drop Overrides. Việc ghi thường trong combat được gom đến màn kết quả. Gem Wallet là prefab dùng lại trên năm tab ngoài Stage.
