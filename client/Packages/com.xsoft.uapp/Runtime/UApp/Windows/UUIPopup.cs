using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.Core.UICore.Utility;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIPopup
    {

        protected override void InitModel()
        {
            base.InitModel();
            ButtonBlue.onClick.AddListener(() => { _ok?.Invoke(); HideWindow(); });
            ButtonBrown.onClick.AddListener(() => { _cancel?.Invoke(); HideWindow(); });
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            ButtonBlue.SetKey("UUIPopup_OK");
            ButtonBrown.SetKey("UUIPopup_Cancel");
        }
        protected override void OnHide()
        {
            base.OnHide();
        }

        private Action _ok;
        private Action _cancel;

        public static async void ShowConfirm(string title, string content, Action ok, Action cancel = null)
        {
            var ui = await UUIManager.S.CreateWindowAsync<UUIPopup>(ui =>
            {
                ui._ok = ok;
                ui._cancel = cancel;
                ui.lb_conent.text = content;
                ui.lb_title.text = title;
                ui.ShowWindow();
            }, WRenderType.Notify);
            
            ui.ButtonBrown.ActiveSelfObject(cancel!=null);
        }
    }
}