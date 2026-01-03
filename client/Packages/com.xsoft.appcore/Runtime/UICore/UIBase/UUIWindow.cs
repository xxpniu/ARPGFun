using System;
using System.Threading;
using App.Core.Core.Components;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming


public class UIResourcesAttribute : Attribute
{
    public UIResourcesAttribute(string name)
    {
        Name = name;
    }

    public string Name { private set; get; }
}

public enum WindowState
{
    NONE,
    ONSHOWING,
    SHOW,
    ONHIDING,
    HIDDEN
}

public abstract class UUIWindow : UUIElement
{
    private WindowState _state = WindowState.NONE;

    private ComponentAsync runner;

    protected UUIWindow()
    {
        CanDestroyWhenHidden = true;
    }

    protected bool CanDestroyWhenHidden { set; get; }

    public bool IsVisible => _state == WindowState.SHOW;

    public bool CanDestroy => _state == WindowState.HIDDEN && CanDestroyWhenHidden;


    protected override void OnDestroy()
    {
        Object.Destroy(uiRoot);
    }

    protected virtual void OnUpdateUIData()
    {
    }

    protected virtual void OnShow()
    {
    }

    protected virtual void OnHide()
    {
    }

    protected virtual void OnUpdate()
    {
    }

    protected virtual void OnBeforeShow()
    {
    }

    protected virtual void OnLanguage()
    {
    }

    public void ShowWindow()
    {
        _state = WindowState.ONSHOWING;
    }

    public void HideWindow()
    {
        _state = WindowState.ONHIDING;
    }

    private void Update()
    {
        switch (_state)
        {
            case WindowState.NONE:
                break;
            case WindowState.ONSHOWING:
                uiRoot.SetActive(true);
                OnBeforeShow();
                _state = WindowState.SHOW;
                OnShow();
                break;
            case WindowState.SHOW:
                OnUpdate();
                break;
            case WindowState.ONHIDING:
                _state = WindowState.HIDDEN;
                OnHide();
                uiRoot.SetActive(false);
                break;
            case WindowState.HIDDEN:

                break;
        }
    }

    public static void UpdateUI(UUIWindow w)
    {
        w.Update();
    }

    public static void UpdateUIData(UUIWindow w)
    {
        if (w._state == WindowState.SHOW) w.OnUpdateUIData();
    }


    protected CancellationToken DestroyCancellationToken()
    {
        return runner.destroyCancellationToken;
    }

    public static void TryToInitWindow(UUIWindow window, GameObject root, Transform parent)
    {
        window.uiRoot = root;
        window.Rect.SetParent(parent, false);
        window.uiRoot.name = $"UI_{window.GetType().Name}";
        window.runner = root.AddComponent<ComponentAsync>();
        window.OnCreate();
        //this.Window = window;
        window.uiRoot.SetActive(false);
    }
}