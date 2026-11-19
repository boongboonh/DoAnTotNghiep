# Yêu cầu thiết kế prototype — Dang Dang

Ngày cập nhật 18/11/2026. Phiên bản 0.2.

Lộ trình hiện hành: [PROTOTYPE_IMPLEMENTATION_PLAN.md](PROTOTYPE_IMPLEMENTATION_PLAN.md). Backlog này thay thế phạm vi skill, loại quái, chính sách dash và các mốc M0–M4 cũ bên dưới khi có khác biệt. Các phần chưa cập nhật bên dưới được giữ làm bối cảnh bản 0.1, không dùng để giao việc trái backlog.

Phạm vi hiện hành: combo ba đòn; nhảy đôi, air dash một lượt và nhảy tường; checkpoint; ba loại quái melee/bắn/bay; Bash trên điểm neo và đạn thay skill sát thương vùng dùng mana; mở khóa đường, cửa tắt và save prototype riêng; tuyến chơi 5–10 phút. Dash vẫn nhận sát thương. Chi tiết luật, binding, phụ thuộc, trạng thái và tiêu chí nghiệm thu nằm trong backlog M0–M7.

Tài liệu xác định yêu cầu cho giai đoạn bắt đầu lại game ở dạng prototype. Các mục có ghi “đề xuất” là lựa chọn triển khai ban đầu để chơi thử và điều chỉnh.

## 1. Mục tiêu

Tạo một bản chơi được của game hành động đi cảnh 2D, trong đó người chơi điều khiển Dang Dang vượt địa hình, đối đầu quái, sử dụng một kỹ năng và hồi sinh tại điểm lưu.

Prototype cần trả lời ba câu hỏi.

1. Chạy, nhảy và dash có chính xác, dễ điều khiển và dễ dự đoán hay chưa?
2. Người chơi có đọc được tầm đánh, thời điểm nguy hiểm và kết quả trúng đòn hay chưa?
3. Một đoạn khám phá ngắn có tạo được vòng chơi thử thách → học cách xử lý → vượt qua → nhận tiến trình hay chưa?

Mỗi lần bàn giao phải có scene khởi chạy, cách điều khiển và danh sách tình huống đã kiểm tra.

## 2. Quyết định đã chốt

| Nội dung | Yêu cầu |
| --- | --- |
| Tên nhân vật | Dang Dang |
| Trình bày | Hình học cơ bản cho Player, quái và vật thể tương tác |
| Hình đại diện nhân vật | Một hình đơn bám collider thân, nhìn rõ kích thước va chạm |
| Ưu tiên | Hoàn thiện logic và cảm giác chơi trước khi nâng cấp mỹ thuật |
| Quả cầu năng lượng | Đã bỏ khỏi hướng gameplay hiện tại |
| Kỹ năng bắn năm viên cũ | Đã bỏ, các skill mới được thiết kế riêng |
| Vũ khí | Có dữ liệu riêng để mở rộng, đổi vũ khí về sau |

Các concept ngoại hình, rig Crystal và yêu cầu animation trước đây là tài liệu tham khảo cho giai đoạn hình ảnh. Khi chúng mâu thuẫn với phạm vi prototype về phần trình bày, áp dụng tài liệu này.

## 3. Phạm vi bản chơi đầu tiên

Quy mô dưới đây là **đề xuất cho lần nghiệm thu đầu**.

| Thành phần | Số lượng đề xuất | Vai trò |
| --- | --- | --- |
| Player | 1 | Chạy, nhảy, dash, đánh, dùng skill, chịu sát thương |
| Vũ khí | 1 | Một đòn cận chiến rõ nhịp, dùng cấu trúc dữ liệu vũ khí |
| Quái cận chiến | 1 loại | Tuần tra, phát hiện, báo trước và đánh |
| Quái lao tới | 1 loại | Báo trước, lao, hồi chiêu và có cơ hội phản công |
| Skill | 1 | Dùng hiệu ứng sát thương vùng sẵn có để kiểm tra niệm, mana, hồi chiêu |
| Checkpoint | 1 | Ghi vị trí hồi sinh |
| Chướng ngại nguy hiểm | 1 loại | Vùng rơi hoặc vùng sát thương có dấu hiệu rõ |
| Đích hoàn thành | 1 | Kết thúc lượt thử và cho phép chơi lại |
| Màn chơi | 1 tuyến ngắn | Gồm khu học thao tác, khu chiến đấu và khu phối hợp |

Hệ đổi vũ khí, combo dài, cây nâng cấp skill, kinh tế nhựa cây, boss, nhiều vùng, thoại, cốt truyện và hiệu ứng vùng được đưa vào các lượt prototype sau, khi vòng chơi đầu đạt nghiệm thu.

## 4. Vòng chơi và bố cục màn

Luồng đề xuất cho một lượt thử.

Điểm xuất phát → khu chạy và nhảy → khu học dash → quái đơn → checkpoint → khu phối hợp quái và địa hình → đích.

| Khu | Thiết kế | Điều cần quan sát |
| --- | --- | --- |
| Xuất phát | Sàn phẳng, nhãn điều khiển ngắn | Người chơi đọc hướng và bắt đầu di chuyển ngay |
| Di chuyển | Bậc thấp, khe nhảy, mép rơi an toàn | Nhả nút nhảy, nhảy ở mép, tiếp đất |
| Dash | Đoạn có khoảng trống ngang | Khoảng dash và hồi chiêu có dễ hiểu |
| Quái đơn | Sàn rộng, mỗi lần gặp một quái | Người chơi đọc báo trước và chọn phản ứng |
| Checkpoint | Vùng an toàn, có trạng thái đã kích hoạt | Sau khi chết có quay về đúng điểm |
| Phối hợp | Hai loại quái bố trí có khoảng thở, thêm một bẫy | Camera, hitbox và khóa hành động có hoạt động cùng nhau |
| Đích | Vùng kích hoạt có nhãn hoàn thành | Luồng kết thúc và chơi lại nhất quán |

Chiều cao bậc và độ rộng khe phải được dựng từ quãng nhảy, quãng dash đã đo trên Player. Tránh yêu cầu thao tác sát giới hạn ngay ở khu hướng dẫn. Checkpoint đặt trước khu phối hợp để rút ngắn thời gian thử lại.

## 5. Quy ước hình đại diện

| Đối tượng | Hình và màu đề xuất | Thông tin cần đọc |
| --- | --- | --- |
| Player | Chữ nhật xanh lam | Collider thân và hướng đang nhìn |
| Quái cận chiến | Chữ nhật đỏ | Collider thân, vùng phát hiện và tầm đánh khi bật debug |
| Quái lao tới | Chữ nhật cam | Hướng lao và thời gian báo trước |
| Địa hình | Khối xám đậm | Mặt có thể đứng và mép va chạm |
| Vùng nguy hiểm | Khối đỏ sẫm có nhãn | Vùng gây sát thương hoặc giết ngay |
| Checkpoint | Khối xanh lá có nhãn trạng thái | Đã kích hoạt hay chưa |
| Đích | Khối vàng có nhãn | Điều kiện hoàn thành |

Nền xám trung tính. Mỗi loại nguy hiểm có hình hoặc nhãn bổ sung để có thể phân biệt cả khi màu khó đọc.

Hình thân thể hiện collider chính. Hitbox tấn công và vùng phát hiện là dữ liệu riêng, có chế độ debug để xem. Với collider tròn hoặc đa giác, hình debug phải vẽ đúng biên collider khi cần đo chính xác.

Đòn đánh hiển thị một vùng hình học trong pha gây sát thương. Báo trước có đổi màu hoặc nhãn trạng thái. Trúng đòn có phản hồi ngắn, đủ để nhận ra mục tiêu đã mất máu.

## 6. Yêu cầu chức năng

### 6.1. Player

| Mã | Yêu cầu | Tiêu chí nghiệm thu |
| --- | --- | --- |
| P01 | Chạy trái và phải, đổi hướng, dừng | Tốc độ đạt giá trị cấu hình, thả nút dừng theo cấu hình và hình bám collider |
| P02 | Nhảy có độ cao theo thời gian giữ nút | Nhả sớm tạo cú nhảy thấp hơn giữ nút, mỗi lần cất chân tiêu thụ một lệnh nhảy |
| P03 | Hỗ trợ coyote time và jump buffer | Lệnh trong cửa sổ cấu hình được nhận, lệnh ngoài cửa sổ bị bỏ |
| P04 | Dash có thời gian và hồi chiêu | Quãng dash lặp lại nhất quán, va tường dừng đúng, gravity phục hồi khi kết thúc hoặc bị ngắt |
| P05 | Chuyển trạng thái rõ | Idle, Run, Jump, Fall, Dash, Attack, Hurt, Cast và Dead có thể quan sát qua debug |
| P06 | Chịu sát thương và đẩy lùi | Mất đúng lượng máu, đẩy ra khỏi nguồn sát thương và tạm khóa hành động theo cấu hình |
| P07 | Bất tử ngắn sau trúng đòn | Va chạm lặp trong cửa sổ bất tử tạo một lần mất máu, hết thời gian nhận sát thương lại |
| P08 | Chết và hồi sinh | Chết khóa input và hủy hành động đang chạy, hồi sinh trả về trạng thái điều khiển hợp lệ |
| P09 | Tách logic và hình ảnh | Thay hình đại diện vẫn giữ được cùng luật di chuyển, hitbox và sát thương |

Chính sách ban đầu đề xuất cho dash là dùng được trên đất và trên không theo hồi chiêu, vẫn nhận sát thương. Mọi thay đổi như dash bất tử hoặc giới hạn một lần trên không cần được ghi rõ sau khi chơi thử.

Ưu tiên ngắt hành động đề xuất theo thứ tự Dead → Hurt → Dash → Cast → Attack → di chuyển. Sau khi ngắt phải phục hồi gravity, khả năng nhận input và hitbox. Lệnh bị từ chối phải hết hiệu lực theo thời gian buffer.

### 6.2. Chiến đấu và vũ khí

| Mã | Yêu cầu | Tiêu chí nghiệm thu |
| --- | --- | --- |
| C01 | Đòn đánh có chuẩn bị, active và hồi đòn | Hitbox chỉ gây sát thương trong pha active, có dấu hiệu nhận biết từng pha |
| C02 | Mỗi đòn chỉ trúng một lần trên cùng mục tiêu | Quái có nhiều collider vẫn nhận một lần sát thương trong một đòn |
| C03 | Đòn quay theo hướng nhìn | Đánh trái/phải có cùng tầm và không gây sát thương sau lưng ngoài vùng đã định |
| C04 | Sát thương dùng đường gọi thống nhất | Vũ khí và skill áp dụng cùng luật nhận sát thương của mục tiêu |
| C05 | Dữ liệu vũ khí chỉnh được | Tầm, sát thương và thời gian đòn sửa được từ asset cấu hình |
| C06 | Hủy đòn an toàn | Chết, bị thương hoặc dash ngắt đòn thì hitbox cũ ngừng gây sát thương |

Quy tắc chặn đòn qua tường phải có lựa chọn rõ khi dựng khu chiến đấu. Đề xuất bản đầu chặn sát thương cận chiến xuyên tường đặc.

### 6.3. Quái

| Mã | Yêu cầu | Tiêu chí nghiệm thu |
| --- | --- | --- |
| E01 | Phát hiện Player | Có khoảng phát hiện và mất dấu cấu hình được, không tấn công mục tiêu đã chết |
| E02 | Báo trước đòn | Đổi màu hoặc hiện trạng thái trước pha gây sát thương |
| E03 | Hồi đòn | Có khoảng người chơi phản công, quái không gây sát thương liên tục ngoài active |
| E04 | Chịu đòn và chết | HP giảm đúng, chết tắt hitbox và hành vi, chỉ phát sự kiện chết một lần |
| E05 | Tái tạo được lượt thử | Chơi lại phục hồi quái đúng vị trí và HP ban đầu |

Quái tuần tra cần cấu hình dừng ở mép hoặc rơi khỏi mép. Quái lao tới cần cấu hình rõ cách dừng khi va tường. Dùng hành vi dừng ở mép và dừng lao khi chạm tường làm mặc định thử đầu.

### 6.4. Skill, checkpoint và vòng đời lượt chơi

| Mã | Yêu cầu | Tiêu chí nghiệm thu |
| --- | --- | --- |
| S01 | Một skill thử có mana và hồi chiêu | Thiếu mana hoặc đang hồi thì từ chối, dùng thành công tiêu thụ tài nguyên đúng một lần |
| S02 | Kỹ năng bị ngắt có chính sách rõ | Đề xuất trừ mana và bắt đầu hồi chiêu khi nhận niệm, bị ngắt vẫn giữ chi phí |
| W01 | Checkpoint kích hoạt được | Hiển thị trạng thái và lưu vị trí hồi sinh an toàn |
| W02 | Hồi sinh nhất quán | HP/mana đầy, cooldown và trạng thái tạm reset, gravity khôi phục, Player đặt đúng checkpoint |
| W03 | Quái và vật nguy hiểm reset khi hồi sinh | Đề xuất phục hồi toàn bộ quái của màn, bẫy về trạng thái đầu và checkpoint vẫn giữ |
| W04 | Hoàn thành màn và chơi lại | Đích chỉ kích hoạt một lần, chơi lại trả toàn bộ lượt về điểm xuất phát |
| W05 | Pause | Dừng gameplay và timer hồi chiêu, tiếp tục không lặp input đã bấm trong menu |

Prototype đầu ưu tiên trạng thái trong một lượt chơi. Save lâu dài và tiến trình mở khóa được nghiệm thu riêng ở giai đoạn sau. Nếu dùng hệ save hiện có, tách dữ liệu thử khỏi hồ sơ gameplay cũ và có thao tác reset rõ ràng.

## 7. Điều khiển và HUD

Bảng phím sau là **đề xuất cho prototype**, cần thống nhất trong input và nhãn hướng dẫn.

| Thao tác | Phím |
| --- | --- |
| Di chuyển | A/D hoặc mũi tên trái/phải |
| Nhảy | Space |
| Dash | Shift hoặc chuột phải |
| Đánh | Chuột trái |
| Skill thử | R |
| Tương tác checkpoint | E |
| Pause | Esc |

Skill dùng R và tương tác dùng E để tránh xung đột F giữa hệ tương tác và slot skill hiện có. Khi triển khai cần ánh xạ tập trung và cập nhật HUD cùng lúc.

HUD tối thiểu hiển thị HP, mana, hồi chiêu skill, trạng thái checkpoint và hoàn thành. Nút chơi lại cần dùng được sau khi chết hoặc hoàn thành.

Chế độ debug có thể bật/tắt để xem trạng thái Player/quái, vận tốc, grounded, collider thân, hitbox đánh và vùng phát hiện. Các phím chọn pose trong scene rig thử cần đứng ngoài luồng input của scene prototype.

## 8. Camera và sân thử

- Camera theo Player, giữ sàn đáp và hướng di chuyển có đủ khoảng nhìn.
- Rung hoặc phản hồi trúng đòn giữ ở mức đọc được va chạm.
- Khi hồi sinh, camera về đúng Player và reset độ trễ để tránh kéo từ vị trí chết.
- Chuyển pause, chơi lại và hoàn thành hoạt động ở cả khi Player đang trên không.
- Đề xuất kiểm tra tối thiểu ở 1920×1080 và 1280×720. Các tỷ lệ khác được bổ sung theo nền tảng đích.

## 9. Tổ chức triển khai

Scene sân thử đã có tại `Assets/Scenes/Prototype/PrototypePlayground.unity`. Các thay đổi bootstrap HP/quái gần nhất còn cần kiểm chứng trong Play Mode; xem trạng thái xuất phát trong backlog.

Prefab dự kiến gồm Player, hai quái, checkpoint, vùng nguy hiểm và đích. Thông số di chuyển, vũ khí và skill là asset riêng. Hình đại diện bám collider và có thể thay sau.

Mỗi prefab phải thể hiện rõ component nào quản lý input, vật lý, trạng thái, sát thương và hiển thị. Một Rigidbody2D chỉ có một luồng chủ động ghi chuyển động.

| Nguồn hiện có | Cách đánh giá để tái sử dụng |
| --- | --- |
| `Assets/Scripts/Player/Core/` | Kiểm tra input, motor và controller trên một prefab prototype thống nhất |
| `PlayerMove`, `PlayerDash` và các caller cũ | Kiểm tra phụ thuộc trước khi chuyển luồng điều khiển |
| `Assets/Scripts/Player/Combat/` | Kiểm tra đòn active, ngắt đòn và sát thương trước khi gắn |
| `Assets/Scripts/Player/Skills/` | Chọn một skill thử, kiểm tra tài nguyên và cooldown |
| `HitboxPlaceholder` | Dùng hình thân khớp bounds collider, thêm debug biên thật nếu cần |
| `CrystalRigPreview` | Dùng tham khảo công cụ thử, controller riêng của preview cần được phân biệt với Player gameplay |
| Prefab quái hiện có | Chọn hai loại phù hợp, xác nhận hoạt động trong scene mới |
| `GF.Save.PlayerPrefsSaveSystem` | Đánh giá khi bổ sung save, dùng khóa riêng cho dữ liệu prototype |

Đánh giá code tái sử dụng bằng thử qua entry point thực tế. Ghi rõ phần đã có code, đã gắn scene và đã chơi thử là ba trạng thái riêng.

## 10. Thứ tự thực hiện và cổng nghiệm thu

Bảng dưới là lịch sử mốc phiên bản 0.1. Khi triển khai tiếp, dùng M0–M7 trong [backlog hiện hành](PROTOTYPE_IMPLEMENTATION_PLAN.md), bắt đầu CTL-01 đến CTL-06. Không dùng cùng tên mốc của hai phiên bản thay thế cho nhau.

| Mốc | Nội dung bàn giao | Điều kiện chuyển mốc |
| --- | --- | --- |
| M0 | Scene hình khối, Player, camera, sàn thử, reset | Mở scene và Play chạy được độc lập, hình khớp collider |
| M1 | Chạy, nhảy, dash, vùng rơi, checkpoint | Qua các tình huống di chuyển và hồi sinh ở bảng kiểm |
| M2 | Một vũ khí và hai quái | Trúng đòn, báo trước, hồi đòn và chết đúng luật |
| M3 | Một skill, HUD, pause, đích, tuyến chơi ngắn | Chơi từ đầu đến cuối và chơi lại được |
| M4 | Điều chỉnh thông số từ chơi thử | Có bản nghiệm thu ghi kết quả, vấn đề còn lại và thông số cuối |

Chỉ mở rộng hệ thống sau khi các lỗi khóa điều khiển, gravity, hitbox tồn dư và hồi sinh của mốc hiện tại đã được xử lý.

## 11. Bảng kiểm chơi thử

| Tình huống | Kết quả cần đạt |
| --- | --- |
| Chạy rồi đổi hướng liên tục | Collider và hình không giật tách, input không bị giữ sai |
| Nhảy ngắn và nhảy cao | Hai độ cao khác nhau rõ, đáp sàn đúng |
| Nhảy sớm trước tiếp đất | Buffer nhận trong cửa sổ đã đặt |
| Nhảy sau khi rời mép | Coyote nhận trong cửa sổ đã đặt |
| Dash trên đất, trên không và vào tường | Quãng và thời gian đúng cấu hình, không xuyên tường |
| Trúng đòn trong lúc dash hoặc niệm | Hành động ngắt theo luật, gravity và input khôi phục |
| Đánh vào quái nhiều collider | Một lần sát thương cho mỗi đòn |
| Đánh sát tường | Kết quả đúng quy tắc chặn tường đã chọn |
| Đứng trong vùng tiếp xúc gây sát thương | Cửa sổ bất tử được tôn trọng |
| Skill khi hết mana hoặc đang hồi | Bị từ chối, không tiêu thêm tài nguyên |
| Chết ở mép, giữa không trung và khi đang đánh | Hồi sinh đúng checkpoint, hitbox cũ đã tắt |
| Pause khi đang dash hoặc skill | Resume không cộng thêm hành động hay lệch timer |
| Chơi lại sau hoàn thành | Player, quái và checkpoint về đúng trạng thái lượt đầu |
| Giả lập bước khung 30/60/120 FPS | Quãng nhảy, dash và sát thương giữ trong sai số đã ghi trước khi kiểm tra |
| Đổi độ phân giải | HUD đọc được, camera giữ Player và vùng đáp cần thiết |

Nghiệm thu cần ít nhất một lượt điều khiển trực tiếp toàn tuyến. Kiểm tra bằng gọi hàm và console hỗ trợ tìm lỗi, cần ghi riêng với kết quả chơi trực tiếp.

## 12. Định nghĩa hoàn thành prototype

- Có scene vào chơi trực tiếp và bản build thử trên nền tảng đã chọn.
- Hoàn thành toàn tuyến, chết, hồi sinh, pause và chơi lại được.
- Mọi hạng mục của bản đầu có kết quả kiểm tra và người chơi đọc được hitbox bằng hình đại diện.
- Console sạch lỗi trong lượt chơi nghiệm thu, không có missing script hoặc tham chiếu bắt buộc bị thiếu.
- Thông số hiện hành và lỗi còn tồn tại được ghi ngắn trong `.agent/STATE.md`.
- Quyết định tiếp tục, chỉnh lại hoặc bỏ cơ chế được ghi sau buổi chơi thử.

## 13. Các quyết định còn cần chốt

Các mặc định cũ về dash theo cooldown, quái lao và skill sát thương vùng đã được thay bởi phạm vi hiện hành ở đầu tài liệu. Các mục còn cần kiểm chứng/chỉnh thông số được theo dõi bằng mã công việc trong backlog.

Các đề xuất trong tài liệu đủ để bắt đầu M0. Những mục sau cần được xác nhận trước khi nghiệm thu phần liên quan.

| Quyết định | Mặc định đề xuất | Thời điểm cần chốt |
| --- | --- | --- |
| Nền tảng kiểm thử | Windows, bàn phím và chuột | Trước bản build đầu |
| Quy tắc dash | Trên đất và trên không theo hồi chiêu, nhận sát thương | Trước nghiệm thu M1 |
| Mức reset khi hồi sinh | HP/mana đầy, cooldown reset, quái của màn hồi lại | Trước nghiệm thu M1/M2 |
| Quái thử | Cận chiến và lao tới | Trước M2 |
| Skill thử | Sát thương vùng | Trước M3 |
| Phím thao tác | Bảng đề xuất ở mục 7 | Khi gắn input ở M0/M1 |
| Mục tiêu hiệu năng | Chọn máy đích và mức FPS để đo | Trước M4 |

Các tài liệu `SKILLS.md`, `WORLD_REGIONS.md`, `BESTIARY.md` và `ART_DIRECTION.md` tiếp tục cung cấp hướng phát triển dài hạn. Phạm vi nghiệm thu prototype được xác định bởi các mốc trong tài liệu này.
