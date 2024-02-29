using System;
using System.Linq;
using Proto;
using ExcelConfig;
using EConfig;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIPackage
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                //todo
                Template.ItemBg.onClick.AddListener(
                    () =>
                    {
                        if (OnClickItem == null)
                            return;
                        OnClickItem(this);
                    });
            }
            public Action<ContentTableModel> OnClickItem;
            public ItemData Config;
            public PlayerItem PItem;
            public async void SetItem(PlayerItem item,bool isWear)
            {
                var itemConfig = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
                Config = itemConfig;
                PItem = item; 
                Template.ItemCount.ActiveSelfObject(item.Num > 1);
                Template.lb_count.text = item.Num>1? item.Num.ToString():string.Empty;
                Template.icon.sprite = await ResourcesManager.S.LoadIcon(itemConfig);
                Template.lb_level.text = item.Level > 0 ? $"+{item.Level}" : string.Empty;
                Template.ItemLevel.ActiveSelfObject(item.Level > 0);
                Template.lb_Name.SetKey(itemConfig.Name);
                Template.Locked.ActiveSelfObject(item.Locked);
                Template.WearOn.ActiveSelfObject(isWear);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(
                HideWindow);

            Bt_Buy.onClick.AddListener(() =>
            {
                UUIPopup.ShowConfirm(LanguageManager.S["UI_PACKAGE_BUY_SIZE_TITLE"],
                    LanguageManager.S.Format("UI_PACKAGE_BUY_SIZE", UApplication.S.Constant.PACKAGE_BUY_COST,
                        UApplication.S.Constant.PACKAGE_BUY_SIZE),
                    OkCallBack);
                return;

                async void OkCallBack()
                {
                    var gate = UApplication.G<GMainGate>();
                    var res = await GateManager.S.GateFunction.BuyPackageSizeAsync(new C2G_BuyPackageSize { SizeCurrent = gate.Package.MaxSize });
                    await UniTask.SwitchToMainThread();
                    if (res.Code.IsOk())
                    {
                        gate.Package.MaxSize = res.PackageCount;
                        OnUpdateUIData();
                    }
                    else
                        UApplication.S.ShowError(res.Code);
                }
            });
        }

        protected override void OnShow()
        {
            base.OnShow();
            lb_title.SetKey("UI_PACKAGE_UI_TITLE");
            Bt_Buy.SetKey("UI_PACKAGE_UI_BUY_BT");
            TextSizeTitle.SetKey("UI_PACKAGE_UI_SIZE_TITLE");
            OnUpdateUIData();

        }
        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            var gate = UApplication.G<GMainGate>();

            lb_TextCountCur.text = $"{ gate.Package.Items.Count}";
            lb_TextCountSize.text = $"/{gate.Package.MaxSize}";
           
            var hero = gate.Hero;
            var index = 0;

            var items = gate.Package.Items
                .Select(t => t.Value).OrderBy(t => t.CreateTime).ToArray();
            ContentTableManager.Count = items.Length;
            foreach (var item in items)
            {
                var i = ContentTableManager[index];
                i.Model.SetItem(item,IsWear(item.GUID,hero));
                i.Model.OnClickItem = ClickItem;
                index++;
            }
        }

        private bool IsWear(string guid, DHero hero)
        {
            foreach (var i in hero.Equips)
                if (i.GUID == guid) return true;
            return false;
        }

        private async void ClickItem(ContentTableModel item)
        {
            await UUIManager.S.CreateWindowAsync<UUIDetail>(ui => ui.Show(item.PItem));
        }
    }
}