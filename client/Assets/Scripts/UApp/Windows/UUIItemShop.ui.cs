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
    [UIResources("UUIItemShop")]
    partial class UUIItemShop : UUIAutoGenWindow
    {
        public class ShopTabTableTemplate : TableItemTemplate
        {
            public ShopTabTableTemplate(){}
            public Toggle ToggleSelected;
            public Text ShopName;

            public override void InitTemplate()
            {
                ToggleSelected = FindChild<Toggle>("ToggleSelected");
                ShopName = FindChild<Text>("ShopName");

            }
        }
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Button ItemBg;
            public Image icon;
            public Image ItemCount;
            public Text t_num;
            public Image Locked;
            public Image ItemLevel;
            public Text lb_level;
            public Image WearOn;
            public Text Name;
            public Button ButtonGold;
            public Button ButtonCoin;

            public override void InitTemplate()
            {
                ItemBg = FindChild<Button>("ItemBg");
                icon = FindChild<Image>("icon");
                ItemCount = FindChild<Image>("ItemCount");
                t_num = FindChild<Text>("t_num");
                Locked = FindChild<Image>("Locked");
                ItemLevel = FindChild<Image>("ItemLevel");
                lb_level = FindChild<Text>("lb_level");
                WearOn = FindChild<Image>("WearOn");
                Name = FindChild<Text>("Name");
                ButtonGold = FindChild<Button>("ButtonGold");
                ButtonCoin = FindChild<Button>("ButtonCoin");

            }
        }


        protected Button ButtonClose;
        protected HorizontalLayoutGroup ShopTab;
        protected GridLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ShopTabTableTemplate, ShopTabTableModel>> ShopTabTableManager = new UITableManager<AutoGenTableItem<ShopTabTableTemplate, ShopTabTableModel>>();
        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            ShopTab = FindChild<HorizontalLayoutGroup>("ShopTab");
            Content = FindChild<GridLayoutGroup>("Content");

            ShopTabTableManager.InitFromLayout(ShopTab);
            ContentTableManager.InitFromLayout(Content);

        }
    }
}