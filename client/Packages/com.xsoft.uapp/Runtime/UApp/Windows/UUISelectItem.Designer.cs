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
    [UIResources("UUISelectItem")]
    // ReSharper disable once InconsistentNaming
    partial class UUISelectItem : UUIAutoGenWindow
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
            public Image ItemLevel;
            public TextMeshProUGUI lb_level;
            public Image WearOn;
            public Image Selected;

            public override void InitTemplate()
            {
                lb_Name = FindChild<TextMeshProUGUI>("lb_Name");
                ItemBg = FindChild<Button>("ItemBg");
                icon = FindChild<Image>("icon");
                ItemCount = FindChild<Image>("ItemCount");
                lb_count = FindChild<TextMeshProUGUI>("lb_count");
                Locked = FindChild<Image>("Locked");
                ItemLevel = FindChild<Image>("ItemLevel");
                lb_level = FindChild<TextMeshProUGUI>("lb_level");
                WearOn = FindChild<Image>("WearOn");
                Selected = FindChild<Image>("Selected");

            }
        }


        protected Button ButtonClose;
        protected GridLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<GridLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}