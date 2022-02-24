using System;
using System.Windows.Threading;
using log4net;
using WebSocketSharp;
using WebSocketTool.Base;

namespace WebSocketTool.Client
{
    public class ClientViewModel : BaseViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(ClientViewModel));
        private IClientView view;
        private SocketClient mClient;

        public ClientViewModel(IClientView view)
        {
            this.view = view;
        }

        private string mWsUrl;
        public string WsUrl
        {
            get => mWsUrl;
            set
            {
                mWsUrl = value;
                RaisePropertyChanged(nameof(WsUrl));
            }
        }

        private string mSendContent = string.Empty;
        public string SendContent
        {
            get => mSendContent;
            set
            {
                mSendContent = value;
                RaisePropertyChanged(nameof(SendContent));
            }
        }

        private long mPingTime = 1000;
        public long PingTime
        {
            get => mPingTime;
            set
            {
                mPingTime = value;
                RaisePropertyChanged(nameof(PingTime));
            }
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(WsUrl))
            {
                view.AppendInfo("请输入正确的WebSocket地址");
                return;
            }

            if (mClient == null)
            {
                InitSocketClient();
            }

            if (mClient.IsAlive())
            {
                view.AppendInfo("WebSocket已连接,请先断开上次连接");
                return;
            }

            view.AppendInfo($"<=== start connect");
            mClient.ConnectAsync();
        }

        public void Send()
        {
            view.AppendInfo($"<=== socket send:{SendContent}");
            mClient.Send(SendContent);
        }

        private bool mIsAlive = false;

        private bool mIsConnectEnable = true;
        public bool IsConnectEnable
        {
            get => mIsConnectEnable;
            set
            {
                mIsConnectEnable = value;
                RaisePropertyChanged(nameof(IsConnectEnable));
            }
        }

        private bool mIsCloseEnable = false;
        public bool IsCloseEnable
        {
            get => mIsCloseEnable;
            set
            {
                mIsCloseEnable = value;
                RaisePropertyChanged(nameof(IsCloseEnable));
            }
        }

        private bool mIsPingChecked;
        public bool IsPingChecked
        {
            get => mIsPingChecked;
            set
            {
                mIsPingChecked = value;
                RaisePropertyChanged(nameof(IsPingChecked));
            }
        }

        private bool mIsPingEnable;
        public bool IsPingEnable
        {
            get => mIsPingEnable;
            set
            {
                mIsPingEnable = value;
                RaisePropertyChanged(nameof(IsPingEnable));
            }
        }

        public void Close()
        {
            view.AppendInfo($"<=== start close");
            mClient?.CloseAsync();
        }

        private DispatcherTimer pingTimer;
        public void StartPing()
        {
            if (!mIsAlive)
            {
                view.AppendInfo("start ping failure: ws is not connected");
                return;
            }
            if (pingTimer?.IsEnabled ?? false)
            {
                pingTimer.Stop();
            }
            pingTimer = new DispatcherTimer();
            pingTimer.Interval = TimeSpan.FromMilliseconds(PingTime);
            pingTimer.Tick += (sender, args) =>
            {
                Ping();
            };
            pingTimer.Start();
            view.AppendInfo($"<===StartPing, time:{PingTime}");
        }

        private void Ping(string msg = null)
        {
            view.AppendInfo($"<=== ping:{msg}");
            mClient.Ping(msg);
        }

        public void StopPing()
        {
            view.AppendInfo("<===StopPing");
            if (pingTimer?.IsEnabled ?? false)
            {
                pingTimer.Stop();
                pingTimer = null;
            }
        }

        private void InitSocketClient()
        {
            view.AppendInfo("InitSocketClient");
            mClient = new SocketClient(WsUrl);
            mClient.OpenEvent += ClientOnOpenEvent;
            mClient.CloseEvent += ClientOnCloseEvent;
            mClient.ErrorEvent += ClientOnErrorEvent;
            mClient.MessageEvent += ClientOnMessageEvent;
        }

        private void ClientOnMessageEvent(object sender, MessageEventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"===> receive message: {e.Data}");
            });
        }

        private void ClientOnErrorEvent(object sender, ErrorEventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"===> socket error: {e.Message}");
                SetState(false);
            });
            StopPing();
        }

        private void SetState(bool isAlive)
        {
            mIsAlive = isAlive;
            if (isAlive)
            {
                IsPingEnable = true;
                IsConnectEnable = false;
                IsCloseEnable = true;
            }
            else
            {
                IsPingEnable = false;
                if (IsPingChecked)
                {
                    IsPingChecked = false;
                }
                IsConnectEnable = true;
                IsCloseEnable = false;
            }
        }

        private void ClientOnCloseEvent(object sender, CloseEventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"===> socket closed:{e.Code}:{e.Reason}");
                SetState(false);
            });
            StopPing();
        }

        private void ClientOnOpenEvent(object sender, EventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"<=== socket connected");
                SetState(true);
            });
        }
    }
}