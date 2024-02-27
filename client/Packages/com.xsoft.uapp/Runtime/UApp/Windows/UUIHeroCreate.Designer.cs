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
    [UIResources("UUIHeroCreate")]
    partial class UUIHeroCreate : UUIAutoGenWindow
    {
        public class ListTableTemplate : TableItemTemplate
        {
            public ListTableTemplate(){}
            public Button BtHero;
            public Text lb_name;

            public override void InitTemplate()
            {
                BtHero = FindChild<Button>("BtHero");
                lb_name = FindChild<Text>("lb_name");

            }
        }


        protected VerticalLayoutGroup List;
        protected InputField InputField;
        protected Button Bt_create;
        protected Text lb_description;


        protected UITableManager<AutoGenTableItem<ListTableTemplate, ListTableModel>> ListTableManager = new UITableManager<AutoGenTableItem<ListTableTemplate, ListTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            List = FindChild<VerticalLayoutGroup>("List");
            InputField = FindChild<InputField>("InputField");
            Bt_create = FindChild<Button>("Bt_create");
            lb_description = FindChild<Text>("lb_description");

            ListTableManager.InitFromLayout(List);

        }
    }
}