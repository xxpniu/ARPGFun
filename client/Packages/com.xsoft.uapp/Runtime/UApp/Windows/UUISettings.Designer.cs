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
    [UIResources("UUISettings")]
    // ReSharper disable once InconsistentNaming
    partial class UUISettings : UUIAutoGenWindow
    {


        protected TextMeshProUGUI lb_title;
        protected Button ButtonClose;
        protected TextMeshProUGUI lb_notice_Text;
        protected TextMeshProUGUI lb_notice_text_value;
        protected Toggle NoticeToggle;
        protected TextMeshProUGUI lb_save_Text;
        protected TextMeshProUGUI lb_save_Text_value;
        protected Toggle SaveToggle;
        protected TextMeshProUGUI lb_bgm;
        protected Slider Slider_bgm;
        protected TextMeshProUGUI lb_sfx;
        protected Slider sfx_Slider;
        protected Button ButtonLanguage;
        protected Image ButtonExit;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_title = FindChild<TextMeshProUGUI>("lb_title");
            ButtonClose = FindChild<Button>("ButtonClose");
            lb_notice_Text = FindChild<TextMeshProUGUI>("lb_notice_Text");
            lb_notice_text_value = FindChild<TextMeshProUGUI>("lb_notice_text_value");
            NoticeToggle = FindChild<Toggle>("NoticeToggle");
            lb_save_Text = FindChild<TextMeshProUGUI>("lb_save_Text");
            lb_save_Text_value = FindChild<TextMeshProUGUI>("lb_save_Text_value");
            SaveToggle = FindChild<Toggle>("SaveToggle");
            lb_bgm = FindChild<TextMeshProUGUI>("lb_bgm");
            Slider_bgm = FindChild<Slider>("Slider_bgm");
            lb_sfx = FindChild<TextMeshProUGUI>("lb_sfx");
            sfx_Slider = FindChild<Slider>("sfx_Slider");
            ButtonLanguage = FindChild<Button>("ButtonLanguage");
            ButtonExit = FindChild<Image>("ButtonExit");


        }
    }
}