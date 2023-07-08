using System;
using System.Linq;
using UGameTools;
using Proto;
using EConfig;
using ExcelConfig;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIMagic
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.BtClick.onClick.AddListener(() => { OnClick?.Invoke(this); });
            }

            public HeroMagic magic;
            public CharacterMagicData config;
            public Action<ContentTableModel> OnClick;

            internal async void SetMagic(CharacterMagicData config,HeroMagic heroMagic)
            {
                magic = heroMagic;
                this.config = config ;
                Template.lb_name.SetKey(config.Name);
                Template.lb_Level.SetKey("UUIMagic_SEL_Level", heroMagic?.Level ?? 1);
                Template.Icon.sprite = await  ResourcesManager.S.LoadIcon(config);
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

        protected override void InitModel()
        {
            base.InitModel();

            ButtonClose.onClick.AddListener(this.HideWindow);
            bt_level_up.onClick.AddListener(async () =>
                {
                    var gate = UApplication.G<GMainGate>();
                    var request = new C2G_MagicLevelUp { Level = selectMagic?.Level ?? 1, MagicId = selectConfig.ID };
                    var res = await GateManager.S.GateFunction.MagicLevelUpAsync(request);
                    Invoke(() =>
                    {
                        if (res.Code.IsOk())
                        {
                            OnUpdateUIData();
                        }
                        else
                        {
                            UApplication.S.ShowError(res.Code);
                        }
                    });
                }
            );
        }


        protected override void OnShow()
        {
            base.OnShow();
            OnUpdateUIData();
        }

        protected override void OnUpdateUIData()
        {

            bt_level_up.SetKey("UUIMagic_LevelUp");
            var gata = UApplication.G<GMainGate>();
            int index = 0;
            var configs = ExcelToJSONConfigManager.Find<CharacterMagicData>(t => t.CharacterID == gata.Hero.HeroID
                && ExcelToJSONConfigManager.Find<MagicLevelUpData>(l=>l.MagicID == t.ID)?.Count()>0);


            ContentTableManager.Count = configs.Length;
            foreach (var i in ContentTableManager)
            {
                TryGetHeto(gata.Hero, configs[index].ID, out HeroMagic m);
                i.Model.SetMagic(configs[index], m);
                i.Model.OnClick = OnItemClick;
                i.Model.UnSelected();
                index++;
            }
            Desc_Root.ActiveSelfObject(false);

            //selected
            if (selected > 0)
            {
                foreach (var i in ContentTableManager)
                {
                    if (i.Model.config.ID == selected)
                    {
                        OnItemClick(i.Model);
                        break;
                    }
                }
            }

        }

        private bool TryGetHeto(DHero hero,int id,out HeroMagic magic)
        {
            foreach (var m in hero.Magics)
            {
                if (m.MagicKey == id)
                {
                    magic = m;
                    return true;
                }
            }
            magic = null;
            return false;
        }

        private int selected=-1;

        private void OnItemClick(ContentTableModel obj)
        {

            if (obj.magic == null)
            {
                UUIPopup.ShowConfirm(
                    LanguageManager.S["UUIMaigc_Active_Title"],
                    LanguageManager.S["UUIMaigc_Active_Content"],
                    async () =>
                    {
                        var res = await GateManager.S.GateFunction
                            .ActiveMagicAsync(new C2G_ActiveMagic { MagicId = obj.config.ID });
                        if (res.Code.IsOk())
                        {
                            //UApplication.S.ShowNotify("")
                            return;
                        }
                        UApplication.S.ShowError(res.Code);

                    });
                return;
            }

            selected = obj.config.ID;
            foreach (var i in ContentTableManager) i.Model.UnSelected();
            obj.Selected();
            ShowDetail(obj.config, obj.magic);
        }

        private async void ShowDetail(CharacterMagicData config, HeroMagic magic)
        {
            this.selectConfig = config;
            selectMagic = magic;

            Desc_Root.ActiveSelfObject(true);
            int level = magic?.Level ?? 1;
            lb_sel_level.SetKey("UUIMagic_SEL_Level", level);
            lb_sel_name.SetKey(config.Name);
            
            SelectedIcon.sprite = await ResourcesManager.S.LoadIcon(config);
            des_Text.SetKey(config.Description);

            var levelData = ExcelToJSONConfigManager.First<MagicLevelUpData>(t => t.Level == level && t.MagicID == config.ID);
            var nextLevel= ExcelToJSONConfigManager.First<MagicLevelUpData>(t => t.Level == level+1 && t.MagicID == config.ID);
            lb_needLevel.SetKey("UUIMagic_NeedLevel", levelData.NeedLevel);
            coin_icon.ActiveSelfObject(false);
            lb_gold.text =$"{levelData?.NeedGold}";
            des_current.SetKey("UUIMagic_CurrentLevel", levelData?.Description);
            des_next.SetKey("UUIMagic_NextLevel", nextLevel.Description);
        }

        private CharacterMagicData selectConfig;
        private HeroMagic selectMagic;

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}