using System;
using UGameTools;
using Proto;
using UnityEngine;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using UApp;
using UApp.GameGates;

namespace Windows
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    // ReSharper disable once InconsistentNaming
    public class GMCommandAttribute : Attribute
    {
        public string GMkey;
        public string name;
        public string[] parmas;
        public string[] DefaultParamas;

        public GMCommandAttribute(string key, string name, params string[] gmparams)
        {
            this.GMkey = key;
            this.name = name;
            this.parmas = gmparams;
        }
    }

    [GMCommand("addcoin", "添加金币", "数量", DefaultParamas = new string[] { "100000" })]
    [GMCommand("addcoin", "添加钻石", "数量", DefaultParamas = new string[] { "100000" })]
    [GMCommand("make", "获得道具", "道具ID", "数量")]
    [GMCommand("addexp", "添加exp", "数量", DefaultParamas = new string[] { "100000" })]
    [GMCommand("level", "设置角色等级", "等级", DefaultParamas = new string[] { "1" })]

    partial class UUIGMPanel
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel()
            {
            }

            public override void InitModel()
            {
                Template.Button.onClick.AddListener(() => { OnClick?.Invoke(this); });
            }

            public Action<ContentTableModel> OnClick;

            public GMCommandAttribute Command;

            internal void SetCommand(GMCommandAttribute command)
            {
                Command = command;
                this.Template.Button.SetText(command.name);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            //Write Code here
            this.bt_close.onClick.AddListener(HideWindow);
            this.Bt_SendGM.onClick.AddListener(SendGmCommand);
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
            int index = 0;
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

        private GMCommandAttribute[] AllCommand
        {
            get
            {
                var att =
                    typeof(UUIGMPanel).GetCustomAttributes(typeof(GMCommandAttribute), false) as GMCommandAttribute[];
                return att;
            }
        }
        
    }
}