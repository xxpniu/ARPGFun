using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using UnityEngine;

namespace Windows
{
    partial class UUILevelUp
    {

        protected override void InitModel()
        {
            base.InitModel();
            //ButtonClose.onClick.AddListener(() => { HideWindow(); });
            //Root.OnMouseClick((g) => { HideWindow(); });
        }

        private float hideTime = -1;
        protected override void OnShow()
        {
            base.OnShow();
            hideTime = Time.time + 2f;
        }
        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (hideTime < Time.time) HideWindow();
        }
        internal void ShowWindow(int level)
        {
            lb_level.text = $"{level}";
            ShowWindow();
        }
    }
}