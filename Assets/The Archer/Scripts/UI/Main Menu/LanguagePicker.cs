using System;
using MrX.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MrX.UI
{
    public sealed class LanguagePicker : MonoBehaviour
    {
        [SerializeField] private Button openButton,closeButton;
        [SerializeField] private Button[] choices;
        [SerializeField] private TMP_Text currentLabel;
        [SerializeField] private GameObject panel;
        [SerializeField] private Color selectedColor = new Color(.55f,.82f,1);
        [SerializeField] private Color idleColor = Color.white;
        private bool ownsDock;private GameObject previous;
        private void Start()
        {
            MenuLanguage.Initialize();openButton.onClick.AddListener(Open);closeButton.onClick.AddListener(Close);
            for(int i=0;i<choices.Length;i++){int index=i;choices[i].onClick.AddListener(()=>{MenuLanguage.Select(MenuLanguage.Codes[index]);Refresh();Close();});}
            MenuLanguage.Changed+=Refresh;Refresh();
        }
        public void Open()
        {
            var menu=GameController.MainMenuScreenBehavior;if(menu==null||!menu.IsDockEnabled||menu.CurrentPageType!=MainMenuPageType.Stages||panel.activeSelf)return;
            previous=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;ownsDock=true;menu.DisableDock();panel.SetActive(true);
            (menu.CurrentPage as MainMenuStagesPageBehavior)?.SetLanguageModalInput(true);
            GameController.InputManager.InputAsset.UI.Cancel.performed+=Cancel;
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(choices[Mathf.Max(0,Array.IndexOf(MenuLanguage.Codes,MenuLanguage.Code))].gameObject);
        }
        public void Close()
        {
            if(panel!=null)panel.SetActive(false);if(!ownsDock)return;ownsDock=false;
            if(GameController.InputManager!=null)GameController.InputManager.InputAsset.UI.Cancel.performed-=Cancel;
            if(GameController.MainMenuScreenBehavior!=null)GameController.MainMenuScreenBehavior.EnableDock();
            if(GameController.MainMenuScreenBehavior!=null)(GameController.MainMenuScreenBehavior.CurrentPage as MainMenuStagesPageBehavior)?.SetLanguageModalInput(false);
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(previous!=null&&previous.activeInHierarchy?previous:openButton.gameObject);
        }
        private void Cancel(InputAction.CallbackContext _){Close();}
        private void Refresh(){currentLabel.text="LANGUAGE · "+MenuLanguage.Code.ToUpperInvariant();for(int i=0;i<choices.Length;i++)choices[i].image.color=MenuLanguage.Codes[i]==MenuLanguage.Code?selectedColor:idleColor;}
        private void OnDisable(){Close();}
        private void OnDestroy(){MenuLanguage.Changed-=Refresh;}
    }
}
