using System;
using UGameTools;
using Proto;
using EConfig;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using UApp;
using UApp.GameGates;

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
            Bt_Return.onClick.AddListener(() =>
            {
                this.HideWindow();
            });
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
#if DEVELOPMENT_BUILD ||UNITY_EDITOR
            UUIPopup.ShowConfirm("GoToServer", "Cancel to local",
                () => GoToServer(item.Data.ID),
                () =>
                {
                    UApplication.S.StartLocalLevel(gate.hero, gate.package, item.Data.ID);
                });
#else
            GoToServer(item.Data.ID);
#endif
        }

        private void GoToServer(int leveID)
        {
            var gate = UApplication.G<GMainGate>();
            Task.Factory.StartNew(async()=>
            {
                var re = await gate.GateFunction.CreateMatchAsync(new C2G_CreateMatch { LevelID =leveID });
                if (!re.Code.IsOk()) Invoke(() => UApplication.S.ShowError(re.Code));
            });      
        }
           
        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}