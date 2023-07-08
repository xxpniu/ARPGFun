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
    [UIResources("UUIPackage")]
    partial class UUIPackage : UUIAutoGenWindow
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
            public Image ItemLevel;
            public Text lb_level;
            public Image WearOn;

            public override void InitTemplate()
            {
                lb_Name = FindChild<Text>("lb_Name");
                ItemBg = FindChild<Button>("ItemBg");
                icon = FindChild<Image>("icon");
                ItemCount = FindChild<Image>("ItemCount");
                lb_count = FindChild<Text>("lb_count");
                Locked = FindChild<Image>("Locked");
                ItemLevel = FindChild<Image>("ItemLevel");
                lb_level = FindChild<Text>("lb_level");
                WearOn = FindChild<Image>("WearOn");

            }
        }


        protected Text lb_title;
        protected Button ButtonClose;
        protected Text TextSizeTitle;
        protected Text lb_TextCountCur;
        protected Text lb_TextCountSize;
        protected Button Bt_Buy;
        protected GridLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_title = FindChild<Text>("lb_title");
            ButtonClose = FindChild<Button>("ButtonClose");
            TextSizeTitle = FindChild<Text>("TextSizeTitle");
            lb_TextCountCur = FindChild<Text>("lb_TextCountCur");
            lb_TextCountSize = FindChild<Text>("lb_TextCountSize");
            Bt_Buy = FindChild<Button>("Bt_Buy");
            Content = FindChild<GridLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}