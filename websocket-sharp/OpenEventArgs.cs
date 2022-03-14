using System;

namespace WebSocketSharp
{
    public class OpenEventArgs : EventArgs
    {
        public string LocalIP { get; private set; }
        public string RemoteIP { get; private set; }
        public OpenEventArgs(string localIp, string remoteIp)
        {
            LocalIP = localIp;
            RemoteIP = remoteIp;
        }
    }
}