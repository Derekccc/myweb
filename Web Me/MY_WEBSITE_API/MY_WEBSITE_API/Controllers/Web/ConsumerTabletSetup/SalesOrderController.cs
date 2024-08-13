using DatabaseAccessor.DatabaseAccessor;
using Logger.Logging;
using MY_WEBSITE_API.Classes;
using MY_WEBSITE_API.Controllers.Common;
using MY_WEBSITE_API.Models.Common;
using MY_WEBSITE_API.Models.Web.ConsumerTabletSetup;
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
using System.Web.Configuration;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using MY_WEBSITE_API.Models.Web.UserTabletSetup;

namespace MY_WEBSITE_API.Controllers.Web.ConsumerTabletSetup
{
    [RoutePrefix("api/salesOrder")]
    public class SalesOrderController : ApiController
    {
        private const string API_TYPE = "Api";
        private const string PAGE_NAME = "Sales Order Maintenance";

        #region Http Get
        [HttpGet]
        [Route("GetAllSalesOrder")]
        public HttpResponseMessage GetAllSalesOrder(string _sysUser, string _customerName, string _orderStatus)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string sql = @"SELECT DISTINCT 
                                SO.[SALES_ORDER_ID],
                                CU.[CUSTOMER_NAME], 
                                SO.[TOTAL_AMOUNT], 
                                SO.[ORDER_DATETIME], 
                                SO.[SALES_ORDER_ACCEPT_FLAG],
                                SO.[ORDER_STATUS], 
                                CONCAT(SU.[USER_ID], ' - ', SU.[USER_NAME]) AS UPDATE_ID, 
                                SO.[UPDATE_DATETIME]
                            FROM 
                                [SALES_ORDER] SO
                            LEFT JOIN 
                                [CUSTOMERS] CU ON CU.[CUSTOMER_ID] = SO.[CUSTOMER_ID]
                            LEFT JOIN 
                                [USERS] SU ON SU.[USER_ID] = SO.[UPDATE_ID]
                            
                            ";

                bool searchCondition = true;

                if ((!string.IsNullOrEmpty(_customerName)) && _customerName != "undefined")
                {
                    sqlParams["@customerName"] = _customerName;
                    if (searchCondition)
                    {
                        sql += @" WHERE CU.CUSTOMER_NAME LIKE ('%' + @customerName + '%')";
                        searchCondition = false;
                    }
                    else
                    {
                        sql += @" AND CU.CUSTOMER_NAME LIKE ('%' + @customerName + '%')";
                    }
                }

                if ((!string.IsNullOrEmpty(_orderStatus)) && _orderStatus != "undefined")
                {
                    sqlParams["@orderStatus"] = _orderStatus;
                    if (searchCondition)
                    {
                        sql += @" WHERE SO.ORDER_STATUS LIKE ('%' + @orderStatus + '%')";
                        searchCondition = false;
                    }
                    else
                    {
                        sql += @" AND SO.ORDER_STATUS LIKE ('%' + @orderStatus + '%')";
                    }
                }

                // Append the ORDER BY clause
                sql += " ORDER BY SO.UPDATE_DATETIME DESC";


                IEnumerable<SalesOrder> returnVal = daSQL.ExecuteQuery<SalesOrder>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        foreach (SalesOrder r in returnVal)
                        {
                            r.UPDATE_DATETIME = r.UPDATE_DATETIME.AddTicks(-(r.UPDATE_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                            if (r.ORDER_DATETIME != null)
                            {
                                r.ORDER_DATETIME = r.ORDER_DATETIME.AddTicks(-(r.ORDER_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                            }   
                        }
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get sales order list";
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
        [Route("GetSalesChartData")]
        public HttpResponseMessage GetSalesChartData(string updateId, DateTime? startDate = null, DateTime? endDate = null)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string query = @"
                                SELECT 
                                    DATEPART(MONTH, ORDER_DATETIME) AS [OrderMonth], 
                                    DATEPART(YEAR, ORDER_DATETIME) AS [OrderYear],
                                    SUM(TOTAL_AMOUNT) AS [TotalAmount]
                                FROM SALES_ORDER
                                WHERE SALES_ORDER_ACCEPT_FLAG = 'Y'
                                ";

                if (startDate.HasValue)
                {
                    query += " AND ORDER_DATETIME >= @StartDate";
                    sqlParams["@StartDate"] = startDate.Value.Date;
                }

                
                if (endDate.HasValue)
                {
                    query += " AND ORDER_DATETIME < DATEADD(DAY, 1, @EndDate)";
                    sqlParams["@EndDate"] = endDate.Value.Date;
                }

                query += " GROUP BY DATEPART(MONTH, ORDER_DATETIME), DATEPART(YEAR, ORDER_DATETIME) " +
                         "ORDER BY DATEPART(YEAR, ORDER_DATETIME), DATEPART(MONTH, ORDER_DATETIME)";

                IEnumerable<SalesChartData> data = daSQL.ExecuteQuery<SalesChartData>(apiCommon.MethodName(), updateId, query, sqlParams, API_TYPE);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
                else
                {
                    error.MESSAGE = "Failed to get sales data";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, error);
                }
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, updateId, apiCommon.MethodName(), ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.Message;

                return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            }
        }





        [HttpGet]
        [Route("api/salesOrder/GetSalesOrderCustomerDetails")]
        public HttpResponseMessage GetSalesOrderCustomerDetails([FromBody] string salesOrderId, string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {

                sqlParams["@salesOrderId"] = salesOrderId;

                string sql = @"
                SELECT DISTINCT
                    SO.[SALES_ORDER_ID],
                    SO.[ORDER_DATETIME],
                    CU.[CUSTOMER_NAME], 
                    CU.[EMAIL],
                    CU.[ADDRESS],
                    CU.[COMPANY_NAME],
                    CU.[PHONE_NO]
                FROM 
                    [SALES_ORDER] SO
                LEFT JOIN 
                    [CUSTOMERS] CU ON CU.[CUSTOMER_ID] = SO.[CUSTOMER_ID] 
                WHERE 
                    UPPER(SO.[SALES_ORDER_ID]) = UPPER(@salesOrderId)
                 ";

                // Execute query
                IEnumerable<SalesOrder> returnVal = daSQL.ExecuteQuery<SalesOrder>(apiCommon.MethodName(), salesOrderId, sql, sqlParams, API_TYPE);

                if (returnVal != null && returnVal.Any())
                {
                    foreach (SalesOrder r in returnVal)
                    {
                        if (r.ORDER_DATETIME != null)
                        {
                            r.ORDER_DATETIME = r.ORDER_DATETIME.AddTicks(-(r.ORDER_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                        }
                    }

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                    return response;
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotFound, "No customer details found for the sales order ID.");
                    return response;
                }
            }
            catch (Exception ex)
            {
                apiCommon.WebApiLog(LogType.ERROR_TYPE, salesOrderId, apiCommon.MethodName(), ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.Message;

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError, error);
                return response;
            }
        }



        [HttpGet]
        [Route("ExportExcel")]
        public HttpResponseMessage ExportExcel([FromUri] string _sysUser, string _customerName, string _orderStatus)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            string currentDatetime = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

            try
            {
                string selectSalesOrderSql = @"
                                        SELECT SO.SALES_ORDER_ID AS [Sales Order ID],
											CU.CUSTOMER_NAME AS [Customer Name],
											SO.TOTAL_AMOUNT AS [Total Amount],
											SO.ORDER_DATETIME AS [Order Date],
                                            SO.ORDER_STATUS AS [Order Status],
                                            (CU.CUSTOMER_ID + ' - ' + CU.CUSTOMER_NAME) AS [Customer Purchased],
                                            SO.UPDATE_DATETIME AS [Last Update Date Time]
                                        FROM SALES_ORDER SO
                                        LEFT JOIN CUSTOMERS CU ON SO.UPDATE_ID = CU.UPDATE_ID
                                        WHERE SO.CUSTOMER_ID = CU.CUSTOMER_ID       
                                        ";

                if ((!string.IsNullOrEmpty(_customerName)) && _customerName != "undefined")
                {
                    sqlParams["@customerName"] = _customerName;
                    selectSalesOrderSql += @" AND CU.CUSTOMER_NAME LIKE ('%'+@customerName+'%')";
                }
                if ((!string.IsNullOrEmpty(_orderStatus)) && _orderStatus != "undefined")
                {
                    sqlParams["@orderStatus"] = _orderStatus;
                    selectSalesOrderSql += @" AND SO.ORDER_STATUS LIKE ('%'+@orderStatus+'%')";
                }
                selectSalesOrderSql += "ORDER BY SO.UPDATE_DATETIME DESC";

                DataTable dtResult = daSQL.ExecuteQuery(apiCommon.MethodName(), _sysUser, selectSalesOrderSql, sqlParams);
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
                    error.MESSAGE = "Cannot find sales order";

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

        [HttpGet]
        [Route("GetCustomerList")]
        public HttpResponseMessage GetCustomerList(string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string selectCustomerSql = @"
                                        SELECT CU.[CUSTOMER_ID] AS [CUSTOMER_ID], CU.[CUSTOMER_NAME] as [CUSTOMER_NAME]
                                        FROM [CUSTOMERS] CU
                                        WHERE CU.[DELETE_FLAG] = 'N' AND CU.[ACTIVE_FLAG] = 'Y'
                                        ORDER BY CU.[CUSTOMER_NAME] 
                                        ";

                IEnumerable<SalesOrder> returnVal = daSQL.ExecuteQuery<SalesOrder>(apiCommon.MethodName(), _sysUser, selectCustomerSql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get customer name";
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
        [Route("GetProductList")]
        public HttpResponseMessage GetProductList(string _sysUser)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string selectProductSql = @"
                                            SELECT PD.[PRODUCT_ID] AS [PRODUCT_ID], PD.[PRODUCT_NAME] AS [PRODUCT_NAME], PD.[QUANTITY] AS [AVAILABLE_QUANTITY],
                                                    PD.[UNIT_SELLING_PRICE] AS [UNIT_SELLING_PRICE]
                                            FROM [PRODUCTS] PD
                                            WHERE PD.[PRODUCT_DELETE_FLAG] = 'N' AND PD.[PRODUCT_ACTIVE_FLAG] = 'Y'
                                            ORDER BY PD.[PRODUCT_NAME]     
                                            ";

                IEnumerable<SalesOrder> returnVal = daSQL.ExecuteQuery<SalesOrder>(apiCommon.MethodName(), _sysUser, selectProductSql, sqlParams, API_TYPE);

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
                    error.MESSAGE = "Failed to get product name";
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
        [Route("InsertSalesOrder")]
        public HttpResponseMessage InsertSalesOrder([FromBody] SalesOrder salesOrder)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = salesOrder.UPDATE_ID;

            try
            {
                // Generate the Sales Order ID
                string salesOrderId = GenerateSalesOrderId(daSQL);
                salesOrder.SALES_ORDER_ID = salesOrderId;
                string salesOrderHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@salesOrderHistoryGuid"] = salesOrderHistoryGuid;
                sqlParams["@salesOrderId"] = salesOrder.SALES_ORDER_ID;
                sqlParams["@customerId"] = salesOrder.CUSTOMER_ID;
                sqlParams["@totalAmount"] = salesOrder.TOTAL_AMOUNT;
                sqlParams["@orderDateTime"] = salesOrder.ORDER_DATETIME;
                sqlParams["@acceptFlag"] = "Y";
                sqlParams["@orderStatus"] = salesOrder.ORDER_STATUS;
                sqlParams["@productId"] = salesOrder.PRODUCT_ID;
                sqlParams["@quantity"] = salesOrder.QUANTITY;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                string sql = @"
            INSERT INTO SALES_ORDER (SALES_ORDER_ID, CUSTOMER_ID, TOTAL_AMOUNT, ORDER_DATETIME, SALES_ORDER_ACCEPT_FLAG, ORDER_STATUS, UPDATE_DATETIME, UPDATE_ID)
            VALUES (@salesOrderId, @customerId, @totalAmount, @orderDateTime, @acceptFlag, @orderStatus, @updateDatetime, @updateId)";

                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                if (nonQueryTransaction != null)
                {
                    sql = @"
                INSERT INTO SALES_ORDER_HISTORY (GUID, SALES_ORDER_ID, CUSTOMER_ID, TOTAL_AMOUNT, ORDER_DATETIME, SALES_ORDER_ACCEPT_FLAG, ORDER_STATUS, UPDATE_DATETIME, UPDATE_ID)
                VALUES (@salesOrderHistoryGuid, @salesOrderId, @customerId, @totalAmount, @orderDateTime, @acceptFlag, @orderStatus, @updateDatetime, @updateId)";

                    sqlParams["@salesOrderHistoryGuid"] = Guid.NewGuid().ToString();
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        // Update product quantity based on sales order details
                        sql = @"
                            UPDATE PRODUCTS
                            SET QUANTITY = QUANTITY - @quantity
                            WHERE PRODUCT_ID = @productId";

                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                        if (daSQL.EndTransaction(apiCommon.MethodName(), userId, ref nonQueryTransaction))
                        {
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                            return response;
                        }
                        else
                        {
                            error.MESSAGE = "Failed to commit insert sales order";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert sales order history";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to insert sales order";
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

                string values = JsonConvert.SerializeObject(salesOrder, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, salesOrder.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        // Generate a unique Sales Order ID
        private string GenerateSalesOrderId(DatabaseAccessorMSSQL daSQL)
        {
            // Example format: YYYYMMDD-HH:mm:ss-RANDOM
            string dateTimePart = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string randomPart = GenerateRandomNumber().ToString("D4"); // Generate a random number
            return $"{dateTimePart}-{randomPart}";
        }

        // Generate a random number (4 digits)
        private int GenerateRandomNumber()
        {
            Random random = new Random();
            return random.Next(1000, 10000); // Generates a random number between 1000 and 9999
        }


        #endregion

        #region Http Put
        [HttpPut]
        [Route("UpdateSalesOrder")]
        public HttpResponseMessage UpdateSalesOrder([FromBody] SalesOrder _salesOrder)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;
            DateTime currentDatetime = DateTime.Now;
            string userId = _salesOrder.UPDATE_ID;

           

            try
            {
                string salesOrderHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                

                sqlParams["@salesOrderHistoryGuid"] = salesOrderHistoryGuid;
                sqlParams["@salesOrderId"] = _salesOrder.SALES_ORDER_ID;
                sqlParams["@customerId"] = _salesOrder.CUSTOMER_ID;
                sqlParams["@customerName"] = _salesOrder.CUSTOMER_NAME;
                sqlParams["@totalAmount"] = _salesOrder.TOTAL_AMOUNT;
                sqlParams["@orderStatus"] = _salesOrder.ORDER_STATUS;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                string sql = @"SELECT SALES_ORDER_ID, CUSTOMER_ID, ORDER_STATUS FROM SALES_ORDER WHERE UPPER(SALES_ORDER_ID) = UPPER(@salesOrderId) ORDER BY [UPDATE_DATETIME] DESC";
                IEnumerable<SalesOrder> returnValSalesOrder = daSQL.ExecuteQuery<SalesOrder>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValSalesOrder != null)
                {
                    sql = @"UPDATE [SALES_ORDER] SET [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID, [ORDER_STATUS] = @orderStatus
                                        WHERE [SALES_ORDER_ID] = @salesOrderId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO [SALES_ORDER_HISTORY] ([GUID], [SALES_ORDER_ID], [CUSTOMER_ID], [TOTAL_AMOUNT], [ORDER_DATETIME], 
                                [SALES_ORDER_ACCEPT_FLAG], [ORDER_STATUS], [UPDATE_DATETIME], [UPDATE_ID])
                                    SELECT @salesOrderHistoryGuid, [SALES_ORDER_ID], [CUSTOMER_ID], [TOTAL_AMOUNT], [ORDER_DATETIME],
                                    [SALES_ORDER_ACCEPT_FLAG], [ORDER_STATUS], [UPDATE_DATETIME], [UPDATE_ID] FROM [SALES_ORDER]
                                    WHERE [SALES_ORDER_ID] = @salesOrderId";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.EDIT, returnValSalesOrder, _salesOrder);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, salesOrderHistoryGuid, _salesOrder.FROM_SOURCE, UserActions.EDIT, systemMessage, userId, currentDatetime);

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

                                    error.MESSAGE = "Failed to commit update sales order.";
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
                            error.MESSAGE = "Failed to insert sales order history.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to update sales order.";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Sales order not found.";
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

                apiCommon.WebApiLog(LogType.ERROR_TYPE, userId, apiCommon.MethodName(), ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.Message;

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("UpdateOrderStatus")]
        public HttpResponseMessage UpdateOrderStatus([FromBody] SalesOrder _salesOrder)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _salesOrder.UPDATE_ID;
            try
            {
                string salesOrderHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@guid"] = salesOrderHistoryGuid;
                sqlParams["@salesOrderId"] = _salesOrder.SALES_ORDER_ID;
                sqlParams["@salesOrderAcceptFlag"] = _salesOrder.SALES_ORDER_ACCEPT_FLAG;
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@updateId"] = userId;


                string sql = @"UPDATE [SALES_ORDER] SET [SALES_ORDER_ACCEPT_FLAG] = @salesOrderAcceptFlag, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID
                                        WHERE [SALES_ORDER_ID] = @salesOrderId";
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                if (nonQueryTransaction != null)
                {
                    sql = @"INSERT INTO [SALES_ORDER_HISTORY] ([GUID], [SALES_ORDER_ID], [CUSTOMER_ID], [TOTAL_AMOUNT], [ORDER_DATETIME], [ORDER_STATUS], 
                            [SALES_ORDER_ACCEPT_FLAG],
                            [UPDATE_DATETIME], [UPDATE_ID])
                                SELECT @guid, [SALES_ORDER_ID], [CUSTOMER_ID], [TOTAL_AMOUNT], [ORDER_DATETIME], [ORDER_STATUS],
                                    [SALES_ORDER_ACCEPT_FLAG],
                                    [UPDATE_DATETIME], [UPDATE_ID] FROM [SALES_ORDER]
                                WHERE [SALES_ORDER_ID] = @salesOrderId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        string systemMessage = GenerateUserActivityLog(UserActions.UPDATE_REVIEW, null, _salesOrder);
                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, salesOrderHistoryGuid, _salesOrder.FROM_SOURCE, UserActions.UPDATE_REVIEW, systemMessage, userId, currentDatetime);
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert system sales order information history.";
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
                            error.MESSAGE = "Failed to commit sales order status update.";
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
                    error.MESSAGE = "Failed to update system sales order status.";
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

                string values = JsonConvert.SerializeObject(_salesOrder, Formatting.Indented, new JsonSerializerSettings
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

        ////[HttpPut]
        ////[Route("DeleteProduct")]
        ////public HttpResponseMessage DeleteProduct([FromBody] Product _product)
        ////{
        ////    ApiCommonController apiCommon = new ApiCommonController();

        ////    Error error = new Error();

        ////    DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
        ////    Hashtable sqlParams = new Hashtable();
        ////    SqlCommand nonQueryTransaction = null;

        ////    DateTime currentDatetime = DateTime.Now;
        ////    string userId = _product.UPDATE_ID;

        ////    try
        ////    {
        ////        string productsHistoryGuid = Guid.NewGuid().ToString();

        ////        sqlParams["@productsHistoryGuid"] = productsHistoryGuid;
        ////        sqlParams["@productId"] = _product.PRODUCT_ID;
        ////        sqlParams["@activeFlag"] = "N";
        ////        sqlParams["@deleteFlag"] = "Y";
        ////        sqlParams["@updateDatetime"] = currentDatetime;
        ////        sqlParams["@updateId"] = userId;

        ////        nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

        ////        if (nonQueryTransaction != null)
        ////        {
        ////            string sql = @"UPDATE PRODUCTS SET PRODUCT_DELETE_FLAG = @deleteFlag,
        ////                                UPDATE_DATETIME = @updateDatetime,
        ////                                UPDATE_ID = @updateId
        ////                            WHERE PRODUCT_ID = @productId ";
        ////            nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

        ////            if (nonQueryTransaction != null)
        ////            {
        ////                sql = @"INSERT INTO PRODUCTS_HISTORY (GUID, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_PRICE, QUANTITY,  PRODUCT_ACTIVE_FLAG, PRODUCT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
        ////                            SELECT @productsHistoryGuid, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_PRICE, QUANTITY, @activeFlag, @deleteFlag, @updateDatetime, @updateId
        ////                            FROM PRODUCTS WHERE PRODUCT_ID = @productId ";
        ////                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
        ////                if (nonQueryTransaction != null)
        ////                {
        ////                    string systemMessage = GenerateUserActivityLog(UserActions.DELETE, null, _product);
        ////                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, productsHistoryGuid, _product.FROM_SOURCE, UserActions.DELETE, systemMessage, userId, currentDatetime);
        ////                }
        ////                else
        ////                {
        ////                    error.MESSAGE = "Failed to commit delete product.";
        ////                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
        ////                    return response;
        ////                }

        ////            }
        ////            else
        ////            {
        ////                error.MESSAGE = "Failed to delete product";
        ////                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
        ////                return response;
        ////            }
        ////        }
        ////        else
        ////        {
        ////            error.MESSAGE = "Failed to connect database";
        ////            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
        ////            return response;
        ////        }
        ////        if (nonQueryTransaction != null)
        ////        {
        ////            if (daSQL.EndTransaction(apiCommon.MethodName(), userId, ref nonQueryTransaction))
        ////            {
        ////                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
        ////                return response;
        ////            }
        ////            else
        ////            {
        ////                if (nonQueryTransaction != null)
        ////                {
        ////                    daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
        ////                }
        ////                error.MESSAGE = "Failed to insert user acitivty log.";
        ////                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
        ////                return response;
        ////            }
        ////        }
        ////        else
        ////        {
        ////            error.MESSAGE = "Failed to insert product history";
        ////            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
        ////            return response;
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        if (nonQueryTransaction != null)
        ////        {
        ////            daSQL.EndTransactionRollback(apiCommon.MethodName(), userId, ref nonQueryTransaction);
        ////        }

        ////        string values = JsonConvert.SerializeObject(_product, Formatting.Indented, new JsonSerializerSettings
        ////        {
        ////            NullValueHandling = NullValueHandling.Ignore
        ////        });
        ////        apiCommon.WebApiLog(LogType.ERROR_TYPE, _product.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());
        ////        error.METHOD_NAME = apiCommon.MethodName();
        ////        error.TYPE = ex.GetType().ToString();
        ////        error.MESSAGE = ex.ToString();

        ////        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
        ////        return response;
        ////    }
        ////}
        #endregion

        #region Common
        private string GenerateUserActivityLog(string action, IEnumerable<SalesOrder> returnVal, SalesOrder _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            string systemMsg = string.Empty;

            try
            {
                if (action.Equals(UserActions.INSERT))
                {
                    systemMsg += @"Insert sales order: [" + _values.SALES_ORDER_ID + "]";
                    systemMsg += apiCommon.GetParamMsg("order date", _values.ORDER_DATETIME.ToString("yyyy-MM-dd"));
                    systemMsg += apiCommon.GetParamMsg("total amount", _values.TOTAL_AMOUNT.ToString("F2"));
                    systemMsg += apiCommon.GetParamMsg("order status", _values.ORDER_STATUS);
                }
                else if (action.Equals(UserActions.EDIT))
                {
                    SalesOrder currObj = returnVal.First();

                    systemMsg += @"Edit sales order: [" + currObj.SALES_ORDER_ID + "]";

                    //if (currObj.ORDER_DATETIME != _values.ORDER_DATETIME)
                    //{
                    //    systemMsg += apiCommon.GetParamMsg("order date", currObj.ORDER_DATETIME.ToString("yyyy-MM-dd"), _values.ORDER_DATETIME.ToString("yyyy-MM-dd"));
                    //}

                    if (currObj.TOTAL_AMOUNT != _values.TOTAL_AMOUNT)
                    {
                        systemMsg += apiCommon.GetParamMsg("total amount", currObj.TOTAL_AMOUNT.ToString("F2"), _values.TOTAL_AMOUNT.ToString("F2"));
                    }

                    if (currObj.ORDER_STATUS != _values.ORDER_STATUS)
                    {
                        systemMsg += apiCommon.GetParamMsg("order status", currObj.ORDER_STATUS, _values.ORDER_STATUS);
                    }


                }
                else if (action == UserActions.UPDATE_REVIEW)
                {
                    string title = (_values.SALES_ORDER_ACCEPT_FLAG == "Y") ? "Accept" : "Reject";
                    systemMsg += title + " sales order: [" + _values.SALES_ORDER_ID + "]";

                    string currReview = (_values.SALES_ORDER_ACCEPT_FLAG == "Y") ? "Accept" : "Reject";
                    string newReview = (_values.SALES_ORDER_ACCEPT_FLAG == "Y") ? "Reject" : "Accept";

                    systemMsg += apiCommon.GetParamMsg("review", currReview, newReview);
                }
                else if (action == UserActions.DELETE)
                {
                    systemMsg += @"Delete sales order: [" + _values.SALES_ORDER_ID + "]";
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