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
    [UIResources("UUIShopGold")]
    // ReSharper disable once InconsistentNaming
    partial class UUIShopGold : UUIAutoGenWindow
    {
        public class ContentsTableTemplate : TableItemTemplate
        {
            public ContentsTableTemplate(){}
            public Button ButtonBlue;
            public Image icon;
            public TextMeshProUGUI lb_gold;
            public TextMeshProUGUI lb_name;

            public override void InitTemplate()
            {
                ButtonBlue = FindChild<Button>("ButtonBlue");
                icon = FindChild<Image>("icon");
                lb_gold = FindChild<TextMeshProUGUI>("lb_gold");
                lb_name = FindChild<TextMeshProUGUI>("lb_name");

            }
        }


        protected Button ButtonClose;
        protected HorizontalLayoutGroup Contents;


        protected UITableManager<AutoGenTableItem<ContentsTableTemplate, ContentsTableModel>> ContentsTableManager = new UITableManager<AutoGenTableItem<ContentsTableTemplate, ContentsTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Contents = FindChild<HorizontalLayoutGroup>("Contents");

            ContentsTableManager.InitFromLayout(Contents);

        }
    }
}