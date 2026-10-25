using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace MrX.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class MenuSettingsShortcut : MonoBehaviour
    {
        private Button button;
        private MainMenuScreenBehavior screen;
        private GameObject previousSelection;
        private bool open;
        private void Awake(){button=GetComponent<Button>();button.onClick.AddListener(Open);}
        private void Open()
        {
            screen=GameController.MainMenuScreenBehavior;
            if(screen==null||!screen.IsDockEnabled||open)return;
            previousSelection=EventSystem.current!=null?EventSystem.current.currentSelectedGameObject:null;
            open=true;screen.DisableDock();screen.SettingsPopup.onPopupClosed+=Closed;screen.SettingsPopup.Show();
        }
        private void Closed()
        {
            if(screen!=null){screen.SettingsPopup.onPopupClosed-=Closed;screen.EnableDock();}
            open=false;
            if(previousSelection!=null&&previousSelection.activeInHierarchy&&EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(previousSelection);
        }
        private void OnDestroy(){if(button!=null)button.onClick.RemoveListener(Open);if(open&&screen!=null)screen.SettingsPopup.onPopupClosed-=Closed;}
    }
}
