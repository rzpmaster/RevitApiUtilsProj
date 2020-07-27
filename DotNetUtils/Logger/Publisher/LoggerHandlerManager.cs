using DotNetUtils.Logger.Handlers;
using System;
using System.Collections.Generic;

namespace DotNetUtils.Logger.Publisher
{
    public class LoggerHandlerManager : ILoggerHandlerManager
    {
        readonly IList<ILoggerHandler> _loggerHandlers;
        readonly IList<LogMessage> _messages;

        public LoggerHandlerManager()
        {
            _loggerHandlers = new List<ILoggerHandler>();
            _messages = new List<LogMessage>();
        }

        public void Publish(LogMessage logMessage)
        {
            if (StoreLogMessages)
                _messages.Add(logMessage);

            foreach (var loggerHandler in _loggerHandlers)
                loggerHandler.Publish(logMessage);
        }

        #region ILoggerHandlerManager Members
        public ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler)
        {
            if (loggerHandler != null)
                _loggerHandlers.Add(loggerHandler);
            return this;
        }

        public ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler, Predicate<LogMessage> filter)
        {
            if ((filter == null) || loggerHandler == null)
                return this;

            return AddHandler(new FilteredLoggerHandler()
            {
                Filter = filter,
                Handler = loggerHandler
            });
        }

        public bool RemoveHandler(ILoggerHandler loggerHandler)
        {
            return _loggerHandlers.Remove(loggerHandler);
        }
        #endregion

        public bool StoreLogMessages { get; set; } = false;

        public IEnumerable<LogMessage> Messages
        {
            get { return _messages; }
        }
    }
}
