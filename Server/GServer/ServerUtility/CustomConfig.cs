using CommandLine;

namespace ServerUtility
{
    public class CustomOption
    {
        /// <summary>
        /// 服务器监听端口
        /// </summary>
        [Option('l', "listen", Required = false)]
        public string ListenHost { set; get; } = "1000";

        /// <summary>
        /// 服务器的zookeeper 查询路径
        /// </summary>
        [Option('c', "config", Required = false)]
        public string Config { set; get; } = "server";

        /// <summary>
        /// 服务器暴露的ip 提供给其他服务器用
        /// </summary>
        [Option('s', "service", Required = false)]
        public string ServiceHost { set; get; } = "192.168.1.1";

        /// <summary>
        /// 数据库链接的地址  mongo database 
        /// </summary>
        [Option('d', "dburl", Required = false)]
        public string DataBaseUrl { set; get; }
        
        [Option('z', "zookeeper", Required = false)]
        public string ZooKeeper { set; get; } 
        
        [Option('k', "kf", Required = false)]
        public string Kafka { set; get; }
    }
}