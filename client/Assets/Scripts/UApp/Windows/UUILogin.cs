using Core;
using UApp;
using UApp.GameGates;
using UGameTools;
using UnityEngine;
using XNet.Libs.Utility;

namespace Windows
{
    partial class UUILogin
    {

        public const string UserNameKey = "KEY_NAME";
        public const string PasswordKey = "Key_Password";

        protected override void InitModel()
        {
            base.InitModel();

            async void LoginCall()
            {
                var userName = TextInputBoxUserName.text;
                var pwd = TextInputBoxPassWord.text;
                var gate = UApplication.G<LoginGate>();
                if (gate == null) return;
                if (CheckBox.isOn)
                {
                    PlayerPrefs.SetString(UserNameKey, userName);
                    PlayerPrefs.SetString(PasswordKey, pwd);
                }
                else
                {
                    PlayerPrefs.DeleteKey(UserNameKey);
                    PlayerPrefs.DeleteKey(PasswordKey);
                }

                UUIManager.S.MaskEvent();

                var md5 = Md5Tool.GetMd5Hash(pwd);
                var r = await gate.DoLogin(userName, md5);
                UUIManager.S.UnMaskEvent();
                if (r.Code.IsOk())
                {
                    UApplication.S.GoServerMainGate(r.ChatServer, r.GateServer, r.UserID, r.Session);
                }
                else
                {
                    UApplication.S.ShowError(r.Code);
                }
            }

            this.ButtonBlue.onClick.AddListener(LoginCall);

            async  void SignupCall()
            {
                await UUIManager.S.CreateWindowAsync<UUISignup>(ui => ui.ShowWindow());
            }

            TextSignup.onClick.AddListener(SignupCall);
            ButtonClose.onClick.AddListener(() =>
            {
                //do nothing
            });

        }

        protected override void OnShow()
        {
            base.OnShow();

            TextInputBoxUserName.text = PlayerPrefs.GetString(UserNameKey);
            TextInputBoxPassWord.text = PlayerPrefs.GetString(PasswordKey);
            CheckBox.isOn = !string.IsNullOrEmpty(TextInputBoxUserName.text);
            lb_title.SetKey("UUILogin_TITLE");
            lb_remember.SetKey("UUILogin_Remember");
            ButtonBlue.SetKey("UUILogin_Bt_Login");
            TextSignup.SetKey("UUILogin_Signup");
        }

        protected override void OnHide()
        {
            base.OnHide();

        }
    }
}