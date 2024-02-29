using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UGameTools;
using Proto;
using P = Proto.HeroPropertyType;
using ExcelConfig;
using EConfig;
using GameLogic.Game;
using GameLogic;
using Layout.LayoutEffects;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIHeroEquip
    {
        [System.Serializable]
        public class HeroPartData
        {
            public Image icon;
            public Text level;
            public Button bt;
            public Image rootLvl;
        }

        public class PropertyListTableModel : TableItemModel<PropertyListTableTemplate>
        {
            public PropertyListTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string key, string value)
            {
                this.Template.lb_text.text = $"{key}:{value}";
            }
        }
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string label)
            {
                Template.lb_text.text = label;
            }
        }

        private Dictionary<EquipmentType, HeroPartData> Equips;// = new Dictionary<EquipmentType, HeroPartData>(); 

        protected override void InitModel()
        {
            base.InitModel();
            bt_Exit.onClick.AddListener(HideWindow);

            Equips = new Dictionary<EquipmentType, HeroPartData>
            {
                {
                    EquipmentType.Arm,
                    new HeroPartData
                        { bt = equip_weapon, icon = icon_weapon, level = weapon_Lvl, rootLvl = weapLeveRoot }
                },
                {
                    EquipmentType.Head,
                    new HeroPartData { bt = equip_head, icon = icon_head, level = head_Lvl, rootLvl = HeadLevelRoot }
                },
                {
                    EquipmentType.Foot,
                    new HeroPartData { bt = equip_shose, icon = icon_shose, level = shose_Lvl, rootLvl = ShoseLeveRoot }
                },
                {
                    EquipmentType.Body,
                    new HeroPartData { bt = equip_cloth, icon = icon_cloth, level = cloth_Lvl, rootLvl = ClothLeveRoot }
                }
            };

            foreach (var i in Equips)
            {
                i.Value.bt.onClick.AddListener(() => { Click(i.Key); });
            }

            bt_level_up.onClick.AddListener(LevelUpCall);


            take_off.onClick.AddListener(TakeOffCall);
            return;

            async void TakeOffCall()
            {
                if (_selected == null) return;
                var g = UApplication.G<GMainGate>();
                if (!g.Package.Items.TryGetValue(_selected.GUID, out PlayerItem item)) return;
                var config = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
                var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(config.ID);

                var r = await GateManager.S.GateFunction.OperatorEquipAsync(new C2G_OperatorEquip
                    { Guid = _selected.GUID, IsWear = false, Part = (EquipmentType)equip.PartType });
                await UniTask.SwitchToMainThread();
                if (r.Code.IsOk())
                {
                    UApplication.S.ShowNotify("UUIHeroEquip_TakeOff_Result".GetAsFormatKeys(config.Name));
                    Right.ActiveSelfObject(false);
                    _selected = null;
                }
                else
                {
                    UApplication.S.ShowError(r.Code);
                }

            }

            async void LevelUpCall()
            {
                if (_selected == null) return;
                var g = UApplication.G<GMainGate>();
                if (!g.Package.Items.TryGetValue(_selected.GUID, out PlayerItem item)) return;
                var req = new C2G_EquipmentLevelUp { Guid = _selected.GUID, Level = item.Level };
                var r = await GateManager.S.GateFunction.EquipmentLevelUpAsync(req);
                await UniTask.SwitchToMainThread();
                if (r.Code.IsOk())
                {
                    UApplication.S.ShowNotify(r.Level > item.Level
                        ? "UUIHeroEquip_Level_Success".GetAsKeyFormat($" +{r.Level}")
                        : LanguageManager.S["UUIHeroEquip_Level_Failure"]);
                }
                else
                {
                    UApplication.S.ShowError(r.Code);
                }

            }
        }

        private async void Click(EquipmentType key)
        {
            var g = UApplication.G<GMainGate>();
            foreach (var i in g.Hero.Equips)
            {
                if (i.Part != key) continue;
                DisplayEquip(i);
                return;
            }
            Right.ActiveSelfObject(false);
            await UUIManager.S.CreateWindowAsync<UUISelectEquip>(ui=>ui.SetPartType(key).ShowWindow());
        }


        private WearEquip _selected;

        private async void DisplayEquip(WearEquip eq)
        {
            this._selected = eq;
            Right.ActiveSelfObject(true);
            var g = UApplication.G<GMainGate>();
            g.Package.Items.TryGetValue(eq.GUID, out PlayerItem it);

            var item = ExcelToJSONConfigManager.GetId<ItemData>(eq.ItemID);
            var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(item.ID);
            await ResourcesManager.S.LoadIcon(item,s=> icon_right.sprite = s);
            equip_lvl.text = $"+{it!.Level}";
            right_name.SetKey(item.Name);
            des_Text.text = item.Description;
            RightERoot.ActiveSelfObject(it.Level > 0);
            var level = ExcelToJSONConfigManager.First<EquipmentLevelUpData>(t => t.Level == it.Level && t.Quality == item.Quality);
            var next = ExcelToJSONConfigManager.First<EquipmentLevelUpData>(t => t.Level == it.Level+1 && t.Quality == item.Quality);
            LevelUp.ActiveSelfObject(next != null);

            if (next != null)
            {
                lb_pro.text = LanguageManager.S.Format("UUIHeroEquip_pro",$"{next.Pro / 100}");
                gold_icon.ActiveSelfObject(next.CostGold > 0);
                coin_icon.ActiveSelfObject(next.CostCoin > 0);
                lb_gold.text = $"{next.CostGold}";
                lb_coin.text = $"{next.CostCoin}";
            }
  
            var properties =  it.GetProperties();

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                var stat = ExcelToJSONConfigManager.GetId<StatData>((int)i.Key);
                EquipmentPropertyTableManager[index]
                    .Model.SetLabel($"{LanguageManager.S[stat.WordKey]}:{i.Value.ToValueString(i.Key)}");

                index++;
            }

        }

        protected override void OnShow()
        {
            base.OnShow();
            Right.ActiveSelfObject(false);
            
            var g = UApplication.G<GMainGate>();
            ShowHero(g.Hero, g.Package);

            take_off.SetKey("UUIHeroEquip_Take_off");
            bt_level_up.SetKey("UUIHeroEquip_bt_level_up");

        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            var g = UApplication.G<GMainGate>();
            ShowHero(g.Hero, g.Package);
            if (_selected == null) Right.ActiveSelfObject(false);
            else DisplayEquip(_selected);
        }

        private async void ShowHero(DHero dHero,PlayerPackage package)
        {
            this.Level.text = LanguageManager.S.Format("UUIHeroEquip_level", $"{dHero.Level}");
            var data = ExcelToJSONConfigManager.GetId<CharacterData>(dHero.HeroID);
            var statData = ExcelToJSONConfigManager.Find<StatData>();
            var properties = new Dictionary<P, ComplexValue>();
            foreach (var i in statData)
            {
                properties.Add((P)i.ID, i.InitValue);
            }

            //var nextLevel = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == dHero.Level + 1);

            foreach (var i in Equips)
            {
                i.Value.icon.ActiveSelfObject(false);
                i.Value.level.text = string.Empty;
                i.Value.rootLvl.ActiveSelfObject(false);
            }

            foreach (var i in dHero.Equips)
            {
                if (!package.Items.TryGetValue(i.GUID, out PlayerItem pItem)) continue;
                var item = ExcelToJSONConfigManager.GetId<ItemData>(i.ItemID);
                var equip = ExcelToJSONConfigManager.GetId<EquipmentData>(item.ID);
                if (equip == null) continue;
                var ps = pItem.GetProperties();
                foreach (var kv in ps)
                {
                    if (properties.TryGetValue(kv.Key, out var v))
                    {
                        v.ModifyValueAdd(AddType.Append, kv.Value);
                    }
                }
                if (!Equips.TryGetValue((EquipmentType)equip.PartType, out var partIcon)) continue;
                partIcon.icon.ActiveSelfObject(true);
                await ResourcesManager.S.LoadIcon(item, s => partIcon.icon.sprite = s);
                if (pItem.Level > 0) partIcon.level.text = $"+{pItem.Level}";
                partIcon.rootLvl.ActiveSelfObject(pItem.Level > 0);
            }
            var keys = properties.Where(t => t.Value > 0).ToArray();
            PropertyListTableManager.Count = keys.Length;
            int index = 0;
            foreach (var i in keys)
            {
                var stat = ExcelToJSONConfigManager.GetId<StatData>((int)i.Key);
                PropertyListTableManager[index].Model.SetLabel(LanguageManager.S[stat.WordKey], $"{i.Value.ToValueString(i.Key)}");
                index++;
            }

        }
    }
}