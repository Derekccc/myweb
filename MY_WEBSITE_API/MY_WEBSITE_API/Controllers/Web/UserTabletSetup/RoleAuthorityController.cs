using Logger.Logging;
using MY_WEBSITE_API.Classes;
using MY_WEBSITE_API.Controllers.Common;
using MY_WEBSITE_API.Models.Common;
using MY_WEBSITE_API.Models.Web.UserTabletSetup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using DatabaseAccessor.DatabaseAccessor;

namespace MY_WEBSITE_API.Controllers.Web.UserTabletSetup
{
    [RoutePrefix("api/roleAuthority")]
    public class RoleAuthorityController : ApiController
    {
        private const string API_TYPE = "Api";
        private const string PAGE_NAME = "Role Authority";

        #region Http Get
        [HttpGet]
        [Route("GetAllRole")]
        public HttpResponseMessage GetAllRole(string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string sql = @"
                SELECT [ROLE_ID], [ROLE_NAME]
                FROM [ROLE_MST]
                WHERE [ROLE_ACTIVE_FLAG] = 'Y' AND [ROLE_DELETE_FLAG] = 'N' 
                    ORDER BY [ROLE_NAME] ASC";

                IEnumerable<RoleAuthority> returnVal = daSQL.ExecuteQuery<RoleAuthority>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get role list.";
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
        [Route("GetFeatureAuthority")]
        public HttpResponseMessage GetFeatureAuthority(string _sysUser, string _roleId, string _moduleCat)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@roleId"] = _roleId;
                sqlParams["@moduleCat"] = _moduleCat;

                string sql = @"
                SELECT DISTINCT SM.[MODULE_NAME] FROM [SYSTEM_MODULE] SM WHERE SM.[CATEGORY_ID] = @moduleCat AND SM.[ACTIVE_FLAG] = 'Y' ORDER BY SM.[MODULE_NAME];";

                IEnumerable<RoleAuthority> returnVal = daSQL.ExecuteQuery<RoleAuthority>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = new HttpResponseMessage();
                        sql = @"SELECT TEMP.[FEATURE_ID], TEMP.[MODULE_NAME], TEMP.[FEATURE_TYPE], TEMP2.[ROLE_AUTHORITY_ID], CASE WHEN TEMP2.[ROLE_AUTHORITY_ID] IS NOT NULL THEN 'Y' ELSE 'N' END AS [AUTHORIZED]
	                            FROM (SELECT SF.[FEATURE_ID], SM.[MODULE_NAME], SF.[FEATURE_TYPE] FROM [SYSTEM_MODULE] SM LEFT JOIN [SYSTEM_FEATURE] SF ON SF.MODULE_ID = SM.MODULE_ID 
                                            WHERE SM.[CATEGORY_ID] = @moduleCat AND SM.[MODULE_NAME] = @moduleName AND SF.[FEATURE_ACTIVE_FLAG] = 'Y') TEMP 
                                LEFT JOIN (SELECT RA.[ROLE_AUTHORITY_ID], RA.[AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA WHERE RA.[ROLE_ID] = @roleId) TEMP2 
	                            ON TEMP2.AUTHORITY_ID = TEMP.FEATURE_ID ORDER BY TEMP.[FEATURE_TYPE]";
                        foreach (RoleAuthority r in returnVal)
                        {
                            sqlParams["@moduleName"] = r.MODULE_NAME;
                            IEnumerable<RoleAuthorityDetails> tempVal = daSQL.ExecuteQuery<RoleAuthorityDetails>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);
                            if (tempVal != null)
                            {
                                r.AUTHORITY_DETAILS = tempVal;
                            }
                            else
                            {
                                error.MESSAGE = "Failed to get authority details.";
                                response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                return response;
                            }
                        }
                        response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get module description.";
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
        [Route("GetStationAuthority")]
        public HttpResponseMessage GetStationAuthority(string _sysUser, string _roleId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            try
            {
                sqlParams["@roleId"] = _roleId;

                string sql = @"
                SELECT CONCAT('CAT_STATION', '@_@', TEMP.[STATION_ID]) AS STATION_ID, TEMP.[STATION_DESC], TEMP2.[ROLE_AUTHORITY_ID], CASE WHEN TEMP2.[ROLE_AUTHORITY_ID] IS NOT NULL THEN 'Y' ELSE 'N' END AS AUTHORIZED
	            FROM (SELECT SM.[STATION_ID], SM.[STATION_DESC] FROM [STATION_MASTER] SM 
			                WHERE SM.[ACTIVE_FLAG] = 'Y' AND SM.[DELETE_FLAG] = 'N') TEMP 
	                    LEFT JOIN (SELECT RA.[ROLE_AUTHORITY_ID], RA.[AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA WHERE RA.[ROLE_ID] = @roleId) TEMP2 
	                    ON TEMP2.[AUTHORITY_ID] = CONCAT('CAT_STATION', '@_@', TEMP.[STATION_ID])
	            ORDER BY TEMP.[STATION_DESC]";

                IEnumerable<RoleAuthority> returnVal = daSQL.ExecuteQuery<RoleAuthority>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get station list";
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
        [Route("GetTabletAuthority")]
        public HttpResponseMessage GetTabletAuthority(string _sysUser, string _roleId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@roleId"] = _roleId;

                string sql = @"
                SELECT TEMP.[DEVICE_MST_ID], TEMP.[DEVICE_DESC], TEMP2.[ROLE_AUTHORITY_ID], CASE WHEN TEMP2.[ROLE_AUTHORITY_ID] IS NOT NULL THEN 'Y' ELSE 'N' END AS AUTHORIZED 
	            FROM (SELECT DM.[DEVICE_MST_ID], DM.[DEVICE_DESC] FROM [DEVICE_MST] DM 
			                WHERE DM.[ACTIVE_FLAG] = 'Y' AND DM.[DELETE_FLAG] = 'N') TEMP 
                        LEFT JOIN (SELECT RA.[ROLE_AUTHORITY_ID], RA.[AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA WHERE RA.[ROLE_ID] = @roleId) TEMP2 
	                    ON TEMP2.AUTHORITY_ID = TEMP.DEVICE_MST_ID
                ORDER BY TEMP.[DEVICE_DESC]";

                IEnumerable<RoleAuthority> returnVal = daSQL.ExecuteQuery<RoleAuthority>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get tablet list";
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
        [Route("GetTabletButtonAuthority")]
        public HttpResponseMessage GetTabletButtonAuthority(string _sysUser, string _roleId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@roleId"] = _roleId;
                sqlParams["@moduleCat"] = "CAT_TABLET_BUTTON";

                string sql = @"
                SELECT TEMP.[FEATURE_ID], TEMP.[MODULE_NAME], TEMP2.[ROLE_AUTHORITY_ID], CASE WHEN TEMP2.[ROLE_AUTHORITY_ID] IS NOT NULL THEN 'Y' ELSE 'N' END AS AUTHORIZED 
                FROM (SELECT SF.[FEATURE_ID], SM.[MODULE_NAME] FROM [SYSTEM_FEATURE] SF LEFT JOIN [SYSTEM_MODULE] SM ON SF.[MODULE_ID] = SM.[MODULE_ID]
                            WHERE SM.[CATEGORY_ID] = @moduleCat AND SM.[ACTIVE_FLAG] = 'Y' AND SF.[FEATURE_ACTIVE_FLAG] = 'Y') TEMP 
                        LEFT JOIN (SELECT RA.[ROLE_AUTHORITY_ID], RA.[AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA WHERE RA.[ROLE_ID] = @roleId) TEMP2 
                        ON TEMP2.AUTHORITY_ID = TEMP.FEATURE_ID
                ORDER BY TEMP.[MODULE_NAME]";

                IEnumerable<RoleAuthority> returnVal = daSQL.ExecuteQuery<RoleAuthority>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get tablet button list";
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
        [Route("GetTabletFeatureAuthority")]
        public HttpResponseMessage GetTabletFeatureAuthority(string _sysUser, string _roleId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@roleId"] = _roleId;
                sqlParams["@moduleCat"] = "CAT_TABLET_FEA_AUTH";

                string sql = @"
                SELECT TEMP.[FEATURE_ID], TEMP.[MODULE_NAME], TEMP2.[ROLE_AUTHORITY_ID], CASE WHEN TEMP2.[ROLE_AUTHORITY_ID] IS NOT NULL THEN 'Y' ELSE 'N' END AS AUTHORIZED 
                FROM (SELECT SF.[FEATURE_ID], SM.[MODULE_NAME] FROM [SYSTEM_FEATURE] SF LEFT JOIN [SYSTEM_MODULE] SM ON SF.[MODULE_ID] = SM.[MODULE_ID]
                            WHERE SM.[CATEGORY_ID] = @moduleCat AND SM.[ACTIVE_FLAG] = 'Y' AND SF.[FEATURE_ACTIVE_FLAG] = 'Y') TEMP 
                        LEFT JOIN (SELECT RA.[ROLE_AUTHORITY_ID], RA.[AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA WHERE RA.[ROLE_ID] = @roleId) TEMP2 
                        ON TEMP2.AUTHORITY_ID = TEMP.FEATURE_ID
                ORDER BY TEMP.[MODULE_NAME]";

                IEnumerable<RoleAuthority> returnVal = daSQL.ExecuteQuery<RoleAuthority>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get tablet feature list";
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
        #endregion

        #region Http Post
        [HttpPost]
        [Route("ChangeFeatureAuthority")]
        public HttpResponseMessage ChangeFeatureAuthority([FromBody] RoleAuthority _roleAuthority)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _roleAuthority.UPDATE_ID;
            string sql = string.Empty;
            try
            {
                string featureId = _roleAuthority.FEATURE_ID;
                string roleId = _roleAuthority.ROLE_ID;
                string updateBy = _roleAuthority.UPDATE_ID;
                string roleAuthorityHistoryGuid = Guid.NewGuid().ToString();


                sqlParams["@roleAuthorityHistoryGuid"] = roleAuthorityHistoryGuid;
                sqlParams["@roleId"] = roleId;
                sqlParams["@featureId"] = featureId;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateBy"] = updateBy;

                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());
                if (_roleAuthority.AUTHORIZED == "Y")
                {
                    sqlParams["@roleAuthorityGuid"] = Guid.NewGuid().ToString();
                    sql = @"
                           BEGIN
                                INSERT INTO [ROLE_AUTHORITY] ([ROLE_AUTHORITY_ID], [ROLE_ID], [AUTHORITY_ID], [UPDATE_DATETIME], [UPDATE_ID])
                                    VALUES (@roleAuthorityGuid, @roleId, @featureId, @updateDatetime, @updateBy);
                                INSERT INTO [ROLE_AUTHORITY_HISTORY] ([GUID], [ROLE_AUTHORITY_ID], [ROLE_ID], [AUTHORITY_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID])
                                    VALUES (@roleAuthorityHistoryGuid, @roleAuthorityGuid, @roleId, @featureId, 'INSERT', @updateDatetime, @updateBy);
                           END;";
                }
                else
                {
                    sql = @"
                           BEGIN
                                INSERT INTO [ROLE_AUTHORITY_HISTORY] ([GUID], [ROLE_AUTHORITY_ID], [ROLE_ID], [AUTHORITY_ID], [ACTION], [UPDATE_DATETIME], [UPDATE_ID])
                                    SELECT @roleAuthorityHistoryGuid, [ROLE_AUTHORITY_ID], @roleId, @featureId, 'DELETE', @updateDatetime, @updateBy FROM [ROLE_AUTHORITY]
                                        WHERE [ROLE_ID] = @roleId AND [AUTHORITY_ID] = @featureId;
                                DELETE FROM [ROLE_AUTHORITY] WHERE [ROLE_ID] = @roleId AND [AUTHORITY_ID] = @featureId;
                           END;";
                }
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), updateBy, sql, sqlParams, nonQueryTransaction, API_TYPE);

                if (nonQueryTransaction == null)
                {
                    error.MESSAGE = "Failed to insert role authority.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                else
                {
                    string systemMessage = GenerateUserActivityLog(UserActions.INSERT, _roleAuthority);
                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, roleAuthorityHistoryGuid, _roleAuthority.FROM_SOURCE, UserActions.INSERT, systemMessage, userId, currentDatetime);
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
                        error.MESSAGE = "Failed to commit update station route.";
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
            catch (Exception ex)
            {
                if (nonQueryTransaction != null)
                {
                    daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
                }
                string ErrorMsg = string.Empty;
                if (_roleAuthority.AUTHORIZED == "Y")
                {
                    ErrorMsg = @"Error on Insert Role Authority." + "\r\n" +
                                " Role ID: " + _roleAuthority.ROLE_ID +
                                " Authority ID: " + _roleAuthority.FEATURE_ID +
                                " Update Datetime: " + currentDatetime.ToString() +
                                " Update ID: " + _roleAuthority.UPDATE_ID;
                }
                else
                {
                    ErrorMsg = @"Error on Delete Role Authority." + "\r\n" +
                                " Role ID: " + _roleAuthority.ROLE_ID +
                                " Authority ID: " + _roleAuthority.FEATURE_ID +
                                " Update Datetime: " + currentDatetime.ToString() +
                                " Update ID: " + _roleAuthority.UPDATE_ID;
                }
                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ErrorMsg;

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }
        #endregion


        #region Common
        private string GenerateUserActivityLog(string action, RoleAuthority _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            string systemMsg = string.Empty;

            try
            {
                if (action.Equals(UserActions.INSERT))
                {
                    systemMsg += @"Insert role authority:";
                    systemMsg += apiCommon.GetParamMsg("role", _values.ROLE_NAME);
                    if (!string.IsNullOrEmpty(_values.FEATURE_ID)) systemMsg += apiCommon.GetParamMsg("feature", _values.FEATURE_ID);
                    if (!string.IsNullOrEmpty(_values.STATION_DESC)) systemMsg += apiCommon.GetParamMsg("station", _values.STATION_DESC);
                    if (!string.IsNullOrEmpty(_values.DEVICE_DESC)) systemMsg += apiCommon.GetParamMsg("device", _values.DEVICE_DESC);
                }
                else
                {
                    systemMsg += @"Delete role authority:";
                    systemMsg += apiCommon.GetParamMsg("role", _values.ROLE_NAME);
                    if (!string.IsNullOrEmpty(_values.FEATURE_ID)) systemMsg += apiCommon.GetParamMsg("feature", _values.FEATURE_ID);
                    if (!string.IsNullOrEmpty(_values.STATION_DESC)) systemMsg += apiCommon.GetParamMsg("station", _values.STATION_DESC);
                    if (!string.IsNullOrEmpty(_values.DEVICE_DESC)) systemMsg += apiCommon.GetParamMsg("device", _values.DEVICE_DESC);
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