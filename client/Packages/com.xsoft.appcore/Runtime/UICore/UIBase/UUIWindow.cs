using System;
using System.Collections;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using App.Core.Core.Components;
using org.apache.zookeeper.data;
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming


public class UIResourcesAttribute:Attribute
{
	public UIResourcesAttribute(string name)
	{
		Name = name;
	}

	public string Name{ private set; get; }
}

public enum WindowState
{
	NONE,
	ONSHOWING,
	SHOW,
	ONHIDING,
	HIDDEN
}

public abstract class UUIWindow:UUIElement
{
	
    private ComponentAsync runner;

	protected UUIWindow ()
	{
        CanDestroyWhenHidden = true;
	}
	

	protected  override void OnDestroy()
	{
        UnityEngine.Object.Destroy(uiRoot);
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

    protected virtual void OnLanguage() { }

	public void ShowWindow()
	{
		this._state = WindowState.ONSHOWING;
	}

	public void HideWindow()
	{
        this._state = WindowState.ONHIDING;
	}

	private void Update()
    {
        switch (_state)
        {
            case WindowState.NONE:
                break;
            case WindowState.ONSHOWING:
                this.uiRoot.SetActive(true);
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
                this.uiRoot.SetActive(false);
                break;
            case WindowState.HIDDEN:
                
                break;
        }
    }

	protected bool CanDestroyWhenHidden { set; get; }

	public bool IsVisible => _state == WindowState.SHOW;

	public bool CanDestroy => _state == WindowState.HIDDEN &&CanDestroyWhenHidden;

	public static void UpdateUI(UUIWindow w)
	{
		w.Update ();
	}

    public static void UpdateUIData(UUIWindow w)
    {
        if (w._state == WindowState.SHOW) w.OnUpdateUIData();
    }

	private WindowState _state =  WindowState.NONE;
	
	
    protected CancellationToken DestroyCancellationToken() =>runner.destroyCancellationToken;
    
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