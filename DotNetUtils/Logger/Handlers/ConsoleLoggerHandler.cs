using DotNetUtils.Logger.Formatters;
using System;

namespace DotNetUtils.Logger.Handlers
{
    public class ConsoleLoggerHandler : ILoggerHandler
    {
        public ConsoleLoggerHandler() : this(new DefaultLoggerFormatter()) { }

        public ConsoleLoggerHandler(ILoggerFormatter loggerFormatter)
        {
            DefaultLoggerFormatter = loggerFormatter;
        }

        public ILoggerFormatter DefaultLoggerFormatter { get; set; }

        public string Name => "控制台日志工具";

        public void Publish(LogMessage logMessage)
        {
            Console.WriteLine(DefaultLoggerFormatter.ApplyFormat(logMessage));
        }
    }
}
