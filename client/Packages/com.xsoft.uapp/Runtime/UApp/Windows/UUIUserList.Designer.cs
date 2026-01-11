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
    [UIResources("UUIUserList")]
    // ReSharper disable once InconsistentNaming
    partial class UUIUserList : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public TextMeshProUGUI TextName;
            public TextMeshProUGUI TextLvScore;
            public Button AddBlue;

            public override void InitTemplate()
            {
                TextName = FindChild<TextMeshProUGUI>("TextName");
                TextLvScore = FindChild<TextMeshProUGUI>("TextLvScore");
                AddBlue = FindChild<Button>("AddBlue");

            }
        }


        protected TextMeshProUGUI Lb_TitleText;
        protected Button ButtonClose;
        protected VerticalLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Lb_TitleText = FindChild<TextMeshProUGUI>("Lb_TitleText");
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<VerticalLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}