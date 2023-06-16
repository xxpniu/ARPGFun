using System;
using App.Core.Core;
using UnityEngine;


public class UITipResourcesAttribute:Attribute
{
	public UITipResourcesAttribute(string name)
	{
		this.Name = name;
	}

	public string Name{ private set;get;}
}

public abstract class UUITip:UUIElement
{
	
    public class CreateUIAsync<T> : CustomYieldInstruction where T:UUITip,new()
    {
        public CreateUIAsync(int index,Transform parent, bool world)
        {
            var attrs = typeof(T).GetCustomAttributes(typeof(UITipResourcesAttribute), false) as UITipResourcesAttribute[];
            if ( attrs ==null || attrs.Length == 0) throw new Exception($"no found UITipResourcesAttribute");
            Load(attrs[0].Name,index,parent,world);
        }

        private async void Load(string resources,int index,Transform parent, bool world)
        {
	        var res = await ResourcesManager.S.LoadResourcesWithExName<GameObject>($"Tips/{resources}.prefab",null);
	        var root = UnityEngine.Object.Instantiate(res);
	        var tip = new T
	        {
		        IsWorld = world,
		        InstanceID = index
	        };
	        root.name = $"_TIP_{index}_{typeof(T).Name}";
	        tip.uiRoot = root;
	        tip.Rect.SetParent(parent, false);
	        tip.OnCreate();
	        Tip = tip;
	        IsDone = true;
        }

        public T Tip { private set; get; }

        public bool IsDone { private set; get; }
        public override bool keepWaiting => !IsDone;
    }


	private bool LastUpdate = false;

    public int InstanceID { get; protected set; } = 0;

    protected override void OnDestroy ()
	{
		GameObject.Destroy(this.uiRoot, 0.1f);
		this._rect = null;
	}

    public bool IsWorld { private set; get; } = false;

	public void LateUpdate()
	{
		LastUpdate = false;
	}

    public bool CanDestory{ get{ return !LastUpdate;}}
    public void LookAt(Camera c)
    {
        uiRoot.transform.LookAt(c.transform);
    }

    public static CreateUIAsync<T> CreateAsync<T>(int index,Transform parent, bool world) where T: UUITip,new()
    {
        return new CreateUIAsync<T>(index,parent, world);
    }

	public static void Update(UUITip tip,Vector2 pos)
	{
        tip.Rect.position = new Vector3(pos.x, pos.y, 0);
        Update(tip);
	}

    public static void Update(UUITip tip, Vector3 pos)
    {
        tip.uiRoot.transform.position = pos;
        Update(tip);
    }

    public static void Update(UUITip tip)
    {
        tip.LastUpdate = true;
        tip.OnUpdate();
    }

    protected virtual void OnUpdate()
    {
        
    }



}