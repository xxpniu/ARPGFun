using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proto;
using UApp;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIComplete
    {
        public class ItemContentTableModel : TableItemModel<ItemContentTableTemplate>
        {
            public ItemContentTableModel(){}
            public override void InitModel()
            {
                //todo
            }
        }

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
    }
}