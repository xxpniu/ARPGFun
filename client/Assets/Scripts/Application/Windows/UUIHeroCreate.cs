using System;
using UnityEngine.UI;
using UGameTools;
using ExcelConfig;
using EConfig;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using BattleViews.Views;
using Core;
using Cysharp.Threading.Tasks;

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
            Bt_create.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(InputField.text) || InputField.text.Length < 2)
                {
                    UApplication.S.ShowNotify("UI_HERONAME_NEED".GetLanguageWord());
                    return;
                }

                var request = new Proto.C2G_CreateHero { HeroID = selectedID, HeroName = InputField.text };
                Task.Factory.StartNew(async () => {
                    var r = await UApplication.G<GMainGate>().GateFunction.CreateHeroAsync(request);
                    Invoke(() => {
                        if (r.Code.IsOk())
                        {
                            UApplication.G<GMainGate>().ShowMain();
                            HideWindow();
                        }
                        else
                            UApplication.S.ShowError(r.Code);
                    });
                });
            });
        }

        protected override void OnShow()
        {
            base.OnShow();

            var heros = ExcelToJSONConfigManager.Find<CharacterPlayerData>();
            
            ListTableManager.Count = heros.Length;
            int index = 0;

            SetHeroId(heros[0],
                ExcelToJSONConfigManager.GetId<CharacterData>(heros[0].CharacterID));
            foreach (var i in heros)
            {
                ListTableManager[index].Model.SetData(heros[index]);
                ListTableManager[index].Model.OnClick = ClickItem;
                index++;
            }
        }

        private int selectedID = 0;

        private void ClickItem(ListTableModel obj)
        {
            SetHeroId(obj.Config,obj.ChaData);
           
        }

        private void SetHeroId(CharacterPlayerData hero, CharacterData  character)
        {
            selectedID = character.ID;

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