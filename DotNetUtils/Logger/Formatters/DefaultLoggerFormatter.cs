using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Logger.Formatters
{
    public class DefaultLoggerFormatter : ILoggerFormatter
    {
        public string ApplyFormat(LogMessage logMessage)
        {
            return string.Format("{0:dd.MM.yyyy HH:mm:ss}: {1} [line: {2} {3} -> {4}()]:\n {5}\n\n",
                            logMessage.DateTime,
                            logMessage.Level, logMessage.LineNumber, logMessage.CallingClass, logMessage.CallingMethod,
                            logMessage.Text);
        }
    }
}
