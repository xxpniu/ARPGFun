using System.Text;
using UApp;

namespace Windows
{
    internal partial class UUIGMDetail
    {
        private GMCommandAttribute cmd;

        protected override void InitModel()
        {
            base.InitModel();

            bt_close.onClick.AddListener(HideWindow);
            bt_send.onClick.AddListener(() =>
            {
                var sb = new StringBuilder();
                sb.Append(cmd.GMkey);
                foreach (var i in ContentTableManager) sb.Append($" {i.Template.InputField.text}");

                GateManager.S.SendCommand(sb.ToString());
                HideWindow();
            });
            //Write Code here
        }

        protected override void OnShow()
        {
            base.OnShow();
            ContentTableManager.Count = cmd.parmas.Length;
            var index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetLabel(cmd.parmas[index], cmd.DefaultParamas?[index]);
                index++;
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        internal void ShowWindow(GMCommandAttribute command)
        {
            cmd = command;

            ShowWindow();
        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string v, string def)
            {
                Template.lb_text.text = v;
                Template.InputField.text = def ?? string.Empty;
            }
        }
    }
}