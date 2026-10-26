using System;
using System.Collections;
using System.Collections.Generic;
using MrX.Crafting;
using MrX.Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class LuckyRewardsPopup : MonoBehaviour
    {
        [SerializeField] private LuckyRewardTable table;
        [SerializeField] private bool isWheel;
        [SerializeField] private TMP_Text heading, walletText, costText, status, resultTitle, resultAmount;
        [SerializeField] private Image resultIcon;
        [SerializeField] private Image costIcon;
        [SerializeField] private Button purchaseButton, closeButton, continueButton;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private RectTransform wheelDisc, chestIllustration;
        [SerializeField] private Transform prizeContent;
        [SerializeField] private LuckyPrizeView prizePrefab;
        [SerializeField] private LuckyWheelSlice slicePrefab;
        [SerializeField] private List<LuckyPrizeView> prizePool = new List<LuckyPrizeView>();
        [SerializeField] private List<LuckyWheelSlice> slicePool = new List<LuckyWheelSlice>();
        private readonly Dictionary<int, float> winningAngles = new Dictionary<int, float>();
        private LuckyRewardsService service;
        private CurrencySave gems;
        private Coroutine animationRoutine;
        private bool wired, animating, ownsDock;
        private long receiptSequence;
        private GameObject previousSelection;
        private float nextRefresh;
        [Header("Reveal animation")]
        [SerializeField, Min(0)] private float chestPulseAmplitude = .045f;
        [SerializeField, Min(0)] private float chestPulseCycles = 5f;
        private Vector3 chestBaseScale = Vector3.one;
        public LuckyRewardTable Table => table;
        private void Update()
        {
            if (service == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 1;
            Refresh();
        }
        private void Wire()
        {
            if (wired) return; wired = true;
            if (chestIllustration) chestBaseScale = chestIllustration.localScale;
            purchaseButton.onClick.AddListener(Purchase); closeButton.onClick.AddListener(Hide); continueButton.onClick.AddListener(Continue);
        }
        public void Show(LuckyRewardsService rewards)
        {
            Wire(); service = rewards; previousSelection = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            gameObject.SetActive(true); transform.SetAsLastSibling();
            // A standalone prefab Canvas becomes nested after placement in the page.
            GetComponent<Canvas>().overrideSorting = true;
            ownsDock = true; GameController.MainMenuScreenBehavior.DisableDock();
            gems = GameController.CurrenciesManager.GetCurrency("gem", false);
            if (gems != null) gems.OnAmountChanged += OnWallet;
            service.Changed += OnChanged; GameController.InputManager.InputAsset.UI.Cancel.performed += OnCancel;
            heading.text = table.title; resultPanel.SetActive(false); animating = false;
            BindPrizes(); Refresh();
            if (service.Save.HasUnseen) DisplayReceipt(service.Save.lastReceipt);
            Focus(resultPanel.activeSelf ? continueButton : purchaseButton.interactable ? purchaseButton : closeButton);
        }
        private void BindPrizes()
        {
            winningAngles.Clear(); int count = 0;
            if (!table.IsValid(out var total)) return;
            int activeCount = 0;
            foreach (var prize in table.prizes)
                if (prize != null && prize.enabled && prize.weight > 0) activeCount++;
            float visualFraction = 1f / activeCount;
            for (int i = 0; i < table.prizes.Count; i++)
            {
                var prize = table.prizes[i]; if (prize == null || !prize.enabled || prize.weight <= 0) continue;
                // Only authored prefab views are instantiated; all graphics/components live in assets.
                if (count >= prizePool.Count) prizePool.Add(Instantiate(prizePrefab, prizeContent));
                var row = prizePool[count]; row.gameObject.SetActive(true);
                var icon = PrizeIcon(prize); row.icon.sprite = icon;
                row.title.text = PrizeTitle(prize); row.amount.text = prize.minimumAmount == prize.maximumAmount ? "x " + prize.minimumAmount : "x " + prize.minimumAmount + " - " + prize.maximumAmount;
                float fraction = (float)(prize.weight / total);
                row.odds.text = (fraction * 100).ToString("0.##") + "%";
                // Visual sectors are equal. Only the reward table determines probability.
                float angle = count * visualFraction * 360;
                winningAngles[i] = angle + visualFraction * 180;
                if (isWheel)
                {
                    if (count >= slicePool.Count)
                    {
                        var view = Instantiate(slicePrefab, wheelDisc); slicePool.Add(view);
                    }
                    slicePool[count].gameObject.SetActive(true);
                    slicePool[count].Bind(angle, visualFraction, icon, "", count);
                }
                count++;
            }
            for (int i = count; i < prizePool.Count; i++) prizePool[i].gameObject.SetActive(false);
            for (int i = count; i < slicePool.Count; i++) slicePool[i].gameObject.SetActive(false);
            if (wheelDisc) wheelDisc.localRotation = Quaternion.identity;
        }
        private Sprite PrizeIcon(LuckyPrize prize)
        {
            if (prize.icon) return prize.icon;
            if (prize.kind == LuckyRewardKind.Currency) return GameController.CurrenciesManager.GetCurrency(prize.currencyId, false)?.Data?.Icon;
            if (prize.item) return prize.item.Icon;
            return GameController.ArmoryManager.Crafting?.Database.Material(prize.material)?.icon;
        }
        private static string PrizeTitle(LuckyPrize prize)
        {
            if (!string.IsNullOrWhiteSpace(prize.title)) return prize.title;
            if (prize.item) return prize.item.ItemName + (prize.kind == LuckyRewardKind.Fragments ? " fragments" : "");
            return prize.kind == LuckyRewardKind.Currency ? prize.currencyId.ToUpperInvariant() : prize.material.ToString();
        }
        private void Refresh()
        {
            if (service == null) return;
            walletText.text = gems != null ? gems.Amount.ToString("N0") : "0";
            bool free = service.HasDailyFreeUse(table);
            costText.text = (free ? "FREE" : (table.price == null ? "--" : table.price.Amount.ToString("N0")) + " GEM") + "\n" + (isWheel ? "SPIN" : "OPEN CHEST");
            if (costIcon != null) costIcon.enabled = !free;
            var state = service.State(table);
            purchaseButton.interactable = !animating && !resultPanel.activeSelf && state == LuckyPurchaseResult.Ready;
            purchaseButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = closeButton, selectOnRight = closeButton };
            closeButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = resultPanel.activeSelf ? continueButton : purchaseButton.interactable ? purchaseButton : closeButton };
            continueButton.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = closeButton };
            status.text = animating ? "Revealing your reward..." : state == LuckyPurchaseResult.InsufficientGems ? "Not enough gems." : state == LuckyPurchaseResult.Ready ? "One reward per purchase. Chances shown on the right." : state == LuckyPurchaseResult.PendingReward ? "Your previous reward has already been saved." : "Reward configuration unavailable.";
            if (!purchaseButton.interactable && EventSystem.current && EventSystem.current.currentSelectedGameObject == purchaseButton.gameObject) Focus(closeButton);
        }
        private void OnWallet(CurrencySave wallet) => Refresh();
        private void OnChanged() => Refresh();
        private void Purchase()
        {
            if (animating || resultPanel.activeSelf || service.State(table) != LuckyPurchaseResult.Ready) return;
            BindPrizes();
            animating = true; Refresh();
            LuckyReceipt receipt;
            try
            {
                var state = service.TryPurchase(table, UnityEngine.Random.value, UnityEngine.Random.value, out receipt);
                if (state != LuckyPurchaseResult.Success) { animating = false; Refresh(); return; }
            }
            catch
            {
                animating = false;
                if (service.Save.HasUnseen) DisplayReceipt(service.Save.lastReceipt);
                Refresh(); throw;
            }
            animationRoutine = StartCoroutine(Animate(receipt));
        }
        private IEnumerator Animate(LuckyReceipt receipt)
        {
            float elapsed = 0, duration = Mathf.Clamp(table.animationSeconds, .1f, 15);
            float from = wheelDisc ? wheelDisc.localEulerAngles.z : 0;
            float target = winningAngles.TryGetValue(receipt.prizeIndex, out var middle) ? middle : 0;
            // Clockwise sectors are authored from twelve o'clock. Rotating the
            // disc by the sector midpoint puts that exact reward under the pointer.
            float turn = from + 360 * 5 + Mathf.Repeat(target - from, 360);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; float t = Mathf.Clamp01(elapsed / duration); float eased = 1 - Mathf.Pow(1 - t, 4);
                if (isWheel && wheelDisc) wheelDisc.localEulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(from, turn, eased));
                else if (chestIllustration) chestIllustration.localScale = chestBaseScale * (1 + Mathf.Sin(t * Mathf.PI * 2 * chestPulseCycles) * chestPulseAmplitude * (1 - t));
                yield return null;
            }
            if (chestIllustration) chestIllustration.localScale = chestBaseScale;
            animating = false; animationRoutine = null; DisplayReceipt(receipt);
        }
        private void DisplayReceipt(LuckyReceipt receipt)
        {
            receiptSequence = receipt.sequence; resultPanel.SetActive(true);
            resultTitle.text = receipt.grant.title;
            resultAmount.text = "x " + receipt.grant.amount + (receipt.grant.queuedEquipment ? "\nStored safely. Expand your equipment bag to receive it." : "\nAdded to your inventory.");
            resultIcon.sprite = service.Delivery.Icon(receipt.grant); resultIcon.enabled = resultIcon.sprite != null;
            Refresh(); Focus(continueButton);
        }
        private void Continue()
        {
            if (!service.Acknowledge(receiptSequence)) return;
            resultPanel.SetActive(false); Refresh(); Focus(purchaseButton.interactable ? purchaseButton : closeButton);
        }
        public void Hide() { gameObject.SetActive(false); }
        private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext context) => Hide();
        private void OnDisable()
        {
            if (animationRoutine != null) { StopCoroutine(animationRoutine); animationRoutine = null; }
            if (wired && chestIllustration) chestIllustration.localScale = chestBaseScale;
            animating = false;
            if (gems != null) gems.OnAmountChanged -= OnWallet;
            if (service != null) service.Changed -= OnChanged;
            if (ownsDock)
            {
                GameController.InputManager.InputAsset.UI.Cancel.performed -= OnCancel;
                if (GameController.MainMenuScreenBehavior != null) GameController.MainMenuScreenBehavior.EnableDock(); ownsDock = false;
                if (EventSystem.current && previousSelection && previousSelection.activeInHierarchy) EventSystem.current.SetSelectedGameObject(previousSelection);
            }
        }
        private static void Focus(Button button) { if (EventSystem.current && button) EventSystem.current.SetSelectedGameObject(button.gameObject); }
    }
}
