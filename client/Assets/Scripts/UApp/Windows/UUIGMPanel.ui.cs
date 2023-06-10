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
    [UIResources("UUIGMPanel")]
    partial class UUIGMPanel : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Button Button;

            public override void InitTemplate()
            {
                Button = FindChild<Button>("Button");

            }
        }


        protected InputField IF_GmText;
        protected Button Bt_SendGM;
        protected GridLayoutGroup Content;
        protected Button bt_close;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            IF_GmText = FindChild<InputField>("IF_GmText");
            Bt_SendGM = FindChild<Button>("Bt_SendGM");
            Content = FindChild<GridLayoutGroup>("Content");
            bt_close = FindChild<Button>("bt_close");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}