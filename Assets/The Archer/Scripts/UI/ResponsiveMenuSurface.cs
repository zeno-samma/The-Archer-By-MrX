using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace MrX.UI
{
    // Fits an authored landscape or portrait design to the current safe area.
    // This only changes RectTransforms; it never constructs UI at runtime.
    [DefaultExecutionOrder(-90)]
    public sealed class ResponsiveMenuSurface : MonoBehaviour
    {
        [SerializeField] private Vector2 landscapeSize = new Vector2(3840,2160);
        [SerializeField] private Vector2 portraitSize = new Vector2(1080,2160);
        private RectTransform root, canvasRect;
        private Vector2 lastPixels, lastCanvas;
        private Rect lastSafe;
        public bool IsPortrait { get; private set; }
        private void Awake() { root=(RectTransform)transform; canvasRect=(RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform; Apply(); }
        private void Update()
        {
            if (lastPixels!=new Vector2(Screen.width,Screen.height)||lastSafe!=Screen.safeArea||lastCanvas!=canvasRect.rect.size)Apply();
        }
        public void Apply()
        {
            if (root==null) return;
            var pixels=new Vector2(Screen.width,Screen.height); var safe=Screen.safeArea;
            if(pixels.x<=0||pixels.y<=0||canvasRect.rect.width<=0||canvasRect.rect.height<=0)return;
            lastPixels=pixels;lastSafe=safe;lastCanvas=canvasRect.rect.size;
            if(safe.width<=0||safe.height<=0)safe=new Rect(Vector2.zero,pixels);
            IsPortrait=pixels.y>pixels.x;var design=IsPortrait?portraitSize:landscapeSize;
            var safeSize=Vector2.Scale(new Vector2(safe.width/pixels.x,safe.height/pixels.y),lastCanvas);
            var center=Vector2.Scale(new Vector2(safe.center.x/pixels.x-.5f,safe.center.y/pixels.y-.5f),lastCanvas);
            root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.sizeDelta=design;root.anchoredPosition=center;
            root.localScale=Vector3.one*Mathf.Min(safeSize.x/design.x,safeSize.y/design.y);
        }
    }
}
