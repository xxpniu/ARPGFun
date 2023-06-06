using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIGMDetail
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string v,string def)
            {
                this.Template.lb_text.text = v;
                this.Template.InputField.text = def ?? string.Empty;
            }
        }

        protected override void InitModel()
        {
            base.InitModel();

            bt_close.onClick.AddListener(() => { HideWindow(); });
            bt_send.onClick.AddListener(() =>
            {
                var sb = new StringBuilder();
                sb.Append(cmd.GMkey);
                foreach (var i in ContentTableManager)
                {
                    sb.Append($" {i.Template.InputField.text}");
                }

                UUIGMPanel.SendCommand(sb.ToString());
                this.HideWindow();

            });
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            this.ContentTableManager.Count = cmd.parmas.Length;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetLabel(cmd.parmas[index],cmd.DefaultParamas?[index]);
                index++;
            }
        }
        protected override void OnHide()
        {
            base.OnHide();
        }

        private GMCommandAttribute cmd;

        internal void ShowWindow(GMCommandAttribute command)
        {
            this.cmd = command;

            this.ShowWindow();
        }
    }
}