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
    [UIResources("UUIHeroEquip")]
    partial class UUIHeroEquip : UUIAutoGenWindow
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
        public class EquipmentPropertyTableTemplate : TableItemTemplate
        {
            public EquipmentPropertyTableTemplate(){}
            public Text lb_text;

            public override void InitTemplate()
            {
                lb_text = FindChild<Text>("lb_text");

            }
        }


        protected Button equip_head;
        protected Image icon_head;
        protected Image HeadLevelRoot;
        protected Text head_Lvl;
        protected Button equip_weapon;
        protected Image icon_weapon;
        protected Image weapLeveRoot;
        protected Text weapon_Lvl;
        protected Button equip_cloth;
        protected Image icon_cloth;
        protected Image ClothLeveRoot;
        protected Text cloth_Lvl ;
        protected Button equip_shose;
        protected Image icon_shose;
        protected Image ShoseLeveRoot;
        protected Text shose_Lvl;
        protected Text Level;
        protected GridLayoutGroup PropertyList;
        protected RectTransform Right;
        protected Image EquipRight;
        protected Image icon_right;
        protected Image RightERoot;
        protected Text equip_lvl;
        protected Text right_name;
        protected Button take_off;
        protected Text des_Text;
        protected GridLayoutGroup EquipmentProperty;
        protected Image LevelUp;
        protected Text lb_pro;
        protected Button bt_level_up;
        protected Image gold_icon;
        protected Text lb_gold;
        protected Image coin_icon;
        protected Text lb_coin;
        protected Button bt_Exit;
        protected RectTransform Text;


        protected UITableManager<AutoGenTableItem<PropertyListTableTemplate, PropertyListTableModel>> PropertyListTableManager = new UITableManager<AutoGenTableItem<PropertyListTableTemplate, PropertyListTableModel>>();
        protected UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>> EquipmentPropertyTableManager = new UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            equip_head = FindChild<Button>("equip_head");
            icon_head = FindChild<Image>("icon_head");
            HeadLevelRoot = FindChild<Image>("HeadLevelRoot");
            head_Lvl = FindChild<Text>("head_Lvl");
            equip_weapon = FindChild<Button>("equip_weapon");
            icon_weapon = FindChild<Image>("icon_weapon");
            weapLeveRoot = FindChild<Image>("weapLeveRoot");
            weapon_Lvl = FindChild<Text>("weapon_Lvl");
            equip_cloth = FindChild<Button>("equip_cloth");
            icon_cloth = FindChild<Image>("icon_cloth");
            ClothLeveRoot = FindChild<Image>("ClothLeveRoot");
            cloth_Lvl  = FindChild<Text>("cloth_Lvl ");
            equip_shose = FindChild<Button>("equip_shose");
            icon_shose = FindChild<Image>("icon_shose");
            ShoseLeveRoot = FindChild<Image>("ShoseLeveRoot");
            shose_Lvl = FindChild<Text>("shose_Lvl");
            Level = FindChild<Text>("Level");
            PropertyList = FindChild<GridLayoutGroup>("PropertyList");
            Right = FindChild<RectTransform>("Right");
            EquipRight = FindChild<Image>("EquipRight");
            icon_right = FindChild<Image>("icon_right");
            RightERoot = FindChild<Image>("RightERoot");
            equip_lvl = FindChild<Text>("equip_lvl");
            right_name = FindChild<Text>("right_name");
            take_off = FindChild<Button>("take_off");
            des_Text = FindChild<Text>("des_Text");
            EquipmentProperty = FindChild<GridLayoutGroup>("EquipmentProperty");
            LevelUp = FindChild<Image>("LevelUp");
            lb_pro = FindChild<Text>("lb_pro");
            bt_level_up = FindChild<Button>("bt_level_up");
            gold_icon = FindChild<Image>("gold_icon");
            lb_gold = FindChild<Text>("lb_gold");
            coin_icon = FindChild<Image>("coin_icon");
            lb_coin = FindChild<Text>("lb_coin");
            bt_Exit = FindChild<Button>("bt_Exit");
            Text = FindChild<RectTransform>("Text");

            PropertyListTableManager.InitFromLayout(PropertyList);
            EquipmentPropertyTableManager.InitFromLayout(EquipmentProperty);

        }
    }
}