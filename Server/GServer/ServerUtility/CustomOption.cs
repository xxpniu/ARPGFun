using CommandLine;

namespace ServerUtility
{
    public class CustomOption
    {
        [Option('l', "listen", Required = false)]
        public string ListenHost { set; get; }

        [Option('c', "config", Required = false)]
        public string Config { set; get; }

        [Option('s', "service", Required = false)]
        public string ServiceHost { set; get; }

        [Option('d', "dbhost", Required = false)]
        public string DBHost { set; get; }

        [Option('n', "dbname", Required = false)]
        public string DBName { set; get; }

        [Option('z', "zk", Required = false)]
        public string ZK { set; get; }
        
        [Option('k', "kf", Required = false)]
        public string Kafka { set; get; }
    }
}