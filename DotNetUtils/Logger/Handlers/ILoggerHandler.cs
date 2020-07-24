using DotNetUtils.Logger.Formatters;

namespace DotNetUtils.Logger.Handlers
{
    public interface ILoggerHandler
    {
        /// <summary>
        /// LogMessage 写入日志的格式
        /// </summary>
        ILoggerFormatter DefaultLoggerFormatter { get; set; }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="logMessage"></param>
        void Publish(LogMessage logMessage);
    }
}
