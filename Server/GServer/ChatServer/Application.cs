using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using org.apache.zookeeper;
using Proto.ServerConfig;
using ServerUtility;
using Utility;
using static org.apache.zookeeper.ZooDefs;
using XNet.Libs.Utility;
using Proto;
using System.Collections.Concurrent;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using ChatTool;
using ChatServerService = ChatServer.Services.ChatServerService;
using ChatService = ChatServer.Services.ChatService;

namespace ChatServer
{
    public class Application : ServerApp<Application>
    {
        private readonly ConcurrentDictionary<int, ChatServer>
            ChatServers = new ConcurrentDictionary<int, ChatServer>();

        private class ChatWatcher : Watcher
        {
            public override async Task process(WatchedEvent @event)
            {
                Debuger.Log($"{@event}");
                var path = @event?.getPath();
                if (path == null) return;
                if (path.StartsWith(S.Config.ChatServerRoot))
                {

                    switch (@event.get_Type())
                    {
                        case Event.EventType.NodeCreated:
                            await S.AddChat(path);
                            break;
                        case Event.EventType.NodeDeleted:
                            await S.RemoveChat(path);
                            break;
                    }
                }
            }
        }

        private async Task AddChat(string path)
        {
            var res = await Zk.getDataAsync(path);
            if (res != null)
            {
                var json = Encoding.UTF8.GetString(res.Data);
                var config = json.TryParseMessage<ChatServerConfig>();
                Debuger.Log($"Load Chat server:{config}");
                if (config.ChatServerID == Config.ChatServerID) return; //self
                await RemoveChannel(config.ChatServerID);
                var add = $"{config.ServicsHost.IpAddress}:{config.ServicsHost.Port}";
                Debuger.Log($"Listen:{add}");
                var logChannel = new LogChannel(add, ChannelCredentials.Insecure);
                var ser = new ChatServer(logChannel);
                ChatServers.TryAdd(config.ChatServerID, ser);
            }
        }
        
        private async Task RemoveChat(string path)
        {
            var id = path.Remove(0, Config.ChatServerRoot.Length);
            if (int.TryParse(id, out var serverID))
            {
                await RemoveChannel(serverID);
            }
        }

        private async Task RemoveChannel(int serverID)
        {
            if (ChatServers.TryRemove(serverID, out ChatServer ser))
            {
                await ser.Channel.ShutdownAsync();
            }
        }

        //event
        private async Task RefreshChatServer(Watcher w)
        {
            var childs = await Zk.getChildrenAsync(Config.ChatServerRoot, w);
            foreach (var c in childs.Children)
            {
                await AddChat($"{Config.ChatServerRoot}/{c}");
            }
        }

        private readonly struct ChatServer
        {
            public readonly LogChannel Channel;

            public Proto.ChatServerService.ChatServerServiceClient Call { get; }

            public  ChatServer(LogChannel channel)
            {
                this.Channel = channel;
                Call =  channel.CreateClient<Proto.ChatServerService.ChatServerServiceClient>();
            }
        }

        public ChatServerConfig Config { get; private set; }
        private LogServer ListenServer { get; set; }
        private LogServer ServiceServer { get; set; }
        private ZooKeeper Zk { get; set; }

        public WatcherServer<string, LoginServerConfig> LoginServers { private set; get; }

        //广播状态改变
        internal async Task NotifyStateForAllFriends(PlayerState state)
        {
            var friends = await DataBase.S.QueryNotifyFriend(state.User.Uuid);
            foreach (var i in friends)
            {
                if (i.State == PlayerState.Types.StateType.Offline) continue;
                if (i.User.ChatServerId == Config.ChatServerID)
                {
                    //本服务器用户
                    await ChatService.NotifyFriendStateChange(i.User.Uuid, Any.Pack(state));
                }
                else if (ChatServers.TryGetValue(i.User.ChatServerId, out ChatServer ser))
                {
                    var msg = new NotifyMsg
                    {
                        AccountID = i.User.Uuid,
                        AnyNotify = {Any.Pack(state)}
                    };
                    await ser.Call.CreateNotifyAsync(msg);
                }
            }

            await Task.CompletedTask;
        }

        public async Task RouteChat(Chat msg)
        {
            if (msg.Receiver.ChatServerId == Config.ChatServerID)
            {
                await ChatService.NotifyFriendStateChange(msg.Receiver.Uuid, Any.Pack(msg));
            }
            else if (ChatServers.TryGetValue(msg.Receiver.ChatServerId, out ChatServer ser))
            {
                await ser.Call.ChatRouteAsync(msg);
            }
        }

        public Application Create(ChatServerConfig config)
        {
            Config = config;
            return this;
        }

        protected override async Task Start(CancellationToken token)
        {
            DataBase.S.Init(Config.DBHost, Config.DBName);
            ListenServer = new LogServer
            {
                Ports = {new ServerPort("0.0.0.0", Config.ListenHost.Port, ServerCredentials.Insecure)}
            };

            ChatService = new ChatService(ListenServer);
            var chat = Proto.ChatService.BindService(ChatService);
            ListenServer.BindServices(chat);

            ListenServer.Interceptor.SetAuthCheck((c) =>
            {
                if (!c.GetHeader("session-key", out var value)) return false;
                if (!ListenServer.CheckSession(value, out var userid)) return false;
                c.RequestHeaders.Add("user-key", userid);
                return true;
            });

            ListenServer.Start();

            Debuger.Log("ListenServer Start!");

            ServiceServer = new LogServer
            {
                Ports = {new ServerPort("0.0.0.0", Config.ServicsHost.Port, ServerCredentials.Insecure)}
            }.BindServices(Proto.ChatServerService.BindService(new ChatServerService()));
            ServiceServer.Start();


            Debuger.Log("Services Start!");

            Zk = new ZooKeeper(Config.ZkServer[0], 3000, new DefaultWatcher());

            if (await Zk.existsAsync(Config.ChatServerRoot) == null)
            {
                await Zk.createAsync(Config.ChatServerRoot, new byte[] {0},
                    Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                Debuger.Log($"create:{Config.ChatServerRoot}");
            }

            var root = $"{Config.ChatServerRoot}/{Config.ChatServerID}";

            Debuger.Log($"Try create:{root}");
            if (await Zk.existsAsync(root) != null)
            {
                Debuger.Log("Reg Chat server failure it exist");
                return;
            }

            await Zk.createAsync(root, Encoding.UTF8.GetBytes(Config.ToJson()), Ids.OPEN_ACL_UNSAFE,
                CreateMode.EPHEMERAL);

            LoginServers = await new WatcherServer<string, LoginServerConfig>(Zk, 
                    Config.LoginServerRoot,
                    (c) => $"{c.ServicsHost.IpAddress}:{c.ServicsHost.Port}")
                .RefreshData();

            await RefreshChatServer(new ChatWatcher());


        }

        public ChatService ChatService { get; private set; }

        protected override async Task Stop(CancellationToken token)
        {
            await Zk.closeAsync();
            foreach (var i in ChatServers) await i.Value.Channel.ShutdownAsync();
            await ListenServer.ShutdownAsync();
            await ServiceServer.ShutdownAsync();
            await GrpcEnvironment.ShutdownChannelsAsync();

        }
    }
}
