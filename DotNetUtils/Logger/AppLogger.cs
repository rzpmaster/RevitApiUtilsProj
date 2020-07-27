using DotNetUtils.Logger.Handlers;
using System;
using System.Collections.Generic;

namespace DotNetUtils.Logger
{
    public class AppLogger
    {
        private static readonly Logger Logger;

        static AppLogger()
        {
            Logger = new Logger();

            // 需要哪种日志工具,就在这里添加那种
            Logger.InitializeLoggerHandlers(new List<ILoggerHandler>() {
                new ConsoleLoggerHandler(),
                new DebugConsoleLoggerHandler(),
                //new FileLoggerHandler()
            });
        }

        public static void Log()
        {
            Logger.Log();
        }

        public static void Log(string message)
        {
            Logger.Log(message);
        }

        public static void Log(LogLevel level, string message)
        {
            Logger.Log(level, message);
        }

        public static void Log<TClass>(Exception exception) where TClass : class
        {
            Logger.Log<TClass>(exception);
        }

        public static void Log<TClass>(string message) where TClass : class
        {
            Logger.Log<TClass>(message);
        }

        public static void Log<TClass>(LogLevel level, string message) where TClass : class
        {
            Logger.Log<TClass>(level, message);
        }
    }
}
