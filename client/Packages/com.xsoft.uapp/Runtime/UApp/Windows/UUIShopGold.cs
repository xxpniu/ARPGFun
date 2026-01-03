using System;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Proto;
using UApp;

namespace Windows
{
    internal partial class UUIShopGold
    {
        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(HideWindow);
        }

        protected override void OnShow()
        {
            base.OnShow();

            var goldItems = ExcelToJSONConfigManager.Find<GoldShopData>();

            ContentsTableManager.Count = goldItems.Length;
            var index = 0;
            foreach (var i in ContentsTableManager)
            {
                i.Model.SetConfig(goldItems[index]);
                i.Model.OnClick = OnItemClick;
                index++;
            }
        }

        private void OnItemClick(ContentsTableModel obj)
        {
            UUIPopup.ShowConfirm(LanguageManager.S["UUIShopGold_Title"],
                LanguageManager.S.Format("UUIShopGold_Content",
                    LanguageManager.S[obj.Config.Name]), OkCallBack);
            return;

            async void OkCallBack()
            {
                var gate = GateManager.Try();
                var request = new C2G_BuyGold { ShopId = obj.Config.ID };
                var res = await GateManager.S.GateFunction.BuyGoldAsync(request);
                await UniTask.SwitchToMainThread();
                if (res.Code.IsOk())
                {
                    gate.coin = res.Coin;
                    gate.gold = res.Gold;
                    UApplication.S.ShowNotify(LanguageManager.S.Format("UUIShopGold_Receive_gold", res.ReceivedGold));
                }
                else
                {
                    UApplication.S.ShowError(res.Code);
                }
            }
        }

        public class ContentsTableModel : TableItemModel<ContentsTableTemplate>
        {
            public GoldShopData Config;

            public Action<ContentsTableModel> OnClick;

            public override void InitModel()
            {
                Template.ButtonBlue.onClick.AddListener(() => OnClick?.Invoke(this));
            }

            internal async void SetConfig(GoldShopData item)
            {
                Config = item;
                Template.icon.sprite = await ResourcesManager.S.LoadIcon(item);
                Template.lb_gold.text = $"{item.ReceiveGold}";
                Template.ButtonBlue.SetText($"{item.Prices}");
                Template.lb_name.SetKey(item.Name);
            }
        }
    }
}