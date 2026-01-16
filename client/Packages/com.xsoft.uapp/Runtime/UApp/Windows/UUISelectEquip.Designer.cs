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
    [UIResources("UUISelectEquip")]
    // ReSharper disable once InconsistentNaming
    partial class UUISelectEquip : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public TextMeshProUGUI lb_Name;
            public Button ItemBg;
            public Image icon;
            public Image ItemCount;
            public TextMeshProUGUI lb_count;
            public Image Locked;
            public Image WearOn;
            public Image ItemLevel;
            public TextMeshProUGUI lb_level;
            public Button bt_equip;

            public override void InitTemplate()
            {
                lb_Name = FindChild<TextMeshProUGUI>("lb_Name");
                ItemBg = FindChild<Button>("ItemBg");
                icon = FindChild<Image>("icon");
                ItemCount = FindChild<Image>("ItemCount");
                lb_count = FindChild<TextMeshProUGUI>("lb_count");
                Locked = FindChild<Image>("Locked");
                WearOn = FindChild<Image>("WearOn");
                ItemLevel = FindChild<Image>("ItemLevel");
                lb_level = FindChild<TextMeshProUGUI>("lb_level");
                bt_equip = FindChild<Button>("bt_equip");

            }
        }


        protected GridLayoutGroup Content;
        protected Button bt_cancel;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Content = FindChild<GridLayoutGroup>("Content");
            bt_cancel = FindChild<Button>("bt_cancel");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}