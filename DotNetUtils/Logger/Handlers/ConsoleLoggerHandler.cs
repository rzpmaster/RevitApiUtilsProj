using DotNetUtils.Logger.Formatters;
using System;

namespace DotNetUtils.Logger.Handlers
{
    public class ConsoleLoggerHandler : ILoggerHandler
    {
        public ConsoleLoggerHandler() : this(new DefaultLoggerFormatter()) { }

        public ConsoleLoggerHandler(ILoggerFormatter loggerFormatter)
        {
            _loggerFormatter = loggerFormatter;
        }

        public ILoggerFormatter DefaultLoggerFormatter { get; set; }

        public void Publish(LogMessage logMessage)
        {
            Console.WriteLine(DefaultLoggerFormatter.ApplyFormat(logMessage));
        }
    }
}
