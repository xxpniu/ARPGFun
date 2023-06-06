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
    [UIResources("UUILogin")]
    partial class UUILogin : UUIAutoGenWindow
    {


        protected Button ButtonClose;
        protected Text lb_title;
        protected Button TextSignup;
        protected Text Text;
        protected Button ButtonBlue;
        protected InputField TextInputBoxUserName;
        protected InputField TextInputBoxPassWord;
        protected Text lb_remember;
        protected Toggle CheckBox;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            lb_title = FindChild<Text>("lb_title");
            TextSignup = FindChild<Button>("TextSignup");
            Text = FindChild<Text>("Text");
            ButtonBlue = FindChild<Button>("ButtonBlue");
            TextInputBoxUserName = FindChild<InputField>("TextInputBoxUserName");
            TextInputBoxPassWord = FindChild<InputField>("TextInputBoxPassWord");
            lb_remember = FindChild<Text>("lb_remember");
            CheckBox = FindChild<Toggle>("CheckBox");


        }
    }
}