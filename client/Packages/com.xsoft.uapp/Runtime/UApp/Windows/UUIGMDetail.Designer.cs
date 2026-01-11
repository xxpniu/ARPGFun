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
    [UIResources("UUIGMDetail")]
    // ReSharper disable once InconsistentNaming
    partial class UUIGMDetail : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public TMP_InputField InputField;
            public TextMeshProUGUI lb_text;

            public override void InitTemplate()
            {
                InputField = FindChild<TMP_InputField>("InputField");
                lb_text = FindChild<TextMeshProUGUI>("lb_text");

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