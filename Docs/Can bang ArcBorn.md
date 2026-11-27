# Cân bằng chiến đấu và tiến trình ArcBorn

Cập nhật ngày 27/11/2026. Phạm vi gồm Ranger, Mage, trang bị, nâng cấp vĩnh viễn và hai ải đang có trong Journey. Secrets Pyramid có 28 phòng và 34 đợt, trong đó 29 đợt chiến đấu. Ancient Temple có 33 đợt trong một phòng, trong đó 27 đợt chiến đấu. Hai ải dùng chung 22 loại quái, tính cả sáu boss và hai cấp Slime con.

## Các điểm mất cân bằng và cách xử lý

| Trước khi chỉnh | Tác động | Thay đổi |
| --- | --- | --- |
| Ranger và Mage nhân sát thương nhận vào với ×2 ở cấp 1 và ×3 từ cấp 2 | 100 HP ở cấp 1 chịu được 50 sát thương gốc. Lên cấp 2 có 110 HP nhưng chỉ chịu được 36,7 sát thương gốc. Cùng một chỉ số còn chi phối hệ số chí mạng. | Sát thương nhận vào cơ sở ×1 ở cả năm cấp. Giáp và Shield giảm hệ số này. Chí mạng cơ sở ×1,75 và dùng riêng cơ chế modifier hiện có. |
| Tốc độ đánh giảm từ 1,8 xuống 1,6 ở cấp 2 | Với Signature Bow cấp 1, DPS lý thuyết trước chí mạng từ 72 giảm còn 67,2 khi nâng hero. | Cả hai hero tăng HP, ATK và tốc độ đánh theo từng cấp. Ranger có nhiều HP và cơ động hơn. Mage gây nhiều sát thương mỗi đòn và có xác suất chí mạng cao hơn. |
| Quái đầu ải có lượng máu tương đồng dù vai trò khác nhau | Bomb, Candle, Jelly và quái bắn xa mất thời gian xử lý gần bằng nhau. Slime tạo thêm nhiều mục tiêu sau khi tách. | Phân nhóm HP theo vai trò. Bomb 74 HP, Candle 84 HP, Eye 92 HP, Cauldron 116 HP và Slime 125 HP trước hệ số ải. Slime con 44 và 21 HP. |
| Nhiều quái gây phần lớn sát thương khi chạm. Candle có dải hệ số đảo chiều. | Một lần chạm Cactus ở Basic phòng 10 có thể lên tới 123,2 sát thương gốc và 246,4 HP với hệ số cũ của hero cấp 1. Queen Wasp chỉ có 1 sát thương gốc cho đòn đánh. | Giảm hệ số chạm của boss, đưa nguy hiểm về đòn có hiệu ứng báo trước. Cactus Basic phòng 10 tối đa 20,53 sát thương gốc khi chạm. Queen Wasp có 14–17 sát thương cơ sở. Sửa thứ tự hai đầu dải của Candle. |
| XP dồn về sau và hai ải dùng giá reroll khởi điểm 200 vàng trong lượt | Người chơi đánh nhiều đợt với kỹ năng mở màn trước khi có thêm lựa chọn. | Mốc 100 XP sau hai đợt chiến đấu đầu ở cả hai ải. Reroll đầu 120 vàng ở Pyramid và 150 ở Temple. Các lần reroll tiếp tục dùng hệ số tăng giá hiện có. |
| HP Recovery từ hero và đồ xuất hiện trong dữ liệu nhưng chưa được cộng lại trong trận | Một phần giá trị của Revival Plate và Amethyst Ring chưa tác dụng. | Sau mỗi đợt có quái, hồi lượng HP cố định từ hero và đồ. Wave Recovery hồi thêm theo phần trăm HP tối đa. Cả hai đều bị giới hạn bởi HP đang thiếu. Wave chỉ có rương dùng cơ chế hồi của rương. |
| Khi chuyển đợt, thời gian chờ bị trừ theo đồng hồ tuyệt đối và có thể đọc cấu hình của đợt kế tiếp | Thời gian nghỉ được cấu hình có thể mất sau vài giây đầu trận. | Dùng thời gian đã trôi qua và cấu hình của đợt vừa hoàn tất. Ancient Temple có khoảng nghỉ cơ sở 0,75 giây sau xử lý thưởng, có tính vào đó thời gian chờ tiếp theo đã trôi qua. |
| Sturdy Armor và Helmet of Swiftness cộng dồn khả năng giảm thương quá mạnh ở cấp cao. Mysterious Cloak mất ability khi lên quá cấp 3. | Chọn mũ phòng thủ lấn át mũ sát thương. Lên cấp áo choàng có thể làm build yếu đi. | Giảm mức bảo vệ cuối của giáp và mũ. Mũ Swiftness có thêm tốc độ di chuyển. Áo choàng giữ ability từ cấp 3 trở đi. Giáp nặng, giáp hồi phục và giáp tấn công có trade-off riêng. |

Dữ liệu trước chỉnh có 113 bản ghi đợt hợp lệ thuộc 17 nhãn phiên, gồm 107 qua đợt, 5 thất bại và 1 bị ngắt. Đợt mở màn gồm 2 Candle, 2 Bomb và 1 Slime có trung vị 14,20 giây qua 12 bản ghi. Ba bản ghi gần nhất có 110 HP tối đa và nhận lần lượt 76,61, 78,51 và 101,29 điểm sát thương. Cactus phòng 10 có bốn bản ghi, trung vị 28,53 giây và sát thương nhận vào trung vị bằng 58,9% HP tối đa. Một trong bốn ca thất bại. Những log này gồm cả lượt phát triển và thiếu độ khó, trang bị cùng thông tin người chơi, nên được dùng để khoanh vùng vấn đề. CSV đợt mới ghi đúng tiêu đề hai cột số lượng và thành phần quái.

## Hero và vũ khí

Các giá trị dưới đây chưa cộng trang bị, nâng cấp mua bằng vàng và ability trong trận. Sát thương, HP, chí mạng và hồi HP cố định được cộng từ các nguồn phù hợp. Tốc độ đánh, tốc độ di chuyển và hệ số sát thương nhận vào được nhân.

| Hero | Cấp | ATK | HP | Đòn/giây | Di chuyển | Chí mạng | HP hồi/đợt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Ranger | 1 | 30 | 112 | 1,80 | 5,4 | 2% | 1 |
| Ranger | 2 | 33 | 120 | 1,84 | 5,5 | 3% | 1 |
| Ranger | 3 | 36 | 128 | 1,88 | 5,6 | 4% | 2 |
| Ranger | 4 | 39 | 137 | 1,92 | 5,7 | 5% | 2 |
| Ranger | 5 | 42 | 146 | 1,96 | 5,8 | 6% | 3 |
| Mage | 1 | 32 | 106 | 1,68 | 5,1 | 4% | 1 |
| Mage | 2 | 35 | 114 | 1,71 | 5,2 | 5% | 2 |
| Mage | 3 | 39 | 122 | 1,74 | 5,3 | 6% | 2 |
| Mage | 4 | 42 | 130 | 1,77 | 5,4 | 7% | 3 |
| Mage | 5 | 46 | 138 | 1,80 | 5,5 | 8% | 3 |

Lên cấp 2–5 cần lần lượt 100 / 300 / 600 / 900 vàng và 5 / 10 / 15 / 20 mảnh hero. Cấp 5 tiếp tục mở ability sẵn có của mỗi hero.

| Vũ khí | ATK cộng thêm, cấp 1 → 9 | Hệ số tốc độ đánh, cấp 1 → 9 | Vai trò |
| --- | --- | --- | --- |
| Signature Bow | 10 → 50 | ×1,00 → ×1,08 | Tốc độ đánh đều, lựa chọn khởi đầu của Ranger. |
| Cross bow | 16 → 66 | ×0,86 → ×0,92 | Đòn nặng, khoảng giữa hai phát dài hơn. |
| Apprentice Wand | 11 → 51 | ×0,96 → ×1,04 | Cộng 2–6 điểm phần trăm chí mạng. Dùng chỉ số tương ứng với nhịp đánh của Mage. |

Ở cấp 1, Ranger với Bow có 40 ATK, 1,80 đòn/giây và 2% chí mạng, tương đương **73,08 DPS** kỳ vọng. Mage với Wand có 43 ATK, 1,613 đòn/giây và 6% chí mạng, tương đương **72,47 DPS**. Đây là phép tính cho đòn thường khi đứng đánh liên tục, trúng đích và chưa dùng ability. Trong trận, thời gian di chuyển, số đạn trúng, animation và loại mục tiêu quyết định DPS thực tế.

| Trang bị | Giá trị cân bằng |
| --- | --- |
| Sturdy Armor | +20 đến +60 HP, sát thương nhận ×0,96 đến ×0,79. |
| Mysterious Cloak | +10 đến +24 HP, tốc độ đánh ×1,02 đến ×1,16, giữ ability sẵn có từ cấp 3. |
| Wizard Hat | +5 đến +20 HP, cộng 3–8 điểm phần trăm chí mạng theo bậc nguyên mà hệ chiến đấu đang dùng. |
| Helmet of Swiftness | +5 đến +28 HP, sát thương nhận ×0,98 đến ×0,88, tốc độ chạy ×1,01 đến ×1,08. |
| Aquamarine Ring | Tốc độ đánh và di chuyển ×1,02 đến ×1,10 theo dữ liệu hiện có. |
| Amethyst Ring | Cộng 4–20 điểm phần trăm chí mạng và 0–3 HP hồi mỗi đợt chiến đấu. |
| Milanese Plate | +28 HP, sát thương nhận ×0,93. |
| Commander's Heavy Armor | +42 HP, sát thương nhận ×0,90, tốc độ di chuyển ×0,94. |
| Revival Plate | +27 HP, sát thương nhận ×0,95, hồi 4 HP mỗi đợt có quái. |
| Dragon's Breath Plate | +22 HP, +5 ATK, sát thương nhận ×0,95. Hai asset hiện trùng tên và chỉ số, mỗi asset giữ ID riêng tương thích với save. |

## Nâng cấp vĩnh viễn

Hiệu lực tại mỗi cấp là tổng mức đang áp dụng. Ví dụ Power cấp 2 cho ×1,19 ATK. Giá trong bảng là số vàng cần trả cho cấp tương ứng.

| Nâng cấp | Hiệu lực trước | Hiệu lực sau theo cấp | Giá mới theo cấp |
| --- | --- | --- | --- |
| Power Surge | +10/20/30/40/50% ATK | +10/19/27/34/40% | 100/240/560/1.150/2.200 |
| Vitality Boost | +10/20/30/40/50% HP | +12/22/31/39/46% | 100/245/550/1.100/2.150 |
| Rapid Strikes | +5/10/15/20/25% tốc độ đánh | +7/13/18/23/28% | 90/230/550/1.150/2.200 |
| Agility Boost | +5/10/15/20/25% tốc độ chạy | +4/7/10/13/16% | 75/195/480/1.000/1.900 |
| Fortified Armor | Giảm 2/4/6/8/10% sát thương | Giảm 4/7/10/13/16% | 70/180/450/1.000/1.900 |
| Critical Chance | +2/4/6/8/10 điểm phần trăm | +3/6/9/12/15 điểm phần trăm | 70/200/475/1.000/1.900 |
| Dodge Chance | +2/4/6/8/10 điểm phần trăm | +3/5/7/9/11 điểm phần trăm | 60/160/400/850/1.600 |
| Wave Recovery | 1/2/3/4/5% HP tối đa sau đợt | 2/3/4/5/6% HP tối đa sau đợt có quái | 80/200/480/970/1.850 |
| Revival | 1/2 lượt hồi sinh | 1/2 lượt hồi sinh | 1.200/2.400 |

Trước đây tám nâng cấp đầu có chung giá 100/250/600/1.200/2.400. Giá mở màn mới cho người chơi thêm lựa chọn với 60–100 vàng. Power và tốc độ đánh vẫn nhân với nhau. Tổng hiệu quả tại cấp tối đa của riêng hai nhánh từ ×1,875 xuống ×1,792 để giảm áp lực phải dồn cả ngân sách vào DPS. Giáp, mũ và Shield cũng nhân với nhau. Sturdy Armor, Helmet of Swiftness và Shield ở cấp tối đa trước đây đẩy hệ số trang bị và nâng cấp xuống 0,378. Bộ mới cho hệ số 0,584, tương ứng giảm khoảng 41,6% trước ability trong trận và trước các nguồn giảm thương theo tình huống.

## Nhịp hai ải Journey

Hệ số HP hoặc sát thương của quái bằng `(gốc + bước phòng × chỉ số phòng + bước đợt × chỉ số đợt) × hệ số độ khó`. Chỉ số phòng và đợt trong phép tính bắt đầu từ 0. Sát thương kỹ năng và sát thương chạm tiếp tục dùng hệ số riêng của đòn.

| Ải | HP gốc / bước phòng / bước đợt | Sát thương gốc / bước phòng / bước đợt | Intermediate HP / sát thương | Hard HP / sát thương |
| --- | --- | --- | --- | --- |
| Secrets Pyramid | 1 / 0,07 / 0,01 → **0,88 / 0,035 / 0,025** | 1 / 0,06 / 0,01 → **0,90 / 0,024 / 0,020** | ×1,25 / ×1,15 → **×1,18 / ×1,12** | ×1,50 / ×1,30 → **×1,38 / ×1,23** |
| Ancient Temple | 1 / 0,10 / 0,05 → **1,12 / 0 / 0,026** | 1 / 0,10 / 0,05 → **1,07 / 0 / 0,018** | ×1,25 / ×1,15 → **×1,20 / ×1,13** | ×1,50 / ×1,30 → **×1,42 / ×1,27** |

| Ải | XP cấu hình | Vàng thưởng đợt cấu hình | Reroll đầu | Mốc nhịp chính |
| --- | ---: | ---: | ---: | --- |
| Secrets Pyramid | 895 → 1.500 | 1.475 → 1.639 | 200 → 120 | Đợt mở màn 550 → 388 HP của nhóm quái lúc spawn. Theo thứ tự thiết kế, có 340 XP trước Cactus phòng 10, trước đó là 195. Mask phòng 19, Ice Slime phòng 28. |
| Ancient Temple | 820 → 1.480 | 1.616 → 1.896 | 200 → 150 | Hai Slime con ở đợt 1 mở đầu rồi tăng sức ép theo đợt. Totem đợt 11, Queen Wasp đợt 22 và Ancient Golem đợt 33. |

Vàng trong bảng là tổng cấu hình thưởng đợt bằng vàng của ải, trước quy tắc thưởng ngoài đợt và khoản tiêu reroll. XP thu được tiếp tục cấp tiến trình tài khoản qua luồng có sẵn. Ngưỡng lên cấp và điều kiện qua Pyramid Hard để mở Ancient Temple dùng quy tắc cũ. Roster, bố cục, số lần spawn, nhóm xáo trộn và ID save được giữ để các tiến trình hiện có tiếp tục đọc được. Các asset ải khác ngoài Journey chưa được cân riêng. Nếu đăng ký lại, chúng sẽ sử dụng các prefab quái đã chỉnh.

Theo bảng ngưỡng XP hiện hành, thu đủ XP được thiết kế sẽ cho khoảng 11 lần lên cấp trong mỗi ải, trước đây là 7 lần. Con số này tính riêng lượt chọn ability khi mở màn. Trước Cactus, 340 XP đủ ba mốc lên cấp so với một mốc ở bản cũ. Mục tiêu là để người chơi có thể hình thành build trước boss đầu. Ở Temple, 375 XP trước Totem và 895 XP trước Queen Wasp tạo thêm cơ hội điều chỉnh build giữa các chặng của cùng một phòng.

Tăng XP và hồi phục sẽ nâng sức mạnh build sớm hơn. Basic ưu tiên thời gian học cách né, trong khi Intermediate và Hard tăng thời gian phải tìm góc đứng bắn và số lần phải né. Các đợt dày quái ở cuối ải vẫn có lợi cho build xuyên và dọn nhóm. Các boss đơn mục tiêu còn là bài kiểm tra sát thương tập trung. Những nhận định về nhịp chơi cần được đối chiếu qua các lượt chơi hoàn chỉnh.


## Sáu boss theo độ khó

HP dưới đây được tính tại phòng hoặc đợt của từng boss. Cột chạm là sát thương gốc tối đa ở Hard, trước hệ số nhận sát thương của người chơi. Đòn kỹ năng, đạn, vùng tồn tại và số lần trúng vẫn có công thức riêng.

| Boss | Vị trí | HP Basic | HP Intermediate | HP Hard | Chạm tối đa Hard |
| --- | --- | ---: | ---: | ---: | ---: |
| Mighty Cactus | Secrets Pyramid 10.01 | 3.260 → 1.792,5 | 4.075 → 2.115,1 | 4.890 → 2.473,7 | 160,16 → 25,26 |
| Mysterious Mask | Secrets Pyramid 19.01 | 4.972 → 2.597,2 | 6.215 → 3.064,7 | 7.458 → 3.584,1 | 54,08 → 31,19 |
| Ice Slime | Secrets Pyramid 28.01 | 5.780 → 3.577 | 7.225 → 4.220,9 | 8.670 → 4.936,3 | 204,36 → 41,97 |
| Cursed Totem | Ancient Temple 1.11 | 3.300 → 2.311,5 | 4.125 → 2.773,8 | 4.950 → 3.282,3 | 156 → 32 |
| Queen Wasp | Ancient Temple 1.22 | 4.305 → 3.032,1 | 5.381,2 → 3.638,5 | 6.457,5 → 4.305,6 | 21,32 → 32,83 |
| Ancient Golem | Ancient Temple 1.33 | 4.940 → 4.001,6 | 6.175 → 4.801,9 | 7.410 → 5.682,3 | 16,22 → 50,92 |

Ở Pyramid, Cactus kiểm tra né chuỗi đạn, Mask tăng áp lực kỹ năng và Ice Slime kết thúc ải với lượng HP cao nhất. Ở Temple, các mốc 11/22/33 chia nhịp ra ba chặng. Mục tiêu ưu tiên thay đổi từ quái lao tới sang địch đánh xa và tạo vùng nguy hiểm.

Một mốc kiểm tra cho Golem Hard là tối đa khoảng 50,92 sát thương gốc khi chạm. Với bộ Mage cấp 5 của ca thử cuối, hệ số nhận sát thương khoảng 0,783 và HP tối đa 240,47. Chạm với giá trị cực đại tương đương khoảng 39,9 HP, hay 16,6% thanh máu. Con số này minh họa một lần trúng trước khi tính né đòn, ability và các nguồn hồi trong trận.

## Chỉ số của 22 loại quái

Đây là giá trị gốc trong prefab. Hệ số ải, độ khó và hệ số kỹ năng được áp thêm lúc spawn hoặc đánh.

| Quái | HP | Sát thương gốc | Hệ số sát thương chạm |
| --- | ---: | ---: | ---: |
| Cauldron | 100 → 116 | 25–35 → 16–20 | 0,2–0,4 → 0,3–0,5 |
| Jelly | 100 → 83 | 8 | 1–2 → 0,7–0,95 |
| Plant | 100 → 110 | 10 → 12 | 2–3 → 0,75–1 |
| Wasp | 100 → 85 | 10 → 11 | 1–1,2 → 0,95–1,12 |
| Eye | 100 → 92 | 15 → 13 | 0,3–0,5 → 0,35–0,55 |
| Slime | 150 → 125 | 10 → 11 | 2–3 → 0,95–1,15 |
| Smaller Slime | 50 → 44 | 6 → 7 | 2–3 → 0,9–1,15 |
| Smallest Slime | 25 → 21 | 2 → 4 | 2–3 → 0,9–1,1 |
| Bomb | 100 → 74 | 10 | 0,5–0,6 → 0,42–0,65 |
| Pumpkin | 100 → 84 | 3 → 8 | 5–6 → 0,65–0,85 |
| Bat | 100 → 90 | 10 → 9 | 1,5–2 → 0,72–0,96 |
| Hand | 100 → 108 | 10 → 13 | 0,5–1 → 0,65–0,9 |
| Mole | 100 → 92 | 1 → 10 | 5–8 → 0,94–1,12 |
| Spike | 100 → 104 | 10 → 11 | 1–1,2 → 1,08–1,28 |
| Trap | 100 → 70 | 5 → 11 | 1,5–2 → 0,85–1,05 |
| Candle | 100 → 84 | 10 → 9 | 2–1 → 0,65–0,87 |
| Queen Wasp | 2.100 → 1.820 | 1 → 14–17 | 5–8 → 0,82–1,05 |
| Mighty Cactus | 2.000 → 1.500 | 10 → 14–16 | 5–8 → 0,9–1,15 |
| Cursed Totem | 2.200 → 1.675 | 10 → 16–18 | 5–8 → 0,9–1,12 |
| Ice Slime | 2.000 → 1.960 | 10 → 17–19 | 5–6 → 0,94–1,16 |
| Mysterious Mask | 2.200 → 1.720 | 10 → 15–17 | 1–2 → 0,86–1,12 |
| Ancient Golem | 1.900 → 2.050 | 8 → 18–21 | 0,5–0,6 → 0,96–1,16 |

Bom tăng hệ số nổ từ ×0,90–1,10 lên ×1,12–1,26. Kiểu đánh của Cauldron, Eye và boss vẫn dùng các hệ số đòn riêng đã có. Với điểm spawn có ghi đè sát thương, hệ số chạm được tính từ giá trị ghi đè đã áp dụng.

## Số liệu cho từng đợt

Vị trí ghi theo phòng.đợt trong asset. Game có thể xáo các nội dung cùng nhóm trong một lượt. Tổng HP tính theo Basic, trước khi quái tách hoặc gọi thêm quái. Dấu — dùng cho đợt không có quái. Vàng là thưởng hoàn thành đợt được cấu hình trong ải.

### Secrets Pyramid

| Phòng.đợt | Thành phần | Tổng HP Basic | XP | Vàng |
| --- | --- | ---: | ---: | ---: |
| 1.01 | Candle ×2, Bomb ×2, Slime | 550 → 388 | 30 → 55 | 10 → 18 |
| 2.01 | Candle ×2, Spike, Slime | 482 → 363 | 20 → 45 | 15 → 20 |
| 3.01 | Jelly, Spike ×2, Slime | 513 → 395 | 20 → 45 | 12 → 20 |
| 4.01 | Jelly ×2, Bomb ×2, Slime | 666 → 432 | 20 → 45 | 16 → 22 |
| 5.01 | Rương | — | 0 | 50 |
| 6.01 | Eye ×2, Pumpkin ×2, Slime | 742 → 503 | 35 → 50 | 20 → 24 |
| 7.01 | Eye, Pumpkin ×3, Slime | 781 → 511 | 35 → 50 | 22 → 24 |
| 8.01 | Eye, Jelly ×4, Slime | 968 → 618 | 35 → 50 | 0 → 26 |
| 9.01 | Rương | — | 0 | 0 |
| 10.01 | Mighty Cactus | 3.260 → 1.792 | 60 → 90 | 100 → 120 |
| 11.01 | Bat ×3, Bomb ×2, Spike, Slime | 1.275 → 796 | 35 → 55 | 25 → 30 |
| 12.01 | Bat ×2, Pumpkin ×2, Bomb, Slime | 1.150 → 692 | 35 → 55 | 20 → 30 |
| 13.01 | Bat, Cauldron ×2, Spike, Slime | 1.012 → 716 | 35 → 55 | 40 → 30 |
| 14.01 | Jelly ×3, Eye ×2, Candle, Slime | 1.432 → 857 | 0 → 55 | 50 |
| 15.01 | Cauldron ×2, Jelly ×2, Pumpkin, Slime | 1.287 → 832 | 40 → 60 | 40 → 35 |
| 16.01 | Cauldron ×2, Bomb ×2, Candle, Slime | 1.332 → 828 | 40 → 60 | 40 → 35 |
| 17.01 | Eye ×6, Slime ×2 | 1.908 → 1.155 | 40 → 60 | 30 → 35 |
| 18.01 | Rương | — | 0 | 0 |
| 19.01 | Mysterious Mask | 4.972 → 2.597 | 100 | 250 → 220 |
| 20.01 | Bat ×4, Slime | 1.282 → 749 | 20 → 40 | 60 → 34 |
| 20.02 | Jelly ×2, Pumpkin, Eye ×2, Smaller Slime | 1.287 → 750 | 25 → 45 | 25 → 38 |
| 21.01 | Cauldron ×3, Eye ×2, Slime | 1.560 → 1.038 | 25 → 40 | 60 → 34 |
| 21.02 | Jelly ×4, Smaller Slime ×2 | 1.205 → 674 | 25 → 45 | 25 → 38 |
| 22.01 | Jelly ×4, Eye, Slime ×2 | 1.976 → 1.089 | 25 → 40 | 60 → 34 |
| 22.02 | Pumpkin ×2, Bat ×2, Smaller Slime, Cauldron | 1.364 → 833 | 25 → 45 | 25 → 38 |
| 23.01 | Rương | — | 0 | 50 |
| 24.01 | Eye ×2, Bomb ×2, Pumpkin ×2, Slime, Jelly ×2 | 2.480 → 1.333 | 25 → 50 | 30 → 42 |
| 24.02 | Bomb ×2, Jelly ×2, Candle, Spike, Bat ×2, Smaller Slime | 2.227 → 1.241 | 25 → 55 | 30 → 46 |
| 25.01 | Candle ×2, Bomb, Eye ×2, Cauldron ×2, Slime | 2.278 → 1.347 | 30 → 50 | 30 → 42 |
| 25.02 | Bomb ×4, Spike ×2, Eye ×3 | 2.421 → 1.361 | 30 → 55 | 30 → 46 |
| 26.01 | Candle ×2, Bomb, Spike, Cauldron, Jelly, Slime | 2.062 → 1.176 | 30 → 50 | 30 → 42 |
| 26.02 | Pumpkin ×2, Cauldron ×2, Smaller Slime, Bat ×2 | 1.794 → 1.111 | 30 → 55 | 30 → 46 |
| 27.01 | Rương | — | 0 | 0 |
| 28.01 | Ice Slime | 5.780 → 3.577 | 0 | 250 → 320 |

### Ancient Temple

| Phòng.đợt | Thành phần | Tổng HP Basic | XP | Vàng |
| --- | --- | ---: | ---: | ---: |
| 1.01 | Smaller Slime ×2 | 100 → 99 | 15 → 40 | 12 → 18 |
| 1.02 | Wasp ×3 | 315 → 292 | 15 → 60 | 14 → 24 |
| 1.03 | Wasp ×3, Smaller Slime ×2 | 440 → 402 | 20 → 40 | 20 → 26 |
| 1.04 | Slime ×2 | 345 → 300 | 20 → 45 | 25 |
| 1.05 | Rương | — | 0 | 0 |
| 1.06 | Bomb ×2, Spike | 375 → 315 | 20 → 45 | 25 → 30 |
| 1.07 | Spike ×2, Smallest Slime ×4 | 390 → 373 | 25 → 45 | 25 → 30 |
| 1.08 | Spike, Jelly ×2 | 405 → 352 | 25 → 50 | 30 → 32 |
| 1.09 | Spike, Wasp ×2, Smaller Slime ×2 | 560 → 481 | 25 → 50 | 35 → 36 |
| 1.10 | Rương | — | 0 | 0 |
| 1.11 | Cursed Totem | 3.300 → 2.312 | 80 → 95 | 100 → 150 |
| 1.12 | Wasp ×4 | 620 → 478 | 25 → 50 | 40 → 42 |
| 1.13 | Spike ×2, Plant | 480 → 455 | 25 → 50 | 45 → 44 |
| 1.14 | Plant ×2, Bomb ×2 | 660 → 537 | 25 → 50 | 45 → 46 |
| 1.15 | Plant ×2, Bat ×2 | 680 → 594 | 30 → 55 | 50 |
| 1.16 | Rương | — | 0 | 0 |
| 1.17 | Slime ×2, Cauldron ×2 | 900 → 740 | 30 → 55 | 50 → 54 |
| 1.18 | Wasp ×2, Bomb ×2, Trap | 925 → 606 | 30 → 55 | 55 → 56 |
| 1.19 | Bomb ×2, Plant, Trap ×2 | 950 → 632 | 30 → 55 | 55 → 58 |
| 1.20 | Trap ×3, Pumpkin ×2 | 975 → 610 | 30 → 55 | 60 |
| 1.21 | Rương | — | 0 | 0 |
| 1.22 | Queen Wasp | 4.305 → 3.032 | 100 → 110 | 150 → 210 |
| 1.23 | Mole ×2 | 420 → 311 | 30 → 50 | 60 |
| 1.24 | Mole, Eye ×2, Jelly | 860 → 617 | 30 → 55 | 55 → 61 |
| 1.25 | Mole ×2, Wasp ×2 | 880 → 617 | 30 → 55 | 60 → 63 |
| 1.26 | Mole, Trap ×2, Bomb ×2 | 1.125 → 673 | 30 → 55 | 65 |
| 1.27 | Rương | — | 0 | 0 |
| 1.28 | Bat ×3, Spike ×3 | 1.410 → 1.060 | 30 → 65 | 65 → 68 |
| 1.29 | Slime ×3, Bomb ×3, Pumpkin ×2 | 2.280 → 1.414 | 30 → 65 | 80 → 74 |
| 1.30 | Hand ×2, Jelly ×2, Plant | 1.225 → 922 | 35 → 65 | 65 → 76 |
| 1.31 | Candle, Cauldron ×2, Eye ×2 | 1.250 → 950 | 35 → 65 | 80 → 78 |
| 1.32 | Rương | — | 0 | 0 |
| 1.33 | Ancient Golem | 4.940 → 4.002 | 0 | 250 → 360 |

## Kiểm chứng trong Unity

Đạt 8/8 ca trong Play Mode của Unity 6000.6.2f1, dùng dữ liệu lưu riêng cho từng ca. Đối chiếu HP tối đa, ATK, tốc độ đánh, hệ số nhận sát thương và hệ số chí mạng theo chỉ số thực tế trước khi chọn ability mở màn. Với quái, kiểm tra HP và dải sát thương sau khi áp dụng độ khó tại vị trí spawn.

| Ca thử | Hero và vũ khí | HP tối đa | ATK | Đòn/giây | Nhận sát thương |
| --- | --- | ---: | ---: | ---: | ---: |
| Secrets Pyramid Basic, đầu ải | Ranger 1, Signature Bow 1 | 112 | 40 | 1,800 | ×1,000 |
| Secrets Pyramid Intermediate, đầu ải | Mage 2, Apprentice Wand 2 | 127,68 | 56,10 | 1,775 | ×0,960 |
| Secrets Pyramid Hard, đầu ải | Ranger 3, Cross bow 3 | 156,16 | 76,16 | 1,848 | ×0,930 |
| Ancient Temple Basic, đầu ải | Mage 3, Apprentice Wand 4 | 179,34 | 77,35 | 1,947 | ×0,874 |
| Ancient Temple Intermediate, đầu ải | Ranger 4, Signature Bow 5 | 218,77 | 87,63 | 2,266 | ×0,828 |
| Ancient Temple Hard, đầu ải | Mage 5, Apprentice Wand 6 | 240,47 | 109,88 | 2,236 | ×0,783 |
| Secrets Pyramid Hard, tiếp tục ở Cactus | Ranger 3, Cross bow 3 | 156,16 | 76,16 | 1,848 | ×0,930 |
| Ancient Temple Hard, tiếp tục ở Golem | Mage 5, Apprentice Wand 6 | 240,47 | 109,88 | 2,236 | ×0,783 |

Hai ca Basic xác nhận đạn tạo từ vũ khí của Ranger và Mage đã gây sát thương lên quái. Đưa 10 sát thương gốc qua hàm nhận đòn thật làm Ranger mất đúng 10 HP và Mage mất 8,74 HP với giáp và nâng cấp thử. Sau khi kết thúc đợt bằng luồng nhận sát thương và chết của quái, Ranger hồi 1 HP. Mage hồi 7,38 HP gồm 2 HP cố định và 3% HP tối đa. Đòn đánh vào người chơi và các đòn kết liễu trong phép đo này được tạo có kiểm soát để đo chính xác.

Kiểm tra dữ liệu đọc từ 22 prefab và 67 đợt xác nhận các dải chỉ số hợp lệ, độ khó tăng đúng thứ tự, thành phần spawn và nhóm xáo trộn trùng bản gốc, và hai trường thưởng vàng của từng đợt khớp nhau. Năm cấp hero tăng đều các chỉ số chiến đấu chính. Chín nhánh nâng cấp giữ số cấp và dùng giá tăng dần.

Phạm vi đã đo tập trung vào đầu ải, hai checkpoint boss và cơ chế sát thương, hồi phục. Chưa có dữ liệu chơi trọn sáu lượt từ đầu đến cuối hoặc thử nghiệm người chơi để kết luận về thời gian hoàn thành, tỷ lệ thắng và độ cuốn hút. Chưa xuất bản Android.

