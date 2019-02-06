using System;
using System.IO;
using Newtonsoft.Json;

namespace Freecon.Client.Managers
{
    public static class ClientLogger
    {
        public static bool _active; // In case you want to deactivate the logger

        public static bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public static void init()
        {
            _active = true;

            var textOut = new StreamWriter(new FileStream("log.html", FileMode.Create, FileAccess.Write));
            textOut.WriteLine("Log File");
            textOut.WriteLine("");
            textOut.WriteLine("<span style=\"font-family: &quot;Kootenay&quot;; color: #000000;\">");
            textOut.WriteLine("Log started at " + DateTime.Now.ToLongTimeString() + "</span><hr />");
            textOut.Close();
        }

        public static void Log(Log_Type type, string text)
        {

            if (!_active) return;
            string begin = "";
            switch (type)
            {
                case Log_Type.ERROR:
                    begin = "<span style=\"color: #00f000;\">";
                    break;
                case Log_Type.INFO:
                    begin = "<span style=\"color: #0008f0;\">";
                    break;
                case Log_Type.WARNING:
                    begin = "<span style=\"color: #00ff00;\">";
                    break;
            }
            text = begin + DateTime.Now.ToLongTimeString() + " : " + text + "</span><br>";
            Output(text);

        }

        public static void LogError(string text)
        {
            Log(Log_Type.ERROR, text);
        }

        public static void LogError(Exception e)
        {
            Log(Log_Type.ERROR, JsonConvert.SerializeObject(e));
        }

        public static void LogInfo(string text)
        {
            Log(Log_Type.INFO, text);
        }

        public static void LogWarning(string text)
        {
            Log(Log_Type.WARNING, text);
        }

        private static void Output(string text)
        {
            try
            {
                var textOut = new StreamWriter(new FileStream("log.html", FileMode.Append, FileAccess.Write));
                textOut.WriteLine(text);
                textOut.Close();
            }
            catch (Exception e)
            {
                string error = e.Message;
            }

        }
    }

    public enum Log_Type
    {
        ERROR = 0,
        WARNING = 1,
        INFO = 2
    }
}