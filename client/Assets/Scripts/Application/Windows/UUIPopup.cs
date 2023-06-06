using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIPopup
    {

        protected override void InitModel()
        {
            base.InitModel();
            ButtonBlue.onClick.AddListener(() => { Ok?.Invoke(); HideWindow(); });
            ButtonBrown.onClick.AddListener(() => { Cancel?.Invoke(); HideWindow(); });
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

        private Action Ok;
        private Action Cancel;

        public static void ShowConfirm(string title, string content, Action ok, Action cancel = null)
        {
            UUIManager.S.CreateWindowAsync<UUIPopup>(ui =>
            {
                ui.Ok = ok;
                ui.Cancel = cancel;
                ui.lb_conent.text = content;
                ui.lb_title.text = title;
                ui.ShowWindow();
            }, WRenderType.Notify);
        }
    }
}