﻿using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    public class BattlePlayerItem
    {
        public bool Dirty { private set; get; } = false;
        public PlayerItem Item { private set; get; }

        public void SetDrity()
        {
            Dirty = true;
        }

        public BattlePlayerItem(PlayerItem item, bool dirty = false)
        {
            this.Dirty = dirty;
            Item = item;
        }
    }

    public class BattlePackage
    {
        public BattlePackage(PlayerPackage package)
        {
            this.Package = package;
            Items = new Dictionary<string, BattlePlayerItem>();
            foreach (var i in package.Items)
            {
                Items.Add(i.Key, new BattlePlayerItem ( i.Value ));
            }
        }

        public PlayerPackage Package { get; }
        public Dictionary<string, BattlePlayerItem> Items { private set; get; }
        public int MaxSize { get { return Package.MaxSize; } }

        public List<BattlePlayerItem> Removes { private set; get; } = new List<BattlePlayerItem>();

        internal bool RemoveItem(string key)
        {
            if (Items.TryGetValue(key, out BattlePlayerItem item))
            {
                Items.Remove(key);
                Removes.Add(item);
                return true;
            }
            return false;

        }
    }
}
