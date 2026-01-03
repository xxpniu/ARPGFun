using System;
using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Components;
using BattleViews.Views;
using Cysharp.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Perceptions;
using Layout;
using Proto;
using UApp;
using UApp.GameGates;
using UGameTools;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Windows
{
    internal partial class UUIBattle
    {
        private const int Size = 75;

        private readonly KeyCode[] _keyCodes = { KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.N, KeyCode.M };
        private Color32[] _colors;

        private string _keyHp = string.Empty;

        private string _keyMp = string.Empty;

        private float _lastTime = -1;

        private int _normalAtt = -1;
        private PlayInput _playInput;

        public Texture2D Map;

        private IBattleGate BattleGate { set; get; }

        protected override void InitModel()
        {
            base.InitModel();


            _playInput = new PlayInput();

            Map = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true);
            var a = new Color(1, 1, 1, 0);
            _colors = new Color32[Size * Size];
            for (var x = 0; x < Size; x++)
            for (var y = 0; y < Size; y++)
                _colors[x + y * Size] = a;

            MapTexture.texture = Map;

            bt_Exit.onClick.AddListener(() =>
            {
                UUIPopup.ShowConfirm(
                    LanguageManager.S["UUIBattle_Quit_Title"],
                    LanguageManager.S["UUIBattle_Quit_Content"],
                    () => { BattleGate.Exit(); },
                    () => { }
                );
            });


            var swipeEv = swipe.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener(v =>
            {
                v *= .5f;
                ThirdPersonCameraContollor.Current.RotationByX(v.y).RotationByY(v.x);
                //BattleGate?.TrySendLookForward(false);
            });

            bt_normal_att.onClick.AddListener(() => { BattleGate?.DoNormalAttack(); });

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
                          * new Vector3(dir.Value.x, 0, dir.Value.y);
                forward = forward.Value.ZeroY();
                Debug.Log($"Forward:{dir} to {forward}");
            }

            if (!BattleGate.ReleaseSkill(item.Data, forward, out var res))
            {
                if (res == ReleaseResult.NoMp)
                {
                    HighLightMp();
                }
                UApplication.S.ShowNotify(LanguageManager.S["UIBattle_Release_Skill_Error"]);
            }
        }

        private async void HighLightMp()
        {
            var token = this.DestroyCancellationToken();
            var delayTime = 1.5f;
            var time = Time.time;
            var normal = new Color(132 / 255f, 132 / 255f, 132 / 255f, 255 / 255f);
            var highLight = Color.red;
            var image = MpSilder.image;
            while (time + delayTime > Time.time)
            {
                image.color = highLight;
                await UniTask.DelayFrame(5, cancellationToken: token);
                image.color = normal;
                await UniTask.DelayFrame(5, cancellationToken: token);
            }

            image.color = normal;
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

        private void InitHero(DHero hero)
        {
            Level_Number.text = $"{hero.Level}";
            Username.text = $"{hero.Name}";
            var data = ExcelToJSONConfigManager.GetId<CharacterData>(hero.HeroID);
            //var character = ExcelToJSONConfigManager.Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == hero.HeroID);
            _normalAtt = data?.NormalAttack ?? -1;
            Level_Number.text = $"{hero.Level}";
            Username.text = $"{hero.Name}";
            var leveUp = ExcelToJSONConfigManager.First<CharacterLevelUpData>(t => t.Level == hero.Level + 1);
            //lb_exp.text = $"{hero.Exprices}/{leveUp?.NeedExprices ?? '-'}";
            float v = 0;
            if (leveUp != null) v = (float)hero.Exprices / leveUp.NeedExp;
            user_exp.fillAmount = v;
        }

        //private PlayerPackage Package;

        internal void ShowWindow(IBattleGate gate)
        {
            BattleGate = gate;

            ShowWindow();
        }

        private void ShowView()
        {
            SetPackage(BattleGate.Package);
            InitHero(BattleGate.Hero);
            foreach (var i in BattleGate.Package.Items)
            {
                var config = ExcelToJSONConfigManager.GetId<ItemData>(i.Value.ItemID);
                if ((ItemType)config.ItemType == ItemType.ItHpitem) _keyHp = config.Params1;
                if ((ItemType)config.ItemType == ItemType.ItMpitem) _keyMp = config.Params1;
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
            _playInput.Enable();
            GridTableManager.Count = 0;
            ShowView();
        }

        protected override void OnHide()
        {
            base.OnHide();
            _playInput.Disable();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var v = _playInput.Player.Move.ReadValue<Vector2>();
            if (v.magnitude > 0.001f)
            {
                if (_lastTime > Time.time) return;
                _lastTime = Time.time + .3f;
                var dir = ThirdPersonCameraContollor.Current.LookRotation * new Vector3(v.x, 0, v.y);
                BattleGate?.MoveDir(dir);
            }
            else
            {
                BattleGate?.MoveDir(Vector2.zero);
            }


            #region 快捷键

            //todo:: 新的input system 不好支持 
            /*  新的input system 不好支持
            var key = _playInput.KeyBoard.Keys.ReadValue<KeyCode>();

            switch (key)
            {
                case KeyCode.B:
                    BattleGate?.DoNormalAttack();
                    break;
                case KeyCode.Q:
                    UseHpItem();
                    break;
                case KeyCode.E:
                    UseMpItem();
                    break;
                case KeyCode.None : break;
                default:
                    for (var i = 0; i < _keyCodes.Length; i++)
                    {
                        if (GridTableManager.Count <= i) break;
                        if (_keyCodes[i] == key) GridTableManager[i].Model.ClickItem(null);
                    }
                    break;
            }*/

            #endregion

            var view = BattleGate?.Owner;
            if (!view) return;
            HPSilder.value = view!.HP / (float)view.HpMax;
            lb_hp.text = $"{view.HP}/{view.HpMax}";
            MpSilder.value = view.MP / (float)view.MpMax;
            lb_mp.text = $"{view.MP}/{view.MpMax}";
            hp_bg.color = Color.Lerp(Color.red, Color.green, Mathf.Max(0, HPSilder.value - 0.5f) * 2);

            foreach (var i in GridTableManager)
                i.Model.Update(view, BattleGate.TimeServerNow,
                    BattleGate.PreView.HaveOwnerKey(i.Model.MagicData.MagicKey));
            UpdateMap();
            if (view.TryGetMagicData(_normalAtt, out var att))
            {
                var time = Mathf.Max(0, att.CDCompletedTime - BattleGate.TimeServerNow);
                var cdTime = Mathf.Max(0.01f, att.CdTotalTime); // view.AttackSpeed 
                //if (cdTime < time) cdTime = time;
                if (cdTime > 0)
                    AttCdMask.fillAmount = time / cdTime;
                else
                    AttCdMask.fillAmount = 0;
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
            for (var y = 0; y < Size; y++)
                _colors[x + y * Size] = a;

            var lookRotation =
                Quaternion.Euler(0, 0, -ThirdPersonCameraContollor.Current.transform.rotation.eulerAngles.y);
            ViewForward.localRotation = lookRotation;

            var r = Size / 2f; // 16; 
            BattleGate.PreView.Each<UCharacterView>(t =>
            {
                var offset = t.transform.position - BattleGate.Owner.transform.position;
                if (offset.magnitude > r) return false;
                _colors[(int)(offset.x + r) + (int)(offset.z + r) * Size] =
                    t.TeamId == BattleGate.Owner.TeamId ? Color.green : Color.red;
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
                    i.Model.SetMagic(list[index], BattleGate, _keyCodes[index]);
                    i.Model.OnClick = OnRelease;
                    index++;
                }
            }

            if (view.TryGetMagicByType(MagicType.MtNormal, out var data))
            {
                var config = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.MagicID);
                att_Icon.sprite = await ResourcesManager.S.LoadIcon(config);
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
                    hp_item_Icon.sprite = await ResourcesManager.S.LoadIcon(config);
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

        public class GridTableModel : TableItemModel<GridTableTemplate>
        {
            private float _cdTime = 0.01f;
            private float _lastTime;
            private MagicData _lMagicData;
            private int _magicID = -1;
            public HeroMagicData Data;
            public CharacterMagicData MagicData;

            public Action<GridTableModel, Vector2?> OnClick;


            private SwipeButton Button { set; get; }

            public override void InitModel()
            {
                Button = Template.Button.GetComponent<SwipeButton>();
                Button.OnSwipeClickEvent.AddListener(ClickItem);
                Template.Forward.gameObject.SetActive(false);
                Button.OnSwipeStarted.AddListener(() => { Template.Forward.gameObject.SetActive(true); });

                Button.OnSwipeEnd.AddListener(() => { Template.Forward.gameObject.SetActive(false); });

                Button.OnDragging.AddListener(dir =>
                {
                    if (!dir.HasValue) return;
                    var lookV = new Vector3(dir.Value.x, 0, dir.Value.y);
                    var look = Quaternion.LookRotation(lookV);
                    //Debug.Log($"{dir} {look.eulerAngles.y}");
                    Template.Forward.transform.rotation = Quaternion.Euler(0, 0, -look.eulerAngles.y);
                });
            }

            public void ClickItem(Vector2? dir)
            {
                if (_lastTime + 0.3f > Time.time) return;
                _lastTime = Time.time;
                if (dir.HasValue)
                {
                    var lookV = new Vector3(dir.Value.x, 0, dir.Value.y);
                    var look = Quaternion.LookRotation(lookV);
                    //Debug.Log($"{dir} {look.eulerAngles.y}");
                }

                OnClick?.Invoke(this, dir);
            }

            public async void SetMagic(HeroMagicData data, IBattleGate battle, KeyCode key)
            {
                Data = data;
                if (_magicID == data.MagicID) return;
                _magicID = data.MagicID;
                MagicData = ExcelToJSONConfigManager.GetId<CharacterMagicData>(data.MagicID);
                var per = battle.PreView as IBattlePerception;
                _lMagicData = per.GetMagicByKey(MagicData.MagicKey);
                Template.Icon.sprite = await ResourcesManager.S.LoadIcon(MagicData);
                Template.tb_key.text = $"{key}";
            }

            public void Update(UCharacterView view, float now, bool haveKey)
            {
                if (_lMagicData == null) return;
                if (_lMagicData.unique) Button.interactable = !haveKey;
                else Button.interactable = true;

                if (!view.TryGetMagicData(_magicID, out var data)) return;
                var time = Mathf.Max(0, data.CDCompletedTime - now);
                Template.CDTime.text = time > 0 ? $"{time:0.0}" : string.Empty;
                _cdTime = Mathf.Max(0.01f, data.CdTotalTime);
                if (time > 0) _lastTime = Time.time;
                if (_cdTime > 0)
                    Template.ICdMask.fillAmount = time / _cdTime;
                else
                    Template.ICdMask.fillAmount = 0;
            }
        }
    }
}