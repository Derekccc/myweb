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
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using DatabaseAccessor.DatabaseAccessor;

namespace MY_WEBSITE_API.Controllers.Web.UserTabletSetup
{
    [RoutePrefix("api/role")]
    public class RoleController : ApiController

    {
        private const string API_TYPE = "Api";
        private const string PAGE_NAME = "Role Maintenance";

        #region Http Get
        [HttpGet]
        [Route("GetAllRoles")]
        public HttpResponseMessage GetAllRoles(string _sysUser, string _roleName)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string sql = @"SELECT RM.ROLE_ID, RM.ROLE_NAME, RM.ROLE_DESC, RM.ROLE_ACTIVE_FLAG, (SU.USER_ID + ' - ' + SU.USER_NAME) AS UPDATE_ID, RM.UPDATE_DATETIME 
                                FROM ROLE_MST RM LEFT JOIN USERS SU
                                    ON RM.UPDATE_ID = SU.USER_ID
                                WHERE RM.ROLE_DELETE_FLAG = 'N'
                                
                                ";

                #region Filter
                if ((!string.IsNullOrEmpty(_roleName)) && _roleName != "undefined" )
                {
                    sqlParams["@roleName"] = _roleName;
                    sql += @"
                            AND RM.[ROLE_NAME] LIKE ('%'+@roleName+'%')
                            ";
                }
                sql += "ORDER BY RM.UPDATE_DATETIME DESC";
                #endregion

                IEnumerable<Role> returnVal = daSQL.ExecuteQuery<Role>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        foreach (Role r in returnVal)
                            r.UPDATE_DATETIME = r.UPDATE_DATETIME.AddTicks(-(r.UPDATE_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get user role list";
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
        public HttpResponseMessage ExportExcel([FromUri] string _sysUser, string _roleName)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            string currentDatetime = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

            try
            {
                string selectRoleSql = @"SELECT RM.ROLE_NAME AS [Role Name],
                                            RM.ROLE_DESC AS [Role Description],
                                            CASE WHEN(RM.ROLE_ACTIVE_FLAG = 'Y') THEN 'Active' ELSE 'Inactive' END AS [Status],
                                            (SU.USER_ID + ' - ' + SU.USER_NAME) AS [Last Update By],
                                            RM.UPDATE_DATETIME AS [Last Update Date Time]
                                        FROM ROLE_MST RM
                                        LEFT JOIN USERS SU
                                        ON RM.UPDATE_ID = SU.USER_ID
                                        WHERE RM.ROLE_DELETE_FLAG = 'N'
                                        ";

                if ((!string.IsNullOrEmpty(_roleName)) && _roleName != "undefined")
                {
                    sqlParams["@roleName"] = _roleName;
                    selectRoleSql += @" AND RM.ROLE_NAME LIKE ('%'+@roleName+'%')";
                }
                selectRoleSql += "ORDER BY RM.UPDATE_DATETIME DESC";

                DataTable dtResult = daSQL.ExecuteQuery(apiCommon.MethodName(), _sysUser, selectRoleSql, sqlParams);
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
                    error.MESSAGE = "Cannot find roles";

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
        [Route("InsertRole")]
        public HttpResponseMessage InsertRole([FromBody] Role _role)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _role.UPDATE_ID;
            try
            { 
                string roleHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@roleHistoryGuid"] = roleHistoryGuid;
                sqlParams["@roleName"] = _role.ROLE_NAME;
                sqlParams["@roleDesc"] = _role.ROLE_DESC;  
                sqlParams["@activeFlag"] = "Y";
                sqlParams["@deleteFlag"] = "N";
                sqlParams["@updateId"] = userId;
                sqlParams["@updateDatetime"] = currentDatetime;

                string sql = @" SELECT ROLE_ID FROM ROLE_MST WHERE UPPER(ROLE_NAME) = UPPER(@roleName) AND ROLE_DELETE_FLAG = 'N'";
                IEnumerable<Role> returnValRole = daSQL.ExecuteQuery<Role>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValRole != null)
                {
                    if (returnValRole.Count() == 0)
                    {
                        sqlParams["@roleId"] = Guid.NewGuid().ToString();
                        sql = @" INSERT INTO ROLE_MST (ROLE_ID, ROLE_NAME, ROLE_DESC, ROLE_ACTIVE_FLAG, ROLE_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                        VALUES (@roleId, @roleName, NULLIF(@roleDesc, ''), @activeFlag, @deleteFlag, @updateDatetime, @updateId)";
                    }
                    else
                    {
                        Role errorObj = new Role
                        {
                            DUPLICATE_ROLE_NAME = true,
                            ROLE_NAME = _role.ROLE_NAME
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
                        sql = @"INSERT INTO ROLE_MST_HISTORY (GUID, ROLE_ID, ROLE_NAME, ROLE_DESC, ROLE_ACTIVE_FLAG, ROLE_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    VALUES (@roleHistoryGuid, @roleId, @roleName, NULLIF(@roleDesc, ''), @activeFlag, @deleteFlag, @updateDatetime, @updateId)";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.INSERT, null, _role);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, roleHistoryGuid, _role.FROM_SOURCE, UserActions.INSERT, systemMessage, userId, currentDatetime);

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
                        error.MESSAGE = "Failed to insert role";
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
                            error.MESSAGE = "Failed to commit insert role";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert role history";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Error while checking role existance.";
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

                string values = JsonConvert.SerializeObject(_role, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _role.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

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
        [Route("UpdateRole")]
        public HttpResponseMessage UpdateRole([FromBody] Role _role)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;
            DateTime currentDatetime = DateTime.Now;
            string userId = _role.UPDATE_ID;

            try
            {
                string roleHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@roleHistoryGuid"] = roleHistoryGuid;
                sqlParams["@roleId"] = _role.ROLE_ID;
                sqlParams["@roleName"] = _role.ROLE_NAME;
                sqlParams["@roleDesc"] = _role.ROLE_DESC;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                string sql = @"SELECT ROLE_ID, ROLE_NAME, ROLE_DESC FROM ROLE_MST WHERE UPPER(ROLE_NAME) = UPPER(@roleName) AND ROLE_DELETE_FLAG = 'N' ORDER BY [UPDATE_DATETIME] DESC";
                IEnumerable<Role> returnValRole = daSQL.ExecuteQuery<Role>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValRole != null)
                {
                    if (returnValRole.Count() > 0 && (returnValRole.First().ROLE_ID != _role.ROLE_ID))
                    {
                        Role errorObj = new Role
                        {
                            DUPLICATE_ROLE_NAME = true,
                            ROLE_NAME = _role.ROLE_NAME
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
                        sql = @"SELECT ROLE_NAME, ROLE_DESC FROM ROLE_MST WHERE ROLE_ID = @roleId";
                        IEnumerable<Role> roleOri = daSQL.ExecuteQuery<Role>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                        if (roleOri != null)
                        {
                            sql = @"UPDATE ROLE_MST SET ROLE_NAME = @roleName,
                                    ROLE_DESC = NULLIF(@roleDesc, ''),
                                    UPDATE_DATETIME = @updateDatetime,
                                    UPDATE_ID = @updateId
                                WHERE ROLE_ID = @roleId ";

                            nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                            if (nonQueryTransaction != null)
                            {
                                sql = @"INSERT INTO ROLE_MST_HISTORY (GUID, ROLE_ID, ROLE_NAME, ROLE_DESC, ROLE_ACTIVE_FLAG, ROLE_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                        SELECT @roleHistoryGuid, ROLE_ID, ROLE_NAME, ROLE_DESC, ROLE_ACTIVE_FLAG, ROLE_DELETE_FLAG, @updateDatetime, @updateId
                                        FROM ROLE_MST WHERE ROLE_ID = @roleId ";

                                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                                if (nonQueryTransaction != null)
                                {
                                    string systemMessage = GenerateUserActivityLog(UserActions.EDIT, roleOri, _role);
                                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, roleHistoryGuid, _role.FROM_SOURCE, UserActions.EDIT, systemMessage, userId, currentDatetime);

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

                                            error.MESSAGE = "Failed to commit update role";

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
                                    error.MESSAGE = "Failed to insert role history";

                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
                            }
                            else
                            {
                                error.MESSAGE = "Failed to update role";

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

                            error.MESSAGE = "Failed to get current role data";

                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }


                    }
                }
                else
                {
                    error.MESSAGE = "Failed to find role";
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

                string values = JsonConvert.SerializeObject(_role, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _role.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("UpdateRoleStatus")]
        public HttpResponseMessage UpdateRoleStatus([FromBody] Role _role)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _role.UPDATE_ID;
            try
            {
                string status = _role.ROLE_ACTIVE_FLAG; ;
                string roleHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@roleHistoryGuid"] = roleHistoryGuid;
                sqlParams["@roleId"] = _role.ROLE_ID;
                sqlParams["@roleActiveFlag"] = _role.ROLE_ACTIVE_FLAG;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;
                
                if (nonQueryTransaction != null)
                {
                    string sql = @"UPDATE ROLE_MST SET ROLE_ACTIVE_FLAG = @roleActiveFlag,
                                    UPDATE_DATETIME = @updateDatetime,
                                    UPDATE_ID = @updateId
                                    WHERE ROLE_ID = @roleId ";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO ROLE_MST_HISTORY (GUID, ROLE_ID, ROLE_NAME, ROLE_DESC, ROLE_ACTIVE_FLAG, ROLE_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    SELECT @roleHistoryGuid, ROLE_ID, ROLE_NAME, ROLE_DESC, @roleActiveFlag, ROLE_DELETE_FLAG, @updateDatetime, @updateId
                                    FROM ROLE_MST WHERE ROLE_ID = @roleId ";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.UPDATE_STATUS, null, _role);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, roleHistoryGuid, _role.FROM_SOURCE, UserActions.UPDATE_STATUS, systemMessage, userId, currentDatetime);
                        }
                        else
                        {
                            error.MESSAGE = "Failed to insert role history.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to update role status.";
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

                        error.MESSAGE = "Failed to commit update role status.";
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

                string values = JsonConvert.SerializeObject(_role, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _role.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("DeleteRole")]
        public HttpResponseMessage DeleteRole([FromBody] Role _role)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _role.UPDATE_ID;

            try
            {
                string roleHistoryGuid = Guid.NewGuid().ToString();

                sqlParams["@roleHistoryGuid"] = roleHistoryGuid;
                sqlParams["@roleId"] = _role.ROLE_ID;
                sqlParams["@activeFlag"] = "N";
                sqlParams["@deleteFlag"] = "Y";
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                if (nonQueryTransaction != null)
                {
                    string sql = @"UPDATE ROLE_MST SET ROLE_DELETE_FLAG = @deleteFlag,
                                        UPDATE_DATETIME = @updateDatetime,
                                        UPDATE_ID = @updateId
                                    WHERE ROLE_ID = @roleId ";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO ROLE_MST_HISTORY (GUID, ROLE_ID, ROLE_NAME, ROLE_DESC, ROLE_ACTIVE_FLAG, ROLE_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    SELECT @roleHistoryGuid, ROLE_ID, ROLE_NAME, ROLE_DESC, @activeFlag, @deleteFlag, @updateDatetime, @updateId
                                    FROM ROLE_MST WHERE ROLE_ID = @roleId ";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.DELETE, null, _role);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, roleHistoryGuid, _role.FROM_SOURCE, UserActions.DELETE, systemMessage, userId, currentDatetime);
                        }
                        else
                        {
                            error.MESSAGE = "Failed to commit delete role.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }

                    }
                    else
                    {
                        error.MESSAGE = "Failed to delete role";
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
                    error.MESSAGE = "Failed to insert role history";
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

                string values = JsonConvert.SerializeObject(_role, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _role.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());
                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }
        #endregion

        #region Common
        private string GenerateUserActivityLog(string action, IEnumerable<Role> returnVal, Role _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            string systemMsg = string.Empty;

            try
            {
                if (action.Equals(UserActions.INSERT))
                {
                    systemMsg += @"Insert role: [" + _values.ROLE_NAME + "]";
                    systemMsg += apiCommon.GetParamMsg("role desc", _values.ROLE_DESC);
                }
                else if (action.Equals(UserActions.EDIT))
                {
                    Role currObj = returnVal.First();

                    systemMsg += @"Edit role: [" + currObj.ROLE_NAME + "]";

                    if (currObj.ROLE_NAME != _values.ROLE_NAME)
                    {
                        systemMsg += apiCommon.GetParamMsg("role name", currObj.ROLE_NAME, _values.ROLE_NAME);
                    }

                    if (currObj.ROLE_DESC != _values.ROLE_DESC)
                    {
                        systemMsg += apiCommon.GetParamMsg("role desc", currObj.ROLE_DESC, _values.ROLE_DESC);
                    }
                }
                else if (action == UserActions.UPDATE_STATUS)
                {
                    string title = (_values.ROLE_ACTIVE_FLAG == "Y") ? "Activate" : "Deactivate";
                    systemMsg += title + " role name: [" + _values.ROLE_NAME + "]";

                    string currStatus = (_values.ROLE_ACTIVE_FLAG == "Y") ? "Active" : "Inactive";
                    string newStatus = (_values.ROLE_ACTIVE_FLAG == "Y") ? "Inactive" : "Active";

                    systemMsg += apiCommon.GetParamMsg("status", currStatus, newStatus);
                }
                else if (action == UserActions.DELETE)
                {
                    systemMsg += @"Delete role: [" + _values.ROLE_NAME + "]";
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