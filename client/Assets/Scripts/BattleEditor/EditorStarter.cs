using BattleViews;
using BattleViews.Components;
using BattleViews.Utility;
using BattleViews.Views;
using Core;
using UnityEngine; 
using Cysharp.Threading.Tasks;
using ExcelConfig;
using UnityEngine.SceneManagement;
using EConfig;
using GameLogic;
using EngineCore.Simulater;
using GameLogic.Game.Perceptions;
using UVector3 = UnityEngine.Vector3;
using GameLogic.Game.AIBehaviorTree;
using Google.Protobuf;
using GameLogic.Game.Elements;
using Layout;
using GameLogic.Game.States;
using XNet.Libs.Utility;
using Layout.AITree;
using GameLogic.Game.LayoutLogics;
using UGameTools;

//#if UNITY_EDITOR

public class EditorStarter : XSingleton<EditorStarter> , IAIRunner, IStateLoader
{
	

	public const string EDITOR_LEVEL_NAME = "EditorReleaseMagic";

	#region implemented abstract members of UGate

	private UPerceptionView PerView { set; get; }

	private GTime Now
	{
		get
		{
			var sim = PerView as ITimeSimulater;
			return sim.Now;
		}
	}

	private  async  void Start()
	{
		AIRunner.Current = this;
		Debuger.Loger = new UnityLoger();

		var excelToJsonConfigManager = new ExcelToJSONConfigManager(ResourcesManager.S); 
		LanguageManager.S.AddLanguage(ExcelToJSONConfigManager.Find<LanguageData>());
		isStarted = false;
		
		await SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Additive);
		tcamera = FindObjectOfType<ThirdPersonCameraContollor>();
		isStarted = true;
		PerView = UPerceptionView.Create(ExcelToJSONConfigManager.GetId<ConstantValue>(1));
		PerView.OnCreateCharacter = (c) => { c.TryAdd<HpTipShower>(); };
		_curState = new BattleState(PerView, this, PerView);
		_curState.Init();
		_curState.Start(Now);
		PerView.UseCache = false;
		await UUIManager.S.CreateWindowAsync<Windows.UUIBattleEditor>(ui => ui.ShowWindow());

	}

	private GState _curState;

    private void OnDestroy()
    {
		_curState.Stop(Now);
	}

    private void Tick()
	{
		if (_curState != null)
		{
			GState.Tick(_curState, Now);
		}
	}

	#endregion

	public MagicReleaser currentReleaser;

	public BattleCharacter releaser;
	public BattleCharacter target;

	public bool EnableTap = false;

	public void ReleaseMagic(MagicData magic)
	{
		Resources.UnloadUnusedAssets();

		if (!target.Enable || !releaser.Enable)
		{
			Debug.LogError("No found target !");
			return;
        }
		var per = _curState.Perception as BattlePerception;
		this.currentReleaser =per.CreateReleaser(string.Empty,this.releaser, magic,
            new ReleaseAtTarget(this.releaser, this.target),
			ReleaserType.Magic, Proto.ReleaserModeType.RmtMagic,0);

	}

	public void ReplaceRelease(int level, CharacterData data, bool stay, bool ai)
	{

		if (!stay && this.releaser)
			this.releaser.SubHP(this.releaser.HP, out _);
		var per = _curState.Perception as BattlePerception;
		var scene = PerView.UScene;
		var magics = data.CreateHeroMagic(); // per.CreateHeroMagic(data.ID);
		var levelData =
			ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.CharacterID == data.ID && t.Level == level);
		var properties = data.CreatePlayerProperties(levelData);

		if (levelData != null)
			properties.TryAddBase(levelData.Properties, levelData.PropertyValues);

		releaser = per.CreateCharacter(per.StateControllor, level, data, magics, properties, 1,
			scene.startPoint.position + (UVector3.right * distanceCharacter / 2)
			, new UVector3(0, 90, 0), string.Empty, data.Name);
	
		tcamera.SetLookAt(releaser.Transform);
	}

	public void ReplaceTarget(int level,CharacterData data, bool stay, bool ai)
	{
		
		if (!stay&&this.target)
            this.target.SubHP(this.target.HP,out _);
		var per = _curState.Perception as BattlePerception;
		var scene = PerView.UScene;
		var magics = data.CreateHeroMagic();// per.CreateHeroMagic(data.ID);
		var levelData = ExcelToJSONConfigManager
            .First<CharacterLevelUpData>(t => t.CharacterID == data.ID && t.Level == level);
		var properties = data.CreatePlayerProperties(levelData);

		if (levelData != null)
			properties.TryAddBase(levelData.Properties, levelData.PropertyValues);


		var target = per.CreateCharacter(per.AIControllor, level, data, magics,properties, 2, scene.enemyStartPoint.position + (UVector3.left * distanceCharacter / 2),
			new UVector3(0, -90, 0), string.Empty, data.Name);
		//target.ResetHPMP();
		if (ai) per.ChangeCharacterAI(data.AIResourcePath, target);
		this.target = target;
	}

    


	public void DoAction(IMessage action)
	{
		if (this.releaser == null) return;
		if (this.releaser.AiRoot == null) return;
		this.releaser?.AddNetAction(action);
	}

    private bool isStarted = false;

	private ThirdPersonCameraContollor tcamera;

	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
		if (!isStarted) return;
		Tick();

		tcamera.SetXY(slider_y,ry);
		tcamera.distance = distance;
		if (isChanged)
		{
			var position = PerView.UScene.startPoint.position;
			var left = position + (UVector3.left * distanceCharacter / 2);
			var right = position + (UVector3.right * distanceCharacter / 2);
			releaser.Position = left;
			target.Position = right;
			isChanged = false;
		}
	}

	public bool isChanged = false;

	public float slider_y = 1f;
	public float distance = 5;
	public float ry =0;
	public float distanceCharacter = 8;


	AITreeRoot IAIRunner.RunAI(TreeNode ai)
	{
		if (_curState.Perception is BattlePerception p)
		{
			var root = p.ChangeCharacterAI(ai, releaser);
			root.IsDebug = true;
			return root;
		}
		return null;
	}

    bool IAIRunner.IsRunning(Layout.EventType eventType)
    {
		return currentReleaser?.IsRuning(eventType) == true;
    }

	bool IAIRunner.ReleaseMagic(MagicData data)
	{
		ReleaseMagic(data);
		return true;
	}

    void IAIRunner.Attach(BattleCharacter character)
    {
		releaser = character;
		if (character.AiRoot != null) character.AiRoot.IsDebug = true;
    }

    public void Load(GState state)
    {
		var configs = ExcelToJSONConfigManager.Find<CharacterData>();
		var releaserData = configs[0];
		var targetData = configs[0];
		_curState = state;

		ReplaceRelease(1, releaserData, false, true);
		ReplaceTarget(1, targetData, false, true);
    }
}
//#endif
