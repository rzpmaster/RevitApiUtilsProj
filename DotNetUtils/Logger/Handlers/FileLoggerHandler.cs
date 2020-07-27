using DotNetUtils.Logger.Formatters;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetUtils.Logger.Handlers
{
    public class FileLoggerHandler : ILoggerHandler
    {
        object lockObj = new object();

        long maxFileSize = 0x600000L;
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

        public ILoggerFormatter DefaultLoggerFormatter { get; set; }

        #region ILoggerHandler Members
        public string Name => $"文件日志工具(路径:{_filePath})";

        public void Publish(LogMessage logMessage)
        {
            // 检查文件夹是否存在
            // 大于 fileSize 需要跟新文件名
            DoesFilePathExistOrTooLarge();

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
            lock (lockObj)
            {
                var directory = Path.GetDirectoryName(_filePath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // 如果当前log文件超过了最大字节大小，分开记录
                if (File.Exists(_filePath) &&
                    new FileInfo(_filePath).Length > this.maxFileSize)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_filePath);
                    string extension = Path.GetExtension(_filePath);
                    int num = 0;
                    while (true)
                    {
                        string str5 = Path.Combine(directory, string.Concat(new object[] { fileNameWithoutExtension, "_", num, extension }));
                        if (!File.Exists(str5))
                        {
                            _filePath = str5;
                            break;
                        }
                        if (new FileInfo(str5).Length < this.maxFileSize)
                        {
                            _filePath = str5;
                            break;
                        }
                        num++;
                    }
                }
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
