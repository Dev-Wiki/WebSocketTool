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

namespace WebSocketTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static readonly ILog Log = LogManager.GetLogger(nameof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
            Directory.CreateDirectory("log");
        }

        public static void RunOnUIThread(Action action)
        {
            if (Current == null)
            {
                Log.Info("application current is null");
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
