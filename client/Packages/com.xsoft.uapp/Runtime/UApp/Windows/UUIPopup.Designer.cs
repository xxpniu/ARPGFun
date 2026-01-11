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
    [UIResources("UUIPopup")]
    // ReSharper disable once InconsistentNaming
    partial class UUIPopup : UUIAutoGenWindow
    {


        protected TextMeshProUGUI lb_title;
        protected TextMeshProUGUI lb_conent;
        protected Button ButtonBlue;
        protected Button ButtonBrown;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_title = FindChild<TextMeshProUGUI>("lb_title");
            lb_conent = FindChild<TextMeshProUGUI>("lb_conent");
            ButtonBlue = FindChild<Button>("ButtonBlue");
            ButtonBrown = FindChild<Button>("ButtonBrown");


        }
    }
}