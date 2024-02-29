using System;
using UGameTools;
using Google.Protobuf.Collections;
using Proto;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIItemShop
    {
        public RepeatedField<ItemsShop> Shops { get; private set; }

        public class ShopTabTableModel : TableItemModel<ShopTabTableTemplate>
        {
            public ShopTabTableModel(){}
            public override void InitModel()
            {
                this.Template.ToggleSelected.onValueChanged.AddListener((isOn) => {
                    if (isOn) OnSelected?.Invoke(this);
                });
            }
            public Action<ShopTabTableModel> OnSelected;

            public ItemsShop Shop { get; private set; }

            internal void SetData(ItemsShop itemsShop)
            {
                this.Shop = itemsShop;
                var config = ExcelConfig.ExcelToJSONConfigManager.GetId<EConfig.ItemShopData>(itemsShop.ShopId);
                this.Template.ShopName.SetKey(config?.Name);
            }
        }
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.ButtonCoin.onClick.AddListener(() => {
                    OnBuy?.Invoke(this);
                });

                Template.ButtonGold.onClick.AddListener(() => {
                    OnBuy?.Invoke(this);
                });

                Template.ItemBg.onClick.AddListener(() => { OnDetail?.Invoke(this); });
            }

            public Action<ContentTableModel> OnDetail;

            public Action<ContentTableModel> OnBuy;

            public ItemsShop.Types.ShopItem ShopItem { get; private set; }
            public ItemsShop Shop { get; private set; }
            public EConfig.ItemData Config { get; private set; }
            internal async void SetItem(ItemsShop.Types.ShopItem shopItem, ItemsShop shop)
            {
                this.ShopItem = shopItem;
                this.Shop = shop;
                Config = ExcelConfig.ExcelToJSONConfigManager.GetId<EConfig.ItemData>(ShopItem.ItemId);
                await ResourcesManager.S.LoadIcon(Config,s=> Template.icon.sprite = s);
                Template.Name.SetKey(Config.Name);
                Template.ItemCount.ActiveSelfObject(shopItem.PackageNum > 1);
                Template.t_num.text = $"{ShopItem.PackageNum}";
                Template.ButtonCoin.ActiveSelfObject(ShopItem.CType == ItemsShop.Types.CoinType.Coin);
                Template.ButtonGold.ActiveSelfObject(ShopItem.CType == ItemsShop.Types.CoinType.Gold);
                Template.ButtonGold.SetText($"{shopItem.Prices}");
                Template.ButtonCoin.SetText($"{shopItem.Prices}");
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(HideWindow);
        }

        protected override void OnShow()
        {
            base.OnShow();
            ShowData();
        }


        private async void ShowData()
        {
            var r = await GateManager.S.GateFunction.QueryShopAsync(new C2G_Shop());
            await UniTask.SwitchToMainThread();
            if (!r.Code.IsOk())
            {
                HideWindow();
                UApplication.S.ShowError(r.Code);
                return;
            } 
            this.Shops = r.Shops;
            this.ShopTabTableManager.Count = Shops.Count;
            var index = 0;
            foreach (var i in ShopTabTableManager)
            {
                i.Model.SetData(Shops[index]);
                i.Model.OnSelected = Selected;
                index++;
            }
            if (Shops.Count > 0) ShopTabTableManager[0].Template.ToggleSelected.isOn = true;
        }

        private void Selected(ShopTabTableModel obj)
        {
            ContentTableManager.Count = obj.Shop.Items.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetItem(obj.Shop.Items[index], obj.Shop);
                i.Model.OnBuy = Buy;
                i.Model.OnDetail = ShowDetail;
                index++;
            }
        }

        private async void ShowDetail(ContentTableModel obj)
        {
            var item = new PlayerItem { ItemID = obj.Config.ID, Level = 0, Num = obj.ShopItem.PackageNum };
            await UUIManager.S.CreateWindowAsync<UUIDetail>(ui => ui.Show(item, true));
        }

        private void Buy(ContentTableModel obj)
        {
            UUIPopup.ShowConfirm(LanguageManager.S["UUIItemShop_Confirm_Title"],
                LanguageManager.S.Format("UUIItemShop_Confirm_Content", LanguageManager.S[obj.Config.Name]),
                OkCall
            );
            return;

            async void OkCall()
            {
                var request = new C2G_BuyItem { ItemId = obj.ShopItem.ItemId, ShopId = obj.Shop.ShopId };
                var gate = UApplication.G<GMainGate>();
                var r = await GateManager.S.GateFunction.BuyItemAsync(request);
                await UniTask.SwitchToMainThread();
                if (r.Code.IsOk())
                {
                    UApplication.S.ShowNotify(LanguageManager.S.Format("UUIItemShop_BUY", LanguageManager.S[$"{obj.Config.Name}"], $"{obj.ShopItem.PackageNum}"));
                }
                else
                {
                    UApplication.S.ShowError(r.Code);
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }


    }
}