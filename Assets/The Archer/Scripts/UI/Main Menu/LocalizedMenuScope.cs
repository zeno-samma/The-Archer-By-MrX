using System.Collections.Generic;
using MrX.Localization;
using TMPro;
using UnityEngine;

namespace MrX.UI
{
    public sealed class LocalizedMenuScope : MonoBehaviour
    {
        private sealed class Binding { public TMP_Text label; public TMP_FontAsset font; public string source, rendered; }
        private readonly Dictionary<TMP_Text,Binding> bindings=new Dictionary<TMP_Text,Binding>();
        private bool started;
        private int lastRenderFrame = -1;
        // Pooled cards call this after assigning their English source text, before measuring TMP.
        public void RefreshNow(){MenuLanguage.Initialize();Bind();}
        private void Start(){started=true;MenuLanguage.Initialize();Bind();}
        private void OnEnable(){if(started)Bind();}
        private void Bind()
        {
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))
                if(text.GetComponentInParent<LocalizedMenuScope>(true)==this&&!bindings.ContainsKey(text))
                    bindings.Add(text,new Binding{label=text,font=text.font,source=text.text});
            MenuLanguage.Changed-=ApplyAll;
            Canvas.preWillRenderCanvases-=ApplyDirty;
            if(isActiveAndEnabled){MenuLanguage.Changed+=ApplyAll;Canvas.preWillRenderCanvases+=ApplyDirty;}
            ApplyAll();
        }
        private void ApplyAll(){foreach(var b in bindings.Values)Apply(b);}
        private void ApplyDirty()
        {
            if(lastRenderFrame==Time.frameCount)return;
            lastRenderFrame=Time.frameCount;
            foreach(var b in bindings.Values)
                if(b.label!=null&&(b.label.text!=b.rendered||b.label.font!=ExpectedFont(b)))Apply(b);
        }
        private TMP_FontAsset ExpectedFont(Binding b)
        {
            var table=MenuLanguage.Table;
            return MenuLanguage.Code!="en"&&b.rendered!=b.source&&table!=null&&table.latinFont!=null?table.latinFont:b.font;
        }
        private void Apply(Binding b)
        {
            if(b.label==null)return;if(b.label.text!=b.rendered)b.source=b.label.text;
            var translated=MenuLanguage.Text(b.source);b.rendered=translated;
            if(b.label.text!=translated)b.label.text=translated;
            var font=ExpectedFont(b);if(b.label.font!=font)b.label.font=font;
        }
        private void OnDisable(){MenuLanguage.Changed-=ApplyAll;Canvas.preWillRenderCanvases-=ApplyDirty;}
    }
}
