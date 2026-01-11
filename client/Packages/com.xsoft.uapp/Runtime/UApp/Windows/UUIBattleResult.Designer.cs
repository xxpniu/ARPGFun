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
    [UIResources("UUIBattleResult")]
    // ReSharper disable once InconsistentNaming
    partial class UUIBattleResult : UUIAutoGenWindow
    {


        protected TextMeshProUGUI Text;
        protected Button Bt_Again;
        protected Button Bt_Ok;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            Text = FindChild<TextMeshProUGUI>("Text");
            Bt_Again = FindChild<Button>("Bt_Again");
            Bt_Ok = FindChild<Button>("Bt_Ok");


        }
    }
}