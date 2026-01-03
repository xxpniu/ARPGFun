using System;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Views;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Proto;
using UApp;
using UApp.GameGates;

namespace Windows
{
    internal partial class UUIHeroCreate
    {
        private int _selectedID;

        protected override void InitModel()
        {
            base.InitModel();

            Bt_create.onClick.AddListener(CreateHeroCall);
            return;

            async void CreateHeroCall()
            {
                if (string.IsNullOrEmpty(InputField.text) || InputField.text.Length < 2)
                {
                    UApplication.S.ShowNotify("UI_HERONAME_NEED".GetLanguageWord());
                    return;
                }

                var request = new C2G_CreateHero { HeroID = _selectedID, HeroName = InputField.text };
                var r = await GateManager.S.GateFunction.CreateHeroAsync(request);
                await UniTask.SwitchToMainThread();
                if (r.Code.IsOk())
                {
                    UApplication.G<GMainGate>().ShowMain();
                    HideWindow();
                }
                else
                {
                    UApplication.S.ShowError(r.Code);
                }
            }
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

        private void ClickItem(ListTableModel obj)
        {
            SetHeroId(obj.Config, obj.ChaData);
        }

        private void SetHeroId(CharacterPlayerData hero, CharacterData character)
        {
            _selectedID = character.ID;

            var v = UApplication.G<GMainGate>().CreateOwner(character.ID, character.Name);
            lb_description.SetKey(hero.Description);

            RunMotion(v, hero.Motion);
        }

        private async void RunMotion(UCharacterView view, string motion)
        {
            await UniTask.Delay(250);

            if (!view) return;
            if (CancellationToken.IsCancellationRequested) return;
            view.PlayMotion(motion);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public class ListTableModel : TableItemModel<ListTableTemplate>
        {
            public CharacterData ChaData;

            public CharacterPlayerData Config;

            public Action<ListTableModel> OnClick;

            public override void InitModel()
            {
                Template.BtHero.onClick.AddListener(() => { OnClick?.Invoke(this); });
            }

            internal void SetData(CharacterPlayerData characterPlayer)
            {
                Config = characterPlayer;
                ChaData = ExcelToJSONConfigManager.GetId<CharacterData>(Config.CharacterID);
                Template.lb_name.SetKey(ChaData.Name);
            }
        }
    }
}