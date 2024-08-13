using DatabaseAccessor.DatabaseAccessor;
using Logger.Logging;
using MY_WEBSITE_API.Classes;
using MY_WEBSITE_API.Controllers.Common;
using MY_WEBSITE_API.Models.Common;
using MY_WEBSITE_API.Models.Web.UserTabletSetup;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Data;
using System.IO;
using System.Net.Http.Headers;

namespace MY_WEBSITE_API.Controllers.Web.UserTabletSetup
{
    [RoutePrefix("api/department")]
    public class DepartmentController : ApiController
    {
        private const string API_TYPE = "Api";
        private const string PAGE_NAME = "Department Maintenance";

        #region Http Get
        [HttpGet]
        [Route("GetAllDepartments")]
        public HttpResponseMessage GetAllDepartments(string _sysUser, string _departmentName)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string sql = @"SELECT DM.DEPARTMENT_ID, DM.DEPARTMENT_NAME, DM.DEPARTMENT_DESC, DM.DEPARTMENT_ACTIVE_FLAG, (SU.USER_ID + ' - ' + SU.USER_NAME) AS UPDATE_ID, DM.UPDATE_DATETIME 
                                FROM DEPT_MST DM LEFT JOIN USERS SU
                                    ON DM.UPDATE_ID = SU.USER_ID
                                WHERE DM.DEPARTMENT_DELETE_FLAG = 'N'
                                ";

                if ((!string.IsNullOrEmpty(_departmentName)) && _departmentName != "undefined")
                {
                    sqlParams["@departmentName"] = _departmentName;
                    sql += @" AND DM.DEPARTMENT_NAME LIKE ('%'+@departmentName+'%')";
                }
                sql += "ORDER BY DM.UPDATE_DATETIME DESC";

                IEnumerable<Department> returnVal = daSQL.ExecuteQuery<Department>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        foreach (Department r in returnVal)
                            r.UPDATE_DATETIME = r.UPDATE_DATETIME.AddTicks(-(r.UPDATE_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get user department list";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _sysUser, apiCommon.MethodName(), ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.Message;

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpGet]
        [Route("ExportExcel")]
        public HttpResponseMessage ExportDepartmentExcel([FromUri] string _sysUser, string _departmentName)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            string currentDatetime = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

            try
            {
                string selectDepartmentSql = @"
                                SELECT    
                                    DM.DEPARTMENT_NAME AS [Department Name],
                                    DM.DEPARTMENT_DESC AS [Department Description],
                                    CASE WHEN(DM.DEPARTMENT_ACTIVE_FLAG = 'Y') THEN 'Active' ELSE 'Inactive' END AS [Status],
                                    (SU.USER_ID + ' - ' + SU.USER_NAME) AS [Last Update By],
                                    DM.UPDATE_DATETIME AS [Last Update Date Time]
                                FROM DEPT_MST DM
                                LEFT JOIN USERS SU ON DM.UPDATE_ID = SU.USER_ID
                                WHERE DM.DEPARTMENT_DELETE_FLAG = 'N'
                                ";

                if ((!string.IsNullOrEmpty(_departmentName)) && _departmentName != "undefined")
                {
                    sqlParams["@departmentName"] = _departmentName;
                    selectDepartmentSql += @" AND DM.DEPARTMENT_NAME LIKE ('%'+@departmentName+'%')";
                }
                selectDepartmentSql += "ORDER BY DM.UPDATE_DATETIME DESC";

                DataTable dtResult = daSQL.ExecuteQuery(apiCommon.MethodName(), _sysUser, selectDepartmentSql, sqlParams);

                if (dtResult != null)
                {
                    FileInformation fi = new FileInformation();
                    fi.FILE_NAME = PAGE_NAME + currentDatetime;
                    fi.FILE_TITLE = PAGE_NAME;
                    fi.FILE_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fi.FILE_EXTENSION = ".xlsx";

                    ExportExcelCls EEC = new ExportExcelCls();
                    byte[] fileDataArr = EEC.ExportDataTableToExcel(dtResult, fi);

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StreamContent(new MemoryStream(fileDataArr));
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = fi.FILE_NAME + fi.FILE_EXTENSION;
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(apiCommon.GetFileInfomationType(fi.FILE_NAME + fi.FILE_EXTENSION, _sysUser));

                    return response;
                }
                else
                {
                    error.MESSAGE = "Cannot find departments";

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _sysUser, apiCommon.MethodName(), ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }


        #endregion

        #region Http Post 
        [HttpPost]
        [Route("InsertDepartment")]
        public HttpResponseMessage InsertDepartment([FromBody] Department _department)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _department.UPDATE_ID;
            try
            {
                string departmentHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@departmentHistoryGuid"] = departmentHistoryGuid;
                sqlParams["@departmentName"] = _department.DEPARTMENT_NAME;
                sqlParams["@departmentDesc"] = _department.DEPARTMENT_DESC;
                sqlParams["@activeFlag"] = "Y";
                sqlParams["@deleteFlag"] = "N";
                sqlParams["@updateId"] = userId;
                sqlParams["@updateDatetime"] = currentDatetime;

                string sql = @" SELECT DEPARTMENT_ID FROM DEPT_MST WHERE UPPER(DEPARTMENT_NAME) = UPPER(@departmentName) AND DEPARTMENT_DELETE_FLAG = 'N'";
                IEnumerable<Department> returnValDepartment = daSQL.ExecuteQuery<Department>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValDepartment != null)
                {
                    if (returnValDepartment.Count() == 0)
                    {
                        sqlParams["@departmentId"] = Guid.NewGuid().ToString();
                        sql = @" INSERT INTO DEPT_MST (DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, DEPARTMENT_ACTIVE_FLAG, DEPARTMENT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                        VALUES (@departmentId, @departmentName, NULLIF(@departmentDesc, ''), @activeFlag, @deleteFlag, @updateDatetime, @updateId)";
                    }
                    else
                    {
                        Department errorObj = new Department
                        {
                            DUPLICATE_DEPARTMENT_NAME = true,
                            DEPARTMENT_NAME = _department.DEPARTMENT_NAME
                        };

                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                        }

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                        return response;
                    }

                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO DEPT_MST_HISTORY (GUID, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, DEPARTMENT_ACTIVE_FLAG, DEPARTMENT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    VALUES (@departmentHistoryGuid, @departmentId, @departmentName, NULLIF(@departmentDesc, ''), @activeFlag, @deleteFlag, @updateDatetime, @updateId)";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.INSERT, null, _department);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, departmentHistoryGuid, _department.FROM_SOURCE, UserActions.INSERT, systemMessage, userId, currentDatetime);

                        }
                        else
                        {
                            error.MESSAGE = "Failed to insert user activity log";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert department";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }

                    if (nonQueryTransaction != null)
                    {
                        if (daSQL.EndTransaction(apiCommon.MethodName(), userId, ref nonQueryTransaction))
                        {
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                            return response;
                        }
                        else
                        {
                            error.MESSAGE = "Failed to commit insert department";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert department history";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Error while checking department existance.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }

            }
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);

                }

                string values = JsonConvert.SerializeObject(_department, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _department.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }
        #endregion

        #region Http Put
        [HttpPut]
        [Route("UpdateDepartment")]
        public HttpResponseMessage UpdateDepartment([FromBody] Department _department)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;
            DateTime currentDatetime = DateTime.Now;
            string userId = _department.UPDATE_ID;

            try
            {
                string departmentHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@departmentHistoryGuid"] = departmentHistoryGuid;
                sqlParams["@departmentId"] = _department.DEPARTMENT_ID;
                sqlParams["@departmentName"] = _department.DEPARTMENT_NAME;
                sqlParams["@departmentDesc"] = _department.DEPARTMENT_DESC;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                string sql = @"SELECT DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC FROM DEPT_MST WHERE UPPER(DEPARTMENT_NAME) = UPPER(@departmentName) AND DEPARTMENT_DELETE_FLAG = 'N' ORDER BY [UPDATE_DATETIME] DESC";
                IEnumerable<Department> returnValDepartment = daSQL.ExecuteQuery<Department>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValDepartment != null)
                {
                    if (returnValDepartment.Count() > 0 && (returnValDepartment.First().DEPARTMENT_ID != _department.DEPARTMENT_ID))
                    {
                        Department errorObj = new Department
                        {
                            DUPLICATE_DEPARTMENT_NAME = true,
                            DEPARTMENT_NAME = _department.DEPARTMENT_NAME
                        };

                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                        }

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                        return response;
                    }
                    else
                    {
                        sql = @"SELECT DEPARTMENT_NAME, DEPARTMENT_DESC FROM DEPT_MST WHERE DEPARTMENT_ID = @departmentId";
                        IEnumerable<Department> departmentOri = daSQL.ExecuteQuery<Department>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                        if (departmentOri != null)
                        {
                            sql = @"UPDATE DEPT_MST SET DEPARTMENT_NAME = @departmentName,
                                    DEPARTMENT_DESC = NULLIF(@departmentDesc, ''),
                                    UPDATE_DATETIME = @updateDatetime,
                                    UPDATE_ID = @updateId
                                WHERE DEPARTMENT_ID = @departmentId ";

                            nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                            if (nonQueryTransaction != null)
                            {
                                sql = @"INSERT INTO DEPT_MST_HISTORY (GUID, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, DEPARTMENT_ACTIVE_FLAG, DEPARTMENT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                        SELECT @departmentHistoryGuid, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, DEPARTMENT_ACTIVE_FLAG, DEPARTMENT_DELETE_FLAG, @updateDatetime, @updateId
                                        FROM DEPT_MST WHERE DEPARTMENT_ID = @departmentId ";

                                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                                if (nonQueryTransaction != null)
                                {
                                    string systemMessage = GenerateUserActivityLog(UserActions.EDIT, departmentOri, _department);
                                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, departmentHistoryGuid, _department.FROM_SOURCE, UserActions.EDIT, systemMessage, userId, currentDatetime);

                                    if (nonQueryTransaction != null)
                                    {
                                        if (daSQL.EndTransaction(apiCommon.MethodName(), userId, ref nonQueryTransaction))
                                        {
                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                                            return response;
                                        }
                                        else
                                        {
                                            if (nonQueryTransaction != null)
                                            {
                                                daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                                            }

                                            error.MESSAGE = "Failed to commit update department";

                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                            return response;
                                        }
                                    }
                                    else
                                    {
                                        error.MESSAGE = "Failed to insert user activity log";

                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                }
                                else
                                {
                                    error.MESSAGE = "Failed to insert department history";

                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
                            }
                            else
                            {
                                error.MESSAGE = "Failed to update department";

                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }
                        }
                        else
                        {
                            if (nonQueryTransaction != null)
                            {
                                daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                            }

                            error.MESSAGE = "Failed to get current department data";

                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }


                    }
                }
                else
                {
                    error.MESSAGE = "Failed to find department";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
            }
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                }

                string values = JsonConvert.SerializeObject(_department, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _department.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("UpdateDepartmentStatus")]
        public HttpResponseMessage UpdateDepartmentStatus([FromBody] Department _department)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _department.UPDATE_ID;
            try
            {
                string status = _department.DEPARTMENT_ACTIVE_FLAG; ;
                string departmentHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@departmentHistoryGuid"] = departmentHistoryGuid;
                sqlParams["@departmentId"] = _department.DEPARTMENT_ID;
                sqlParams["@departmentActiveFlag"] = _department.DEPARTMENT_ACTIVE_FLAG;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                if (nonQueryTransaction != null)
                {
                    string sql = @"UPDATE DEPT_MST SET DEPARTMENT_ACTIVE_FLAG = @departmentActiveFlag,
                                    UPDATE_DATETIME = @updateDatetime,
                                    UPDATE_ID = @updateId
                                    WHERE DEPARTMENT_ID = @departmentId ";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO DEPT_MST_HISTORY (GUID, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, DEPARTMENT_ACTIVE_FLAG, DEPARTMENT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    SELECT @departmentHistoryGuid, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, @departmentActiveFlag, DEPARTMENT_DELETE_FLAG, @updateDatetime, @updateId
                                    FROM DEPT_MST WHERE DEPARTMENT_ID = @departmentId ";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.UPDATE_STATUS, null, _department);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, departmentHistoryGuid, _department.FROM_SOURCE, UserActions.UPDATE_STATUS, systemMessage, userId, currentDatetime);
                        }
                        else
                        {
                            error.MESSAGE = "Failed to insert department history.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to update department status.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to connect database.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                if (nonQueryTransaction != null)
                {
                    if (daSQL.EndTransaction(apiCommon.MethodName(), userId, ref nonQueryTransaction))
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                        }

                        error.MESSAGE = "Failed to commit update department status.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to insert user activity log.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
            }
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                }

                string values = JsonConvert.SerializeObject(_department, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _department.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }


        [HttpPut]
        [Route("DeleteDepartment")]
        public HttpResponseMessage DeleteDepartment([FromBody] Department _department)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _department.UPDATE_ID;

            try
            {
                string departmentHistoryGuid = Guid.NewGuid().ToString();

                sqlParams["@departmentHistoryGuid"] = departmentHistoryGuid;
                sqlParams["@departmentId"] = _department.DEPARTMENT_ID;
                sqlParams["@activeFlag"] = "N";
                sqlParams["@deleteFlag"] = "Y";
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                if (nonQueryTransaction != null)
                {
                    string sql = @"UPDATE DEPT_MST SET DEPARTMENT_DELETE_FLAG = @deleteFlag,
                                        UPDATE_DATETIME = @updateDatetime,
                                        UPDATE_ID = @updateId
                                    WHERE DEPARTMENT_ID = @departmentId ";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO DEPT_MST_HISTORY (GUID, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, DEPARTMENT_ACTIVE_FLAG, DEPARTMENT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    SELECT @departmentHistoryGuid, DEPARTMENT_ID, DEPARTMENT_NAME, DEPARTMENT_DESC, @activeFlag, @deleteFlag, @updateDatetime, @updateId
                                    FROM DEPT_MST WHERE DEPARTMENT_ID = @departmentId ";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.DELETE, null, _department);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, departmentHistoryGuid, _department.FROM_SOURCE, UserActions.DELETE, systemMessage, userId, currentDatetime);
                        }
                        else
                        {
                            error.MESSAGE = "Failed to commit delete department.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }

                    }
                    else
                    {
                        error.MESSAGE = "Failed to delete department";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to connect database";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                if (nonQueryTransaction != null)
                {
                    if (daSQL.EndTransaction(apiCommon.MethodName(), userId, ref nonQueryTransaction))
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                        }
                        error.MESSAGE = "Failed to insert user acitivty log.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to insert department history";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
            }
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                }

                string values = JsonConvert.SerializeObject(_department, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _department.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());
                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }
        #endregion

        #region Common
        private string GenerateUserActivityLog(string action, IEnumerable<Department> returnVal, Department _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            string systemMsg = string.Empty;

            try
            {
                if (action.Equals(UserActions.INSERT))
                {
                    systemMsg += @"Insert department: [" + _values.DEPARTMENT_NAME + "]";
                    systemMsg += apiCommon.GetParamMsg("department desc", _values.DEPARTMENT_DESC);
                }
                else if (action.Equals(UserActions.EDIT))
                {
                    Department currObj = returnVal.First();

                    systemMsg += @"Edit department: [" + currObj.DEPARTMENT_NAME + "]";

                    if (currObj.DEPARTMENT_NAME != _values.DEPARTMENT_NAME)
                    {
                        systemMsg += apiCommon.GetParamMsg("department name", currObj.DEPARTMENT_NAME, _values.DEPARTMENT_NAME);
                    }

                    if (currObj.DEPARTMENT_DESC != _values.DEPARTMENT_DESC)
                    {
                        systemMsg += apiCommon.GetParamMsg("department desc", currObj.DEPARTMENT_DESC, _values.DEPARTMENT_DESC);
                    }
                }
                else if (action == UserActions.UPDATE_STATUS)
                {
                    string title = (_values.DEPARTMENT_ACTIVE_FLAG == "Y") ? "Activate" : "Deactivate";
                    systemMsg += title + " department name: [" + _values.DEPARTMENT_NAME + "]";

                    string currStatus = (_values.DEPARTMENT_ACTIVE_FLAG == "Y") ? "Active" : "Inactive";
                    string newStatus = (_values.DEPARTMENT_ACTIVE_FLAG == "Y") ? "Inactive" : "Active";

                    systemMsg += apiCommon.GetParamMsg("status", currStatus, newStatus);
                }
                else if (action == UserActions.DELETE)
                {
                    systemMsg += @"Delete delete: [" + _values.DEPARTMENT_NAME + "]";
                }

                return systemMsg;
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _values.UPDATE_ID, apiCommon.MethodName(), ex.ToString());
                return null;
            }
        }
        #endregion
    }
}