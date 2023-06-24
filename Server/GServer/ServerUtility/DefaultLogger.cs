using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Confluent.Kafka;
using Grpc.Core.Logging;
using XNet.Libs.Utility;

namespace ServerUtility
{
    /// <summary>
    /// Console 日志记录、默认
    /// </summary>
    public class DefaultLogger : Loger, ILogger, IDisposable
    {

        internal static string GetForegroundColorEscapeCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return "\x1B[30m";
                case ConsoleColor.DarkRed:
                    return "\x1B[31m";
                case ConsoleColor.DarkGreen:
                    return "\x1B[32m";
                case ConsoleColor.DarkYellow:
                    return "\x1B[33m";
                case ConsoleColor.DarkBlue:
                    return "\x1B[34m";
                case ConsoleColor.DarkMagenta:
                    return "\x1B[35m";
                case ConsoleColor.DarkCyan:
                    return "\x1B[36m";
                case ConsoleColor.Gray:
                    return "\x1B[37m";
                case ConsoleColor.Red:
                    return "\x1B[1m\x1B[31m";
                case ConsoleColor.Green:
                    return "\x1B[1m\x1B[32m";
                case ConsoleColor.Yellow:
                    return "\x1B[1m\x1B[33m";
                case ConsoleColor.Blue:
                    return "\x1B[1m\x1B[34m";
                case ConsoleColor.Magenta:
                    return "\x1B[1m\x1B[35m";
                case ConsoleColor.Cyan:
                    return "\x1B[1m\x1B[36m";
                case ConsoleColor.White:
                default:
                    return "\x1B[1m\x1B[37m";

            }
            // default foreground color
        }
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
                {
                    var text = $"GetForegroundColorEscapeCode(ConsoleColor.Red){str}";
                    Console.WriteLine(text);
                }
                    break;
                case LogerType.Waring:
                case LogerType.Debug:
                {
                    var text = $"{GetForegroundColorEscapeCode(ConsoleColor.Yellow)}{str}";
                    Console.WriteLine(text);
                } break;
                default:
                {
                    var text = $"{GetForegroundColorEscapeCode(ConsoleColor.White)}{str}";
                    Console.WriteLine(text);
                }
                    break;
            }



        }
    }
}

