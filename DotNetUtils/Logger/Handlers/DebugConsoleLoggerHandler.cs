using DotNetUtils.Logger.Formatters;

namespace DotNetUtils.Logger.Handlers
{
    public class DebugConsoleLoggerHandler : ILoggerHandler
    {
        public DebugConsoleLoggerHandler() : this(new DefaultLoggerFormatter()) { }

        public DebugConsoleLoggerHandler(ILoggerFormatter loggerFormatter)
        {
            DefaultLoggerFormatter = loggerFormatter;
        }

        public ILoggerFormatter DefaultLoggerFormatter { get; set; }

        public void Publish(LogMessage logMessage)
        {
            System.Diagnostics.Debug.WriteLine(DefaultLoggerFormatter.ApplyFormat(logMessage));
        }
    }
}
