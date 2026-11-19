# Backlog triển khai prototype — Dang Dang

Ngày cập nhật: 18/11/2026. Phiên bản 0.1.

Tài liệu thực thi cho [yêu cầu prototype](PROTOTYPE_REQUIREMENTS.md). Hướng đã thống nhất trong trao đổi: hình học cơ bản, điều khiển linh hoạt, combat cận chiến, khám phá bằng năng lực mới. Các luật dưới đây là mặc định thiết kế để triển khai và chơi thử, không phải mô tả chính xác mọi hành vi của Ori.

## 1. Phạm vi và cách theo dõi

Mục tiêu cuối: một tuyến chơi 5–10 phút có chạy/nhảy/dash, nhảy tường, combat, checkpoint, Bash, một đường mở bằng năng lực mới và trạng thái hoàn thành. Thời lượng cần đo bằng chơi thật.

- Chỉ dùng hình khối, màu và nhãn đơn giản. Animation bằng visual child; collider và root vật lý không co giãn theo animation.
- Tái sử dụng input, motor, FSM, combat, damage và feedback hiện có. Một Rigidbody2D chỉ có một luồng chủ động ghi chuyển động.
- Thông số gameplay chỉnh qua Inspector hoặc config; không tạo hệ tổng quát lớn trước khi cần.
- Không tự đổi package, ProjectSettings hoặc dữ liệu gameplay legacy chỉ để phục vụ backlog.
- Mỗi mã công việc được cập nhật riêng; hoàn thành code chưa đồng nghĩa nghiệm thu gameplay.

Trạng thái: **Chưa làm** → **Đang làm** → **Có code / chờ kiểm chứng** → **Đạt nghiệm thu**. Dùng **Cần chỉnh** nếu kiểm tra phát hiện lỗi. Chỉ đánh dấu checklist khi có bằng chứng đáp ứng.

P0: hoàn thiện nền điều khiển. P1: di chuyển và combat. P2: năng lực và khám phá. P3: ghép màn và nghiệm thu. Đây là thứ tự thực hiện, không phải yêu cầu triển khai tất cả trong một lượt.

## 2. Điểm xuất phát

Đối chiếu mã nguồn và lịch sử kiểm tra trong `.agent/STATE.md` ngày 18/11/2026; không thay cho kiểm tra Editor hiện hành.

| Hệ thống | Trạng thái xuất phát | Việc còn thiếu |
| --- | --- | --- |
| Sân thử `Assets/Scenes/Prototype/PrototypePlayground.unity` | Đã có scene hình khối, camera, sàn và bia | Xác nhận lại entry point và luồng reset hiện tại |
| Chạy, nhảy cao/thấp, coyote, buffer, dash | Có code; đã có kiểm tra từng phần trước đây | Chơi liên tục và kiểm tra hồi quy sau các thay đổi mới |
| New Input System | Đã chuyển input của prototype; biên dịch đạt | Thử input thực tế, UniKey, focus và bản build; chưa migrate toàn game legacy |
| Combo ba đòn | Đã có kiểm tra tick/physics trong Play Mode trước đây | Thử cảm giác bằng chuột; kiểm tra cùng hệ chịu đòn mới |
| HP, quái melee, chết/hồi sinh | Có code bootstrap runtime trong `PrototypePlayground.Start` | Chưa xác nhận bootstrap, AI và vòng đời mới trong Play Mode |
| Squash/stretch | Có code và biên dịch đạt | Chưa xác nhận trực quan nhịp nhảy, rơi, đáp đất |
| Checkpoint, nhảy đôi, nhảy tường, Bash | Chưa nghiệm thu trong prototype này | Thực hiện các mốc bên dưới |

MCP Editor mất kết nối ở lần triển khai gần nhất. Kết quả biên dịch bằng Roslyn không được ghi thành kết quả Console hoặc chơi thật.

## 3. M0 — Kiểm chứng control hiện tại (P0)

Điểm vào: `PlayerInputReader`, `PlayerActionBuffer`, `PlayerController`, `PlayerMotor`, `PrototypePlayerMotion`, `PrototypePlayground`.

- [ ] **CTL-01 — Input vật lý:** thử A/D, mũi tên, Space, Shift/RMB, LMB và F5 với UniKey bật/tắt trong Game View và Windows build. Reset không dùng Backspace vì bộ gõ có thể phát phím này khi ghép dấu. Thử chuỗi A rồi S nhiều lần: không reset; F5 vẫn reset. Nếu còn bị chặn phím, tái hiện và xác định nguyên nhân trước khi chọn giải pháp input khác.
- [ ] **CTL-02 — Buffer/coyote:** kiểm tra lệnh trước tiếp đất, sau rời mép và ngoài cửa sổ; một lệnh chỉ được tiêu thụ một lần. Nhả nhảy sớm vẫn cắt độ cao của cú nhảy buffered đúng luật.
- [ ] **CTL-03 — Ngắt hành động:** dash ngắt attack; Hurt và Dead ngắt hành động theo thứ tự ưu tiên. Không giữ hitbox, combo chờ hoặc gravity override sau khi bị ngắt.
- [ ] **CTL-04 — Focus và thời gian:** mất focus xóa lệnh chờ; hitstop vẫn nhận buffer; pause thường không ghi lệnh gameplay. Phần pause kiểm tra lại khi PAU-01 được triển khai.
- [ ] **CTL-05 — Visual:** nhảy/đáp nén X rộng Y thấp; bay lên/rơi kéo X hẹp Y cao; không chậm thời điểm nhảy vật lý, không thay collider. Nhảy buffered ngay khi đáp không chồng sai pulse.
- [ ] **CTL-06 — Reset:** reset lúc đang dash, đánh, Hurt và Dead trả Player/camera/bia/quái về trạng thái hợp lệ; chơi lại nhiều lần không nhân đôi object hoặc callback.

Nghiệm thu: một lượt điều khiển trực tiếp gồm chạy → nhảy thấp/cao → dash → combo → chịu đòn → chết → hồi sinh. Ghi riêng lỗi, thông số và các kiểm tra chưa thực hiện; không chuyển mốc nếu còn lỗi kẹt state hoặc mất quyền điều khiển.

## 4. M1 — Nhảy đôi và dash trên không (P1)

Trạng thái 19/11/2026: **Có code / chờ kiểm chứng bằng điều khiển trực tiếp** cho MOV-01 đến MOV-05. Đã qua 16 kiểm tra motor/cast trên object tạm trong Editor và 15 kiểm tra tích hợp qua gọi hàm trong Play Mode. Chưa đánh dấu đạt nghiệm thu cảm giác chơi. M0 vẫn còn kiểm tra UniKey/focus/build trực tiếp.

Triển khai hiện hành: `PlayerMotor` giữ lượt và chỉ hồi khi chuyển sang grounded hợp lệ hoặc reset; `PlayerController` ưu tiên ground/coyote trước air jump, chặn dash theo lượt và cooldown. `PlayerMovementConfig.airJumpVelocity` mặc định 10.5, `dashExitSpeed` mặc định 6.5; có thể chỉnh Inspector. Dấu/nhãn dưới Player tạo runtime bởi `PrototypePlayground`: `DASH: 1 READY`, `DASH: 1 WAIT` hoặc `DASH: 0`. WAIT gồm cooldown hoặc đang bị khóa hành động. Không thay serialized scene/config trong lượt này.

Phụ thuộc M0. Thay chính sách dash trên không chỉ theo cooldown của bản 0.1 bằng lượt dùng rõ ràng.

- [ ] **MOV-01 — Nhảy đôi:** tối đa một lần bổ sung trên không. Đặt vận tốc dọc về giá trị nhảy đôi cấu hình; không cộng dồn lực theo số lần bấm. Lệnh nhảy mặt đất/coyote được xét trước lượt nhảy đôi và mỗi lần bấm chỉ tạo một cú bật.
- [ ] **MOV-02 — Hồi lượt:** mặt đất hợp lệ hồi lượt nhảy và dash. Chạm cạnh tường, trần hoặc trigger không hồi lượt. Rời mép mà không nhảy vẫn có một lượt nhảy đôi sau khi hết coyote.
- [ ] **MOV-03 — Air dash:** tối đa một lần mỗi lượt trên không, đồng thời tôn trọng cooldown. Dash mặt đất không tạo chuỗi air dash vô hạn khi bay khỏi mép; một lần dash khởi động trên đất rồi rời mép cũng tiêu thụ lượt air dash.
- [ ] **MOV-04 — Kết thúc/ngắt dash:** dừng ở tường, trả gravity đúng một lần; vận tốc thoát có giới hạn cấu hình. Hurt, Dead và reset không để lại trạng thái dash.
- [ ] **MOV-05 — Dấu lượt:** ký hiệu nhỏ cho lượt dash còn/hết; không chỉ dựa vào thay màu thân.

Nghiệm thu: nhảy → nhảy đôi → dash qua khe, hết lượt không bay thêm; đáp đất hồi lượt. Kiểm tra bấm nhảy liên tục, giữ nút, dash khỏi mép, đụng trần và đáp trong cooldown.

## 5. M2 — Trượt tường và nhảy tường (P1)

Trạng thái 19/11/2026: **Có code / chờ chơi trực tiếp** cho WAL-01 đến WAL-05. Thêm `WallSlide` ở cuối enum locomotion, state riêng và pose/ngấn tiếp xúc xanh. `PlayerMotor` dò ngang giữa thân theo groundMask, bỏ self/trigger/platform một chiều; grounded có ưu tiên. Chỉ trượt khi rơi và giữ hướng vào tường. Nhảy xét ground/coyote → wall jump → air jump, không hồi lượt M1 khi chạm hoặc bật tường.

Thông số Inspector trong `PlayerMovementConfig` → Tuong: `wallCheckDistance=.06`, `wallSlideSpeed=2.5`, `wallJumpVelocity=(8,11)`, `wallJumpControlTime=.14`. Khoảng bảo vệ đầu giữ lực bật ra; hết khoảng này trả điều khiển ngang. Dash/Hurt/Dead/Locked/reset xóa trạng thái bảo vệ. Không override gravity cho slide nên thoát tường không giữ gravity tùy chỉnh.

Sân thử tạo hai vách runtime tại x=19 và x=22 trên Far Landing, cao 5.5, kèm nhãn hướng dẫn. Không sửa serialized scene/prefab. Đã qua 17 kiểm tra motor/cast trong Editor và 21 kiểm tra tích hợp gọi hàm trong Play Mode, gồm buffer khi vừa chạm, hai phía, bảo vệ/hết bảo vệ, không hồi lượt, giữ/nhả hướng, ngắt hành động và collider vách. Đã sửa lỗi WallSlide/Fall đổi qua lại do AnyTransition bỏ qua state hiện tại. Chưa nghiệm thu leo liên tục bằng phím, hình ảnh/feel, UniKey hoặc bản build; M1 vẫn chờ chơi trực tiếp.

Phụ thuộc M1. Không thêm leo tự do hoặc bám trần ở mốc này.

- [ ] **WAL-01 — Phát hiện tường:** nhận trái/phải, tách khỏi grounded; lọc self, trigger, góc sàn và mặt dưới platform.
- [ ] **WAL-02 — Trượt:** chỉ trượt khi đang rơi và giữ hướng vào tường; giới hạn tốc độ dọc. Thả hướng hoặc rời tường trả lại rơi thường.
- [ ] **WAL-03 — Bật tường:** nhảy đẩy lên và ra ngoài theo vận tốc cấu hình; ưu tiên lực bật trong thời gian ngắn rồi trả điều khiển ngang. Nhận jump buffer khi vừa chạm tường.
- [ ] **WAL-04 — Quyền hành động:** ưu tiên nhảy đất/coyote → nhảy tường hợp lệ → nhảy đôi; không tiêu hai loại nhảy trong một lệnh. Nhảy tường không tự hồi lượt nhảy đôi hoặc air dash.
- [ ] **WAL-05 — Hiển thị/ngắt:** pose nghiêng và dấu tiếp xúc; dash, Hurt, Dead hoặc reset thoát wall state và phục hồi giới hạn rơi.

Nghiệm thu: leo hành lang đứng bằng hai tường; chủ động rời tường được; không rung góc sàn, tự leo bằng giữ nút hoặc bám khi di chuyển ra xa.

## 6. M3 — Địa hình, nguy hiểm và checkpoint (P1)

Trạng thái 19/11/2026: **Có code / chờ chơi trực tiếp** cho ENV-01 đến CHK-03. `PrototypeTerrain` tạo runtime một platform một chiều x=3/y=2.2, platform di động từ x=10 đến 14/y=4.5, gai x=5 và checkpoint x=9. Không sửa serialized scene/prefab; Unity đã import script mới. Nền và hố của sân hiện có được giữ; rơi dưới y=-4 hồi sinh ngay, với fallback y=-7 ở playground.

Điều khiển: S hoặc xuống + Space khi đứng trên platform một chiều để nhảy xuống; chỉ bỏ collision với platform đang đứng, không tiêu lượt nhảy đôi và không dùng coyote để bật lại. Sau khi ra khỏi platform thì trả collision; chết/reset/disable cũng trả collision. Platform di động dùng kinematic body, motor mang Player theo dịch chuyển vật lý của support, nhảy thì tách support; vật liệu runtime không ma sát tránh đẩy ngang hai lần. Nếu bị ép vào vật cản trong lúc được mang theo hoặc drop bị kẹt quá thời gian an toàn, hồi checkpoint.

E kích hoạt checkpoint khi còn sống, grounded và không Hurt/khóa điều khiển; E được dành cho tương tác trong prototype, không ghi slot skill cũ. Spawn an toàn (9,.6) tách khỏi tâm marker, checkpoint đổi vàng sang xanh. F5/chết/hố/ép kẹt hồi về checkpoint đã kích hoạt hoặc spawn đầu, đầy HP và reset trạng thái tạm, collider ignore, combo/buffer/cooldown, lượt nhảy/dash, camera, quái/bia và platform. Ctrl+F5 hoặc `RestartRun()` xóa checkpoint của lượt rồi chơi lại từ đầu; chưa có tiến trình M6 hoặc projectile M4 để lưu/xóa/reset. Đạn tương lai phải nối vào reset phòng khi triển khai M4.

Đã qua 38 kiểm tra gọi hàm/input giả lập/vật lý trong Play Mode, gồm đi xuyên platform từ dưới và đáp trên, S+Space, collision được trả, checkpoint/pit/dead/reset, gai/i-frame, mang Player/tách support, ép trần, E không gọi skill, camera và không nhân đôi object. Kiểm tra platform khứ hồi 330 bước .02s có độ lệch ngang tối đa .0301 đơn vị. Console cuối 0 error. Chưa nghiệm thu chơi liên tục bằng phím, UniKey, cảm giác/hiển thị hoặc Windows build; không đánh dấu checklist hoàn tất nghiệm thu.

Phụ thuộc M2. Đưa checkpoint vào trước khi tăng độ khó combat.

- [ ] **ENV-01 — Platform một chiều:** xuyên từ dưới, đứng trên ổn định; không biến thành tường cho wall jump.
- [ ] **ENV-02 — Nhảy xuống:** xuống + nhảy chỉ tạm bỏ va chạm platform đang đứng; lệnh này không đồng thời kích hoạt nhảy đôi. Phục hồi va chạm kể cả khi chết/reset.
- [ ] **ENV-03 — Platform di động:** mang Player theo khi đứng; nhảy rời không bị kéo lại, va trần không đẩy xuyên vật cản. Xử lý trường hợp ép kẹt bằng hồi checkpoint cho bản thử.
- [ ] **ENV-04 — Gai và hố:** gai gây damage có i-frame; hố chết đưa về checkpoint. Quy tắc này cần hiển thị khác nhau, không chỉ khác màu.
- [ ] **CHK-01 — Kích hoạt:** một checkpoint trong vùng an toàn, tương tác E; lưu spawn tách khỏi vị trí tâm marker, hiển thị đã kích hoạt. Không cho kích hoạt khi Dead.
- [ ] **CHK-02 — Hồi sinh:** đầy HP, hồi lượt nhảy/dash, xóa buffer/combo, gravity override, lực đẩy và trạng thái tạm. Camera snap về Player. Quái/bẫy/đạn trong phòng reset.
- [ ] **CHK-03 — Phân biệt reset:** chết giữ checkpoint và tiến trình đã nhận; chơi lại toàn tuyến xóa tiến trình lượt thử và về đầu. M0/M1 dùng reset sân hiện có cho đến khi có checkpoint.

Nghiệm thu: chết trên không, trong đòn đánh và trên platform; hồi sinh không chồng bẫy, không còn đạn hoặc hitbox từ lượt trước. Reset phòng không nhân đôi quái.

## 7. M4 — Hoàn thiện combat và quái (P1)

Trạng thái 19/11/2026: **Có code / chờ chơi trực tiếp** cho CMB-01 đến ENC-01. Đã qua 57 kiểm tra gọi hàm/input giả lập/cast/vật lý trong Unity Play Mode; Console cuối 0 error. Chưa nghiệm thu điều khiển liên tục, độ rõ của báo đòn/animation, cân bằng đợt quái hoặc Windows build. Các checklist dưới vẫn chờ nghiệm thu bằng chơi thật.

### Triển khai hiện hành

- `CombatHit.cs`: thêm `CombatFaction`, hợp đồng tùy chọn `ICombatReceiver` và `CombatOcclusion`. Receiver phân biệt nguồn/phe/điểm trúng, trả về đã nhận damage hay chưa; không thay chữ ký `IDamageable` legacy. Player, melee, shooter và flyer dùng receiver mới, từ chối damage cùng phe và damage sau chết.
- `HitScanner`: giữ một hit mỗi mục tiêu mỗi nhát, nhận collider con không cần gán tag riêng nếu tổ tiên có tag mục tiêu; melee kiểm tra tường đặc từ vị trí vật lý Player tới mục tiêu trước khi damage. Feedback chỉ phát khi receiver chấp nhận damage. World solid của sân prototype dùng layer Default; hệ occlusion/projectile/rush hiện dùng quy ước đó.
- `PlayerCombat`: giữ combo ba đòn 1/1/2 hiện có; W hoặc mũi tên lên + LMB khóa hướng lên theo từng nhát, đổi offset/size vùng đánh mà không tạo cây combo khác. Nhả W không xoay nhát đang chạy. `airMoveControl=.65` trong Inspector quyết định mức điều khiển ngang khi đánh trên không; mặt đất giữ moveControl của từng step. Feedback/vùng đánh/gizmo khớp hướng, nhãn thêm UP, pose đánh lên nghiêng rõ hơn; không hất tung/juggle.
- Melee giữ HP4, báo trước .5s, active .12s, recovery .7s; Hurt .25s ngắt Windup/Attack. Chết tắt collider và phát sự kiện đúng một lần. Player giữ knockback/Hurt/i-frame và dash vẫn nhận damage.
- `PrototypeSpecialEnemy` dùng hai mode Shooter/Flyer, HP3. Detect7, windup .65s khóa hướng, recovery1s, Hurt .25s; đứng/di chuyển trong vùng giới hạn quanh home. Shooter kiểm tra đường nhìn và bắn một viên rồi hồi. Flyer lượn gần home, lao theo hướng đã báo với speed8 trong .4s, quét box để không bỏ sót va chạm, thử damage một lần/lượt lao, va tường dừng vào Recovery. Hit Player chỉ trong pha lao, không có damage tiếp xúc thường trực.
- `PrototypeProjectile`: tốc độ7, tuổi thọ4s, damage1, phe Enemy. Circle cast theo toàn quãng di chuyển, chọn va chạm hợp lệ gần nhất; bỏ chủ thể/cùng phe, chạm solid hoặc Player thì hủy đúng một lần, hết tuổi thọ cũng hủy. Chưa có homing hoặc phản đạn; API Bash thuộc M5.

### Phòng thử và reset

`PrototypeCombatRoom` tạo runtime phần sàn nối bên trái sân, từ x=-23 đến -7. Đi sang trái qua quái mẫu cũ, vào x<-9 để bắt đầu; vẫn có bia và quái mẫu độc lập ngoài phòng. Nhãn báo đợt đặt tại (-15,4.6).

| Đợt | Thành phần | Spawn |
| --- | --- | --- |
| 1 | Một melee | (-16,.55) |
| 2 | Một shooter | (-17,.55) |
| 3 | Melee + shooter + flyer | (-18,.55), (-12,.55), (-15,2.4) |

Hết toàn bộ quái của đợt mới chờ .8s rồi chuyển đợt; không tự thắng khi còn flyer. Kết thúc đợt cuối hiện COMBAT TRIAL COMPLETE. Rời khu thử thì tắt/reset quái và xóa đạn, giữ số đợt để thử lại khi quay vào. F5/chết/checkpoint reset xóa toàn bộ đạn đang quản lý và khôi phục đầy HP cả đợt hiện tại, chờ Player vào lại khu thử. Ctrl+F5 đưa về đợt đầu và giữ luật xóa checkpoint của M3; reset sau khi đã hoàn thành cũng trở lại đợt đầu. Player chết không thể chuyển đợt hoặc hoàn thành phòng. Không nhân đôi actor khi reset. Chưa nối tiến trình save M6 hoặc điều kiện đích M7.

Kiểm tra đã đạt gồm collider con/nhiều collider/scan lặp, frame dài qua cửa sổ hit, combo1/1/2, aim lên khóa/nhả và mục tiêu trên đầu, điều khiển ngang trên không, tường mỏng/LOS/đạn, phe và knockback, i-frame với nhiều hit, Hurt ngắt gồng/lao, dash ngắt combo và bị Hurt ngắt, chết một lần, tuổi thọ đạn, thứ tự đợt/reset/không nhân đôi. Không sửa serialized scene/prefab/config hoặc thêm package; các script mới được Unity import cùng meta.

Phụ thuộc M3; sửa lỗi combat nền có thể làm ngay ở M0. Điểm vào: `PlayerCombat`, `HitScanner`, `PrototypeMeleeFeedback`, `PrototypePlayerHealth`, `PrototypeMeleeEnemy`, `PrototypeHealthState`.

- [ ] **CMB-01 — Damage/hurtbox:** một mục tiêu nhiều collider chỉ mất HP một lần mỗi nhát. Phân biệt nguồn, phe, damage và hướng knockback; chặn melee xuyên tường đặc.
- [ ] **CMB-02 — Combo:** giữ ba đòn 1/1/2 damage hiện có làm mốc; một lần bấm chỉ xếp một đòn tiếp. Hết cửa sổ về đòn đầu; dash/Hurt/Dead xóa chuỗi và hitbox chờ.
- [ ] **CMB-03 — Không chiến:** cấu hình điều khiển ngang khi đánh trên không. Thêm hướng đánh lên bằng giữ lên + LMB; dùng timing/damage của combo, không tạo thêm cây combo. Chưa thêm hất tung hoặc juggle.
- [ ] **CMB-04 — Chịu đòn:** knockback → Hurt → trả điều khiển, i-frame có biểu hiện rõ. Dash vẫn nhận damage; không tự thêm bất tử khi dash.
- [ ] **CMB-05 — Ngắt đòn quái:** mặc định đòn Player trúng quái thường sẽ ngắt Windup/Attack sang Hurt; hết Hurt quay về hành vi hợp lệ. Chưa thêm super armor.
- [ ] **CMB-06 — Feedback:** phân biệt gồng/active/hồi, đánh hụt và trúng; finisher có phản hồi mạnh hơn. Hết HP chỉ phát chết một lần, tắt tấn công và hurtbox.
- [ ] **ENM-01 — Melee:** kiểm chứng detect → chase → windup khóa hướng → active → recovery; dừng mép/tường, không gây damage tiếp xúc, không đánh Player chết.
- [ ] **ENM-02 — Quái bắn:** đứng trong vùng giới hạn, kiểm tra đường nhìn, báo hướng bắn rồi bắn một viên thẳng và hồi. Đạn có tốc độ, tuổi thọ, phe, damage; hủy khi va tường/reset. Không homing ở bản đầu.
- [ ] **ENM-03 — Quái bay:** ở vùng hoạt động giới hạn → báo lao và khóa hướng → lao đoạn ngắn → hồi; va tường dừng, không đuổi vô hạn. Dùng để thử đánh hướng lên.
- [ ] **ENC-01 — Phòng thử:** ba lượt gặp ngắn: melee đơn → quái bắn đơn → phối hợp. Chỉ kết thúc đợt khi các quái thuộc đợt đã chết; reset khôi phục cả đợt.

Nghiệm thu: né báo đòn rồi phản công có hiệu quả; không mất HP ngoài active hoặc qua tường. Kiểm tra finisher, nhiều collider, frame dài, đổi hướng, đánh trên không, hai quái cùng tấn công và chết đúng lúc đang gây damage.

## 8. M5 — Bash kết nối di chuyển với combat (P2)

Trạng thái 19/11/2026: **Có code / chờ kiểm chứng** cho BSH-01 đến BSH-05 và BSH-07; BSH-06 mới xử lý phần hitstop, pause chờ PAU-01. Biên dịch Roslyn toàn Assembly-CSharp đạt (chỉ warning LeanTween cũ). Unity import/compile sạch, Console 0 error. Play Mode (gọi hàm, Physics2D Script mode): bootstrap tạo Bash và tìm được mục tiêu; Begin → time scale 0,12 và gravity 0; thả R → Launch, time scale về 1, hồi air dash, bay ~4,5 đơn vị rồi trả gravity; Cancel trả time scale/gravity; đạn Bash một lần, đổi phe Player, bay ngược và phá cổng; đạn quái thường không phá cổng. Agent đã tự chơi bằng input giả lập trên bàn phím/chuột (bot Editor `AgentBashBot`, không cần focus): R + vị trí chuột → khóa neo, ngắm, thả phóng; chuỗi ba neo liên tiếp (31→35→39,5→44) rồi air dash + nhảy đôi qua nửa hố còn lại, đáp ở x≈57 còn sống; phản đạn của quái bắn: đạn đổi phe Player, bay về phía cổng và phá cổng. Sửa theo kết quả chơi: bệ đỡ che tầm nhìn quái nên quái giờ lơ lửng bên trái (52,5, 5,6), cổng cao 5,5 ở x=58 để không nhảy qua. Chỉnh sau khi chơi: sau mỗi lần phóng Player lơ lửng 0,45s (gravity ×0,3, rơi tối đa 4 u/s) để nối chuỗi; bot nối ba neo với độ trễ 0,8s giữa các lần Bash vẫn qua hố (trước chỉnh chỉ chịu ~0,5s). Chưa có người chơi trực tiếp đánh giá cảm giác.

Triển khai: giữ R (phím vật lý) khi có mục tiêu trong tầm 7 và không bị tường chắn → khóa mục tiêu gần nhất, time scale 0,12 qua `HitStop.SetBaseScale` (không đè hitstop), Player đứng yên không gravity, mũi tên theo chuột (dự phòng: hướng nhìn/hướng hợp lệ cuối), tối đa 1,2s unscaled rồi tự phóng. Thả R: Player bay 20 u/s trong 0,22s, dừng ở vật cản, vận tốc thoát tối đa 8; hồi air dash, không hồi nhảy đôi. Đạn bị Bash đổi sang phe Player, bay ngược hướng phóng, tốc độ 10, damage 3, chỉ Bash một lần; điểm neo chỉ dùng lại sau khi Player rời tầm 7. Mất focus/khóa input/pause, Hurt, Dead, Locked, reset hoặc mục tiêu mất hiệu lực đều hủy ngắm và trả time scale/gravity. `PrototypeBashBarrier` chỉ vỡ khi đạn đã Bash chạm. Khu thử tạo runtime bên phải x=23..70: hố rộng 19 với ba điểm neo, quái bắn trên bệ cao và cổng chắn. Code: `PrototypeBash`, `PrototypeBashAnchor`, `PrototypeBashBarrier`, `PrototypeBashCourse`; sửa `PlayerController` (BashState, `PlayerLocomotion.Bash` cuối enum), `PlayerMotor`, `PlayerInputReader`, `PrototypeProjectile`, `HitStop`, `PrototypePlayground`. `unlocked` mặc định bật; M6 sẽ điều khiển. Chưa kiểm tra hố có vượt được bằng chuỗi neo, góc phản đạn tới cổng, cân bằng tầm/tốc độ.

Phụ thuộc M4, đặc biệt vòng đời và phe của đạn. Bash thay skill sát thương vùng/mana trong phạm vi bản thử này; không nối tài nguyên skill legacy chỉ để dùng Bash.

- [ ] **BSH-01 — Mục tiêu:** hỗ trợ điểm neo cố định và một loại đạn ENM-02. Lọc theo tầm/đường nhìn, chọn mục tiêu gần nhất; highlight mục tiêu được chọn. Chưa Bash trực tiếp vào quái.
- [ ] **BSH-02 — Ngắm:** giữ nút Bash để khóa mục tiêu, làm chậm thế giới và hiện mũi tên theo con trỏ. Có giới hạn thời gian ngắm theo unscaled time; hết hạn tự phóng theo hướng hợp lệ cuối cùng. Con trỏ trùng tâm dùng hướng nhìn làm dự phòng.
- [ ] **BSH-03 — Phóng:** thả nút đẩy Player theo hướng đã chọn bằng vận tốc cấu hình. Đạn đi ngược hướng, đổi phe sang Player và gây damage cho quái; không lập tức tự trúng Player.
- [ ] **BSH-04 — Nối chuỗi:** Bash thành công hồi một lượt air dash, không hồi nhảy đôi. Giữ cooldown dash hiện hành; không Bash lặp cùng một điểm neo trước khi rời phạm vi của nó. Mỗi viên đạn chỉ Bash một lần.
- [ ] **BSH-05 — Hủy an toàn:** mục tiêu biến mất hoặc mất hợp lệ thì hủy, không phóng và không hồi lượt. Hurt, Dead, pause, mất focus hoặc reset hủy ngắm, phục hồi thời gian/gravity/input. Không tự tìm mục tiêu mới giữa lần ngắm.
- [ ] **BSH-06 — Quản lý thời gian:** hitstop, Bash slow motion và pause có quyền sở hữu rõ; kết thúc hiệu ứng này không vô tình gỡ hiệu ứng khác. Pause ưu tiên dừng gameplay, mọi nhánh thoát trả quyền đúng.
- [ ] **BSH-07 — Vật cản phản đạn:** một khối chỉ phá được bằng đạn đã Bash, có dấu hiệu nhận biết. Không phá bằng melee thường.

Nghiệm thu: nối hai điểm neo → dash tới sàn; phản đạn giết quái và phá vật cản. Kiểm tra nhiều mục tiêu gần nhau, mục tiêu sau tường, bị thương/đổi focus lúc ngắm và pause trong hitstop.

## 9. M6 — Năng lực, khám phá và lưu tiến trình (P2)

Trạng thái 19/11/2026: **Có code / chờ chơi trực tiếp** cho PRG-01 đến PRG-06. Unity compile sạch, Console 0 error; kiểm tra trong Play Mode bằng gọi hàm và bot, chưa nghiệm thu bằng chơi thật hoặc Windows build.

Triển khai: `PrototypeProgress` giữ save riêng (PlayerPrefs khóa `DangDangPrototype.Save`, JSON version 1, qua `GF.Save.PlayerPrefsSaveSystem`) với định danh `bash_orb`, `hp_up_1`, `shortcut_door`, `cp_main`; không đụng save game cũ. Khu thử Bash (x=23..70) thêm: orb Bash cyan ở x=25,5 trước hố (chưa nhận thì Bash khóa, nhãn "NEEDS BASH"); cửa tắt ở x=63 cao 3,2, chỉ mở bằng E từ phía phải, vẫn nhảy đôi qua được từ bên trái; vật phẩm +1 máu tối đa ở x=67 (tăng max và hồi đầy). Chết/F5 giữ tiến trình; Ctrl+F5 (`RestartRun`) xóa save, đóng cửa, khóa Bash, trả máu tối đa về 5, vật phẩm xuất hiện lại. Checkpoint lưu id; id không còn tồn tại thì bỏ dấu và về điểm đầu. JSON hỏng hoặc sai version thì dùng dữ liệu rỗng. `debugUnlockBash` trên `PrototypeProgress` mở Bash trong bộ nhớ, không ghi save.

Đã kiểm tra Play Mode: nhận orb/HP/cửa/checkpoint, nhận trùng bị từ chối, F5 và thoát/vào lại Play Mode khôi phục đúng (Bash, max HP 6, cửa mở, checkpoint, vị trí hồi sinh), checkpoint không tồn tại, JSON hỏng, version 99, RestartRun xóa sạch. Đo khoảng cách: chạy + nhảy + nhảy đôi + dash không Bash chỉ tới x≈41 trong khi bờ bên kia ở x=50, nên không vượt hố được nếu thiếu Bash; có Bash qua được bằng ba neo.

Phụ thuộc M5. Nhảy đôi/air dash/wall jump mở sẵn ở đầu tuyến; Bash là năng lực nhận giữa màn.

- [ ] **PRG-01 — Cờ năng lực:** chưa có Bash thì input không kích hoạt; nhận một lần, có nhãn hướng dẫn ngắn và khu thử an toàn ngay sau đó. Chế độ debug có thể mở sẵn nhưng không ảnh hưởng save thường.
- [ ] **PRG-02 — Đường bị khóa:** cho thấy một lối cần Bash trước khi nhận; sau mở khóa có thể quay lại vượt. Đo khoảng cách bằng movement thực tế để nhảy/dash thường không vô tình bỏ qua.
- [ ] **PRG-03 — Cửa tắt:** mở từ một phía; giữ trạng thái khi chết hoặc nạp lại save.
- [ ] **PRG-04 — Vật phẩm HP:** một vật phẩm tăng máu tối đa, nhận một lần; không cộng lặp sau hồi sinh/nạp lại. Khi nhận, tăng max HP và hồi đầy.
- [ ] **PRG-05 — Save riêng:** lưu checkpoint ID, năng lực, item đã lấy và cửa đã mở bằng định danh ổn định. Dùng khóa/file prototype riêng, có version và xử lý checkpoint không còn tồn tại bằng về điểm đầu an toàn.
- [ ] **PRG-06 — Reset tiến trình:** thao tác chơi lại toàn tuyến xóa dữ liệu prototype của lượt; chết/reset phòng giữ tiến trình. Không thay hoặc xóa hồ sơ game legacy.

Nghiệm thu: nhận Bash → mở lối cũ → mở cửa tắt → nhận HP → chết → thoát/mở lại; không mất tiến trình và không nhận trùng item. Bản build phải đọc/ghi được save riêng.

## 10. M7 — Camera, HUD, pause và màn hoàn chỉnh (P3)

Camera/reset cơ bản đã có; bổ sung từng phần khi mốc liên quan cần, tích hợp cuối tại đây.

Trạng thái 19/11/2026: **Có code / chờ chơi trực tiếp** cho CAM-01, HUD-01, PAU-01, DBG-01, LVL-01, END-01. **QA-01 chưa làm** (chưa xuất Windows build). Unity compile sạch, Console 0 error.

Triển khai: `PrototypeCameraFollow` (nhìn lệch 2,2 theo hướng, vùng đệm dọc lên 1,6/xuống 0,9 để thấy sàn đáp, giới hạn x −16..64 và y 2..8, snap và xóa smoothing khi hồi sinh). `PrototypeHud` vẽ bằng IMGUI (không Canvas/package): HP theo ô, air dash, Bash khóa/mở/ngắm, checkpoint, thời gian, mục tiêu hiện tại, nhãn phím khớp binding, banner hoàn thành; không có mana/cooldown. F3 bật debug: state, vận tốc, grounded/tường, buffer nhảy/dash/đánh, lượt nhảy/dash, combat, Bash, trạng thái quái, khung hurtbox Player/quái và vùng đánh đang hoạt động; chỉ đọc, không ghi tiến trình. `PrototypePause`: Esc đặt timeScale 0 và dừng timer; hủy ngắm Bash và hitstop trước khi dừng; resume xóa lệnh đã bấm; menu Tiếp tục / Về checkpoint / Chơi lại toàn tuyến (hai lần bấm, ghi rõ xóa tiến trình). Đích: cờ ở x=68,8, chỉ nhận khi Player sống và phòng thử cuối (3 đợt có quái bay) đã xong — kết quả lưu thành `combat_trial_clear` trong save; sự kiện `Finished` phát một lần, tắt quái và đạn, trả timeScale về 1; Ctrl+F5 hoặc Restart route chơi lại. Thứ tự tuyến (LVL-01) được dẫn bằng dòng mục tiêu: checkpoint → combat trial → orb Bash → qua hố/phá cổng/qua cửa → cờ.

Đã kiểm tra Play Mode: camera giữ giới hạn và vẫn thấy Player ở x=66; Esc thật làm timeScale 0, timer đứng yên, nhấn Space lúc pause không buffer, Esc lần hai trả timeScale 1 và Player không nhảy; cờ từ chối khi chưa xong trial, nhận một lần khi đủ điều kiện, lần gọi thứ hai bị từ chối; RestartRun bật lại quái, xóa save, đặt thời gian về 0; F3 bật debug; ảnh chụp 1920×1080 cho thấy HUD và debug hiển thị, Console không lỗi. Chưa kiểm tra: 1280×720, 30/120 FPS, nhấn chuột vào menu pause, Windows build.

- [ ] **CAM-01 — Theo dõi:** nhìn lệch theo hướng đi, vùng đệm dọc, giới hạn theo phòng; ưu tiên thấy sàn đáp. Chuyển phòng không kéo camera qua vùng ngoài màn, hồi sinh snap và xóa smoothing cũ.
- [ ] **HUD-01 — Thông tin:** HP, lượt air dash, Bash khóa/mở, checkpoint và trạng thái hoàn thành. Nhãn phím khớp binding; không làm mana/cooldown UI khi gameplay chưa dùng.
- [ ] **PAU-01 — Pause:** Esc dừng gameplay và timer; resume không thi hành lệnh đã bấm trong menu. Có tiếp tục, về checkpoint và chơi lại toàn tuyến; thao tác cuối ghi rõ sẽ reset tiến trình prototype.
- [ ] **DBG-01 — Debug:** bật/tắt state, velocity, grounded/wall, buffer, lượt nhảy/dash, hitbox/hurtbox và trạng thái quái. Debug không tự tiêu thụ input gameplay hoặc ghi tiến trình.
- [ ] **LVL-01 — Tuyến chơi:** khu chạy/nhảy → tường đứng → checkpoint → melee và quái bắn → nhận Bash → tập neo → phối hợp đạn/địa hình → cửa tắt → phòng thử cuối → đích. Dùng quái bay ở phòng thử cuối.
- [ ] **END-01 — Hoàn thành:** đích chỉ nhận khi Player còn sống và phòng thử cuối đã xong; sự kiện phát một lần, ngừng spawn/attack, trả thời gian về trạng thái hợp lệ và cho chơi lại.
- [ ] **QA-01 — Bản thử:** xuất Windows build, chạy toàn tuyến bằng phím/chuột thật, ghi thời lượng và vấn đề cảm giác. Chốt máy/FPS mục tiêu trước khi kết luận hiệu năng.

Nghiệm thu: đi hết màn, chết/thử lại, pause/resume và hoàn thành/chơi lại đều được; không missing script, lỗi Console hoặc dữ liệu bắt buộc thiếu. Thử 1280×720 và 1920×1080; kiểm tra ở 30/60/120 FPS nếu máy hỗ trợ và ghi điều kiện đo.

## 11. Điều khiển dự kiến

Đây là binding mục tiêu; chỉ ghi “đã áp dụng” sau khi input và nhãn được cập nhật cùng nhau. Các slot skill legacy E/R/F không được hoạt động song song gây xung đột trong sân prototype.

| Thao tác | Binding mục tiêu |
| --- | --- |
| Chạy | A/D hoặc trái/phải |
| Nhảy, nhảy đôi, nhảy tường | Space theo ngữ cảnh |
| Dash | Shift hoặc chuột phải |
| Đánh / đánh lên | LMB / giữ W hoặc lên + LMB |
| Đi xuống platform | S hoặc xuống + Space |
| Bash | Giữ R, ngắm bằng chuột, thả R để phóng |
| Checkpoint | E trong vùng tương tác |
| Pause | Esc |
| Reset nhanh | F5, về checkpoint; trước M3 dùng spawn sân thử |
| Chơi lại toàn tuyến | Ctrl+F5, xóa checkpoint của lượt và về spawn đầu |
| Debug | F3, chỉ dùng khi bổ sung DBG-01 |

## 12. Quy trình bàn giao và nhật ký nghiệm thu

1. Chọn một mốc hoặc nhóm mã nhỏ; đọc trạng thái hiện tại và kiểm tra phụ thuộc.
2. Triển khai code và cấu hình cần thiết trong phạm vi đã được cho phép. Dữ liệu serialized sửa qua Unity Editor, không sửa YAML thủ công.
3. Chạy kiểm tra phù hợp, xem diff; khi có Editor thì kiểm tra import/Console và hành vi thực tế. Ghi rõ nếu chỉ biên dịch hoặc gọi hàm mô phỏng.
4. Chơi thử tình huống nghiệm thu của mã vừa làm, sau đó kiểm tra hồi quy các hành động bị ảnh hưởng.
5. Cập nhật trạng thái mã, kết quả và `.agent/STATE.md`; chỉ chuyển mốc khi không còn lỗi chặn nghiệm thu.

| Ngày | Mã | Trạng thái | Bằng chứng / thông số | Việc còn lại |
| --- | --- | --- | --- | --- |
| 18/11/2026 | M0–M7 | Chưa nghiệm thu theo backlog này | Điểm xuất phát ở mục 2; tài liệu được tạo, chưa chạy kiểm tra mới | Bắt đầu CTL-01 đến CTL-06 |
| 19/11/2026 | MOV-01–MOV-05 | Có code / chờ chơi trực tiếp | 31 kiểm tra gọi hàm/cast: motor, coyote/air jump, cooldown, landing, dash khỏi mép, tường, ngắt combo/Hurt/Dead/Locked/reset; Console 0 error | Chơi bằng phím: nhảy → nhảy đôi → dash qua khe, đụng trần, đáp trong cooldown; UniKey và Windows build chưa nghiệm thu |
| 19/11/2026 | WAL-01–WAL-05 | Có code / chờ chơi trực tiếp | 38 kiểm tra motor/cast và tích hợp gọi hàm đạt; Console 0 error; hai vách tập runtime bên phải | Leo liên tục bằng A/D + Space; quan sát pose/contact marker, thử nhả hướng, ngắt hành động và hồi quy M1 bằng phím |
| 19/11/2026 | ENV-01–CHK-03 | Có code / chờ chơi trực tiếp | 38 kiểm tra input/call/physics đạt, gồm khứ hồi platform 330 bước; Console 0 error | Đi toàn khu mới, thử gai/hố, E, F5/Ctrl+F5, rơi từ platform, hồi quy M1/M2 bằng phím và build |
| 19/11/2026 | CMB-01–ENC-01 | Có code / chờ chơi trực tiếp | 57 kiểm tra combat/input/cast/physics/đợt quái đạt, Console 0 error | Đi sang trái vào phòng thử, chơi ba đợt bằng phím/chuột, thử đánh lên và reset checkpoint; cân bằng thời gian/tầm đánh và build |
| 19/11/2026 | BSH-01–BSH-05, BSH-07 | Có code / chờ chơi trực tiếp | Roslyn toàn assembly exit 0; chưa có Play Mode vì Editor không mở | Mở Unity, kiểm tra Console, chơi khu Bash bên phải: nối ba neo, phản đạn phá cổng, hủy khi bị thương/đổi focus; BSH-06 chờ pause |
| 19/11/2026 | PRG-01–PRG-06 | Có code / chờ chơi trực tiếp | Play Mode: orb/HP/cửa/checkpoint, save khôi phục sau thoát-vào, checkpoint lạ, JSON hỏng, version sai, RestartRun; đo hố cần Bash; Console 0 error | Chơi thật: nhấn E mở cửa, nhảy đôi qua cửa, nhận orb rồi qua hố; Windows build đọc/ghi PlayerPrefs |
| 19/11/2026 | CAM-01, HUD-01, PAU-01, DBG-01, LVL-01, END-01 | Có code / chờ chơi trực tiếp | Play Mode: camera giới hạn, Esc pause/resume, cờ đích một lần, restart, F3, ảnh HUD 1920×1080; Console 0 error. QA-01 chưa làm | Chơi toàn tuyến bằng tay, thử 1280×720 và 30/60/120 FPS, nhấn chuột menu pause, xuất Windows build |
| 19/11/2026 | Polish 60 FPS: control, cảnh báo, chỉ hướng | Có code / chờ chơi trực tiếp | Khóa 60 FPS (vSync 0, fixedDeltaTime 1/60, trả lại khi thoát); ngắm Bash bằng WASD/mũi tên khi không dùng chuột; HUD: nháy đỏ khi bị thương, viền + chữ LOW HP, dấu "!" khi quái gồng, mũi tên mép màn hình cho quái/đạn ngoài tầm nhìn, thanh thời gian ngắm Bash, mũi tên mục tiêu kèm khoảng cách; bot Bash vẫn qua hố (x≈57); Console 0 error | Chơi tay ở 60 FPS: cảm giác ngắm bằng phím, độ rõ cảnh báo, chỉnh cường độ viền/mũi tên nếu gây rối |
| 19/11/2026 | Tay cầm; vật phẩm hồi máu và bẫy; quái Charger và boss Warden | Có code / chờ chơi trực tiếp | Play Mode: gamepad ảo di chuyển/nhảy; hồi máu (đầy máu không tiêu); gai nhịp và cưa gây 1 damage; Charger gồng rồi lao xuyên Player gây damage; boss: kích hoạt ở x≥77, 2 pha (≤12 HP), đạn phản gây 3 damage và làm choáng, không nhận damage cùng phe, chết lưu vào tiến trình, ResetRun giữ boss đã chết, RestartRun reset. Cờ đích dời sang x=98,4 và cần hạ boss; Console 0 error | Chơi tay trận boss (độ khó, nhịp đòn, cửa sổ phản công), cân bằng Charger và bẫy, boss checkpoint bằng E |
| 19/11/2026 | Tấn công kiểu Ori 2: Spirit Edge 5 nhát, chém xuống bật lên, đòn ngang xa; Spirit Arc | Có code / chờ chơi trực tiếp | Play Mode: combo 5 nhát; chém xuống trúng quái -1 HP và bật lên một lần; Arc tốn 0,25 năng lượng, trúng -2 HP; chém trúng +0,25 năng lượng; Console 0 error | Chơi tay: nhịp 5 nhát, chém xuống, giữ xuống + chém dưới đất, bắn Arc bằng chuột/LT, cân bằng năng lượng (3 ô, +0,25 mỗi đòn) |

## 13. Để sau prototype

Bay chậm/luồng gió, grapple, bơi, đào cát, nhiều vũ khí, shard/build nhân vật, skill mana, NPC/nhiệm vụ, kinh tế, boss nhiều phase và mỹ thuật/rig hoàn chỉnh. Chỉ mở rộng khi vòng di chuyển–combat–khám phá hiện tại đã có kết quả chơi thử.

## 14. Nguồn tham khảo

- [Ori and the Blind Forest — hướng dẫn chính thức](https://manuals.thqnordic.com/OriAndTheBlindForest/Ori_PC_QuickstartGuide_online.pdf): tham khảo nhóm năng lực nhảy đôi, nhảy tường, dash, Bash và bay chậm.
- [Xbox Wire — Ori and the Will of the Wisps](https://news.xbox.com/en-us/2020/03/11/ori-and-the-will-of-the-wisps-available-now/): tham khảo hướng combat, vũ khí, kỹ năng và shard. Backlog chủ động để hệ nhiều vũ khí/shard sang giai đoạn sau.

Các tham số, hồi lượt, cách reset và binding ở tài liệu này là thiết kế đề xuất riêng cho Dang Dang, cần điều chỉnh sau chơi thử.
