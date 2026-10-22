# Nhân vật Adventurers và vũ khí theo lớp

Cập nhật 21/10/2026. Đường dẫn tính từ gốc dự án. Nội dung tái sử dụng model KayKit, rig/hiệu ứng Ranger và `AC_Hero_Ranger`; không thay hệ lưu tiến trình.

## Nội dung đã đăng ký

| Hero | Lớp | Mặc định | Starter Weapon |
| --- | --- | --- | --- |
| Ranger | Ranger | Hiện, mở sẵn, mặc định cũ | Signature Bow |
| Mage | Mage | Hiện, mua 500 gem | Mage Staff |
| Barbarian | Barbarian | Hiện, mua 750 gem | Barbarian Battle Axe |
| Knight | Knight | Ẩn, chưa mở | Knight Sword and Shield |
| Rogue | Rogue | Ẩn, chưa mở | Rogue Hand Crossbow |
| Rogue Hooded | Rogue | Ẩn, chưa mở; ID/level riêng | Rogue Dagger |

Database hiện có sáu hero và 15 vũ khí: ba món cũ cộng 12 món mới. Ranger giữ nguyên ID/level/save. Archer/Thief của template còn trên đĩa nhưng không tự đăng ký vào roster.

| Lớp | Vũ khí mới |
| --- | --- |
| Ranger | Ranger Longbow |
| Mage | Mage Wand, Mage Staff, Arcane Spellbook |
| Barbarian | Barbarian Axe and Shield, Barbarian Battle Axe |
| Knight | Knight Sword and Shield, Knight Greatsword, Royal Greatsword |
| Rogue / Rogue Hooded | Rogue Dagger, Rogue Hand Crossbow, Rogue Heavy Crossbow |

Bow/Cross bow cũ cho phép Ranger/Rogue. Ceremonial Wand cũ cho phép Ranger/Mage để giữ đường dùng của hồ sơ cũ. Món mới mặc định thuộc lớp tương ứng; sửa Allowed Classes nếu muốn mở rộng.

## Hiện, ẩn và mở khóa

1. Chọn HeroData trong `Assets/The Archer/Scriptables/Armory/Heroes/Adventurers/`. Ranger nằm ở `Assets/The Archer/Scriptables/Armory/Heroes/Hero Data 003.asset`.
2. Inspector có **Hero Class**, **Hide In Roster**, **Starter Weapon**, **Is Default Hero**, **Is Unlocked By Default** và Hero Levels.
3. Để phát hành Knight cần mua: tắt Hide In Roster, giữ Is Unlocked By Default tắt, chỉnh giá trong **Hero Levels → phần tử 0 → Price**.
4. Để thử ngay: tắt Hide In Roster, bật Is Unlocked By Default, dừng/chạy lại Play để nạp cấu hình.
5. Tắt Is Unlocked By Default không thu hồi hero đã sở hữu trong save. Hide In Roster vẫn ẩn và chặn chọn hero đó. Giữ ít nhất một hero mặc định hiện và hợp lệ; không xóa asset khỏi database chỉ để ẩn.
6. Không đổi ID/GUID khi chỉ đổi tên, giá hoặc trạng thái hiển thị. Không cần xóa save thật.

Hàng đổi hero có sáu nút đã lưu trong prefab. Runtime bật/tắt theo dữ liệu; HorizontalLayoutGroup chia khoảng trống. Nút mở popup hiện có để chọn/mua/nâng level. Rogue Hooded là biến thể dùng chung lớp và vũ khí Rogue nhưng có tiến trình riêng.

## Đổi lớp và túi đồ

`ArmoryManager.TrySelectHero` chỉ chọn hero hiện, đã mở và có thể có vũ khí phù hợp. Hệ thống giữ món đang cầm nếu đúng lớp; nếu không, ưu tiên starter đã sở hữu rồi một món phù hợp khác trong túi. Nếu chưa có, cấp starter level đầu một lần khi còn ô. Đổi qua lại không nhân bản hoặc tự tăng level.

Nếu túi đầy và thiếu món phù hợp, mở rộng túi trước khi chọn hero đó. Có vũ khí phù hợp sẵn thì vẫn chọn được khi túi đầy. Món sai lớp bị từ chối; popup ghi **Wrong class / Không đúng lớp**, nhưng vẫn có thể nâng level món đang xem.

Hero Levels và Item Levels tiếp tục là nơi chỉnh stat/giá/ability. Giá trị mới là cấu hình khởi tạo để tích hợp, chưa phải cân bằng phát hành cuối cùng. Loadout mới cập nhật model, weapon, slot và stat cùng nhau.

## Dữ liệu và prefab

| Nội dung | Đường dẫn |
| --- | --- |
| Database và mapping hero–weapon | `Assets/The Archer/Scriptables/Armory/Armory Database.asset` |
| HeroData mới | `Assets/The Archer/Scriptables/Armory/Heroes/Adventurers/` |
| WeaponData mới | `Assets/The Archer/Scriptables/Armory/Items/Adventurers/` |
| Prefab hero | `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/` |
| Prefab vũ khí | `Assets/The Archer/Prefabs/Weapons/Adventurers/` |
| Khiên, sách đóng và cốc trang trí | `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/` |
| Icon từ model | `Assets/The Archer/Sprites/UI/Adventurers/` |
| Material hero riêng | `Assets/The Archer/Materials/Heroes/Adventurers/` |
| Override controller, clip attack, WeaponAnimationsSet | `Assets/The Archer/Animations/Heroes/Adventurers/` |
| UI hàng đổi hero | `Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab` |

WeaponData có Allowed Classes, Is Right Hand Weapon, Offhand Visual Prefab, Is Melee và Attack Reach. Cặp kiếm/rìu + khiên là một món WeaponData; không thêm slot inventory thứ năm. Hình khiên không tự cấp stat: thiết kế phòng thủ trong Hero Levels/Item Levels.

MeleeWeaponBehavior trong prefab cho chỉnh Swing Angle, Extra Strike Damage, sound và particle tùy chọn. Đũa/trượng/sách/nỏ dùng WeaponBehavior và pool đạn cũ; chỉnh Projectile Spawn, Projectile Prefab, Shooting Sound tại prefab.

Vũ khí mới có recipe 30 mảnh riêng, dùng nguyên liệu hiện có. Không tự sửa rate boss để phát món mới. Muốn farm món nào, thêm đúng ItemData/mảnh vào bảng thưởng boss hoặc Lucky Rewards; xem [Crafting](Crafting.md) và [Lucky Rewards](Lucky%20Rewards.md).

## Rig, animation và combat

Prefab mới giữ skeleton Generic của Ranger, remap mesh/bones của model tương ứng và cập nhật RenderersHandler. Không đổi hàng loạt ModelImporter hoặc pipeline. Material riêng dùng MrX Character URP và palette của model để hỗ trợ hit/rim/status effects.

Nguồn animation: `Assets/The Archer/Models/Heroes/Ranger/Animation_Pack/`. Controller cơ sở: `Assets/The Archer/Models/Heroes/Ranger/AC_Hero_Ranger.controller`. Mỗi vũ khí có override controller; clip attack được chép ra `.anim` và có một event **Shoot** tại điểm ra đòn. Di chuyển/chết/hồi sinh tiếp tục theo vòng đời cũ.

Set mới bật **Normalize Attack Speed**, coi Attack Speed là số chu kỳ đánh mỗi giây. Set Ranger cũ giữ quy tắc tốc độ cũ. Khi thay clip, cập nhật Attack Animation Length và kiểm tra event. Chỉnh cách cầm bằng local transform của model dưới root prefab vũ khí; chỉnh pose ở override controller. Kiểm tra idle, chạy, contact/fire và camera gameplay, không chỉ T-pose.

Cận chiến dùng query không cấp phát trong trường hợp thường, kiểm tra range/góc/tường và khử collider trùng theo enemy. Damage đi qua TakeDamage cùng stat/crit/elemental effects cũ. Pause không gây damage; multishot đang chờ không tiếp tục đánh khi di chuyển. Front Shot bổ sung hệ số damage; Rear/Diagonal Shot mở vùng đánh; range modifier cập nhật detector/tầm đánh; Flying Weapon có nhịp đánh khi tách.

AbilityData có cờ **Ranged Weapons Only**. Đã bật cho Bouncy Shot, Piercing Strike, Ricochet, Split Shot, Bullseye và Sniper để không xuất hiện trong lựa chọn cận chiến. Attach/preset tự cấu hình vẫn phải chọn kỹ năng phù hợp; cờ không xóa ability trong save cũ.

## Công cụ và file logic

- **Tools → MrX Studio → Armory → Install Adventurer Roster**: bổ sung asset/mapping thiếu và dựng hàng nút trong prefab. Asset hero/weapon/animation đã có được giữ. Lệnh dựng lại hàng nút, nên xem diff; không cần chạy lại chỉ để sửa stat.
- **Rebuild Adventurer Portraits**: tạo lại PNG từ prefab, giữ `.meta`. Ghi đè ảnh đã chỉnh tay trong thư mục Adventurers; chỉ chạy khi muốn tạo lại icon. Cả hai lệnh yêu cầu Edit Mode.

| Logic | File |
| --- | --- |
| Authoring | `Assets/The Archer/Scripts/Editor/HeroRosterAuthoring.cs` |
| Lớp/flags | `Assets/The Archer/Scripts/Armory/HeroClass.cs` |
| Cấu hình hero | `Assets/The Archer/Scripts/Player/Hero/Data/HeroData.cs` |
| Cấu hình weapon | `Assets/The Archer/Scripts/Armory/WeaponData.cs` |
| Chọn hero/loadout/starter | `Assets/The Archer/Scripts/Armory/ArmoryManager.cs` |
| Cận chiến | `Assets/The Archer/Scripts/Armory/MeleeWeaponBehavior.cs` |
| Tay cầm/offhand/animation | `Assets/The Archer/Scripts/Player/Hero/HeroBehavior.cs` |
| Tầm detector | `Assets/The Archer/Scripts/Player/EnemiesDetector.cs` |
| Lọc kỹ năng | `Assets/The Archer/Scripts/Abilities/Data/AbilitiesManager.cs` |
| Preview | `Assets/The Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs` |
| UI roster/stat/túi | `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs` |

Tên mới có năm ngôn ngữ trong Menu Translations.asset; nguồn bổ sung ở Menu Translation Additions.txt. Xem [Menu Languages](Menu%20Languages.md) để nhập dịch/glyph. UI được lưu trong prefab, không tạo cây UI runtime.

## Kiểm tra

Đã kiểm tra compilation, prefab/bone/reference, 23 cặp hero–weapon, chọn qua sáu hero trong profile thử, từ chối sai lớp/hero ẩn, một weapon được trang bị và túi đầy khi thiếu starter. Smoke test Barbarian cận chiến và Mage bắn đạn trong run; kiểm tra cận chiến khi pause, ngoài tầm, có tường và collider trùng. VerificationSession giữ nguyên hash save thật.

Chưa phải xác nhận cân bằng, FPS trên mọi máy hoặc chơi hết mọi stage bằng mọi món. Chỉ build khi người dùng yêu cầu rõ ràng.

## Stage Showcase và chỉnh sửa ngày 21/10/2026

Mage và Barbarian hiện không mở sẵn với profile mới. Giá mặc định: Mage **500 gem**, Barbarian **750 gem**. Mở HeroData tương ứng, chọn **Hero Levels → Element 0 → Price**, đặt Currency Id là `gem` và Amount theo ý muốn. Các level nâng cấp sau vẫn giữ tiền tệ/giá đã cấu hình riêng. Hero đã mở trong save cũ được giữ nguyên; không thu hồi hoặc xóa save để áp giá mới. Popup kiểm tra lại số dư trước khi thay đổi trạng thái để từ chối giao dịch thiếu gem.

Barbarian có chỉ số gốc level đầu **220 HP, 55 Damage** (Ranger 100 HP/30 Damage, Mage 85 HP/35 Damage). Đây là chỉ số HeroData; số trên menu còn cộng trang bị/nâng cấp. Các level Barbarian cao hơn được tăng theo cùng tỷ lệ của đường tăng trưởng cũ. Có thể cân bằng từng level ở Inspector.

### Bố cục Stage

- Prefab chính: `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab`.
- Mở prefab trực tiếp để chỉnh `Hero Panel`, bốn ô trang bị và `Stage Showcase Card`. Nhân vật lớn ở trên; bảng stage rộng nằm dưới, tên bên trái, ảnh bên phải, hàng phần thưởng và Start ở đáy.
- Nền dùng `ui_slot` dạng Sliced. Không có cây UI mới được dựng trong runtime.
- `ResponsiveMenuLayout` chứa hai bộ vị trí Landscape/Portrait. Nếu chỉnh RectTransform rồi bị trả về khi Play, chỉnh thêm profile tương ứng trong component này. Runtime chỉ áp vị trí đã lưu.
- View ảnh stage dùng `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stage Icon.prefab`, mask chữ nhật bo góc và AspectRatioFitter Envelope Parent; ảnh giữ tỷ lệ và được cắt vừa vùng banner. Đổi Stage Image trong StageData để thay hình, không cần sửa code.
- Camera xem hero dùng `Assets/The Archer/Prefabs/UI/Armory/Hero Preview.prefab`; khung camera đã mở rộng để vừa mũ Mage/Barbarian. Hình hero giữ tỷ lệ vuông.
- Ô trang bị cập nhật theo vị trí slot khi đổi bố cục; hàng phần thưởng dùng anchor, không giữ kích thước grid cứng của bản cũ.

### Animation và điểm cầm vũ khí

`Assets/The Archer/Animations/Heroes/Adventurers/Loop *.anim` là các bản sao Idle/Walking/Running có Loop Time bật, không sửa FBX gốc. Các override controller Adventurers tham chiếu các clip này. Attack clip vẫn giữ đúng một sự kiện Shoot và logic dừng để đánh.

Chỉnh góc/vị trí model con trong `Assets/The Archer/Prefabs/Weapons/Adventurers/Mage Staff.prefab`, `Mage Wand.prefab`, `Barbarian Battle Axe.prefab`, `Barbarian Axe and Shield.prefab`. Giữ root weapon tại socket; chỉnh model con để không làm lệch logic projectile/weapon. Battle Axe được căn theo hướng giữa hai tay của tư thế Melee_2H_Idle; Staff nghiêng nhẹ ra ngoài, đúng đầu trượng hướng lên. Đây là attachment theo animation sẵn có, không phải IK hai tay cho mọi frame.

### Công cụ Editor

`Tools → MrX Studio → Stage → Apply Hero Showcase Layout` áp lại bố cục, camera, clip lặp và điểm cầm mặc định. Lệnh này ghi đè các thông số trình bày tương ứng; không dùng sau khi tự tinh chỉnh nếu muốn giữ bố cục riêng. Không đặt lại giá gem hoặc stat.

`Tools → MrX Studio → Armory → Apply Mage Barbarian Gem Defaults` đặt lại riêng giá mở khóa và stat mặc định nói trên; không tác động save đã sở hữu. Source: `Assets/The Archer/Scripts/Editor/StageShowcaseAuthoring.cs`.

### Phạm vi kiểm tra

Đã chạy trong Unity qua MCP với profile riêng: profile mới khóa Mage/Barbarian; thiếu gem không mở/trừ tiền; đủ gem mở đúng nhân vật và trừ đúng giá; thay stage cập nhật banner; đi/chạy Mage và Barbarian vượt qua nhiều vòng animation; kiểm tra trực quan bố cục landscape và profile portrait 1080×2160, đồng bộ ô trang bị/hàng thưởng. Kiểm tra portrait là profile trong Editor, chưa phải xác nhận trên thiết bị mobile thật. Không build. Save thật được kiểm tra hash sau khi kết thúc thử nghiệm.

Console: trong lượt Play cuối không ghi nhận error/warning gameplay. Sau domain reload, plugin MCP có một warning `[WebSocket] ... WebSocket is not initialised`; kết nối HTTP MCP vẫn hoạt động. Không quy warning plugin này thành lỗi gameplay và không khẳng định Console sạch tuyệt đối sau mọi lần reload.
