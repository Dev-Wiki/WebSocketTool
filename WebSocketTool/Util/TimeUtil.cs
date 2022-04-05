using System;
using log4net.DateFormatter;

namespace WebSocketTool.Util
{
    public class TimeUtil
    {
        public static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss.ms");
        }
        
        public static string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ms");
        }
    }
}