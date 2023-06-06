using UGameTools;
using EConfig;
using ExcelConfig;
using System.Threading.Tasks;
using Core;

namespace Windows
{
    partial class UUIMain
    {

        protected override void InitModel()
        {
            base.InitModel();
     
            this.MenuMap.onClick.AddListener(() =>
            {
                UUIManager.Singleton.CreateWindowAsync<UUILevelList>((ui) => ui.ShowWindow() );
            });

            MenuItems.onClick.AddListener(() =>
                {
                    UUIManager.S.CreateWindowAsync<UUIPackage>((ui)=>ui.ShowWindow());
                });

            MenuSetting.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindowAsync<UUISettings>((ui) => ui.ShowWindow());
            });

            MenuWeapon.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindowAsync<UUIHeroEquip>((ui) => ui.ShowWindow());
            });

            MenuShop.onClick.AddListener(() =>
            {

                UUIManager.S.CreateWindowAsync<UUIItemShop>((ui) => ui.ShowWindow());

            });
            MenuSkill.onClick.AddListener(() => {
                UUIManager.S.CreateWindowAsync<UUIMagic>(ui =>
                {
                    ui.ShowWindow();
                });
            });
            MenuMessages.onClick.AddListener(() => {
                UUIManager.S.CreateWindowAsync<UUIMessages>(ui => ui.ShowWindow());
            });

            MenuRefresh.onClick.AddListener(() => {
                UUIManager.S.CreateWindowAsync<UUIItemRefresh>(ui => ui.ShowWindow());
            });

            //user_info.onClick.AddListener(() => {  });

            var swipeEv = swip.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                //v *= .5f;
                //ThridPersionCameraContollor.Current.RotationX(v.y);
                var gate = UApplication.G<GMainGate>();
                gate.RotationHero(v.x);
                //.RotationY(v.x);
            });

            btn_goldadd.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindowAsync<UUIShopGold>(ui => ui.ShowWindow());
            });

            this.Button_Play.onClick.AddListener(() =>
            {
                var gate = UApplication.G<GMainGate>();
                if (gate.Group == null) return;
                Task.Factory.StartNew(async () =>
                {
                   var rs = await gate.GateFunction.BeginGameAsync(new Proto.C2G_BeginGame { GroupID = gate.Group.Id });
                    if (!rs.Code.IsOk())
                        Invoke(() => { UApplication.S.ShowError(rs.Code); });
                });
            });

            bt_invite.onClick.AddListener(() => {
                var gate = UApplication.G<GMainGate>();
                if (gate.Group == null) return;
                UUIManager.S.CreateWindowAsync<UUIUserInvite>(ui => ui.ShowWindow());
            });

            bt_Exit.onClick.AddListener(() =>
            {

                UUIPopup.ShowConfirm("Leave_Title".GetLanguageWord(), "Leave_Content".GetLanguageWord(), () =>
                {
                    var gate = UApplication.G<GMainGate>();
                    if (gate.Group == null) return;
                    Task.Factory.StartNew(async () =>
                    {
                       await gate.GateFunction.LeaveMatchGroupAsync(new Proto.C2G_LeaveMatchGroup { });
                    });
                });
            });

            Button_AddFriend.onClick.AddListener(() => {
                UUIManager.S.CreateWindowAsync<UUIUserList>(ui => ui.ShowWindow());
            });
            //Write Code here
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
            var leveUp = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == gate.hero.Level+1);
            lb_exp.text = $"{gate.hero.Exprices}/{leveUp?.NeedExp ?? '-'}";
            float v = 0;
            if (leveUp != null)  v = (float)gate.hero.Exprices / leveUp.NeedExp;
            ExpSilder.value = v;

            Root.ActiveSelfObject(gate.Group == null);
            Match.ActiveSelfObject(gate.Group != null);

            if (gate.Group != null)
            {
                var level = ExcelToJSONConfigManager.GetId<BattleLevelData>(gate.Group.LevelID);
                Button_Play.SetText(level.Name);
            }
        }
    }
}