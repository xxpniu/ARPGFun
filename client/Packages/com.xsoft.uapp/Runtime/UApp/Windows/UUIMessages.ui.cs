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
    [UIResources("UUIMessages")]
    partial class UUIMessages : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Image ButtonBrown;
            public Image ButtonGreen;
            public Text TextName;
            public Text TextMessage;

            public override void InitTemplate()
            {
                ButtonBrown = FindChild<Image>("ButtonBrown");
                ButtonGreen = FindChild<Image>("ButtonGreen");
                TextName = FindChild<Text>("TextName");
                TextMessage = FindChild<Text>("TextMessage");

            }
        }


        protected Button ButtonClose;
        protected VerticalLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<VerticalLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}