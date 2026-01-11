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
    [UIResources("UUILevelUp")]
    // ReSharper disable once InconsistentNaming
    partial class UUILevelUp : UUIAutoGenWindow
    {


        protected Image Root;
        protected Button ButtonClose;
        protected TextMeshProUGUI lb_level;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            Root = FindChild<Image>("Root");
            ButtonClose = FindChild<Button>("ButtonClose");
            lb_level = FindChild<TextMeshProUGUI>("lb_level");


        }
    }
}