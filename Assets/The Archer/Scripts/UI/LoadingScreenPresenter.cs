using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    // Presentation only: every graphic, label and rect is authored in Loading Screen.prefab.
    [ExecuteAlways]
    public sealed class LoadingScreenPresenter : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private RectTransform progressCursor;
        [SerializeField] private TMP_Text percentageText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private RectTransform safeContent;
        [SerializeField,Range(0,1)] private float previewProgress=.68f;
        private UnityEngine.Rect previousSafeArea;
        private Vector2 previousSize;
        private float value;

        private void OnEnable()
        {
            if(Application.isPlaying){SceneLoadingProgress.Changed+=ShowProgress;MenuLanguage.Changed+=RefreshLanguage;}
            ShowProgress(Application.isPlaying?SceneLoadingProgress.Value:previewProgress);
            ApplySafeArea();
        }
        private void OnDisable()
        {
            SceneLoadingProgress.Changed-=ShowProgress;MenuLanguage.Changed-=RefreshLanguage;
        }
        private void LateUpdate()
        {
            if(previousSafeArea!=Screen.safeArea||previousSize!=new Vector2(Screen.width,Screen.height))ApplySafeArea();
        }
        private void OnValidate()
        {
            if(!Application.isPlaying)ShowProgress(previewProgress);
        }
        public void ShowProgress(float progress)
        {
            value=float.IsNaN(progress)||float.IsInfinity(progress)?0:Mathf.Clamp01(progress);
            if(progressBar!=null)progressBar.SetValueWithoutNotify(value);
            if(progressCursor!=null)
            {
                progressCursor.anchorMin=progressCursor.anchorMax=new Vector2(value,.5f);
                progressCursor.anchoredPosition=Vector2.zero;
            }
            if(percentageText!=null)percentageText.text=Mathf.FloorToInt(value*100+.001f)+"%";
            RefreshLanguage();
        }
        private void RefreshLanguage()
        {
            if(statusText!=null)statusText.text=MenuLanguage.Text("Loading data...");
        }
        private void ApplySafeArea()
        {
            previousSafeArea=Screen.safeArea;previousSize=new Vector2(Screen.width,Screen.height);
            if(safeContent==null||Screen.width<=0||Screen.height<=0)return;
            safeContent.anchorMin=new Vector2(previousSafeArea.xMin/Screen.width,previousSafeArea.yMin/Screen.height);
            safeContent.anchorMax=new Vector2(previousSafeArea.xMax/Screen.width,previousSafeArea.yMax/Screen.height);
            safeContent.offsetMin=safeContent.offsetMax=Vector2.zero;
        }
    }
}
