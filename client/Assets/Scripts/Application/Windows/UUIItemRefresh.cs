using System;
using System.Collections.Generic;
using UGameTools;
using Proto;
using ExcelConfig;
using EConfig;
using GameLogic.Game;
using P = Proto.HeroPropertyType;
using GameLogic;
using System.Threading.Tasks;
using Core;

namespace Windows
{
    partial class UUIItemRefresh
    {
        public class PropertyListTableModel : TableItemModel<PropertyListTableTemplate>
        {
            public PropertyListTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string v)
            {
                Template.lb_text.text = v;
            }
        }
        public class ItemListTableModel : TableItemModel<ItemListTableTemplate>
        {
            public ItemListTableModel(){}
            public override void InitModel()
            {
                this.Template.BtSelected.onClick.AddListener(() => OnClick?.Invoke(this));
            }

            public Action<ItemListTableModel> OnClick;


            public PlayerItem PlayerItem { private set; get; }
            internal void SetEmpty()
            {
                Template.icon_right.ActiveSelfObject(false);
                Template.AddIconSel.ActiveSelfObject(true);
                Template.ERoot.ActiveSelfObject(false);
            }

            public async void SetPlayItem(PlayerItem item)
            {
                PlayerItem = item;
                Template.ERoot.ActiveSelfObject(item.Level > 0);
                Template.AddIconSel.ActiveSelfObject(false);
                var config = ExcelToJSONConfigManager.GetId<ItemData>(PlayerItem.ItemID);
                Template.equip_lvl.text = $"+{item.Level}";
                await ResourcesManager.S.LoadIcon(config, s =>
                {
                    this.Template.icon_right.sprite = s;
                    Template.icon_right.ActiveSelfObject(true);
                });
            }
        }
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel(){}

            public override void InitModel() { }
  
            internal void SetLabel(string v)
            {
                Template.lb_text.text = v;
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            CloseButton.onClick.AddListener(() => HideWindow());
            equipRefresh.onClick.AddListener(() => SelectedItem());

            bt_level_up.onClick.AddListener(() => BeginRefresh());
        }

        private void BeginRefresh()
        {
            if (currentRefreshData == null) return;
            if (refreshItem == null)
            {
                UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_noItem"]);
                return;
            }
            if (customItems == null || customItems.Count < currentRefreshData.NeedItemCount)
            {
                //UUIItemRefresh_custom_empty
                UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_custom_empty"]);
                return;
            }

            

            var gate = UApplication.G<GMainGate>();
            var request = new C2G_RefreshEquip { EquipUuid = refreshItem.GUID };

            foreach (var i in customItems)
            {
                request.CoustomItem.Add(i.GUID);
            }

            Task.Factory.StartNew(async () =>
            {
                var res = await gate.GateFunction.RefreshEquipAsync(request);
                Invoke(() =>
                {
                    if (res.Code.IsOk())
                    {
                        UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_Sucess"]);
                        return;
                    }
                    UApplication.S.ShowError(res.Code);
                });
            });
        }


        private void SelectedItem()
        {
            UUIManager.S.CreateWindowAsync<UUISelectItem>(ui =>
            {
                ui.ShowSelect(1, false);
                ui.OnSelectedItems = OnSelectRefresh;
            } );
        }

        private void OnSelectRefresh(List<PlayerItem> obj)
        {
            customItems = null;
            refreshItem = obj[0];
            ShowRefreshItem(refreshItem);
            ShowRight(refreshItem);
        }

        private PlayerItem refreshItem;
        private EquipRefreshData currentRefreshData;
        private List<PlayerItem> customItems;

        private void ShowRight(PlayerItem item)
        {
            var config = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
            var refreshData = currentRefreshData;
            if (refreshData.MaxRefreshTimes <= item.Data?.RefreshTime)
            {
                UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_max_times"]);
                Right.ActiveSelfObject(false);
                return;
            }

            Right.ActiveSelfObject(true);
            
            var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(config.ID);
            //var refreshData = currentRefreshData = ExcelToJSONConfigManager.GetId<EquipRefreshData>(config.Quality);
            ItemListTableManager.Count = refreshData.NeedItemCount;
            foreach (var i in ItemListTableManager)
            {
                i.Model.SetEmpty();
                i.Model.OnClick = ClickCustom;
            }
            LevelUp.ActiveSelfObject(refreshData.MaxRefreshTimes > item.Data?.RefreshTime);
            lb_pro.SetKey("UUIRefreshItem_pro", currentRefreshData.Pro / 100);
            coin_icon.ActiveSelfObject(false);
            lb_gold.text = $"{currentRefreshData.CostGold}";
            EquipmentPropertyTableManager.Count = 0;
        }

        private void ClickCustom(ItemListTableModel obj)
        {
            UUIManager.S.CreateWindowAsync<UUISelectItem>(ui =>
            {
                ui.ShowSelect(currentRefreshData?.NeedItemCount??1,true, refreshItem.GUID, currentRefreshData.NeedQuality);
                ui.OnSelectedItems = OnSelectCustomItems;
            });
        }

        private void OnSelectCustomItems(List<PlayerItem> obj)
        {
            customItems = obj;
            int index = 0;
            foreach (var i in ItemListTableManager)
            {
                i.Model.SetPlayItem(obj[index]);
                index++;
            }
            ShowProperty(obj);
        }
        private void ShowProperty(List<PlayerItem> obj)
        {
            var properties = new Dictionary<P, ComplexValue>();
            foreach (var it in obj)
            {
                var item = ExcelToJSONConfigManager.GetId<ItemData>(it.ItemID);
                var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(item.ID);
                var pro = equip.Properties.SplitToInt();
               
                for (var ip = 0; ip < pro.Count; ip++)
                {
                    var pr = (P)pro[ip];
                    var fpv = ExcelToJSONConfigManager.GetId<RefreshPropertyValueData>((int)pr);
                    if (fpv == null) continue;
                    if (!properties.ContainsKey(pr))  properties.Add(pr, 0);
                }
            }

            foreach (var i in properties)
            {
                var fpv = ExcelToJSONConfigManager.GetId<RefreshPropertyValueData>((int)i.Key);
                i.Value.SetBaseValue(fpv.Value);
            }

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                var stat = ExcelToJSONConfigManager.GetId<EConfig.StatData>((int)i.Key);
                EquipmentPropertyTableManager[index]
                    .Model.SetLabel(
                    $"{stat.WordKey.GetLanguageWord()}:{currentRefreshData.PropertyAppendMin * i.Value.FinalValue}~{currentRefreshData.PropertyAppendMax * i.Value.FinalValue}");
                index++;
            }
        }


        private async void ShowRefreshItem(PlayerItem it)
        {
            var item = ExcelToJSONConfigManager.GetId<ItemData>(it.ItemID);
            var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(item.ID);
            await ResourcesManager.S.LoadIcon(item, s => icon.sprite = s);
            currentRefreshData = ExcelToJSONConfigManager.GetId<EquipRefreshData>(item.Quality);
            lb_Lvl.text = $"+{it.Level}";
            lb_equipname.SetKey(item.Name);
            lb_description.SetKey( item.Description);
            lb_equiprefresh.SetKey("UUIItemRefresh_RefreshTimes", $"{ currentRefreshData.MaxRefreshTimes - it.Data?.RefreshTime}");
            LevelRoot.ActiveSelfObject(it.Level > 0);
            var properties = it.GetProperties();
            PropertyListTableManager .Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                PropertyListTableManager[index]
                    .Model.SetLabel(
                    $"{i.Key.ToWord()}:{i.Value.ToString()}");

                index++;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            customItems = null;
            lb_Lvl.text =
            lb_equipname.text = lb_equiprefresh.text =
            lb_description.text = string.Empty;

            LevelRoot.ActiveSelfObject(false);
            Right.ActiveSelfObject(false);
            lb_custom_title.SetKey("UUIItemRefresh_custom_title");
            lb_custom_property_title.SetKey("UUIItemRefresh_refresh_got_title");
            SelectedItem();
        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            if (refreshItem != null)
            {
                var gata = UApplication.G<GMainGate>();
                if (gata.package.Items.TryGetValue(refreshItem.GUID, out refreshItem))
                {
                    OnSelectRefresh(new List<PlayerItem> { refreshItem });
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}