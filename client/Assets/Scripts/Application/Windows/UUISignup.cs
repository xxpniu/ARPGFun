using Proto;
using UnityEngine;

namespace Windows
{
    partial class UUISignup
    {

        protected override void InitModel()
        {
            base.InitModel();

            TextSignin.onClick.AddListener(() => { HideWindow(); });

            ButtonClose.onClick.AddListener(() => { HideWindow(); });
            ButtonBlue.onClick.AddListener(() =>
            {
                var userName = TextInputBoxEmail.text;
                var pwd = TextInputBoxPassword.text;
                var rpwd = TextInputBoxPasswordRepeat.text;
                if (pwd != rpwd)
                {
                    UApplication.S.ShowNotify("Password not same!");
                    return;
                }

                UUIManager.S.MaskEvent();
                var gate = UApplication.G<LoginGate>();
                gate.GoReg(userName, pwd, (r) =>
                {
                    UUIManager.S.UnMaskEvent();
                    if (r.Code == ErrorCode.Ok)
                    {
                        UApplication.Singleton.GoServerMainGate(r.ChatServer, r.GateServer, r.UserID, r.Session);
                        HideWindow();
                        PlayerPrefs.SetString(UUILogin. UserNameKey, userName);
                        PlayerPrefs.SetString(UUILogin.PasswordKey, pwd);
                    }
                    else
                    {
                        UUITipDrawer.Singleton.ShowNotify("Server Response:" + r.Code);
                    }
                });
            });
        }

        protected override void OnShow()
        {
            base.OnShow();
        }
        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}