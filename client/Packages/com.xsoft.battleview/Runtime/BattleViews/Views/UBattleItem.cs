using App.Core.Core;
using App.Core.UICore.Utility;
using BattleViews.Utility;
using EConfig;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace BattleViews.Views
{
    public class UBattleItem : UElementView, IBattleItem
    {
        public PlayerItem Item { private set; get; }
        public int TeamIndex { private set; get; }
        public int GroupIndex { private set; get; }

        int IBattleItem.TeamIndex => TeamIndex;

        int IBattleItem.GroupIndex => GroupIndex;

        public ItemData Config;

        private async void Start()
        {
#if !UNITY_SERVER
            var go = await  ResourcesManager.S.LoadModel(Config);
            Instantiate(go, transform).transform.RestRTS();
#endif
        }

        private void Awake()
        {
            var box = this.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = Vector3.one * 1;
            box.center = Vector3.one * .5f;

        }

        private void OnTriggerEnter(Collider other)
        {
            var ch = other.GetComponent<UCharacterView>();
            if (ch) ch.OnItemTrigger?.Invoke(this);
        }

        public override IMessage ToInitNotify()
        {
            return new Notify_Drop
            {
                Index = Index,
                GroupIndex = GroupIndex,
                Item = Item,
                Pos = this.transform.position.ToPVer3(),
                TeamIndex = TeamIndex
            };
        }

        internal void SetInfo(PlayerItem item, int teamIndex, int groupId)
        {
            this.GroupIndex = groupId;
            this.TeamIndex = teamIndex;
            this.Item = item;
            Config = ExcelConfig.ExcelToJSONConfigManager.GetId<ItemData>(item.ItemID);
        }

        void IBattleItem.ChangeGroupIndex(int groupIndex)
        {
#if UNITY_SERVER || UNITY_EDITOR
            CreateNotify(new Notify_BattleItemChangeGroupIndex { GroupIndex = groupIndex, Index = Index });
#endif
            GroupIndex = groupIndex;
        }

        public bool IsOwner(int index)
        {
            return GroupIndex == index || GroupIndex < 0;
        }
    }
}
