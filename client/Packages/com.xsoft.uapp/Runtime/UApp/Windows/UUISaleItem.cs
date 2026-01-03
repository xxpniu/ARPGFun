using App.Core.Core;
using App.Core.UICore.Utility;
using EConfig;
using ExcelConfig;
using Proto;
using UApp;

namespace Windows
{
    internal partial class UUISaleItem
    {
        private ItemData config;

        private PlayerItem Item;
        private int saleNum = 1;

        protected override void InitModel()
        {
            base.InitModel();
            s_salenum.onValueChanged.AddListener(v =>
            {
                saleNum = (int)v;
                ShowSale();
            });
            bt_close.onClick.AddListener(HideWindow);

            bt_OK.onClick.AddListener(OkCall);
            return;

            async void OkCall()
            {
                if (saleNum == 0) return;

                var saleItem = new C2G_SaleItem.Types.SaleItem { Guid = Item.GUID, Num = saleNum };
                var re = new C2G_SaleItem();
                re.Items.Add(saleItem);
                var r = await GateManager.S.GateFunction.SaleItemAsync(re);
                if (r.Code.IsOk())
                {
                    HideWindow();
                    UApplication.S.ShowNotify(LanguageManager.S["UUISaleItem_Sale_Success"]);
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
            config = ExcelToJSONConfigManager.GetId<ItemData>(Item.ItemID);
            t_name.SetKey(config.Name);

            s_salenum.minValue = 0;
            s_salenum.maxValue = Item.Num;
            s_salenum.value = saleNum = Item.Num;
            bt_OK.SetKey("UI_SALE_ITEM_BT_OK");
            t_title.SetKey("UI_SALE_ITEM_TITLE");
            ShowSale();
        }


        private void ShowSale()
        {
            t_num.text = saleNum.ToString();
            t_pricetotal.text = (saleNum * config.SalePrice).ToString();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public void Show(PlayerItem item)
        {
            Item = item;
            ShowWindow();
        }
    }
}