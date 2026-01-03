namespace Windows
{
    internal partial class UUIMessages
    {
        protected override void InitModel()
        {
            base.InitModel();

            ButtonClose.onClick.AddListener(HideWindow);
            //Write Code here
        }

        protected override void OnShow()
        {
            base.OnShow();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public override void InitModel()
            {
                //todo
            }
        }
    }
}