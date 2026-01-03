namespace Windows
{
    internal partial class UUIBattleResult
    {
        protected override void InitModel()
        {
            base.InitModel();
            Bt_Ok.onClick.AddListener(() =>
            {
                //UApplication.Singleton.GoToMainGate();
            });

            Bt_Again.onClick.AddListener(() => { });
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

        public void ShowResult(bool isWin)
        {
        }
    }
}