using System;
using System.Collections.Generic;
using System.Linq;
using UGameTools;
using Proto;
using EConfig;
using ExcelConfig;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUISelectEquip
    {
        public class PlayerEquipItem
        {
            public PlayerItem Item;
            public EquipmentData data;

        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel() { }

            public Action<ContentTableModel> OnWearClick { get; set; }

            public override void InitModel()
            {
                Template.bt_equip.onClick.AddListener(() => {
                    this.OnWearClick?.Invoke(this);
                });
            }

            public EquipmentData Equip;
            public PlayerItem IItem;

            internal async void SetItem(PlayerItem playerItem)
            {
                Template.bt_equip.SetKey("UUISelectEquip_Wear");
                this.IItem = playerItem;
                var item = ExcelToJSONConfigManager.GetId<ItemData>(playerItem.ItemID);
                Equip = ExcelToJSONConfigManager.GetId<EquipmentData>(item.ID);
                this.Template.lb_level.text = playerItem.Level > 0 ? $"+{ playerItem.Level}" : string.Empty;
                this.Template.lb_Name.SetKey(item.Name);
                this.Template.ItemLevel.ActiveSelfObject(playerItem.Level > 0);
                Template.icon.sprite = await ResourcesManager.S.LoadIcon(item);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            this.bt_cancel.onClick.AddListener(() => { HideWindow(); });
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            if (!_part.HasValue) { HideWindow(); }
            else
                ShowEquipList();
        }



        private void ShowEquipList()
        {
            var equip = new List<PlayerEquipItem>();
            var g =    GateManager.Try();
            foreach (var i in g.Package.Items)
            {
                var item = ExcelToJSONConfigManager.GetId<ItemData>(i.Value.ItemID);
                if ((ItemType)item.ItemType != ItemType.ItEquip) continue;
                var wear = false;
                foreach (var e in g.Hero.Equips)
                {
                    if (e.GUID != i.Key) continue;
                    wear = true;
                    break;
                }
                if (wear) continue ;
                var ec = ExcelToJSONConfigManager.GetId<EquipmentData>(item.ID);
                if ((EquipmentType)ec.PartType != _part) continue;

                equip.Add( new PlayerEquipItem { data = ec, Item = i.Value });

            }

            equip = equip.OrderByDescending(t => t.data.Quality).ToList();

            this.ContentTableManager.Count = equip.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetItem(equip[index].Item);
                i.Model.OnWearClick = WearClick;
                index++;
            }
        }

        private async void WearClick(ContentTableModel obj)
        {
            //var g = UApplication.G<GMainGate>();
            var req = new C2G_OperatorEquip
            {
                Guid = obj.IItem.GUID,
                IsWear = true,
                Part = (EquipmentType)obj.Equip.PartType
            };
            var r = await GateManager.S.GateFunction.OperatorEquipAsync(req);
            await UniTask.SwitchToMainThread();
            if (!r.Code.IsOk())
            {
                UApplication.S.ShowError(r.Code);
            }
            HideWindow();
            
        }

        private  EquipmentType? _part;

        public UUISelectEquip SetPartType(EquipmentType type)
        {
            this._part = type;
            return this;
        }
    }
}