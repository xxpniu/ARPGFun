using System;
using System.Linq;
using App.Core.Core;
using App.Core.UICore.Utility;
using Cysharp.Threading.Tasks;
using Proto;
using UApp;
using UApp.GameGates;

namespace Windows
{
    internal partial class UUIUserInvite
    {
        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(HideWindow);

            //Write Code here
        }

        protected override void OnShow()
        {
            base.OnShow();

            var users = ChatManager.S.Friends
                .Values.Where(t => t.State == PlayerState.Types.StateType.Online).ToArray();
            ContentTableManager.Count = users.Length;
            var index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetPlayer(users[index]);
                i.Model.OnClickInvite = InviteFriend;
                index++;
            }
        }

        private static async void InviteFriend(ContentTableModel obj)
        {
            var gate = UApplication.G<GMainGate>();
            if (!gate) return;
            var group = gate.Group;
            if (group == null) return;
            obj.Invited();
            var res = await GateManager.S.MatchServiceClient.InviteJoinMatchAsync(new C2G_InviteJoinMatch
            {
                AccountUuid = obj.Player.User.Uuid,
                GroupID = group.Id,
                LevelID = group.LevelID
            });
            await UniTask.SwitchToMainThread();
            if (!res.Code.IsOk()) UApplication.S.ShowError(res.Code);
        }

        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public Action<ContentTableModel> OnClickInvite { get; set; }
            public PlayerState Player { get; private set; }

            public override void InitModel()
            {
                Template.InviteBlue.onClick.AddListener(() => { OnClickInvite?.Invoke(this); });
            }

            internal void SetPlayer(PlayerState playerState)
            {
                Template.InviteBlue.ActiveSelfObject(true);
                Player = playerState;
                Template.TextName.text = playerState.User.UserName;
                Template.TextLvScore.text = "lvl:0";
            }

            internal void Invited()
            {
                Template.InviteBlue.ActiveSelfObject(false);
            }
        }
    }
}