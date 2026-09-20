using OctoberStudio.UI.Armory;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.UI
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

        public ExpandedUpgradePopup ExpandedUpgradePopup => expandedUpgradePopup;
        public ExpandedHeroPopup ExpandedHeroPopup => expandedHeroPopup;
        public ExpandedItemPopup ExpandedItemPopup => expandedItemPopup;

        public MainMenuSettingsPopup SettingsPopup => settingsPopup;
        public ContinuePlayingPopup ContinuePlayingPopup => continuePlayingPopup;
        public MainMenuInfoPopup InfoPopup => infoPopup;

        protected virtual void Awake()
        {
            CurrentPage = pages.Find(page => page.PageType == defaultPageType);

            GameController.RegisterMainMenuScreenBehavior(this);
        }

        protected virtual void Start()
        {
            dockBehavior.Init(defaultPageType);
            dockBehavior.onPageSelected += OnPageSelected;
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