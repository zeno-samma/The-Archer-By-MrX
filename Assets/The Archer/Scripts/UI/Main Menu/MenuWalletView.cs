using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    // Prefab supplies every graphic and layout value. Only wallet binding lives here.
    public sealed class MenuWalletView : MonoBehaviour
    {
        [SerializeField] private string currencyId = "gem";
        [SerializeField] private TMP_Text amount;
        [SerializeField] private Image icon;
        private CurrencySave wallet;
        private bool started;
        private void Start() { started = true; Bind(); }
        private void OnEnable() { if (started) Bind(); }
        private void Bind()
        {
            Unbind(); wallet = GameController.CurrenciesManager?.GetCurrency(currencyId, false);
            if (wallet == null) return;
            icon.sprite = wallet.Data.Icon; Refresh(wallet); wallet.OnAmountChanged += Refresh;
        }
        private void Refresh(CurrencySave value) { amount.text = value.Amount.ToString("N0"); }
        private void Unbind() { if (wallet != null) wallet.OnAmountChanged -= Refresh; wallet = null; }
        private void OnDisable() { Unbind(); }
    }
}
