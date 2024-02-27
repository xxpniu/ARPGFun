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
    [UIResources("UUILevelUp")]
    partial class UUILevelUp : UUIAutoGenWindow
    {


        protected Image Root;
        protected Button ButtonClose;
        protected Text lb_level;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            Root = FindChild<Image>("Root");
            ButtonClose = FindChild<Button>("ButtonClose");
            lb_level = FindChild<Text>("lb_level");


        }
    }
}