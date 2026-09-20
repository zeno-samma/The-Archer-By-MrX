using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [CreateAssetMenu(fileName = "Currencies Database", menuName = "October/Currencies Database")]
    public class CurrenciesDatabase : ScriptableObject
    {
        [SerializeField] protected List<CurrencyData> currencies;

        public int Count => currencies.Count;

        public CurrencyData GetCurrency(string id)
        {
            for(int i = 0; i < currencies.Count; i++)
            {
                if (currencies[i].ID == id) return currencies[i];
            }

            return null;
        }

        public CurrencyData GetCurrency(int i)
        {
            if(i < 0 || i >= currencies.Count) return null;

            return currencies[i];
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorPrefs.SetBool("CurrencyCache initialized", false);
#endif
        }
    }
}