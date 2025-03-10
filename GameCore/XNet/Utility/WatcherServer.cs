﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using org.apache.zookeeper;
using Utility;

namespace XNet.Libs.Utility
{
    public class DefaultWatcher : Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            return Task.CompletedTask;
        }
    }

    public class WatcherServer<TKey, TServer> : IEnumerable<TServer>
        where TServer : IMessage, new()
    {
        private int _nextIndex = 0;
        public delegate void WatchChanged(TServer[] old, TServer[] newList);
        private class ZkWatcher : Watcher
        {

            public ZkWatcher(WatcherServer<TKey, TServer> server)
            {
                _watcherServer = server;
            }

            private readonly WatcherServer<TKey, TServer> _watcherServer;

            public override async Task process(WatchedEvent @event)
            {
                if (@event == null) return;
                switch (@event.get_Type())
                {
                    case Event.EventType.NodeChildrenChanged:
                    case Event.EventType.NodeCreated:
                    case Event.EventType.NodeDeleted:
                    case Event.EventType.NodeDataChanged:
                        await _watcherServer.RefreshData();
                        break;
                    case Event.EventType.None:
                    default:
                        break;
                }
            }
        }

        private readonly Func<TServer, TKey> _keyHandler;
        private readonly ZkWatcher _watcher;

        private ZooKeeper Zk { get; }
        private string Root { get; }

        public WatcherServer(ZooKeeper zk, string root, Func<TServer, TKey> handler, bool autoGen = true)
        {
            AutoGen = autoGen;
            _watcher = new ZkWatcher(this);
            Zk = zk;
            Root = root;
            _keyHandler = handler;
        }

        private bool AutoGen { get; }

        private readonly ConcurrentDictionary<TKey, TServer> _serverConfigs = new ConcurrentDictionary<TKey, TServer>();

        private async Task<bool> LoadServerAsync(string path)
        {
            var node = await Zk.getDataAsync(path);
            var json = Encoding.UTF8.GetString(node.Data);
            var server = json.TryParseMessage<TServer>();
            var key = _keyHandler(server);
            _serverConfigs.AddOrUpdate(key, server, (k, v) => server);
            return true;
        }

        public TServer Find(TKey key)
        {
            return _serverConfigs.TryGetValue(key, out var server) ? server : default;
        }

        public async Task ForEach(Func<TServer, Task<bool>> each)
        {
            foreach (var i in this._serverConfigs)
            {
                if (await each(i.Value)) break;
            }
        }

        public async Task<WatcherServer<TKey, TServer>> RefreshData()
        {
            Debuger.Log($"Watcher Refresh Data:{Root} ");

            var s = await Zk.existsAsync(Root);
            if (s == null)
            {
                if (AutoGen)
                {
                    await Zk.createAsync(Root, new byte[] {0}, ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.PERSISTENT);
                }
                else
                {
                    Debuger.LogError($"Not found :{Root}");
                    return this;
                }
            }

            var old = _serverConfigs.Values.ToArray();

            var children = await Zk.getChildrenAsync(Root, _watcher);
            foreach (var i in children.Children)
            {
                var path = $"{Root}/{i}";
                Debuger.Log($"Load:{path}");
                await LoadServerAsync(path);
            }

            OnRefreshed?.Invoke();
            var newList = _serverConfigs.Values.ToArray();
            
            OnChanged?.Invoke(old,newList);
            return this;
        }

        public IEnumerator<TServer> GetEnumerator()
        {
            return _serverConfigs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TServer NextServer()
        {
            if (_nextIndex < 0) _nextIndex = 0;
            if (_nextIndex >= _serverConfigs.Count)
            {
                _nextIndex = 0;
            }

            TServer server = default;
            var start = 0;
            foreach (var kv in _serverConfigs)
            {
                if (start == _nextIndex)
                {
                    server = kv.Value;
                    break;
                }

                start++;
            }

            _nextIndex++;

            return server;
        }

        public Action OnRefreshed;
        public WatchChanged OnChanged;
    }
}
