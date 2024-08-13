using DatabaseAccessor.DatabaseAccessor;
using Logger.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using MY_WEBSITE_API.Models.Web;
using MY_WEBSITE_API.Controllers.Common;
using MY_WEBSITE_API.Models.Common;
using MY_WEBSITE_API.Classes;

namespace MY_WEBSITE_API.Controllers.Web
{
    [RoutePrefix("api/login")]
    public class LoginController : ApiController
    {
        private const string API_TYPE = "Api";

        #region Http Post 
        [HttpPost]
        [Route("UserLogin")]
        public HttpResponseMessage UserLogin([FromBody] Login _login)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string currentMethod = apiCommon.MethodName();
            string userId = _login.USER_ID;

            try
            {
                string password = Password.encryptedPass(userId, _login.PASSWORD);
                string usersHistoryGuid = Guid.NewGuid().ToString();

                nonQueryTransaction = daSQL.BeginTransaction(currentMethod);

                sqlParams["@usersHistoryGuid"] = usersHistoryGuid;

                sqlParams["@userId"] = userId;
                sqlParams["@accessDateTime"] = currentDatetime;
                sqlParams["@yesFlag"] = "Y";
                sqlParams["@noFlag"] = "N";
                sqlParams["@resetFailCount"] = 0;

                string checkUserIdSql = @"SELECT [USER_ID], [PASSWORD], [USER_NAME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG], [RESET_FLAG], [RESET_DATETIME], [DEFAULT_PASSWORD], [ACTIVE_FLAG] 
                                FROM [USERS] WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";

                IEnumerable<Login> userVal = daSQL.ExecuteQuery<Login>(currentMethod, userId, checkUserIdSql, sqlParams, API_TYPE);

                if (userVal == null)
                {
                    if (nonQueryTransaction != null)
                    {
                        daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                    }
                    error.MESSAGE = "Failed to get user info.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                else
                {
                    if (!userVal.Any())//no user found
                    {
                        Login errorObj = new Login
                        {
                            VALID = "LOGIN_FAILED",
                            USER_ID = userId,
                            ERROR_MSG = string.Concat(userId, " not exist."),
                        };
                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                        }
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                        return response;
                    }
                    else
                    {
                        Login currUserObj = userVal.First();
                        string insertHistorySql = @"
                                    INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                        [DEPT_ID], [EMAIL], [ACTIVE_FLAG],
                                        [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                        [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                        [UPDATE_DATETIME], [UPDATE_ID])
                                    SELECT @usersHistoryGuid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                        [DEPT_ID], [EMAIL], [ACTIVE_FLAG],
                                        [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                        [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                        [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                    WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";

                        #region Check Is Power User
                        if (IsPowerUser(userId))
                        {
                            if (SafeEqual(password, currUserObj.PASSWORD))
                            {
                                string updateUserLoginSql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @resetFailCount WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, updateUserLoginSql, sqlParams, nonQueryTransaction, API_TYPE);
                                if (nonQueryTransaction == null)
                                {
                                    error.MESSAGE = "Failed to login.";
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
                                nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, insertHistorySql, sqlParams, nonQueryTransaction, API_TYPE);

                                if (nonQueryTransaction == null)
                                {
                                    error.MESSAGE = "Failed to insert system user history.";
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
                                string systemMessage = GenerateUserActivityLog("LOGIN_SUCCESS", currUserObj, _login);
                                nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _login.FROM_SOURCE, "Login Success", systemMessage, userId, currentDatetime);

                                if (nonQueryTransaction == null)
                                {
                                    error.MESSAGE = "Failed to insert user activity log.";
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }

                                if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                {
                                    Login successObj = new Login
                                    {
                                        VALID = "LOGIN_SUCCESS",
                                        USER_ID = currUserObj.USER_ID,
                                        USER_NAME = currUserObj.USER_NAME,
                                        AUTO_LOGOUT_DURATION = 500 //mins
                                    };
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, successObj);
                                    return response;
                                }
                                else
                                {
                                    if (nonQueryTransaction != null)
                                    {
                                        daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                    }
                                    error.MESSAGE = "Failed to commit user login info.";
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
                            }
                        }

                        #endregion

                        if (currUserObj.ACCOUNT_LOCK_FLAG == "Y") // account locked
                        {
                            Login errorObj = new Login
                            {
                                VALID = "ACCOUNT_LOCK",
                                USER_NAME = currUserObj.USER_NAME,
                                ERROR_MSG = string.Concat(currUserObj.USER_NAME, " account is locked. Please contact IT department."),
                            };
                            if (nonQueryTransaction != null)
                            {
                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                            }
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                            return response;
                        }
                        if (currUserObj.ACTIVE_FLAG == "N") // account not active
                        {
                            Login errorObj = new Login
                            {
                                VALID = "LOGIN_FAILED",
                                USER_NAME = currUserObj.USER_NAME,
                                ERROR_MSG = string.Concat(currUserObj.USER_NAME, " account is inactive. Please contact IT department."),
                            };
                            if (nonQueryTransaction != null)
                            {
                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                            }
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                            return response;
                        }
                        else
                        {
                            string selectLoginLimitsSql = @"
                                SELECT [LOGIN_FAIL_COUNT], [PASSWORD_AGE], [AUTO_LOGOUT_DURATION] FROM
                                    (SELECT CONVERT (INT, [POLICY_VALUE]) AS [LOGIN_FAIL_COUNT] FROM [POLICY_MST] WHERE [POLICY_ID] = 'LOGIN_FAIL' AND [DELETE_FLAG] = 'N') [LoginFail],
                                    (SELECT CONVERT (INT, [POLICY_VALUE]) AS [PASSWORD_AGE] FROM [POLICY_MST] WHERE [POLICY_ID] = 'PASSWORD_AGE' AND [DELETE_FLAG] = 'N') [PasswordAge],
                                    (SELECT CONVERT (INT, [POLICY_VALUE]) AS [AUTO_LOGOUT_DURATION] FROM [POLICY_MST] WHERE [POLICY_ID] = 'WEB_LOCKOUT_DURATION' AND [DELETE_FLAG] = 'N') [LogoutDuration];";
                            IEnumerable<Login> limitations = daSQL.ExecuteQuery<Login>(currentMethod, userId, selectLoginLimitsSql, sqlParams, API_TYPE);

                            if (limitations == null)
                            {
                                if (nonQueryTransaction != null)
                                {
                                    daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                }
                                error.MESSAGE = "Failed to get login attemp limit.";
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }

                            Login limit = limitations.First(); //login limitations                       

                            sqlParams["@loginFailCount"] = currUserObj.LOGIN_FAIL_COUNT + 1;
                            #region User need to reset password
                            if (currUserObj.RESET_FLAG == "Y")
                            {
                                // password matched
                                if (SafeEqual(password, currUserObj.DEFAULT_PASSWORD))
                                {
                                    #region Login direct to reset password
                                    string updateResetFlagsql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @resetFailCount WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, updateResetFlagsql, sqlParams, nonQueryTransaction, API_TYPE);
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to login.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, insertHistorySql, sqlParams, nonQueryTransaction, API_TYPE);

                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert system user history.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    string systemMessage = GenerateUserActivityLog("LOGIN_SUCCESS", currUserObj, _login);
                                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _login.FROM_SOURCE, "Login Success", systemMessage, userId, currentDatetime);


                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert user activity log.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                    {
                                        Login successObj = new Login
                                        {
                                            VALID = "RESET",
                                            USER_ID = currUserObj.USER_ID,
                                            USER_NAME = currUserObj.USER_NAME,
                                        };
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, successObj);
                                        return response;
                                    }
                                    else
                                    {
                                        if (nonQueryTransaction != null)
                                        {
                                            daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                        }
                                        error.MESSAGE = "Failed to commit user login info.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    #endregion
                                }
                    
                                // password not matched
                                else
                                {
                                    #region Login Failed
                                    string updateFailSql = string.Empty;
                                    if (currUserObj.LOGIN_FAIL_COUNT == (limit.LOGIN_FAIL_COUNT - 1))
                                    {
                                        updateFailSql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @loginFailCount, [ACCOUNT_LOCK_FLAG] = @yesFlag WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    }
                                    else
                                    {
                                        updateFailSql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @loginFailCount WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    }
                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, updateFailSql, sqlParams, nonQueryTransaction, API_TYPE);
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to login.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, insertHistorySql, sqlParams, nonQueryTransaction, API_TYPE);

                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert system user history.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    string systemMessage = GenerateUserActivityLog("LOGIN_FAILED", currUserObj, _login);
                                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _login.FROM_SOURCE, "Login Fail", systemMessage, userId, currentDatetime);

                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert user activity log.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                    {
                                        if (currUserObj.LOGIN_FAIL_COUNT == (limit.LOGIN_FAIL_COUNT - 1))
                                        {
                                            Login errorObj = new Login
                                            {
                                                VALID = "LOGIN_FAILED",
                                                USER_ID = currUserObj.USER_ID,
                                                USER_NAME = currUserObj.USER_NAME,
                                                ERROR_MSG = string.Concat(currUserObj.USER_NAME, " account is locked. Please contact IT department."),
                                            };

                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                                            return response;
                                        }
                                        else
                                        {
                                            Login errorObj = new Login
                                            {
                                                VALID = "LOGIN_FAILED",
                                                USER_ID = currUserObj.USER_ID,
                                                USER_NAME = currUserObj.USER_NAME,
                                                ERROR_MSG = string.Concat((currUserObj.LOGIN_FAIL_COUNT + 1).ToString(), "/", limit.LOGIN_FAIL_COUNT.ToString(), "Invalid login attemps."),
                                            };

                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                                            return response;
                                        }
                                    }
                                    else
                                    {
                                        if (nonQueryTransaction != null)
                                        {
                                            daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                        }
                                        error.MESSAGE = "Failed to commit user login info.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }

                                    #endregion
                                }
                            }
                            #endregion

                            #region user no need to reset password
                            else
                            {
                                // password matched
                                if (SafeEqual(password, currUserObj.PASSWORD))
                                {
                                    #region Login successful
                                    string updateUserLoginSql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @resetFailCount WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, updateUserLoginSql, sqlParams, nonQueryTransaction, API_TYPE);
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to login.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                    else
                                    {
                                        nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, insertHistorySql, sqlParams, nonQueryTransaction, API_TYPE);
                                    }
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert system user history.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                    else
                                    {
                                        string systemMessage = GenerateUserActivityLog("LOGIN_SUCCESS", currUserObj, _login);
                                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _login.FROM_SOURCE, "Login Success", systemMessage, userId, currentDatetime);

                                    }
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert user activity log.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                    else
                                    {
                                        if (currentDatetime.Date >= (currUserObj.RESET_DATETIME.Date.AddDays(limit.PASSWORD_AGE)))
                                        {
                                            if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                            {
                                                Login successObj = new Login
                                                {
                                                    VALID = "RESET",
                                                    USER_ID = currUserObj.USER_ID,
                                                    USER_NAME = currUserObj.USER_NAME,
                                                    ERROR_MSG = "Password expired, please change your password.",
                                                };
                                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, successObj);
                                                return response;
                                            }
                                            else
                                            {
                                                if (nonQueryTransaction != null)
                                                {
                                                    daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                                }
                                                error.MESSAGE = "Failed to commit user login info.";
                                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                                return response;
                                            }

                                        }

                                        if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                        {
                                            Login successObj = new Login
                                            {
                                                VALID = "LOGIN_SUCCESS",
                                                USER_ID = currUserObj.USER_ID,
                                                USER_NAME = currUserObj.USER_NAME,
                                                AUTO_LOGOUT_DURATION = limit.AUTO_LOGOUT_DURATION
                                            };
                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, successObj);
                                            return response;
                                        }
                                        else
                                        {
                                            if (nonQueryTransaction != null)
                                            {
                                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                            }
                                            error.MESSAGE = "Failed to commit user login info.";
                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                            return response;
                                        }
                                    }
                                    #endregion
                                }
                                // password not matched
                                else
                                {
                                    #region Login Failed
                                    string updateLoginFailSql = string.Empty;
                                    //fail count reached limits
                                    if (currUserObj.LOGIN_FAIL_COUNT == (limit.LOGIN_FAIL_COUNT - 1))
                                    {
                                        updateLoginFailSql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @loginFailCount, [ACCOUNT_LOCK_FLAG] = @yesFlag WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    }
                                    //fail count not yet reached limits
                                    else
                                    {
                                        updateLoginFailSql = @"UPDATE [USERS] SET [LAST_ACCESS_DATETIME] = @accessDateTime, [LOGIN_FAIL_COUNT] = @loginFailCount WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    }
                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, updateLoginFailSql, sqlParams, nonQueryTransaction, API_TYPE);
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to login.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                    else
                                    {
                                        nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, insertHistorySql, sqlParams, nonQueryTransaction, API_TYPE);
                                    }
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert system user history.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                    else
                                    {
                                        string systemMessage = GenerateUserActivityLog("LOGIN_FAILED", currUserObj, _login);
                                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _login.FROM_SOURCE, "Login Fail", systemMessage, userId, currentDatetime);

                                    }
                                    if (nonQueryTransaction == null)
                                    {
                                        error.MESSAGE = "Failed to insert user activity log.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                    else
                                    {
                                        if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                        {
                                            //fail count reached limits
                                            if (currUserObj.LOGIN_FAIL_COUNT == (limit.LOGIN_FAIL_COUNT - 1))
                                            {
                                                Login errorObj = new Login
                                                {
                                                    VALID = "LOGIN_FAILED",
                                                    USER_ID = currUserObj.USER_ID,
                                                    USER_NAME = currUserObj.USER_NAME,
                                                    ERROR_MSG = string.Concat(currUserObj.USER_NAME, " account is locked. Please contact IT department."),
                                                };
                                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                                                return response;
                                            }
                                            //fail count not yet reached limits
                                            else
                                            {
                                                Login errorObj = new Login
                                                {
                                                    VALID = "LOGIN_FAILED",
                                                    USER_ID = currUserObj.USER_ID,
                                                    USER_NAME = currUserObj.USER_NAME,
                                                    ERROR_MSG = string.Concat((currUserObj.LOGIN_FAIL_COUNT + 1).ToString(), "/", limit.LOGIN_FAIL_COUNT.ToString(), "Invalid login attemps."),
                                                };
                                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                                                return response;
                                            }
                                        }
                                        else
                                        {
                                            if (nonQueryTransaction != null)
                                            {
                                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                            }
                                            error.MESSAGE = "Failed to commit user login info.";
                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                            return response;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                }

                string values = JsonConvert.SerializeObject(_login, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _login.USER_ID, currentMethod, "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = currentMethod;
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }


        private bool SafeEqual(string insertedPassword, string currentPassword)
        {
            if (insertedPassword.Length != currentPassword.Length)
            {
                return false;
            }
            int equal = 0;
            for (int i = 0; i < insertedPassword.Length; i++)
            {
                equal |= insertedPassword.ElementAt(i) ^ currentPassword.ElementAt(i);
            }
            return equal == 0;
        }
        #endregion

        #region Http Put
        [HttpPut]
        [Route("ChangePassword")]
        public HttpResponseMessage ChangePassword([FromBody] Login _login)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            string sql = string.Empty;

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string currentMethod = apiCommon.MethodName();
            string userId = _login.USER_ID;

            try
            {
                string password = Password.encryptedPass(userId, _login.PASSWORD);
                string passwordOri = Password.decryptedPass(_login.PASSWORD);
                string usersHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(currentMethod);

                sqlParams["@usersHistoryGuid"] = usersHistoryGuid;
                sqlParams["@userId"] = _login.USER_ID;
                sqlParams["@yesFlag"] = "Y";
                sqlParams["@noFlag"] = "N";
                sqlParams["@password"] = password;
                sqlParams["@updateDateTime"] = DateTime.Now;

                sql = @"SELECT [USER_ID], [USER_NAME] FROM [USERS] WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                IEnumerable<Login> returnVal = daSQL.ExecuteQuery<Login>(currentMethod, userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                if (returnVal == null)
                {
                    if (nonQueryTransaction != null)
                    {
                        daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                    }
                    error.MESSAGE = "Failed to find existing user.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                else
                {
                    if (!returnVal.Any())
                    {
                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                        }
                        error.MESSAGE = "User not found.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                    else
                    {
                        sql = @"SELECT TOP (3) [PASSWORD] FROM (
                                    SELECT DISTINCT [PASSWORD], [UPDATE_DATETIME] FROM [USERS_HISTORY] WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag) TEMP 
                                ORDER BY [UPDATE_DATETIME] DESC;";
                    }
                    Login currObj = returnVal.First();
                    IEnumerable<Login> previousPass = daSQL.ExecuteQuery<Login>(currentMethod, userId, sql, sqlParams, API_TYPE);
                    if (previousPass == null)
                    {
                        if (nonQueryTransaction != null)
                        {
                            daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                        }
                        error.MESSAGE = "Failed to check previous password.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;

                    }
                    else
                    {
                        List<string> previousPassList = previousPass.Select(p => p.PASSWORD).ToList();
                        bool repeatFlag = previousPass.Select(p => p.PASSWORD).ToList().Contains(password);
                        if (repeatFlag)
                        {
                            Login errorObj = new Login
                            {
                                VALID = "PASSWORD_REPEAT",
                                USER_ID = currObj.USER_ID,
                                USER_NAME = currObj.USER_NAME,
                                ERROR_MSG = "Password could not be same as previous three changes.",
                            };
                            if (nonQueryTransaction != null)
                            {
                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                            }
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                            return response;
                        }
                        else
                        {
                            sql = @"
                                SELECT [PASSWORD_REGEX], [AUTO_LOGOUT_DURATION] FROM
                                    (SELECT [POLICY_VALUE] AS [PASSWORD_REGEX] FROM [POLICY_MST] WHERE [POLICY_ID] = 'PASSWORD_REGEX' AND [DELETE_FLAG] = 'N') [PasswordRegex],
                                    (SELECT CONVERT (INT, [POLICY_VALUE]) AS [AUTO_LOGOUT_DURATION] FROM [POLICY_MST] WHERE [POLICY_ID] = 'WEB_LOCKOUT_DURATION' AND [DELETE_FLAG] = 'N') [LogoutDuration];";
                        }
                        IEnumerable<Login> limitations = daSQL.ExecuteQuery<Login>(currentMethod, userId, sql, sqlParams, API_TYPE);
                        if (limitations == null)
                        {
                            if (nonQueryTransaction != null)
                            {
                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                            }
                            error.MESSAGE = "Failed to get password regex.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;

                        }
                        else
                        {
                            if (!limitations.Any())
                            {
                                if (nonQueryTransaction != null)
                                {
                                    daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                }
                                error.MESSAGE = "Password regex not found.";
                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }
                            else
                            {
                                Login limit = limitations.First();
                                Regex regex = new Regex(@limit.PASSWORD_REGEX);
                                if (!regex.IsMatch(passwordOri))
                                {
                                    Login errorObj = new Login
                                    {
                                        VALID = "PASSWORD_INVALID",
                                        USER_ID = currObj.USER_ID,
                                        USER_NAME = currObj.USER_NAME,
                                        ERROR_MSG = "Incorrect password format! Password must be contains at least 8 characters.",
                                    };
                                    if (nonQueryTransaction != null)
                                    {
                                        daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                    }
                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, errorObj);
                                    return response;
                                }
                                else
                                {
                                    sql = @"UPDATE [USERS] SET [PASSWORD] = @password, [RESET_FLAG] = @noFlag, [RESET_DATETIME] = @updateDateTime, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @userId
                                WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                    nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                                    if (nonQueryTransaction != null)
                                    {
                                        sql = @"
                                            INSERT INTO [USERS_HISTORY] ([GUID], [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                                [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                                [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                                [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                                [UPDATE_DATETIME], [UPDATE_ID])
                                                    SELECT @usersHistoryGuid, [USER_ID], [PASSWORD], [STAFF_NO], [USER_NAME], [USERROLE_ID],
                                                        [DEPT_ID], [EMAIL], [EXT_NO], [ERROR_EMAIL_FLAG], [ACTIVE_FLAG],
                                                        [DELETE_FLAG], [LAST_ACCESS_DATETIME], [LOGIN_FAIL_COUNT], [ACCOUNT_LOCK_FLAG],
                                                        [RESET_DATETIME], [RESET_FLAG], [DEFAULT_PASSWORD],
                                                        [UPDATE_DATETIME], [UPDATE_ID] FROM [USERS]
                                                    WHERE UPPER([USER_ID]) = UPPER(@userId) AND [DELETE_FLAG] = @noFlag";
                                        nonQueryTransaction = daSQL.ExecuteNonQuery(currentMethod, userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                                        if (nonQueryTransaction == null)
                                        {
                                            error.MESSAGE = "Failed to insert system user information history.";
                                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                            return response;
                                        }
                                        else
                                        {
                                            string systemMessage = GenerateUserActivityLog("CHANGE_PASSWORD", currObj, _login);
                                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, usersHistoryGuid, _login.FROM_SOURCE, "Change Password", systemMessage, userId, currentDatetime);
                                        }
                                        if (nonQueryTransaction != null)
                                        {
                                            if (daSQL.EndTransaction(currentMethod, userId, ref nonQueryTransaction))
                                            {
                                                Login successObj = new Login
                                                {
                                                    VALID = "LOGIN_SUCCESS",
                                                    USER_ID = currObj.USER_ID,
                                                    USER_NAME = currObj.USER_NAME,
                                                    AUTO_LOGOUT_DURATION = limit.AUTO_LOGOUT_DURATION

                                                };
                                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, successObj);
                                                return response;
                                            }
                                            else
                                            {
                                                if (nonQueryTransaction != null)
                                                {
                                                    daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                                }
                                                error.MESSAGE = "Failed to commit user password change.";
                                                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                                return response;
                                            }
                                        }
                                        else
                                        {
                                            if (nonQueryTransaction != null)
                                            {
                                                daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
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
                                            daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                                        }
                                        error.MESSAGE = "Failed to change system user password.";
                                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(currentMethod, userId, ref nonQueryTransaction);
                }

                string values = JsonConvert.SerializeObject(_login, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, userId, currentMethod, "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = currentMethod;
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        #endregion

        #region Common
        private string GenerateUserActivityLog(string action, Login currObj, Login _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string systemMsg = string.Empty;

            try
            {
                if (action.Equals("LOGIN_SUCCESS"))
                {
                    systemMsg += @"User Login Successful: [" + currObj.USER_NAME + "]";
                    systemMsg += apiCommon.GetParamMsg("user name", currObj.USER_NAME);
                    systemMsg += apiCommon.GetParamMsg("login fail count", currObj.LOGIN_FAIL_COUNT.ToString(), "0");
                }
                else if (action.Equals("LOGIN_FAILED"))
                {
                    systemMsg += @"User Login Fail: [" + currObj.USER_NAME + "]";

                    systemMsg += apiCommon.GetParamMsg("login fail count", currObj.LOGIN_FAIL_COUNT.ToString(), (currObj.LOGIN_FAIL_COUNT + 1).ToString());

                    if (currObj.LOGIN_FAIL_COUNT == 2)
                    {
                        systemMsg += @" --Account Locked";
                    }
                }
                else if (action == "CHANGE_PASSWORD")
                {
                    systemMsg += @"User Change Password: [" + currObj.USER_NAME + "]";
                }

                return systemMsg;
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _values.USER_ID, apiCommon.MethodName(), ex.ToString());
                return null;
            }
        }
        #endregion

        private bool IsPowerUser(string userId)
        {
            if (userId.ToLower() == "system") return true;
            return false;
        }
    }
}