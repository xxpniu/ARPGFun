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
    [UIResources("UUIGMDetail")]
    partial class UUIGMDetail : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public InputField InputField;
            public Text lb_text;

            public override void InitTemplate()
            {
                InputField = FindChild<InputField>("InputField");
                lb_text = FindChild<Text>("lb_text");

            }
        }


        protected VerticalLayoutGroup Content;
        protected Button bt_send;
        protected Button bt_close;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Content = FindChild<VerticalLayoutGroup>("Content");
            bt_send = FindChild<Button>("bt_send");
            bt_close = FindChild<Button>("bt_close");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}