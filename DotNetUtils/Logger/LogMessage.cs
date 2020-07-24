using DotNetUtils.Logger.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Logger
{
    public class LogMessage
    {
        public LogMessage() { }

        public LogMessage(Logger.LogLevel level, string text, DateTime dateTime, string callingClass, string callingMethod, int lineNumber)
        {
            Level = level;
            Text = text;
            DateTime = dateTime;
            CallingClass = callingClass;
            CallingMethod = callingMethod;
            LineNumber = lineNumber;
        }

        public DateTime DateTime { get; set; }
        public Logger.LogLevel Level { get; set; }
        public string Text { get; set; }
        public string CallingClass { get; set; }
        public string CallingMethod { get; set; }
        public int LineNumber { get; set; }

        public override string ToString()
        {
            return new DefaultLoggerFormatter().ApplyFormat(this);
        }
    }
}
