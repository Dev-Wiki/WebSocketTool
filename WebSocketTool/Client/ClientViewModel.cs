using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using log4net;
using WebSocketSharp;
using WebSocketTool.Base;
using WebSocketTool.Util;
using WebSocketTool.View.Dialog;
using LogManager = log4net.LogManager;

namespace WebSocketTool.Client
{
    public class ClientViewModel : BaseViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(ClientViewModel));
        private readonly IClientView view;
        private SocketClient mClient;

        public ClientViewModel(IClientView view)
        {
            this.view = view;
        }

        #region data bingding 
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

        private long mPingTime = 2000;
        public long PingTime
        {
            get => mPingTime;
            set
            {
                mPingTime = value;
                RaisePropertyChanged(nameof(PingTime));
            }
        }

        #region Proxy

        private string proxyAddress;
        private string proxyUserName;
        private string proxyPassword;
        
        public string ProxyAddress
        {
            get => proxyAddress;
            set
            {
                proxyAddress = value;
                RaisePropertyChanged(nameof(ProxyAddress));
            }
        }

        public string ProxyUserName
        {
            get => proxyUserName;
            set
            {
                proxyUserName = value;
                RaisePropertyChanged(nameof(ProxyUserName));
            }
        }

        public string ProxyPassword
        {
            get => proxyPassword;
            set
            {
                proxyPassword = value;
                RaisePropertyChanged(nameof(ProxyPassword));
            }
        }

        private bool isProxyChecked;

        public bool IsProxyChecked
        {
            get => isProxyChecked;
            set
            {
                isProxyChecked = value;
                RaisePropertyChanged(nameof(IsProxyChecked));
            }
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
        #endregion
        
        #endregion

        private string FormatInfo(string info, int type = 0)
        {
            var tag = "【Hint】";
            switch (type)
            {
                case 1:
                    tag = "【Client】==>【Server】";
                    break;
                case 2:
                    tag = "【Client】<==【Server】";
                    break;
            }
            return $"{tag} {TimeUtil.GetCurrentDateTime()} \n {info}";
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(WsUrl) || (!WsUrl.StartsWith("wss://") && !WsUrl.StartsWith("ws://")))
            {
                view.AppendInfo(FormatInfo("请输入正确的WebSocket地址"));
                return;
            }

            if (mClient == null)
            {
                InitSocketClient();
            }
            
            if (IsProxyChecked)
            {
                if (!string.IsNullOrEmpty(ProxyAddress))
                {
                    view.AppendInfo(FormatInfo($"设置代理地址：{ProxyAddress}"));
                }
                mClient?.SetHttpProxy(ProxyAddress, ProxyUserName, ProxyPassword);
            }

            if (mClient != null && mClient.IsAlive())
            {
                view.AppendInfo(FormatInfo("WebSocket已连接,请先断开"));
                return;
            }

            if (IsProxyChecked)
            {
                if (!string.IsNullOrEmpty(ProxyAddress))
                {
                    view.AppendInfo(FormatInfo($"使用代理服务器: {ProxyAddress}"));
                    mClient?.SetHttpProxy(ProxyAddress, ProxyUserName, ProxyPassword);
                }
                else
                {
                    view.ShowToast("代理地址为空，请输入代理地址!");
                    view.AppendInfo(FormatInfo("代理地址为空，请输入正确的代理地址！"));
                }
            }
            view.AppendInfo(FormatInfo($"开始连接Socket:{mWsUrl}"));
            mClient.ConnectAsync();
        }

        public void Send()
        {
            view.AppendInfo(FormatInfo(SendContent, 1));
            mClient.Send(SendContent);
        }

        public void Close()
        {
            view.AppendInfo(FormatInfo("关闭Socket"));
            if (mClient != null)
            {
                mClient.Close();
            }
        }

        private Timer pingTimer;
        public void StartPing()
        {
            if (!mIsAlive)
            {
                view.AppendInfo(FormatInfo("启动 ping 失败: ws 未链接成功！"));
                return;
            }

            if (PingTime < 500)
            {
                view.AppendInfo(FormatInfo("ping间隔时间必须大于500ms"));
                return;
            }

            pingTimer = new Timer {Interval = PingTime, AutoReset = true};
            pingTimer.Enabled = true;
            pingTimer.Elapsed += (s, e) =>
            {
                Ping();
            };
            pingTimer.Start();
            view.AppendInfo(FormatInfo($"启动ping，时间间隔：{PingTime}"));
        }

        private void Ping(string msg = null)
        {
            mClient.Ping(msg);
            App.RunOnUIThread(() => view.AppendInfo(FormatInfo("发送ping", 1)));
        }

        public void StopPing()
        {
            App.RunOnUIThread(() => view.AppendInfo(FormatInfo("停止ping")));
            if (pingTimer != null)
            {
                pingTimer.Stop();
                pingTimer = null;
            }
        }
        
        private void InitSocketClient()
        {
            view.AppendInfo(FormatInfo("初始化Socket客户端"));
            mClient = new SocketClient(WsUrl);
            if (WsUrl.StartsWith("wss:"))
            {
                mClient.AddServerCertificateValidationCallback(CertificateValidationCallback);
            }
            mClient.OpenEvent += ClientOnOpenEvent;
            mClient.CloseEvent += ClientOnCloseEvent;
            mClient.ErrorEvent += ClientOnErrorEvent;
            mClient.MessageEvent += ClientOnMessageEvent;
            if (IsProxyChecked)
            {
                mClient.SetHttpProxy(ProxyAddress, ProxyUserName, ProxyPassword);
            }
            
        }

        private void UninitSocketClient()
        {
            if (mClient != null)
            {
                if (WsUrl.StartsWith("wss:"))
                {
                    mClient.RemoveServerCertificateValidationCallback(CertificateValidationCallback);
                }
                mClient.OpenEvent -= ClientOnOpenEvent;
                mClient.CloseEvent -= ClientOnCloseEvent;
                mClient.ErrorEvent -= ClientOnErrorEvent;
                mClient.MessageEvent -= ClientOnMessageEvent;
                mClient = null;
            }
        }

        private bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            App.RunOnUIThread(() => { view.AppendInfo(FormatInfo($"ServerCertificateError:{errors}", 2)); });
            Log.Info($"error:{errors}");
            Log.Info($"Issuer: {certificate.Issuer},Subject:{certificate.Subject}");
            Log.Info("ChainStatus:");
            foreach (var status in chain.ChainStatus)
            {
                Log.Info($"{status.StatusInformation}: {status}");
            }

            Log.Info("ChainElements:");
            foreach (var element in chain.ChainElements)
            {
                Log.Info($"element: {element.Certificate},{element.Information}");
            }

            return errors == SslPolicyErrors.None;
        }

        private void ClientOnMessageEvent(object sender, MessageEventArgs e)
        {
            App.RunOnUIThread(() => view.AppendInfo(FormatInfo(e.Data, 2)));
        }

        private void ClientOnErrorEvent(object sender, ErrorEventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo(FormatInfo($"Socket Error: {e.Message}", 2));
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
                view.AppendInfo(FormatInfo($"Socket Closed:{e.Code}:{e.Reason}", 2));
                SetState(false);
                UninitSocketClient();
            });
            StopPing();
        }

        private void ClientOnOpenEvent(object sender, EventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo(FormatInfo("Socket Connected", 2));
                SetState(true);
            });
        }
    }
}