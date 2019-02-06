using System;

namespace Freecon.Core.Utils
{
    public class VerboseLoggerUtil : ILoggerUtil
    {
        public VerboseLoggerUtil()
        {
            // Todo specify log level
        }

        public void Log(string message, LogLevel level)
        {
            switch (level)
            {
                // Todo: Bake another logger that's more useful in Production.
                case LogLevel.Verbose:
                case LogLevel.Warning:
                case LogLevel.Error:
                default:
                    Console.WriteLine(message);
                    break;
            }
        }

        public void Log(string message, string statsdKey, LogLevel level)
        {
            Log(message, statsdKey, level, EventLogType.Increment);
        }

        public void Log(string message, string statsdKey, LogLevel level, EventLogType eventLogType)
        {
            Log(message, level);
        }
    }

    public interface ILoggerUtil
    {
        void Log(string message, LogLevel level);
        void Log(string message, string statsdKey, LogLevel level);
        void Log(string message, string statsdKey, LogLevel level, EventLogType evengLogType);
    }

    public enum LogLevel
    {
        Verbose,
        Warning,
        Error,
        Message
    }

    public enum EventLogType
    {
        Increment
    }
}
