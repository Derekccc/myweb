using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using Logger.Logging;
using MY_WEBSITE_API.Models.Common;
using DatabaseAccessor.DatabaseAccessor;
using MY_WEBSITE_API.Models.Web.SystemSetup;

namespace MY_WEBSITE_API.Controllers.Common
{
    [RoutePrefix("api/WebCommon")]

    public class WebCommonController : ApiController
    {
        private const string API_TYPE = "Api";

        #region Http Get
        [HttpGet]
        [Route("GetUserAuthority")]
        public HttpResponseMessage GetUserAuthority(string _userId, string _moduleCat)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@userId"] = _userId;
                sqlParams["@moduleCat"] = _moduleCat;
                string sql =
                    @"SELECT DISTINCT TEMP1.[MODULE_ID] FROM
                            (SELECT SM.[MODULE_ID], SF.[FEATURE_ID] FROM [SYSTEM_MODULE] SM 
                                LEFT JOIN [SYSTEM_FEATURE] SF ON SM.[MODULE_ID] = SF.[MODULE_ID]) TEMP1
                        LEFT JOIN (SELECT TEMP2.[AUTHORITY_ID] FROM
                            (SELECT [AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA 
                                LEFT JOIN [USERROLE] UR ON UR.[ROLE_ID] = RA.[ROLE_ID] WHERE UR.[USER_ID] = @userId) TEMP2 
                            LEFT JOIN (SELECT SM.[MODULE_NAME], RA.[AUTHORITY_ID] FROM [SYSTEM_FEATURE] SF 
                            LEFT JOIN [ROLE_AUTHORITY] RA ON RA.[AUTHORITY_ID] = SF.[FEATURE_ID] 
                            LEFT JOIN [SYSTEM_MODULE] SM ON SM.[MODULE_ID] = SF.[MODULE_ID]
                            WHERE SM.[CATEGORY_ID] = @moduleCat AND SF.[FEATURE_ACTIVE_FLAG] = 'Y') TEMP3
                            ON TEMP3.[AUTHORITY_ID] = TEMP2.[AUTHORITY_ID] WHERE TEMP3.[MODULE_NAME] IS NOT NULL) TEMP4 
                        ON TEMP4.[AUTHORITY_ID] = TEMP1.[FEATURE_ID] WHERE TEMP4.[AUTHORITY_ID] IS NOT NULL ORDER BY TEMP1.[MODULE_ID]";
                IEnumerable<WebCommon> returnVal = daSQL.ExecuteQuery<WebCommon>(apiCommon.MethodName(), _userId, sql, sqlParams, API_TYPE);

                if (returnVal == null)
                {
                    error.MESSAGE = "Failed to get user authority.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                else
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, null, apiCommon.MethodName(), "Values: " + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpGet]
        [Route("GetPageAuthority")]
        public HttpResponseMessage GetPageAuthority(string _userId, string _moduleId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@userId"] = _userId;
                sqlParams["@moduleId"] = _moduleId;

                string sql = @"
                SELECT (CASE WHEN TEMP.[FEATURE_TYPE] = 'R' THEN 'READ' WHEN TEMP.[FEATURE_TYPE] = 'W' THEN 'WRITE' END) AS [FEATURE_TYPE], 
                    (CASE WHEN TEMP2.[AUTHORITY_ID] IS NULL THEN 'N' ELSE 'Y' END) AS [AUTHORIZED] FROM 
                    (SELECT DISTINCT RA.[AUTHORITY_ID], SM.[MODULE_ID], SF.[FEATURE_TYPE] FROM [SYSTEM_FEATURE] SF 
                        LEFT JOIN [ROLE_AUTHORITY] RA ON RA.[AUTHORITY_ID] = SF.[FEATURE_ID]
                        LEFT JOIN [SYSTEM_MODULE] SM ON SM.[MODULE_ID] = SF.[MODULE_ID]) TEMP 
                LEFT JOIN (SELECT [AUTHORITY_ID] FROM [ROLE_AUTHORITY] RA 
                            LEFT JOIN [USERROLE] UR ON UR.[ROLE_ID] = RA.[ROLE_ID] WHERE UR.[USER_ID] = @userId) TEMP2
                ON TEMP2.[AUTHORITY_ID] = TEMP.[AUTHORITY_ID] WHERE TEMP.[MODULE_ID] = @moduleId";

                IEnumerable<WebCommon> returnVal = daSQL.ExecuteQuery<WebCommon>(apiCommon.MethodName(), _userId, sql, sqlParams, API_TYPE);

                if (returnVal == null)
                {
                    error.MESSAGE = "Fail to get user page authority.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                else
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
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, null, apiCommon.MethodName(), "Values: " + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpGet]
        [Route("GetProjectName")]
        public HttpResponseMessage GetProjectName(string _userId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                sqlParams["@userId"] = _userId;
                sqlParams["@policy"] = "PROJECT_NAME";
                string sql =
                    @"SELECT POLICY_VALUE, POLICY_DESC FROM POLICY_MST WHERE POLICY_ID =@policy";
                IEnumerable<Policy> returnVal = daSQL.ExecuteQuery<Policy>(apiCommon.MethodName(), _userId, sql, sqlParams, API_TYPE);

                if (returnVal == null)
                {
                    error.MESSAGE = "Failed to get project name.";
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                    return response;
                }
                else
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent);
                        return response;
                    }
                    else
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, null, apiCommon.MethodName(), "Values: " + ex.ToString());

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
        [Route("LogWebError")]
        public void LogWebError([FromBody] WebCommon _values)
        {
            ApiCommonController apiCommonController = new ApiCommonController();

            try
            {
                apiCommonController.WebLog(LogType.ERROR_TYPE, _values.USER_ID, _values.PAGE_NAME, _values.FUNCTION_NAME, _values.MESSAGE);
            }
            catch (Exception ex)
            {
                apiCommonController.WebApiLog(LogType.ERROR_TYPE, _values.USER_ID, apiCommonController.MethodName(), ex.ToString());
            }
        }
        #endregion
    }
}