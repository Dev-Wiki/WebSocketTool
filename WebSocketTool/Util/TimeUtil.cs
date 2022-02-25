using System;
using log4net.DateFormatter;

namespace WebSocketTool.Util
{
    public class TimeUtil
    {
        public static string GetLogTime()
        {
            return DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.ms]");
        }
    }
}