using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UGameTools;
using UnityEngine.UI;
using TMPro;
//AUTO GenCode Don't edit it.
namespace Windows
{
    [UIResources("UUISignup")]
    // ReSharper disable once InconsistentNaming
    partial class UUISignup : UUIAutoGenWindow
    {


        protected Button ButtonClose;
        protected Button TextSignin;
        protected Button ButtonBlue;
        protected TMP_InputField TextInputBoxEmail;
        protected TMP_InputField TextInputBoxPassword;
        protected TMP_InputField TextInputBoxPasswordRepeat;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            TextSignin = FindChild<Button>("TextSignin");
            ButtonBlue = FindChild<Button>("ButtonBlue");
            TextInputBoxEmail = FindChild<TMP_InputField>("TextInputBoxEmail");
            TextInputBoxPassword = FindChild<TMP_InputField>("TextInputBoxPassword");
            TextInputBoxPasswordRepeat = FindChild<TMP_InputField>("TextInputBoxPasswordRepeat");


        }
    }
}