# Kiểm kê prefab và khả năng chỉnh Inspector

Tạo lúc 2026-10-29 15:57. Chỉ đọc asset, không áp lại giao diện hoặc đổi save.

- Prefab đã đọc: **961**; prefab có RectTransform: **427**.
- Component script bị thiếu: **0**.
- Source C# đã quét: **676**; dấu hiệu dựng UI runtime cần xem lại: **0**.

Quét source là kiểm tra tĩnh, không chứng minh mọi callback hay mọi thiết bị hoạt động. Các hàm authoring trong #if UNITY_EDITOR không được tính là dựng UI runtime. Danh sách động, số tiến trình, nội dung bản dịch và hiệu ứng vẫn cần script binding.

## Dấu hiệu dựng UI runtime

Không thấy trong các mẫu API UI được kiểm tra.

## Ghi trạng thái hiển thị runtime cần đối chiếu

Có 192 dòng. Đây là danh sách cần phân loại, không đồng nghĩa mọi dòng là lỗi: popup/page/pool có vòng đời mở đóng; nội dung/progress có binding. Decoration phải qua InspectorVisibility; component tắt không được tự bật lại.

- `Assets/The Archer/Scripts/UI/BackgroundTintUI.cs:20`: `image.enabled = true;`
- `Assets/The Archer/Scripts/UI/BackgroundTintUI.cs:40`: `image.enabled = false;`
- `Assets/The Archer/Scripts/UI/BackgroundTintUI.cs:44`: `alphaCoroutine = image.DoAlpha(0, duration).SetUnscaledTime(true).SetOnFinish(() => image.enabled = false);`
- `Assets/The Archer/Scripts/UI/BossfightProgressbarBehavior.cs:21`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/BossfightProgressbarBehavior.cs:30`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/CompactRunResult.cs:29`: `if(shown)return;shown=true;gameObject.SetActive(true);var s=StageController.RunStatistics;if(s==null)return;`
- `Assets/The Archer/Scripts/UI/CompactRunResult.cs:49`: `private void Add(Sprite icon,string name,int amount){if(amount<=0)return;var row=Instantiate(rewardPrefab,rewards);row.gameObject.SetActive(true);row.Bind(icon,name,amount);}`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:87`: `blackFadeImage.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:89`: `pauseWindow.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:90`: `evilChestUI.gameObject.SetActive(false); hpChestUI.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:140`: `blackFadeImage.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:152`: `blackFadeImage.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:161`: `blackFadeImage.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:163`: `transitionFade = blackFadeImage.DoAlpha(0, 0.2f).SetUnscaledTime(true).SetOnFinish(() => blackFadeImage.gameObject.SetActive(false));`
- `Assets/The Archer/Scripts/UI/HorizontalSelectable.cs:41`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/InspectorVisibility.cs:37`: `if (target.activeSelf != visible) target.SetActive(visible);`
- `Assets/The Archer/Scripts/UI/InspectorVisibility.cs:50`: `if (target.enabled != visible) target.enabled = visible;`
- `Assets/The Archer/Scripts/UI/ResponsiveMenuLayout.cs:60`: `foreach(var style in visibility)if(style.target!=null)style.target.SetActive(portrait?style.portrait:style.landscape);`
- `Assets/The Archer/Scripts/UI/RunSeasonView.cs:24`: `private void Update(){if(StageController.IsResultFinalized){badge.gameObject.SetActive(false);information.SetActive(false);return;}if(!initialized){Refresh();return;}if(introUntil>0&&Time.unscaledTime>=introUntil){introUntil=0;information.SetActive(false);}}`
- `Assets/The Archer/Scripts/UI/RunSeasonView.cs:32`: `initialized=true;if(season==null){badge.gameObject.SetActive(false);information.SetActive(false);return;}`
- `Assets/The Archer/Scripts/UI/RunSeasonView.cs:36`: `if(autoShowIntroduction){information.SetActive(true);introUntil=Time.unscaledTime+introductionDuration;}`
- `Assets/The Archer/Scripts/UI/RunSeasonView.cs:38`: `private void Toggle(){if(!isActiveAndEnabled)return;introUntil=0;information.SetActive(!information.activeSelf);}`
- `Assets/The Archer/Scripts/UI/RunSeasonView.cs:39`: `private void CloseInformation(){information.SetActive(false);}`
- `Assets/The Archer/Scripts/UI/RunSummaryPresenter.cs:31`: `details.SetActive(true);`
- `Assets/The Archer/Scripts/UI/RunSummaryPresenter.cs:79`: `{ details.SetActive(false); if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(openButton.gameObject); }`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:89`: `newCard.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:94`: `coinsRewardCard.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:102`: `StageController.FinalizeRunRewards(true);gameObject.SetActive(true);canvas.enabled=true;`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:106`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:118`: `continueButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:134`: `card.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:147`: `card.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:90`: `newCard.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:95`: `coinsRewardCard.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:103`: `StageController.FinalizeRunRewards();gameObject.SetActive(true);canvas.enabled=true;`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:107`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:119`: `canvas.enabled = true;`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:120`: `continueButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:136`: `card.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:149`: `card.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/StageObjectiveUI.cs:39`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/StageObjectiveUI.cs:59`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:105`: `if (prevWaveIndicator != null) prevWaveIndicator.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:106`: `currentWaveIndicator.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:107`: `nextWaveIndicator.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:108`: `if (nextNextWaveIndicator != null) nextNextWaveIndicator.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:174`: `button.enabled = false;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:239`: `button.enabled = false;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:256`: `cardBackRect.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:257`: `cardFrontRect.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:266`: `cardBackRect.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:267`: `cardFrontRect.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:286`: `cardBackRect.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:287`: `cardFrontRect.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:302`: `button.enabled = false;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:312`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:319`: `cardBackRect.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:320`: `cardFrontRect.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:330`: `button.enabled = false;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:332`: `upgradeIndicator.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:138`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:152`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:168`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:256`: `skipButton.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:295`: `skipButton.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:610`: `slot.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:615`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:624`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:637`: `skipButton.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:660`: `{ slot.MainCard.onAbilitySelected -= OnAbilitySelected; slot.gameObject.SetActive(false); }`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:663`: `onClosed = null; IsOpen = false; gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:102`: `cards[i].Card.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:205`: `card.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryAbilityCard.cs:33`: `lockImage.SetActive(isLocked);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:178`: `Wire(); gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:274`: `for (int i = poolSize; i < cells.Count; i++) cells[i].button.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:303`: `cell.button.gameObject.SetActive(index < total); if (index >= total) continue;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:368`: `modal.gameObject.SetActive(true); modal.SetAsLastSibling(); modalTitle.text = title;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:403`: `expansionModal = false; modal.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:140`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:152`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:164`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:170`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:179`: `card.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:358`: `card.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:54`: `bottomBackgroundImage.enabled = false;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:56`: `button.enabled = !IsSelected;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:73`: `bottomBackgroundImage.enabled = true;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:80`: `button.enabled = false;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:97`: `bottomBackgroundImage.enabled = true;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:104`: `button.enabled = true;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:123`: `bottomBackgroundImage.enabled = false;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:131`: `bottomBackgroundImage.enabled = false;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:27`: `leftGamepadIndicator.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:28`: `rightGamepadIndicator.SetActive(GameController.InputManager.ActiveInput == Input.InputType.Gamepad);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:48`: `leftGamepadIndicator.SetActive(!itemsTab.IsSelected);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:49`: `rightGamepadIndicator.SetActive(!heroesTab.IsSelected);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:53`: `leftGamepadIndicator.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:54`: `rightGamepadIndicator.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:103`: `leftGamepadIndicator.SetActive(!itemsTab.IsSelected);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabsPanel.cs:104`: `rightGamepadIndicator.SetActive(!heroesTab.IsSelected);`
- `Assets/The Archer/Scripts/UI/Armory/ExpandedArmoryPopup.cs:185`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs:65`: `heroesGrid.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs:76`: `tabsPanel.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs:77`: `itemsGrid.gameObject.SetActive(false); heroesGrid.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs:91`: `itemsGrid.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/MainMenuArmoryPageBehavior.cs:92`: `heroesGrid.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/SelectableGamepadIndicator.cs:15`: `gamepadIndicator.SetActive(newInput == InputType.Gamepad && IsSelected);`
- `Assets/The Archer/Scripts/UI/Armory/SelectableGamepadIndicator.cs:23`: `gamepadIndicator.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/SelectableGamepadIndicator.cs:33`: `gamepadIndicator.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/SelectableGamepadIndicator.cs:53`: `gamepadIndicator.SetActive(isSelected && GameController.InputManager.ActiveInput == InputType.Gamepad);`
- `Assets/The Archer/Scripts/UI/Armory/SelectableGamepadIndicator.cs:60`: `gamepadIndicator.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/StatsGrid.cs:211`: `indicators[i].gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:138`: `acceptButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:139`: `rejectButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:144`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:156`: `acceptButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:157`: `rejectButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:169`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Chest/HPChestUI.cs:59`: `firstAbilityCard.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Chest/HPChestUI.cs:65`: `secondAbilityCard.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Chest/HPChestUI.cs:107`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Chest/HPChestUI.cs:116`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/ContinuePlayingPopup.cs:42`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/ContinuePlayingPopup.cs:55`: `yesButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/ContinuePlayingPopup.cs:56`: `noButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/ContinuePlayingPopup.cs:65`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/DeveloperToolsPanel.cs:49`: `panel.SetActive(true); panel.transform.SetAsLastSibling(); ownsDock = true; screen.DisableDock();`
- `Assets/The Archer/Scripts/UI/Main Menu/DeveloperToolsPanel.cs:79`: `panel.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/LanguagePicker.cs:30`: `previous=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;ownsDock=true;menu.DisableDock();panel.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/LanguagePicker.cs:37`: `if(panel!=null)panel.SetActive(false);if(!ownsDock)return;ownsDock=false;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuInfoPopup.cs:47`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuInfoPopup.cs:111`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuPageBehavior.cs:25`: `canvas.enabled = visible;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuPageBehavior.cs:91`: `canvas.enabled = true;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuPageBehavior.cs:102`: `canvas.enabled = true;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuPageBehavior.cs:108`: `canvas.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:49`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:57`: `closeButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:58`: `quitButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:72`: `closeButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:73`: `quitButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:81`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuStageSelector.cs:22`: `if (editorStagePreview) editorStagePreview.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/MenuEconomyPanel.cs:86`: `modal.SetActive(true); feedback.text = "";`
- `Assets/The Archer/Scripts/UI/Main Menu/MenuEconomyPanel.cs:110`: `if (modal != null) modal.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:138`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:140`: `quitButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:141`: `resumeButton.enabled = false;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:225`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:238`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Text Indicators/ComboTextIndicatorBehavior.cs:45`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Text Indicators/TextIndicatorBehavior.cs:98`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:21`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:40`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:103`: `upgradeButtonLabel.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:104`: `upgradeButtonText.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:105`: `maxLvlButtonText.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:115`: `upgradeButtonLabel.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:116`: `upgradeButtonText.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:117`: `maxLvlButtonText.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs:46`: `selectedCheckmark.SetActive(showCheckmark && saveData.IsEquipped);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs:54`: `selectedCheckmark.SetActive(showCheckmark && saveData.IsEquipped);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs:68`: `lockedStateObject.gameObject.SetActive(!SaveData.IsUnlocked);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs:69`: `rarityBackgroundImage.gameObject.SetActive(SaveData.IsUnlocked);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs:45`: `heroPreview.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs:55`: `heroPreview.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroPreviewBehavior.cs:92`: `if (heroPreview != null) heroPreview.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:22`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:41`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:99`: `upgradeButtonLabel.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:100`: `upgradeButtonText.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:101`: `maxLvlButtonText.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:141`: `upgradeButtonLabel.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:142`: `upgradeButtonText.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:143`: `maxLvlButtonText.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:192`: `row.lastProgress = -1; row.lastState = -1; row.rect.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:197`: `for (; index < rowPool.Count; index++) { rowPool[index].definition = null; rowPool[index].rect.gameObject.SetActive(false); }`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:220`: `rect.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:61`: `gameObject.SetActive(true); transform.SetAsLastSibling();`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:66`: `heading.text = table.title; resultPanel.SetActive(false); animating = false;`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:84`: `var row = prizePool[count]; row.gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:98`: `slicePool[count].gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:103`: `for (int i = count; i < prizePool.Count; i++) prizePool[i].gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:104`: `for (int i = count; i < slicePool.Count; i++) slicePool[i].gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:176`: `receiptSequence = receipt.sequence; resultPanel.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:185`: `resultPanel.SetActive(false); Refresh(); Focus(purchaseButton.interactable ? purchaseButton : closeButton);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:187`: `public void Hide() { gameObject.SetActive(false); }`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/ExpandedUpgradePopup.cs:54`: `gameObject.SetActive(true);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/ExpandedUpgradePopup.cs:102`: `gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/MainMenuUpgradePageBehavior.cs:154`: `upgradeCard.gameObject.SetActive(false);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardSelectable.cs:97`: `gameObject.SetActive(false);`

## Ghi trình bày runtime cần đối chiếu

Có 210 dòng. Kiểm tra mốc Inspector của animation, tùy chọn Use State Colors/Auto Fit, bố cục responsive đã author và nội dung từ database. Kiểm kê construction bằng 0 không chứng minh Inspector được tôn trọng.

- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:105`: `titleShineRect.anchoredPosition = titleLinePosition;`
- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:136`: `titleRect.localScale = animationDefaults.Scale(titleRect);`
- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:138`: `titleGradientRect.localScale = animationDefaults.Scale(titleGradientRect);`
- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:140`: `titleShineRect.anchoredPosition = titleLinePosition;`
- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:145`: `titleRect.localScale = animationDefaults.Scale(titleRect, new Vector3(0, 1, 1));`
- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:146`: `titleGradientRect.localScale = animationDefaults.Scale(titleGradientRect, new Vector3(0, 1, 1));`
- `Assets/The Archer/Scripts/UI/AnimatedTitleUI.cs:148`: `titleShineRect.anchoredPosition = titleLinePosition;`
- `Assets/The Archer/Scripts/UI/CurrencyScreenIncicatorBehavior.cs:29`: `icon.sprite = Currency.Data.Icon;`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:63`: `pauseButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/GameScreenBehavior.cs:64`: `goldIndicatorRect.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/HeroExperienceBar.cs:30`: `portrait.sprite=hero.Icon;`
- `Assets/The Archer/Scripts/UI/HeroExperienceBar.cs:34`: `portrait.rectTransform.anchorMin=new Vector2(b.x,b.y);portrait.rectTransform.anchorMax=new Vector2(b.z,b.w);`
- `Assets/The Archer/Scripts/UI/HorizontalSelector.cs:111`: `CurrentSelectable.RectTransform.anchoredPosition = new Vector2(-700, 0);`
- `Assets/The Archer/Scripts/UI/HorizontalSelector.cs:130`: `CurrentSelectable.RectTransform.anchoredPosition = new Vector2(700, 0);`
- `Assets/The Archer/Scripts/UI/LoadingScreenPresenter.cs:46`: `progressCursor.anchorMin=progressCursor.anchorMax=new Vector2(value,.5f);`
- `Assets/The Archer/Scripts/UI/LoadingScreenPresenter.cs:47`: `progressCursor.anchoredPosition=Vector2.zero;`
- `Assets/The Archer/Scripts/UI/LoadingScreenPresenter.cs:60`: `safeContent.anchorMin=new Vector2(previousSafeArea.xMin/Screen.width,previousSafeArea.yMin/Screen.height);`
- `Assets/The Archer/Scripts/UI/LoadingScreenPresenter.cs:61`: `safeContent.anchorMax=new Vector2(previousSafeArea.xMax/Screen.width,previousSafeArea.yMax/Screen.height);`
- `Assets/The Archer/Scripts/UI/ResponsiveMenuLayout.cs:16`: `public void Apply(RectTransform r){r.pivot=pivot;r.anchorMin=min;r.anchorMax=max;r.offsetMin=offsetMin;r.offsetMax=offsetMax;r.localScale=scale;}`
- `Assets/The Archer/Scripts/UI/ResponsiveMenuLayout.cs:58`: `foreach(var style in texts)if(style.target!=null){float size=portrait?style.portrait:style.landscape;float minimum=portrait?style.portraitMinimum:style.landscapeMinimum;style.target.fontSize=style.target.fontSizeMax=size;style.target.fontSizeMin=minimum>0?minimum:size*.6f;style.target.enableAutoSizing=style.autoSize;}`
- `Assets/The Archer/Scripts/UI/ResponsiveMenuLayout.cs:62`: `foreach(var style in images)if(style.target!=null)style.target.pixelsPerUnitMultiplier=portrait?style.portrait:style.landscape;`
- `Assets/The Archer/Scripts/UI/ResponsiveMenuSurface.cs:32`: `root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.sizeDelta=design;root.anchoredPosition=center;`
- `Assets/The Archer/Scripts/UI/ResponsiveMenuSurface.cs:33`: `root.localScale=Vector3.one*Mathf.Min(safeSize.x/design.x,safeSize.y/design.y);`
- `Assets/The Archer/Scripts/UI/ResultRewardView.cs:14`: `{icon.sprite=sprite;inspectorVisibility.SetVisible(icon,sprite!=null);title.text=MenuLanguage.Text(name);amount.text="× "+count.ToString("#,0");}`
- `Assets/The Archer/Scripts/UI/RunRecordHighlights.cs:30`: `timeBadge.color = dpsBadge.color = recordColor;`
- `Assets/The Archer/Scripts/UI/RunSeasonView.cs:34`: `icon.sprite=season.icon;visibility.SetVisible(icon,season.icon!=null);background.color=season.color;`
- `Assets/The Archer/Scripts/UI/SafeAreaManager.cs:46`: `screens[i].anchorMin = anchorMin;`
- `Assets/The Archer/Scripts/UI/SafeAreaManager.cs:47`: `screens[i].anchorMax = anchorMax;`
- `Assets/The Archer/Scripts/UI/ScalableAnimatedTitleUI.cs:62`: `scalable.anchoredPosition = initialPosition + Vector2.right * difference / 2;`
- `Assets/The Archer/Scripts/UI/ScalableAnimatedTitleUI.cs:66`: `scalable.anchoredPosition = initialPosition + Vector2.left * difference / 2;`
- `Assets/The Archer/Scripts/UI/ScalableAnimatedTitleUI.cs:75`: `scalable.sizeDelta = initialSize + Vector2.right * difference;`
- `Assets/The Archer/Scripts/UI/ScalingLabelBehavior.cs:39`: `icon.sprite = sprite;`
- `Assets/The Archer/Scripts/UI/SmallItemRewardCard.cs:41`: `itemTypeBackgroundImage.color = rarityData.SecondaryColor;`
- `Assets/The Archer/Scripts/UI/SmallItemRewardCard.cs:42`: `itemTypeIconImage.sprite = itemTypeData.ItemTypeIcon;`
- `Assets/The Archer/Scripts/UI/SmallItemRewardCard.cs:43`: `rarityBackgroundImage.sprite = rarityData.NarrowBorderBackground;`
- `Assets/The Archer/Scripts/UI/SmallRewardCard.cs:25`: `iconImage.sprite = icon;`
- `Assets/The Archer/Scripts/UI/SmallRewardCard.cs:37`: `transform.localScale = animationDefaults.Scale(transform, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:87`: `newCard.transform.localScale = animationDefaults.Scale(coinsRewardCard.transform);`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:123`: `titleRect.localScale = animationDefaults.Scale(titleRect, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:129`: `rewardsTitle.localScale = animationDefaults.Scale(rewardsTitle, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/StageCompleteScreen.cs:174`: `buttonRect.localScale = animationDefaults.Scale(buttonRect, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:88`: `newCard.transform.localScale = animationDefaults.Scale(coinsRewardCard.transform);`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:125`: `titleRect.localScale = animationDefaults.Scale(titleRect, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:131`: `rewardsTitle.localScale = animationDefaults.Scale(rewardsTitle, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/StageFailedScreen.cs:176`: `buttonRect.localScale = animationDefaults.Scale(buttonRect, new Vector3(0, 0.9f, 1f));`
- `Assets/The Archer/Scripts/UI/ToggleBehavior.cs:29`: `toggleImage.sprite = IsOn ? onSprite : offSprite;`
- `Assets/The Archer/Scripts/UI/WaveIndicatorBehavior.cs:38`: `waveIconImage.sprite = iconSprite;`
- `Assets/The Archer/Scripts/UI/WaveIndicatorBehavior.cs:51`: `numberRect.localScale = animationDefaults.Scale(numberRect);`
- `Assets/The Archer/Scripts/UI/WaveIndicatorBehavior.cs:63`: `numberRect.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:23`: `transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:33`: `transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:46`: `currentWaveIndicator.RectTransform.anchoredPosition = Vector2.up * 5;`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:47`: `currentWaveIndicator.transform.localScale = animationDefaults.Scale(currentWaveIndicator.transform) * 1.3f;`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:56`: `prevWaveIndicator.RectTransform.anchoredPosition = new Vector2(-100, 5);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:57`: `prevWaveIndicator.transform.localScale = animationDefaults.Scale(prevWaveIndicator.transform);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:64`: `nextWaveIndicator.RectTransform.anchoredPosition = new Vector2(100, 5);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:65`: `nextWaveIndicator.transform.localScale = animationDefaults.Scale(nextWaveIndicator.transform);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:74`: `nextNextWaveIndicator.RectTransform.anchoredPosition = new Vector2(200, 5);`
- `Assets/The Archer/Scripts/UI/WavePanelBehavior.cs:75`: `nextNextWaveIndicator.transform.localScale = animationDefaults.Scale(nextNextWaveIndicator.transform);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:129`: `iconImage.sprite = data.Icon;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:135`: `gradientImage.color = rarityData.GradientColor;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:136`: `cardBackImage.sprite = rarityData.CardBackSprite;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:137`: `nameBadgeImage.sprite = rarityData.NameBadgeSprite;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:168`: `cardBackSideImage.sprite = moving ? cardBackMovingSprite : cardBackSprite;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:180`: `cardBackSideImage.sprite = cardBackSprite;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:190`: `transform.localScale = animationDefaults.Scale(transform, new Vector3(scaleX, scaleY, 1));`
- `Assets/The Archer/Scripts/UI/Abilities/AbilityCardBehavior.cs:333`: `upgradeIndicator.localScale = animationDefaults.Scale(upgradeIndicator, new Vector3(0, 0.6f, 1));`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:309`: `rarityRect.localScale = animationDefaults.Scale(rarityRect);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:457`: `rarityText.color = data.RarityTextColor;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:460`: `rarityBackgroundImage.color = data.RarityBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:461`: `rarityBackgroundImage.rectTransform.sizeDelta = new Vector2(Mathf.Max(data.RarityBackgroundWidth, rarityText.preferredWidth + 40f), rarityBackgroundImage.rectTransform.sizeDelta.y);`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:632`: `rerollButton.transform.localScale = animationDefaults.Scale(rerollButton.transform, new Vector3(0, 0.6f, 1));`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorBehavior.cs:635`: `goldIndicator.transform.localScale = animationDefaults.Scale(goldIndicator.transform, new Vector3(0, 0.6f, 1f));`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:94`: `cards[i].Card.RectTransform.localScale = cards[i].Card.AuthoredScale;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:117`: `gradientImage.color = initialGradientColor;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:132`: `highestCard.RectTransform.anchoredPosition = adjustedCardPosition;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:200`: `card.transform.localScale = card.AuthoredScale * cardsScale;`
- `Assets/The Archer/Scripts/UI/Abilities/AbilitySelectorSlotBehavior.cs:223`: `highestCard.RectTransform.anchoredPosition = cardPosition + new Vector2(0, difference);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryAbilityCard.cs:29`: `abilityIconImage.sprite = data.Icon;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryCardBehavior.cs:51`: `rarityImage.sprite = rarityData.backgroundSprite;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryCardBehavior.cs:52`: `rarityBackgroundImage.color = GameController.ArmoryManager.GetItemRarityData(rarityType).SecondaryColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:270`: `content.sizeDelta = new Vector2(0, Mathf.Ceil(total / (float)columns) * stride);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:271`: `var offset = content.anchoredPosition; offset.y = Mathf.Clamp(offset.y, 0, Mathf.Max(0, content.sizeDelta.y - viewport.rect.height)); content.anchoredPosition = offset;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:304`: `var rect = (RectTransform)cell.button.transform; rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:305`: `rect.sizeDelta = Vector2.one * Mathf.Max(1, cellSize - cellGap * Scale);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:306`: `rect.anchoredPosition = new Vector2(index % columns * stride, -(index / columns) * stride);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:317`: `cell.button.image.sprite = empty ? art.emptyFrame : cell.item != null ? art.equipmentFrame : art.stackFrame;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:318`: `cell.button.image.color = cell.frameColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:323`: `if(rarity >= 0 && rarity < art.rarityFrames.Length && art.rarityFrames[rarity] != null)cell.button.image.sprite=art.rarityFrames[rarity];`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:326`: `cell.icon.sprite = empty ? art.emptySlotIcon : cell.item != null ? Armory.GetItemData(cell.item.Id).Icon : cell.stack.icon;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:327`: `cell.icon.color = cell.iconColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:332`: `cell.icon.sprite = art.emptyPlus;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:333`: `cell.icon.color = emptyPlusColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryDashboard.cs:427`: `{ rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(right, top); rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one; }`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:137`: `rectTransform.localScale = animationDefaults.Scale(rectTransform);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:149`: `rectTransform.localScale = animationDefaults.Scale(rectTransform, new Vector3(1, scaleFactor, 1));`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryGridUI.cs:161`: `rectTransform.localScale = animationDefaults.Scale(rectTransform, new Vector3(1, scaleFactor, 1));`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:222`: `card.RectTransform.anchoredPosition = slot.SlotRect.anchoredPosition;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:223`: `card.RectTransform.sizeDelta = slot.SlotRect.sizeDelta;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:225`: `card.RectTransform.localScale = cardBaseScales[card] * slotsScale;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:241`: `card.RectTransform.sizeDelta = slot.SlotRect.sizeDelta;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:262`: `if (slotBaseScales.TryGetValue(slot, out var baseScale)) slot.SlotRect.localScale = baseScale * value;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryHeroPanel.cs:268`: `if (card != null && cardBaseScales.TryGetValue(card, out var baseScale)) card.RectTransform.localScale = baseScale * value;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:49`: `graphic.color = IsSelected ? selectedContentColor : deselectedContentColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:52`: `topBackgroundImage.color = IsSelected ? selectedBackgroundColor : deselectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:53`: `bottomBackgroundImage.color = IsSelected ? deselectedBackgroundColor : selectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:74`: `bottomBackgroundImage.color = deselectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:76`: `topBackgroundImage.color = selectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:77`: `topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform, new Vector3(0, 0.7f, 1f));`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:98`: `bottomBackgroundImage.color = deselectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:100`: `topBackgroundImage.color = selectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:101`: `topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:120`: `topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:121`: `topBackgroundImage.color = selectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:128`: `topBackgroundImage.rectTransform.localScale = animationDefaults.Scale(topBackgroundImage.rectTransform);`
- `Assets/The Archer/Scripts/UI/Armory/ArmoryTabBehavior.cs:129`: `topBackgroundImage.color = deselectedBackgroundColor;`
- `Assets/The Archer/Scripts/UI/Armory/ExpandedArmoryPopup.cs:187`: `closeButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Armory/ExpandedArmoryPopup.cs:192`: `panelRect.localScale = animationDefaults.Scale(panelRect, new Vector3(0, 1, 1));`
- `Assets/The Archer/Scripts/UI/Armory/StatIndicator.cs:28`: `valueText.rectTransform.sizeDelta = new Vector2(valueText.preferredWidth + 1, valueText.rectTransform.sizeDelta.y);`
- `Assets/The Archer/Scripts/UI/Armory/StatIndicator.cs:42`: `iconImage.sprite = icon;`
- `Assets/The Archer/Scripts/UI/Armory/StatIndicator.cs:50`: `iconImage.sprite = icon;`
- `Assets/The Archer/Scripts/UI/Armory/StatIndicator.cs:60`: `valueText.rectTransform.sizeDelta = new Vector2(valueText.preferredWidth + 1, valueText.rectTransform.sizeDelta.y);`
- `Assets/The Archer/Scripts/UI/Armory/StatsGrid.cs:86`: `indicator.RectTransform.anchoredPosition = new Vector2(x, y);`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:153`: `acceptButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Chest/EvilChestUI.cs:154`: `rejectButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/ContinuePlayingPopup.cs:43`: `panelRect.localScale = animationDefaults.Scale(panelRect);`
- `Assets/The Archer/Scripts/UI/Main Menu/ContinuePlayingPopup.cs:44`: `titleRect.localScale = animationDefaults.Scale(titleRect);`
- `Assets/The Archer/Scripts/UI/Main Menu/DeveloperToolsPanel.cs:70`: `itemIcon.sprite = item ? item.Icon : null; visibility.SetVisible(itemIcon, item != null); grantItem.interactable = item;`
- `Assets/The Archer/Scripts/UI/Main Menu/LanguagePicker.cs:44`: `private void Refresh(){currentLabel.text="LANGUAGE · "+MenuLanguage.Code.ToUpperInvariant();if(useStateColors)for(int i=0;i<choices.Length;i++)choices[i].image.color=MenuLanguage.Codes[i]==MenuLanguage.Code?selectedColor:idleColor;}`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuDockItemBehavior.cs:43`: `if (icon != null) iconImage.sprite = icon;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuInfoPopup.cs:61`: `descriptionRect.sizeDelta = startRectSize;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:54`: `panelRect.localScale = animationDefaults.Scale(panelRect, new Vector3(0, 1, 1));`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuSettingsPopup.cs:60`: `closeButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuStageIcon.cs:21`: `iconImage.sprite = icon;`
- `Assets/The Archer/Scripts/UI/Main Menu/MainMenuStagesPageBehavior.cs:326`: `playButtonShineLine.DoAnchorPosition(targetPosition, 0.5f).SetOnFinish(() => playButtonShineLine.anchoredPosition = linePosition);`
- `Assets/The Archer/Scripts/UI/Main Menu/MenuWalletView.cs:21`: `icon.sprite = wallet.Data.Icon; Refresh(wallet); wallet.OnAmountChanged += Refresh;`
- `Assets/The Archer/Scripts/UI/Main Menu/StageDifficultySelector.cs:52`: `if (useStateColors) choices[i].image.color = mode == save.SelectedDifficulty ? selectedColor : unlocked ? idleColor : lockedColor;`
- `Assets/The Archer/Scripts/UI/Pause/AbilitiesGridBehavior.cs:31`: `rectTransform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Pause/MiniAbilityCardBehavior.cs:13`: `abilityIcon.sprite = data.Icon;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:149`: `titleRect.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:155`: `abilitiesRect.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:163`: `settingsRect.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:169`: `buttonsRect.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:180`: `resumeButtonShineRect.anchoredPosition = resumeShineRectPosition;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:186`: `resumeButtonShineRect.anchoredPosition = resumeShineRectPosition;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:190`: `quitButtonShineRect.anchoredPosition = quitShineRectPosition;`
- `Assets/The Archer/Scripts/UI/Pause/PauseScreenBehavior.cs:196`: `quitButtonShineRect.anchoredPosition = quitShineRectPosition;`
- `Assets/The Archer/Scripts/UI/Text Indicators/AbilitySelectedIndicatorBehavior.cs:42`: `gradientImage.color = rarityData.Color.SetAlpha(gradientAlpha);`
- `Assets/The Archer/Scripts/UI/Text Indicators/ItemTextIndicatorBehavior.cs:46`: `gradientImage.color = rarityData.MainColor.SetAlpha(gradientAlpha);`
- `Assets/The Archer/Scripts/UI/Text Indicators/ItemTextIndicatorBehavior.cs:55`: `iconImage.rectTransform.sizeDelta = Vector2.one * (textheight - heightDifference);`
- `Assets/The Archer/Scripts/UI/Text Indicators/ItemTextIndicatorBehavior.cs:63`: `iconImage.sprite = item.Icon;`
- `Assets/The Archer/Scripts/UI/Text Indicators/TextIndicatorBehavior.cs:80`: `rectTransform.anchorMin = viewportPosition;`
- `Assets/The Archer/Scripts/UI/Text Indicators/TextIndicatorBehavior.cs:81`: `rectTransform.anchorMax = viewportPosition;`
- `Assets/The Archer/Scripts/UI/Text Indicators/TextIndicatorBehavior.cs:86`: `rectTransform.anchoredPosition = position;`
- `Assets/The Archer/Scripts/UI/Text Indicators/TextIndicatorBehavior.cs:91`: `rectTransform.localScale = scale;`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:128`: `costText.color = enabledCostTextColor;`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:132`: `costText.color = disabledCostTextColor;`
- `Assets/The Archer/Scripts/UI/Armory/Hero/ExpandedHeroPopup.cs:184`: `rarityText.color = rarityData.MainColor;`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs:36`: `iconImage.sprite = data.Icon;`
- `Assets/The Archer/Scripts/UI/Armory/Hero/HeroCardBehavior.cs:70`: `lockedStateBackgroundImage.color = GameController.ArmoryManager.GetItemRarityData(rarityType).MainColor;`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:111`: `costText.color = enabledCostTextColor;`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:115`: `costText.color = disabledCostTextColor;`
- `Assets/The Archer/Scripts/UI/Armory/Items/ExpandedItemPopup.cs:196`: `rarityText.color = rarityData.MainColor;`
- `Assets/The Archer/Scripts/UI/Armory/Items/ItemCardBehavior.cs:46`: `iconImage.sprite = data.Icon;`
- `Assets/The Archer/Scripts/UI/Armory/Items/ItemCardBehavior.cs:54`: `itemTypeImage.sprite = GetItemTypeSprite(data.ItemType);`
- `Assets/The Archer/Scripts/UI/Armory/Items/ItemCardBehavior.cs:70`: `itemTypeBackgroundImage.color = rarityBackgroundImage.color;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementRowFocus.cs:27`: `scroll.content.anchoredPosition = position;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:168`: `if (useStateColors) tab.background.color = selected ? selectedTabColor : idleTabColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:170`: `if (useStateColors) tab.title.color = selected ? Blue : Ink;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:195`: `row.icon.sprite = definition.icon != null ? definition.icon : database.trophySprite;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:271`: `if (useStateColors) row.buttonImage.color = state == 2 ? claimedButtonColor : Color.white;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:272`: `if (useStateColors) row.background.color = state == 1 ? claimableRowColor : idleRowColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Achievements/AchievementsPageBehavior.cs:333`: `{ var fraction = Mathf.Clamp01(value); rect.anchorMax = new Vector2(fraction, 1); authoredVisibility.SetVisible(rect.gameObject, fraction > 0); }`
- `Assets/The Archer/Scripts/UI/Main Menu/Forge/ForgePageBehavior.cs:125`: `showcaseIcon.sprite = detailIcon.sprite = quote.item != null ? quote.item.Icon : database.forgeIcon;`
- `Assets/The Archer/Scripts/UI/Main Menu/Forge/ForgePageBehavior.cs:131`: `if (useStateColors) costLabel.color = quote.fragments < quote.requiredFragments ? Red : Green;`
- `Assets/The Archer/Scripts/UI/Main Menu/Forge/ForgePageBehavior.cs:136`: `view.count.text = amount + " / " + required; if (useStateColors) view.count.color = amount >= required ? Green : Red;`
- `Assets/The Archer/Scripts/UI/Main Menu/Forge/ForgePageBehavior.cs:142`: `if (useStateColors) statusLabel.color = ready || isOwned ? Green : Red;`
- `Assets/The Archer/Scripts/UI/Main Menu/Forge/ForgePageBehavior.cs:167`: `if(useStateColors) row.after.color = Ink; row.arrow.text = "";`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:85`: `var icon = PrizeIcon(prize); row.icon.sprite = icon;`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:168`: `else if (chestIllustration) chestIllustration.localScale = chestBaseScale * (1 + Mathf.Sin(t * Mathf.PI * 2 * chestPulseCycles) * chestPulseAmplitude * (1 - t));`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:171`: `if (chestIllustration) chestIllustration.localScale = chestBaseScale;`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:179`: `resultIcon.sprite = service.Delivery.Icon(receipt.grant); authoredVisibility.SetEnabled(resultIcon, resultIcon.sprite != null);`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyRewardsPopup.cs:192`: `if (wired && chestIllustration) chestIllustration.localScale = chestBaseScale;`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyWheelSlice.cs:17`: `sector.color = index % 2 == 0 ? evenSectorColor : oddSectorColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyWheelSlice.cs:20`: `icon.sprite = sprite; icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = Vector2.one * .5f + position * iconRadius;`
- `Assets/The Archer/Scripts/UI/Main Menu/Lucky Rewards/LuckyWheelSlice.cs:21`: `icon.rectTransform.anchoredPosition = Vector2.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Quests/DailyQuestsPageBehavior.cs:139`: `row.icon.sprite = quest.icon != null ? quest.icon : database.questIcon;`
- `Assets/The Archer/Scripts/UI/Main Menu/Quests/DailyQuestsPageBehavior.cs:142`: `if (useStateColors) row.buttonImage.color = claimed ? claimedButtonColor : complete ? Color.white : incompleteButtonColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Quests/DailyQuestsPageBehavior.cs:151`: `if (useStateColors) chest.icon.color = claimed ? claimedChestColor : available ? Color.white : lockedChestColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Quests/DailyQuestsPageBehavior.cs:153`: `if (useStateColors) chest.state.color = claimed ? claimedTextColor : Ink;`
- `Assets/The Archer/Scripts/UI/Main Menu/Quests/DailyQuestsPageBehavior.cs:238`: `{ var fraction = Mathf.Clamp01(value); fill.anchorMax = new Vector2(fraction, 1); authoredVisibility.SetVisible(fill.gameObject, fraction > 0); }`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/ExpandedUpgradePopup.cs:109`: `closeButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardBehavior.cs:118`: `upgradeImage.sprite = Data.Icon;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardSelectable.cs:47`: `RectTransform.localScale = animationDefaults.Scale(RectTransform, new Vector3(0, 1, 1));`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardSelectable.cs:92`: `RectTransform.localScale = animationDefaults.Scale(RectTransform);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardSelectable.cs:93`: `RectTransform.anchoredPosition = Vector2.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardsSelector.cs:36`: `leftButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardsSelector.cs:37`: `rightButton.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardsSelector.cs:39`: `leftGamepadIndicator.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardsSelector.cs:40`: `rightGamepadIndicator.transform.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardsSelector.cs:42`: `selectablesParent.localScale = Vector3.zero;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeCardsSelector.cs:50`: `selectablesParent.localScale = animationDefaults.Scale(selectablesParent, new Vector3(0, 1, 1));`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDashboardPage.cs:90`: `row.icon.sprite = row.data.Icon; row.title.text = row.data.Title; row.description.text = row.data.Description;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDashboardPage.cs:95`: `if (useStateColors) row.background.color = row == selected ? selectedRowColor : idleRowColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDashboardPage.cs:96`: `for (int i = 0; i < row.segments.Length; i++) row.segments[i].color = (i + 1f) / row.segments.Length <= (level + 1f) / Mathf.Max(1, row.data.LevelsCount) ? filledSegmentColor : emptySegmentColor;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDashboardPage.cs:100`: `selectedIcon.sprite = selected.data.Icon; selectedTitle.text = selected.data.Title; selectedDescription.text = selected.data.Description;`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDescriptionBlock.cs:81`: `statsBackgroundRect.sizeDelta = statsBackgroundRect.sizeDelta.SetX(statsText.preferredWidth + backgroundPadding * 2);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDescriptionBlock.cs:83`: `descriptionText.rectTransform.sizeDelta = new Vector2(descriptionText.rectTransform.sizeDelta.x + 1, TextHeight() + 1);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDescriptionBlock.cs:84`: `statsBackgroundRect.anchoredPosition = Vector2.down * (TextHeight() + spacing);`
- `Assets/The Archer/Scripts/UI/Main Menu/Upgrades/UpgradeDescriptionBlock.cs:91`: `statsBackgroundRect.localScale = Vector3.zero;`

## Danh sách prefab

| Prefab | Có UI | Component script bị thiếu |
| --- | --- | ---: |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/_PrefabsPanel_Buttons.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle128_Dark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle128_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle147_Gray.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle147_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Circle147_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_FlushLeft_Gray.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_FlushLeft_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_FlushRight_Gray.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_FlushRight_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Hexagon199_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Hexagon199_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square01_Sky.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square02_Gray.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square03_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square04_Dark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square04_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square05_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_Square06_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_TaperedDown.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button_TaperedLeft.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_BlueGray.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Gray.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Orange.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Purple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Sky.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button01_BtnText_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button02_BtnIconText_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button02_BtnIconText_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button03_BtnIconText_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Button03_BtnIconText_Purple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Buttons/Menu_TabMenu.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/_PrefabsPanel_Frames.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BannerFrame01_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BannerFrame01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BannerFrame02_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BannerFrame02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BannerFrame03_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BannerFrame03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Circle77_Dark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Circle77_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round12_Gradient_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round12_Gradient_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round12_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round12_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round20_Transparent_Navy_1.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round20_Transparent_Navy_2.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Round20_Transparent_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Square_Dark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BasicFrame_Square_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Circle81_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Circle81_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round01_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round01_Sky.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round02_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round03_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round04_Transparent_Purple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round04_Transparent_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Round05_Gradient.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Square01_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BorderFrame_Square01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BubbleFrame01_Single_OffWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BubbleFrame01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BubbleFrame02_Left-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/BubbleFrame02_Right-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame01-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame02-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame03-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame04-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame05-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame06-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame07-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/CardFrame08-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/Frame_FlushDown.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/Frame_FlushLeft.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/Frame_FlushUp.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame01-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame02-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame03-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame04-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame05-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame06-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ItemFrame07-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ListFrame01-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ListFrame02-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ListFrame03-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ListFrame04-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame01_Round_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame01_Round_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame02_Round_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame02_Round_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame03_Topbar_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame03_Topbar_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame04_TopbarDivided_Single_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame04_TopbarDivided_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame05_Bottom_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame05_Bottom_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame06_Bottom_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/PanelFrame06_Bottom_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ProfileFrame01_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/ProfileFrame01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/SkillFrame01-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/SkillFrame02-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Frames/StageFrame-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/_PrefabsPanel_Labels.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Bubble01_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Bubble01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Bubble02_Single_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Bubble02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Ribbon_Single_Orange.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Ribbon_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Round01_Black.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Round01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Round02_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Round02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Tapered_Black.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Tapered_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Label_Trapezoid-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Flag01-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Flag02-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Line_Divider_TransparentWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Line_Divider_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Oval_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Oval_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Labels/Title_Ribbon-Group-.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/_PrefabsPanel_Popups.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth01_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth02_TransparentDark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth02_TransparentWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth03_Single_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_FullWidth03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_Slide01_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_Slide01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_Slide02_Dark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup_Slide02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup01_Single_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup01_Single_Purple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup02_Divided_Single_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup02_Divided_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup03_Divided_Single_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/Popup03_Divided_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/ToastMessage_Topbar_Single_Purple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Popups/ToastMessage_Topbar_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/_PrefabsPanel_Sliders.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic01_DemoBlue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic02_DemoYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic03_DemoYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic04_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic04_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Basic04_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTrapezoid01_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTrapezoid01_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTrapezoid01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTrapezoid02_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTrapezoid02_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTrapezoid02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTriple_Demo.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_BasicTriple_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Fusion_Demo.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Handle_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Handle_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon01_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon02_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon03_OrangeYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon04_Green-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon04_Red-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon05_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Icon05_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level01_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level01_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level01_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level01_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level02_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level03_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Level04_Purple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_StageHorizontal_Demo.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_StageHorizontal_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Wave_Demo.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_Sliders/Slider_Wave_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/_PrefabsPanel_UI_Etc.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Dot_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Dot_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Text_l_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Text_l_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Text_l_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Text_s_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Text_s_Red.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Alert_Text_s_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/InputField01-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/InputField02_Navy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/InputField02_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/InputField03_Dark.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/InputField03_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/ResourceBar-Group.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Swich_Single_Off.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Swich_Single_On.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Swich_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Toggle01_Check_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Toggle01_Check_Orange.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Toggle01_Check_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Toggle02_CheckBox_Orange.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/Toggle02_CheckBox_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/UserInfo01_Left.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_Component_UI_Etc/UserInfo02_Right.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Ad_Remove.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Character.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Character_List.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Character_Select.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Character_SuperPassive.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Chat.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Clan.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Component_1.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Component_2.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Component_3.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Component_4.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Component_5.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Equipment.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Equipment_Popup_AddSlot.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Equipment_Popup_Detail1.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Equipment_Popup_Detail2.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/ErrorNetwork.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Inventory.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/LevelUp.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Loading.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Lobby.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/LockScreen.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Missions.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Pass.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_BattleVS.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_Continue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_Pause.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_Result.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_Type1.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_Type2.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Play_TypeText.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_ChangeName.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Checking.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_EUALPrivacy.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Inbox.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Login.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Name.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Offline.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Pass.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_RewardGet.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_RewardMonth.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_RewardWeek.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Setting.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Setting_Language.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Signup.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Subscribe.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_Update.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Popup_UserInfo.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Ranking.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Rate.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Roulette.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/RuneFuse.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Shop.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Stage_Select_Type1.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Stage_Select_Type2.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Stage_Select_Type2_Detail.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Title.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Title_LoadingBar.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Tutorial_Character.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Panels/Tutorial_Hand.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light01.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light02.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light03.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light04.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light05.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Rotate_Light06.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Shines_Glow01.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Shines_Glow02.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Shines_Glow03.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Shines_Light01.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Shines_Light02.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Snow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar01_ClearBlue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar01_ClearGreen.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar01_ClearPurple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar01_ClearRed.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar01_ClearWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar01_ClearYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar02_BlurryBlue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar02_BlurryGreen.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar02_BlurryPurple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar02_BlurryRed.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar02_BlurryWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_LongStar02_BlurryYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star01_CustomColor_Blue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star01_CustomColor_Green.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star01_CustomColor_Pink.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star01_CustomColor_White.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star01_CustomColor_Yellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star02_ClearBlue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star02_ClearGreen.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star02_ClearPurple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star02_ClearRed.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star02_ClearWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_Star02_ClearYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar01_BlurryBlue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar01_BlurryGreen.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar01_BlurryPurple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar01_BlurryRed.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar01_BlurryWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar01_BlurryYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar02_ClearBlue.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar02_ClearGreen.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar02_ClearPurple.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar02_ClearRed.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar02_ClearWhite.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Sparkle_XStar02_ClearYellow.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Circle01.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Circle02.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star01.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star02.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star03.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star04.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star05.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/Prefabs/Prefabs_DemoScene_Particle/Fx_Spread_Star06.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/UI_Etc/UserInfo01.prefab` | Có | 0 |
| `Assets/The Archer/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprite/Components/UI_Etc/UserInfo02.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Combo Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Invulnerability Star Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Painful Power Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Perfect Stance Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Rage Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Buff Abilities/Rage Star Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Cure Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Heal Enhancement Ability Behavior.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Kill To Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Power Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Protector Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Safety Bubble Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Second Chance Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Defensive Abilities/Strong Aura Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Freeze Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Freezing Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Ignite Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Ignite Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Meteor Rain Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Meteor Strike Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Poison Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Poison Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Shock Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Shock Enhancement Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Elemental Abilities/Ultimate Meteor Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Common Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Common Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Legendary Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Legendary Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Mystic Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Mystic Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Rare Coins Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Endgame Abilities/Rare Heal Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Fire Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Ice Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Laser Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Orbs Enhansment Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Poison Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Shock Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Ultimate Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Orb Abilities/Vampiric Orbs Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Fire Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Freeze Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Poison Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Shock Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Spirit Enhansment Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Spirit Rage Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Spirit Abilities/Ultimate Spirit Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Boss Slayer Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Critical Strike Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Four Leaf Clover Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Gnome Form Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Lucky Charm Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Major Health Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Major Power Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Major Swiftness Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Medium Power Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Attack Speed Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Health Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Power Boost Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Speed Increase Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Minor Swiftness Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Starfall Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Titan Form Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Stat Abilities/Trifecta Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Bouncy Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Bullseye Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Close Combat Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Diagonal Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Flying Weapon Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Front Projectile Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Multi Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Piercing Strike Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Quick Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Rear Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Ricochet Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Sniper Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Split Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Abilities/Weapon Abilities/Traversing Shot Ability.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Audio/Audio Source.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Chests/Evil Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Chests/Good Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Chests/Health Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Boss Loot Chest.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Heal.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Invulnerability Star.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Item.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/Rage Star.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Drop/XP Gem.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Boss Spawn Point.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Cactus Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Golem Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Ice Slime Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Mask Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Queen Wasp Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Mage Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Minion Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Rogue Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Skeletons/Skeleton Warrior Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Bosses/Totem Boss.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Enemy Healthbar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Bat Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Bomb Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Candle Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Cauldron Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/B Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/C Attack 1 Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/C Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/Ca Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/E Attack Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/G Attack 1 Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/G Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/H Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/I Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/M Attack Finish.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/M Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/P HIt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/Pu Attack Charge.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/Q Charge 1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/S Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/S Spawn.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Mesh Prefabs/T Hit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Enemy Spawn Point.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Eye Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Hand Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Jelly Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Mole Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Plant Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Pumpkin Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Slime Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Smaller Slime Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Smallest Slime Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Spike Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Trap Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Enemies/Simple Enemies/Wasp Enemy.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Flying Weapon Holder.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Barbarian.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Knight.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Mage.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Rogue.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Adventurers/Rogue_Hooded.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Archer Hero.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Ranger.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Heroes/Thief Hero.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Player/Player Behavior.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Player/Player Healthbar.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Bat Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cactus Flex Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cactus Machine Gun Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cactus Spike.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Candle Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cauldron Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Cauldron Puddle Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Eye Laser Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Golem Line Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Golem Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Honey Mine.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Crystal.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Puddle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Shard.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Ice Sphere.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Jellyfish Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Mask Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Mask Wave.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Plant Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Pumpkin Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Queen Wasp Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Shock Spread Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Arcane Bolt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Skeleton Crossbow Bolt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Totem Fist.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Totem Laser.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Totem Tornado.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Enemies/Wasp Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Meteors/Meteor Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Meteors/Ultimate Meteor Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Fire Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Ice Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Laser Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Orb Laser.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Poison Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Shock Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Ultimate Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Orbs/Vampiric Orb.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Arrow Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Split Arrow Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Split Wand Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Player/Player's Wand Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Fire Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Freeze Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Poison Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Shock Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Projectiles/Spirits Projectiles/Ultimate Spirit Projectile.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Big High Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Big Low Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Crack.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Env Frozen.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Env.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Exit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Chamber Small Env.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Icicles 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Icicles.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small High Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle Center.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle End L.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle End R.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Chamber/Small Low Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/arch.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/arch_gate.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bench.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bench_decorated.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bone_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bone_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/bone_C.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle_melted.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle_thin.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/candle_triple.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/coffin.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/coffin_decorated.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/crypt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_broken.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_gate.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_pillar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_pillar_broken.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_seperate.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/fence_seperate_broken.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/floor_dirt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/floor_dirt_grave.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/floor_dirt_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/grave_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/grave_A_destroyed.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/grave_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/gravemarker_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/gravemarker_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/gravestone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/Halloween Crypt Arena.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/lantern_hanging.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/lantern_standing.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_A.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_B.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_C.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/path_D.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pillar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/plaque.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/plaque_candles.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/post.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/post_lantern.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/post_skull.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_orange.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_orange_jackolantern.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_orange_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_yellow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_yellow_jackolantern.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/pumpkin_yellow_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/ribcage.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/shrine.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/shrine_candles.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/skull.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/skull_candle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/Stage 5 Bone Courtyard.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_large.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_large_decorated.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_dead_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_orange_large.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_orange_medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_orange_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_yellow_large.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_yellow_medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Halloween/tree_pine_yellow_small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Archive Temple 1 Clone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Archive Temple 1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Archive Temple.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Beast Altar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Block.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Border Strips_1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Border Strips_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Border Strips_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Broken Pillar.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert Annex.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert House Block.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert House Medium.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Desert Tower.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Flat Rock_02.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Flat Rock_03.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Flat Rock_04.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Floor Tile Modules.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Floor Tile Modules_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Floor Tile Modules_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Lily Pads_1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Lily Pads_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Lily Pads_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Obelisk Tower.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Patterned Floor.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Pillar Crystal.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Plain Floor.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Pool Rim.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Relic Block Clone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Relic Block.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Ruined House Front.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Ruined House Top.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Rune.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Sandstone Stairs.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Slab.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Small Corner Pieces_1.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Small Corner Pieces_2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Small Corner Pieces_3.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_01.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_02.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_03.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_04.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_05.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Pyramid/Tall Rock_06.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/High Big Obstacle.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/High Small Obstacle 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Low Big Obstacle 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Low Small Obstacle 2.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Temple Env Clone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Prop/Temple/Temple Env.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Fire Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Freeze Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Poison Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Shock Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Spirits/Ultimate Spirit.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Armory Ability Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Armory Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Armory Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Expanded Hero Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Expanded Item Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Hero Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Hero Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Hero Preview.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Info Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Item Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Rewards.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Stat Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Armory/Tabs Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Button Selection Arrow.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Selector Slot.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Abilities Selector/Ability Selector.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Evil Chest UI.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Heal Chest UI.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Joystick.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause Menu.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause/Abilities Grid.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause/Abilities Row.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Pause/Mini Ability Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Run Loot Reward Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Small Item Reward Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Small Reward Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Stage Complete.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/Stage Failed.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Ability Select Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Combo Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Dodge Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Burning Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Crit Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Freezing Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Poison Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Enemy Damage/Shock Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Heal Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Item Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Burning Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Freezing Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Physical Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Poison Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Game/World Space Text/Player Damage/Player Shock Damage Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Gold Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Loading/Loading Screen.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Gem Chest Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Lucky Wheel Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Prize Row.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Stage Lucky Rewards.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Lucky Rewards/Wheel Slice.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Continue Playing Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Dock.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Info Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stage Icon.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stage Panel.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Journey Page/Stages Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Settings Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Expanded Upgrade Popup.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Expanded Upgrades Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Selectable Upgrade.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Upgrades Card.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Main Menu/Upgrades Page/Upgrades Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Achievements Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Armory Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Daily Quests Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Forge Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Gem Wallet.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Dock.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Menu Economy.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Responsive Menu Surface.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Reward Notification Dot.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Menu Pages/Upgrades Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Offline Progression/Run Summary.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Offline Progression/Stage Difficulty Selector.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Compact Run Result.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Hero EXP Bar.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Result Reward Tile.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Run Progression/Run Season HUD.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Stage/Developer Tools.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Stage/Language Picker.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Stage/Stage Page.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Text Effects/UI Text Effects Demo.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/UI/Wave Indicator.prefab` | Có | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Attack Speed Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Base HP Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Crit Chance Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Dodge Chance Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/HP Recovery Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Move Speed Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Power Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Revive Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Upgrades/Shield Upgrade.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/Shade Sphere.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Blob.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cactus Leaf.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cactus Spike Small.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Capsule Half.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cone Flat.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Cone.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Disc.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Dome.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Gem.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Ice Shard.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Ice Sphere.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Lase Plane.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Mole Dirt.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Ring.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Shine Square.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Stinger.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Target Ring Big.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Target Ring Solid.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Target Ring.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/VFX Models Prefabs/VFX Tornado.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Arcane Spellbook.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Barbarian Axe and Shield.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Barbarian Battle Axe.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Knight Greatsword.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Knight Sword and Shield.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Mage Staff.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Mage Wand.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/mug_empty.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/mug_full.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_badge_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_round.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_round_barbarian.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_round_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_spikes.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_spikes_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_square.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/shield_square_color.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Offhand/spellbook_closed.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Ranger Longbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Rogue Dagger.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Rogue Hand Crossbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Rogue Heavy Crossbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Adventurers/Royal Greatsword.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Bow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Crossbow.prefab` | — | 0 |
| `Assets/The Archer/Prefabs/Weapons/Wand.prefab` | — | 0 |
| `Assets/The Archer/Resources/Crafting Drop.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_green.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_orange.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_teal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_trifecta.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_buff_yellow.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_painful_power.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_perfect_stance.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_rage.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Buffs/vfx_strong_aura.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_ignite.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_crit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_default.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_freeze.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_poison.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Arrow/vfx_arrow_trail_shock.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_fire.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_fireball.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_fireball_alt.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_freeze.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_ignite.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_meteor.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_poison.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_shock.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_shock_trail.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_slimy_trail.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/vfx_ultimate_meteor.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_crit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_default.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_fire.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_freeze.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_poison.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Effects/Wand/vfx_wand_projectile_shock.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_bubble.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_bubble_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_bubble_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_heal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/General/vfx_heal_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_fire_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_freeze_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_laser.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_laser_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_poison_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_shock_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_ultimate_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Orbs/vfx_vampiric_orb.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_fire_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_freeze_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_poison_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_shock_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/Projectiles/vfx_ultimate_spirit_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_fire_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_freeze_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_poison_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_shock_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Abilities/Spirits/vfx_ultimate_spirit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_evil.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_evil_shine.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_good.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_good_shine.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_heal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_heal_shine.prefab` | — | 0 |
| `Assets/The Archer/VFX/Chests/vfx_chest_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_gem.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_gem_pickup.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_item_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_star_highlight.prefab` | — | 0 |
| `Assets/The Archer/VFX/Drop & Items/vfx_star_pickup.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bat/vfx_bat_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bat/vfx_bat_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bell/vfx_shade_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bomb/vfx_bomb_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Bomb/vfx_bomb_fuse.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_leaf.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_leaf_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_shot.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_slam.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_spike.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_spike_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cactus/vfx_cactus_spike_small.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Candle/vfx_candle_fire.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Candle/vfx_candle_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Candle/vfx_candle_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cauldron/vfx_cauldron_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cauldron/vfx_cauldron_puddle.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Cauldron/vfx_cauldron_splash.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Eye/vfx_eye_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Eye/vfx_eye_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Eye/vfx_laser_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_eye.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_landing.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spin_circle.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spin_lightning.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_spin_trail.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_golem_take_off.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_lightning_strike.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_lightning_vertical.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_zap.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Golem/vfx_zap_line.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_aura.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_circle.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_crystal.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_shard.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_sphere.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Ice Slime/vfx_ice_sphere_crack.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Jellyfish/vfx_jellyfish_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Jellyfish/vfx_jellyfish_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mask/vfx_mask_wave_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mask/vfx_shade_sphere.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mask/vfx_shade_splatter.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mole/vfx_mole_burst.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mole/vfx_mole_digging.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Mole/vfx_mole_dirt.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Plant/vfx_plant_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Plant/vfx_plant_pop.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Plant/vfx_plant_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Pumpkin/vfx_pumpkin_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Queen Wasp/vfx_honey_splatter.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Queen Wasp/vfx_queen_wasp_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Queen Wasp/vfx_queen_wasp_projectile.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Slime/vfx_slime_split.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_laser_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_tornado.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_fist_slam.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_laser.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_monkey_slam.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Totem/vfx_totem_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Trap/vfx_trap_surprise.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Trap/vfx_trap_surprise_3D.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Wasp/vfx_wasp_spark.prefab` | — | 0 |
| `Assets/The Archer/VFX/Enemies/Wasp/vfx_wasp_stinger.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_boss_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_enemy_defeat.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_enemy_spawn.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_exit_sparks.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_field_explosion.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_fireflies.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_player_ring.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_running_dust.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_running_flame.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_target_ring.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_target_ring_big.prefab` | — | 0 |
| `Assets/The Archer/VFX/Gameplay/vfx_warning.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_arrow_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_bow_shot.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_wand_hit.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_wand_shot.prefab` | — | 0 |
| `Assets/The Archer/VFX/Weapons/vfx_weapon_cloud.prefab` | — | 0 |
