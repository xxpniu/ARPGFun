﻿using System;
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
    }
}