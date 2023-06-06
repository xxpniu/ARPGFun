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
    [UIResources("UUISaleItem")]
    partial class UUISaleItem : UUIAutoGenWindow
    {


        protected Text t_title;
        protected Text t_pricetotal;
        protected Text t_num;
        protected Text t_name;
        protected Slider s_salenum;
        protected Button bt_OK;
        protected Button bt_close;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            t_title = FindChild<Text>("t_title");
            t_pricetotal = FindChild<Text>("t_pricetotal");
            t_num = FindChild<Text>("t_num");
            t_name = FindChild<Text>("t_name");
            s_salenum = FindChild<Slider>("s_salenum");
            bt_OK = FindChild<Button>("bt_OK");
            bt_close = FindChild<Button>("bt_close");


        }
    }
}