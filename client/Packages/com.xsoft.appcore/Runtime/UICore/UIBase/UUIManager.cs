using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;

public abstract class UUIElement
{
    private CancellationTokenSource _tokenSource = new CancellationTokenSource();
    public CancellationToken CancellationToken => _tokenSource.Token;
	protected GameObject uiRoot;
	protected abstract void OnDestroy ();
	protected abstract void OnCreate ();
    protected RectTransform _rect;

    private void _Destroy()
    {
        _tokenSource.Cancel();
        OnDestroy();
    }

    public RectTransform Rect
    {
        get{ 
            if (_rect)
                return _rect;
            else
            {
                _rect = this.uiRoot.GetComponent<RectTransform>();
                return _rect;
            }
        } 
    }

	public static void Destroy(UUIElement el)
    {
		el._Destroy();
	}
    protected T FindChild<T>(string name) where T: Component
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


public class UUIManager:XSingleton<UUIManager>
{
    protected override void Awake()
    {
        base.Awake();
        if (eventMask!=null)
            eventMask.SetActive(false);

        Ratio = Screen.width / (float)Screen.height;

        var w = Mathf.Lerp(0, 1, Mathf.Max(0, (Ratio - 1.5f)/.5f));

        var bc = this.BaseCanvas.GetComponent<CanvasScaler>();
        var nc = this.NotifyCanvas.GetComponent<CanvasScaler>();
        bc.matchWidthOrHeight = w;
        nc.matchWidthOrHeight = w;



    }

    public float Ratio = 1;

	private readonly Dictionary<string,UUIWindow> _window=new Dictionary<string, UUIWindow> ();
    private readonly Dictionary<int,UUITip> _tips= new Dictionary<int, UUITip> ();

    protected  void Update()
    {
        //base.Update();
        while (_addTemp.Count > 0) {
			var t = _addTemp.Dequeue ();
			_window.Add (t.GetType ().Name, t);
		}

		foreach (var i in _window) {
			UUIWindow.UpdateUI( i.Value);
			if (i.Value.CanDestroy) {
				_delTemp.Enqueue (i.Value);
			}
		}

		while (_delTemp.Count > 0) {
			var t = _delTemp.Dequeue ();
			if (_window.Remove (t.GetType ().Name))
				UUIElement.Destroy (t);
		}
            
	}

    public void UpdateUIData()
    {
        foreach (var i in _window)
        {
            UUIWindow.UpdateUIData(i.Value);
        }
    }

    public void UpdateUIData<T>()  where T: UUIWindow, new()
    {
        var ui=  GetUIWindow<T>();
        if (ui != null)
            UUIWindow.UpdateUIData(ui);
    }
    private readonly Queue<UUITip> _tipDelTemp = new();

    void LateUpdate()
    {
        foreach (var i in _tips)
        {
            if (i.Value == null) continue;
            if (i.Value.CanDestory)
            {
                _tipDelTemp.Enqueue(i.Value);
            }
            else
            {
                i.Value.LateUpdate();
            }
        }

        while (_tipDelTemp.Count > 0)
        {
            var tip = _tipDelTemp.Dequeue();
            _tips.Remove(tip.InstanceID);
            UUIElement.Destroy(tip);
        }

        if (maskTime > 0 && maskTime < Time.time)
        {
            maskTime = -1;
            eventMask.SetActive(false);
        }
    }

	public T GetUIWindow<T>()where T:UUIWindow, new()
	{
        if (_window.TryGetValue(typeof(T).Name, out UUIWindow obj))
        {
            return obj as T;
        }
        return default;
	}

	private readonly Queue<UUIWindow> _addTemp = new();
	private readonly Queue<UUIWindow> _delTemp = new();

  
    public async Task<T> CreateWindowAsync<T>(Action<T> callBack = default, 
        WRenderType wRender = WRenderType.Base) where T : UUIWindow, new()
    {
        return await CreateWindow(callBack,wRender);
    }

    private async Task<T> CreateWindow<T>(Action<T> callback, WRenderType wRender) where T : UUIWindow, new()
    {
        var ui = GetUIWindow<T>();
        if (ui != null) return ui;
        var root = this.BaseCanvas.transform;
        switch (wRender)
        {

            case WRenderType.Notify:
                root = NotifyCanvas.transform;
                break;
            case WRenderType.WithCanvas:
                root = this.transform;
                break;
            case WRenderType.Base:
            default:
                break;
        }

        ui = await UIResourcesLoader<T>.OpenUIAsync(root.transform, callback);
        _addTemp.Enqueue(ui);

        await UniTask.Yield();

        return ui;
    }

    public int TryToGetTip<T>(int id, bool world, out T tip,Vector3? offset =null) where T : UUITip, new()
    {
        if (_tips.TryGetValue(id, out UUITip t))
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

    private int _index = 0;

    private IEnumerator CreateTipAsync<T>(bool world,int index, Vector3? offset) where T : UUITip, new()
    {
        var root = world ? worldTop.transform : this.top.transform;
        var async = UUITip.CreateAsync<T>(index,root, world);
        yield return async;
        var tip = async.Tip;
      
        if (_tips.ContainsKey(index))
        {
            this._tips[index] = tip;
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

	public void ShowLoading(float p,string text = "Loading")
	{
		BackImage.transform.FindChild<Scrollbar> ("Scrollbar").value =  p;
        BackImage.transform.FindChild<Text>("LoadingText").text = text;
	}

    public Image BackImage;
	public GameObject top;
    public GameObject worldTop;
    public Canvas NotifyCanvas;
    public Canvas BaseCanvas;

    public Vector2 OffsetInUI(Vector3 position)
    {
        var pos = Camera.main.WorldToScreenPoint(position) ;
        return new Vector2(pos.x, pos.y);
    }

    public void HideAll()
    {
        foreach (var i in _window)
        {
            if (i.Value.IsVisible)
                i.Value.HideWindow();
        }
    }

    public GameObject eventMask;

    /// <summary>
    /// 当前mask
    /// </summary>
    private float maskTime = 0;

    public void MaskEvent()
    {
        maskTime = Time.time+ 2f;
        eventMask.SetActive(true);
    }

    public void UnMaskEvent()
    {
        maskTime = -1;
        eventMask.SetActive(false);
    }

}