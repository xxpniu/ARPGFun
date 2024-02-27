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
    [UIResources("UUIUserInvite")]
    partial class UUIUserInvite : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Button InviteBlue;
            public Text TextName;
            public Text TextLvScore;

            public override void InitTemplate()
            {
                InviteBlue = FindChild<Button>("InviteBlue");
                TextName = FindChild<Text>("TextName");
                TextLvScore = FindChild<Text>("TextLvScore");

            }
        }


        protected Text Lb_TitleText;
        protected Button ButtonClose;
        protected VerticalLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Lb_TitleText = FindChild<Text>("Lb_TitleText");
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<VerticalLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}