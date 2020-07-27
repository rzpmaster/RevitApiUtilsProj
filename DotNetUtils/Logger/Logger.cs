using DotNetUtils.Logger.Handlers;
using DotNetUtils.Logger.Publisher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Logger
{
    public class Logger : ILogger
    {
        public Logger()
        {
            Manager = new LoggerHandlerManager();
        }

        public LogLevel DefaultLevel { get; set; } = LogLevel.Information;

        public LoggerHandlerManager Manager { get; }

        public void InitializeLoggerHandlers(IEnumerable<ILoggerHandler> loggerHandlers)
        {
            string message = "已初始化" + "\r\n";
            foreach (var handler in loggerHandlers)
            {
                Manager.AddHandler(handler);
                message += handler.Name + "\r\n";
            }

            Log(message);
        }

        public void AddLoggerHandlers(IEnumerable<ILoggerHandler> loggerHandlers)
        {
            string message = "已添加" + "\r\n";
            foreach (var handler in loggerHandlers)
            {
                Manager.AddHandler(handler);
                message += handler.Name + "\r\n";
            }

            Log(message);
        }

        public void RemoveLoggerHandlers(IEnumerable<ILoggerHandler> loggerHandlers)
        {
            bool flag = false;
            string message = "已移除" + "\r\n";
            foreach (var handler in loggerHandlers)
            {
                if (Manager.RemoveHandler(handler))
                {
                    message += handler.Name + "\r\n";
                    flag = true;
                }
            }
            if (flag) Log(message);
        }

        public void Log()
        {
            Log("There is no message");
        }

        public void Log(string message)
        {
            Log(DefaultLevel, message);
        }

        public void Log(LogLevel level, string message)
        {
            var stackFrame = FindStackFrame();
            var methodBase = GetCallingMethodBase(stackFrame);
            var callingMethod = methodBase.Name;
            var callingClass = methodBase.ReflectedType.Name;
            var lineNumber = stackFrame.GetFileLineNumber();

            Log(level, message, callingClass, callingMethod, lineNumber);
        }

        public void Log<TClass>(Exception exception) where TClass : class
        {
            var message = string.Format("Log exception -> Message: {0}\nStackTrace: {1}", exception.Message,
                                        exception.StackTrace);
            Log<TClass>(LogLevel.Error, message);
        }

        public void Log<TClass>(string message) where TClass : class
        {
            Log<TClass>(DefaultLevel, message);
        }

        public void Log<TClass>(LogLevel level, string message) where TClass : class
        {
            var stackFrame = FindStackFrame();
            var methodBase = GetCallingMethodBase(stackFrame);
            var callingMethod = methodBase.Name;
            var callingClass = typeof(TClass).Name;
            var lineNumber = stackFrame.GetFileLineNumber();

            Log(level, message, callingClass, callingMethod, lineNumber);
        }

        #region ILogger Member
        public void Log(LogLevel logLevel, string message, string callingClass, string callingMethod, int lineNumber)
        {
            var currentDateTime = DateTime.Now;
            var logMessage = new LogMessage(logLevel, message, currentDateTime, callingClass, callingMethod, lineNumber);

            Manager.Publish(logMessage);
        }
        #endregion

        //
        // private Methods
        //

        /// <summary>
        /// 当前线程的调用堆栈
        /// </summary>
        /// <returns></returns>
        private StackFrame FindStackFrame()
        {
            var stackTrace = new StackTrace();
            for (var i = 0; i < stackTrace.GetFrames().Count(); i++)
            {
                var methodBase = stackTrace.GetFrame(i).GetMethod();
                var name = MethodBase.GetCurrentMethod().Name;
                if (!methodBase.Name.Equals("Log") && !methodBase.Name.Equals(name))
                    return new StackFrame(i, true);
            }
            return null;
        }

        private MethodBase GetCallingMethodBase(StackFrame stackFrame)
        {
            return stackFrame == null
                ? MethodBase.GetCurrentMethod() : stackFrame.GetMethod();
        }
    }
}
