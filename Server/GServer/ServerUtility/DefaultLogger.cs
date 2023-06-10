using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Confluent.Kafka;
using Grpc.Core.Logging;
using XNet.Libs.Utility;
using Console = Colorful.Console;

namespace ServerUtility
{
    /// <summary>
    /// Console 日志记录、默认
    /// </summary>
    public class DefaultLogger : Loger, ILogger, IDisposable
    {

        public string Topic { get; }

        public DefaultLogger(IList<string> kafka, string topic,string clientID)
        {
            var sb = new StringBuilder();
            sb.AppendJoin(',', kafka);

            var config = new ProducerConfig
            {
                BootstrapServers =  sb.ToString(),
                ClientId = clientID
            };
            Topic = topic;
            //Producer = new ProducerBuilder<Null, string>(config).Build();
        }

        private IProducer<Null, string> Producer { get; } = null!;

        public void Debug(string message)
        {
            Debuger.DebugLog(message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            Debuger.DebugLog(string.Format(format, formatArgs));
        }

        public void Dispose()
        {
            Producer?.Dispose();
        }

        public void Error(string message)
        {
            Debuger.LogError(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            Debuger.LogError(string.Format(format, formatArgs));
        }

        public void Error(Exception exception, string message)
        {
            Debuger.LogError(exception.ToString());
            Debuger.LogError(message);
        }

        public ILogger ForType<T>()
        {
            return this;
        }

        public void Info(string message)
        {
            Debuger.Log(message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            Debuger.Log(string.Format(format, formatArgs));
        }

        public void Warning(string message)
        {
            Debuger.LogWaring(message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            Debuger.LogWaring(string.Format(format, formatArgs));
        }

        public void Warning(Exception exception, string message)
        {
            Debuger.LogWaring(exception.ToString());
            Debuger.LogWaring(message);
        }

        public override void WriteLog(DebugerLog log)
        {
            var str = $"{Thread.CurrentThread.ManagedThreadId}->{log}";
            switch (log.Type)
            {
                case LogerType.Error:
                    Console.WriteLine(str,Color.Red );
                    break;
                case LogerType.Waring:
                case LogerType.Debug:
                    Console.WriteLine(str,Color.Yellow);
                    break;
                default:
                    Console.WriteLine(str);
                    break;
            } 

             

        }
    }
}

