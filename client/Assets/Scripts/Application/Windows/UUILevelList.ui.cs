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
    [UIResources("UUILevelList")]
    partial class UUILevelList : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Button ButtonBrown;
            public Button ButtonGreen;
            public Image missionImage;
            public Text Name;
            public Text Desc;

            public override void InitTemplate()
            {
                ButtonBrown = FindChild<Button>("ButtonBrown");
                ButtonGreen = FindChild<Button>("ButtonGreen");
                missionImage = FindChild<Image>("missionImage");
                Name = FindChild<Text>("Name");
                Desc = FindChild<Text>("Desc");

            }
        }


        protected Text lb_title;
        protected Button Bt_Return;
        protected VerticalLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_title = FindChild<Text>("lb_title");
            Bt_Return = FindChild<Button>("Bt_Return");
            Content = FindChild<VerticalLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}