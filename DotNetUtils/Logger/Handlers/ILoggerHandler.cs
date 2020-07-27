namespace DotNetUtils.Logger.Handlers
{
    public interface ILoggerHandler
    {
        string Name { get; }

        /// <summary>
        /// 发布日志
        /// </summary>
        /// <param name="logMessage"></param>
        void Publish(LogMessage logMessage);
    }
}
