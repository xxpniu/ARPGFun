using System;
using System.Collections.Generic;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Components;
using Cysharp.Threading.Tasks;
using EConfig;
using EngineCore.Simulater;
using ExcelConfig;
using GameLogic;
using GameLogic.Game;
using GameLogic.Game.AIBehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Google.Protobuf;
using Layout;
using Layout.AITree;
using Layout.LayoutElements;
using Proto;
using UnityEngine;
using UVector3 = UnityEngine.Vector3;


namespace BattleViews.Views
{
    public class UPerceptionView : MonoBehaviour, IBattlePerception, ITimeSimulater, IViewBase
    {
        public UGameScene UScene;
        public bool UseCache = true;
        public Action<UCharacterView> OnCreateCharacter;
        private TreeNode LoadTreeXml(string pathTree)
        {
            var xml = ResourcesManager.Singleton.LoadText(pathTree);
            var root = XmlParser.DeSerialize<TreeNode>(xml);
            return root;
        }
        public UCharacterView Owner => GetViewByIndex<UCharacterView>(OwnerIndex);
        public int OwnerIndex { set; get; } = -1;
        public int OwnerTeamIndex { set; get; } = -1;
        private float _startTime = 0;
        private void Awake()
        {
            UScene = FindObjectOfType<UGameScene>();
        }

        private async void  Start()
        {
            _startTime = Time.timeSinceLevelLoad;
            _now = new GTime( Time.timeSinceLevelLoad-_startTime, Time.deltaTime);
           
            _magicData = new Dictionary<string, MagicData>();
            _timeLines = new Dictionary<string, TimeLine>();
#if !UNITY_SERVER
            var g=  GPUBillboardBuffer.S;

            await UniTask.WaitUntil(()=>g.IsReady);
            g.SetupBillboard(1000);
            g.SetDisappear(2);
            g.SetScaleParams(0f, 0.5f, 0.5f, 1f, 1f);

            _param = new DisplayNumberInputParam()
            {
                RandomXInitialSpeedMin = 0f,
                RandomXInitialSpeedMax = 0f,

                RandomYInitialSpeedMin = 1f,
                RandomYInitialSpeedMax = 2f,

                RandomXaccelerationMin = 0,
                RandomXaccelerationMax = 0,

                RandomYaccelerationMin = 1,
                RandomYaccelerationMax = 3,
                NormalTime =.25f,
                FadeTime = .5f,

            };

            _mulParam = new DisplayNumberInputParam()
            {
                RandomXInitialSpeedMin = 0f,
                RandomXInitialSpeedMax = 0f,

                RandomYInitialSpeedMin = 1f,
                RandomYInitialSpeedMax = 2f,

                RandomXaccelerationMin = 0,
                RandomXaccelerationMax = 0,

                RandomYaccelerationMin = 1,
                RandomYaccelerationMax = 3,
                NormalTime = .8f,
                FadeTime = .3f,

            };
#endif
        }

        private DisplayNumberInputParam _param;
        private DisplayNumberInputParam _mulParam;

        internal void ShowHpCure(UVector3 pos, int hp)
        {
#if !UNITY_SERVER
            GPUBillboardBuffer.S.DisplayNumberRandom($"{hp}", new Vector2(.2f, .2f), pos, Color.green, true, _param);
#endif
        }

        internal void ShowMpCure(UVector3 pos, int mp)
        {
#if !UNITY_SERVER
            GPUBillboardBuffer.S.DisplayNumberRandom($"{mp}", new Vector2(.2f, .2f), pos, Color.blue, true, _param);
#endif
        }

        public T GetViewByIndex<T>(int releaseIndex) where T: UElementView
        {
            if (_attachElements.TryGetValue(releaseIndex, out UElementView vi))
                return vi as T;
            return null;
        }

        void Update() => _now.TickTime(Time.deltaTime);

        public int timeLineCount = 0;
        public int magicCount =0;

        private Dictionary<string,TimeLine> _timeLines;
        private Dictionary<string ,MagicData> _magicData;

        private readonly Queue<IMessage> _notify = new Queue<IMessage>();

        private readonly Dictionary<int, UElementView> _attachElements = new Dictionary<int, UElementView>();

        private readonly Dictionary<int, UMagicReleaserView> _ownerReleasers = new Dictionary<int, UMagicReleaserView>();

        public void DeAttachView(UElementView battleElement)
        {
            _attachElements.Remove(battleElement.Index);
            if (battleElement is not UMagicReleaserView r) return;
            if (r.CharacterReleaser.Index == OwnerIndex)
            {
                _ownerReleasers.Remove(r.Index);//, r);
            }
        }

        public void AttachView(UElementView battleElement)
        {
            _attachElements.Add(battleElement.Index, battleElement);
            if (battleElement is not UMagicReleaserView r) return;
            if (r.CharacterReleaser.Index == OwnerIndex) _ownerReleasers.Add(r.Index, r);
        }

        public bool HaveOwnerKey(string key)
        {
            foreach (var i in _ownerReleasers)
            {
                if (i.Value.MagicKey == key) return true;
            }
            return false;
        }

        private readonly IMessage[] _empty = Array.Empty<IMessage>();

        public IMessage[] GetAndClearNotify()
        {
            if (_notify.Count <= 0) return _empty;
            var list = _notify.ToArray();
            _notify.Clear();
            return list;
        }

        public IMessage[] GetInitNotify()
        {
            var list = new List<IMessage>();
            foreach (var i in _attachElements)
            {
                if (i.Value is ISerializationElement sElement)
                {
                    list.Add(sElement.ToInitNotify());
                }
            }
            return list.ToArray();
        }

        public void AddNotify(IMessage notify)
        {
#if UNITY_SERVER || UNITY_EDITOR
            _notify.Enqueue(notify);
#endif
        }

        private GTime _now;

        public GTime GetTime() =>_now; 

        public static UPerceptionView Create(ConstantValue constValue)
        {
            var go = new GameObject("PreView");
            var u= go.AddComponent<UPerceptionView>();
            u._constValue = constValue;
            return u;
        }

        GTime ITimeSimulater.Now => GetTime();

        private ConstantValue _constValue;

        ConstantValue IViewBase.GetConstant => _constValue;

        private TimeLine TryToLoad(string path)
        {
            var lineAsset = ResourcesManager.S.LoadText(path);
            if (string.IsNullOrEmpty(lineAsset)) return null;

            var line = XmlParser.DeSerialize<TimeLine> (lineAsset);
            if (UseCache) 
            {
                _timeLines.Add (path, line);
            } 
            return line;
        }

        private MagicData TryLoadMagic(string key)
        {
            if (_magicData.TryGetValue(key, out var magic)) return magic;
            var asset = ResourcesManager.S.LoadText($"Magics/{key}.xml");
            if (string.IsNullOrEmpty(asset)) return null;
            magic = XmlParser.DeSerialize<MagicData>(asset);
            if(UseCache) _magicData.Add(key, magic);
            return magic;
        }

        public void Each<T>(Func<T, bool> invoke) where T : UElementView
        {
            foreach (var i in _attachElements)
            {
                if (!i.Value) continue;
                if (!(i.Value is T t)) continue;
                if (invoke?.Invoke(t) == true) return;
            }
        }

        #region IBattlePerception implementation

        bool IBattlePerception.ProcessDamage(int owner, int target, int damage, bool isMissed, int crtmult)
        {
#if UNITY_SERVER|| UNITY_EDITOR
            AddNotify(new Notify_DamageResult
            {
                Index = owner,
                TargetIndex = target,
                Damage = damage,
                IsMissed = isMissed,
                CrtMult = crtmult
            });
#endif

#if !UNITY_SERVER
            var  chDisplay = GetViewByIndex<UCharacterView>(isMissed ? owner : target);
            if (!chDisplay) return true;
            var num = (isMissed ? "MISS" : $"{damage}");
            var bone = chDisplay.GetBoneByName(UCharacterView.TopBone);
            if (!bone) return true;
            var pos = bone.transform.position;
            if (crtmult > 1) {
                GPUBillboardBuffer.S.
                    DisplayNumberRandom(num,
                        new Vector2(.5f, .5f)*crtmult, pos, Color.red, true, _mulParam);
            }
            else
            {
                GPUBillboardBuffer.S.
                    DisplayNumberRandom(num,
                        new Vector2(.2f, .2f), pos, Color.red, true, _param);
            }
#endif
            return true;
        }

        TimeLine IBattlePerception.GetTimeLineByPath(string path)
        {
            if (UseCache && _timeLines.TryGetValue(path, out var  line)) return line;
            line = TryToLoad(path);
            if (line == null)
            {
                Debug.LogError($"Not found:{path}");
            }
            return line;
        }

        MagicData IBattlePerception.GetMagicByKey(string key)
        {
            MagicData magic;
            if (UseCache)
            {
                if (_magicData.TryGetValue(key, out magic))
                {
                    return magic;
                }
            }
            magic = TryLoadMagic(key);
            if (magic == null) Debug.LogError("No found magic by key:" + key);
            return magic;
        }

        bool IBattlePerception.ExistMagicKey (string key) => TryLoadMagic(key) !=null;

        IBattleCharacter IBattlePerception.CreateBattleCharacterView(string accountId,
            int config, int teamId, Proto.Vector3 pos, Proto.Vector3 forward,
            int level, string characterName,IList<HeroMagicData> cds,int owner,IList<HeroProperty> properties,int hp, int mp)
        {
            var dic = new Dictionary<HeroPropertyType, ComplexValue>();
            foreach (var i in properties)
            {
                dic.Add(i.Property, i.Value);
            }

            var data = ExcelToJSONConfigManager.GetId<CharacterData>(config);
            var qu = Quaternion.Euler(forward.X, forward.Y, forward.Z);
            var root = new GameObject(data.ResourcesPath);
            root.transform.SetParent(this.transform, false);
            root.transform.position = pos.ToUV3();
            root.transform.rotation = Quaternion.identity;
            var body = new GameObject("__VIEW__");
            body.transform.SetParent(root.transform, false);
            body.transform.RestRTS();

            var view = root.AddComponent<UCharacterView>();
            view.SetPerception(this);
            view.LookQuaternion = view.targetLookQuaternion = qu;
            view.TeamId = teamId;
            view.Level = level;
            view.Speed = dic[HeroPropertyType.MoveSpeed]/100f;
            view.ConfigID = config;
            view.accoundUuid = accountId;
            view.Name = characterName;
            view.OwnerIndex  = owner;
            view.properties = properties;
            if (cds != null) { foreach (var i in cds) view.AddMagicCd(i.MagicID, i.CDCompletedTime, i.MType,i.CdTotalTime,i.MPCost); }
            if (view is IBattleCharacter ch) ch.SetHpMp(hp, dic[HeroPropertyType.MaxHp], mp, dic[HeroPropertyType.MaxMp]);
            view.SetCharacter(body, data.ResourcesPath);
            view.SetScale(data.ViewSize);
            OnCreateCharacter?.Invoke(view);
            return view;
        }

        IMagicReleaser IBattlePerception.CreateReleaserView(Proto.Vector3 pos, Proto.Vector3 ration, int releaser, int target, string magicKey, Proto.Vector3 targetPos, Proto.ReleaserModeType rmType)
        {
            var obj = new GameObject($"Releaser:{magicKey}");
            obj.transform.SetParent(this.transform, false);
            obj.transform.position = pos.ToUV3();
            obj.transform.rotation = Quaternion.Euler(ration.ToUV3());
            var view = obj.AddComponent<UMagicReleaserView>();
            view.SetPerception(this);
            view.SetData(releaser, target, targetPos.ToUV3(),rmType,magicKey);
            return view;
        }
        
        IBattleMissile IBattlePerception.CreateMissile(int releaseIndex, int targetIndex, string res, Proto.Vector3 offset , string fromBone, string toBone, float speed,int mType, float maxDis, float maxLifeTime)
        {
            var root = new GameObject(res);
            var missile = root.AddComponent<UBattleMissileView> (); //NO
            missile.fromBone = fromBone;
            missile.toBone = toBone;
            missile.speed = speed;
            missile.offset = offset.ToUV3();
            missile.res = res;
            missile.SetPerception(this);
            missile.releaserIndex = releaseIndex;
            missile.MaxDis = maxDis;
            missile.MaxLifeTime = maxLifeTime;
            missile.MType = (MovementType) mType;
            missile.TargetIndex = targetIndex;
            return missile;
        }

        public IParticlePlayer CreateParticlePlayer(IMagicReleaser releaser, ParticleLayout layout, IBattleCharacter eventTarget)
        {
            var viewRoot = new GameObject(layout.path);
            var view = viewRoot.AddComponent<UParticlePlayer>();
            view.Path = layout.path;
            var viewRelease = releaser as UMagicReleaserView;
            var viewTarget = viewRelease!.CharacterTarget;
            var characterView = viewRelease.CharacterReleaser;
            var eventView = eventTarget as UCharacterView;
            var bind = layout.Bind;
            UCharacterView form =null;
            UVector3? formPos =UVector3.zero;
            var rotation = Quaternion.identity;
            switch (layout.fromTarget)
            {
                case TargetType.EventTarget:
                    form = eventView;
                    break;
                case TargetType.Releaser:
                    form = characterView;
                    break;
                case TargetType.Target:
                    form = viewTarget;
                    break;
                case TargetType.TargetPosition:
                    bind = false;
                    formPos = viewRelease.TargetPos;
                    break;
                case TargetType.ReleaseInstance:
                    formPos = viewRelease.transform.position;
                    break;
            }

            if (form)
            {
                formPos = form.GetBoneByName(layout.fromBoneName).position;
                rotation = ((IBattleCharacter)form).Rotation;
            }


            if (bind)
            {
                var bone = form!.GetBoneByName(layout.fromBoneName);
                if (bone) viewRoot.transform.SetParent(bone, false);
                viewRoot.transform.RestRTS();
            }
            else
            {
                viewRoot.transform.SetParent(transform, false);
                viewRoot.transform.RestRTS();
                viewRoot.transform.position = formPos.Value;
            }

            viewRoot.transform.rotation = rotation *  Quaternion.Euler(layout.rotation.ToUV3());
            viewRoot.transform.position += viewRoot.transform.rotation * layout.offet.ToUV3();
            viewRoot.transform.localScale =  UVector3 .one* layout.localsize;
            return view;
        }

        ITimeSimulater IBattlePerception.GetTimeSimulater() => this;

        TreeNode IBattlePerception.GetAITree (string pathTree) =>LoadTreeXml(pathTree);

        IBattlePerception IViewBase.Create(ITimeSimulater simulater) => this;
        
        TreeNode ITreeLoader.Load(string path) =>LoadTreeXml(path);

        IBattleItem IBattlePerception.CreateDropItem(Proto.Vector3 pos, PlayerItem item, int teamIndex, int groupId)
        {
            var config = ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
            var root = new GameObject(config.Name);
            root.transform.SetParent(this.transform);
            root.transform.RestRTS();
            root.transform.position = pos.ToUV3();
            var bi = root.AddComponent<UBattleItem>();
            bi.SetInfo(item, teamIndex, groupId);
            bi.SetPerception(this);
            return bi;
        }


        #endregion

    }
}
