using System;
using UGameTools;
using Proto;
using EConfig;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using UApp;
using UApp.GameGates;
using UnityEngine;

namespace Windows
{
    partial class UUILevelList
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                this.Template.ButtonGreen.onClick.AddListener(() =>
                {
                    Onclick?.Invoke(this);
                });
            }

            public Action<ContentTableModel> Onclick;
            public BattleLevelData Data{ set; get; }

            public async void SetLevel(BattleLevelData level)
            {
                Template.ButtonBrown.ActiveSelfObject(false);
                Data = level;
                this.Template.Name.text = $"{level.Name} Lvl:{level.LimitLevel}";
                this.Template.Desc.text = $"{level.Name}";
                Template.missionImage.sprite = await  ResourcesManager.S.LoadIcon(level);
                this.Template.ButtonGreen.SetKey("UUILevelList_Enter");
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            Bt_Return.onClick.AddListener(this.HideWindow);
        }
        protected override void OnShow()
        {
            base.OnShow();
            lb_title.SetKey("UUILevelList_Title");

            var levels = ExcelConfig.ExcelToJSONConfigManager.Find<BattleLevelData>();
            ContentTableManager.Count = levels.Length;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetLevel(levels[index]);
                i.Model.Onclick = OnItemClick;
                
                index++;
            }
        }

        private void OnItemClick(ContentTableModel item)
        {
            var gate = UApplication.G<GMainGate>();
            var runType = (Proto.LevelRunType)item.Data.RunType;
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
                default:
                    //donothing
                    break;
            }
        }

        private async void GoToServer(int leveID)
        {
            var gate = UApplication.G<GMainGate>();
            var re = await GateManager.S.GateFunction.CreateMatchAsync(new C2G_CreateMatch { LevelID = leveID });
            if (!re.Code.IsOk()) Invoke(() => UApplication.S.ShowError(re.Code));
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}