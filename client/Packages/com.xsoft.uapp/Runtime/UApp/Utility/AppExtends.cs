using System;
using System.Threading.Tasks;
using App.Core.UICore.Utility;
using UnityEngine;

namespace UGameTools
{
    public static class AppExtends
    {
        public static UIMouseClick OnMouseClick(this Component c, Action<object> click, object userState = null)
        {
            var t = c.TryAdd<UIMouseClick>();
            t.userState = userState;
            t.OnClick = click;
            return t;
        }

        public static async Task<T> CreateChildWindow<T>(this UUIWindow win,　WRenderType renderType = WRenderType.Base) where T : UUIWindow, new()
        {
           // var parent = win;
            var ui = await UUIManager.S.CreateWindowAsync<T>( wRender : renderType);
            //ui = ui;
            return ui;
        }

        public static Vector3 ZeroY(this Vector3 vec)
        {
            return new Vector3(vec.x, 0, vec.z);
        }
    }
}
