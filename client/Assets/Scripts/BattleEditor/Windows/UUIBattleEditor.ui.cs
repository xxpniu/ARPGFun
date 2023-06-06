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
    [UIResources("UUIBattleEditor")]
    partial class UUIBattleEditor : UUIAutoGenWindow
    {
        public class GridTableTemplate : TableItemTemplate
        {
            public GridTableTemplate(){}
            public Button Button;
            public Image ICdMask;
            public Text Cost;

            public override void InitTemplate()
            {
                Button = FindChild<Button>("Button");
                ICdMask = FindChild<Image>("ICdMask");
                Cost = FindChild<Text>("Cost");

            }
        }


        protected GridLayoutGroup Grid;
        protected Image Joystick_Left;
        protected Slider s_distance;
        protected Slider s_rot_y;
        protected Slider s_rot_x;
        protected Slider s_time_scale;
        protected Slider s_distance_camera;
        protected InputField input_index;
        protected Button bt_releaser;
        protected Button bt_targe;
        protected Toggle to_enable_ai;
        protected Toggle to_do_remove;
        protected InputField input_Level;
        protected InputField input_skill;
        protected Button bt_add;
        protected Button bt_remove;
        protected Button bt_normal_att;


        protected UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>> GridTableManager = new UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Grid = FindChild<GridLayoutGroup>("Grid");
            Joystick_Left = FindChild<Image>("Joystick_Left");
            s_distance = FindChild<Slider>("s_distance");
            s_rot_y = FindChild<Slider>("s_rot_y");
            s_rot_x = FindChild<Slider>("s_rot_x");
            s_time_scale = FindChild<Slider>("s_time_scale");
            s_distance_camera = FindChild<Slider>("s_distance_camera");
            input_index = FindChild<InputField>("input_index");
            bt_releaser = FindChild<Button>("bt_releaser");
            bt_targe = FindChild<Button>("bt_targe");
            to_enable_ai = FindChild<Toggle>("to_enable_ai");
            to_do_remove = FindChild<Toggle>("to_do_remove");
            input_Level = FindChild<InputField>("input_Level");
            input_skill = FindChild<InputField>("input_skill");
            bt_add = FindChild<Button>("bt_add");
            bt_remove = FindChild<Button>("bt_remove");
            bt_normal_att = FindChild<Button>("bt_normal_att");

            GridTableManager.InitFromGrid(Grid);

        }
    }
}