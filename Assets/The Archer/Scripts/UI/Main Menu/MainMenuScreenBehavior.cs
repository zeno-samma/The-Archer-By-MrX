using MrX.UI.Armory;
using System.Collections.Generic;
using UnityEngine;

namespace MrX.UI
{
    public class MainMenuScreenBehavior : MonoBehaviour
    {
        [SerializeField] protected MainMenuDockBehavior dockBehavior;

        [Header("Pages")]
        [SerializeField] protected MainMenuPageType defaultPageType;
        [SerializeField] protected List<MainMenuPageBehavior> pages;

        [Space]
        [SerializeField] protected ExpandedUpgradePopup expandedUpgradePopup;
        [SerializeField] protected ExpandedHeroPopup expandedHeroPopup;
        [SerializeField] protected ExpandedItemPopup expandedItemPopup;
        [SerializeField] protected MainMenuSettingsPopup settingsPopup;
        [SerializeField] protected ContinuePlayingPopup continuePlayingPopup;
        [SerializeField] protected MainMenuInfoPopup infoPopup;

        public MainMenuPageType CurrentPageType => CurrentPage.PageType;
        public MainMenuPageBehavior CurrentPage { get; protected set; }
        public bool IsDockEnabled => dockBehavior.IsDockEnabled;

        public ExpandedUpgradePopup ExpandedUpgradePopup => expandedUpgradePopup;
        public ExpandedHeroPopup ExpandedHeroPopup => expandedHeroPopup;
        public ExpandedItemPopup ExpandedItemPopup => expandedItemPopup;

        public MainMenuSettingsPopup SettingsPopup => settingsPopup;
        public ContinuePlayingPopup ContinuePlayingPopup => continuePlayingPopup;
        public MainMenuInfoPopup InfoPopup => infoPopup;

        protected virtual void Awake()
        {
            CurrentPage = pages.Find(page => page.PageType == defaultPageType)
                ?? pages.Find(page => page.PageType == MainMenuPageType.Stages)
                ?? pages[0];

            GameController.RegisterMainMenuScreenBehavior(this);
            foreach (var page in pages) page.PrepareInitialState(page == CurrentPage);
        }

        public virtual bool OpenPage(MainMenuPageType pageType)
        {
            if (!pages.Exists(page => page.PageType == pageType)) return false;
            return dockBehavior.TrySelectPage(pageType);
        }

        public virtual bool OpenHeroUpgrades()
        {
            if (!OpenPage(MainMenuPageType.Armory)) return false;
            var armory = CurrentPage as MainMenuArmoryPageBehavior;
            if (armory != null) armory.OpenHeroesTab();
            return armory != null;
        }

        protected virtual void Start()
        {
            dockBehavior.Init(CurrentPage.PageType);
            dockBehavior.onPageSelected += OnPageSelected;
            if (CurrentPage.PageType != defaultPageType)
                Easing.EasingManager.DoNextFrame(() => OpenPage(defaultPageType));
        }

        protected virtual void OnPageSelected(MainMenuPageType pageType)
        {
            if (CurrentPage.PageType == pageType)
            {
                return;
            }

            var currentIndex = dockBehavior.GetDockItemIndex(CurrentPageType);
            var targetIndex = dockBehavior.GetDockItemIndex(pageType);
            var targetPage = pages.Find(page => page.PageType == pageType);
            if (targetPage == null || targetIndex < 0) return;

            if (targetIndex > currentIndex)
            {
                CurrentPage.MoveLeft();
                targetPage.SpawnRight();
            }
            else
            {
                CurrentPage.MoveRight();
                targetPage.SpawnLeft();
            }

            targetPage.MoveCenter();

            CurrentPage = targetPage;
        }

        public virtual void EnableDock()
        {
            dockBehavior.IsDockEnabled = true;
        }

        public virtual void DisableDock()
        {
            dockBehavior.IsDockEnabled = false;
        }
    }
}
