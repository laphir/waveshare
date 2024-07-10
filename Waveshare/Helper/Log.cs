using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveshare.Helper
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class Log
    {
        public static bool IsDebugEnabled { get; set; } = false;

        public static void WriteLine(LogLevel level, string message)
        {
            if (level == LogLevel.Debug && !IsDebugEnabled)
            {
                return;
            }

            Console.WriteLine(message);
        }
    }
}
