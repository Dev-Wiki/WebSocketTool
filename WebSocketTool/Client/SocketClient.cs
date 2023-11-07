using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using log4net;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace WebSocketTool.Client
{
    public class SocketClient
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(SocketClient));
        private readonly WebSocket mSocket;
        public event EventHandler<MessageEventArgs> MessageEvent;
        public event EventHandler<ErrorEventArgs> ErrorEvent;
        public event EventHandler<EventArgs> OpenEvent;
        public event EventHandler<CloseEventArgs> CloseEvent;
        public string Url { get; private set; }

        public SocketClient(string url)
        {
            Log.Info($"create socket:{url}");
            Url = url;
            mSocket = new WebSocket(url);
            if (url.StartsWith("wss"))
            {
                mSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            }
            mSocket.OnOpen += OnOpen;
            mSocket.OnClose += OnClose;
            mSocket.OnError += OnError;
            mSocket.OnMessage += OnMessage;
        }

        #region Config

        public SocketClient AddServerCertificateValidationCallback(RemoteCertificateValidationCallback callback)
        {
            Log.Info("SetServerCertificateValidationCallback");
            mSocket.SslConfiguration.ServerCertificateValidationCallback += callback;
            return this;
        }

        public SocketClient RemoveServerCertificateValidationCallback(RemoteCertificateValidationCallback callback)
        {
            Log.Info("RemoveServerCertificateValidationCallback");
            mSocket.SslConfiguration.ServerCertificateValidationCallback -= callback;
            return this;
        }

        public SocketClient SetEnableSslProtocols(SslProtocols protocols)
        {
            Log.Info($"SetEnableSslProtocols:{protocols}");
            mSocket.SslConfiguration.EnabledSslProtocols = protocols;
            return this;
        }

        public SocketClient SetHttpProxy(string address, string userName, string password)
        {
            Log.Info($"SetHttpProxy, address:{address}, userName:{userName}, password:{password}");
            if (address.StartsWith("http://"))
            {
                mSocket.SetProxy(address, userName, password);
            }
            return this;
        }

        #endregion

        #region Event Handler
        private void OnMessage(object sender, MessageEventArgs e)
        {
            Log.Info($"OnMessage, isPing:{e.IsPing}, isText:{e.IsText}, isBinary:{e.IsBinary}, data:{e.Data}, rawData:{e.RawData.Length}");
            MessageEvent?.Invoke(this, e);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Log.Info($"OnError, message:{e.Message}, exception:{e.Exception}");
            ErrorEvent?.Invoke(this, e);
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            Log.Info($"OnClose, code:{e.Code}, reason:{e.Reason}");
            CloseEvent?.Invoke(this, e);
        }

        private void OnOpen(object sender, EventArgs e)
        {
            Log.Info("OnOpen");
            OpenEvent?.Invoke(this, e);
        }
        #endregion

        #region State
        public bool IsAlive()
        {
            Log.Info($"IsAlive:{mSocket?.IsAlive ?? false}");
            return mSocket?.IsAlive ?? false;
        }
        #endregion
        
        #region Operate
        public void Connect()
        {
            Log.Info("Connect");
            mSocket.Connect();
        }

        public void ConnectAsync()
        {
            Log.Info("ConnectAsync");
            mSocket.ConnectAsync();
        }
        
        public void Send(string content)
        {
            Log.Info($"send:{content}");
            if (content == null)
            {
                Log.Error("content is null!");
                return;
            }
            mSocket.Send(content);
        }
        
        public void Ping(string msg = null)
        {
            Log.Info($"ping:{msg}");
            if (msg != null)
            {
                mSocket.Ping(msg);
            }
            else
            { 
                mSocket?.Ping();
            }
        }

        public void Close()
        {
            Log.Info("Close");
            mSocket.Close();
        }

        public void CloseAsync()
        {
            Log.Info("CloseAsync");
            mSocket.CloseAsync();
        }
        #endregion
    }
}