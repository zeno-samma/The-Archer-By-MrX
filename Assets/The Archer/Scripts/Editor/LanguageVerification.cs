using System;
using System.Collections.Generic;
using MrX.Localization;
using MrX.Save;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class LanguageVerification
    {
        public static string Verify()
        {
            var table=Resources.Load<MenuTranslations>("Menu Translations");int n=0;
            Action<bool,string> check=(ok,msg)=>{if(!ok)throw new Exception(msg);n++;};
            check(MenuLanguage.Codes.Length==5&&Array.IndexOf(MenuLanguage.Codes,"en")>=0&&Array.IndexOf(MenuLanguage.Codes,"vi")>=0,"Five languages including EN and VI");
            foreach(var e in table.entries)foreach(var code in MenuLanguage.Codes)
            {
                check(!string.IsNullOrWhiteSpace(e.Get(code)),"Missing translation "+e.english+" "+code);
                if(e.english.Contains("{0}"))check(e.Get(code).Contains("{0}"),"Placeholder changed");
            }
            foreach(var code in MenuLanguage.Codes){check(table.Translate("Stage 4",code).Contains("4"),"Stage number preserved");check(table.Translate("1984",code)=="1984","Wallet amount preserved");}
            check(table.Translate("Unknown Item Name","vi")=="Unknown Item Name","Unknown content keeps source");
            check(table.Translate("START","vi")=="BẮT ĐẦU"&&table.Translate("Stage 3","vi")=="Màn 3","Vietnamese static/dynamic translation");
            return "Localization: "+n+" entries, placeholders, fallbacks and language checks passed.";
        }
    }
}
