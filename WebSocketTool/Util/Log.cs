using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using log4net;

namespace WebSocketTool.Util
{
    public class LogManager
    {
        public static readonly Lazy<LogManager> Lazy = new Lazy<LogManager>(() => new LogManager());

        private readonly Dictionary<string, Log> logs = new Dictionary<string, Log>();
        public static string LogName { get; private set; } = nameof(App);
        private LogManager() { }

        public static LogManager GetManager()
        {
            return Lazy.Value;
        }

        public void Init(string logName)
        {
            if (!string.IsNullOrEmpty(logName))
            {
                LogName = logName;
            }
        }

        public Log GetLog(string tag)
        {
            if (logs.ContainsKey(tag))
            {
                return logs[tag];
            }
            else
            {
                var log = new Log(tag);
                logs[tag] = log;
                return log;
            }
        }
    }
    public class Log
    {
        private readonly ILog log;
        public Log(string tag)
        {
            log = log4net.LogManager.GetLogger($"[{LogManager.LogName}]:[{tag}]");
        }

        public void Debug(string msg)
        {
            log.Debug(msg);
        }

        public void Info(string msg)
        {
            log.Info(msg);
        }

        public void Warn(string msg)
        {
            log.Warn(msg);
        }

        public void Error(string msg, Exception e)
        {
            log.Error(msg, e);
        }

        public void Error(Exception e)
        {
            log.Error(e);
        }

        public void Fatal(string msg, Exception e = null)
        {
            log.Fatal(msg, e);
        }

        public void Fatal(Exception e)
        {
            log.Fatal(e);
        }

    }
}