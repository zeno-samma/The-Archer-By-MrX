using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class WavePanelBehavior : MonoBehaviour
    {
        [SerializeField] protected GameObject waveIndicatorPrefab;
        [SerializeField] protected RectTransform panelRect;
        [SerializeField] protected RectTransform maskRect;
        [SerializeField] protected AnimationCurve movementCurve;

        protected PoolComponent<WaveIndicatorBehavior> waveIndicatorPool;

        protected virtual void Awake()
        {
            waveIndicatorPool = new PoolComponent<WaveIndicatorBehavior>(waveIndicatorPrefab, 4, maskRect);

            transform.localScale = Vector3.zero;
        }

        public virtual void Animate()
        {
            StartCoroutine(AnimationCoroutine());
        }

        protected virtual IEnumerator AnimationCoroutine()
        {
            transform.localScale = Vector3.zero;
            transform.DoLocalScale(Vector3.one, 0.3f).SetEasing(EasingType.CubicOut);

            waveIndicatorPool.DisableAllEntities();

            var currentWave = StageController.RoomsShuffler.GetActiveWaveData();
            var prevWave = StageController.RoomsShuffler.GetPrevWaveData();
            var nextWave = StageController.RoomsShuffler.GetNextWaveData();
            var nextNextWave = StageController.RoomsShuffler.GetNextWaveData(nextWave);

            var currentIndex = StageController.RoomsShuffler.GetShuffledWaveIndex(currentWave);

            var currentWaveIndicator = waveIndicatorPool.GetEntity();
            currentWaveIndicator.RectTransform.anchoredPosition = Vector2.up * 5;
            currentWaveIndicator.transform.localScale = Vector3.one * 1.3f;
            currentWaveIndicator.Init(currentWave, currentIndex + 1);
            currentWaveIndicator.SetMaskable(false);
            currentWaveIndicator.ShowNumber(true);

            WaveIndicatorBehavior prevWaveIndicator = null;
            if (prevWave != null)
            {
                prevWaveIndicator = waveIndicatorPool.GetEntity();
                prevWaveIndicator.RectTransform.anchoredPosition = new Vector2(-100, 5);
                prevWaveIndicator.transform.localScale = Vector3.one * 1f;
                prevWaveIndicator.Init(prevWave, currentIndex);
                prevWaveIndicator.SetMaskable(true);
                prevWaveIndicator.HideNumber(true);
            }

            var nextWaveIndicator = waveIndicatorPool.GetEntity();
            nextWaveIndicator.RectTransform.anchoredPosition = new Vector2(100, 5);
            nextWaveIndicator.transform.localScale = Vector3.one;
            nextWaveIndicator.Init(nextWave, currentIndex + 2);
            nextWaveIndicator.SetMaskable(true);
            nextWaveIndicator.HideNumber(true);

            WaveIndicatorBehavior nextNextWaveIndicator = null;
            if (nextNextWave != null)
            {
                nextNextWaveIndicator = waveIndicatorPool.GetEntity();
                nextNextWaveIndicator.RectTransform.anchoredPosition = new Vector2(200, 5);
                nextNextWaveIndicator.transform.localScale = Vector3.one;
                nextNextWaveIndicator.Init(nextNextWave, currentIndex + 3);
                nextNextWaveIndicator.SetMaskable(true);
                nextNextWaveIndicator.HideNumber(true);
            }

            yield return new WaitForSeconds(0.5f);

            if (prevWaveIndicator != null) prevWaveIndicator.RectTransform.DoAnchorPosition(new Vector2(-200, 5), 0.6f).SetEasingCurve(movementCurve);
            currentWaveIndicator.RectTransform.DoAnchorPosition(new Vector2(-100, 5), 0.6f).SetEasingCurve(movementCurve);
            nextWaveIndicator.RectTransform.DoAnchorPosition(new Vector2(0, 5), 0.6f).SetEasingCurve(movementCurve);
            if (nextNextWaveIndicator != null) nextNextWaveIndicator.RectTransform.DoAnchorPosition(new Vector2(100, 5), 0.6f).SetEasingCurve(movementCurve);

            nextWaveIndicator.SetMaskable(false);

            yield return new WaitForSeconds(0.2f);

            currentWaveIndicator.RectTransform.DoLocalScale(Vector3.one, 0.2f).SetEasing(EasingType.CubicInOut);
            nextWaveIndicator.RectTransform.DoLocalScale(Vector3.one * 1.3f, 0.2f).SetEasing(EasingType.CubicInOut);

            nextWaveIndicator.ShowNumber(false);

            yield return new WaitForSeconds(0.2f);

            currentWaveIndicator.HideNumber(false);

            yield return new WaitForSeconds(0.7f);

            yield return transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.CubicIn);

            if (prevWaveIndicator != null) prevWaveIndicator.gameObject.SetActive(false);
            currentWaveIndicator.gameObject.SetActive(false);
            nextWaveIndicator.gameObject.SetActive(false);
            if (nextNextWaveIndicator != null) nextNextWaveIndicator.gameObject.SetActive(false);
        }
    }
}