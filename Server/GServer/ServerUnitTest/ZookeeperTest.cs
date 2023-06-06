using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static org.apache.zookeeper.ZooDefs;

namespace ServerUnitTest
{
   public class ZookeeperTest:Watcher
    {
        public async override Task process(WatchedEvent @event)
        {
            output.WriteLine($"Process: {@event}");
            await Task.FromResult(@event);

        }

        public ZookeeperTest(ITestOutputHelper oupt)
        {
            this.output = oupt;
        }

        readonly ITestOutputHelper output;


        [Theory]
        //[InlineData("admin", "123456")]
        [InlineData("129.211.9.75:2181", "/configs")]
        [InlineData("129.211.9.75:2181", "/zookeeper")]
        public async Task ZookeeperClient(string connectStr, string path)
        {
            ZooKeeper zoo = new ZooKeeper(connectStr, 2000, this);
            var r = await zoo.getChildrenAsync(path);
            foreach (var p in r.Children)
            {
                var d = await zoo.getDataAsync($"{path}/{p}");
                output.WriteLine($"{path}/{p}-> {Encoding.UTF8.GetString(d.Data)}");
            }
            Assert.True(r.Children.Count > 0);
        }

        [Theory]
        [InlineData("129.211.9.75:2181", "/test", "1112")]
        public async Task ZookeeperSetData(string connectStr, string path, string data)
        {
            ZooKeeper zoo = new ZooKeeper(connectStr, 2000, null);
            var x = await zoo.existsAsync(path);
            if (x != null) await zoo.deleteAsync(path);

            await zoo.createAsync(path, Encoding.UTF8.GetBytes(data), Ids.OPEN_ACL_UNSAFE,CreateMode.PERSISTENT);
            var res = await zoo.getDataAsync(path);
            var str = Encoding.UTF8.GetString(res.Data);
            Assert.Equal(str, data);
            output.WriteLine($"{path}->{str}");
        }
    }
}
