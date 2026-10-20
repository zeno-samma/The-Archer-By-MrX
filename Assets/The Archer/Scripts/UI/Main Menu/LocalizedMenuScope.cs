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
        private void Start(){started=true;MenuLanguage.Initialize();Bind();}
        private void OnEnable(){if(started)Bind();}
        private void Bind()
        {
            foreach(var text in GetComponentsInChildren<TMP_Text>(true))if(!bindings.ContainsKey(text))bindings.Add(text,new Binding{label=text,font=text.font,source=text.text});
            MenuLanguage.Changed-=ApplyAll;MenuLanguage.Changed+=ApplyAll;
            Canvas.preWillRenderCanvases-=ApplyDirty;Canvas.preWillRenderCanvases+=ApplyDirty;ApplyAll();
        }
        private void ApplyAll(){foreach(var b in bindings.Values)Apply(b);}
        private void ApplyDirty()
        {
            foreach(var b in bindings.Values)
                if(b.label!=null&&(b.label.text!=b.rendered||b.label.font!=(MenuLanguage.Code!="en"&&b.rendered!=b.source&&MenuLanguage.Table!=null?MenuLanguage.Table.latinFont:b.font)))Apply(b);
        }
        private void Apply(Binding b)
        {
            if(b.label==null)return;if(b.label.text!=b.rendered)b.source=b.label.text;
            var translated=MenuLanguage.Text(b.source);b.rendered=translated;
            b.label.text=translated;
            b.label.font=MenuLanguage.Code!="en"&&translated!=b.source&&MenuLanguage.Table!=null&&MenuLanguage.Table.latinFont!=null?MenuLanguage.Table.latinFont:b.font;
        }
        private void OnDisable(){MenuLanguage.Changed-=ApplyAll;Canvas.preWillRenderCanvases-=ApplyDirty;}
    }
}
