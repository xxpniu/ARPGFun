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
    [UIResources("UUIItemRefresh")]
    partial class UUIItemRefresh : UUIAutoGenWindow
    {
        public class PropertyListTableTemplate : TableItemTemplate
        {
            public PropertyListTableTemplate(){}
            public Text lb_text;

            public override void InitTemplate()
            {
                lb_text = FindChild<Text>("lb_text");

            }
        }
        public class ItemListTableTemplate : TableItemTemplate
        {
            public ItemListTableTemplate(){}
            public Button BtSelected;
            public Image icon_right;
            public Image AddIconSel;
            public Image ERoot;
            public Text equip_lvl;

            public override void InitTemplate()
            {
                BtSelected = FindChild<Button>("BtSelected");
                icon_right = FindChild<Image>("icon_right");
                AddIconSel = FindChild<Image>("AddIconSel");
                ERoot = FindChild<Image>("ERoot");
                equip_lvl = FindChild<Text>("equip_lvl");

            }
        }
        public class EquipmentPropertyTableTemplate : TableItemTemplate
        {
            public EquipmentPropertyTableTemplate(){}
            public Text lb_text;

            public override void InitTemplate()
            {
                lb_text = FindChild<Text>("lb_text");

            }
        }


        protected Text lb_equipname;
        protected Text lb_equiprefresh;
        protected Button equipRefresh;
        protected Image icon;
        protected Image LevelRoot;
        protected Text lb_Lvl;
        protected Image AddIcon;
        protected Image Descript;
        protected Text lb_description;
        protected GridLayoutGroup PropertyList;
        protected RectTransform Right;
        protected Text lb_custom_title;
        protected GridLayoutGroup ItemList;
        protected Text lb_custom_property_title;
        protected GridLayoutGroup EquipmentProperty;
        protected Image LevelUp;
        protected Text lb_pro;
        protected Button bt_level_up;
        protected Image gold_icon;
        protected Text lb_gold;
        protected Image coin_icon;
        protected Text lb_coin;
        protected Button CloseButton;


        protected UITableManager<AutoGenTableItem<PropertyListTableTemplate, PropertyListTableModel>> PropertyListTableManager = new UITableManager<AutoGenTableItem<PropertyListTableTemplate, PropertyListTableModel>>();
        protected UITableManager<AutoGenTableItem<ItemListTableTemplate, ItemListTableModel>> ItemListTableManager = new UITableManager<AutoGenTableItem<ItemListTableTemplate, ItemListTableModel>>();
        protected UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>> EquipmentPropertyTableManager = new UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_equipname = FindChild<Text>("lb_equipname");
            lb_equiprefresh = FindChild<Text>("lb_equiprefresh");
            equipRefresh = FindChild<Button>("equipRefresh");
            icon = FindChild<Image>("icon");
            LevelRoot = FindChild<Image>("LevelRoot");
            lb_Lvl = FindChild<Text>("lb_Lvl");
            AddIcon = FindChild<Image>("AddIcon");
            Descript = FindChild<Image>("Descript");
            lb_description = FindChild<Text>("lb_description");
            PropertyList = FindChild<GridLayoutGroup>("PropertyList");
            Right = FindChild<RectTransform>("Right");
            lb_custom_title = FindChild<Text>("lb_custom_title");
            ItemList = FindChild<GridLayoutGroup>("ItemList");
            lb_custom_property_title = FindChild<Text>("lb_custom_property_title");
            EquipmentProperty = FindChild<GridLayoutGroup>("EquipmentProperty");
            LevelUp = FindChild<Image>("LevelUp");
            lb_pro = FindChild<Text>("lb_pro");
            bt_level_up = FindChild<Button>("bt_level_up");
            gold_icon = FindChild<Image>("gold_icon");
            lb_gold = FindChild<Text>("lb_gold");
            coin_icon = FindChild<Image>("coin_icon");
            lb_coin = FindChild<Text>("lb_coin");
            CloseButton = FindChild<Button>("CloseButton");

            PropertyListTableManager.InitFromLayout(PropertyList);
            ItemListTableManager.InitFromLayout(ItemList);
            EquipmentPropertyTableManager.InitFromLayout(EquipmentProperty);

        }
    }
}