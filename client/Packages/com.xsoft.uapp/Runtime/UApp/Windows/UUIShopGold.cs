using System;
using UGameTools;
using EConfig;
using Proto;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using ExcelConfig;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIShopGold
    {
        public class ContentsTableModel : TableItemModel<ContentsTableTemplate>
        {
            public ContentsTableModel(){}
            public override void InitModel()
            {
                Template.ButtonBlue.onClick.AddListener(() => OnClick?.Invoke(this));
            }

            public Action<ContentsTableModel> OnClick;

            public GoldShopData Config;

            internal async void SetConfig(GoldShopData item)
            {
                this.Config = item;
                Template.icon.sprite = await ResourcesManager.S.LoadIcon(item);
                Template.lb_gold.text = $"{item.ReceiveGold}";
                Template.ButtonBlue.SetText($"{item.Prices}");
                Template.lb_name.SetKey(item.Name);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(HideWindow) ;
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

    }
}