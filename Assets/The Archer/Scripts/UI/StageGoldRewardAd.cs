using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    [RequireComponent(typeof(StageCompleteScreen))]
    public sealed class StageGoldRewardAd : MonoBehaviour
    {
        private const string GoldCurrencyId = "gold";
        private const string AndroidTestRewardedId = "ca-app-pub-3940256099942544/5224354917";

        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button claimButton;
        [SerializeField] private SmallRewardCard goldRewardCard;

        private static bool sdkInitializing;
        private static bool sdkInitialized;

        private StageCompleteScreen resultScreen;
        private CurrencySave wallet;
        private RewardedAd loadedAd;
        private int bonusGold;
        private bool isShowingAd;
        private bool rewardGranted;
        private bool requestInProgress;

        private void Awake()
        {
            resultScreen = GetComponent<StageCompleteScreen>();
            if (watchAdButton == null) return;

            watchAdButton.interactable = false;
            watchAdButton.onClick.AddListener(ShowAd);
        }

        private void OnEnable()
        {
            if (watchAdButton == null || claimButton == null || claimButton == watchAdButton) return;
            if (Application.platform != RuntimePlatform.Android || !Debug.isDebugBuild) return;

            StartCoroutine(PrepareForResult());
        }

        private IEnumerator PrepareForResult()
        {
            yield return new WaitUntil(() => resultScreen.IsOpen);

            if (rewardGranted || isShowingAd) yield break;

            wallet = GameController.CurrenciesManager.GetCurrency(GoldCurrencyId, false);
            if (wallet == null ||
                !StageController.DropManager.ResultCurrencies.TryGetValue(GoldCurrencyId, out bonusGold) ||
                bonusGold <= 0 || bonusGold > int.MaxValue / 2)
            {
                yield break;
            }

            if (!sdkInitialized && !sdkInitializing)
            {
                sdkInitializing = true;
                MobileAds.Initialize(status => MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    sdkInitialized = status != null;
                    sdkInitializing = false;
                }));
            }

            yield return new WaitUntil(() => !sdkInitializing);
            if (sdkInitialized) LoadAd();
        }

        private void LoadAd()
        {
            if (loadedAd != null)
            {
                watchAdButton.interactable = loadedAd.CanShowAd();
                if (watchAdButton.interactable) return;

                loadedAd.Destroy();
                loadedAd = null;
            }

            if (!isActiveAndEnabled || requestInProgress || rewardGranted) return;

            requestInProgress = true;
            RewardedAd.Load(AndroidTestRewardedId, new AdRequest(), (ad, error) =>
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    if (this == null)
                    {
                        ad?.Destroy();
                        return;
                    }

                    requestInProgress = false;
                    if (ad == null || error != null)
                    {
                        ad?.Destroy();
                        Debug.LogWarning($"[ArcBorn] Could not load x2 Gold test ad: {error}");
                        return;
                    }

                    loadedAd = ad;
                    watchAdButton.interactable = isActiveAndEnabled && ad.CanShowAd();
                }));
        }

        private void ShowAd()
        {
            if (!isActiveAndEnabled || !resultScreen.IsOpen || isShowingAd || rewardGranted) return;
            if (loadedAd == null || !loadedAd.CanShowAd())
            {
                watchAdButton.interactable = false;
                LoadAd();
                return;
            }

            var ad = loadedAd;
            loadedAd = null;
            isShowingAd = true;
            var claimWasInteractable = claimButton.interactable;
            claimButton.interactable = false;
            watchAdButton.interactable = false;

            var resultWallet = wallet;
            var resultBonus = bonusGold;
            var adFinished = false;

            void FinishAd()
            {
                if (adFinished) return;

                adFinished = true;
                ad.Destroy();
                if (this == null) return;

                isShowingAd = false;
                claimButton.interactable = claimWasInteractable;
                if (!rewardGranted) LoadAd();
            }

            ad.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(FinishAd);
            ad.OnAdFullScreenContentFailed += error => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.LogWarning($"[ArcBorn] Could not show x2 Gold test ad: {error}");
                FinishAd();
            });

            ad.Show(_ => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (rewardGranted) return;
                if (resultWallet.Amount > int.MaxValue - resultBonus)
                {
                    Debug.LogWarning("[ArcBorn] Gold balance would overflow.");
                    return;
                }

                rewardGranted = true;
                try
                {
                    resultWallet.Deposit(resultBonus);
                }
                finally
                {
                    GameController.SaveManager.Save(false);
                }

                if (this == null) return;
                watchAdButton.interactable = false;
                if (goldRewardCard != null) goldRewardCard.SetAmount(resultBonus * 2);
                Debug.Log($"[ArcBorn] x2 Gold granted: +{resultBonus} gold.");
            }));
        }

        private void OnDestroy()
        {
            if (watchAdButton != null) watchAdButton.onClick.RemoveListener(ShowAd);
            loadedAd?.Destroy();
        }
    }
}
