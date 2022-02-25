using System;
using System.Net;
using System.Text.RegularExpressions;

namespace WebSocketTool.Util
{
    public class NetUtil
    {

        private static readonly Log Log = LogManager.GetManager().GetLog(nameof(NetUtil));
        //A类地址：10.0.0.0--10.255.255.255
        //B类地址：172.16.0.0--172.31.255.255 
        //C类地址：192.168.0.0--192.168.255.255
        private const string AIp = @"^10\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])$";
        private const string BIp = @"^172\.(1[6789]|2[0-9]|3[01])\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])$";
        private const string CIp = @"^192\.168\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])\.(1\d{2}|2[0-4]\d|25[0-5]|[1-9]\d|[0-9])$";

        public static void GetHostAddress(string url)
        {
            try
            {
                var entry = Dns.GetHostAddresses(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static bool IsInnerIp(string url)
        {
            var entry = Dns.GetHostAddresses(url);
            if (entry == null)
            {
                return true;
            }
            var isInner = true;
            foreach (var address in entry)
            {
                var ip = address.ToString();
                isInner = isInner && (Regex.IsMatch(ip, AIp) || Regex.IsMatch(ip, BIp) || Regex.IsMatch(ip, CIp));
            }
            return isInner;
        }
    }
}