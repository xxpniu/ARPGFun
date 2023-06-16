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

namespace Windows
{
    partial class UUIBattle
    {
        public class GridTableModel : TableItemModel<GridTableTemplate>
        {
            public GridTableModel() { }
            public override void InitModel()
            {
                this.Template.Button.onClick.AddListener(ClickItem);
            }

            public void ClickItem()
            {
                if ((lastTime + 0.3f > UnityEngine.Time.time)) return;
                lastTime = UnityEngine.Time.time;
                OnClick?.Invoke(this);
            }

            public Action<GridTableModel> OnClick;
            public HeroMagicData Data;
            public async void SetMagic(HeroMagicData  data,IBattleGate battle, KeyCode key )
            {
                Data = data;
                if (magicID == data.MagicID) return;
                magicID = data.MagicID;
                MagicData = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.MagicID);
                var per = battle.PreView as IBattlePerception;
                LMagicData = per.GetMagicByKey(MagicData.MagicKey);
                Template.Icon.sprite =await ResourcesManager.S.LoadIcon(MagicData);
                Template.tb_key.text = $"{key}";
            }
            private int magicID = -1;
            public CharacterMagicData MagicData;
            private float cdTime = 0.01f;
            private float lastTime = 0;
            private MagicData LMagicData;

            public void Update(UCharacterView view, float now,bool haveKey)
            {
                if (LMagicData == null) return;
                if (LMagicData.unique)  Template.Button.interactable = !haveKey;
                else  Template.Button.interactable = true;

                if (!view.TryGetMagicData(magicID, out var data)) return;
                var time = Mathf.Max(0, data.CDCompletedTime - now);
                this.Template.CDTime.text = time > 0 ? $"{time:0.0}" : string.Empty;
                cdTime = Mathf.Max(0.01f, data.CdTotalTime);
                if (time > 0)
                {
                    lastTime = UnityEngine.Time.time;
                }
                if (cdTime > 0)
                {
                    this.Template.ICdMask.fillAmount = time / cdTime;
                }
                else
                {
                    this.Template.ICdMask.fillAmount = 0;
                }
            }
        }

        public Texture2D Map;
        private Color32[] Colors;
        private readonly int size=75;

        protected override void InitModel()
        {
            base.InitModel();

            Map = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            var a = new Color(1, 1, 1, 0);
            Colors = new Color32[size * size];
            for (var x =0; x< size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    Colors[x + y* size] = a;
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

            var bt = this.Joystick_Left.GetComponent<zFrame.UI.Joystick>();
            float lastTime = -1;
            //Vector2 last = Vector2.zero;
            bt.OnValueChanged.AddListener((v) =>
            {
                if (lastTime > UnityEngine.Time.time) return;
                lastTime = UnityEngine.Time.time + .3f;
                var dir = ThirdPersonCameraContollor.Current.LookRotation * new Vector3(v.x, 0, v.y);
                BattleGate?.MoveDir(dir);
            });

            var swipeEv = swipe.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                v *= .5f;
                
                ThirdPersonCameraContollor.Current.RotationByX(v.y).RotationByY(v.x);
                BattleGate?.TrySendLookForward(false);
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

        private string keyHp = string.Empty;

        private string keyMp = string.Empty;

        private void InitHero(DHero hero)
        {
            this.Level_Number.text = $"{hero.Level}";
            this.Username.text = $"{hero.Name}";
            var data = ExcelToJSONConfigManager.GetId<CharacterData>(hero.HeroID);
            //var character = ExcelToJSONConfigManager.Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == hero.HeroID);
            _normalAtt = data?.NormalAttack??-1;
            this.Level_Number.text = $"{hero.Level}";
            this.Username.text = $"{hero.Name}";
            var leveUp = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == hero.Level + 1);
            //lb_exp.text = $"{hero.Exprices}/{leveUp?.NeedExprices ?? '-'}";
            float v = 0;
            if (leveUp != null)
                v = (float)hero.Exprices / leveUp.NeedExp;
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
                    keyHp = config.Params1;
                }
                if ((ItemType)config.ItemType == ItemType.ItMpitem)
                {
                    keyMp = config.Params1;
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

        public IBattleGate BattleGate { private set; get; }

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
                GridTableManager[i].Model.ClickItem();
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
            bt_hp.interactable = !BattleGate.PreView.HaveOwnerKey(keyHp);
            bt_mp.interactable = !BattleGate.PreView.HaveOwnerKey(keyMp);

            //Debug.Log(BattleGate.LeftTime);

            var lTime = TimeSpan.FromSeconds(Mathf.Max(0, BattleGate.LeftTime));
            lb_text.text = $"{(int)lTime.TotalMinutes}:{lTime.Seconds}"; 
          
        }

        private void UpdateMap()
        {

            var wi = Map.width;
           
            if (!BattleGate.Owner) return;
            var a = new Color(1, 1, 1, 0);
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    Colors[x+ y* size] =a;
                }
            }

            var lookRotation = Quaternion.Euler(0, 0, ThirdPersonCameraContollor.Current.transform.rotation.eulerAngles.y);
            this.ViewForward.localRotation = lookRotation;

            var r = size / 2f;// 16; 
            BattleGate.PreView.Each<UCharacterView>(t =>
            {
                var offset = t.transform.position - BattleGate.Owner.transform.position;
                if (offset.magnitude > r) return false;
                Colors[(int)(offset.x + r)+ (int)(offset.z + r)* size] = t.TeamId == BattleGate.Owner.TeamId ? Color.green : Color.red;
                return false;
            });

            Map.SetPixels32(Colors);
            Map.Apply();
        }
        

        private async void InitCharacter(UCharacterView view)
        {
            
            if (view.TryGetMagicsType(MagicType.MtMagic, out var list))
            {
                this.GridTableManager.Count = list.Count;
                int index = 0;
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
        
        private void OnRelease(GridTableModel item)
        {
            if (!BattleGate.ReleaseSkill(item.Data))
            {
                UApplication.S.ShowNotify(LanguageManager.S["UIBattle_Release_Skill_Error"]);
            }
        }

        public bool IsMagic(int id)
        {
            var data = ExcelToJSONConfigManager.GetId<CharacterMagicData>(id);
            if (data == null) return false;
            return data.ReleaseType == (int)MagicReleaseType.MrtMagic;
        }

    
    }
}