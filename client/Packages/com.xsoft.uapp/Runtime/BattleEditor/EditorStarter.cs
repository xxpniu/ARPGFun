using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews;
using BattleViews.Components;
using BattleViews.Utility;
using BattleViews.Views;
using EConfig;
using EngineCore.Simulater;
using ExcelConfig;
using GameLogic;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Google.Protobuf;
using Layout;
using Layout.AITree;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using XNet.Libs.Utility;
using UVector3 = UnityEngine.Vector3;

//#if UNITY_EDITOR

namespace BattleEditor
{
	public class EditorStarter : XSingleton<EditorStarter> , IAIRunner, IStateLoader
	{
		
		public const string EDITOR_LEVEL_NAME = "EditorReleaseMagic";

		#region implemented abstract members of UGate

		private UPerceptionView PerView { set; get; }

		private GTime Now
		{
			get
			{
				var sim = PerView as ITimeSimulator;
				return sim.Now;
			}
		}

		private  async void Start()
		{
			AIRunner.Current = this;
			Debuger.Loger = new UnityLogger();

			var excelToJsonConfigManager = new ExcelToJSONConfigManager(ResourcesManager.S); 
			LanguageManager.S.AddLanguage(ExcelToJSONConfigManager.Find<LanguageData>());
			_isStarted = false;
		
			await SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Additive);
			_tCamera = FindFirstObjectByType<ThirdPersonCameraContollor>();
		
			PerView = UPerceptionView.Create(ExcelToJSONConfigManager.GetId<ConstantValue>(1));
			PerView.OnCreateCharacter = (c) => { c.TryAdd<HpTipShower>(); };
			_curState = new BattleState(PerView, this, PerView);
			_curState.Init();
			_curState.Start(Now);
			PerView.UseCache = false;
			await UUIManager.S.CreateWindowAsync<Windows.UUIBattleEditor>(ui => ui.ShowWindow());

			_isStarted = true;
		}

		private GState _curState;

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_curState.Stop(Now);
		}

		private void Tick()
		{
			if (_curState != null && _isStarted)
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
			this.currentReleaser =per!.CreateReleaser(string.Empty,this.releaser, magic,
				new ReleaseAtTarget(this.releaser, this.target),
				ReleaserType.Magic, Proto.ReleaserModeType.RmtMagic,0);

		}

		public void ReplaceRelease(int level, CharacterData data, bool stay, bool ai)
		{
			if (!stay && this.releaser) this.releaser.SubHP(this.releaser.HP, out _);
			var per = _curState.Perception as BattlePerception;
			var scene = PerView.UScene;
			var magics = data.CreateHeroMagic(); // per.CreateHeroMagic(data.ID);
			var levelData =
				ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.CharacterID == data.ID && t.Level == level);
			var properties = data.CreatePlayerProperties(levelData);

			if (levelData != null) properties.TryAddBase(levelData.Properties, levelData.PropertyValues);

			releaser = per!.CreateCharacter(per.StateControllor, level, data, magics, properties, 1,
				scene.startPoint.position + (UVector3.right * distanceCharacter / 2)
				, new UVector3(0, 90, 0), string.Empty, data.Name);
	
			_tCamera.SetLookAt(releaser.Transform);
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


			var target = per!.CreateCharacter(per.AIControllor, level, data, magics,properties, 2, scene.enemyStartPoint.position + (UVector3.left * distanceCharacter / 2),
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

		private bool _isStarted = false;

		private ThirdPersonCameraContollor _tCamera;

		// Update is called once per frame
		protected  void Update()
		{
			if (!_isStarted) return;
			Tick();
			_tCamera.SetXY(sliderY,ry);
			_tCamera.distance = distance;
			if (!isChanged) return;
			var position = PerView.UScene.startPoint.position;
			var left = position + (UVector3.left * distanceCharacter / 2);
			var right = position + (UVector3.right * distanceCharacter / 2);
			releaser.Position = left;
			target.Position = right;
			isChanged = false;
		}

		public bool isChanged = false;

		public float sliderY = 1f;
		public float distance = 5;
		public float ry =0;
		public float distanceCharacter = 8;


		AITreeRoot IAIRunner.RunAI(TreeNode ai)
		{
			if (_curState.Perception is not BattlePerception p) return null;
			var root = p.ChangeCharacterAI(ai, releaser);
			root.IsDebug = true;
			return root;
		}

		bool IAIRunner.IsRunning(Layout.EventType eventType)
		{
			return currentReleaser?.IsRunning(eventType) == true;
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
}
//#endif
