using System;
using System.Collections.Generic;
using System.Linq;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Components;
using BattleViews.Views;
using UnityEngine;
using Proto;
using ExcelConfig;
using GameLogic.Game.Perceptions;
using EConfig;
using Vector3 = UnityEngine.Vector3;
using Layout;
using UApp;
using UApp.GameGates;
using UGameTools;

namespace Windows
{
    
    partial class UUIBattle
    {
        public class GridTableModel : TableItemModel<GridTableTemplate>
        {
            public GridTableModel() { }
            private SwipeButton Button {  set; get; }

            public override void InitModel()
            {
                Button = this.Template.Button.GetComponent<SwipeButton>();
                Button.OnSwipeClickEvent.AddListener(ClickItem);
                Template.Forward.gameObject.SetActive(false);
                Button.OnSwipeStarted.AddListener(() =>
                {
                    Template.Forward.gameObject.SetActive(true);
                });
                
                Button.OnSwipeEnd.AddListener(() =>
                {
                    Template.Forward.gameObject.SetActive(false);
                });
                
                Button.OnDragging.AddListener((dir) =>
                {
                    if (!dir.HasValue) return;
                    var lookV = new Vector3(dir.Value.x, 0, dir.Value.y);
                    var look = Quaternion.LookRotation(lookV);
                    //Debug.Log($"{dir} {look.eulerAngles.y}");
                    Template.Forward.transform.rotation = Quaternion.Euler(0,0,-look.eulerAngles.y);
                });
            }

            public void ClickItem(Vector2? dir)
            {
                if ((_lastTime + 0.3f > UnityEngine.Time.time)) return;
                _lastTime = Time.time;
                if (dir.HasValue)
                {
                    var lookV = new Vector3(dir.Value.x, 0, dir.Value.y);
                    var look = Quaternion.LookRotation(lookV);
                    //Debug.Log($"{dir} {look.eulerAngles.y}");
                }
                OnClick?.Invoke(this,dir);
            }

            public Action<GridTableModel,Vector2?> OnClick;
            public HeroMagicData Data;
            public async void SetMagic(HeroMagicData  data,IBattleGate battle, KeyCode key )
            {
                Data = data;
                if (_magicID == data.MagicID) return;
                _magicID = data.MagicID;
                MagicData = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.MagicID);
                var per = battle.PreView as IBattlePerception;
                _lMagicData = per.GetMagicByKey(MagicData.MagicKey);
                Template.Icon.sprite =await ResourcesManager.S.LoadIcon(MagicData);
                Template.tb_key.text = $"{key}";
            }
            private int _magicID = -1;
            public CharacterMagicData MagicData;
            private float _cdTime = 0.01f;
            private float _lastTime = 0;
            private MagicData _lMagicData;

            public void Update(UCharacterView view, float now,bool haveKey)
            {
                if (_lMagicData == null) return;
                if (_lMagicData.unique)  Button.interactable = !haveKey;
                else  Button.interactable = true;

                if (!view.TryGetMagicData(_magicID, out var data)) return;
                var time = Mathf.Max(0, data.CDCompletedTime - now);
                this.Template.CDTime.text = time > 0 ? $"{time:0.0}" : string.Empty;
                _cdTime = Mathf.Max(0.01f, data.CdTotalTime);
                if (time > 0)
                {
                    _lastTime = Time.time;
                }
                if (_cdTime > 0)
                {
                    Template.ICdMask.fillAmount = time / _cdTime;
                }
                else
                {
                    Template.ICdMask.fillAmount = 0;
                }
            }
        }

        public Texture2D Map;
        private Color32[] _colors;
        private const int Size = 75;

        protected override void InitModel()
        {
            base.InitModel();

            Map = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true);
            var a = new Color(1, 1, 1, 0);
            _colors = new Color32[Size * Size];
            for (var x =0; x< Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    _colors[x + y* Size] = a;
                }
            }

            this.MapTexture.texture = Map;

            bt_Exit.onClick.AddListener(() =>
                {
                    UUIPopup.ShowConfirm(
                        LanguageManager.S["UUIBattle_Quit_Title"],
                        LanguageManager.S["UUIBattle_Quit_Content"],
                        () =>
                    {
                        BattleGate.Exit();
                    });
                });

            var bt = this.Joystick_Left.GetComponent<ETCJoystick>();
            float lastTime = -1;
            //Vector2 last = Vector2.zero;
            bt.onMove.AddListener((v) =>
            {
                if (lastTime > UnityEngine.Time.time) return;
                lastTime = UnityEngine.Time.time + .3f;
                var dir = ThirdPersonCameraContollor.Current.LookRotation * new Vector3(v.x, 0, v.y);
                BattleGate?.MoveDir(dir);
            });
            bt.onMoveEnd.AddListener(() =>
            {
                BattleGate?.MoveDir(Vector2.zero);
            });

            var swipeEv = swipe.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                v *= .5f;
                ThirdPersonCameraContollor.Current.RotationByX(v.y).RotationByY(v.x);
                //BattleGate?.TrySendLookForward(false);
            });

            bt_normal_att.onClick.AddListener(() =>
            {
                BattleGate?.DoNormalAttack();
            });

            bt_hp.onClick.AddListener(UseHpItem);
            bt_mp.onClick.AddListener(UseMpItem);

            ThirdPersonCameraContollor.Current
                .SetClampX(15, 80).SetForwardOffset(Vector3.up * 1.5f);
        }
        private void OnRelease(GridTableModel item, Vector2? dir)
        {
            Vector3? forward = null;
            if (dir.HasValue)
            {
                forward = ThirdPersonCameraContollor.Current.LookRotation 
                          * new Vector3(dir.Value.x, 0 , dir.Value.y);
                forward = forward.Value.ZeroY();
                Debug.Log($"Forward:{dir} to {forward}");
            }

            if (!BattleGate.ReleaseSkill(item.Data, forward))
            {
                UApplication.S.ShowNotify(LanguageManager.S["UIBattle_Release_Skill_Error"]);
            }
        }

        private void UseMpItem()
        {
            if (BattleGate?.IsMpFull() == true)
            {
                UApplication.S.ShowNotify(LanguageManager.S["UUIBattle_MP_Full"]);
                return;
            }

            BattleGate?.SendUseItem(ItemType.ItMpitem);
        }

        private void UseHpItem()
        {
            if (BattleGate?.IsHpFull() == true)
            {
                UApplication.S.ShowNotify(LanguageManager.S["UUIBattle_HP_Full"]);
                return;
            }

            BattleGate?.SendUseItem(ItemType.ItHpitem);
        }

        private string _keyHp = string.Empty;

        private string _keyMp = string.Empty;

        private void InitHero(DHero hero)
        {
            Level_Number.text = $"{hero.Level}";
            Username.text = $"{hero.Name}";
            var data = ExcelToJSONConfigManager.GetId<CharacterData>(hero.HeroID);
            //var character = ExcelToJSONConfigManager.Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == hero.HeroID);
            _normalAtt = data?.NormalAttack??-1;
            Level_Number.text = $"{hero.Level}";
            Username.text = $"{hero.Name}";
            var leveUp = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == hero.Level + 1);
            //lb_exp.text = $"{hero.Exprices}/{leveUp?.NeedExprices ?? '-'}";
            float v = 0;
            if (leveUp != null) v = (float)hero.Exprices / leveUp.NeedExp;
            user_exp.fillAmount = v;
        }

        private int _normalAtt = -1;

        //private PlayerPackage Package;

        internal void ShowWindow(IBattleGate gate)
        {
            this.BattleGate = gate;
            ShowWindow();
        }

        private void ShowView()
        {
            SetPackage(BattleGate.Package);
            InitHero(BattleGate.Hero);
            foreach (var i in BattleGate.Package.Items)
            {
                var config = ExcelToJSONConfigManager.GetId<ItemData>(i.Value.ItemID);
                if ((ItemType)config.ItemType == ItemType.ItHpitem)
                {
                    _keyHp = config.Params1;
                }
                if ((ItemType)config.ItemType == ItemType.ItMpitem)
                {
                    _keyMp = config.Params1;
                }
            }
            InitCharacter(BattleGate.Owner);
        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            ShowView();
        }

        protected override void OnShow()
        {
            base.OnShow();
            this.GridTableManager.Count = 0;
            ShowView();
        }

        private IBattleGate BattleGate { set; get; }

        private readonly KeyCode[] _keyCodes = { KeyCode.H ,KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.N, KeyCode.M};

        protected override void OnUpdate()
        {
            base.OnUpdate();

            #region  快捷键

            if (Input.GetKey(KeyCode.B))
            {
                BattleGate?.DoNormalAttack();
            }
            
            if (Input.GetKey(KeyCode.Q))
            {
                UseHpItem();
            }

            if (Input.GetKey(KeyCode.E))
            {
                UseMpItem();
            }

            for (var i = 0; i < _keyCodes.Length; i++)
            {
                if (GridTableManager.Count <= i) break;
                if (!Input.GetKey(_keyCodes[i])) continue;
                GridTableManager[i].Model.ClickItem(null);
            }


            #endregion
            
            var view = BattleGate?.Owner;
            if (!view) return;
            HPSilder.value = view.HP / (float)view.HpMax;
            lb_hp.text = $"{view.HP}/{view.HpMax}";
            MpSilder.value = view.MP / (float)view.MpMax;
            lb_mp.text = $"{view.MP}/{view.MpMax}";
            hp_bg.color = Color.Lerp(Color.red,Color.green,  Mathf.Max(0,HPSilder.value -0.5f)*2);

            foreach (var i in GridTableManager)
            {
                i.Model.Update(view, BattleGate.TimeServerNow, BattleGate.PreView.HaveOwnerKey(i.Model.MagicData.MagicKey));
            }
            UpdateMap();
            if (view.TryGetMagicData(_normalAtt, out HeroMagicData att))
            {
                var time = Mathf.Max(0, att.CDCompletedTime - BattleGate.TimeServerNow);
                var cdTime = Mathf.Max(0.01f, att.CdTotalTime);// view.AttackSpeed 
                //if (cdTime < time) cdTime = time;
                if (cdTime > 0)
                {
                    this.AttCdMask.fillAmount = time / cdTime;
                }
                else
                {
                    this.AttCdMask.fillAmount = 0;
                }
            }
            bt_hp.interactable = !BattleGate.PreView.HaveOwnerKey(_keyHp);
            bt_mp.interactable = !BattleGate.PreView.HaveOwnerKey(_keyMp);

            //Debug.Log(BattleGate.LeftTime);

            var lTime = TimeSpan.FromSeconds(Mathf.Max(0, BattleGate.LeftTime));
            lb_text.text = $"{(int)lTime.TotalMinutes}:{lTime.Seconds}"; 
          
        }

        private void UpdateMap()
        {

            var wi = Map.width;
           
            if (!BattleGate.Owner) return;
            var a = new Color(1, 1, 1, 0);
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    _colors[x+ y* Size] =a;
                }
            }

            var lookRotation = Quaternion.Euler(0, 0, -ThirdPersonCameraContollor.Current.transform.rotation.eulerAngles.y);
            this.ViewForward.localRotation = lookRotation;

            var r = Size / 2f;// 16; 
            BattleGate.PreView.Each<UCharacterView>(t =>
            {
                var offset = t.transform.position - BattleGate.Owner.transform.position;
                if (offset.magnitude > r) return false;
                _colors[(int)(offset.x + r)+ (int)(offset.z + r)* Size] = t.TeamId == BattleGate.Owner.TeamId ? Color.green : Color.red;
                return false;
            });

            Map.SetPixels32(_colors);
            Map.Apply();
        }
        

        private async void InitCharacter(UCharacterView view)
        {
            
            if (view.TryGetMagicsType(MagicType.MtMagic, out var list))
            {
                GridTableManager.Count = list.Count;
                var index = 0;
                foreach (var i in GridTableManager)
                {
                    i.Model.SetMagic(list[index],BattleGate, _keyCodes[index]);
                    i.Model.OnClick = OnRelease;
                    index++; 
                }
            }

            if (view.TryGetMagicByType(MagicType.MtNormal, out var  data))
            {
                var config = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.MagicID);
                att_Icon.sprite = await  ResourcesManager.S.LoadIcon(config);
            }
            Player.texture = BattleGate.LookAtView;
        }

        private async void SetPackage(PlayerPackage package)
        {
            int hp = 0, mp = 0;

            foreach (var i in package.Items)
            {
                var config = ExcelToJSONConfigManager.GetId<ItemData>(i.Value.ItemID);
                if ((ItemType)config.ItemType == ItemType.ItHpitem)
                {
                    hp += i.Value.Num;
                    hp_item_Icon.sprite= await ResourcesManager.S.LoadIcon(config);
                }

                if ((ItemType)config.ItemType != ItemType.ItMpitem) continue;
                mp_item_Icon.sprite = await ResourcesManager.S.LoadIcon(config);
                mp += i.Value.Num;
            }

            bt_hp.ActiveSelfObject(hp > 0);
            bt_mp.ActiveSelfObject(mp > 0);
            hp_num.text = $"{hp}";
            mp_num.text = $"{mp}";
        }
        
  

        public bool IsMagic(int id)
        {
            var data = ExcelToJSONConfigManager.GetId<CharacterMagicData>(id);
            if (data == null) return false;
            return data.ReleaseType == (int)MagicReleaseType.MrtMagic;
        }

    
    }
}