using DotNetUtils.Logger.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Logger.Publisher
{
    public interface ILoggerHandlerManager
    {
        ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler);
        ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler, Predicate<LogMessage> filter);

        bool RemoveHandler(ILoggerHandler loggerHandler);
    }
}
