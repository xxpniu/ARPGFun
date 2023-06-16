using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using System.Threading.Tasks;
using App.Core.Core;
using Proto;
using UApp;
using UApp.GameGates;
using XNet.Libs.Utility;

namespace Windows
{
    partial class UUIUserList
    {
        public IList<G2C_SearchPlayer.Types.Player> Players { get; private set; }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public G2C_SearchPlayer.Types.Player Player { private set; get; }
            public Action<ContentTableModel> OnAddClick { get;  set; }

            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.AddBlue.onClick.AddListener(() =>
                {
                    this.OnAddClick?.Invoke(this);
                });
            }

            internal void SetPlayer(G2C_SearchPlayer.Types.Player player)
            {
                Template.TextName.text = player.HeroName;
                Template.TextLvScore.text = $"Lvl:{player.Level}";
                this.Player = player;
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(() => { HideWindow(); });
        }
        protected override void OnShow()
        {
            base.OnShow();

            var g = UApplication.G<GMainGate>();
            if (!g) { HideWindow(); return; }

            Task.Factory.StartNew(async () => {
                var res = await g.GateFunction.SearchPlayerAsync(new Proto.C2G_SearchPlayer());
                if (!res.Code.IsOk()) return;
                Invoke(() => {
                    this.Players = res.Players.Where(t=>t.AccountUuid!=UApplication.S.AccountUuid).ToList();
                    InitData();
                });
            });

        }

        private void InitData()
        {
            this.ContentTableManager.Count = Players.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetPlayer(Players[index]);
                i.Model.OnAddClick = ClickItem;
                index++;
            }
        }

        private async void ClickItem(ContentTableModel obj)
        {

            var r = await ChatManager.S.ChatClient.LinkFriendAsync(new C2CH_LinkFriend
                { FriendId = obj.Player.AccountUuid });
            Invoke(() =>
            {
                if (!r.Code.IsOk())
                    UApplication.S.ShowError(r.Code);
                Debuger.Log($"{r.Code} {obj.Player.HeroName}");
            });


        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}