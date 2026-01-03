using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
public abstract class UUIElement
{
    protected RectTransform _rect;
    private readonly CancellationTokenSource _tokenSource = new();
    protected GameObject uiRoot;
    public CancellationToken CancellationToken => _tokenSource.Token;

    public RectTransform Rect
    {
        get
        {
            if (_rect) return _rect;

            _rect = uiRoot.GetComponent<RectTransform>();
            return _rect;
        }
    }

    protected abstract void OnDestroy();
    protected abstract void OnCreate();

    private void _Destroy()
    {
        _tokenSource.Cancel();
        OnDestroy();
    }

    public static void Destroy(UUIElement el)
    {
        el._Destroy();
    }

    protected T FindChild<T>(string name) where T : Component
    {
        return uiRoot.transform.FindChild<T>(name);
    }
}


public enum WRenderType
{
    Base,
    Notify,
    WithCanvas
}

[Name("UIManager")]
// ReSharper disable once CheckNamespace
// ReSharper disable once InconsistentNaming
public class UUIManager : XSingleton<UUIManager>
{
    public float Ratio = 1;

    public Image BackImage;
    public GameObject top;
    public GameObject worldTop;
    public Canvas NotifyCanvas;
    public Canvas BaseCanvas;

    public GameObject eventMask;

    private readonly Queue<UUIWindow> _addTemp = new();
    private readonly Queue<UUIWindow> _delTemp = new();
    private readonly Queue<UUITip> _tipDelTemp = new();
    private readonly Dictionary<int, UUITip> _tips = new();

    private readonly Dictionary<string, UUIWindow> _window = new();

    private int _index;

    /// <summary>
    ///     当前mask
    /// </summary>
    private float _maskTime;

    protected override void Awake()
    {
        base.Awake();
        if (eventMask != null)
            eventMask.SetActive(false);

        Ratio = Screen.width / (float)Screen.height;
        Debug.Log($"W:{Screen.width} H:{Screen.height}");

        var w = Mathf.Lerp(0, 1, Mathf.Max(0, (Ratio - 1.5f) / .5f));

        var bc = BaseCanvas.GetComponent<CanvasScaler>();
        var nc = NotifyCanvas.GetComponent<CanvasScaler>();
        bc.matchWidthOrHeight = w;
        nc.matchWidthOrHeight = w;
    }

    protected void Update()
    {
        //base.Update();
        while (_addTemp.Count > 0)
        {
            var t = _addTemp.Dequeue();
            _window.Add(t.GetType().Name, t);
        }

        foreach (var i in _window)
        {
            UUIWindow.UpdateUI(i.Value);
            if (i.Value.CanDestroy) _delTemp.Enqueue(i.Value);
        }

        while (_delTemp.Count > 0)
        {
            var t = _delTemp.Dequeue();
            if (_window.Remove(t.GetType().Name))
                UUIElement.Destroy(t);
        }
    }

    private void LateUpdate()
    {
        foreach (var i in _tips)
        {
            if (i.Value == null) continue;
            if (i.Value.CanDestory)
                _tipDelTemp.Enqueue(i.Value);
            else
                i.Value.LateUpdate();
        }

        while (_tipDelTemp.Count > 0)
        {
            var tip = _tipDelTemp.Dequeue();
            _tips.Remove(tip.InstanceID);
            UUIElement.Destroy(tip);
        }

        if (_maskTime > 0 && _maskTime < Time.time)
        {
            _maskTime = -1;
            eventMask.SetActive(false);
        }
    }

    public void UpdateUIData()
    {
        foreach (var i in _window) UUIWindow.UpdateUIData(i.Value);
    }

    public void UpdateUIData<T>() where T : UUIWindow, new()
    {
        var ui = GetUIWindow<T>();
        if (ui != null)
            UUIWindow.UpdateUIData(ui);
    }

    public T GetUIWindow<T>() where T : UUIWindow, new()
    {
        if (_window.TryGetValue(typeof(T).Name, out var obj)) return obj as T;
        return default;
    }


    public async Task<T> CreateWindowAsync<T>(Action<T> callBack = default,
        WRenderType wRender = WRenderType.Base, CancellationToken token = default) where T : UUIWindow, new()
    {
        return await CreateWindow(callBack, wRender, token);
    }

    private async Task<T> CreateWindow<T>(Action<T> callback, WRenderType wRender, CancellationToken token = default)
        where T : UUIWindow, new()
    {
        var ui = GetUIWindow<T>();
        if (ui != null) return ui;
        var root = BaseCanvas.transform;
        switch (wRender)
        {
            case WRenderType.Notify:
                root = NotifyCanvas.transform;
                break;
            case WRenderType.WithCanvas:
                root = transform;
                break;
            case WRenderType.Base:
            default:
                break;
        }

        ui = await UIResourcesLoader<T>.OpenUIAsync(root.transform, callback, token: token);
        _addTemp.Enqueue(ui);
        await UniTask.Yield();
        return ui;
    }

    public int TryToGetTip<T>(int id, bool world, out T tip, Vector3? offset = null) where T : UUITip, new()
    {
        if (_tips.TryGetValue(id, out var t))
        {
            tip = t as T;
            return id;
        }

        var tIndex = _index++;
        if (_index == int.MaxValue) _index = 0;
        _tips.Add(tIndex, null);
        StartCoroutine(CreateTipAsync<T>(world, tIndex, offset));
        tip = null;
        return tIndex;
    }

    private IEnumerator CreateTipAsync<T>(bool world, int index, Vector3? offset) where T : UUITip, new()
    {
        var root = world ? worldTop.transform : top.transform;
        var async = UUITip.CreateAsync<T>(index, root, world);
        yield return async;
        var tip = async.Tip;

        if (_tips.ContainsKey(index))
        {
            _tips[index] = tip;
            if (offset.HasValue)
                UUITip.Update(tip, offset.Value);
            else
                UUITip.Update(tip);
        }
        else
        {
            UUIElement.Destroy(tip);
        }
    }

    public void ShowMask(bool show)
    {
        if (show)
        {
            BackImage.ActiveSelfObject(true);
            BackImage.transform.FindChild<AutoValueScrollbar>("LoadingBg").ResetValue(1);
        }
        else
        {
            BackImage.ActiveSelfObject(false);
        }
    }

    public void ShowLoading(float p, string text = "Loading")
    {
        BackImage.transform.FindChild<Scrollbar>("Scrollbar").value = p;
        BackImage.transform.FindChild<Text>("LoadingText").text = text;
    }

    public Vector2 OffsetInUI(Vector3 position)
    {
        var pos = Camera.main!.WorldToScreenPoint(position);
        return new Vector2(pos.x, pos.y);
    }

    public void HideAll()
    {
        foreach (var i in _window)
            if (i.Value.IsVisible)
                i.Value.HideWindow();
    }

    public void MaskEvent()
    {
        _maskTime = Time.time + 2f;
        eventMask.SetActive(true);
    }

    public void UnMaskEvent()
    {
        _maskTime = -1;
        eventMask.SetActive(false);
    }
}