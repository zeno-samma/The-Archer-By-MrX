# TECHNICAL RISKS — Rủi ro và thứ tự xử lý

Ngày: **30/10/2026**. Chỉ audit, không sửa lỗi hoặc refactor. Những mục dưới đây phân biệt **mẫu code đã xác nhận** với **hậu quả suy luận cần tái hiện**. Không phải kết quả test/Play/build và không phải cam kết Console không lỗi.

## 1. Quy ước ưu tiên

- **P1:** ưu tiên thiết kế/kiểm chứng sớm vì liên quan quyền chỉnh UI, tiền/thưởng/save hoặc có thể chặn tiến trình.
- **P2:** xử lý tiếp theo về lifecycle, tính nhất quán, mở rộng và hiệu năng.
- **P3:** tăng khả năng bảo trì/đo lường sau khi các tuyến trọng yếu ổn định.

P1 không có nghĩa mọi lỗi được nêu đã xảy ra trên save hiện tại. Thứ tự trong bảng là đề xuất; chưa thực hiện thay đổi nào.

## 2. Danh sách rủi ro có bằng chứng

### R01 — P1 — Quyền sở hữu presentation chưa thống nhất

**Quan sát:** ResponsiveMenuLayout.Apply ghi anchors/offset/scale/font/visibility; ArmoryDashboard.RenderCells bố trí cell/icon/label bằng code; Stage page và upgrade/forge/quest chọn lại Image.sprite theo state.

**Nguồn:** [Placement.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuLayout.cs#L16), [ArmoryDashboard.RenderCells](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L232), [MainMenuStagesPageBehavior.InitButtonsWithPurchaseButton](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStagesPageBehavior.cs#L249), [UpgradeDashboardPage.Refresh](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDashboardPage.cs#L59), [ForgePageBehavior.Refresh](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Forge/ForgePageBehavior.cs#L115).

**Nguy cơ:** sửa Image/RectTransform/active trực tiếp trên prefab bị writer runtime thay. Nếu nhiều writer trỏ cùng object, kết quả phụ thuộc lifecycle/thứ tự áp. Không khẳng định mọi target đều trùng.

**Kiểm chứng/đề xuất:** lập ownership target–property trong prefab thực; so giá trị trước/sau Start/Apply/Refresh/đổi orientation; chốt chính sách giữ presentation Inspector trước khi thay code. Bản đồ chi tiết: [UI_RUNTIME_OWNERSHIP.md](UI_RUNTIME_OWNERSHIP.md).

### R02 — P1 — Có đường giao dịch mới và đường UI cũ cùng ghi một dữ liệu

**Quan sát:** dashboard mới dùng PermanentUpgradeService.TryBuy với quote/stale/busy và TryWithdrawAndCommit. `UpgradeCardBehavior.OnUpgradeButtonClicked` vẫn Withdraw trước rồi IncrementUpgradeLevel. Hero popup sửa unlock/level trước rồi Withdraw trực tiếp. Gear popup đã qua EquipmentForge. Không thể coi tất cả đường mua đã được một service bảo vệ.

**Nguồn:** [PermanentUpgradeService.TryBuy](../../Assets/The%20Archer/Scripts/Upgrades/PermanentUpgradeService.cs#L28), [UpgradeCardBehavior.OnUpgradeButtonClicked](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeCardBehavior.cs#L221), [ExpandedHeroPopup.OnUpgradeButtonClicked](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs#L239), [ExpandedItemPopup.OnUpgradeButtonClicked](../../Assets/The%20Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs#L226).

**Nguy cơ:** khác chính sách kiểm tra giá/max/level/khả năng chi trả; UI/state cũ hoặc exception trong event có thể khiến chuỗi ghi dừng giữa các bước. Việc exploit thực tế phụ thuộc đường UI nào đang mở được, chưa tái hiện.

**Kiểm chứng/đề xuất:** xác định call path của card/popup cũ trong prefab đang dùng; thử thiếu tiền/max/double-click/stale quote trên dữ liệu thử; chỉ sau đó chốt một cổng giao dịch chung. Không tự xóa UI cũ.

### R03 — P1 — Event đồng bộ nối kinh tế, save DTO và UI

**Quan sát:** ItemSaveData.Level gọi AchievementsManager.Record rồi OnItemLevelChanged; HeroSaveData.Level gọi quest/achievement rồi event. CurrencySave.Deposit/Withdraw/TryWithdrawAndCommit phát event đồng bộ. Quest claim gọi Deposit và page Quests subscribe wallet để Refresh ngay, dù chỉ Canvas bị ẩn. Achievement page dùng dirty flag và refresh có gate.

**Nguồn:** [ItemSaveData](../../Assets/The%20Archer/Scripts/Armory/ItemSaveData.cs#L7), [HeroSaveData](../../Assets/The%20Archer/Scripts/Armory/HeroSaveData.cs#L7), [CurrencySave.TryWithdrawAndCommit](../../Assets/The%20Archer/Scripts/Toolkit/Currency/CurrencySave.cs#L54), [DailyQuestsManager.Grant](../../Assets/The%20Archer/Scripts/Quests/DailyQuestsManager.cs#L88), [DailyQuestsPageBehavior.Start](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Quests/DailyQuestsPageBehavior.cs#L58), [AchievementsPageBehavior.LateUpdate](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Achievements/AchievementsPageBehavior.cs#L104).

**Nguy cơ:** một UI listener ném exception sẽ truyền ngược lên thao tác tiền/level; callback có thể đọc hoặc sửa collection trong cùng call stack. Đây là khu vực liên quan triệu chứng claim quest rồi đổi tab, **chưa đủ bằng chứng kết luận là nguyên nhân của stack trace cũ** nếu thiếu stack đầy đủ/runtime data.

**Kiểm chứng/đề xuất:** ghi thứ tự callback trên save thử, kiểm tra row binding và trạng thái page ẩn; tách quyết định giao dịch khỏi render sau khi đã có bằng chứng. Không dùng try/catch nuốt lỗi như kết luận sửa sẵn.

### R04 — P1 — Quyết toán run có guard nhưng chưa là transaction nguyên tử

**Quan sát:** DropManager đặt itemsGranted=true trước khi grant hết. StageController đặt IsResultFinalized=true trước PrepareForResult và CommitGameplayBatch. CurrenciesManager.ApplyGameplayAmounts gọi Deposit ví lâu dài rồi Clear ví run. BossChestService có receipt/settlementRequested/claimed/recovery, nhưng một số marker được gán trước Delivery.Grant.

**Nguồn:** [DropManager.GrantPickedUpItems](../../Assets/The%20Archer/Scripts/Drop/DropManager.cs#L73), [StageController.FinalizeRunRewards](../../Assets/The%20Archer/Scripts/Stage/StageController.cs#L437), [CurrenciesManager.ApplyGameplayAmounts](../../Assets/The%20Archer/Scripts/Toolkit/Currency/CurrenciesManager.cs#L91), [BossChestService.CommitRun](../../Assets/The%20Archer/Scripts/Lucky%20Rewards/BossChestService.cs#L81), [BossChestService.RecoverPending](../../Assets/The%20Archer/Scripts/Lucky%20Rewards/BossChestService.cs#L100).

**Nguy cơ:** nếu callback/UI/ghi file lỗi tại một điểm, có thể có state đã ghi một phần nhưng guard chặn lần gọi lại trong phiên; nếu Deposit ném từ listener trước Clear, ví run và ví lâu dài có thể chưa nhất quán. Receipt recovery giảm một phần nguy cơ, không phải bằng chứng transaction toàn run.

**Kiểm chứng/đề xuất:** xác định các mốc durable của grant/transfer/disable/commit; mô phỏng gián đoạn bằng dữ liệu test trong tác vụ riêng; làm rõ exactly-once/idempotency theo từng runToken trước khi đưa online.

### R05 — P1 — Save identity và độ bền file

**Quan sát:** GetSave(string) dùng GetHashCode; SaveCell nhận diện bằng hash. SerializationHelper ghi JSON trực tiếp bằng File.WriteAllText trong nhánh file; chưa thấy cơ chế file tạm + replace/backup/version envelope trong đường ghi này.

**Nguồn:** [SaveManager.GetSave](../../Assets/The%20Archer/Scripts/Toolkit/Save/SaveManager.cs#L77), [SaveDatabase.GetSave](../../Assets/The%20Archer/Scripts/Toolkit/Save/SaveDatabase.cs#L39), [SaveCell](../../Assets/The%20Archer/Scripts/Toolkit/Save/SaveCell.cs#L6), [SerializationHelper](../../Assets/The%20Archer/Scripts/Toolkit/Save/SerializationHelper.cs#L12).

**Nguy cơ:** va chạm/đổi cách hash hoặc đổi key khó migration; crash/lỗi IO khi ghi có thể ảnh hưởng file. Không khẳng định hash đang khác giữa hai lần chạy cụ thể, không khẳng định save đã hỏng, không coi JSON là mã hóa.

**Kiểm chứng/đề xuất:** chụp schema/key/version và thiết kế migration trước khi đổi nhận diện; kiểm tra round-trip/gián đoạn trên bản sao save. Không reset save thật để thử.

### R06 — P1 — Dữ liệu Inspector có thể phá hợp đồng số lượng/index

**Quan sát:** UpgradeDashboardPage.Refresh tạo hai mảng stat dài 5 nhưng lặp theo `currentStats.Length`, đồng thời truy cập `nextStats[i]`. ArmoryDashboard.Refresh truy cập bốn statValues/bonuses cố định. ArmoryDashboard.Wire ghép heroButtons[i] với heroIds[i]. Result screen dùng currencies[0].

**Nguồn:** [UpgradeDashboardPage.Refresh](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDashboardPage.cs#L59), [ArmoryDashboard.Refresh](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L167), [ArmoryDashboard.Wire](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L116), [StageCompleteScreen.CreateCurrencyRewardCards](../../Assets/The%20Archer/Scripts/UI/StageCompleteScreen.cs#L64), [StageFailedScreen.CreateCurrencyRewardCards](../../Assets/The%20Archer/Scripts/UI/StageFailedScreen.cs#L65).

**Nguy cơ có điều kiện rõ:** currentStats >5, nextStats ngắn hơn, list hero lệch hoặc currency rỗng có thể truy cập ngoài phạm vi/null. Database currency hiện có 3 entry, vì vậy ví dụ currencies rỗng không phải cấu hình hiện tại đã xác nhận.

**Kiểm chứng/đề xuất:** kiểm tra serialized list/reference thực và bổ sung authoring contract/validation trong tác vụ được phép sửa sau này. Đây là lý do quyền sửa Inspector cần đi kèm contract, không phải khóa Inspector bằng code.

### R07 — P2 — Page ẩn, component disabled và object destroyed có lifecycle khác nhau

**Quan sát:** MainMenuPageBehavior tắt Canvas; Quests vẫn subscribe Refresh đến OnDestroy. ExpandedArmoryPopup tháo input qua nút Close nhưng chưa có fallback OnDisable/OnDestroy. ArmoryHeroPanel.Init thêm listener mỗi lần, Clear tháo; MainMenuStagesPageBehavior subscribe StageSave trong Start, tháo OnDisable nhưng không rebind OnEnable.

**Nguồn:** [MainMenuPageBehavior.OnMoveFinished](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuPageBehavior.cs#L105), [DailyQuestsPageBehavior.Start](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Quests/DailyQuestsPageBehavior.cs#L58), [ExpandedArmoryPopup.OnCloseButtonClicked](../../Assets/The%20Archer/Scripts/UI/Armory/ExpandedArmoryPopup.cs#L149), [ArmoryHeroPanel.Init](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L76), [ArmoryHeroPanel.Clear](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L308), [MainMenuStagesPageBehavior.OnDisable](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStagesPageBehavior.cs#L502).

**Nguy cơ:** hidden refresh, đăng ký lặp nếu gọi Init sai chu kỳ, callback tới popup cũ hoặc mất cập nhật khi tắt/bật object bằng Inspector. Không coi mọi thiếu RemoveListener trên button con cùng lifetime là leak.

**Kiểm chứng/đề xuất:** lập và kiểm chứng protocol Show→Hide/Disable→Destroy, đặc biệt external SetActive(false) và reload scene; xem bảng event đầy đủ trong UI_RUNTIME_OWNERSHIP.

### R08 — P2 — Nhiều owner của pause, dock và UI focus

**Quan sát:** pause/ability/chest/result đều đặt hoặc ease Time.timeScale; các popup có DisableDock/EnableDock và đổi EventSystem selection. Không thấy token/stack owner chung cho toàn bộ panel. GameScreen.PrepareForResult đã có dọn panel khi chốt kết quả.

**Nguồn:** [PauseScreenBehavior.OnResumeButtonClicked](../../Assets/The%20Archer/Scripts/UI/Pause/PauseScreenBehavior.cs#L215), [AbilitySelectorBehavior.OnSkipButtonClicked](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs#L282), [HPChestUI.Show](../../Assets/The%20Archer/Scripts/UI/Chest/HPChestUI.cs#L45), [StageCompleteScreen.Show](../../Assets/The%20Archer/Scripts/UI/StageCompleteScreen.cs#L84), [MainMenuScreenBehavior.DisableDock](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuScreenBehavior.cs#L100), [GameScreenBehavior.PrepareForResult](../../Assets/The%20Archer/Scripts/UI/GameScreenBehavior.cs#L80).

**Nguy cơ:** callback trễ đóng A khôi phục gameplay/input trong khi B còn mở; dock có thể kẹt nếu không đi đúng nhánh đóng. Đây là tình huống cần kiểm tra, chưa khẳng định giao diện hiện tại luôn cho phép mở mọi cặp popup.

**Kiểm chứng/đề xuất:** kiểm tra chồng popup/reward/XP ngay khi chết, pause và đổi scene; thiết kế ownership sau khi inventory các caller hoàn tất.

### R09 — P2 — Bootstrap và static dependency có phạm vi lỗi lớn

**Quan sát:** GameController/StageController chứa service locator static; Game scene đọc dịch vụ menu; StageController downcast ISaveManager để batch. Không có assembly boundary trong Assets.

**Nguồn:** [GameController](../../Assets/The%20Archer/Scripts/Other/GameController.cs#L17), [StageController.Awake](../../Assets/The%20Archer/Scripts/Stage/StageController.cs#L119), [StageController.FinalizeRunRewards](../../Assets/The%20Archer/Scripts/Stage/StageController.cs#L437), [PlayerBehavior.Awake](../../Assets/The%20Archer/Scripts/Player/PlayerBehavior.cs#L103).

**Nguy cơ:** thiếu/đăng ký sai một manager có thể làm nhiều UI/gameplay lỗi tiếp; thay ISaveManager có thể bỏ mất batching. Khó chạy subsystem độc lập. ResetStatics hiện đã tồn tại, không kết luận thiếu reset toàn bộ.

**Kiểm chứng/đề xuất:** tài liệu hóa prerequisite từng entry, giữ bootstrap chuẩn; tiến tới dependency explicit theo từng service sau khi contract dữ liệu ổn định.

### R10 — P2 — Nhiều cách tính stat và một mapping crit cần đối chiếu

**Quan sát:** PlayerStatsHandler.Init lấy CritDamageMultiplier từ StatType.DamageReduction. ArmoryHeroPanel tính HP/damage cơ bản; ArmoryStatsSnapshot tự switch áp permanent upgrades; gameplay chạy UpgradeBehavior thực.

**Nguồn:** [PlayerStatsHandler.Init](../../Assets/The%20Archer/Scripts/Player/PlayerStatsHandler.cs#L115), [ArmoryHeroPanel.RecalculateStats](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L157), [ArmoryStatsSnapshot.Calculate](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryStatsSnapshot.cs#L17), [UpgradesManager.OnStageLoaded](../../Assets/The%20Archer/Scripts/Upgrades/UpgradesManager.cs#L37).

**Nguy cơ:** menu projection lệch combat khi thêm/đổi upgrade; crit multiplier có thể dùng sai stat theo ý đồ thiết kế. Lệnh mapping đã xác nhận; kết luận “sát thương crit hiện sai bao nhiêu” cần đọc data và thử combat.

**Kiểm chứng/đề xuất:** so hero+gear+upgrade theo cùng bộ cấu hình ở menu/combat, xác nhận đơn vị crit 0–100 và multiplier trước khi cân bằng.

### R11 — P2 — Điều kiện chuyển wave phụ thuộc UI; phép tính delay bất thường

**Quan sát:** EndWave chờ AllWaveDropPickedUp/HasAliveIndicators/WaitForLevelUpToEnd. Nó lấy `startWaitingTime = Time.time`, sau đó `delay -= startWaitingTime`, tức trừ timestamp tuyệt đối thay vì khoảng chờ đã trôi.

**Nguồn:** [RoomBehavior.EndWave](../../Assets/The%20Archer/Scripts/Stage/Room/RoomBehavior.cs#L212), [RoomBehavior.OnAbilitiesSelectorClosed](../../Assets/The%20Archer/Scripts/Stage/Room/RoomBehavior.cs#L324), [DropManager.OnItemTextIndicatorHidden](../../Assets/The%20Archer/Scripts/Drop/DropManager.cs#L244).

**Nguy cơ:** thiếu callback indicator/selector có thể giữ wave chờ; khi timestamp lớn hơn delay cấu hình thì delay bị clamp0, làm nhịp spawn khác cấu hình. Phép tính đã thấy; chưa đo nhịp thực tế của từng stage.

**Kiểm chứng/đề xuất:** đo delay trước/sau phòng dài và đường selector/notification bị đóng bởi kết quả; giữ nguyên pacing cho đến khi có số đo.

### R12 — P2 — Preview hero có ngân sách ảnh cố định và cleanup chưa rõ

**Quan sát:** HeroPreviewBehavior tạo RT512×512 ARGBHalf; gán camera/raw image trong Awake; Clear không Release/Destroy RT, không có OnDestroy trong class.

**Nguồn:** [HeroPreviewBehavior.Awake](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L27), [HeroPreviewBehavior.Clear](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L97).

**Nguy cơ:** phóng RawImage lớn có giới hạn nét; lặp menu/preview cần kiểm tra tài nguyên native và camera/model. Chưa có profiling nên không khẳng định memory leak hoặc RT là nguyên nhân duy nhất của ảnh nhòe.

**Kiểm chứng/đề xuất:** đo kích thước hiển thị và texture/camera count qua nhiều lần vào/ra, sau đó đặt chính sách chất lượng cấu hình được bằng Inspector.

### R13 — P2 — Pool và modifier phụ thuộc vòng đời caller

**Quan sát:** PoolComponent.GetEntity bật GameObject trước khi caller gán target/damage/transform. GetProjectile/GetParticle trả null nếu chưa Register. SimpleProjectile.ResetOverrides hiện đã reset TargetToIgnore/piercing/ignore obstacles/crit, nên không dùng nhận định cũ rằng các field đó luôn bị sót. Ability base tháo Data event trong OnDestroy; permanent upgrade Clear RemoveModifier.

**Nguồn:** [PoolComponent.GetEntity](../../Assets/The%20Archer/Scripts/Toolkit/Pool/PoolComponent.cs#L43), [ProjectilesManager.GetProjectile](../../Assets/The%20Archer/Scripts/Projectiles/ProjectilesManager.cs#L26), [ParticlesManager.GetParticle](../../Assets/The%20Archer/Scripts/Particles/ParticlesManager.cs#L26), [SimpleProjectile.ResetOverrides](../../Assets/The%20Archer/Scripts/Projectiles/SimpleProjectile.cs#L122), [AbilityBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/Abilities/Behaviors/AbilityBehavior.cs#L47), [PowerUpgradeBehavior.Clear](../../Assets/The%20Archer/Scripts/Upgrades/Behaviors/PowerUpgradeBehavior.cs#L21).

**Nguy cơ:** thêm OnEnable/timer/effect mới có thể dùng dữ liệu lần trước; caller quên registration hoặc cleanup tạo lỗi dây chuyền. Đây là contract nhạy cảm, không phải kết luận reset hiện tại sai toàn bộ.

**Kiểm chứng/đề xuất:** kiểm tra lần spawn đầu, tái sử dụng, chết giữa animation, Clear/Hide và thoát scene cho nội dung mới; kiểm kê modifier theo owner.

### R14 — P3 — Ghi HP callback lặp và số đo thời gian có thể khó diễn giải

**Quan sát:** PlayerBehavior.StartPlaying thêm `onHPChanged += SaveHP` mỗi lần, trong khi StageController.LoadNextRoom lại gọi StartPlaying. OnDestroy chỉ tháo một lần. ConsoleTimer.LogAllResults cộng vào totalElapsedTime không reset ở đầu; ResetSession để timerResults.Clear thành comment.

**Nguồn:** [PlayerBehavior.StartPlaying](../../Assets/The%20Archer/Scripts/Player/PlayerBehavior.cs#L186), [PlayerBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/Player/PlayerBehavior.cs#L573), [StageController.LoadNextRoom](../../Assets/The%20Archer/Scripts/Stage/StageController.cs#L377), [ConsoleTimer.LogAllResults](../../Assets/The%20Archer/Scripts/Other/ConsoleTimer.cs#L79), [ConsoleTimer.ResetSession](../../Assets/The%20Archer/Scripts/Other/ConsoleTimer.cs#L93).

**Nguy cơ:** nhiều callback SaveHP cùng ghi một field; gọi log nhiều lần có thể tổng cộng dồn. **SaveHP chỉ ghi object RAM, không tự ghi file**, không dùng nó để kết luận mỗi lần mất HP đều ghi đĩa.

**Kiểm chứng/đề xuất:** đếm callback/keys qua nhiều room và so timestamp thực; ổn định định nghĩa thời gian gameplay/pause trước khi dùng để cân bằng/ranking.

### R15 — P2 — Localization là writer dựa trên text nguồn, không phải ID ổn định

**Quan sát:** MenuTranslations.Translate dùng bản text tiếng Anh được normalize/template; LocalizedMenuScope giữ source text/font rồi Apply trước render. UI khác vẫn gán text theo data/state.

**Nguồn:** [MenuTranslations.Translate](../../Assets/The%20Archer/Scripts/Localization/MenuTranslations.cs#L38), [LocalizedMenuScope.Bind](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LocalizedMenuScope.cs#L13), [LocalizedMenuScope.Apply](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LocalizedMenuScope.cs#L21).

**Nguy cơ:** đổi câu English hoặc chuỗi động làm lookup không khớp; tên skill/item mới chưa có entry; thứ tự binding/dịch/font có thể khiến thiếu dịch hoặc ghi qua lại. Không coi mọi text tiếng Anh là lỗi nếu chưa kiểm tra key/attachment scope.

**Kiểm chứng/đề xuất:** kiểm kê key/context và các text động, xác định owner cuối trước render; chỉ chuyển sang stable key khi có kế hoạch dữ liệu.

### R16 — P2 — Công cụ dev và kế hoạch online chưa có ranh giới authority server

**Quan sát:** DeveloperToolsSettings có toolsEnabled/allowInDevelopmentBuilds/allowInReleaseBuilds; DeveloperGrantService cấp qua delivery/save local. Save DTO và ví vẫn writable từ client. Chưa tìm thấy auth/server API/leaderboard service trong source gameplay đã khảo sát.

**Nguồn:** [DeveloperToolsSettings](../../Assets/The%20Archer/Scripts/Development/DeveloperToolsSettings.cs#L7), [DeveloperGrantService.GrantCurrency](../../Assets/The%20Archer/Scripts/Development/DeveloperGrantService.cs#L19), [CurrencySave](../../Assets/The%20Archer/Scripts/Toolkit/Currency/CurrencySave.cs#L8), [GameController](../../Assets/The%20Archer/Scripts/Other/GameController.cs#L17).

**Nguy cơ tương lai:** chỉ bật một cờ online và upload JSON không biến thao tác tiền/loot/ranking thành server-authoritative. DevTools cờ hiển thị cũng không phải quyền admin server.

**Đề xuất:** khi được giao online, tách authentication và API transaction/run validation; giữ offline provider; không dùng trạng thái client làm dữ liệu xếp hạng đáng tin cậy. Hiện chưa tạo backend hoặc đổi kiến trúc.

## 3. Những cơ chế đang bảo vệ hệ thống

Không chỉ ghi nhận rủi ro; source hiện có:

- Guard loading/start; energy charge tách resume; content ID của run khác stage selection UI.
- Expected level/capacity và busy guard trong EquipmentForge, PermanentUpgradeService, InventoryBagService.
- Result finalization guard; BossChest receipt/pending recovery; CurrencySave.TryWithdrawAndCommit commit state trước wallet observer bình thường.
- Gameplay save defer và worker join trước ForceSave, giảm một số ghi trùng/ghi đè cũ.
- Dirty flag/gate ở dashboard Upgrades/Achievements/Forge; nhiều listener đã có cleanup; SimpleProjectile reset các override quan trọng.

Nguồn: [GameController.TryStartNewRun](../../Assets/The%20Archer/Scripts/Other/GameController.cs#L201), [EquipmentForge.TryUpgrade](../../Assets/The%20Archer/Scripts/Armory/EquipmentForge.cs#L60), [InventoryBagService.TryExpand](../../Assets/The%20Archer/Scripts/Armory/InventoryBagService.cs#L29), [PermanentUpgradeService.TryBuy](../../Assets/The%20Archer/Scripts/Upgrades/PermanentUpgradeService.cs#L28), [StageController.FinalizeRunRewards](../../Assets/The%20Archer/Scripts/Stage/StageController.cs#L437), [SaveManager.ForceSave](../../Assets/The%20Archer/Scripts/Toolkit/Save/SaveManager.cs#L276), [SimpleProjectile.ResetOverrides](../../Assets/The%20Archer/Scripts/Projectiles/SimpleProjectile.cs#L122). Các cơ chế này cần được bảo toàn nếu xử lý rủi ro sau audit.

## 4. Thứ tự xử lý đề xuất

| Đợt | Mục tiêu | Phạm vi nên chốt trước | Xác minh đề xuất khi được phép thực hiện |
| --- | --- | --- | --- |
| 1 | Trả quyền chỉnh UI và tránh crash do cấu hình | R01, R06, R07; target/property, list/reference contract, lifecycle panel | So prefab trước/sau Play; tab/claim/language/orientation; external disable/re-enable |
| 2 | Tiền, thưởng và save nhất quán | R02–R05; mọi writer/call path, commit markers và recovery | Save thử: thiếu tiền/max/full bag/claim lặp; chết/clear/quit/resume; gián đoạn từng mốc |
| 3 | Gameplay và stat có thể cân bằng | R10, R11, R13, R14 | So stat menu/combat, callback pool, delay wave/room và thời gian không pause |
| 4 | Chất lượng/khả năng bảo trì | R08, R09, R12, R15 | Popup chồng, bootstrap, preview tài nguyên, completeness dịch |
| 5 | Nền tảng online khi có yêu cầu riêng | R16; identity, transaction, server validation, migration | Môi trường test/server riêng, không dùng save/tiền thật |

Không có thao tác sửa source hay asset nào được thực hiện cùng báo cáo. Chưa thể kết luận “không còn lỗi Console” từ đọc tĩnh.
