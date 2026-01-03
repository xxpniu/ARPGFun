using System.Collections.Generic;
using App.Core.Core;
using Tips;
using UnityEngine;

public struct AppNotify
{
    public string Message;
    public float endTime;
}

public class UUITipDrawer : XSingleton<UUITipDrawer>
{
    protected void Update()
    {
        //base.Update();
        foreach (var i in notifys)
        {
            i.ID = DrawUUINotify(i.ID, i.message);
            if (i.time < Time.time) _dels.Enqueue(i);
        }

        while (_dels.Count > 0) notifys.Remove(_dels.Dequeue());
    }


    public int DrawUUITipNameBar(int instanceId, string name,
        int level, int hp, int hpMax, bool owner, Vector3 offset, Camera c)
    {
        instanceId = UUIManager.S.TryToGetTip(instanceId, true, out UUITipNameBar tip, offset);
        if (tip != null)
        {
            tip.SetInfo(name, level, hp, hpMax, owner);
            tip.LookAt(c);
            UUITip.Update(tip, offset);
            return tip.InstanceID;
        }

        return instanceId;
    }

    public int DrawItemName(int instanceId, string name, bool owner, Vector3 offset, Camera c)
    {
        instanceId = UUIManager.S.TryToGetTip(instanceId, true, out UUIName tip, offset);
        if (tip != null)
        {
            tip.ShowName(name, owner);
            tip.LookAt(c);
            UUITip.Update(tip, offset);
            return tip.InstanceID;
        }

        return instanceId;
    }

    private class NotifyMessage
    {
        public int ID = -1;
        public string message;
        public float time;

        public static implicit operator NotifyMessage(AppNotify notify)
        {
            return new NotifyMessage { message = notify.Message, time = notify.endTime };
        }
    }

    #region Notify

    private int DrawUUINotify(int instanceId, string notify)
    {
        instanceId = UUIManager.S.TryToGetTip(instanceId, false, out UUINotify tip);
        if (tip != null)
        {
            tip.SetNotify(notify);
            UUITip.Update(tip);
            return tip.InstanceID;
        }

        return instanceId;
    }

    private readonly List<NotifyMessage> notifys = new();
    private readonly Queue<NotifyMessage> _dels = new();

    public void ShowNotify(AppNotify notify)
    {
        notifys.Add(notify);
    }

    public void ShowNotify(string notify, float dur = 4.5f)
    {
        notifys.Add(new NotifyMessage { message = notify, time = Time.time + dur });
        Debug.Log(notify);
    }

    #endregion
}