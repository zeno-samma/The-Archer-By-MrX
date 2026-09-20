using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Currency
{
    /// <summary>
    /// Interface for extending the asset with a custom currency system.
    /// The default implementation is <see cref="CurrenciesManager"/>.
    /// </summary>
    public interface ICurrenciesManager
    {
        /// <summary>
        /// Gets the currency save.
        /// Default implementation: <see cref="CurrenciesManager.GetCurrency(string, bool)"/>.
        /// </summary>
        /// <param name="currencyId">Unique identifier of the currency.</param>
        /// <param name="gameplay">If true - gets stable currency, if false - gets temporary gameplay currency</param>
        CurrencySave GetCurrency(string currencyId, bool gameplay);

        /// <summary>
        /// Gets the first currency save.
        /// Default implementation: <see cref="CurrenciesManager.GetDefaultCurrency bool)"/>.
        /// </summary>
        /// <param name="gameplay">If true - gets stable currency, if false - gets temporary gameplay currency</param>
        CurrencySave GetDefaultCurrency(bool gameplay);

        /// <summary>
        /// Adds amount earned during gameplay to the actual amount.
        /// Default implementation: <see cref="CurrenciesManager.ApplyGameplayAmounts()"/>.
        /// </summary>
        void ApplyGameplayAmounts();

        /// <summary>
        /// Erases amount earned during gameplay.
        /// Default implementation: <see cref="CurrenciesManager.EraseGameplayAmounts()"/>.
        /// </summary>
        void EraseGameplayAmounts();

        /// <summary>
        /// Gets a list of all currencies.
        /// Default implementation: <see cref="CurrenciesManager.GetAllCurrencies(bool)"/>.
        /// <param name="gameplay">If true - gets stable currencies, if false - gets temporary gameplay currencies</param>
        /// </summary>
        List<CurrencySave> GetAllCurrencies(bool gameplay);
    }
}