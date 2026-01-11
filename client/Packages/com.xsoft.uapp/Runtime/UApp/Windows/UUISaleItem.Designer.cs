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
    [UIResources("UUISaleItem")]
    // ReSharper disable once InconsistentNaming
    partial class UUISaleItem : UUIAutoGenWindow
    {


        protected TextMeshProUGUI t_title;
        protected TextMeshProUGUI t_pricetotal;
        protected TextMeshProUGUI t_num;
        protected TextMeshProUGUI t_name;
        protected Slider s_salenum;
        protected Button bt_OK;
        protected Button bt_close;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            t_title = FindChild<TextMeshProUGUI>("t_title");
            t_pricetotal = FindChild<TextMeshProUGUI>("t_pricetotal");
            t_num = FindChild<TextMeshProUGUI>("t_num");
            t_name = FindChild<TextMeshProUGUI>("t_name");
            s_salenum = FindChild<Slider>("s_salenum");
            bt_OK = FindChild<Button>("bt_OK");
            bt_close = FindChild<Button>("bt_close");


        }
    }
}