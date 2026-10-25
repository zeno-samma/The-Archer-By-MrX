using System;
using System.Collections.Generic;
using System.Text;
using MrX.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MrX.EditorTools
{
    public static class RepMenuInputVerification
    {
        [MenuItem("Tools/MrX Studio/UI/Verify Menu Pointer Targets (Play Mode)")]
        public static void Run() { Debug.Log(VerifyDock()); }

        // Exercise actual GraphicRaycaster results, without bypassing hit testing through onClick.Invoke.
        // Read-only: this does not change page, rewards, currency, selection or the player's save.
        public static string VerifyDock()
        {
            if(!Application.isPlaying || EventSystem.current==null)throw new InvalidOperationException("Run from Main Menu in Play Mode.");
            var screen=GameController.MainMenuScreenBehavior;
            if(screen==null || !screen.IsDockEnabled)throw new InvalidOperationException("Close menu popups before verifying dock hit regions.");
            var dock=UnityEngine.Object.FindFirstObjectByType<MainMenuDockBehavior>();
            if(dock==null)throw new InvalidOperationException("Main menu dock is missing.");
            var report=new StringBuilder();int tested=0;
            foreach(var tab in dock.GetComponentsInChildren<MainMenuDockItemBehavior>(false))
            {
                var rect=(RectTransform)tab.transform;
                foreach(var sample in new[]{new Vector2(.5f,.66f),new Vector2(.5f,.14f),new Vector2(.10f,.42f)})
                {
                    var local=new Vector2(rect.rect.xMin+rect.rect.width*sample.x,rect.rect.yMin+rect.rect.height*sample.y);
                    var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(local))};
                    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
                    var target=hits.Count>0?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject):null;
                    if(target!=tab.gameObject)throw new InvalidOperationException(tab.PageType+" at "+sample+" blocked by "+(hits.Count>0?hits[0].gameObject.name:"no raycast target"));
                    tested++;report.AppendLine(tab.PageType+" "+sample+" PASS");
                }
            }
            if(tested!=18)throw new InvalidOperationException("Expected 18 samples over six dock tabs, got "+tested);
            return "Menu pointer verification: 18 / 18 PASS\n"+report;
        }
    }
}
