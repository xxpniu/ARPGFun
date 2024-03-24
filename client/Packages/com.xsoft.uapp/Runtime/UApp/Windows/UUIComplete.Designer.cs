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
    [UIResources("UUIComplete")]
    // ReSharper disable once InconsistentNaming
    partial class UUIComplete : UUIAutoGenWindow
    {
        public class ItemContentTableTemplate : TableItemTemplate
        {
            public ItemContentTableTemplate(){}
            public Text itemText;
            public Image Icon;

            public override void InitTemplate()
            {
                itemText = FindChild<Text>("itemText");
                Icon = FindChild<Image>("Icon");

            }
        }


        protected Image ButtonBlue;
        protected Image ButtonBrown;
        protected Image ButtonGreen;
        protected Image ButtonClose;
        protected Text TitleText;
        protected Image Starb1;
        protected Image Star1;
        protected Image Starb2;
        protected Image Star2;
        protected Image Starb3;
        protected Image Star3;
        protected Text ScoreText;
        protected Text ScoreNumber;
        protected HorizontalLayoutGroup ItemContent;


        protected UITableManager<AutoGenTableItem<ItemContentTableTemplate, ItemContentTableModel>> ItemContentTableManager = new UITableManager<AutoGenTableItem<ItemContentTableTemplate, ItemContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonBlue = FindChild<Image>("ButtonBlue");
            ButtonBrown = FindChild<Image>("ButtonBrown");
            ButtonGreen = FindChild<Image>("ButtonGreen");
            ButtonClose = FindChild<Image>("ButtonClose");
            TitleText = FindChild<Text>("TitleText");
            Starb1 = FindChild<Image>("Starb1");
            Star1 = FindChild<Image>("Star1");
            Starb2 = FindChild<Image>("Starb2");
            Star2 = FindChild<Image>("Star2");
            Starb3 = FindChild<Image>("Starb3");
            Star3 = FindChild<Image>("Star3");
            ScoreText = FindChild<Text>("ScoreText");
            ScoreNumber = FindChild<Text>("ScoreNumber");
            ItemContent = FindChild<HorizontalLayoutGroup>("ItemContent");

            ItemContentTableManager.InitFromLayout(ItemContent);

        }
    }
}