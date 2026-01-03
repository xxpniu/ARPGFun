using System;
using System.Linq;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Proto;
using UApp;

namespace Windows
{
    internal partial class UUIMagic
    {
        private CharacterMagicData _selectConfig;
        private HeroMagic _selectMagic;

        private int selected = -1;

        protected override void InitModel()
        {
            base.InitModel();

            ButtonClose.onClick.AddListener(HideWindow);

            bt_level_up.onClick.AddListener(LevelUpCall);
            return;

            async void LevelUpCall()
            {
                //var gate = UApplication.G<GMainGate>();
                var request = new C2G_MagicLevelUp { Level = _selectMagic?.Level ?? 1, MagicId = _selectConfig.ID };
                var res = await GateManager.S.GateFunction.MagicLevelUpAsync(request);
                await UniTask.SwitchToMainThread();
                if (res.Code.IsOk())
                    OnUpdateUIData();
                else
                    UApplication.S.ShowError(res.Code);
            }
        }


        protected override void OnShow()
        {
            base.OnShow();
            OnUpdateUIData();
        }

        protected override void OnUpdateUIData()
        {
            bt_level_up.SetKey("UUIMagic_LevelUp");
            var gata = GateManager.Try();
            var index = 0;
            var configs = ExcelToJSONConfigManager.Find<CharacterMagicData>(t => t.CharacterID == gata.Hero.HeroID
                && ExcelToJSONConfigManager.Find<MagicLevelUpData>(l => l.MagicID == t.ID)?.Count() > 0);


            ContentTableManager.Count = configs.Length;
            foreach (var i in ContentTableManager)
            {
                TryGetHeto(gata.Hero, configs[index].ID, out var m);
                i.Model.SetMagic(configs[index], m);
                i.Model.OnClick = OnItemClick;
                i.Model.UnSelected();
                index++;
            }

            Desc_Root.ActiveSelfObject(false);

            //selected
            if (selected > 0)
                foreach (var i in ContentTableManager)
                    if (i.Model.Config.ID == selected)
                    {
                        OnItemClick(i.Model);
                        break;
                    }
        }

        private bool TryGetHeto(DHero hero, int id, out HeroMagic magic)
        {
            foreach (var m in hero.Magics)
                if (m.MagicKey == id)
                {
                    magic = m;
                    return true;
                }

            magic = null;
            return false;
        }

        private void OnItemClick(ContentTableModel obj)
        {
            if (obj.Magic == null)
            {
                UUIPopup.ShowConfirm(
                    LanguageManager.S["UUIMaigc_Active_Title"],
                    LanguageManager.S["UUIMaigc_Active_Content"],
                    async () =>
                    {
                        var res = await GateManager.S.GateFunction
                            .ActiveMagicAsync(new C2G_ActiveMagic { MagicId = obj.Config.ID });
                        if (res.Code.IsOk())
                            //UApplication.S.ShowNotify("")
                            return;
                        UApplication.S.ShowError(res.Code);
                    });
                return;
            }

            selected = obj.Config.ID;
            foreach (var i in ContentTableManager) i.Model.UnSelected();
            obj.Selected();
            ShowDetail(obj.Config, obj.Magic);
        }

        private async void ShowDetail(CharacterMagicData config, HeroMagic magic)
        {
            _selectConfig = config;
            _selectMagic = magic;

            Desc_Root.ActiveSelfObject(true);
            var level = magic?.Level ?? 1;
            lb_sel_level.SetKey("UUIMagic_SEL_Level", level);
            lb_sel_name.SetKey(config.Name);

            SelectedIcon.sprite = await ResourcesManager.S.LoadIcon(config);
            des_Text.SetKey(config.Description);

            var levelData =
                ExcelToJSONConfigManager.First<MagicLevelUpData>(t => t.Level == level && t.MagicID == config.ID);
            var nextLevel =
                ExcelToJSONConfigManager.First<MagicLevelUpData>(t => t.Level == level + 1 && t.MagicID == config.ID);
            lb_needLevel.SetKey("UUIMagic_NeedLevel", levelData?.NeedLevel);
            coin_icon.ActiveSelfObject(false);
            lb_gold.text = $"{levelData?.NeedGold}";
            des_current.SetKey("UUIMagic_CurrentLevel", levelData?.Description);
            des_next.SetKey("UUIMagic_NextLevel", nextLevel?.Description);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public CharacterMagicData Config;

            public HeroMagic Magic;
            public Action<ContentTableModel> OnClick;

            public override void InitModel()
            {
                Template.BtClick.onClick.AddListener(() => { OnClick?.Invoke(this); });
            }

            internal async void SetMagic(CharacterMagicData config, HeroMagic heroMagic)
            {
                Magic = heroMagic;
                Config = config;
                Template.lb_name.SetKey(config.Name);
                Template.lb_Level.SetKey("UUIMagic_SEL_Level", heroMagic?.Level ?? 1);
                Template.Icon.sprite = await ResourcesManager.S.LoadIcon(config);
            }

            internal void Selected()
            {
                Template.Selected.ActiveSelfObject(true);
            }

            internal void UnSelected()
            {
                Template.Selected.ActiveSelfObject(false);
            }
        }
    }
}