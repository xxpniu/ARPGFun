using App.Core.Core;
using Cysharp.Threading.Tasks;
using Proto;
using UApp;
using UApp.GameGates;
using UnityEngine;

namespace Windows
{
    partial class UUISignup
    {

        protected override void InitModel()
        {
            base.InitModel();

            TextSignin.onClick.AddListener(HideWindow);
            ButtonClose.onClick.AddListener(HideWindow);

            ButtonBlue.onClick.AddListener(RegCall);

            return;

            async void RegCall()
            {
                var userName = TextInputBoxEmail.text;
                var pwd = TextInputBoxPassword.text;
                var rPwd = TextInputBoxPasswordRepeat.text;
                if (pwd != rPwd)
                {
                    UApplication.S.ShowNotify("Password not same!");
                    return;
                }

                UUIManager.S.MaskEvent();
                var gate = UApplication.G<LoginGate>();
                var r = await LoginGate.DoReg(userName, pwd);
                await UniTask.SwitchToMainThread();
                UUIManager.S.UnMaskEvent();
                if (r.Code.IsOk())
                {
                    UApplication.Singleton.GoServerMainGate(r.ChatServer, r.GateServer, r.UserID, r.Session);
                    HideWindow();
                    PlayerPrefs.SetString(UUILogin.UserNameKey, userName);
                    PlayerPrefs.SetString(UUILogin.PasswordKey, pwd);
                }
                else
                {
                    UUITipDrawer.Singleton.ShowNotify("Server Response:" + r.Code);
                }
                
            }
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