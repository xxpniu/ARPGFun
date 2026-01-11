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
    [UIResources("UUIDetail")]
    // ReSharper disable once InconsistentNaming
    partial class UUIDetail : UUIAutoGenWindow
    {
        public class EquipmentPropertyTableTemplate : TableItemTemplate
        {
            public EquipmentPropertyTableTemplate(){}
            public TextMeshProUGUI lb_text;

            public override void InitTemplate()
            {
                lb_text = FindChild<TextMeshProUGUI>("lb_text");

            }
        }


        protected RectTransform root;
        protected Button ItemBg;
        protected Image icon;
        protected Image ItemCount;
        protected TextMeshProUGUI t_num;
        protected Image Locked;
        protected Image ItemLevel;
        protected TextMeshProUGUI lb_level;
        protected Image WearOn;
        protected TextMeshProUGUI t_name;
        protected TextMeshProUGUI t_descript;
        protected GridLayoutGroup EquipmentProperty;
        protected TextMeshProUGUI t_prices;
        protected Button bt_cancel;
        protected Button bt_sale;
        protected Button bt_equip;


        protected UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>> EquipmentPropertyTableManager = new UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            root = FindChild<RectTransform>("root");
            ItemBg = FindChild<Button>("ItemBg");
            icon = FindChild<Image>("icon");
            ItemCount = FindChild<Image>("ItemCount");
            t_num = FindChild<TextMeshProUGUI>("t_num");
            Locked = FindChild<Image>("Locked");
            ItemLevel = FindChild<Image>("ItemLevel");
            lb_level = FindChild<TextMeshProUGUI>("lb_level");
            WearOn = FindChild<Image>("WearOn");
            t_name = FindChild<TextMeshProUGUI>("t_name");
            t_descript = FindChild<TextMeshProUGUI>("t_descript");
            EquipmentProperty = FindChild<GridLayoutGroup>("EquipmentProperty");
            t_prices = FindChild<TextMeshProUGUI>("t_prices");
            bt_cancel = FindChild<Button>("bt_cancel");
            bt_sale = FindChild<Button>("bt_sale");
            bt_equip = FindChild<Button>("bt_equip");

            EquipmentPropertyTableManager.InitFromLayout(EquipmentProperty);

        }
    }
}