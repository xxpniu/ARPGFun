using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GameLogic;
using Proto;
using UApp;
using UGameTools;

namespace Windows
{
    internal partial class UUIDetail
    {
        private ItemData config;

        private PlayerItem item;
        private bool nobt;

        protected override void InitModel()
        {
            base.InitModel();
            bt_cancel.onClick.AddListener(HideWindow);
            bt_sale.onClick.AddListener(SaleCall);
            bt_equip.onClick.AddListener(EquipCall);
            uiRoot.transform.OnMouseClick(_ => { HideWindow(); }).CheckMove = false;

            return;

            async void SaleCall()
            {
                HideWindow();
                await UUIManager.S.CreateWindowAsync<UUISaleItem>(ui => { ui.Show(item); });
            }

            async void EquipCall()
            {
                var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(config.ID);
                if (equip == null) return;
                var rEqu = new C2G_OperatorEquip
                    { IsWear = true, Guid = item.GUID, Part = (EquipmentType)equip.PartType };
                var r = await GateManager.S.GateFunction.OperatorEquipAsync(rEqu);
                await UniTask.SwitchToMainThread();
                if (r.Code.IsOk())
                {
                    UApplication.S.ShowNotify(LanguageManager.S.Format("UUIDETAIL_WEAR_SUCESS",
                        LanguageManager.S[config.Name]));
                    HideWindow();
                }
                else
                {
                    UApplication.S.ShowError(r.Code);
                }
            }
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
            t_num.text = $"{item.Num}";
            t_descript.SetKey(config.Description);
            t_name.SetKey(config.Name);
            t_prices.SetKey("UUIDetail_PRICES", $"{config.SalePrice}");
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
                var g = GateManager.Try();
                var wear = false;
                foreach (var i in g.Hero.Equips)
                {
                    if (i.GUID != item.GUID) continue;
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
            else
            {
                EquipmentPropertyTableManager.Count = 0;
            }
        }


        private void ShowEquip(PlayerItem pItem)
        {
            var properties = pItem.GetProperties();
            EquipmentPropertyTableManager.Count = properties.Count;
            var index = 0;
            foreach (var i in properties)
            {
                var stat = ExcelToJSONConfigManager.GetId<StatData>((int)i.Key);
                EquipmentPropertyTableManager[index]
                    .Model
                    .SetLabel($"{stat.WordKey.GetLanguageWord()}:{i.Value.ToValueString(i.Key)}");
                index++;
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public void Show(PlayerItem playerItem, bool nobt = false)
        {
            this.nobt = nobt;
            item = playerItem;
            ShowWindow();
        }

        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string label)
            {
                Template.lb_text.text = label;
            }
        }
    }
}