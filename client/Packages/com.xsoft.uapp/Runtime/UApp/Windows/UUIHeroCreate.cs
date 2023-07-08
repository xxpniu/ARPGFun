using System;
using UnityEngine.UI;
using UGameTools;
using ExcelConfig;
using EConfig;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Views;
using Cysharp.Threading.Tasks;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIHeroCreate
    {
        public class ListTableModel : TableItemModel<ListTableTemplate>
        {
            public ListTableModel(){}
            public override void InitModel()
            {
                this.Template.BtHero.onClick.AddListener(() =>
                {
                    OnClick?.Invoke(this);
                });
            }

            internal void SetData(CharacterPlayerData characterPlayer)
            {
                Config = characterPlayer;
                ChaData = ExcelToJSONConfigManager.GetId<CharacterData>(Config.CharacterID);
                this.Template.lb_name.SetKey(ChaData.Name);
            }

            public CharacterPlayerData Config;

            public CharacterData ChaData;

            public Action<ListTableModel> OnClick;
        }

        protected override void InitModel()
        {
            base.InitModel();
            Bt_create.onClick.AddListener(async () =>
            {
                if (string.IsNullOrEmpty(InputField.text) || InputField.text.Length < 2)
                {
                    UApplication.S.ShowNotify("UI_HERONAME_NEED".GetLanguageWord());
                    return;
                }
                var request = new Proto.C2G_CreateHero { HeroID = _selectedID, HeroName = InputField.text };
                var r = await GateManager.S.GateFunction.CreateHeroAsync(request);
                Invoke(() =>
                {
                    if (r.Code.IsOk())
                    {
                        UApplication.G<GMainGate>().ShowMain();
                        HideWindow();
                    }
                    else
                        UApplication.S.ShowError(r.Code);
                });
            });

        }

        protected override void OnShow()
        {
            base.OnShow();

            var characters = ExcelToJSONConfigManager.Find<CharacterPlayerData>();

            ListTableManager.Count = characters.Length;
            var index = 0;

            SetHeroId(characters[0],
                ExcelToJSONConfigManager.GetId<CharacterData>(characters[0].CharacterID));
            foreach (var i in characters)
            {
                ListTableManager[index].Model.SetData(characters[index]);
                ListTableManager[index].Model.OnClick = ClickItem;
                index++;
            }
        }

        private int _selectedID = 0;

        private void ClickItem(ListTableModel obj)
        {
            SetHeroId(obj.Config,obj.ChaData);
           
        }

        private void SetHeroId(CharacterPlayerData hero, CharacterData  character)
        {
            _selectedID = character.ID;

            var v =UApplication.G<GMainGate>().CreateOwner(character.ID, character.Name);
            lb_description.SetKey(  hero.Description);

            RunMotion(v, hero.Motion);
        }

        private async void RunMotion(UCharacterView view, string motion)
        {
            await UniTask.Delay(250);

            if (!view) return;
            if (this.CancellationToken.IsCancellationRequested) return;
            view.PlayMotion(motion);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}