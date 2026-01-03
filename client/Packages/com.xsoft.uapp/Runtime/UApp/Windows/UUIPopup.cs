using System;
using App.Core.UICore.Utility;

namespace Windows
{
    internal partial class UUIPopup
    {
        private Action _cancel;

        private Action _ok;

        protected override void InitModel()
        {
            base.InitModel();
            ButtonBlue.onClick.AddListener(() =>
            {
                _ok?.Invoke();
                HideWindow();
            });
            ButtonBrown.onClick.AddListener(() =>
            {
                _cancel?.Invoke();
                HideWindow();
            });
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

        public static async void ShowConfirm(string title, string content, Action ok, Action cancel = default,
            bool onlyOk = false)
        {
            var ui = await UUIManager.S.CreateWindowAsync<UUIPopup>(ui =>
            {
                ui._ok = ok;
                ui._cancel = cancel;
                ui.lb_conent.text = content;
                ui.lb_title.text = title;
                ui.ShowWindow();
            }, WRenderType.Notify);

            ui.ButtonBrown.ActiveSelfObject(!onlyOk);
        }
    }
}