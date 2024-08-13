using System;
using System.Configuration;
using System.IO;
using System.Threading;

namespace Logger.Logging
{
    public class TextFileLogger : ILogger
    {
        public string ProjectName = ConfigurationManager.AppSettings["ProjectName"];
        public string WebLogPath = ConfigurationManager.AppSettings["WebLogPath"];
        public string WebApiLogPath = ConfigurationManager.AppSettings["WebApiLogPath"];

        public void WebLog(string _logType, string _logMessage)
        {
            string folder = "";
            string fileName = "";

            if (_logType == LogType.INFORMATION_TYPE)
            {
                folder = Path.Combine(WebLogPath, "Information");
                fileName = "_" + ProjectName + "_WEB_INFORMATION_LOG.txt";
            }
            else if (_logType == LogType.WARNING_TYPE)
            {
                folder = Path.Combine(WebLogPath, "Warning");
                fileName = "_" + ProjectName + "_WEB_WARNING_LOG.txt";
            }
            else if (_logType == LogType.ERROR_TYPE)
            {
                folder = Path.Combine(WebLogPath, "Error");
                fileName = "_" + ProjectName + "_WEB_ERROR_LOG.txt";
            }

            StreamWriter writer = CreateWriter(folder, fileName);

            if (writer != null)
            {
                writer.WriteLine(DateTime.Now + " ##### " + _logType + " ##### \r\n" + _logMessage + "\r\n\r\n");
                writer.Close();
            }
        }

        public void WebApiLog(string _logType, string _logMessage)
        {
            string folder = "";
            string fileName = "";

            if (_logType == LogType.INFORMATION_TYPE)
            {
                folder = Path.Combine(WebApiLogPath, "Information");
                fileName = "_" + ProjectName + "_WEB_API_INFORMATION_LOG.txt";
            }
            else if (_logType == LogType.WARNING_TYPE)
            {
                folder = Path.Combine(WebApiLogPath, "Warning");
                fileName = "_" + ProjectName + "_WEB_API_WARNING_LOG.txt";
            }
            else if (_logType == LogType.ERROR_TYPE)
            {
                folder = Path.Combine(WebApiLogPath, "Error");
                fileName = "_" + ProjectName + "_WEB_API_ERROR_LOG.txt";
            }

            StreamWriter writer = CreateWriter(folder, fileName);

            if (writer != null)
            {
                writer.WriteLine(DateTime.Now + " ##### " + _logType + " ##### \r\n" + _logMessage + "\r\n\r\n");
                writer.Close();
            }
        }

        private StreamWriter CreateWriter(string folder, string fileName)
        {
            StreamWriter writer = null;
            try
            {
                string logFileName = String.Format("{0:yyyyMMdd}", DateTime.Now) + fileName;
                string path = Path.Combine(folder, logFileName);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (!File.Exists(path))
                {
                    writer = new StreamWriter(path);
                }
                else
                {
                    writer = File.AppendText(path);
                }
            }
            catch (Exception ex)
            {
                //fail to write file will retry again, after 150ms
                Thread.Sleep(150);
                writer = CreateWriter(folder, fileName);
            }
            return writer;


        }

    }
}