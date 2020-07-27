using System;

namespace DotNetUtils.Logger.Handlers
{
    public class FilteredLoggerHandler : ILoggerHandler
    {
        public FilteredLoggerHandler() { }

        public FilteredLoggerHandler(Predicate<LogMessage> predicate, ILoggerHandler loggerHandler)
        {
            Filter = predicate;
            Handler = loggerHandler;
        }

        public Predicate<LogMessage> Filter { get; set; }
        public ILoggerHandler Handler { get; set; }

        public string Name => nameof(FilteredLoggerHandler);

        public void Publish(LogMessage logMessage)
        {
            if (Filter(logMessage))
                Handler.Publish(logMessage);
        }
    }
}
