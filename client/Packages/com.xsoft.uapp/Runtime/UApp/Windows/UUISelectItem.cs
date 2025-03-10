using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.Core.Core;
using App.Core.UICore.Utility;
using Proto;
using EConfig;
using ExcelConfig;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUISelectItem
    {

        public class DisplayItemData 
        {
            public PlayerItem Item;
            public ItemData Config;
        }
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.ItemBg.onClick.AddListener(() => OnClickItem?.Invoke(this));
            }
            public Action<ContentTableModel> OnClickItem;
            public ItemData Config;
            public PlayerItem pItem;
            public async void SetItem(PlayerItem item, bool isWear)
            {
                Config = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
                pItem = item;
                Template.ItemCount.ActiveSelfObject(item.Num > 1);
                Template.lb_count.text = item.Num > 1 ? item.Num.ToString() : string.Empty;
                await ResourcesManager.S.LoadIcon(Config, s => Template.icon.sprite = s);
                Template.lb_level.text = item.Level > 0 ? $"+{item.Level}" : string.Empty;
                Template.ItemLevel.ActiveSelfObject(item.Level > 0);
                Template.lb_Name.SetKey(Config.Name);
                Template.Locked.ActiveSelfObject(item.Locked);
                Template.WearOn.ActiveSelfObject(isWear);
                Template.Selected.ActiveSelfObject(false);
            }

            internal void UnSelect()
            {
                Template.Selected.ActiveSelfObject(false);
            }

            internal void Select()
            {
                Template.Selected.ActiveSelfObject(true);
            }
        }
    
        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(HideWindow);
            //Write Code here
        }
        
        private bool IsWear(string guid, DHero hero)
        {
            foreach (var i in hero.Equips)
                if (i.GUID == guid) return true;
            return false;
        }
        
        protected override void OnShow()
        {
            base.OnShow();
            var hero =   GateManager.Try().Hero;
            ContentTableManager.Count = ListItems.Count;
            var  index = 0;
            foreach (var i in ContentTableManager)
            {
                var t = ListItems[index].Item;
                
                i.Model.SetItem(t, IsWear(t.GUID, hero));
                i.Model.OnClickItem = ClickItem;
                index++;
            }

        }

        private void ClickItem(ContentTableModel obj)
        {
            if (selected.Contains(obj))
            {
                selected.Remove(obj);
                obj.UnSelect();
            }

            else
            {
                selected.Add(obj);
                obj.Select();

                if (selected.Count == needcount)
                {
                    var list = new List<PlayerItem>();
                    foreach (var i in selected)
                        list.Add(i.pItem);
                    OnSelectedItems?.Invoke(list);
                    HideWindow();
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public Action<List<PlayerItem>> OnSelectedItems;

        private List<ContentTableModel> selected = new List<ContentTableModel>();

        private List<DisplayItemData> ListItems;

        private int needcount = 0;

        public void ShowSelect(int count, bool nowear, string exceptId = null, int quality=-1)
        {
            needcount = count;
            var gate =  GateManager.Try();
            ListItems = gate.Package.Items.Where(t => t.Key != exceptId)
                .Select(t => new DisplayItemData
                {
                    Item = t.Value,
                    Config = ExcelToJSONConfigManager.GetId<ItemData>(t.Value.ItemID)
                }).Where(t => t.Config.ItemType == (int)ItemType.ItEquip && t.Config.Quality>=quality)
                .Select(t => t).ToList();

            if (nowear)
            {
                HashSet<string> wears = new HashSet<string>();
                foreach (var i in gate.Hero.Equips)
                {
                    wears.Add(i.GUID);
                }

                ListItems= ListItems.Where(t => !wears.Contains(t.Item.GUID)).ToList();

            }



            ShowWindow();
        }
        
    }
}