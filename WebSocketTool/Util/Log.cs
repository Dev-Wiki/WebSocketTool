using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

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
            InitLog4Net();
        }

        private void InitLog4Net()
        {
            Hierarchy hierarchy = (Hierarchy)log4net.LogManager.GetRepository();
            var appender = CreateReleaseAppender();
            appender.ActivateOptions();
            if (hierarchy != null)
            {
                hierarchy.Root.Level = Level.Info;
                log4net.Config.BasicConfigurator.Configure(hierarchy, appender);
            }
            else
            {
                log4net.Config.BasicConfigurator.Configure(appender);
            }
        }

        private static RollingFileAppender CreateReleaseAppender()
        {
            var appender = new RollingFileAppender
            {
                Name = "ReleaseFileLog",
                File = Path.Combine("log", "WebSocketTool.log"),
                AppendToFile = true,
                MaxSizeRollBackups = 10,
                MaximumFileSize = "10MB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true,
                Encoding = Encoding.UTF8,
                Layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level %logger - %message%newline")
            };
            return appender;
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

        public void Error(string msg, Exception e = null)
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