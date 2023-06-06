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
    [UIResources("UUIMain")]
    partial class UUIMain : UUIAutoGenWindow
    {


        protected RectTransform Root;
        protected Button Button_AddFriend;
        protected Button MenuShop;
        protected Image item_notification;
        protected Button MenuItems;
        protected Button MenuSkill;
        protected Button MenuWeapon;
        protected Button MenuRefresh;
        protected Button MenuMessages;
        protected Image message_notification;
        protected Text MessagCountText;
        protected Button MenuMap;
        protected Image mission_notification;
        protected Button Menu;
        protected Button Button_Friend;
        protected Slider ExpSilder;
        protected Text lb_exp;
        protected RawImage user_defalut;
        protected Button user_info;
        protected Text Level_Number;
        protected Text Username;
        protected Image swip;
        protected Text lb_gold;
        protected Button btn_goldadd;
        protected Text lb_gem;
        protected Image btn_addgem;
        protected Button MenuSetting;
        protected RectTransform Match;
        protected Button Button_Play;
        protected Button bt_info;
        protected Button bt_invite;
        protected Button bt_Exit;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            Root = FindChild<RectTransform>("Root");
            Button_AddFriend = FindChild<Button>("Button_AddFriend");
            MenuShop = FindChild<Button>("MenuShop");
            item_notification = FindChild<Image>("item_notification");
            MenuItems = FindChild<Button>("MenuItems");
            MenuSkill = FindChild<Button>("MenuSkill");
            MenuWeapon = FindChild<Button>("MenuWeapon");
            MenuRefresh = FindChild<Button>("MenuRefresh");
            MenuMessages = FindChild<Button>("MenuMessages");
            message_notification = FindChild<Image>("message_notification");
            MessagCountText = FindChild<Text>("MessagCountText");
            MenuMap = FindChild<Button>("MenuMap");
            mission_notification = FindChild<Image>("mission_notification");
            Menu = FindChild<Button>("Menu");
            Button_Friend = FindChild<Button>("Button_Friend");
            ExpSilder = FindChild<Slider>("ExpSilder");
            lb_exp = FindChild<Text>("lb_exp");
            user_defalut = FindChild<RawImage>("user_defalut");
            user_info = FindChild<Button>("user_info");
            Level_Number = FindChild<Text>("Level_Number");
            Username = FindChild<Text>("Username");
            swip = FindChild<Image>("swip");
            lb_gold = FindChild<Text>("lb_gold");
            btn_goldadd = FindChild<Button>("btn_goldadd");
            lb_gem = FindChild<Text>("lb_gem");
            btn_addgem = FindChild<Image>("btn_addgem");
            MenuSetting = FindChild<Button>("MenuSetting");
            Match = FindChild<RectTransform>("Match");
            Button_Play = FindChild<Button>("Button_Play");
            bt_info = FindChild<Button>("bt_info");
            bt_invite = FindChild<Button>("bt_invite");
            bt_Exit = FindChild<Button>("bt_Exit");


        }
    }
}