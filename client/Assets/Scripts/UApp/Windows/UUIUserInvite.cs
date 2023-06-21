using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using System.Threading.Tasks;
using App.Core.Core;
using App.Core.UICore.Utility;
using UApp;
using UApp.GameGates;

namespace Windows
{
    partial class UUIUserInvite
    {
        
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}

            public Action<ContentTableModel> OnClickInvite { get;  set; }
            public PlayerState Player { get; private set; }

            public override void InitModel()
            {
                Template.InviteBlue.onClick.AddListener(() => { OnClickInvite?.Invoke(this); });
            }

            internal void SetPlayer(PlayerState playerState)
            {
                Template.InviteBlue.ActiveSelfObject(true);
                this.Player = playerState;
                this.Template.TextName.text = playerState.User.UserName;
                this.Template.TextLvScore.text = $"lvl:0";
            }

            internal void Invited()
            {
                Template.InviteBlue.ActiveSelfObject(false);
            }
        }

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
                .Values.Where(t=>t.State== PlayerState.Types.StateType.Online).ToArray();
            ContentTableManager.Count = users.Length;
            int index = 0;
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
            var res = await GateManager.S.GateFunction.InviteJoinMatchAsync(new C2G_InviteJoinMatch
            {
                AccountUuid = obj.Player.User.Uuid,
                GroupID = group.Id,
                LevelID = group.LevelID
            });
            if (!res.Code.IsOk())
            {
                UApplication.S.ShowError(res.Code);
            }
        }
        
    }
}