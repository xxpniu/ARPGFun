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
    [UIResources("UUIMagic")]
    partial class UUIMagic : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Image Pic;
            public Image Icon;
            public Text lb_name;
            public Text lb_Level;
            public Image Selected;
            public Button BtClick;

            public override void InitTemplate()
            {
                Pic = FindChild<Image>("Pic");
                Icon = FindChild<Image>("Icon");
                lb_name = FindChild<Text>("lb_name");
                lb_Level = FindChild<Text>("lb_Level");
                Selected = FindChild<Image>("Selected");
                BtClick = FindChild<Button>("BtClick");

            }
        }


        protected Button ButtonClose;
        protected VerticalLayoutGroup Content;
        protected Image Desc_Root;
        protected Text lb_sel_name;
        protected Text lb_sel_level;
        protected Image SelectedPic;
        protected Image SelectedIcon;
        protected Text des_Text;
        protected Text des_current;
        protected Image NextLevel;
        protected Text des_next;
        protected Image LevelUp;
        protected Text lb_needLevel;
        protected Button bt_level_up;
        protected Image gold_icon;
        protected Text lb_gold;
        protected Image coin_icon;
        protected Text lb_coin;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<VerticalLayoutGroup>("Content");
            Desc_Root = FindChild<Image>("Desc_Root");
            lb_sel_name = FindChild<Text>("lb_sel_name");
            lb_sel_level = FindChild<Text>("lb_sel_level");
            SelectedPic = FindChild<Image>("SelectedPic");
            SelectedIcon = FindChild<Image>("SelectedIcon");
            des_Text = FindChild<Text>("des_Text");
            des_current = FindChild<Text>("des_current");
            NextLevel = FindChild<Image>("NextLevel");
            des_next = FindChild<Text>("des_next");
            LevelUp = FindChild<Image>("LevelUp");
            lb_needLevel = FindChild<Text>("lb_needLevel");
            bt_level_up = FindChild<Button>("bt_level_up");
            gold_icon = FindChild<Image>("gold_icon");
            lb_gold = FindChild<Text>("lb_gold");
            coin_icon = FindChild<Image>("coin_icon");
            lb_coin = FindChild<Text>("lb_coin");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}