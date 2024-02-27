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
    [UIResources("UUIPopup")]
    partial class UUIPopup : UUIAutoGenWindow
    {


        protected Button ButtonBlue;
        protected Button ButtonBrown;
        protected Text lb_title;
        protected Text lb_conent;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonBlue = FindChild<Button>("ButtonBlue");
            ButtonBrown = FindChild<Button>("ButtonBrown");
            lb_title = FindChild<Text>("lb_title");
            lb_conent = FindChild<Text>("lb_conent");


        }
    }
}