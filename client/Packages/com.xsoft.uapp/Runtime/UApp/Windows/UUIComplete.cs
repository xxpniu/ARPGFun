using Proto;
using UApp;
using UGameTools;

namespace Windows
{
    internal partial class UUIComplete
    {
        public void ShowWindowByResult(G2C_LocalBattleFinished reward)
        {
            ShowWindow();
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.OnMouseClick(_ => { HideWindow(); });
        }

        protected override void OnShow()
        {
            base.OnShow();
        }

        protected override void OnHide()
        {
            base.OnHide();
            UApplication.S.GoBackToMainGate();
        }

        public class ItemContentTableModel : TableItemModel<ItemContentTableTemplate>
        {
            public override void InitModel()
            {
                //todo
            }
        }
    }
}