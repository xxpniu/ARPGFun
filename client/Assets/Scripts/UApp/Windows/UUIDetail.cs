using UGameTools;
using Proto;
using ExcelConfig;
using EConfig;
using GameLogic;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIDetail
    {
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel() { }
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string label)
            {
                Template.lb_text.text = label;
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            bt_cancel.onClick.AddListener(() =>
                {
                    HideWindow();
                });
            bt_sale.onClick.AddListener(() =>
                {
                    this.HideWindow();
                   UUIManager.S.CreateWindowAsync<UUISaleItem>((ui)=> {
                       ui.Show(this.item);
                   });
                });
            bt_equip.onClick.AddListener(() =>
            {
                
                
                var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(config.ID);
                if (equip == null) return;

                var requ = new C2G_OperatorEquip
                {
                    IsWear = true,
                    Guid = item.GUID,
                    Part = (EquipmentType)equip.PartType
                };

                Task.Factory.StartNew(async () =>
                {
                    var r = await UApplication.G<GMainGate>().GateFunction.OperatorEquipAsync(requ);
                    this.Invoke(() =>
                    {
                        if (r.Code.IsOk())
                        {
                            UApplication.S.ShowNotify(
                                LanguageManager.S.Format("UUIDETAIL_WEAR_SUCESS",
                                LanguageManager.S[config.Name]));
                            HideWindow();
                        }
                        else
                        {
                            UApplication.S.ShowError(r.Code);
                        }
                    });
                });

            });

            this.uiRoot.transform.OnMouseClick((obj) =>
            {
                HideWindow();
            });
            //Write Code here
        }

        protected override void OnShow()
        {
            base.OnShow();
            ShowData();
        }

        private async void ShowData()
        {
            
            bt_equip.SetKey("UUIDetail_WEAR");
            bt_sale.SetKey("UUIDetail_SELL");

            config = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
            t_num.text = $"{ item.Num}";
            t_descript.SetKey(config.Description);
            t_name.SetKey(config.Name);
            t_prices.SetKey("UUIDetail_PRICES", $"{ config.SalePrice}") ;
            icon.sprite = await ResourcesManager.S.LoadIcon(config);

            ItemLevel.ActiveSelfObject(item.Level > 0);
            lb_level.text = $"{item.Level}";
            ItemCount.ActiveSelfObject(item.Num > 1);
            Locked.ActiveSelfObject(item.Locked);
            
           

            if (nobt)
            {
                bt_equip.ActiveSelfObject(false);
                bt_sale.ActiveSelfObject(false);
                WearOn.ActiveSelfObject(false);
            }
            else
            {
                var g = UApplication.G<GMainGate>();
                var wear = false;
                foreach (var i in g.hero.Equips)
                    if (i.GUID == item.GUID)
                    {
                        wear = true;
                        break;
                    }
                WearOn.ActiveSelfObject(wear);
                bt_equip.ActiveSelfObject(!wear && (ItemType)config.ItemType == ItemType.ItEquip);
                bt_sale.ActiveSelfObject(!wear);
            }

            if ((ItemType)config.ItemType == ItemType.ItEquip)
            {
                var eq = ExcelToJSONConfigManager.GetId<EquipmentData>(config.ID);
                ShowEquip(item);
            }
            else {
                EquipmentPropertyTableManager.Count = 0;
            }
        }


        private void ShowEquip(PlayerItem pItem)
        {
            var properties = pItem.GetProperties();
            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                var stat = ExcelToJSONConfigManager.GetId<StatData>((int)i.Key);
                EquipmentPropertyTableManager[index]
                    .Model
                    .SetLabel($"{stat.WordKey.GetLanguageWord()}:{i.Value.ToValueString(i.Key)}");
                index++;
            }
        }

        private ItemData config;
        private bool nobt = false;

        protected override void OnHide()
        {
            base.OnHide();
        }

        private PlayerItem item;

        public void Show(PlayerItem item,bool nobt =false)
        {
            this.nobt = nobt;
            this.item = item;
            this.ShowWindow();
        }
    }
}