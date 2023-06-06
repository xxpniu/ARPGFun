using System;
using Google.Protobuf;
using System.Collections.Concurrent;
using org.apache.zookeeper;
using System.Threading.Tasks;
using System.Text;
using XNet.Libs.Utility;
using System.Collections.Generic;
using System.Collections;

namespace Utility
{
    public class DefaultWatcher : Watcher
    {
        public override Task process(WatchedEvent @event)
        {
            return Task.CompletedTask;
        }
    }

    public class WatcherServer<Key, TServer> : IEnumerable<TServer>
        where TServer : IMessage, new()
    {
        public class ZKWatcher : Watcher
        {

            public ZKWatcher(WatcherServer<Key, TServer> server)
            {
                this.WatherServer = server;
            }

            private readonly WatcherServer<Key, TServer> WatherServer;

            public async override Task process(WatchedEvent @event)
            {
                if (@event == null) return;
                switch (@event.get_Type())
                {
                    case Event.EventType.NodeChildrenChanged:
                    case Event.EventType.NodeCreated:
                    case Event.EventType.NodeDeleted:
                        await WatherServer.RefreshData();
                        break;
                }
            }
        }

        private readonly Func<TServer, Key> KeyHandler;
        private readonly ZKWatcher watcher;

        public ZooKeeper ZK { get; }
        public string Root { get; }

        public WatcherServer(ZooKeeper zk, string root, Func<TServer, Key> handler)
        {
            watcher = new ZKWatcher(this);
            this.ZK = zk;
            this.Root = root;
            this.KeyHandler = handler;
        }

        readonly ConcurrentDictionary<Key, TServer> ServerConfigs = new ConcurrentDictionary<Key, TServer>();

        private async Task<bool> LoadServerAsync(string path)
        {
            var node = await ZK.getDataAsync(path).ConfigureAwait(false);
            var json = Encoding.UTF8.GetString(node.Data);
            var server = json.TryParseMessage<TServer>();
            var key = KeyHandler(server);
            ServerConfigs.AddOrUpdate(key, server, (k, v) => server);
            return true;
        }

        public TServer Find(Key key)
        {
            if (ServerConfigs.TryGetValue(key, out TServer server))
            {
                return server;
            }
            return default;
        }

        public async Task ForEach(Func<TServer, Task<bool>> each)
        {
            foreach (var i in this.ServerConfigs)
            {
                if (await each(i.Value)) break;
            }
        }

        public async Task<WatcherServer<Key, TServer>> RefreshData()
        {
            var childs = await ZK.getChildrenAsync(Root, watcher);
            foreach (var i in childs.Children)
            {
                var path = $"{Root}/{i}";
                Debuger.Log($"Load:{path}");
                await LoadServerAsync(path);
            }

            OnRefreshed?.Invoke();
            return this;
        }

        public IEnumerator<TServer> GetEnumerator()
        {
            return this.ServerConfigs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Action OnRefreshed;
    }
}
