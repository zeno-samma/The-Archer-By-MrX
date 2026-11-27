# ArcBorn Challenge Playtest

Bộ cân bằng đã được áp dụng vào **Secrets Pyramid** và **Ancient Temple** trong Journey. Lượt mới dùng cùng đội hình, độ khó, rương và cơ chế boss đã chơi thử. Lượt Journey đang dở tiếp tục theo bố cục lúc bắt đầu, rồi áp dụng cân bằng mới từ lượt sau. Cấp trang bị, nâng cấp và tiến trình mở khóa vẫn dùng hồ sơ Journey.

## Mở bản thử

Trong Unity, mở scene **Main Menu** ở Edit Mode, rồi vào **Tools → MrX Studio → Challenge Playtest**. Chọn ải, độ khó và bấm **Mở bản chơi thử**. Bấm **Start** trong game để vào trận.

Thiết lập mặc định là **Secrets Pyramid → Basic**, với hồ sơ mới. Chơi và nhận thưởng ở Basic, dùng vàng trong Armory hoặc Upgrade, rồi vào Intermediate. Thắng Intermediate sẽ mở Hard. Menu tự chọn độ khó vừa mở khóa. Hãy giữ Unity ở Play Mode giữa các lượt để dùng cùng hồ sơ thử.

Bật **Sao chép trang bị và tiến trình hiện tại** nếu muốn thử với hero, cấp vũ khí, trang bị, nâng cấp và vàng trong save Editor đang dùng. Mỗi phiên mở từ công cụ có save riêng. Thoát Play và mở công cụ lại sẽ bắt đầu một phiên thử mới.

Để chỉnh đội hình, chọn **Mở dữ liệu trong Stage Creator**. Nếu công cụ yêu cầu scene Game, dùng **Open Game Scene** để nạp Game cùng Main Menu. Chọn phòng hoặc đợt để sửa spawn, rương, XP và phần thưởng. **Play Difficulty Trial** ở trang ải mở một lượt thử từ đầu với độ khó đã chọn. **Use Journey Stage Database** chuyển lại bộ dữ liệu Journey trong công cụ.

## Đòn boss và cảnh báo HP trong Secrets Pyramid

- **Mysterious Mask, phòng 19.** Mở trận bằng các loạt sóng xen kẽ. Loạt thứ hai bắn vào giữa các làn của loạt đầu, rồi nhịp tiếp theo đổi lại. Khi còn 50% HP, Mask rút ngắn khoảng nghỉ và có thể nối ngay một loạt sóng sau đòn triệu hồi tay.
- **Ice Slime, phòng 28.** Khi còn 55% HP, boss chuyển sang đánh dấu vị trí người chơi bằng tinh thể băng theo từng lần tung chiêu. Tinh thể có vòng báo trước khi gây sát thương. Đòn mảnh băng ở pha này bắn theo quạt ba hướng, mỗi mảnh gây 70% sát thương của một mảnh ở pha đầu.
- **Cảnh báo HP thấp.** Viền đỏ nhấp nháy từ 20% HP, mờ dần vào giữa để vùng chiến đấu giữ nguyên màu. Độ đục tối đa ở sát mép tăng từ 20% lên 48% khi HP còn 10%. Viền nằm sau HUD để nút và joystick tiếp tục nhận thao tác. Cảnh báo tắt khi hồi trên 20% HP, mở trang tạm dừng hoặc vào màn kết quả. Màu và độ đục chỉnh được ở **Game → Canvas → Low Health Overlay** và component **Game Screen Behavior** trên Canvas.

Đã quan sát các loạt sóng xen kẽ và chuỗi tay → sóng của Mask trong Play Mode. Ice Slime đã gọi tinh thể ngay tại vị trí nhân vật và bắn quạt ba mảnh qua sự kiện animation. Đã chụp viền đỏ ở 20% và 10% HP trong phòng Mighty Cactus và xác nhận viền tắt khi tạm dừng hoặc hồi trên 20% HP. [Ảnh kiểm tra](Images/Low%20Health%20Border%20Playtest.png)

Luồng tiến trình được kiểm tra trong một hồ sơ thử mới với kết quả thắng và vàng được tạo có kiểm soát. Sau khi nhận thưởng Basic, giao dịch nâng cung dùng 100 vàng và Intermediate mở ngay trên menu. Sau Intermediate, giao dịch tiếp theo dùng 200 vàng để nâng cung lên cấp 3 và Hard mở đúng thứ tự. Các phép kiểm tra này xác thực nhận thưởng và lưu tiến trình giữa lượt. Cảm giác chiến đấu ở ba lượt chơi trọn ải vẫn chờ kiểm tra trực tiếp.

## Những gì đã thay đổi

Ba lượt Secrets Pyramid trước khi chỉnh có tổng sát thương nhận vào lần lượt là 90,45, 104,8 và 139,6 HP. Cả chín lần gặp boss trong ba lượt đều ghi nhận **0 sát thương nhận vào**. Ice Slime kết thúc trong 5,37–20,58 giây. Rương hồi ngay trước Ice Slime có thể đưa máu về đầy trước khi đánh boss. Nhật ký cũ thiếu cột độ khó, nên không gán từng dòng cho Basic, Intermediate hay Hard.

| Ải | Điểm spawn cũ → thử | Đợt quái thường trong bản thử | Rương hồi hoặc tăng HP | Rương Evil trước ba boss |
| --- | ---: | --- | --- | --- |
| Secrets Pyramid | 167 → 263 | 7–11 quái/đợt đầu và giữa ải, 9–13 ở các phòng cuối | Phòng 5, sau combat phòng 14, phòng 23 | Phòng 9 → Cactus 10, phòng 18 → Mask 19, phòng 27 → Ice Slime 28 |
| Ancient Temple | 105 → 240 | 7–12 quái/đợt thường trong cùng đấu trường | Đợt 5, 16, 27 | Đợt 10 → Totem 11, đợt 21 → Queen Wasp 22, đợt 32 → Golem 33 |

Mỗi đợt chiến đấu đều được tăng quân. Chẳng hạn Pyramid phòng 1 có 3 Candle, 2 Bomb, 1 Slime, 1 Spike và 1 Bat. Spike và Bat gây áp lực từ hai bên, Candle phủ đạn từ hàng sau. Temple đợt 1 có 2 Smaller Slime, 2 Wasp, 1 Candle, 1 Jelly và 1 Spike. Các đợt sau đổi sang đội hình gồm quái lao tới, bắn xa và khóa vùng. Sáu boss giữ đòn đánh sẵn có và có thêm 3–4 hộ vệ. Đợt boss chỉ kết thúc khi cả boss, hộ vệ và các điểm spawn đang chờ đã được xử lý.

Rương hồi giữ hai lựa chọn **Heal** và **Minor Health Boost**. Rương Evil mất **22% HP tối đa, trừ trực tiếp vào HP hiện tại** khi nhận ability. Có thể từ chối. Nếu trả phí sẽ đưa HP về 0 hoặc thấp hơn, nút nhận bị khóa. Nhờ vậy lượng máu mang vào boss phụ thuộc vào lựa chọn ở rương và cách vượt các đợt trước.

Hồi tự động sau đợt dùng 30% hiệu lực cũ trong bản thử. Quái thêm vào không tăng lượng Heal, Rage Star và Invincibility Star rơi ngẫu nhiên. Rage của Ranger nạp 60% lượng cũ cho quái thường ở Pyramid và 55% ở Temple, làm tròn khi cộng điểm. Cấp vũ khí, chỉ số trang bị và giá nâng cấp dùng dữ liệu hiện hành. Vàng thưởng đợt giữ 1.639 ở Pyramid và 1.896 ở Temple. XP mỗi ải trong bản thử là 1.255 và 1.282, đạt mốc kỹ năng từ XP vào khoảng đợt chiến đấu thứ ba.

## Các mức độ và boss

| Ải | HP thường Basic, gốc và bước tăng | Sát thương thường Basic, gốc và bước tăng | Intermediate, HP và sát thương so với Basic | Hard, HP và sát thương so với Basic |
| --- | --- | --- | --- | --- |
| Secrets Pyramid | ×1,22, thêm 0,056 mỗi phòng và 0,035 mỗi đợt trong phòng | ×1,12, thêm 0,034 mỗi phòng và 0,035 mỗi đợt | ×1,26 và ×1,22 | ×1,62 và ×1,50 |
| Ancient Temple | ×1,42, thêm 0,039 mỗi đợt | ×1,21, thêm 0,026 mỗi đợt | ×1,28 và ×1,22 | ×1,64 và ×1,50 |

Đội hình được giữ giống nhau giữa ba độ khó để so sánh cùng bố cục. Boss có hệ số HP riêng ×1,90 ở Pyramid và ×2,03 ở Temple. Sát thương boss nhân thêm ×1,22 và ×1,20. Hộ vệ dùng hệ số quái thường của đợt.

| Boss | HP Hard trước | HP Basic thử | HP Intermediate thử | HP Hard thử |
| --- | ---: | ---: | ---: | ---: |
| Mighty Cactus | 2.474 | 4.913 | 6.191 | 7.960 |
| Mysterious Mask | 3.584 | 7.281 | 9.174 | 11.795 |
| Ice Slime | 4.936 | 10.174 | 12.819 | 16.482 |
| Cursed Totem | 3.282 | 6.154 | 7.878 | 10.093 |
| Queen Wasp | 4.306 | 8.272 | 10.588 | 13.566 |
| Ancient Golem | 5.682 | 11.103 | 14.212 | 18.209 |

## Kết quả thử trong Unity

Các phép đo dưới đây dùng tự bắn của game, giữ nhân vật đứng yên trong đợt đầu và dừng khi qua đợt hoặc bị hạ. Trừ ca Pyramid Basic, cả năm ca dùng cùng bản sao hồ sơ với Ranger, Signature Bow, 82 ATK, 201 HP và tốc độ đánh 2,04 đòn/giây trước lá bài mở màn.

| Ải | Độ khó | Quái đợt đầu | Kết quả đứng yên |
| --- | --- | ---: | --- |
| Secrets Pyramid | Basic, hồ sơ mới | 8 | Bị hạ sau 6,1 giây, còn 5 quái |
| Secrets Pyramid | Intermediate | 8 | Bị hạ sau 13,5 giây, còn 2 quái |
| Secrets Pyramid | Hard | 8 | Bị hạ sau 11,5 giây, còn 3 quái |
| Ancient Temple | Basic | 7 | Qua sau 8,6 giây, còn khoảng 80% HP |
| Ancient Temple | Intermediate | 7 | Qua sau 10,5 giây, còn khoảng 56% HP |
| Ancient Temple | Hard | 7 | Qua sau 14,3 giây, còn khoảng 21% HP |

Với checkpoint kiểm tra trước hai boss cuối Hard, mở rương Evil bằng va chạm trong phòng và chọn nhận ability. Trên bộ 201 HP tối đa, giá làm tròn lên thành **45 HP**, đưa HP hiện tại từ 140,7 xuống 95,7. HP tối đa vẫn là 201. Sau đó Ice Slime nạp 16.481,8 HP và Golem nạp 18.208,7 HP, mỗi trận cùng bốn hộ vệ. Trong phép thử tiếp tục đứng yên, hero bị hạ sau 7,9 giây ở Ice Slime và 3,2 giây ở Golem. Đây là thử sức ép từ checkpoint với 70% HP trước rương, dùng để đo hậu quả khi đứng yên.

Đã kiểm tra sáu đợt đầu, hai đoạn Evil → boss cuối và hai lá bài của rương hồi. Kiểm tra dữ liệu xác nhận đủ 67 đợt và sáu rương Evil đặt sát sáu boss. Save gốc có cùng mã kiểm tra trước và sau phép thử. Cảm giác né đòn và tỷ lệ thắng của người chơi qua một lượt hoàn chỉnh vẫn cần buổi chơi thử trực tiếp.

