using DotNetUtils.Logger.Formatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetUtils.Logger.Handlers
{
    public class FileLoggerHandler : ILoggerHandler
    {
        object lockObj = new object();

        long fileSize = 0x600000L;
        string _filePath;

        public FileLoggerHandler() : this(new DefaultLoggerFormatter()) { }

        public FileLoggerHandler(ILoggerFormatter loggerFormatter) : this(GetDefaultFilePath(), loggerFormatter)
        {
            DefaultLoggerFormatter = loggerFormatter;
        }

        public FileLoggerHandler(string filePath, ILoggerFormatter loggerFormatter)
        {
            // 检查文件位置是否和法
            if (string.IsNullOrEmpty(_filePath))
                throw new ArgumentNullException("路径不能为空");

            //string fullPathRegex = @"^([a-zA-Z]:\\)?[^\/\:\*\?\""\<\>\|\,]*$";
            string pathRegex = @"^[^\/\:\*\?\""\<\>\|\,]+$";
            Regex regex = new Regex(pathRegex);
            Match m = regex.Match(filePath);
            if (!m.Success)
            {
                throw new ArgumentException("路径不合法");
            }

            // 测试下,能否用相对路径?

            _filePath = filePath;
            DefaultLoggerFormatter = loggerFormatter;
        }

        #region ILoggerHandler Members
        public ILoggerFormatter DefaultLoggerFormatter { get; set; }

        public void Publish(LogMessage logMessage)
        {
            lock (lockObj)
            {
                // 检查文件夹是否存在
                // 大于 fileSize 需要跟新文件名
                DoesFilePathExistOrTooLarge();
            }

            // 异步写入文件 
            WriterAsyn(logMessage);
        }
        #endregion

        public void Write(LogMessage logMessage)
        {
            lock (lockObj)
            {
                using (StreamWriter writer = new StreamWriter(File.Open(_filePath, FileMode.Append, FileAccess.Write, FileShare.None), Encoding.Default))
                {
                    writer.WriteLine(DefaultLoggerFormatter.ApplyFormat(logMessage));
                }
            }
        }

        public void WriterAsyn(LogMessage logMessage)
        {
            Action<LogMessage> writeHandler = (LogMessage) => this.Write(LogMessage);
            AsyncCallback callback = ar => ((Action<LogMessage>)ar.AsyncState).EndInvoke(ar);
            writeHandler.BeginInvoke(logMessage, callback, writeHandler);
        }

        void DoesFilePathExistOrTooLarge()
        {
            var directory = Path.GetDirectoryName(_filePath);

            if (Directory.Exists(directory))
            {
            }
            else
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string GetDefaultFilePath()
        {
            var currentDate = DateTime.Now;
            var guid = Guid.NewGuid();
            return string.Format("Log_{0:0000}{1:00}{2:00}-{3:00}{4:00}_{5}.log",
                currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, guid);
        }
    }
}
