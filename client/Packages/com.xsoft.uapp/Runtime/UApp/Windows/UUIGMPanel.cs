using System;
using App.Core.UICore.Utility;
using UApp;

namespace Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    // ReSharper disable once InconsistentNaming
    public class GMCommandAttribute : Attribute
    {
        public string[] DefaultParamas;
        public string GMkey;
        public string name;
        public string[] parmas;

        public GMCommandAttribute(string key, string name, params string[] gmparams)
        {
            GMkey = key;
            this.name = name;
            parmas = gmparams;
        }
    }

    [GMCommand("addcoin", "添加金币", "数量", DefaultParamas = new[] { "100000" })]
    [GMCommand("addcoin", "添加钻石", "数量", DefaultParamas = new[] { "100000" })]
    [GMCommand("make", "获得道具", "道具ID", "数量")]
    [GMCommand("addexp", "添加exp", "数量", DefaultParamas = new[] { "100000" })]
    [GMCommand("level", "设置角色等级", "等级", DefaultParamas = new[] { "1" })]
    internal partial class UUIGMPanel
    {
        private GMCommandAttribute[] AllCommand
        {
            get
            {
                var att =
                    typeof(UUIGMPanel).GetCustomAttributes(typeof(GMCommandAttribute), false) as GMCommandAttribute[];
                return att;
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            //Write Code here
            bt_close.onClick.AddListener(HideWindow);
            Bt_SendGM.onClick.AddListener(SendGmCommand);
            return;


            void SendGmCommand()
            {
                GateManager.S.SendCommand(IF_GmText.text);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();

            var all = AllCommand;
            var index = 0;
            ContentTableManager.Count = all.Length;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetCommand(all[index]);
                i.Model.OnClick = ClickItem;
                index++;
            }
        }

        private async void ClickItem(ContentTableModel m)
        {
            await UUIManager.S.CreateWindowAsync<UUIGMDetail>(ui => { ui.ShowWindow(m.Command); });
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public GMCommandAttribute Command;

            public Action<ContentTableModel> OnClick;

            public override void InitModel()
            {
                Template.Button.onClick.AddListener(() => { OnClick?.Invoke(this); });
            }

            internal void SetCommand(GMCommandAttribute command)
            {
                Command = command;
                Template.Button.SetText(command.name);
            }
        }
    }
}