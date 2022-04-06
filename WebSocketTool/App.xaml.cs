using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using log4net.Config;
using WebSocketTool.Util;
using LogManager = WebSocketTool.Util.LogManager;

namespace WebSocketTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static Log log;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Directory.CreateDirectory("log");
            LogManager.GetManager().Init("WebSocketTool");
            log = LogManager.GetManager().GetLog(nameof(App));
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error($"app crash:{e.ExceptionObject}");
        }

        public static void RunOnUIThread(Action action)
        {
            if (Current == null)
            {
                log.Info("application current is null");
                return;
            }
            if (Current.CheckAccess())
            {
                action?.Invoke();
            }
            else
            {
                Current.Dispatcher.BeginInvoke(action);
            }
        }
    }
}
