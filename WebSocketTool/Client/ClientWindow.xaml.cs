using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using log4net;
using WebSocketTool.View.Dialog;

namespace WebSocketTool.Client
{
    /// <summary>
    /// ClientWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientWindow : Window, IClientView
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(ClientWindow));
        private readonly ClientViewModel viewModel;
        public ClientWindow()
        {
            InitializeComponent();
            viewModel = new ClientViewModel(this);
            DataContext = viewModel;
        }

        public void ShowToast(string msg)
        {
            var toast = Toast.CreateToast(this, msg, ToastLocation.Center);
            toast.Show();
        }

        public void AppendInfo(string info)
        {
            Log.Info($"AppendInfo:{info}");
            InfoTb.Text += $"\n{info}";
            InfoTb.ScrollToEnd();
        }

        private void ConnectBtn_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.Connect();
        }

        private void DisconnectBtn_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }
        private void SendContentBtn_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.Send();
        }

        private void SendPingBtn_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void SendPingBtn_OnChecked(object sender, RoutedEventArgs e)
        {
            viewModel.StartPing();
        }

        private void SendPingBtn_OnUnchecked(object sender, RoutedEventArgs e)
        {
            viewModel.StopPing();
        }

        private void ClientWindow_OnClosed(object sender, EventArgs e)
        {
            viewModel.Close();
        }
    }

    public interface IClientView
    {
        void ShowToast(string msg);
        void AppendInfo(string info);
    }
}
