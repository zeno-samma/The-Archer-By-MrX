using OctoberStudio.Currency;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class CostButtonBehavior : MonoBehaviour
    {
        [SerializeField] protected Button button;
        [SerializeField] protected Sprite activeButtonSprite;
        [SerializeField] protected Sprite disabledButtonSprite;

        [Space]
        [SerializeField] protected CurrencyId currency = "gold";
        [SerializeField] protected bool useTempGameplayCurrency = false;
        [SerializeField] protected ScalingLabelBehavior costLabel;

        public RectTransform RectTransform { get; protected set; }
        public Selectable Selectable => button;

        public bool IsActive { get; protected set; }
        public bool ButtonEnabled { get => button.enabled; set => button.enabled = value; }

        public bool HasEnoughMoney => HasEnough();

        public Button Button => button;

        public CurrencySave Currency { get; protected set; }

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Start()
        {
            if (Currency != null) return;

            Currency = GameController.CurrenciesManager.GetCurrency(currency, useTempGameplayCurrency);

            if (Currency == null)
            {
                Debug.LogError($"Could not find {currency} in currencies database");
                return;
            }

            costLabel.SetSprite(Currency.Data.Icon);

            Currency.OnAmountChanged -= OnCurrencyAmountChanged;
            Currency.OnAmountChanged += OnCurrencyAmountChanged;
        }

        protected virtual void OnEnable()
        {
            if(Currency != null)
            {
                Currency.OnAmountChanged -= OnCurrencyAmountChanged;
                Currency.OnAmountChanged += OnCurrencyAmountChanged;
            }
        }

        protected virtual void OnDisable()
        {
            if (Currency != null)
            {
                Currency.OnAmountChanged -= OnCurrencyAmountChanged;
            }
        }

        protected virtual bool HasEnough()
        {
            if (Currency == null) return false;

            return Currency.Amount >= costLabel.Amount;
        }

        public virtual bool WithdrawCost()
        {
            if (Currency == null) return false;
            if(Currency.Amount >= costLabel.Amount)
            {
                Currency.Withdraw(costLabel.Amount);
                return true;
            }

            return false;
        }

        public virtual void SetOnClick(UnityAction onClick)
        {
            button.onClick.AddListener(onClick);
        }

        public virtual void RemoveOnClick(UnityAction onClick)
        {
            button.onClick.RemoveListener(onClick);
        }

        public virtual void SetCost(int cost)
        {
            costLabel.SetAmount(cost);
            RecalculateVisuals();
        }

        public virtual void SetCurrency(CurrencySave currency)
        {
            if(Currency != null)
            {
                Currency.OnAmountChanged -= OnCurrencyAmountChanged;
            }

            Currency = currency;

            costLabel.SetSprite(Currency.Data.Icon);

            Currency.OnAmountChanged -= OnCurrencyAmountChanged;
            Currency.OnAmountChanged += OnCurrencyAmountChanged;

            RecalculateVisuals();
        }

        protected virtual void OnCurrencyAmountChanged(CurrencySave currency)
        {
            RecalculateVisuals();
        }

        public virtual void RecalculateVisuals()
        {
            if (Currency == null) return;

            var currencyAmount = Currency.Amount;
            ButtonEnabled = currencyAmount >= costLabel.Amount;
            button.image.sprite = ButtonEnabled ? activeButtonSprite : disabledButtonSprite;
        }

        public virtual void DisableButton()
        {
            ButtonEnabled = false;
            button.image.sprite = ButtonEnabled ? activeButtonSprite : disabledButtonSprite;
        }
    }
}