# UI RUNTIME OWNERSHIP — Quyền điều khiển giao diện

Ngày: **30/10/2026**. Chỉ báo cáo source hiện tại; không sửa presentation, component, prefab hoặc scene. Quy ước bằng chứng tại [SYSTEM_OVERVIEW.md](SYSTEM_OVERVIEW.md).

## 1. Kết luận có bằng chứng

**Hiện tại chưa thể coi mọi giá trị chỉnh trực tiếp trên UI prefab là giá trị cuối cùng khi Play.** Source còn có các writer runtime cho RectTransform, font, sprite, màu, active state, Canvas và animation. Đây là quan sát về lệnh ghi, không phải khẳng định mọi script đang cùng sửa một object trong scene cụ thể.

Ví dụ trực tiếp:

| Thuộc tính | Writer và thời điểm | Vì sao Inspector có thể khác Play |
| --- | --- | --- |
| Anchor/pivot/offset/scale | [Placement.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuLayout.cs#L16), lần LateUpdate đầu và đổi orientation | Áp profile serialize vào target, thay giá trị RectTransform đang nhìn trong prefab |
| Kích thước/scale mặt phẳng menu | [ResponsiveMenuSurface.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuSurface.cs#L17), Awake và đổi screen/safe area/canvas size | Tính lại root từ design size và vùng an toàn |
| Ô inventory | [ArmoryDashboard.Refresh](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L167), [ArmoryDashboard.RenderCells](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L232) | Tính size/position theo cột và viewport, ép anchor/pivot, đặt lại rect icon/label |
| Sprite nút Start | [MainMenuStagesPageBehavior.InitButtonsWithPurchaseButton](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStagesPageBehavior.cs#L249), `InitButtonsWithoutPurchaseButton` | Chọn sprite enabled/disabled do script giữ; đổi Image.sprite đơn lẻ có thể bị ghi lại |
| Sprite nút upgrade/forge/claim | [UpgradeDashboardPage.Refresh](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDashboardPage.cs#L59), [ForgePageBehavior.Refresh](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Forge/ForgePageBehavior.cs#L115), [DailyQuestsPageBehavior.Refresh](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Quests/DailyQuestsPageBehavior.cs#L137) | Lấy từ các field sprite hoặc page database theo trạng thái giao dịch |
| Font/text | [LocalizedMenuScope.Apply](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LocalizedMenuScope.cs#L21); [Placement.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuLayout.cs#L16) | Localization đặt text/font; responsive đặt font size/auto-size |
| GameObject.activeSelf | [Placement.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuLayout.cs#L16), [MainMenuStagesPageBehavior.InitInfoButton](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStagesPageBehavior.cs#L234), [MainMenuDockBehavior.OnInputChanged](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuDockBehavior.cs#L132) | Runtime chủ động bật/tắt theo orientation/stage/input, không đọc một cờ “tác giả cho phép hiện” riêng cho mọi view |

**Không kết luận lỗi do Unity tự ý đổi prefab.** Các lệnh ghi trên là ứng viên cụ thể. Muốn gán trách nhiệm cho một nút/ảnh trong scene cần nối reference target, profile và prefab instance override của chính object đó.

## 2. Các tầng writer hiện hữu

| Tầng | Chủ sở hữu trong source | Phạm vi ghi |
| --- | --- | --- |
| Authoring | Scene/prefab + Inspector; công cụ Editor Build | Cây view, field reference, hình gốc, layout ban đầu |
| Responsive | ResponsiveMenuSurface, ResponsiveMenuLayout, SafeAreaManager | Root, rect, profile font/visibility/grid |
| Điều hướng page | MainMenuScreen/Dock/Page | Canvas, alpha/raycast, page offset, selected state, input selection |
| Binding nghiệp vụ | Các dashboard/page/card | Text giá/stat, icon vật phẩm, enabled/interactable, sprite theo state |
| Animation | EasingManager và script caller | Scale/position/alpha/rotation; một số giá trị đích hardcoded |
| Localization | MenuLanguage/LocalizedMenuScope | TMP text/font trước render |
| Layout component | Unity LayoutGroup/ContentSizeFitter/AspectRatioFitter trong view | Kích thước con và aspect theo cấu hình component; phải kiểm tra target trùng với các script trên |

Phụ thuộc vào layout component có trong prefab không tự chứng minh xung đột. Nguồn code của tầng đầu đến localization được dẫn ở các mục tiếp theo; cấu hình thực tế nằm ở scene/prefab tương ứng.

## 3. Các script layout quan trọng cần đọc trước khi chỉnh prefab

| File / class.method | Hành vi runtime đã thấy | Lưu ý về ownership |
| --- | --- | --- |
| [ResponsiveMenuSurface.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuSurface.cs#L17) | Đặt root về anchor/pivot giữa, size design, position và fit scale | Hợp đồng responsive nằm ở component này, không chỉ RectTransform gốc |
| [Placement.Apply](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuLayout.cs#L16) | Áp rectangles/texts/rows/visibility/grids và 4/6 cột inventory | Có thể bật lại view tắt bằng activeSelf trong prefab |
| [SafeAreaManager.ApplySafeArea](../../Assets/The%20Archer/Scripts/UI/SafeAreaManager.cs#L23) | Đổi anchors các screen theo safe area | Nếu cùng target với surface/profile cần xác định thứ tự, chưa xác nhận mọi target trùng |
| [MainMenuPageBehavior.MoveLeft](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuPageBehavior.cs#L40) / `MoveRight`, `MoveCenter`, `SpawnLeft/Right` | Easing offset/alpha, bật Canvas và điều khiển raycast | Layout page có animation runtime là chủ đích; điểm kết thúc không hoàn toàn lấy từ rect đã sửa |
| [ArmoryDashboard.RenderCells](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L232) | Ép geometry cell/icon/label, empty icon full rect, lock/label active | Dùng view serialize sẵn nhưng vẫn bố trí bằng số trong code |
| [ArmoryHeroPanel.InitSlots](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L182) / `SetSlotsScale`, `RecalculateStats` | Card theo slot rect; scale slot/card; tính width stat panel và vị trí label | Cùng panel có thể được Stage và Armory khởi tạo theo thời điểm khác |
| [StatsGrid.RecalculateSize](../../Assets/The%20Archer/Scripts/UI/Armory/StatsGrid.cs#L75) / [StatIndicator.UpdateSize](../../Assets/The%20Archer/Scripts/UI/Armory/StatIndicator.cs#L56) | Grid stat và width theo preferredWidth | Đây là adaptive layout, không phải text binding đơn thuần |
| [ExpandedHeroPopup.InitVisuals](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs#L43) / [ExpandedItemPopup.InitVisuals](../../Assets/The%20Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs#L44) | Tính height panel, vị trí close, height abilities theo số stat/card | Chiều cao chỉnh tay có thể bị tính lại mỗi lần Refresh |
| [ForgePageBehavior.RefreshStats](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Forge/ForgePageBehavior.cs#L150) | Chia lại anchor từng dòng stat theo số dòng đang có | Tác động ngay lúc bind quote |
| [AchievementsPageBehavior.Fill](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Achievements/AchievementsPageBehavior.cs#L281) / [DailyQuestsPageBehavior.Fill](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Quests/DailyQuestsPageBehavior.cs#L256) | Thay anchorMax và active progress fill | Đây là binding tiến độ; vẫn là writer geometry/visibility |
| [UpgradeDescriptionBlock.RecalculateBackgroundRect](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDescriptionBlock.cs#L63) | Size và position background/stat text theo nội dung | Luồng card/popup upgrade cũ |
| [MainMenuInfoPopup.Show](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuInfoPopup.cs#L43) | Khôi phục start size rồi tính content/animation | Inspector preview có thể khác nội dung runtime |
| [AbilitySelectorBehavior.InitRarityData](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs#L443) / [AbilityCardBehavior.Init](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs#L162) | Width rarity, scale card, flip/move slot | Selector có geometry do rarity/animation quyết định |
| [AbilitySelectorSlotBehavior.ScrollingCoroutine](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs#L111) | Recycle position/scale card trong hiệu ứng cuộn | Không phải danh sách tĩnh |
| [HorizontalSelector.OnLeftButtonClicked](../../Assets/The%20Archer/Scripts/UI/HorizontalSelector.cs#L109) / `OnRightButtonClicked` | Vị trí selectable ±700 và easing | Giá trị cố định cần được nhận diện khi đổi design size |
| [ScalableAnimatedTitleUI.RecalculateSizesAndScales](../../Assets/The%20Archer/Scripts/UI/ScalableAnimatedTitleUI.cs#L50) | Width/position các phần title theo preferredWidth | Đọc cùng AnimatedTitleUI và TMP |
| [ScalingLabelBehavior](../../Assets/The%20Archer/Scripts/UI/ScalingLabelBehavior.cs#L8) | Đo text/icon và cập nhật rect qua helper | Giá trị text thay đổi kéo theo layout |
| [StageCompleteScreen.Show](../../Assets/The%20Archer/Scripts/UI/StageCompleteScreen.cs#L84) / [StageFailedScreen.Show](../../Assets/The%20Archer/Scripts/UI/StageFailedScreen.cs#L85) | Reward card scale và bố trí kết quả | Clone view/pool từ prefab; không phải dựng control mới từ đầu |
| [TextIndicatorBehavior.SetAnchors](../../Assets/The%20Archer/Scripts/UI/Text%20Indicators/TextIndicatorBehavior.cs#L78) / `SetPosition`, `SetScale` | World text anchors/position/scale trong mỗi lần spawn/update | Dynamic world UI theo mục tiêu |
| [LuckyRewardsPopup.BindPrizes](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Lucky%20Rewards/LuckyRewardsPopup.cs#L54) / [LuckyWheelSlice.Bind](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Lucky%20Rewards/LuckyWheelSlice.cs#L11) | Slice rect stretch, góc/fill; icon đặt theo góc vòng quay | Geometry wheel được tính runtime; xác suất phần thưởng lấy từ table |

`ForgePageBehavior.ResizeText`, `DailyQuestsPageBehavior.ResizeRows`/`ScaleText` và `AchievementsPageBehavior.ResizeLayout` còn chứa lệnh thay font/height, nhưng tìm call trong source UI hiện chỉ thấy **định nghĩa**. Chúng là helper cần lưu ý, chưa có bằng chứng đang được lifecycle gọi. Không gộp chúng thành lỗi runtime đang xảy ra. Các hàm public khác trong bảng cũng phải xem scene/reference/caller để xác định đang dùng hay không.

## 4. Runtime construction và prefab instance

Trong lần quét các script dự án ngoài thư mục Editor, có loại bỏ block `#if UNITY_EDITOR` khi xét lệnh trực tiếp, chưa thấy cây uGUI/TMP được dựng ở player runtime bằng `new GameObject` + `AddComponent<Image/Button/TMP...>`.

Tuy nhiên vẫn có **clone view có sẵn** và pool:

- `ArmoryGridUI`, `ArmoryHeroPanel`, `StatsGrid`, ability selector/cards, world text: dùng `PoolComponent<T>` với prefab.
- `LuckyRewardsPopup.BindPrizes`: instantiate row/slice prefab khi cần.
- `StageCompleteScreen.CreateCurrencyRewardCards` và StageFailedScreen tương tự: clone `coinsRewardCard` có sẵn cho từng currency.
- `HeroPreviewBehavior` tạo scene/model preview và RenderTexture; đây là 3D preview, không phải dựng cây Button/TMP.

Nguồn: [ArmoryHeroPanel.EnsureInitialized](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L63), [LuckyRewardsPopup.BindPrizes](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Lucky%20Rewards/LuckyRewardsPopup.cs#L54), [StageCompleteScreen.CreateCurrencyRewardCards](../../Assets/The%20Archer/Scripts/UI/StageCompleteScreen.cs#L64), [PoolComponent.CreateEntity](../../Assets/The%20Archer/Scripts/Toolkit/Pool/PoolComponent.cs#L23), [HeroPreviewBehavior.Awake](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L27).

Các hàm `Build`, `CreateCell`, `Text`, `Button`, `Rect`, `BakeModal` trong ArmoryDashboard và các helper build ở Forge/Quests/Achievements nằm trong `#if UNITY_EDITOR`. Chúng vẫn compile trong Editor, nhưng không có nghĩa tự chạy khi Play; cần call authoring cụ thể. Không được xóa chúng chỉ vì tìm thấy chuỗi AddComponent. [ArmoryDashboard.Build](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L41).

Phân biệt then chốt: **không dựng cây UI mới** không có nghĩa **không đổi layout/sprite bằng code**. Các runtime writer ở mục 1–3 vẫn tồn tại.

## 5. Vòng đời page và event

`MainMenuPageBehavior` dùng `Canvas.enabled` và CanvasGroup để ẩn/khóa trang. Tắt Canvas không tắt GameObject/MonoBehaviour và không tự gọi OnDisable. Vì vậy “không thấy panel” và “đã hủy listener” là hai trạng thái khác nhau. [MainMenuPageBehavior.PrepareInitialState](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuPageBehavior.cs#L23), [MainMenuPageBehavior.OnMoveFinished](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuPageBehavior.cs#L105).

| Component | Đăng ký | Hủy / cơ chế khi ẩn | Nhận định dựa trên source |
| --- | --- | --- | --- |
| MainMenuScreenBehavior | Dock.onPageSelected trong Start | Chưa thấy OnDestroy đối xứng trong class | Publisher dock thường cùng scene; không đủ bằng chứng gọi đây là leak xuyên scene |
| MainMenuDockBehavior | Input.onInputChanged/UI.Triggers trong Start; item.onSelected trong Init | OnDestroy tháo input; chưa thấy tháo từng item.onSelected | Init nhiều lần cần kiểm tra; không suy ra đã Init lặp |
| MainMenuStagesPageBehavior | StageSave.onSelectedStageChanged trong Start; input theo vào/ra page | OnDisable tháo StageSave và input; không có OnEnable rebind StageSave | Re-enable GameObject sau Disable khác đường Canvas transition; có rủi ro mất cập nhật selection |
| MainMenuStageSelector | StageSave.onSelectedStageChanged trong Init | OnDestroy tháo StageSave/input | Đã có cleanup cho publisher save lâu dài |
| MainMenuArmoryPageBehavior | Tab/popup/equipment trong Start; input khi page vào giữa | MoveLeft/Right tháo input, HideDashboard; OnMoveFinished Clear hero; OnDestroy tháo event | Đường page bình thường có cleanup chủ động |
| ArmoryDashboard | Show subscribe Armory/Bag/Crafting/UpgradeData; BindSaves subscribe level | Hide tháo service/data; OnDestroy gọi Hide nếu listening | Chủ yếu lifecycle Show/Hide, không phải OnEnable/OnDisable |
| ArmoryHeroPanel | Init += Armory equipment/hero | Clear và OnDestroy -= | Init không có guard subscriber; EnsureInitialized chỉ guard tạo pool. Gọi Init lặp không Clear có thể tích lũy listener |
| HeroPreviewBehavior | Show += Armory.OnSelectedHeroChanged | Clear -= | Không thấy OnDestroy riêng; cleanup dựa vào caller Clear |
| UpgradeDashboardPage | Start subscribe UpgradeData, wallet, Armory | Leave chỉ visible=false; OnDestroy tháo | Hidden page nhận event để MarkDirty, Refresh có visible guard |
| ForgePageBehavior | Start subscribe wallet; LateUpdate tìm Crafting và subscribe | OnDestroy tháo wallet/Crafting/settings; tắt enabled khi rời | Dùng dirty flag, không tháo mọi event khi Canvas ẩn |
| DailyQuestsPageBehavior | Start subscribe manager.Changed→Refresh, wallet→Refresh | OnDestroy tháo | Wallet thay đổi vẫn có thể Refresh ngay khi Canvas ẩn |
| AchievementsPageBehavior | Start subscribe manager/wallet→MarkDirty | OnDestroy tháo; LateUpdate gate canvas/visible | Hidden event không tự render row ngay như Quests |
| LocalizedMenuScope | Bind remove/add MenuLanguage.Changed + Canvas.preWillRenderCanvases | OnDisable tháo cả hai | Canvas ẩn đơn thuần chưa chắc tháo; ApplyDirty còn dựa render callback |
| MenuWalletView / CostButtonBehavior | Bind/SetCurrency và enable subscribe wallet | Rebind tháo ví trước; OnDisable tháo | Pattern tương đối rõ cho wallet publisher lâu dài |
| MainMenuSettingsPopup | OnEnable subscribe toggle/input/click | OnDisable tháo đối xứng | Có cleanup khi popup bị tắt bằng GameObject |
| ExpandedArmoryPopup, Hero/Item popup | Show → SubscribeToEvents(input/cancel) | Close click → Unsubscribe; base không có OnDisable/OnDestroy fallback | External deactivate/destroy không đi Close cần được kiểm tra riêng |
| ExpandedUpgradePopup | Show subscribe Cancel/selector; Awake subscribe selector/click | Hide tháo Cancel và selector input | Chưa thấy fallback cleanup đầy đủ cho external disable trong class |
| LuckyRewardsPopup | Show subscribe gem/service/Cancel, khóa dock | OnDisable dừng coroutine, tháo cả ba, trả dock/selection | Có nhánh cleanup cho việc tắt popup ngoài nút close |
| AbilitySelectorBehavior | Awake XP; OnEnable input; mỗi lần Show wallet/card | OnDisable tháo input; hide/CancelForResult tháo wallet/card; OnDestroy tháo XP | Nhiều phạm vi listener; CancelForResult quan trọng khi chốt kết quả |

Nguồn cho bảng: [MainMenuScreenBehavior.Start](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuScreenBehavior.cs#L59), [MainMenuDockBehavior.Init](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuDockBehavior.cs#L74), [MainMenuStagesPageBehavior.OnDisable](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStagesPageBehavior.cs#L502), [MainMenuStageSelector.OnDestroy](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStageSelector.cs#L84), [MainMenuArmoryPageBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs#L180), [ArmoryDashboard.Show](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L126), [ArmoryDashboard.Hide](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L138), [ArmoryHeroPanel.Init](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L76), [HeroPreviewBehavior.Clear](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L97), [UpgradeDashboardPage.OnDestroy](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDashboardPage.cs#L123), [ForgePageBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Forge/ForgePageBehavior.cs#L205), [DailyQuestsPageBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Quests/DailyQuestsPageBehavior.cs#L245), [AchievementsPageBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Achievements/AchievementsPageBehavior.cs#L272), [LocalizedMenuScope.OnDisable](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LocalizedMenuScope.cs#L34), [MenuWalletView.OnDisable](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MenuWalletView.cs#L25), [CostButtonBehavior.SetCurrency](../../Assets/The%20Archer/Scripts/UI/CostButtonBehavior.cs#L106), [MainMenuSettingsPopup.OnDisable](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuSettingsPopup.cs#L101), [ExpandedArmoryPopup.OnCloseButtonClicked](../../Assets/The%20Archer/Scripts/UI/Armory/ExpandedArmoryPopup.cs#L149), [ExpandedUpgradePopup.Hide](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/ExpandedUpgradePopup.cs#L73), [LuckyRewardsPopup.OnDisable](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Lucky%20Rewards/LuckyRewardsPopup.cs#L171), [AbilitySelectorBehavior.CancelForResult](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs#L643).

Listener click lambda trên button con cùng tuổi thọ không được tự coi là leak chỉ vì không RemoveListener. Rủi ro chính là publisher sống lâu hơn subscriber, hoặc đăng ký nhiều lần không tháo. Serialized UnityEvent trong prefab cũng là một nguồn callback; audit này chưa khẳng định mọi persistent listener/Animator event trên mọi prefab đều sạch.

Ngoài UI: `HeroBehavior.OnDestroy → UnsubscribeFromPlayer` có tháo các listener player/stat trong source hiện tại; base `AbilityBehavior.OnDestroy` tháo event AbilityData; permanent upgrade Clear đang RemoveModifier tương ứng. Không lặp lại lỗi `+=`/thêm modifier ở Clear trong ghi chú cũ nếu source hiện tại đã khác. [HeroBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/Player/Hero/HeroBehavior.cs#L352), [AbilityBehavior.OnDestroy](../../Assets/The%20Archer/Scripts/Abilities/Behaviors/AbilityBehavior.cs#L47), [PowerUpgradeBehavior.Clear](../../Assets/The%20Archer/Scripts/Upgrades/Behaviors/PowerUpgradeBehavior.cs#L21).

## 6. Dock, input và pause là state dùng chung

- Menu screen giữ IsDockEnabled; page/popup như ExpandedArmoryPopup, Settings caller, LuckyRewardsPopup, LanguagePicker, DeveloperToolsPanel có quyền Disable/EnableDock.
- EventSystem.current selected object được nhiều page/card/popup thay khi focus/đóng/mở.
- `PauseScreenBehavior`, `AbilitySelectorBehavior`, HPChestUI, EvilChestUI, hai result screen và `EasingManager.DoTimeScale` đều ghi hoặc animate `Time.timeScale`.

Nguồn: [MainMenuScreenBehavior.DisableDock](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuScreenBehavior.cs#L100), [LanguagePicker.Open](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LanguagePicker.cs#L24), [DeveloperToolsPanel.Open](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/DeveloperToolsPanel.cs#L41), [PauseScreenBehavior.Show](../../Assets/The%20Archer/Scripts/UI/Pause/PauseScreenBehavior.cs#L129), [AbilitySelectorBehavior.ShowCoroutine](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs#L191), [HPChestUI.Show](../../Assets/The%20Archer/Scripts/UI/Chest/HPChestUI.cs#L45), [EvilChestUI](../../Assets/The%20Archer/Scripts/UI/Chest/EvilChestUI.cs#L13), [EasingManager.DoTimeScale](../../Assets/The%20Archer/Scripts/Toolkit/Easing/EasingManager.cs#L63).

Rủi ro có điều kiện: popup A đóng và restore 1/dock trong khi popup B vẫn mở. Chưa có bằng chứng một token/stack pause hoặc modal owner thống nhất cho tất cả đường đi. `GameScreen.PrepareForResult` đã có dọn selector/chest/pause để giảm một phần giao thoa; không bỏ qua cơ chế đang có. [GameScreenBehavior.PrepareForResult](../../Assets/The%20Archer/Scripts/UI/GameScreenBehavior.cs#L80).

## 7. Preview hero

`HeroPreviewBehavior.Awake` tạo RenderTexture **512×512, depth16, ARGBHalf**, gán vào preview camera và RawImage; nếu có AspectRatioFitter thì đặt aspect từ texture. `Show` instantiate model hero; `SetWeaponData` instantiate/gắn weapon; `Clear` tắt preview, hủy hero và tháo event. Chưa thấy OnDestroy Release/Destroy RenderTexture riêng trong class. [HeroPreviewBehavior.Awake](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L27), [HeroPreviewBehavior.Show](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L44), [HeroPreviewBehavior.Clear](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L97).

Đây là bằng chứng một giới hạn độ phân giải và ownership tài nguyên. Không thể kết luận chỉ riêng 512×512 gây toàn bộ hiện tượng nhòe trong ảnh: còn cần đối chiếu kích thước RawImage thực, Render Scale, AA, post-processing và camera khi Play. Không có điều chỉnh chất lượng nào được thực hiện trong audit.

## 8. Thứ tự lấy lại quyền chỉnh Inspector — chỉ đề xuất

1. Lập bảng target/property cho đúng prefab instance đang dùng: ai giữ layout, ai bind icon/text, ai animate.
2. Chốt chính sách với người phát triển: thuộc tính authoring giữ nguyên; thuộc tính responsive có profile rõ; animation dùng baseline được serialize; conditional view có cờ cho phép của tác giả.
3. Ưu tiên Start/difficulty/action sprite và inventory cell, vì hiện code chọn lại sprite/rect trực tiếp; cập nhật cấu hình đúng nguồn thay vì sửa một Image bị writer khác ghi lại.
4. Sau đó mới thống nhất Show/Hide/OnDisable/OnDestroy và pause/dock. Xác minh bằng chuyển tab, đổi ngôn ngữ, orientation và disable/re-enable có chủ đích.
5. Giữ clone/pool từ prefab cho danh sách động; không cần chuyển mọi thay đổi fillAmount/text data thành công việc thủ công.

Không có bước nào trong danh sách đã được triển khai.


## 9. Phụ lục kiểm kê writer UI trong source

Bảng tra bổ sung sau quét toàn bộ `Scripts/UI` (93 file) và hai view input/healthbar ngoài thư mục UI. **G**: geometry/transform/helper/easing; **F**: font; **V**: visibility/Canvas/raycast; **S**: sprite. Có 79 file khớp các mẫu ghi này. File không có trong bảng có thể chỉ bind text, màu/alpha, nhận input hoặc gọi writer ở class khác; bảng không thay thế phân tích caller.

Loại bỏ thư mục Editor, comment dòng và nhánh `#if UNITY_EDITOR` khi quét. Các method được liệt kê là điểm tra source (gồm helper), không xác nhận tất cả đang được gọi trong scene. Một số lệnh G chỉ thay scale animation hoặc biến geometry tạm; phải xem target ở dòng được dẫn. Những helper chưa thấy caller như ResizeText/ResizeRows/ScaleText/ResizeLayout được phân biệt ở mục 3.

| File | Nhóm ghi | Method cần tra |
| --- | --- | --- |
| [Player/HealthbarBehavior.cs](../../Assets/The%20Archer/Scripts/Player/HealthbarBehavior.cs#L200) | G | `Redraw` |
| [Toolkit/Input/JoystickBehavior.cs](../../Assets/The%20Archer/Scripts/Toolkit/Input/JoystickBehavior.cs#L60) | G | `Awake`, `OnPointerDown`, `OnDrag`, `ResetJoystick` |
| [UI/Abilities/AbilityCardBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs#L121) | G, V, S | `SetData`, `ShowHidden`, `Init`, `ChangeAbilityCoroutine`, `OnCardBackScaledToZero`, `Disable`, `ResetUI` |
| [UI/Abilities/AbilitySelectorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs#L127) | G, V | `Open`, `Show`, `OnXpLevelChanged`, `ShowCoroutine`, `CompleteShowCoroutine`, `OnSkipButtonClicked`, `OnRerollButtonClicked`, `InitRarityData`, `DoRarityFlash`, `OnRarityFlashScaleFinished`, `Hide`, `OnBackgroundHidden`, `ResetUI`, `CancelForResult` |
| [UI/Abilities/AbilitySelectorSlotBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs#L92) | G, V | `StopScrolling`, `StartScrolling`, `ScrollingCoroutine`, `DoFlash`, `OnFlashInAminationEnded` |
| [UI/AnimatedTitleUI.cs](../../Assets/The%20Archer/Scripts/UI/AnimatedTitleUI.cs#L69) | G | `Show`, `ShowAnimatedLine`, `Hide`, `Skip`, `ResetUI` |
| [UI/Armory/ArmoryAbilityCard.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryAbilityCard.cs#L29) | V, S | `Init` |
| [UI/Armory/ArmoryCardBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryCardBehavior.cs#L51) | S | `InitRarityVisuals` |
| [UI/Armory/ArmoryDashboard.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryDashboard.cs#L128) | G, V, S | `Show`, `Refresh`, `RenderCells`, `CreateModal`, `CloseModal`, `Place` |
| [UI/Armory/ArmoryGridUI.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryGridUI.cs#L94) | G, V | `MoveLeft`, `MoveRight`, `MoveCenter`, `SpawnCenter`, `SpawnLeft`, `SpawnRight`, `OnMoveFinished`, `Clear` |
| [UI/Armory/ArmoryHeroPanel.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs#L177) | G, V | `RecalculateStats`, `InitSlots`, `SetSlotsScale`, `ClearSlots` |
| [UI/Armory/ArmoryTabBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs#L75) | G | `Select`, `Deselect`, `OnSelectFinished`, `OnDeselectFinished` |
| [UI/Armory/ArmoryTabsPanel.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs#L27) | V | `Start`, `OnInputChanged`, `OnShouldersPerformed` |
| [UI/Armory/ExpandedArmoryPopup.cs](../../Assets/The%20Archer/Scripts/UI/Armory/ExpandedArmoryPopup.cs#L165) | G, V | `HideCoroutine`, `ShowCoroutine` |
| [UI/Armory/Hero/ExpandedHeroPopup.cs](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs#L21) | G, V, S | `Show`, `HideCoroutine`, `InitVisuals`, `InitButtonsUpgradable`, `InitButtonsMaxLevel`, `InitButtonsNotBought`, `InitAbilities` |
| [UI/Armory/Hero/HeroCardBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs#L36) | V, S | `SetData`, `OnHeroSelected`, `InitRarityVisuals` |
| [UI/Armory/Hero/HeroPreviewBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs#L41) | V | `Awake`, `Show`, `Clear` |
| [UI/Armory/Items/ExpandedItemPopup.cs](../../Assets/The%20Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs#L22) | G, V, S | `Show`, `HideCoroutine`, `InitVisuals`, `InitButtonsUpgradable`, `InitButtonsMaxLevel`, `InitAbilities` |
| [UI/Armory/Items/ItemCardBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Armory/Items/ItemCardBehavior.cs#L45) | S | `SetData` |
| [UI/Armory/MainMenuArmoryPageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs#L65) | V | `OpenHeroesTab`, `MoveCenter` |
| [UI/Armory/SelectableGamepadIndicator.cs](../../Assets/The%20Archer/Scripts/UI/Armory/SelectableGamepadIndicator.cs#L15) | V | `OnInputChanged`, `OnSelect`, `OnDeselect`, `OnEnable`, `OnDisable` |
| [UI/Armory/StatIndicator.cs](../../Assets/The%20Archer/Scripts/UI/Armory/StatIndicator.cs#L28) | G, S | `Awake`, `Init`, `UpdateSize` |
| [UI/Armory/StatsGrid.cs](../../Assets/The%20Archer/Scripts/UI/Armory/StatsGrid.cs#L86) | G, V | `RecalculateSize`, `Clear` |
| [UI/BossfightProgressbarBehavior.cs](../../Assets/The%20Archer/Scripts/UI/BossfightProgressbarBehavior.cs#L21) | V | `Show`, `Hide` |
| [UI/Chest/EvilChestUI.cs](../../Assets/The%20Archer/Scripts/UI/Chest/EvilChestUI.cs#L80) | G, V | `Show`, `Hide`, `ResetUI`, `OnBackgroundHidden` |
| [UI/Chest/HPChestUI.cs](../../Assets/The%20Archer/Scripts/UI/Chest/HPChestUI.cs#L57) | V | `Show`, `ResetUI`, `OnBackgroundHidden` |
| [UI/CostButtonBehavior.cs](../../Assets/The%20Archer/Scripts/UI/CostButtonBehavior.cs#L134) | S | `RecalculateVisuals`, `DisableButton` |
| [UI/CurrencyScreenIncicatorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/CurrencyScreenIncicatorBehavior.cs#L29) | S | `Start` |
| [UI/GameScreenBehavior.cs](../../Assets/The%20Archer/Scripts/UI/GameScreenBehavior.cs#L60) | G, V | `Awake`, `HideSideUI`, `PrepareForResult`, `ShowSideUI`, `FadeInOut`, `FadeOut` |
| [UI/HorizontalSelectable.cs](../../Assets/The%20Archer/Scripts/UI/HorizontalSelectable.cs#L24) | G, V | `MoveCenter`, `MoveLeft`, `MoveRight`, `OnMoveAwayFinished` |
| [UI/HorizontalSelector.cs](../../Assets/The%20Archer/Scripts/UI/HorizontalSelector.cs#L93) | G, V | `InitButtons`, `OnLeftButtonClicked`, `OnRightButtonClicked` |
| [UI/Main Menu/Achievements/AchievementRowFocus.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Achievements/AchievementRowFocus.cs#L27) | G | `OnSelect` |
| [UI/Main Menu/Achievements/AchievementsPageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Achievements/AchievementsPageBehavior.cs#L115) | G, F, V, S | `MoveCenter`, `DisableInteraction`, `RefreshVisible`, `BindCategory`, `RefreshRow`, `ResizeLayout`, `Fill` |
| [UI/Main Menu/ContinuePlayingPopup.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/ContinuePlayingPopup.cs#L32) | G, V | `Show`, `Hide`, `OnPopupHidden` |
| [UI/Main Menu/DeveloperToolsPanel.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/DeveloperToolsPanel.cs#L38) | V, S | `Availability`, `Open`, `Refresh`, `Close` |
| [UI/Main Menu/Forge/ForgePageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Forge/ForgePageBehavior.cs#L75) | G, F, V, S | `MoveCenter`, `DisableInteraction`, `Refresh`, `RefreshStats`, `ResizeText` |
| [UI/Main Menu/LanguagePicker.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LanguagePicker.cs#L27) | V | `Open`, `Close` |
| [UI/Main Menu/LocalizedMenuScope.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/LocalizedMenuScope.cs#L32) | F | `Apply` |
| [UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Lucky%20Rewards/LuckyRewardsPopup.cs#L42) | G, V, S | `Show`, `BindPrizes`, `Animate`, `DisplayReceipt`, `Continue`, `Hide` |
| [UI/Main Menu/Lucky Rewards/LuckyWheelSlice.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Lucky%20Rewards/LuckyWheelSlice.cs#L13) | G, V, S | `Bind` |
| [UI/Main Menu/MainMenuDockBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuDockBehavior.cs#L127) | V | `CheckGamepadIndicators`, `OnInputChanged` |
| [UI/Main Menu/MainMenuDockItemBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuDockItemBehavior.cs#L41) | G, F, S | `Configure`, `Select`, `Deselect` |
| [UI/Main Menu/MainMenuInfoPopup.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuInfoPopup.cs#L45) | G, V | `Show`, `Hide` |
| [UI/Main Menu/MainMenuPageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuPageBehavior.cs#L25) | G, V | `PrepareInitialState`, `MoveLeft`, `MoveRight`, `MoveCenter`, `SpawnLeft`, `SpawnRight`, `OnMoveFinished` |
| [UI/Main Menu/MainMenuSettingsPopup.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuSettingsPopup.cs#L38) | G, V | `Show`, `Hide` |
| [UI/Main Menu/MainMenuStageIcon.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStageIcon.cs#L13) | V, S | `SetIcon` |
| [UI/Main Menu/MainMenuStageSelector.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStageSelector.cs#L22) | V | `Init` |
| [UI/Main Menu/MainMenuStagesPageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MainMenuStagesPageBehavior.cs#L225) | G, V, S | `InitAttemptsText`, `InitInfoButton`, `InitButtonsWithPurchaseButton`, `InitButtonsWithoutPurchaseButton`, `OnMoveCenterFinished`, `OnPurchaseButtonClicked`, `OnInputChanged` |
| [UI/Main Menu/MenuEconomyPanel.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MenuEconomyPanel.cs#L47) | V | `RefreshTimer`, `Open`, `RefreshLimit`, `Close` |
| [UI/Main Menu/MenuWalletView.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/MenuWalletView.cs#L21) | S | `Bind` |
| [UI/Main Menu/Quests/DailyQuestsPageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Quests/DailyQuestsPageBehavior.cs#L96) | G, F, V, S | `ResizeRows`, `ScaleText`, `MoveCenter`, `DisableInteraction`, `Refresh`, `Fill` |
| [UI/Main Menu/Upgrades/ExpandedUpgradePopup.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/ExpandedUpgradePopup.cs#L51) | G, V | `Show`, `HideCoroutine`, `ShowCoroutine` |
| [UI/Main Menu/Upgrades/MainMenuUpgradePageBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/MainMenuUpgradePageBehavior.cs#L133) | G, V | `OnCardSelected`, `OnMoveFinished` |
| [UI/Main Menu/Upgrades/UpgradeCardBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeCardBehavior.cs#L88) | V, S | `OnInputChanged`, `RedrawVisuals`, `InitGamepadIndicator`, `RedrawButton`, `Clear` |
| [UI/Main Menu/Upgrades/UpgradeCardSelectable.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeCardSelectable.cs#L38) | G, V | `MoveCenter`, `MoveAway`, `Clear` |
| [UI/Main Menu/Upgrades/UpgradeCardsSelector.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeCardsSelector.cs#L34) | G | `Show`, `Hide` |
| [UI/Main Menu/Upgrades/UpgradeDashboardPage.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDashboardPage.cs#L61) | V, S | `MoveCenter`, `Leave`, `Refresh` |
| [UI/Main Menu/Upgrades/UpgradeDescriptionBlock.cs](../../Assets/The%20Archer/Scripts/UI/Main%20Menu/Upgrades/UpgradeDescriptionBlock.cs#L39) | G, V | `OnUpgradeLevelChanged`, `RecalculateBackgroundRect`, `Show`, `Hide` |
| [UI/Pause/AbilitiesGridBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Pause/AbilitiesGridBehavior.cs#L29) | G | `Show` |
| [UI/Pause/AbilitiesRowBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Pause/AbilitiesRowBehavior.cs#L30) | G | `Show` |
| [UI/Pause/MiniAbilityCardBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Pause/MiniAbilityCardBehavior.cs#L13) | S | `SetData` |
| [UI/Pause/PauseScreenBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Pause/PauseScreenBehavior.cs#L133) | G, V | `Show`, `OnResumeButtonClicked`, `OnQuitButtonClicked` |
| [UI/ResponsiveMenuLayout.cs](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuLayout.cs#L16) | G, F, V | `Apply` |
| [UI/ResponsiveMenuSurface.cs](../../Assets/The%20Archer/Scripts/UI/ResponsiveMenuSurface.cs#L32) | G | `Apply` |
| [UI/SafeAreaManager.cs](../../Assets/The%20Archer/Scripts/UI/SafeAreaManager.cs#L46) | G | `ApplySafeArea` |
| [UI/ScalableAnimatedTitleUI.cs](../../Assets/The%20Archer/Scripts/UI/ScalableAnimatedTitleUI.cs#L62) | G | `RecalculateSizesAndScales` |
| [UI/ScalingLabelBehavior.cs](../../Assets/The%20Archer/Scripts/UI/ScalingLabelBehavior.cs#L38) | G, S | `SetSprite`, `RecalculatePositions` |
| [UI/SmallItemRewardCard.cs](../../Assets/The%20Archer/Scripts/UI/SmallItemRewardCard.cs#L15) | V, S | `SetReward`, `SetItem` |
| [UI/SmallRewardCard.cs](../../Assets/The%20Archer/Scripts/UI/SmallRewardCard.cs#L15) | G, V, S | `SetIcon`, `SetAmount`, `Show` |
| [UI/StageCompleteScreen.cs](../../Assets/The%20Archer/Scripts/UI/StageCompleteScreen.cs#L74) | G, V | `CreateCurrencyRewardCards`, `Show` |
| [UI/StageFailedScreen.cs](../../Assets/The%20Archer/Scripts/UI/StageFailedScreen.cs#L75) | G, V | `CreateCurrencyRewardCards`, `Show` |
| [UI/StageObjectiveUI.cs](../../Assets/The%20Archer/Scripts/UI/StageObjectiveUI.cs#L39) | G, V | `Show`, `Hide` |
| [UI/Text Indicators/AbilitySelectedIndicatorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Text%20Indicators/AbilitySelectedIndicatorBehavior.cs#L38) | G | `SetAbilityType` |
| [UI/Text Indicators/ComboTextIndicatorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Text%20Indicators/ComboTextIndicatorBehavior.cs#L38) | G, V | `SetText`, `Update` |
| [UI/Text Indicators/ItemTextIndicatorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Text%20Indicators/ItemTextIndicatorBehavior.cs#L54) | G, S | `SetReward` |
| [UI/Text Indicators/TextIndicatorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/Text%20Indicators/TextIndicatorBehavior.cs#L80) | G, V | `SetAnchors`, `SetPosition`, `SetScale`, `Update` |
| [UI/ToggleBehavior.cs](../../Assets/The%20Archer/Scripts/UI/ToggleBehavior.cs#L29) | S | `SetToggle` |
| [UI/WaveIndicatorBehavior.cs](../../Assets/The%20Archer/Scripts/UI/WaveIndicatorBehavior.cs#L36) | G, S | `SetSprite`, `ShowNumber`, `HideNumber`, `Grow` |
| [UI/WavePanelBehavior.cs](../../Assets/The%20Archer/Scripts/UI/WavePanelBehavior.cs#L21) | G, V | `Awake`, `AnimationCoroutine` |

### 9.1. UI asset demo ngoài Scripts

| File/class.method | Khả năng ghi | Phạm vi reference đã thấy |
| --- | --- | --- |
| [PanelControl.Start / Click_Prev / Click_Next / SetMode](../../Assets/The%20Archer/Layer%20Lab/Scripts/PanelControl.cs#L41) | Bật/tắt panel, nút mũi tên, thay tiêu đề | DemoScene của Layer Lab |
| [PanelView.OnEnable / OnDisable](../../Assets/The%20Archer/Layer%20Lab/Scripts/PanelView.cs#L11) | SetActive các otherPanels | DemoScene của Layer Lab |
| [UIParticleSystem.OnPopulateMesh / Update / LateUpdate](../../Assets/The%20Archer/Layer%20Lab/GUI%20Pro-CasualGame/Extensions/UIParticle/UIParticleSystem.cs#L134) | Mesh particle và cập nhật render trong Canvas; không phải bố trí nút/menu game | DemoScene của Layer Lab |

Không đưa các component demo này vào danh sách nguyên nhân của menu game nếu chưa tìm thấy target/caller tương ứng.
