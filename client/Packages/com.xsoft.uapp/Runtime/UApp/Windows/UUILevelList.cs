using System;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using Proto;
using UApp;
using UnityEngine;

namespace Windows
{
    internal partial class UUILevelList
    {
        protected override void InitModel()
        {
            base.InitModel();
            Bt_Return.onClick.AddListener(HideWindow);
        }

        protected override void OnShow()
        {
            base.OnShow();
            lb_title.SetKey("UUILevelList_Title");

            var levels = ExcelToJSONConfigManager.Find<BattleLevelData>();
            ContentTableManager.Count = levels.Length;
            var index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetLevel(levels[index]);
                i.Model.Onclick = OnItemClick;

                index++;
            }
        }

        private void OnItemClick(ContentTableModel item)
        {
            var gate = GateManager.Try();
            var runType = (LevelRunType)item.Data.RunType;
            switch (runType)
            {
                case LevelRunType.LrtLocal:
                    UApplication.S.StartLocalLevel(gate.Hero, gate.Package, item.Data.ID);
                    break;
                case LevelRunType.LrtTeam:
                    GoToServer(item.Data.ID);
                    break;
                case LevelRunType.LrtServer:
                    Debug.LogError($"not supported:{runType}");
                    break;
            }
        }

        private async void GoToServer(int leveID)
        {
            var re = await GateManager.S.MatchServiceClient.CreateMatchAsync(new C2G_CreateMatch { LevelID = leveID });
            await UniTask.SwitchToMainThread();
            if (!re.Code.IsOk()) UApplication.S.ShowError(re.Code);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public Action<ContentTableModel> Onclick;
            public BattleLevelData Data { set; get; }

            public override void InitModel()
            {
                Template.ButtonGreen.onClick.AddListener(() => { Onclick?.Invoke(this); });
            }

            public async void SetLevel(BattleLevelData level)
            {
                Template.ButtonBrown.ActiveSelfObject(false);
                Data = level;
                Template.Name.text = $"{level.Name} Lvl:{level.LimitLevel}";
                Template.Desc.text = $"{level.Name}";
                Template.missionImage.sprite = await ResourcesManager.S.LoadIcon(level);
                Template.ButtonGreen.SetKey("UUILevelList_Enter");
            }
        }
    }
}