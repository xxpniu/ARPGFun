
    using CommandLine;


    public class CommandOption
    {
        [Option("host", Default = "127.0.0.1:1900", Required = true)]
        public string ServiceAddress { set; get; }

        [Option("listen", Default = "127.0.0.1:2301", Required = true)]
        public string ListenAddress { set; get; }

        [Option("id", Default = "battle1", Required = true)]
        public string Id { set; get; }

        [Option("zkroot", Default = "/battle", Required = true)]
        public string ZkRoot { set; get; }

        [Option("zklogin", Default = "/login", Required = true)]
        public string ZkLogin { set; get; }

        [Option("exconfig", Default = "/configs", Required = true)]
        public string ZkExConfig { set; get; }

        [Option("maxplayer", Default = "100", Required = true)]
        public string MaxPlayer { set; get; }

        [Option("zkmatch", Default = "/match", Required = true)]
        public string ZkMatch { set; get; }

        [Option("zk", Default = "127.0.0.1:2181", Required = true)]
        public string Zk { set; get; }

        [Option("kafka", Default = "127.0.0.1:9092", Required = true)]
        public string kafka { set; get; }
        
        [Option("map", Default = "1", Required = true)]
        public string MapId { set; get; }
        //KafkaServer
    }
    
