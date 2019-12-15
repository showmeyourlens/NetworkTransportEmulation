using System;
using System.Collections.Generic;
using System.Text;

namespace ToolsLibrary
{
    public static class TimeStamp
    {
        public static string TAB = "            ";
        public static void WriteLine(string value)
        {
             Console.WriteLine("{0} {1}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), value);
        }
        public static void WriteLine(string format, params object[] arg)
        {         
            Console.Write("{0} ", DateTime.UtcNow.ToString("HH:mm:ss.fff"));
            Console.WriteLine(format, arg);
        }
    }
}
