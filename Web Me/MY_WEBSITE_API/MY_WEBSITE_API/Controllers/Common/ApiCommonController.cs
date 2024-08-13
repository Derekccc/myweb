using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Logger.Logging;
using MY_WEBSITE_API.Models.Common;
using MY_WEBSITE_API.Classes;
using DatabaseAccessor.DatabaseAccessor;


namespace MY_WEBSITE_API.Controllers.Common
{
    public class ApiCommonController : ApiController
    {

        public void WebLog(string _logType, string _userId, string _pageName, string _functionName, string _message)
        {
            ILogger Log = LoggerFactory.Logger;

            string logMessage = "";

            if (!string.IsNullOrEmpty(_userId))
            {
                logMessage += "User Id: " + _userId + "\r\n";
            }

            logMessage += "Page Name: " + _pageName + "\r\n";
            logMessage += "Function Name: " + _functionName + "\r\n";
            logMessage += "Message: " + _message;

            Log.WebLog(_logType, logMessage);
        }

        public void WebApiLog(string _logType, string _userId, string _methodName, string _message)
        {
            ILogger Log = LoggerFactory.Logger;

            string logMessage = "";

            if (!string.IsNullOrEmpty(_userId))
            {
                logMessage += "User Id: " + _userId + "\r\n";
            }

            logMessage += _methodName + "\r\n" + _message;

            Log.WebApiLog(_logType, logMessage);
        }

        public string MethodName()
        {
            try
            {
                var stackTrace = new StackTrace();
                MethodBase method = stackTrace.GetFrame(1).GetMethod();
                string methodName = method.Name;
                string className = method.ReflectedType.Name;

                return className + "." + methodName + "()";
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, "", "MethodName()", ex.ToString());
                return null;
            }
        }

        public bool CreateLocalPath(string _folderPath, string _userId)
        {
            try
            {
                string convertedPath = _folderPath.Replace(@"\", @"/");
                if (!Directory.Exists(convertedPath))
                {
                    Directory.CreateDirectory(convertedPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, _userId, MethodName(), "Error on create local path. " + _folderPath + ex.ToString());
                return false;
            }
        }

        public string GetFileInfomationType(string _fileName, string _userId)
        {
            try
            {
                string fileExtention = GetFileExtension(_fileName, _userId);
                string fileType = string.Empty;

                if (fileExtention == "xlsx")
                    fileType = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                if (fileExtention == "png" || fileExtention == "jpg" || fileExtention == "jpeg")
                    fileType = @"image/" + fileExtention;
                else
                    fileType = @"application/" + fileExtention;

                return fileType;
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, _userId, MethodName(), "Error on Get File Infomation Type. File name: " + _fileName + ex.ToString());
                return null;
            }
        }

        public string GetFileExtension(string _fileName, string _userId)
        {
            try
            {
                string[] fileNameArr = _fileName.Split('.');
                string fileExtension = fileNameArr[(fileNameArr.Length - 1)];

                return fileExtension;
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, _userId, MethodName(), "Error on Get File Extension. File name: " + _fileName + ex.ToString());
                return null;
            }
        }

        public void MoveImage(string uploadingFileName, string fullFilePath, int maxWidth, int maxHeight)
        {
            try
            {
                File.Move(uploadingFileName, fullFilePath);

                Image uploadedImage = Image.FromFile(fullFilePath);
                Image resizedImage = ScaleImage(uploadedImage, maxWidth, maxHeight);

                resizedImage.Save(uploadingFileName);

                uploadedImage.Dispose();
                resizedImage.Dispose();

                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                }

                File.Move(uploadingFileName, fullFilePath);
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, null, MethodName(), "Error on moving file. " + ex.ToString());
            }
        }

        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        public string GetParamMsg(string _title, string _from, string _to)
        {
            try
            {
                return @", " + _title + " from [" + _from + "] => [" + _to + "] ";
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, null, MethodName(), "Error on generate system message from to. " + ex.ToString());
                return null;
            }
        }

        // Message when insert new data
        public string GetParamMsg(string _title, string _data)
        {
            string msg = string.Empty;

            try
            {
                msg = @", " + _title + ": [" + _data + "]";
                return msg;
            }
            catch (Exception ex)
            {
                WebApiLog(LogType.ERROR_TYPE, null, MethodName(), "Failed to generate system message. " + ex.ToString());
                return null;
            }
        }

        public DataTable DataTableSetColumnsOrder(DataTable table, String[] columnNames)
        {
            string methodName = MethodName();
            try
            {
                int columnIndex = 0;
                foreach (var columnName in columnNames)
                {
                    if (columnName != null)
                    {
                        if (columnName != string.Empty)
                        {
                            table.Columns[columnName].SetOrdinal(columnIndex);
                            columnIndex++;
                        }
                    }
                }
                return table;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public SqlCommand InsertUserActivityLog(SqlCommand _nonQueryTransaction, string _historyGuid, UserActivity _fromSource, string _action, string _systemMessage, string _updateID, DateTime _currentDatetime)
        {
            SqlCommand nonQueryTransaction = _nonQueryTransaction;
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();

            Hashtable SqlParams = new Hashtable();

            string GUID = Guid.NewGuid().ToString();

            try
            {
                SqlParams["@guid"] = GUID;
                SqlParams["@historyGuid"] = _historyGuid;
                SqlParams["@source"] = _fromSource.SOURCE;
                SqlParams["@moduleID"] = _fromSource.MODULE_ID;
                SqlParams["@action"] = _action;
                SqlParams["@systemMessage"] = _systemMessage;
                SqlParams["@updateId"] = _updateID;
                SqlParams["@updateDatetime"] = _currentDatetime;
                

                if (nonQueryTransaction != null)
                {
                    string sql = @"
                    INSERT INTO USER_ACTIVITY_LOG (GUID, HISTORY_GUID, SOURCE, MODULE_ID, ACTION, SYSTEM_MESSAGE, UPDATE_DATETIME, UPDATE_ID) 
                    VALUES (@guid, @historyGuid, @source, @moduleID, @action, @systemMessage, @updateDatetime, @updateId) ";

                    nonQueryTransaction = daSQL.ExecuteNonQuery(MethodName(), _updateID, sql, SqlParams, nonQueryTransaction, "Api");
                }
            }
            catch (Exception ex)
            {
                string ErrorMsg = @"
                Error on Insert User Activity Log." + "\r\n" + @"
                GUID: " + GUID + @"
                History Guid: " + _historyGuid + @"
                Source: " + _fromSource.SOURCE + @"
                Module ID: " + _fromSource.MODULE_ID + @"
                Action: " + _action + @"
                System Message: " + _systemMessage + @"
                Update Datetime: " + _currentDatetime.ToString() + @"
                Update ID: " + _updateID;

                WebApiLog(LogType.ERROR_TYPE, null, MethodName(), ErrorMsg + ex.ToString());

                nonQueryTransaction = null;
            }

            return nonQueryTransaction;
        }
    }
}