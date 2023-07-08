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
    [UIResources("UUISettings")]
    partial class UUISettings : UUIAutoGenWindow
    {


        protected Text lb_title;
        protected Button ButtonClose;
        protected Text lb_notice_Text;
        protected Text lb_notice_text_value;
        protected Toggle NoticeToggle;
        protected Text lb_save_Text;
        protected Text lb_save_Text_value;
        protected Toggle SaveToggle;
        protected Text lb_bgm;
        protected Slider Slider_bgm;
        protected Text lb_sfx;
        protected Slider sfx_Slider;
        protected Button ButtonLanguage;
        protected Image ButtonExit;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_title = FindChild<Text>("lb_title");
            ButtonClose = FindChild<Button>("ButtonClose");
            lb_notice_Text = FindChild<Text>("lb_notice_Text");
            lb_notice_text_value = FindChild<Text>("lb_notice_text_value");
            NoticeToggle = FindChild<Toggle>("NoticeToggle");
            lb_save_Text = FindChild<Text>("lb_save_Text");
            lb_save_Text_value = FindChild<Text>("lb_save_Text_value");
            SaveToggle = FindChild<Toggle>("SaveToggle");
            lb_bgm = FindChild<Text>("lb_bgm");
            Slider_bgm = FindChild<Slider>("Slider_bgm");
            lb_sfx = FindChild<Text>("lb_sfx");
            sfx_Slider = FindChild<Slider>("sfx_Slider");
            ButtonLanguage = FindChild<Button>("ButtonLanguage");
            ButtonExit = FindChild<Image>("ButtonExit");


        }
    }
}