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

        #endregion

        public void Connect()
        {
            if (string.IsNullOrEmpty(WsUrl) || (!WsUrl.StartsWith("wss://") && !WsUrl.StartsWith("ws://")))
            {
                view.AppendInfo($"Hint {TimeUtil.GetCurrentDateTime()} \n 请输入正确的WebSocket地址");
                return;
            }

            if (mClient == null)
            {
                InitSocketClient();
            }

            if (mClient.IsAlive())
            {
                view.AppendInfo($"Hint {TimeUtil.GetCurrentDateTime()} \n WebSocket已连接,请先断开上次连接");
                return;
            }

            view.AppendInfo($"Hint {TimeUtil.GetCurrentDateTime()} \n Start Connect Socket");
            if (IsProxyChecked)
            {
                if (!string.IsNullOrEmpty(ProxyAddress))
                {
                    view.AppendInfo($"Hint use proxy: {ProxyAddress}");
                    mClient.SetHttpProxy(ProxyAddress, ProxyUserName, ProxyPassword);
                }
                else
                {
                    view.ShowToast("请输入代理地址!");
                    view.AppendInfo($"use proxy address is empty!");
                }
            }
            mClient.ConnectAsync();
        }

        public void Send()
        {
            view.AppendInfo($"You {TimeUtil.GetCurrentDateTime()} \n {SendContent}");
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
            view.AppendInfo($"Hint {TimeUtil.GetCurrentDateTime()} \n Start Close Socket");
            mClient?.CloseAsync();
        }

        private Timer pingTimer;
        public void StartPing()
        {
            if (!mIsAlive)
            {
                view.AppendInfo("start ping failure: ws is not connected");
                return;
            }

            if (PingTime <= 0)
            {
                view.AppendInfo("ping time interval must > 0");
                return;
            }

            pingTimer = new Timer {Interval = PingTime, AutoReset = true};
            pingTimer.Enabled = true;
            pingTimer.Elapsed += (s, e) =>
            {
                Ping();
            };
            pingTimer.Start();
            view.AppendInfo($"You {TimeUtil.GetCurrentDateTime()} \n StartPing, TimeSpan:{PingTime}");
        }

        private void Ping(string msg = null)
        {
            mClient.Ping(msg);
            App.RunOnUIThread(() => view.AppendInfo($"You {TimeUtil.GetCurrentDateTime()} \n Send Ping:{msg}"));
        }

        public void StopPing()
        {
            App.RunOnUIThread(() => view.AppendInfo($"You {TimeUtil.GetCurrentDateTime()} \n StopPing"));
            if (pingTimer != null)
            {
                pingTimer.Stop();
                pingTimer = null;
            }
        }

        private void InitSocketClient()
        {
            view.AppendInfo($"Hint {TimeUtil.GetCurrentDateTime()} \n InitSocketClient");
            mClient = new SocketClient(WsUrl);
            if (WsUrl.StartsWith("wss:"))
            {
                mClient.SetServerCertificateValidationCallback(CertificateValidationCallback);
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

        private bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            App.RunOnUIThread(() => { view.AppendInfo($"ServerCertificateError:{errors}"); });
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
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"Server {TimeUtil.GetCurrentDateTime()} \n {e.Data}");
            });
        }

        private void ClientOnErrorEvent(object sender, ErrorEventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"Server {TimeUtil.GetCurrentDateTime()} \n Socket Error: {e.Message}");
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
                view.AppendInfo($"Server {TimeUtil.GetCurrentDateTime()} \n Socket Closed:{e.Code}:{e.Reason}");
                SetState(false);
            });
            StopPing();
        }

        private void ClientOnOpenEvent(object sender, EventArgs e)
        {
            App.RunOnUIThread(() =>
            {
                view.AppendInfo($"Server {TimeUtil.GetCurrentDateTime()} \n Socket Connected");
                SetState(true);
            });
        }
    }
}