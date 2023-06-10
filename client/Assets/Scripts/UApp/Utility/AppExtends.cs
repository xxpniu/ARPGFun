using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UGameTools
{
    public static class AppExtends
    {
        public static UIMouseClick OnMouseClick(this Component c, Action<object> click, object userstate = null)
        {
            var t = c.TryAdd<UIMouseClick>();
            t.userState = userstate;
            t.OnClick = click;
            return t;
        }

        public static async Task<T> CreateWindow<T>(this UUIWindow ui, WRenderType renderType = WRenderType.Base) where T : UUIWindow, new()
        {
            return await UUIManager.S.CreateWindowAsync<T>( wRender : renderType);
        }
    }
}
