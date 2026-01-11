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
    [UIResources("UUILogin")]
    // ReSharper disable once InconsistentNaming
    partial class UUILogin : UUIAutoGenWindow
    {


        protected Button ButtonClose;
        protected TextMeshProUGUI lb_title;
        protected Button TextSignup;
        protected TextMeshProUGUI Text;
        protected Button ButtonBlue;
        protected TMP_InputField TextInputBoxUserName;
        protected TMP_InputField TextInputBoxPassWord;
        protected TextMeshProUGUI lb_remember;
        protected Toggle CheckBox;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            lb_title = FindChild<TextMeshProUGUI>("lb_title");
            TextSignup = FindChild<Button>("TextSignup");
            Text = FindChild<TextMeshProUGUI>("Text");
            ButtonBlue = FindChild<Button>("ButtonBlue");
            TextInputBoxUserName = FindChild<TMP_InputField>("TextInputBoxUserName");
            TextInputBoxPassWord = FindChild<TMP_InputField>("TextInputBoxPassWord");
            lb_remember = FindChild<TextMeshProUGUI>("lb_remember");
            CheckBox = FindChild<Toggle>("CheckBox");


        }
    }
}