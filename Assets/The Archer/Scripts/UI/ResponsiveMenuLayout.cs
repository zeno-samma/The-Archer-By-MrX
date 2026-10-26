using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class ResponsiveMenuLayout : MonoBehaviour
    {
        [Serializable] public class Placement
        {
            public Vector2 min,max,offsetMin,offsetMax,pivot;
            public Vector3 scale=Vector3.one;
            public static Placement Capture(RectTransform r) {return new Placement{min=r.anchorMin,max=r.anchorMax,offsetMin=r.offsetMin,offsetMax=r.offsetMax,pivot=r.pivot,scale=r.localScale};}
            public void Apply(RectTransform r){r.pivot=pivot;r.anchorMin=min;r.anchorMax=max;r.offsetMin=offsetMin;r.offsetMax=offsetMax;r.localScale=scale;}
        }
        [Serializable] public class RectStyle {public RectTransform target;public Placement landscape,portrait;}
        [Serializable] public class TextStyle {public TMP_Text target;public float landscape,portrait;public bool autoSize;}
        [Serializable] public class RowStyle {public LayoutElement target;public float landscape,portrait;}
        [Serializable] public class Visibility {public GameObject target;public bool landscape=true,portrait=true;}
        [Serializable] public class GridStyle {public GridLayoutGroup target;public Vector2 landscapeSize,portraitSize;public RectOffset landscapePadding,portraitPadding;}
        [Serializable] public class ImageStyle {public Image target;public float landscape=3,portrait=6;}
        public List<RectStyle> rectangles=new List<RectStyle>();
        public List<TextStyle> texts=new List<TextStyle>();
        public List<RowStyle> rows=new List<RowStyle>();
        public List<Visibility> visibility=new List<Visibility>();
        public List<GridStyle> grids=new List<GridStyle>();
        public List<ImageStyle> images=new List<ImageStyle>();
        [HideInInspector] public bool editorPortrait;
        [HideInInspector] public bool captureInspectorEdits = true;
        [SerializeField] private ArmoryDashboard inventory;
        private ResponsiveMenuSurface surface;
        private bool initialized,lastPortrait;
        private void OnEnable(){initialized=false;surface=GetComponentInParent<ResponsiveMenuSurface>();}
        private void OnTransformParentChanged(){surface=GetComponentInParent<ResponsiveMenuSurface>();initialized=false;}
        private void LateUpdate()
        {
            if(surface!=null&&(!initialized||surface.IsPortrait!=lastPortrait))Apply(surface.IsPortrait);
        }
        public void Apply(bool portrait)
        {
            initialized=true;lastPortrait=portrait;
            foreach(var style in rectangles)if(style.target!=null)(portrait?style.portrait:style.landscape).Apply(style.target);
            foreach(var style in texts)if(style.target!=null){float size=portrait?style.portrait:style.landscape;style.target.fontSize=style.target.fontSizeMax=size;style.target.fontSizeMin=size*.6f;style.target.enableAutoSizing=style.autoSize;}
            foreach(var style in rows)if(style.target!=null)style.target.preferredHeight=portrait?style.portrait:style.landscape;
            foreach(var style in visibility)if(style.target!=null)style.target.SetActive(portrait?style.portrait:style.landscape);
            foreach(var style in grids)if(style.target!=null){style.target.cellSize=portrait?style.portraitSize:style.landscapeSize;style.target.padding=portrait?style.portraitPadding:style.landscapePadding;}
            foreach(var style in images)if(style.target!=null)style.target.pixelsPerUnitMultiplier=portrait?style.portrait:style.landscape;
            if(inventory!=null)inventory.SetColumns(portrait?4:6);
            foreach(var panel in GetComponentsInChildren<Armory.ArmoryHeroPanel>(true)) panel.RefreshSlotLayout();
        }
    }
}
