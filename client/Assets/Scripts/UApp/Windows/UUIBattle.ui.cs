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
    [UIResources("UUIBattle")]
    partial class UUIBattle : UUIAutoGenWindow
    {
        public class GridTableTemplate : TableItemTemplate
        {
            public GridTableTemplate(){}
            public Button Button;
            public Image Icon;
            public Text CDTime;
            public Image ICdMask;

            public override void InitTemplate()
            {
                Button = FindChild<Button>("Button");
                Icon = FindChild<Image>("Icon");
                CDTime = FindChild<Text>("CDTime");
                ICdMask = FindChild<Image>("ICdMask");

            }
        }


        protected Text lb_text;
        protected RawImage MapTexture;
        protected RectTransform ViewForward;
        protected RawImage Player;
        protected Button user_info;
        protected Image user_exp;
        protected Text Level_Number;
        protected RoundGridLayout Grid;
        protected Button bt_Exit;
        protected Image Joystick_Left;
        protected Image swipe;
        protected Button bt_normal_att;
        protected Image att_Icon;
        protected Image AttCdMask;
        protected Button bt_hp;
        protected Image hp_item_Icon;
        protected Text hp_num;
        protected Button bt_mp;
        protected Image mp_item_Icon;
        protected Text mp_num;
        protected Slider HPSilder;
        protected Image hp_bg;
        protected Text lb_hp;
        protected Slider MpSilder;
        protected Text lb_mp;
        protected Text Username;


        protected UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>> GridTableManager = new UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_text = FindChild<Text>("lb_text");
            MapTexture = FindChild<RawImage>("MapTexture");
            ViewForward = FindChild<RectTransform>("ViewForward");
            Player = FindChild<RawImage>("Player");
            user_info = FindChild<Button>("user_info");
            user_exp = FindChild<Image>("user_exp");
            Level_Number = FindChild<Text>("Level_Number");
            Grid = FindChild<RoundGridLayout>("Grid");
            bt_Exit = FindChild<Button>("bt_Exit");
            Joystick_Left = FindChild<Image>("Joystick_Left");
            swipe = FindChild<Image>("swipe");
            bt_normal_att = FindChild<Button>("bt_normal_att");
            att_Icon = FindChild<Image>("att_Icon");
            AttCdMask = FindChild<Image>("AttCdMask");
            bt_hp = FindChild<Button>("bt_hp");
            hp_item_Icon = FindChild<Image>("hp_item_Icon");
            hp_num = FindChild<Text>("hp_num");
            bt_mp = FindChild<Button>("bt_mp");
            mp_item_Icon = FindChild<Image>("mp_item_Icon");
            mp_num = FindChild<Text>("mp_num");
            HPSilder = FindChild<Slider>("HPSilder");
            hp_bg = FindChild<Image>("hp_bg");
            lb_hp = FindChild<Text>("lb_hp");
            MpSilder = FindChild<Slider>("MpSilder");
            lb_mp = FindChild<Text>("lb_mp");
            Username = FindChild<Text>("Username");

            GridTableManager.InitFromLayout(Grid);

        }
    }
}