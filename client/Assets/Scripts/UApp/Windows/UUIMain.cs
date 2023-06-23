using UGameTools;
using EConfig;
using ExcelConfig;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIMain
    {

        protected override void InitModel()
        {
            base.InitModel();

            this.MenuMap.onClick.AddListener(async () =>
            {
                var ui = await this.CreateChildWindow<UUILevelList>();
                ui.ShowWindow();
                //await UUIManager.S.CreateWindowAsync<UUILevelList>((ui) => ui.ShowWindow());
            });

            MenuItems.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIPackage>((ui) => ui.ShowWindow());
            });

            MenuSetting.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUISettings>((ui) => ui.ShowWindow());
            });

            MenuWeapon.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIHeroEquip>((ui) => ui.ShowWindow());
            });

            MenuShop.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIItemShop>((ui) => ui.ShowWindow());
            });
            MenuSkill.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIMagic>(ui => { ui.ShowWindow(); });
            });
            MenuMessages.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIMessages>(ui => ui.ShowWindow());
            });

            MenuRefresh.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIItemRefresh>(ui => ui.ShowWindow());
            });

            //user_info.onClick.AddListener(() => {  });

            var swipeEv = swip.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                //v *= .5f;
                var gate = UApplication.G<GMainGate>();
                gate.RotationHero(v.x);
                //.RotationY(v.x);
            });

            btn_goldadd.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIShopGold>(ui => ui.ShowWindow());
            });

            this.Button_Play.onClick.AddListener(async () =>
            {
                var gate = UApplication.G<GMainGate>();
                if(gate ==null) return;
                var rs = await GateManager.S.GateFunction
                    .BeginGameAsync(new Proto.C2G_BeginGame
                {
                    GroupID = gate.Group.Id
                });
                if (!rs.Code.IsOk()) UApplication.S.ShowError(rs.Code);
            });

            bt_invite.onClick.AddListener(async () =>
            {
                var gate = UApplication.G<GMainGate>();
                if (gate.Group == null) return;
                await UUIManager.S.CreateWindowAsync<UUIUserInvite>(ui => ui.ShowWindow());
            });

            bt_Exit.onClick.AddListener(() =>
            {
                UUIPopup.ShowConfirm("Leave_Title".GetLanguageWord(), 
                    "Leave_Content".GetLanguageWord(), async () =>
                { 
                    await GateManager.S.GateFunction.LeaveMatchGroupAsync(new Proto.C2G_LeaveMatchGroup { });
                });
            });

            Button_AddFriend.onClick.AddListener(async () =>
            {
                await UUIManager.S.CreateWindowAsync<UUIUserList>(ui => ui.ShowWindow());
            });
        }



        protected override void OnShow()
        {
            base.OnShow();
            this.Username.text = string.Empty;
            MenuSetting.SetKey("UI_MAIN_SETTING");
            MenuItems.SetKey("UI_MAIN_ITEM");
            MenuWeapon.SetKey("UI_MAIN_WEAPON");
            MenuSkill.SetKey("UI_MAIN_SKILL");
            MenuShop.SetKey("UI_MAIN_SHOP");
            MenuMessages.SetKey("UI_MAIN_MESSAGE");
            Button_Play.SetKey("UI_MAIN_PLAY");
            MenuRefresh.SetKey("UI_MAIN_Refresh");
            MenuMap.SetKey("UI_MAIN_MAP");
            OnUpdateUIData();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            var gate = UApplication.G<GMainGate>();
            if (gate == null) return;
            if (gate.hero == null) return;
            user_defalut.texture = gate.LookAtView;
            lb_gold.text = gate.Gold.ToString("N0");
            lb_gem.text = gate.Coin.ToString("N0");
            if (gate.hero == null) return;
            this.Level_Number.text = $"{gate.hero.Level}";
            this.Username.text = $"{gate.hero.Name}";
            var leveUp = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == gate.hero.Level + 1);
            lb_exp.text = $"{gate.hero.Exprices}/{leveUp?.NeedExp ?? '-'}";
            float v = 0;
            if (leveUp != null) v = (float)gate.hero.Exprices / leveUp.NeedExp;
            ExpSilder.value = v;

            Root.ActiveSelfObject(gate.Group == null);
            Match.ActiveSelfObject(gate.Group != null);

            if (gate.Group == null) return;
            var level = ExcelToJSONConfigManager.GetId<BattleLevelData>(gate.Group.LevelID);
            Button_Play.SetText(level.Name);
        }
    }
}