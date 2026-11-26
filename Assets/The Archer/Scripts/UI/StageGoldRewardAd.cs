using System;
using System.Collections;
using System.Collections.Generic;
using MrX.Progression;
using MrX.Rewards;
using MrX.Save;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    [RequireComponent(typeof(StageCompleteScreen))]
    public sealed class StageGoldRewardAd : MonoBehaviour
    {
        private const string AndroidTestRewardedId = "ca-app-pub-3940256099942544/5224354917";
        private const string DailyUsageSaveKey = "Stage Reward Ad Daily Usage";
        private const int DefaultResetUtcOffsetHours = 7;

        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button claimButton;
        [SerializeField] private TMP_Text dailyAdsRemaining;
        [SerializeField, Min(1)] private int dailyAdLimit = 5;

        private static bool sdkInitializing;
        private static bool sdkInitialized;

        private StageCompleteScreen resultScreen;
        private RewardedAd loadedAd;
        private readonly Dictionary<string, long> currencyBonuses = new Dictionary<string, long>();
        private readonly List<LuckyGrant> lootBonuses = new List<LuckyGrant>();
        private int bonusXp;
        private bool isShowingAd;
        private bool rewardGranted;
        private bool requestInProgress;
        private StageRewardAdDailySave dailyUsage;
        private float nextDailyCheck;

        private int DailyLimit => Mathf.Max(1, dailyAdLimit);
        private int ResetUtcOffsetHours => GameController.DailyQuestsManager?.Database?.resetUtcOffsetHours
            ?? DefaultResetUtcOffsetHours;
        private bool AdsSupported => Application.platform == RuntimePlatform.Android && Debug.isDebugBuild;
        private bool CanOfferAd => isActiveAndEnabled && resultScreen.IsOpen && !isShowingAd && !rewardGranted
            && dailyUsage != null && dailyUsage.Remaining(DailyLimit) > 0 && CanGrantBonus();

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

            StartCoroutine(PrepareForResult());
        }

        private IEnumerator PrepareForResult()
        {
            yield return new WaitUntil(() => resultScreen.IsOpen);

            dailyUsage = GameController.SaveManager.GetSave<StageRewardAdDailySave>(DailyUsageSaveKey);
            if (!RefreshDailyUsage()) UpdateDailyUsageDisplay();
            if (!AdsSupported) yield break;
            if (rewardGranted || isShowingAd) yield break;

            CaptureRewards();
            if (!CanGrantBonus()) yield break;

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

        private void Update()
        {
            if (dailyUsage == null || !resultScreen.IsOpen || Time.unscaledTime < nextDailyCheck) return;

            nextDailyCheck = Time.unscaledTime + 1f;
            if (RefreshDailyUsage() && AdsSupported && sdkInitialized) LoadAd();
        }

        private bool RefreshDailyUsage()
        {
            if (!dailyUsage.EnsureDay(DateTime.UtcNow, ResetUtcOffsetHours)) return false;

            UpdateDailyUsageDisplay();
            GameController.SaveManager.Save(false);
            return true;
        }

        private void UpdateDailyUsageDisplay()
        {
            if (dailyAdsRemaining != null)
                dailyAdsRemaining.text = $"{dailyUsage.Remaining(DailyLimit)}/{DailyLimit}";
        }

        private void CaptureRewards()
        {
            currencyBonuses.Clear();
            lootBonuses.Clear();
            bonusXp = StageController.ResultProfileXp;
            foreach (var entry in StageController.DropManager.ResultCurrencies)
                AddCurrencyBonus(entry.Key, entry.Value);

            foreach (var reward in StageController.DropManager.ResultItems)
            {
                if (reward.amount <= 0) continue;
                if (reward.kind == LuckyRewardKind.Currency)
                    AddCurrencyBonus(reward.currencyId, reward.amount);
                else
                    lootBonuses.Add(reward);
            }
        }

        private void AddCurrencyBonus(string id, int amount)
        {
            if (amount <= 0) return;
            currencyBonuses.TryGetValue(id, out long previous);
            currencyBonuses[id] = previous + amount;
        }

        private bool CanGrantBonus()
        {
            if (bonusXp <= 0 && currencyBonuses.Count == 0 && lootBonuses.Count == 0) return false;
            foreach (var entry in currencyBonuses)
            {
                var wallet = GameController.CurrenciesManager.GetCurrency(entry.Key, false);
                if (wallet == null || entry.Value > (long)int.MaxValue - wallet.Amount) return false;
            }
            return true;
        }

        private void GrantBonus()
        {
            var wallets = GameController.CurrenciesManager;
            var armory = GameController.ArmoryManager;
            var delivery = new LuckyRewardDelivery(armory, wallets);
            foreach (var entry in currencyBonuses)
                wallets.GetCurrency(entry.Key, false).Deposit((int)entry.Value);

            var equipmentAwarded = new Dictionary<LuckyGrant, int>();
            foreach (var reward in lootBonuses)
            {
                if (reward.kind == LuckyRewardKind.Equipment)
                {
                    // Each copy follows the usual unlock, duplicate upgrade, or full-bag queue rules.
                    int awarded = 0;
                    for (int i = 0; i < reward.amount; i++)
                    {
                        if (armory.IsItemUnlocked(reward.itemId))
                        {
                            if (!armory.IncreaseItemLevel(reward.itemId)) break;
                        }
                        else
                        {
                            armory.UnlockItem(reward.itemId);
                        }

                        awarded++;
                    }
                    equipmentAwarded.Add(reward, awarded);
                }
                else
                    delivery.Grant(reward);
            }

            int awardedXp = PlayerProgression.AwardResultBonusXp(bonusXp);
            GameController.AchievementsManager?.FlushPending();
            armory.Crafting?.FlushPending();
            GameController.SaveManager.Save(false);
            PlayerProgression.NotifyRunSettled(awardedXp);
            resultScreen.ShowDoubledRewards(awardedXp, equipmentAwarded);
        }

        private void LoadAd()
        {
            if (!CanOfferAd)
            {
                watchAdButton.interactable = false;
                return;
            }

            if (loadedAd != null)
            {
                watchAdButton.interactable = loadedAd.CanShowAd();
                if (watchAdButton.interactable) return;

                loadedAd.Destroy();
                loadedAd = null;
            }

            if (requestInProgress) return;

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
                        Debug.LogWarning($"[ArcBorn] Could not load x2 Rewards test ad: {error}");
                        return;
                    }

                    loadedAd = ad;
                    watchAdButton.interactable = CanOfferAd && ad.CanShowAd();
                }));
        }

        private void ShowAd()
        {
            if (!AdsSupported || dailyUsage == null) return;
            RefreshDailyUsage();
            if (!CanOfferAd) return;
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
                Debug.LogWarning($"[ArcBorn] Could not show x2 Rewards test ad: {error}");
                FinishAd();
            });

            ad.Show(_ => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (this == null || rewardGranted || !resultScreen.IsOpen) return;
                if (!CanGrantBonus())
                {
                    Debug.LogWarning("[ArcBorn] Cannot credit the result bonus because a reward balance would overflow.");
                    return;
                }

                dailyUsage.EnsureDay(DateTime.UtcNow, ResetUtcOffsetHours);
                if (!dailyUsage.TryRecordReward(DailyLimit)) return;

                rewardGranted = true;
                watchAdButton.interactable = false;
                UpdateDailyUsageDisplay();
                GrantBonus();
            }));
        }

        private void OnDestroy()
        {
            if (watchAdButton != null) watchAdButton.onClick.RemoveListener(ShowAd);
            loadedAd?.Destroy();
        }
    }

    [Serializable]
    internal sealed class StageRewardAdDailySave : ISave
    {
        [SerializeField] private int activeDay = -1;
        [SerializeField] private int rewardsGranted;

        public bool EnsureDay(DateTime utcNow, int utcOffsetHours)
        {
            int day = (int)(utcNow.AddHours(utcOffsetHours).Ticks / TimeSpan.TicksPerDay);
            if (day <= activeDay) return false;

            activeDay = day;
            rewardsGranted = 0;
            return true;
        }

        public int Remaining(int limit) => limit - Mathf.Clamp(rewardsGranted, 0, limit);

        public bool TryRecordReward(int limit)
        {
            int remaining = Remaining(limit);
            if (remaining == 0) return false;

            rewardsGranted = limit - remaining + 1;
            return true;
        }

        public void Flush() { }
    }
}
