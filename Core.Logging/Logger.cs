using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace Core.Logging
{
    public static class Logger
    {
        public static bool _active; // In case you want to deactivate the logger
        private static string logFileName;
        private static List<string> LogQueue;

        public static bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public static void Initialize()
        {
            _active = true;
            LogQueue = new List<string>();

            logFileName = string.Format(
                "log-{0}-{1}.html",
                DateTime.Now.ToUniversalTime().ToLongDateString(),
                DateTime.Now.ToUniversalTime().ToLongTimeString()
            );

            var ss = new StringBuilder(logFileName.Length);
            for (int i = 0; i < logFileName.Length; i++)
            {
                if (logFileName[i] == '\\' || logFileName[i] == '/')
                    ss.Append('-');
                else if (logFileName[i] == ':')
                    ss.Append('.');
                else
                    ss.Append(logFileName[i]);
            }
            logFileName = ss.ToString();

            var textOut = new StreamWriter(new FileStream(logFileName, FileMode.Create, FileAccess.Write));
            textOut.WriteLine("Log File");
            textOut.WriteLine("");
            textOut.WriteLine("<span style=\"font-family: &quot;Kootenay&quot;; color: #000000;\">");
            textOut.WriteLine("Log started at " + DateTime.Now.ToUniversalTime().ToLongTimeString() + "</span><hr />");
            textOut.Close();
        }

        public static void Log(Log_Type type, params string[] text)
        {
            if (!_active)
            {
                return;
            }

            var begin = "";
            switch (type)
            {
                case Log_Type.ERROR:
                    begin = "<span style=\"color: #00f000;\">";
                    break;
                case Log_Type.INFO:
                default:
                    begin = "<span style=\"color: #0008f0;\">";
                    break;
                case Log_Type.WARNING:
                    begin = "<span style=\"color: #00ff00;\">";
                    break;
            }
            
            var concatText = text.Aggregate("", (output, t) => output + "<li>" + SecurityElement.Escape(t) + "</li>");
            
            Output(begin + DateTime.Now.ToUniversalTime().ToLongTimeString() + " : <ol>" + concatText + "</ol></span><br>");
        }

        /// <summary>
        /// Only writes log to file when told to flush. This allows for less disk io
        /// </summary>
        public static void Flush()
        {
            if (LogQueue.Count == 0)
                return;
            try
            {
                var textOut = new StreamWriter(new FileStream(logFileName, FileMode.Append, FileAccess.Write));
                foreach (string s in LogQueue)
                    textOut.WriteLine(s);
                textOut.Close();
                LogQueue.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine("Logger Flush Error: " + e.ToString());
            }
        }

        private static void Output(string text)
        {
            LogQueue.Add(text);
        }
        
        public static void LogRedisInfo(string info, string key, string data)
        {
            Log(Log_Type.INFO, info, key, data);
        }

        public static void LogRedisError(Exception e, string key, string data)
        {
            var highLevelMessage = "Redis error.";

            Log(Log_Type.ERROR, highLevelMessage, e.ToString(), key, data);
        }
    }

    public enum Log_Type
    {
        ERROR = 0,
        WARNING = 1,
        INFO = 2
    }
}
