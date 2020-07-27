namespace DotNetUtils.Logger
{
    public interface ILogger
    {
        void Log(LogLevel logLevel, string message, string callingClass, string callingMethod, int lineNumber);
    }

    public enum LogLevel
    {
        None = 0,
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
    }
}
