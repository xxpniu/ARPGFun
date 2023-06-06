using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UGameTools;
using UnityEngine.UI;
//AUTO GenCode Don't edit it.
namespace Windows
{
    [UIResources("UUISignup")]
    partial class UUISignup : UUIAutoGenWindow
    {


        protected Button ButtonClose;
        protected Button TextSignin;
        protected Button ButtonBlue;
        protected InputField TextInputBoxEmail;
        protected InputField TextInputBoxPassword;
        protected InputField TextInputBoxPasswordRepeat;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            TextSignin = FindChild<Button>("TextSignin");
            ButtonBlue = FindChild<Button>("ButtonBlue");
            TextInputBoxEmail = FindChild<InputField>("TextInputBoxEmail");
            TextInputBoxPassword = FindChild<InputField>("TextInputBoxPassword");
            TextInputBoxPasswordRepeat = FindChild<InputField>("TextInputBoxPasswordRepeat");


        }
    }
}