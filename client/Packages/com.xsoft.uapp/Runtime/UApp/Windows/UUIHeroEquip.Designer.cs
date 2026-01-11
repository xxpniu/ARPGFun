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
    [UIResources("UUIHeroEquip")]
    // ReSharper disable once InconsistentNaming
    partial class UUIHeroEquip : UUIAutoGenWindow
    {
        public class PropertyListTableTemplate : TableItemTemplate
        {
            public PropertyListTableTemplate(){}
            public TextMeshProUGUI lb_text;

            public override void InitTemplate()
            {
                lb_text = FindChild<TextMeshProUGUI>("lb_text");

            }
        }
        public class EquipmentPropertyTableTemplate : TableItemTemplate
        {
            public EquipmentPropertyTableTemplate(){}
            public TextMeshProUGUI lb_text;

            public override void InitTemplate()
            {
                lb_text = FindChild<TextMeshProUGUI>("lb_text");

            }
        }


        protected Button equip_head;
        protected Image icon_head;
        protected Image HeadLevelRoot;
        protected TextMeshProUGUI head_Lvl;
        protected Button equip_weapon;
        protected Image icon_weapon;
        protected Image weapLeveRoot;
        protected TextMeshProUGUI weapon_Lvl;
        protected Button equip_cloth;
        protected Image icon_cloth;
        protected Image ClothLeveRoot;
        protected TextMeshProUGUI cloth_Lvl ;
        protected Button equip_shose;
        protected Image icon_shose;
        protected Image ShoseLeveRoot;
        protected TextMeshProUGUI shose_Lvl;
        protected TextMeshProUGUI Level;
        protected GridLayoutGroup PropertyList;
        protected RectTransform Right;
        protected Image EquipRight;
        protected Image icon_right;
        protected Image RightERoot;
        protected TextMeshProUGUI equip_lvl;
        protected TextMeshProUGUI right_name;
        protected Button take_off;
        protected TextMeshProUGUI des_Text;
        protected GridLayoutGroup EquipmentProperty;
        protected Image LevelUp;
        protected TextMeshProUGUI lb_pro;
        protected Button bt_level_up;
        protected Image gold_icon;
        protected TextMeshProUGUI lb_gold;
        protected Image coin_icon;
        protected TextMeshProUGUI lb_coin;
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
            head_Lvl = FindChild<TextMeshProUGUI>("head_Lvl");
            equip_weapon = FindChild<Button>("equip_weapon");
            icon_weapon = FindChild<Image>("icon_weapon");
            weapLeveRoot = FindChild<Image>("weapLeveRoot");
            weapon_Lvl = FindChild<TextMeshProUGUI>("weapon_Lvl");
            equip_cloth = FindChild<Button>("equip_cloth");
            icon_cloth = FindChild<Image>("icon_cloth");
            ClothLeveRoot = FindChild<Image>("ClothLeveRoot");
            cloth_Lvl  = FindChild<TextMeshProUGUI>("cloth_Lvl ");
            equip_shose = FindChild<Button>("equip_shose");
            icon_shose = FindChild<Image>("icon_shose");
            ShoseLeveRoot = FindChild<Image>("ShoseLeveRoot");
            shose_Lvl = FindChild<TextMeshProUGUI>("shose_Lvl");
            Level = FindChild<TextMeshProUGUI>("Level");
            PropertyList = FindChild<GridLayoutGroup>("PropertyList");
            Right = FindChild<RectTransform>("Right");
            EquipRight = FindChild<Image>("EquipRight");
            icon_right = FindChild<Image>("icon_right");
            RightERoot = FindChild<Image>("RightERoot");
            equip_lvl = FindChild<TextMeshProUGUI>("equip_lvl");
            right_name = FindChild<TextMeshProUGUI>("right_name");
            take_off = FindChild<Button>("take_off");
            des_Text = FindChild<TextMeshProUGUI>("des_Text");
            EquipmentProperty = FindChild<GridLayoutGroup>("EquipmentProperty");
            LevelUp = FindChild<Image>("LevelUp");
            lb_pro = FindChild<TextMeshProUGUI>("lb_pro");
            bt_level_up = FindChild<Button>("bt_level_up");
            gold_icon = FindChild<Image>("gold_icon");
            lb_gold = FindChild<TextMeshProUGUI>("lb_gold");
            coin_icon = FindChild<Image>("coin_icon");
            lb_coin = FindChild<TextMeshProUGUI>("lb_coin");
            bt_Exit = FindChild<Button>("bt_Exit");
            Text = FindChild<RectTransform>("Text");

            PropertyListTableManager.InitFromLayout(PropertyList);
            EquipmentPropertyTableManager.InitFromLayout(EquipmentProperty);

        }
    }
}