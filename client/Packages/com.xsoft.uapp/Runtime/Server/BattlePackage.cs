using System.Collections.Generic;
using Proto;

namespace Server
{
    public class BattlePlayerItem
    {
        public BattlePlayerItem(PlayerItem item, bool dirty = false)
        {
            Dirty = dirty;
            Item = item;
        }

        public bool Dirty { private set; get; }
        public PlayerItem Item { private set; get; }

        public void SetDirty()
        {
            Dirty = true;
        }
    }

    public class BattlePackage
    {
        public BattlePackage(PlayerPackage package)
        {
            Package = package;
            Items = new Dictionary<string, BattlePlayerItem>();
            foreach (var i in package.Items) Items.Add(i.Key, new BattlePlayerItem(i.Value));
        }

        public PlayerPackage Package { get; }
        public Dictionary<string, BattlePlayerItem> Items { get; }
        public int MaxSize => Package.MaxSize;

        public List<BattlePlayerItem> Removes { get; } = new();

        internal bool RemoveItem(string key)
        {
            if (!Items.Remove(key, out var item)) return false;
            Removes.Add(item);
            return true;
        }
    }
}