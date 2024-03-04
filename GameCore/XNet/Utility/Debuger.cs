﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// ReSharper disable IdentifierTypo
// ReSharper disable ClassNeverInstantiated.Global

namespace XNet.Libs.Utility
{
    /// <summary>
    /// author:xxp
    /// </summary>
    public class Debuger
    {
        public static void Log(object msg)
        {
            DoLog(LogerType.Log, msg.ToString());
        }

        public static void LogWaring(object msg)
        {
            DoLog(LogerType.Waring, msg.ToString());
        }

        public static void LogError(object msg)
        {
            DoLog(LogerType.Error, msg.ToString());
        }

        public static void DebugLog(string msg)
        { 
            DoLog(LogerType.Debug, msg);
        }

        private static void DoLog(LogerType type, string msg)
        {
            var log = new DebugerLog()
            {
                LogTime = DateTime.Now,
                Message = msg,
                Type = type
            };
            Loger.WriteLog(log);
        }

        static Debuger()
        {
           //Loger = new DefaultLoger();
        }

        public static Loger Loger { set; get; }
    }
    /// <summary>
    /// 日志记录者
    /// </summary>
    public abstract class Loger
    {
        public abstract void WriteLog(DebugerLog log);
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogerType
    {
        Log,
        Waring,
        Error,
        Debug
    }
    /// <summary>
    /// 日志
    /// </summary>
    public class DebugerLog
    {
        public LogerType Type { set; get; }
        public string Message { set; get; }
        public DateTime LogTime { set; get; }
        public override string ToString()
        {
            return $"[{Type}][{LogTime}]:{Message}";
        }
    }


}
