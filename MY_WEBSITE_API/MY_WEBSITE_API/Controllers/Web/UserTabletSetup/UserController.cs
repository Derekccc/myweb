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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace MY_WEBSITE_API.Controllers.Web
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        private const string API_TYPE = "Api";
        private const string PAGE_NAME = "User Maintenance";

        #region Http Get
        [HttpGet]
        [Route("GetAllUsers")]
        public HttpResponseMessage GetAllUsers(string _userId, string _role, string _department, string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string sql = @"
                    SELECT DISTINCT SU.[USER_ID],
                                    SU.[USER_NAME],
                                    SU.[EMAIL], 
                                    SU.[USERROLE_ID],
                                    (SELECT a.[ROLE_NAME] 
                                     FROM [ROLE_MST] a 
                                     WHERE a.[ROLE_ID] = SU.[USERROLE_ID]) AS [USER_CATEGORY],
				                    SU.[DEPT_ID],
				                    (SELECT b.[DEPARTMENT_NAME] 
				                     FROM [DEPT_MST] b 
                                     WHERE b.[DEPARTMENT_ID] = SU.[DEPT_ID]) AS [USER_DEPT_CATEGORY],

                                    SU.[ACCOUNT_LOCK_FLAG], 
                                    SU.[LAST_ACCESS_DATETIME], 
                                    SU.[ACTIVE_FLAG],
                                    (SELECT (U.[USER_ID] + ' - ' + U.[USER_NAME]) 
                                     FROM [USERS] U 
                                     WHERE U.[USER_ID] = SU.[UPDATE_ID]) AS [UPDATE_ID],
                                    SU.[UPDATE_DATETIME],

                                    (SELECT STRING_AGG(UR.[ROLE_ID], ',')
                                     WITHIN GROUP (ORDER BY RM.[ROLE_NAME])
                                     FROM [USERROLE] UR
                                     JOIN [ROLE_MST] RM ON RM.[ROLE_ID] = UR.[ROLE_ID]
                                     WHERE UR.[USER_ID] = SU.[USER_ID]) AS [ROLE_ID],

                                    (SELECT STRING_AGG(RM.[ROLE_NAME], ',')
                                     WITHIN GROUP (ORDER BY RM.[ROLE_NAME])
                                     FROM [USERROLE] UR
                                     JOIN [ROLE_MST] RM ON RM.[ROLE_ID] = UR.[ROLE_ID]
                                     WHERE UR.[USER_ID] = SU.[USER_ID]) AS [ROLE_NAME],

                                    (SELECT STRING_AGG(DP.[DEPARTMENT_ID], ',')
                                     WITHIN GROUP (ORDER BY DM.[DEPARTMENT_NAME])
                                     FROM [DEPT] DP
                                     JOIN [DEPT_MST] DM ON DM.[DEPARTMENT_ID] = DP.[DEPARTMENT_ID]
                                     WHERE DP.[USER_ID] = SU.[USER_ID]) AS [DEPARTMENT_ID],

                                    (SELECT STRING_AGG(DM.[DEPARTMENT_NAME], ',')
                                     WITHIN GROUP (ORDER BY DM.[DEPARTMENT_NAME])
                                     FROM [DEPT] DP
                                     JOIN [DEPT_MST] DM ON DM.[DEPARTMENT_ID] = DP.[DEPARTMENT_ID]
                                     WHERE DP.[USER_ID] = SU.[USER_ID]) AS [DEPARTMENT_NAME]
 
                                    FROM [USERS] SU
                                    LEFT JOIN [USERROLE] UR ON SU.[USER_ID] = UR.[USER_ID]
                                    LEFT JOIN [ROLE_MST] RM ON UR.[ROLE_ID] = RM.[ROLE_ID]
                                    LEFT JOIN [DEPT] DP ON SU.[USER_ID] = DP.[USER_ID]
                                    LEFT JOIN [DEPT_MST] DM ON DP.[DEPARTMENT_ID] = DM.[DEPARTMENT_ID]
                                    WHERE SU.[DELETE_FLAG] = 'N'
                ";

                #region Filter
                if ((!string.IsNullOrEmpty(_userId)) && _userId != "undefined")
                {
                    sqlParams["@userId"] = _userId;
                    sql += @" AND SU.USER_ID LIKE ('%'+@userId+'%')";
                }

                if (_role != null && _role != "" && _role != "undefined")
                {
                    List<string> roles = _role.Split(',').ToList();
                    roles.ForEach(x => sqlParams["@role" + roles.IndexOf(x)] = x);
                    sql += " AND UR.[ROLE_ID] IN (@role0";
                    for (int i = 1; i < roles.Count; i++)
                    {
                        sql += String.Concat(", @role", i);
                    }
                    sql += ") ";
                }
                if (_department != null && _department != "" && _department != "undefined")
                {
                    List<string> departments = _department.Split(',').ToList();
                    departments.ForEach(x => sqlParams["@department" + departments.IndexOf(x)] = x);
                    sql += " AND DP.[DEPARTMENT_ID] IN (@department0";
                    for (int i = 1; i < departments.Count; i++)
                    {
                        sql += String.Concat(", @department", i);
                    }
                    sql += ") ";
                }
                sql += " ORDER BY SU.[USER_ID] ";
                #endregion

                IEnumerable<User> returnVal = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);
                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        foreach (User u in returnVal)
                        {
                            u.UPDATE_DATETIME = u.UPDATE_DATETIME.AddTicks(-(u.UPDATE_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                            u.LAST_ACCESS_DATETIME = u.LAST_ACCESS_DATETIME.AddTicks(-(u.LAST_ACCESS_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                        }

                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get user";
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
        [Route("GetRoleListFilter")]
        public HttpResponseMessage GetRoleListFilter(string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string selectRoleSql = @"
                SELECT RM.ROLE_ID AS [ROLE_ID], RM.ROLE_NAME as [ROLE_NAME]
                FROM ROLE_MST RM
                WHERE RM.ROLE_DELETE_FLAG = 'N'
                ORDER BY RM.ROLE_NAME ";

                IEnumerable<User> returnVal = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), _sysUser, selectRoleSql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get role name";
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
        [Route("GetRoleList")]
        public HttpResponseMessage GetRoleList(string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string selectRoleSql = @"
                SELECT RM.ROLE_ID AS [ROLE_ID], RM.ROLE_NAME as [ROLE_NAME]
                FROM ROLE_MST RM
                WHERE RM.ROLE_DELETE_FLAG = 'N'
                AND RM.ROLE_ACTIVE_FLAG = 'Y'
                ORDER BY RM.ROLE_NAME ";

                IEnumerable<User> returnVal = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), _sysUser, selectRoleSql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get role name";
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
        [Route("GetDepartmentList")]
        public HttpResponseMessage GetDepartmentList(string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string selectDepartmentSql = @"
                 SELECT DM.DEPARTMENT_ID AS [DEPARTMENT_ID], DM.DEPARTMENT_NAME AS [DEPARTMENT_NAME]
                 FROM DEPT_MST DM
                 WHERE DM.DEPARTMENT_DELETE_FLAG = 'N'
                 AND DM.DEPARTMENT_ACTIVE_FLAG = 'Y'
                 ORDER BY DM.DEPARTMENT_NAME;
                ";

                IEnumerable<User> returnVal = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), _sysUser, selectDepartmentSql, sqlParams, API_TYPE);

                if (returnVal != null )
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get role name";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
            } catch (Exception ex) 
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
        public HttpResponseMessage ExportExcel([FromUri] string _userId, string _role, string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            string currentDatetime = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

            try
            {
                string sql = @"
                SELECT DISTINCT
                    SU.USER_ID AS [User ID],
                    SU.USER_NAME AS [User Name],
                    SU.EMAIL AS [Email],
                    (SELECT a.[ROLE_NAME] FROM [ROLE_MST] a WHERE a.[ROLE_ID] = SU.[USERROLE_ID]) AS [User Category],
                    (SELECT DISTINCT STRING_AGG([ROLE_NAME], ', ')
                        WITHIN GROUP (ORDER BY [ROLE_NAME])
                        FROM (SELECT DISTINCT UR.[USER_ID], UR.[ROLE_ID], RM.[ROLE_NAME]
                                FROM [USERROLE] UR, [ROLE_MST] RM
                             WHERE UR.[ROLE_ID] = RM.[ROLE_ID]
                                AND UR.[USER_ID] = SU.[USER_ID]) x
                    )AS [Role],
                    CASE WHEN(SU.ACTIVE_FLAG = 'Y') THEN 'Active' ELSE 'Inactive' END AS [Status],
                    SU.UPDATE_ID AS [Update By],
                    SU.UPDATE_DATETIME AS [Last Update Date Time]
                FROM USERS SU LEFT JOIN USERROLE UR ON SU.[USER_ID] = UR.[USER_ID], [ROLE_MST] RM
                WHERE SU.DELETE_FLAG = 'N' ";

                if ((!string.IsNullOrEmpty(_userId)) && _userId != "undefined")
                {
                    sqlParams["@userId"] = _userId;
                    sql += @" AND SU.USER_ID LIKE ('%'+@userId+'%')";
                }
                if (_role != null && _role != "" && _role != "undefined")
                {
                    List<string> roles = _role.Split(',').ToList();
                    roles.ForEach(x => sqlParams["@role" + roles.IndexOf(x)] = x);
                    sql += " AND UR.[ROLE_ID] IN (@role0";
                    for (int i = 1; i < roles.Count; i++)
                    {
                        sql += String.Concat(", @role", i);
                    }
                    sql += ") ";
                }
                sql += " ORDER BY SU.[USER_ID] ";

                DataTable dtResult = daSQL.ExecuteQuery(apiCommon.MethodName(), _sysUser, sql, sqlParams);

                if (dtResult != null)
                {
                    FileInformation fi = new FileInformation();
                    fi.FILE_NAME = PAGE_NAME + currentDatetime;
                    fi.FILE_TITLE = PAGE_NAME;
                    fi.FILE_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fi.FILE_EXTENSION = ".xlsx";

                    ExportExcelCls EEC = new ExportExcelCls();
                    byte[] fileDataArr = EEC.ExportDataTableToExcel(dtResult, fi);

                    HttpResponseMessage response = null;

                    response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StreamContent(new MemoryStream(fileDataArr));
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = fi.FILE_NAME + fi.FILE_EXTENSION;
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(apiCommon.GetFileInfomationType(fi.FILE_NAME + fi.FILE_EXTENSION, _sysUser));

                    return response;
                }
                else
                {
                    error.MESSAGE = "Cannot find user";

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
        [Route("RegisterUser")]
        public HttpResponseMessage RegisterUser([FromBody] User _user)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            string sql = string.Empty;

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _user.UPDATE_ID;
            try
            {
                List<string> selectedRoleList = _user.USER_ROLE_LIST ?? new List<string>();
                List<string> selectedDepartmentList = _user.USER_DEPARTMENT_LIST ?? new List<string>();
                string password = Password.encryptedPass(_user.USER_ID, _user.DEFAULT_PASSWORD);

                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());
                string usersHistoryGuid = Guid.NewGuid().ToString();

                sqlParams["@usersHistoryGuid"] = usersHistoryGuid;
                sqlParams["@userId"] = _user.USER_ID;
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@userName"] = _user.USER_NAME;
                sqlParams["@userRoleId"] = _user.USERROLE_ID;
                sqlParams["@deptId"] = _user.DEPT_ID;
                sqlParams["@email"] = _user.EMAIL;
                sqlParams["@password"] = password;
                sqlParams["@updateId"] = userId;
                sqlParams["@yesFlag"] = "Y";
                sqlParams["@noFlag"] = "N";
                sqlParams["@loginFailCount"] = 0;

                sql = @"SELECT [USER_ID], [DELETE_FLAG] FROM [USERS] WHERE UPPER([USER_ID]) = UPPER(@userId)";
                IEnumerable<User> returnValUser = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                if (returnValUser != null)
                {
                    if (returnValUser.Count() == 0)
                    {
                        sql = @"INSERT INTO USERS ([USER_ID], [USER_NAME], [USERROLE_ID], [DEPT_ID], [EMAIL], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG], [DELETE_FLAG],
                                    [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG], [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD], [UPDATE_DATETIME], [UPDATE_ID])
                                VALUES (@userId, @userName, NULLIF(@userRoleId, ''), NULLIF(@deptId, ''), @email, @noFlag, @yesFlag, @noFlag, 
                                    @updateDateTime, @loginFailCount, @noFlag, @updateDateTime, @yesFlag, @password, @updateDateTime, @updateId)";
                    }
                    else
                    {
                        if (returnValUser.First().DELETE_FLAG == "Y")
                        {
                            sql = @"UPDATE USERS SET [USER_NAME] = @userName, [USERROLE_ID] = NULLIF(@userRoleId, ''), [DEPT_ID] = NULLIF(@deptId, ''), [EMAIL] = @email, [ERROR_EMAIL_FLAG] = @noFlag, [ACTIVE_FLAG] = @yesFlag, [DELETE_FLAG] = @noFlag, 
                                        [LAST_ACCESS_DATETIME] = @updateDateTime, [LOGIN_FAIL_COUNT] = @loginFailCount, [ACCOUNT_LOCK_FLAG] = @noFlag, [RESET_DATETIME] = @updateDateTime, [RESET_FLAG] =@yesFlag, 
                                        [DEFAULT_PASSWORD] = @password, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateId WHERE UPPER([USER_ID]) = UPPER(@userId)";
                        }
                        else
                        {
                            User errorObj = new User
                            {
                                DUPLICATE_USER_ID = true,
                                USER_ID = _user.USER_ID
                            };

                            if (nonQueryTransaction != null)
                            {
                                daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                            }

                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                            return response;
                        }
                    }

                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                    [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                    [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                    [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                    [UPDATE_DATETIME], [UPDATE_ID])
                                        SELECT @usersHistoryGuid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                            [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                            [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                            [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                            [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                        WHERE [USER_ID] = @userId";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    }
                    else
                    {
                        error.MESSAGE = "Error while inserting user info.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }

                    if (nonQueryTransaction != null)
                    {
                        if (selectedRoleList.Count() > 0)
                        {
                            IEnumerable<User> dbUserRole = RegisteredRoles(daSQL, nonQueryTransaction, sqlParams, userId);
                            if (dbUserRole != null)
                            {
                                nonQueryTransaction = UpdateUserRole(daSQL, nonQueryTransaction, sqlParams, dbUserRole, selectedRoleList, userId, UserActions.INSERT);
                            }
                            else
                            {
                                if (nonQueryTransaction != null)
                                {
                                    daSQL.EndTransactionRollback(apiCommon.MethodName(), _user.USER_ID, ref nonQueryTransaction);
                                }
                                error.MESSAGE = "Failed to check user role.";
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;

                            }
                            if (nonQueryTransaction != null)
                            {
                                string systemMessage = GenerateUserActivityLog(UserActions.INSERT, null, _user);
                                nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _user.FROM_SOURCE, UserActions.INSERT, systemMessage, userId, currentDatetime);
                            }
                            else
                            {
                                error.MESSAGE = "Failed to insert user role.";
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }
                            if (selectedDepartmentList.Count() > 0)
                            {
                                IEnumerable<User> dbUserDepartments = RegisteredDepartments(daSQL, nonQueryTransaction, sqlParams, userId);
                                if (dbUserDepartments != null)
                                {
                                    nonQueryTransaction = UpdateUserDepartment(daSQL, nonQueryTransaction, sqlParams, dbUserDepartments, selectedDepartmentList, userId, UserActions.INSERT);
                                }
                                else
                                {
                                    if (nonQueryTransaction != null)
                                    {
                                        daSQL.EndTransactionRollback(apiCommon.MethodName(), _user.USER_ID, ref nonQueryTransaction);
                                    }
                                    error.MESSAGE = "Failed to check user departments.";
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }

                                if (nonQueryTransaction != null)
                                {
                                    string systemMessage = GenerateUserActivityLog(UserActions.INSERT, null, _user);
                                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _user.FROM_SOURCE, UserActions.INSERT, systemMessage, userId, currentDatetime);
                                }
                                else
                                {
                                    error.MESSAGE = "Failed to insert user departments.";
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
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
                                    error.MESSAGE = "Failed to commit user register.";
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
                        else
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

                                error.MESSAGE = "Failed to commit user register.";
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert user history.";
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
                    error.MESSAGE = "Error while checking user Id existance.";
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

                string values = JsonConvert.SerializeObject(_user, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _user.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

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
        [Route("UpdateUser")]
        public HttpResponseMessage UpdateUser([FromBody] User _user)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            string sql = string.Empty;

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _user.UPDATE_ID;
            try
            {
                string usersHistoryGuid = Guid.NewGuid().ToString();
                List<string> selectedRoleList = _user.USER_ROLE_LIST ?? new List<string>();
                List<string> selectedDepartmentList = _user.USER_DEPARTMENT_LIST ?? new List<string>();

                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());
                sqlParams["@guid"] = usersHistoryGuid;
                sqlParams["@userId"] = _user.USER_ID;
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@userName"] = _user.USER_NAME;
                sqlParams["@userRoleId"] = _user.USERROLE_ID;
                sqlParams["@deptId"] = _user.DEPT_ID;
                sqlParams["@email"] = _user.EMAIL;
                sqlParams["@updateId"] = userId;

                sql = @"SELECT [USER_ID], [USER_NAME], [EMAIL], [USERROLE_ID], [DEPT_ID], [DEFAULT_PASSWORD] FROM [USERS] WHERE UPPER([USER_ID]) = UPPER(@userId) ORDER BY [UPDATE_DATETIME] DESC";
                IEnumerable<User> userOri = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                if (userOri != null)
                {
                    sql = @"UPDATE [USERS] SET [USER_NAME] = @userName, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID, [EMAIL] = @email, [USERROLE_ID] = @userRoleId, [DEPT_ID] = @deptId
                                        WHERE [USER_ID] = @userId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                [UPDATE_DATETIME], [UPDATE_ID])
                                    SELECT @guid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                        [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                        [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                        [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                        [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                    WHERE [USER_ID] = @userId";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.EDIT, userOri, _user);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _user.FROM_SOURCE, UserActions.EDIT, systemMessage, userId, currentDatetime);
                        }
                        else
                        {
                            error.MESSAGE = "Failed to insert system user information history.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                        if (nonQueryTransaction != null)
                        {
                            IEnumerable<User> dbUserRole = RegisteredRoles(daSQL, nonQueryTransaction, sqlParams, userId);
                            if (dbUserRole != null)
                            {
                                nonQueryTransaction = UpdateUserRole(daSQL, nonQueryTransaction, sqlParams, dbUserRole, selectedRoleList, userId, UserActions.EDIT);
                                if (nonQueryTransaction != null)
                                {
                                    if (nonQueryTransaction != null)
                                    {
                                        IEnumerable<User> dbUserDepartment = RegisteredDepartments(daSQL, nonQueryTransaction, sqlParams, userId);
                                        if (dbUserDepartment != null)
                                        {
                                            nonQueryTransaction = UpdateUserDepartment(daSQL, nonQueryTransaction, sqlParams, dbUserDepartment, selectedDepartmentList, userId, UserActions.EDIT);
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
                                                    error.MESSAGE = "Failed to commit user update.";
                                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                                    return response;
                                                }
                                            }
                                            else
                                            {
                                                if (nonQueryTransaction != null)
                                                {
                                                    daSQL.EndTransactionRollback(apiCommon.MethodName(), _user.USER_ID, ref nonQueryTransaction);
                                                }
                                                error.MESSAGE = "Failed to update user department.";
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
                                            error.MESSAGE = "Department of the user not found";
                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                            return response;
                                        }
                                    }

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
                                        error.MESSAGE = "Failed to commit user update.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                }
                                else
                                {
                                    if (nonQueryTransaction != null)
                                    {
                                        daSQL.EndTransactionRollback(apiCommon.MethodName(), _user.USER_ID, ref nonQueryTransaction);
                                    }
                                    error.MESSAGE = "Failed to update user role.";
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
                                error.MESSAGE = "Role of the user not found";
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
                            error.MESSAGE = "Failed to insert user activity log.";
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
                        error.MESSAGE = "Failed to update user information.";
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
                    error.MESSAGE = "Failed to find existing user.";
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

                string values = JsonConvert.SerializeObject(_user, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _user.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("UpdateUserStatus")]
        public HttpResponseMessage UpdateUserStatus([FromBody] User _user)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _user.UPDATE_ID;
            try
            {
                string sysUserHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@guid"] = sysUserHistoryGuid;
                sqlParams["@userId"] = _user.USER_ID;
                sqlParams["@activeFlag"] = _user.ACTIVE_FLAG;
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@updateId"] = userId;


                string sql = @"UPDATE [USERS] SET [ACTIVE_FLAG] = @activeFlag, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID
                                        WHERE [USER_ID] = @userId";
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                if (nonQueryTransaction != null)
                {
                    sql = @"INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                            [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                            [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                            [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                            [UPDATE_DATETIME], [UPDATE_ID])
                                SELECT @guid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                    [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                    [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                    [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                    [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                WHERE [USER_ID] = @userId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        string systemMessage = GenerateUserActivityLog(UserActions.UPDATE_STATUS, null, _user);
                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, sysUserHistoryGuid, _user.FROM_SOURCE, UserActions.UPDATE_STATUS, systemMessage, userId, currentDatetime);
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert system user information history.";
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
                            error.MESSAGE = "Failed to commit user status update.";
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
                        error.MESSAGE = "Failed to insert user activity log.";
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
                    error.MESSAGE = "Failed to update system user status.";
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

                string values = JsonConvert.SerializeObject(_user, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, userId, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("UnlockUser")]
        public HttpResponseMessage UnlockUser([FromBody] User _user)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _user.UPDATE_ID;
            try
            {
                string sysUserHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@guid"] = sysUserHistoryGuid;
                sqlParams["@userId"] = _user.USER_ID;
                sqlParams["@resetFailCount"] = 0;
                sqlParams["@noFlag"] = "N";
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@updateId"] = userId;


                string sql = @"UPDATE [USERS] SET [LOGIN_FAIL_COUNT] = @resetFailCount, [ACCOUNT_LOCK_FLAG] = @noFlag, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID
                                        WHERE [USER_ID] = @userId";
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                if (nonQueryTransaction != null)
                {
                    sql = @"INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                            [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                            [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                            [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                            [UPDATE_DATETIME], [UPDATE_ID])
                                SELECT @guid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                    [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                    [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                    [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                    [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                WHERE [USER_ID] = @userId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        string systemMessage = GenerateUserActivityLog("Unlock", null, _user);
                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, sysUserHistoryGuid, _user.FROM_SOURCE, "Unlock User", systemMessage, userId, currentDatetime);
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert user information history.";
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
                            error.MESSAGE = "Failed to commit user status update.";
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
                        error.MESSAGE = "Failed to insert user activity log.";
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
                    error.MESSAGE = "Failed to update system user status.";
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

                string values = JsonConvert.SerializeObject(_user, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, userId, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("ResetPassword")]
        public HttpResponseMessage ResetPassword([FromBody] User _user)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            string sql = string.Empty;

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _user.UPDATE_ID;

            try
            {
                string password = Password.encryptedPass(_user.USER_ID, _user.DEFAULT_PASSWORD);
                string usersHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@guid"] = usersHistoryGuid;
                sqlParams["@userId"] = _user.USER_ID;
                sqlParams["@email"] = _user.EMAIL;
                sqlParams["@yesFlag"] = "Y";
                sqlParams["@noFlag"] = "N";
                sqlParams["@password"] = password;
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@updateId"] = userId;
                sqlParams["@loginFailCount"] = 0;

                sql = @"SELECT [USER_ID], [USER_NAME], [EMAIL], [USERROLE_ID], [DEPT_ID], [DEFAULT_PASSWORD] FROM [USERS] WHERE [USER_ID] = @userId";
                IEnumerable<User> userOri = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                if (userOri != null)
                {
                    sql = @"UPDATE [USERS] SET [LOGIN_FAIL_COUNT] = @loginFailCount, [ACCOUNT_LOCK_FLAG] = @noFlag, [RESET_DATETIME] = @updateDateTime, [RESET_FLAG] = @yesFlag, [DEFAULT_PASSWORD] = @password, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID
                                WHERE [USER_ID] = @userId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction == null)
                    {
                        error.MESSAGE = "Failed to reset system user password.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                    else
                    {
                        sql = @"INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                [UPDATE_DATETIME], [UPDATE_ID])
                                    SELECT @guid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                        [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                        [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                        [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                        [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                    WHERE [USER_ID] = @userId";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    }
                    if (nonQueryTransaction == null)
                    {
                        error.MESSAGE = "Failed to insert system user information history.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                    else
                    {
                        string systemMessage = GenerateUserActivityLog("RESET_PASSWORD", userOri, _user);
                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _user.FROM_SOURCE, "Reset Password", systemMessage, userId, currentDatetime);
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
                            error.MESSAGE = "Failed to commit user password reset.";
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
                        error.MESSAGE = "Failed to insert user activity log.";
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
                    error.MESSAGE = "Failed to find existing user.";
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

                string values = JsonConvert.SerializeObject(_user, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, userId, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("DeleteUser")]
        public HttpResponseMessage DeleteUser([FromBody] User _user)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _user.UPDATE_ID;
            try
            {
                string usersHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@guid"] = usersHistoryGuid;
                sqlParams["@userId"] = _user.USER_ID;
                sqlParams["@deleteFlag"] = "Y";
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@updateId"] = userId;

                nonQueryTransaction = UpdateUserRole(daSQL, nonQueryTransaction, sqlParams, null, null, userId, UserActions.DELETE);
                if (nonQueryTransaction != null)
                {
                    string sql = @"UPDATE [USERS] SET [DELETE_FLAG] = @deleteFlag, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID
                                            WHERE [USER_ID] = @userId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                [UPDATE_DATETIME], [UPDATE_ID])
                                    SELECT @guid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                        [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                        [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                        [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                        [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                    WHERE [USER_ID] = @userId";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.DELETE, null, _user);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _user.FROM_SOURCE, UserActions.DELETE, systemMessage, userId, currentDatetime);
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
                                    error.MESSAGE = "Failed to commit user delete.";
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
                                error.MESSAGE = "Failed to insert user activity log.";
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }
                        }
                        else
                        {
                            error.MESSAGE = "Failed to insert system user information history.";
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
                        error.MESSAGE = "Failed to delete system user.";
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
                    error.MESSAGE = "Failed to delete user registered role.";
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

                string values = JsonConvert.SerializeObject(_user, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, userId, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }
        #endregion

        #region Common
        private string GenerateUserActivityLog(string action, IEnumerable<User> returnVal, User _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string systemMsg = string.Empty;

            try
            {
                if (action.Equals(UserActions.INSERT))
                {
                    systemMsg += @"Insert user: [" + _values.USER_ID + "]";
                    systemMsg += apiCommon.GetParamMsg("user name", _values.USER_NAME);
                }
                else if (action.Equals(UserActions.EDIT))
                {
                    User currObj = returnVal.First();
                    systemMsg += @"Edit user: [" + currObj.USER_NAME + "]";

                    if (currObj.USER_NAME != _values.USER_NAME)
                    {
                        systemMsg += apiCommon.GetParamMsg("user name", currObj.USER_NAME, _values.USER_NAME);
                    }
                    if (currObj.EMAIL != _values.EMAIL)
                    {
                        systemMsg += apiCommon.GetParamMsg("email", currObj.EMAIL, _values.EMAIL);
                    }
                    if (currObj.USERROLE_ID != _values.USERROLE_ID && (!string.IsNullOrEmpty(currObj.USERROLE_ID) || !string.IsNullOrEmpty(_values.USERROLE_ID)))
                    {
                        systemMsg += apiCommon.GetParamMsg("user category", currObj.USERROLE_ID, _values.USERROLE_ID);
                    }
                    if (currObj.DEPT_ID != _values.DEPT_ID && (!string.IsNullOrEmpty(currObj.DEPT_ID) || !string.IsNullOrEmpty(_values.DEPT_ID)))
                    {
                        systemMsg += apiCommon.GetParamMsg("user category", currObj.DEPT_ID, _values.DEPT_ID);
                    }
                }
                else if (action == UserActions.UPDATE_STATUS)
                {
                    string title = (_values.ACTIVE_FLAG == "Y") ? "Activate" : "Deactivate";
                    systemMsg += title + " user: [" + _values.USER_NAME + "]";

                    string currStatus = (_values.ACTIVE_FLAG == "Y") ? "Inactive" : "Active";
                    string newStatus = (_values.ACTIVE_FLAG == "Y") ? "Active" : "Inactive";

                    systemMsg += apiCommon.GetParamMsg("status", currStatus, newStatus);
                }
                else if (action == UserActions.DELETE)
                {
                    systemMsg += @"Delete user: [" + _values.USER_NAME + "]";
                }
                else if (action == "Unlock")
                {
                    systemMsg += @"Unlock user: [" + _values.USER_NAME + "]";
                }
                else
                {
                    User currObj = returnVal.First();
                    systemMsg += @"Reset user default password: [" + currObj.USER_NAME + "]";
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

        private SqlCommand UpdateUserRole(DatabaseAccessorMSSQL daSQL, SqlCommand nonQueryTransaction, Hashtable sqlParams, IEnumerable<User> dbUserRole,  List<string> selectedRole, string userId, string action)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string sql = string.Empty;

            List<string> newList = new List<string>();
            List<string> toBeDeleteList = new List<string>();

            if (action == UserActions.INSERT)
            {
                List<string> dbRoleId = dbUserRole == null ? new List<string>() : dbUserRole.Select(r => r.ROLE_ID).ToList();
                List<string> selectedRoleId = selectedRole ?? new List<string>();

                newList = selectedRoleId;
                toBeDeleteList = dbRoleId ?? new List<string>();

                newList.ForEach(x => sqlParams["@newRoleId" + newList.IndexOf(x)] = x);
                toBeDeleteList.ForEach(x => sqlParams["@toBeDeleteId" + toBeDeleteList.IndexOf(x)] = x);
            }
            else
            {
                List<string> dbRoleId = dbUserRole == null ? new List<string>() : dbUserRole.Select(r => r.ROLE_ID).ToList();
                List<string> selectedRoleId = selectedRole ?? new List<string>();

                newList = dbRoleId.Count() > 0 ? selectedRoleId.Except(dbRoleId).ToList() : selectedRoleId;
                toBeDeleteList = dbRoleId.Count() > 0 ? dbRoleId.Except(selectedRoleId).ToList() : new List<string>();

                newList.ForEach(x => sqlParams["@newRoleId" + newList.IndexOf(x)] = x);
                toBeDeleteList.ForEach(x => sqlParams["@toBeDeleteId" + toBeDeleteList.IndexOf(x)] = x);
            }


            if (newList.Count() > 0)
            {
                sqlParams["@action"] = "ADD";
                sqlParams["@userRoleGuid0"] = Guid.NewGuid().ToString();
                sqlParams["@userRoleAddHistoryGuid0"] = Guid.NewGuid().ToString();
                sql = @" BEGIN
                            INSERT INTO [USERROLE] ([USERROLE_ID], [USER_ID], [ROLE_ID], [UPDATE_DATETIME], [UPDATE_ID]) VALUES (@userRoleGuid0, @userId, @newRoleId0, @updateDateTime, @updateId); 
                            INSERT INTO [USERROLE_HISTORY] ([GUID], [USERROLE_ID], [USER_ID], [ROLE_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userRoleAddHistoryGuid0, [USERROLE_ID], [USER_ID], [ROLE_ID], @action, [UPDATE_DATETIME], [UPDATE_ID] FROM [USERROLE] WHERE [USERROLE_ID] = @userRoleGuid0;";
                for (int i = 1; i < newList.Count; i++)
                {
                    sqlParams["@userRoleGuid" + i] = Guid.NewGuid().ToString();
                    sqlParams["@userRoleAddHistoryGuid" + i] = Guid.NewGuid().ToString();
                    sql += String.Concat(@"
                            INSERT INTO [USERROLE] ([USERROLE_ID], [USER_ID], [ROLE_ID], [UPDATE_DATETIME], [UPDATE_ID]) VALUES (@userRoleGuid", i, @", @userId, @newRoleId", i, @", @updateDateTime, @updateId); 
                            INSERT INTO [USERROLE_HISTORY] ([GUID], [USERROLE_ID], [USER_ID], [ROLE_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userRoleAddHistoryGuid", i, @", [USERROLE_ID], [USER_ID], [ROLE_ID], @action, [UPDATE_DATETIME], [UPDATE_ID] FROM [USERROLE] WHERE [USERROLE_ID] = @userRoleGuid", i, ";");
                }
                sql += @"
                        END;";
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
            }

            if (nonQueryTransaction != null)
            {
                if (toBeDeleteList.Count() > 0)
                {
                    sql = @"SELECT [USERROLE_ID] FROM [USERROLE] WHERE [USER_ID] = @userId AND [ROLE_ID] IN (@toBeDeleteId0";
                    for (int i = 1; i < toBeDeleteList.Count; i++)
                    {
                        sql += String.Concat(", @toBeDeleteId" + i);
                    }
                    sql += ")";
                    IEnumerable<User> toBeDeleteUserRole = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                    if (toBeDeleteUserRole != null)
                    {
                        List<string> toBeDeleteRole = toBeDeleteUserRole.Count() == 0 ? new List<string>() : toBeDeleteUserRole.Select(u => u.USERROLE_ID).ToList();
                        toBeDeleteRole.ForEach(x => sqlParams["@toBeDeleteRoleId" + toBeDeleteRole.IndexOf(x)] = x);
                        sqlParams["@action"] = "DELETE";
                        sqlParams["@userRoleDeleteHistoryGuid0"] = Guid.NewGuid().ToString();
                        sql = @"BEGIN
                            INSERT INTO [USERROLE_HISTORY] ([GUID], [USERROLE_ID], [USER_ID], [ROLE_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userRoleDeleteHistoryGuid0, [USERROLE_ID], [USER_ID], [ROLE_ID], @action, @updateDateTime, @updateId FROM [USERROLE] WHERE [USERROLE_ID] = @toBeDeleteRoleId0;";
                        for (int i = 1; i < toBeDeleteRole.Count; i++)
                        {
                            sqlParams["@userRoleDeleteHistoryGuid" + i] = Guid.NewGuid().ToString();
                            sql += String.Concat(@"
                            INSERT INTO [USERROLE_HISTORY] ([GUID], [USERROLE_ID], [USER_ID], [ROLE_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userRoleDeleteHistoryGuid", i, ", [USERROLE_ID], [USER_ID], [ROLE_ID], @action, @updateDateTime, @updateId FROM [USERROLE] WHERE [USERROLE_ID] = @toBeDeleteRoleId", i, ";");
                        }
                        sql += @"DELETE FROM [USERROLE] WHERE [USERROLE_ID] IN (@toBeDeleteRoleId0";
                        for (int i = 1; i < toBeDeleteRole.Count; i++)
                        {
                            sql += String.Concat(@", @toBeDeleteRoleId", i);
                        }
                        sql += @")
                            END;";

                    }
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                }
            }
            return nonQueryTransaction;
        }

        // For update user department
        private SqlCommand UpdateUserDepartment(DatabaseAccessorMSSQL daSQL, SqlCommand nonQueryTransaction, Hashtable sqlParams, IEnumerable<User> dbUserDepartment, List<string> selectedDepartment, string userId, string action)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string sql = string.Empty;

            List<string> newList = new List<string>();
            List<string> toBeDeleteList = new List<string>();

            if (action == UserActions.INSERT)
            {
                List<string> dbDepartmentId = dbUserDepartment == null ? new List<string>() : dbUserDepartment.Select(r => r.DEPARTMENT_ID).ToList();
                List<string> selectedDepartmentId = selectedDepartment ?? new List<string>();

                newList = selectedDepartmentId;
                toBeDeleteList = dbDepartmentId ?? new List<string>();

                newList.ForEach(x => sqlParams["@newDepartmentId" + newList.IndexOf(x)] = x);
                toBeDeleteList.ForEach(x => sqlParams["@toBeDeleteId" + toBeDeleteList.IndexOf(x)] = x);
            }
            else
            {
                List<string> dbDepartmentId = dbUserDepartment == null ? new List<string>() : dbUserDepartment.Select(r => r.DEPARTMENT_ID).ToList();
                List<string> selectedDepartmentId = selectedDepartment ?? new List<string>();

                newList = dbDepartmentId.Count() > 0 ? selectedDepartmentId.Except(dbDepartmentId).ToList() : selectedDepartmentId;
                toBeDeleteList = dbDepartmentId.Count() > 0 ? dbDepartmentId.Except(selectedDepartmentId).ToList() : new List<string>();

                newList.ForEach(x => sqlParams["@newDepartmentId" + newList.IndexOf(x)] = x);
                toBeDeleteList.ForEach(x => sqlParams["@toBeDeleteId" + toBeDeleteList.IndexOf(x)] = x);
            }


            if (newList.Count() > 0)
            {
                sqlParams["@action"] = "ADD";
                sqlParams["@userDepartmentGuid0"] = Guid.NewGuid().ToString();
                sqlParams["@userDepartmentAddHistoryGuid0"] = Guid.NewGuid().ToString();
                sql = @" BEGIN
                            INSERT INTO [DEPT] ([DEPT_ID], [USER_ID], [DEPARTMENT_ID], [UPDATE_DATETIME], [UPDATE_ID]) VALUES (@userDepartmentGuid0, @userId, @newDepartmentId0, @updateDateTime, @updateId); 
                            INSERT INTO [DEPT_HISTORY] ([GUID], [DEPT_ID], [USER_ID], [DEPARTMENT_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userDepartmentAddHistoryGuid0, [DEPT_ID], [USER_ID], [DEPARTMENT_ID], @action, [UPDATE_DATETIME], [UPDATE_ID] FROM [DEPT] WHERE [DEPT_ID] = @userDepartmentGuid0;";
                for (int i = 1; i < newList.Count; i++)
                {
                    sqlParams["@userDepartmentGuid" + i] = Guid.NewGuid().ToString();
                    sqlParams["@userDepartmentAddHistoryGuid" + i] = Guid.NewGuid().ToString();
                    sql += String.Concat(@"
                            INSERT INTO [DEPT] ([DEPT_ID], [USER_ID], [DEPARTMENT_ID], [UPDATE_DATETIME], [UPDATE_ID]) VALUES (@userDepartmentGuid", i, @", @userId, @newDepartmentId", i, @", @updateDateTime, @updateId); 
                            INSERT INTO [DEPT_HISTORY] ([GUID], [DEPT_ID], [USER_ID], [DEPARTMENT_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userDepartmentAddHistoryGuid", i, @", [DEPT_ID], [USER_ID], [DEPARTMENT_ID], @action, [UPDATE_DATETIME], [UPDATE_ID] FROM [DEPT] WHERE [DEPT_ID] = @userDepartmentGuid", i, ";");
                }
                sql += @"
                        END;";
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
            }

            if (nonQueryTransaction != null)
            {
                if (toBeDeleteList.Count() > 0)
                {
                    sql = @"SELECT [DEPT_ID] FROM [DEPT] WHERE [USER_ID] = @userId AND [DEPARTMENT_ID] IN (@toBeDeleteId0";
                    for (int i = 1; i < toBeDeleteList.Count; i++)
                    {
                        sql += String.Concat(", @toBeDeleteId" + i);
                    }
                    sql += ")";
                    IEnumerable<User> toBeDeleteUserDepartment = daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                    if (toBeDeleteUserDepartment != null)
                    {
                        List<string> toBeDeleteDepartment = toBeDeleteUserDepartment.Count() == 0 ? new List<string>() : toBeDeleteUserDepartment.Select(u => u.DEPT_ID).ToList();
                        toBeDeleteDepartment.ForEach(x => sqlParams["@toBeDeleteDepartmentId" + toBeDeleteDepartment.IndexOf(x)] = x);
                        sqlParams["@action"] = "DELETE";
                        sqlParams["@userDepartmentDeleteHistoryGuid0"] = Guid.NewGuid().ToString();
                        sql = @"BEGIN
                            INSERT INTO [DEPT_HISTORY] ([GUID], [DEPT_ID], [USER_ID], [DEPARTMENT_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userDepartmentDeleteHistoryGuid0, [DEPT_ID], [USER_ID], [DEPARTMENT_ID], @action, @updateDateTime, @updateId FROM [DEPT] WHERE [DEPT_ID] = @toBeDeleteDepartmentId0;";
                        for (int i = 1; i < toBeDeleteDepartment.Count; i++)
                        {
                            sqlParams["@userDepartmentDeleteHistoryGuid" + i] = Guid.NewGuid().ToString();
                            sql += String.Concat(@"
                            INSERT INTO [DEPT_HISTORY] ([GUID], [DEPT_ID], [USER_ID], [DEPARTMENT_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID]) 
                             SELECT @userDepartmentDeleteHistoryGuid", i, ", [DEPT_ID], [USER_ID], [DEPARTMENT_ID], @action, @updateDateTime, @updateId FROM [DEPT] WHERE [DEPT_ID] = @toBeDeleteDepartmentId", i, ";");
                        }
                        sql += @"DELETE FROM [DEPT] WHERE [DEPT_ID] IN (@toBeDeleteDepartmentId0";
                        for (int i = 1; i < toBeDeleteDepartment.Count; i++)
                        {
                            sql += String.Concat(@", @toBeDeleteDepartmentId", i);
                        }
                        sql += @")
                            END;";

                    }
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                }
            }
            return nonQueryTransaction;
        }

        private IEnumerable<User> RegisteredRoles(DatabaseAccessorMSSQL daSQL, SqlCommand nonQueryTransaction, Hashtable sqlParams, string userId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string sql = @"SELECT DISTINCT(TEMP.[ROLES]) AS [ROLE_ID]
                        FROM USERROLE UR LEFT JOIN 
                        (SELECT DISTINCT(ROLE_ID) AS [ROLES], [USER_ID] FROM USERROLE WHERE [USER_ID] = @userId) TEMP
                        ON UR.[USER_ID] = TEMP.[USER_ID] WHERE TEMP.[ROLES] IS NOT NULL; ";
            return daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
        }

        private IEnumerable<User> RegisteredDepartments(DatabaseAccessorMSSQL daSQL, SqlCommand nonQueryTransaction, Hashtable sqlParams, string userId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string sql = @"SELECT DISTINCT(TEMP.[DEPARTMENTS]) AS [DEPARTMENT_ID]
                        FROM DEPT DP LEFT JOIN 
                        (SELECT DISTINCT(DEPARTMENT_ID) AS [DEPARTMENTS], [USER_ID] FROM DEPT WHERE [USER_ID] = @userId) TEMP
                        ON DP.[USER_ID] = TEMP.[USER_ID] WHERE TEMP.[DEPARTMENTS] IS NOT NULL; ";
            return daSQL.ExecuteQuery<User>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
        }

    }
}