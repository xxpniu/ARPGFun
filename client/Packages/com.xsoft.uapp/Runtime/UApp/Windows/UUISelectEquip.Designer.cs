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
    [UIResources("UUISelectEquip")]
    partial class UUISelectEquip : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Text lb_Name;
            public Button ItemBg;
            public Image icon;
            public Image ItemCount;
            public Text lb_count;
            public Image Locked;
            public Image WearOn;
            public Image ItemLevel;
            public Text lb_level;
            public Button bt_equip;

            public override void InitTemplate()
            {
                lb_Name = FindChild<Text>("lb_Name");
                ItemBg = FindChild<Button>("ItemBg");
                icon = FindChild<Image>("icon");
                ItemCount = FindChild<Image>("ItemCount");
                lb_count = FindChild<Text>("lb_count");
                Locked = FindChild<Image>("Locked");
                WearOn = FindChild<Image>("WearOn");
                ItemLevel = FindChild<Image>("ItemLevel");
                lb_level = FindChild<Text>("lb_level");
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

            ContentTableManager.InitFromGrid(Content);

        }
    }
}