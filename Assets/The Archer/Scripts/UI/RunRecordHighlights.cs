using MrX.Localization;
using TMPro;
using UnityEngine;

namespace MrX.UI
{
    // Authored cards only; runs while the result screen has paused gameplay.
    public sealed class RunRecordHighlights : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeTitle, timeValue, timeBest, timeBadge;
        [SerializeField] private TMP_Text dpsTitle, dpsValue, dpsBest, dpsBadge, damageTotal;
        [SerializeField, Min(.1f)] private float countDuration = 2.4f;
        [SerializeField] private Color recordColor = new Color(.95f, .52f, .05f);
        private RunCombatStatistics stats;
        private bool animating;
        private float elapsed, nextDraw;
        public static string FormatTime(double seconds) => UITextEffect.FormatTime(seconds);
        private static string T(string source) => MenuLanguage.Text(source);
        private static string F(string source, string value) => string.Format(T(source), value);
        public void Play(RunCombatStatistics value, bool victory)
        {
            bool same = ReferenceEquals(stats, value);
            stats = value;
            if (stats == null) return;
            timeTitle.text = T(victory ? "CLEAR TIME" : "TIME SURVIVED");
            dpsTitle.text = T("AVERAGE DPS");
            timeBest.text = stats.previousBestTime > 0 ? F("Previous best clear: {0}", FormatTime(stats.previousBestTime)) : T(victory ? "Your first clear record" : "Complete this stage to set a time record.");
            dpsBest.text = stats.previousBestDps > 0 ? F("Previous best DPS: {0}", stats.previousBestDps.ToString("#,0.##")) : T("No previous DPS record");
            damageTotal.text = F("Total damage: {0}", stats.damageDealt.ToString("#,0.##"));
            timeBadge.color = dpsBadge.color = recordColor;
            elapsed = nextDraw = 0;
            animating = !same;
            Draw(same ? 1 : 0);
        }
        private void Update()
        {
            if (!animating || stats == null) return;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < nextDraw && elapsed < countDuration) return;
            nextDraw = elapsed + 1f / 30;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.1f, countDuration));
            Draw(1 - Mathf.Pow(1 - t, 3));
            if (t >= 1) animating = false;
        }
        private void Draw(float t)
        {
            timeValue.text = FormatTime(stats.seconds * (double)t);
            dpsValue.text = (stats.Dps * t).ToString("#,0.##");
            timeBadge.text = t >= 1 && stats.newTimeRecord ? T("NEW RECORD!") : "";
            dpsBadge.text = t >= 1 && stats.newDpsRecord ? T("NEW RECORD!") : "";
        }
    }
}
