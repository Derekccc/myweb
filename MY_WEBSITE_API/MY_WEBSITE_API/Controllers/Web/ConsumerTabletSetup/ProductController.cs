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
using MY_WEBSITE_API.Models.Web.ConsumerTabletSetup;
using System.Data;
using System.IO;
using System.Net.Http.Headers;

namespace MY_WEBSITE_API.Controllers.Web.ConsumerTabletSetup
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        private const string API_TYPE = "Api";
        private const string PAGE_NAME = "Product Maintenance";

        #region Http Get
        [HttpGet]
        [Route("GetAllProducts")]
        public HttpResponseMessage GetAllProducts(string _sysUser, string _productName)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            try
            {
                string sql = @"SELECT PD.PRODUCT_ID, PD.PRODUCT_NAME, PD.PRODUCT_DESC, PD.UNIT_COST, PD.UNIT_SELLING_PRICE, PD.QUANTITY, PD.PRODUCT_ACTIVE_FLAG, (SU.USER_ID + ' - ' + SU.USER_NAME) AS UPDATE_ID, PD.UPDATE_DATETIME 
                                FROM PRODUCTS PD 
                                LEFT JOIN USERS SU ON PD.UPDATE_ID = SU.USER_ID
                                WHERE PD.PRODUCT_DELETE_FLAG = 'N'
                                ";
                #region Filter
                if ((!string.IsNullOrEmpty(_productName)) && _productName != "undefined")
                {
                    sqlParams["@productName"] = _productName;
                    sql += @" AND PD.PRODUCT_NAME LIKE ('%'+@productName+'%')";
                }
                
                sql += " ORDER BY PD.[UPDATE_DATETIME] DESC";
                #endregion

                IEnumerable<Product> returnVal = daSQL.ExecuteQuery<Product>(apiCommon.MethodName(), _sysUser, sql, sqlParams, API_TYPE);

                if (returnVal != null)
                {
                    if (returnVal.Count() == 0)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                        return response;
                    }
                    else
                    {
                        foreach (Product r in returnVal)
                            r.UPDATE_DATETIME = r.UPDATE_DATETIME.AddTicks(-(r.UPDATE_DATETIME.Ticks % TimeSpan.TicksPerSecond));
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, returnVal);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Failed to get product list";
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
        public HttpResponseMessage ExportProductExcel([FromUri] string _sysUser, string _productName, string _productId)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            string currentDatetime = DateTime.Now.ToString("_ddMMyyyy_HHmmss");

            try
            {
                string selectProductSql = @"
                                            SELECT
                                                PR.PRODUCT_ID AS [Product ID],
                                                PR.PRODUCT_NAME AS [Product Name],
                                                PR.PRODUCT_DESC AS [Product Description],
                                                PR.UNIT_COST AS [Unit Cost],
                                                PR.UNIT_SELLING_PRICE AS [Unit Selling Price],
                                                PR.QUANTITY AS [Quantity],
                                                CASE WHEN(PR.PRODUCT_ACTIVE_FLAG = 'Y') THEN 'Active' ELSE 'Inactive' END AS [Status],
                                                (SU.USER_ID + ' - ' + SU.USER_NAME) AS [Last Update By],
                                                PR.UPDATE_DATETIME AS [Last Update Date Time]
                                            FROM PRODUCTS PR
                                            LEFT JOIN USERS SU ON PR.UPDATE_ID = SU.USER_ID
                                            WHERE PR.PRODUCT_DELETE_FLAG = 'N'
                                            ";

                if ((!string.IsNullOrEmpty(_productName)) && _productName != "undefined")
                {
                    sqlParams["@productName"] = _productName;
                    selectProductSql += @" AND PR.PRODUCT_NAME LIKE ('%'+@productName+'%')";
                }
                if ((!string.IsNullOrEmpty(_productId)) && _productId != "undefined")
                {
                    sqlParams["@productId"] = _productId;
                    selectProductSql += @" AND PR.PRODUCT_ID LIKE ('%'+@productId+'%')";
                }
                selectProductSql += " ORDER BY PR.[UPDATE_DATETIME] DESC ";

                DataTable dtResult = daSQL.ExecuteQuery(apiCommon.MethodName(), _sysUser, selectProductSql, sqlParams);

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
                    error.MESSAGE = "Cannot find products";

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
        [Route("InsertProduct")]
        public HttpResponseMessage InsertProduct([FromBody] Product _product)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _product.UPDATE_ID;
            try
            {
                string productsHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@productsHistoryGuid"] = productsHistoryGuid;
                sqlParams["@productName"] = _product.PRODUCT_NAME;
                sqlParams["@productDesc"] = _product.PRODUCT_DESC;
                sqlParams["@unitCost"] = _product.UNIT_COST;
                sqlParams["@unitSellingPrice"] = _product.UNIT_SELLING_PRICE;
                sqlParams["@quantity"] = _product.QUANTITY;
                sqlParams["@activeFlag"] = "Y";
                sqlParams["@deleteFlag"] = "N";
                sqlParams["@updateId"] = userId;
                sqlParams["@updateDatetime"] = currentDatetime;

                string sql = @" SELECT PRODUCT_ID FROM PRODUCTS WHERE UPPER(PRODUCT_NAME) = UPPER(@productName) AND PRODUCT_DELETE_FLAG = 'N'";
                IEnumerable<Product> returnValProduct = daSQL.ExecuteQuery<Product>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValProduct != null)
                {
                    if (returnValProduct.Count() == 0)
                    {
                        sqlParams["@productId"] = Guid.NewGuid().ToString();
                        sql = @" INSERT INTO PRODUCTS (PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY, PRODUCT_ACTIVE_FLAG, PRODUCT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                        VALUES (@productId, @productName, NULLIF(@productDesc, ''), @unitCost, @unitSellingPrice, @quantity, @activeFlag, @deleteFlag, @updateDatetime, @updateId)";
                    }
                    else
                    {
                        Product errorObj = new Product
                        {
                            DUPLICATE_PRODUCT_NAME = true,
                            PRODUCT_NAME = _product.PRODUCT_NAME
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
                        sql = @"INSERT INTO PRODUCTS_HISTORY (GUID, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY, PRODUCT_ACTIVE_FLAG, PRODUCT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    VALUES (@productsHistoryGuid, @productId, @productName, NULLIF(@productDesc, ''), @unitCost, @unitSellingPrice, @quantity, @activeFlag, @deleteFlag, @updateDatetime, @updateId)";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.INSERT, null, _product);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, productsHistoryGuid, _product.FROM_SOURCE, UserActions.INSERT, systemMessage, userId, currentDatetime);

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
                        error.MESSAGE = "Failed to insert product";
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
                            error.MESSAGE = "Failed to commit insert product";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert product history";
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                        return response;
                    }
                }
                else
                {
                    error.MESSAGE = "Error while checking product existance.";
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

                string values = JsonConvert.SerializeObject(_product, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _product.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

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
        [Route("UpdateProduct")]
        public HttpResponseMessage UpdateProduct([FromBody] Product _product)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            Error error = new Error();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;
            DateTime currentDatetime = DateTime.Now;
            string userId = _product.UPDATE_ID;

            try
            {
                string productsHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@productsHistoryGuid"] = productsHistoryGuid;
                sqlParams["@productId"] = _product.PRODUCT_ID;
                sqlParams["@productName"] = _product.PRODUCT_NAME;
                sqlParams["@productDesc"] = _product.PRODUCT_DESC;
                sqlParams["@unitCost"] = _product.UNIT_COST;
                sqlParams["@unitSellingPrice"] = _product.UNIT_SELLING_PRICE;
                sqlParams["@quantity"] = _product.QUANTITY;
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                string sql = @"SELECT PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY FROM PRODUCTS WHERE UPPER(PRODUCT_NAME) = UPPER(@productName) AND PRODUCT_DELETE_FLAG = 'N' ORDER BY [UPDATE_DATETIME] DESC";
                IEnumerable<Product> returnValProduct = daSQL.ExecuteQuery<Product>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);

                if (returnValProduct != null)
                {
                    if (returnValProduct.Count() > 0 && (returnValProduct.First().PRODUCT_ID != _product.PRODUCT_ID))
                    {
                        Product errorObj = new Product
                        {
                            DUPLICATE_PRODUCT_NAME = true,
                            PRODUCT_NAME = _product.PRODUCT_NAME
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
                        sql = @"SELECT PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY FROM PRODUCTS WHERE PRODUCT_ID = @productId";
                        IEnumerable<Product> productOri = daSQL.ExecuteQuery<Product>(apiCommon.MethodName(), userId, sql, sqlParams, API_TYPE, nonQueryTransaction);
                        if (productOri != null)
                        {
                            sql = @"UPDATE PRODUCTS SET PRODUCT_NAME = @productName,
                                    PRODUCT_DESC = NULLIF(@productDesc, ''),
                                    UNIT_COST = @unitCost,
                                    UNIT_SELLING_PRICE = @unitSellingPrice,
                                    QUANTITY = @quantity,
                                    UPDATE_DATETIME = @updateDatetime,
                                    UPDATE_ID = @updateId
                                WHERE PRODUCT_ID = @productId ";

                            nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                            if (nonQueryTransaction != null)
                            {
                                sql = @"INSERT INTO PRODUCTS_HISTORY (GUID, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY, PRODUCT_ACTIVE_FLAG, PRODUCT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                        SELECT @productsHistoryGuid, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY, PRODUCT_ACTIVE_FLAG, PRODUCT_DELETE_FLAG, @updateDatetime, @updateId
                                        FROM PRODUCTS WHERE PRODUCT_ID = @productId ";

                                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                                if (nonQueryTransaction != null)
                                {
                                    string systemMessage = GenerateUserActivityLog(UserActions.EDIT, productOri, _product);
                                    nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, productsHistoryGuid, _product.FROM_SOURCE, UserActions.EDIT, systemMessage, userId, currentDatetime);

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

                                            error.MESSAGE = "Failed to commit update product";

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
                                    error.MESSAGE = "Failed to insert product history";

                                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                                    return response;
                                }
                            }
                            else
                            {
                                error.MESSAGE = "Failed to update product";

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

                            error.MESSAGE = "Failed to get current product data";

                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }


                    }
                }
                else
                {
                    error.MESSAGE = "Failed to find product";
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

                string values = JsonConvert.SerializeObject(_product, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _product.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());

                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }

        [HttpPut]
        [Route("UpdateProductStatus")]
        public HttpResponseMessage UpdateProductStatus([FromBody] Product _product)
        {
            ApiCommonController apiCommon = new ApiCommonController();
            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();

            DateTime currentDatetime = DateTime.Now;
            SqlCommand nonQueryTransaction = null;
            Error error = new Error();
            string userId = _product.UPDATE_ID;
            try
            {
                string productsHistoryGuid = Guid.NewGuid().ToString();
                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                sqlParams["@guid"] = productsHistoryGuid;
                sqlParams["@productId"] = _product.PRODUCT_ID;
                sqlParams["@productActiveFlag"] = _product.PRODUCT_ACTIVE_FLAG;
                sqlParams["@updateDateTime"] = DateTime.Now;
                sqlParams["@updateId"] = userId;


                string sql = @"UPDATE [PRODUCTS] SET [PRODUCT_ACTIVE_FLAG] = @productActiveFlag, [UPDATE_DATETIME] = @updateDateTime, [UPDATE_ID] = @updateID
                                        WHERE [PRODUCT_ID] = @productId";
                nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                if (nonQueryTransaction != null)
                {
                    sql = @"INSERT INTO [PRODUCTS_HISTORY] ([GUID], [PRODUCT_ID], [PRODUCT_NAME], [PRODUCT_DESC], [UNIT_COST], [UNIT_SELLING_PRICE], [QUANTITY],
                            [PRODUCT_ACTIVE_FLAG], [PRODUCT_DELETE_FLAG],
                            [UPDATE_DATETIME], [UPDATE_ID])
                                SELECT @guid, [PRODUCT_ID], [PRODUCT_NAME], [PRODUCT_DESC], [UNIT_COST], [UNIT_SELLING_PRICE], [QUANTITY],
                                    [PRODUCT_ACTIVE_FLAG], [PRODUCT_DELETE_FLAG],
                                    [UPDATE_DATETIME], [UPDATE_ID] FROM [PRODUCTS]
                                WHERE [PRODUCT_ID] = @productId";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                    if (nonQueryTransaction != null)
                    {
                        string systemMessage = GenerateUserActivityLog(UserActions.UPDATE_STATUS, null, _product);
                        nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, productsHistoryGuid, _product.FROM_SOURCE, UserActions.UPDATE_STATUS, systemMessage, userId, currentDatetime);
                    }
                    else
                    {
                        error.MESSAGE = "Failed to insert system product information history.";
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
                            error.MESSAGE = "Failed to commit product status update.";
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
                    error.MESSAGE = "Failed to update system product status.";
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

                string values = JsonConvert.SerializeObject(_product, Formatting.Indented, new JsonSerializerSettings
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
        [Route("DeleteProduct")]
        public HttpResponseMessage DeleteProduct([FromBody] Product _product)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            Error error = new Error();

            DatabaseAccessorMSSQL daSQL = new DatabaseAccessorMSSQL();
            Hashtable sqlParams = new Hashtable();
            SqlCommand nonQueryTransaction = null;

            DateTime currentDatetime = DateTime.Now;
            string userId = _product.UPDATE_ID;

            try
            {
                string productsHistoryGuid = Guid.NewGuid().ToString();

                sqlParams["@productsHistoryGuid"] = productsHistoryGuid;
                sqlParams["@productId"] = _product.PRODUCT_ID;
                sqlParams["@activeFlag"] = "N";
                sqlParams["@deleteFlag"] = "Y";
                sqlParams["@updateDatetime"] = currentDatetime;
                sqlParams["@updateId"] = userId;

                nonQueryTransaction = daSQL.BeginTransaction(apiCommon.MethodName());

                if (nonQueryTransaction != null)
                {
                    string sql = @"UPDATE PRODUCTS SET PRODUCT_DELETE_FLAG = @deleteFlag,
                                        UPDATE_DATETIME = @updateDatetime,
                                        UPDATE_ID = @updateId
                                    WHERE PRODUCT_ID = @productId ";
                    nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);

                    if (nonQueryTransaction != null)
                    {
                        sql = @"INSERT INTO PRODUCTS_HISTORY (GUID, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY, PRODUCT_ACTIVE_FLAG, PRODUCT_DELETE_FLAG, UPDATE_DATETIME, UPDATE_ID)
                                    SELECT @productsHistoryGuid, PRODUCT_ID, PRODUCT_NAME, PRODUCT_DESC, UNIT_COST, UNIT_SELLING_PRICE, QUANTITY, @activeFlag, @deleteFlag, @updateDatetime, @updateId
                                    FROM PRODUCTS WHERE PRODUCT_ID = @productId ";
                        nonQueryTransaction = daSQL.ExecuteNonQuery(apiCommon.MethodName(), userId, sql, sqlParams, nonQueryTransaction, API_TYPE);
                        if (nonQueryTransaction != null)
                        {
                            string systemMessage = GenerateUserActivityLog(UserActions.DELETE, null, _product);
                            nonQueryTransaction = apiCommon.InsertUserActivityLog(nonQueryTransaction, productsHistoryGuid, _product.FROM_SOURCE, UserActions.DELETE, systemMessage, userId, currentDatetime);
                        }
                        else
                        {
                            error.MESSAGE = "Failed to commit delete product.";
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                            return response;
                        }

                    }
                    else
                    {
                        error.MESSAGE = "Failed to delete product";
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
                    error.MESSAGE = "Failed to insert product history";
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

                string values = JsonConvert.SerializeObject(_product, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                apiCommon.WebApiLog(LogType.ERROR_TYPE, _product.UPDATE_ID, apiCommon.MethodName(), "Values: " + values + "\r\n\r\n" + ex.ToString());
                error.METHOD_NAME = apiCommon.MethodName();
                error.TYPE = ex.GetType().ToString();
                error.MESSAGE = ex.ToString();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, error);
                return response;
            }
        }
        #endregion

        #region Common
        private string GenerateUserActivityLog(string action, IEnumerable<Product> returnVal, Product _values)
        {
            ApiCommonController apiCommon = new ApiCommonController();

            string systemMsg = string.Empty;

            try
            {
                if (action.Equals(UserActions.INSERT))
                {
                    systemMsg += @"Insert product: [" + _values.PRODUCT_NAME + "]";
                    systemMsg += apiCommon.GetParamMsg("product desc", _values.PRODUCT_DESC);
                }
                else if (action.Equals(UserActions.EDIT))
                {
                    Product currObj = returnVal.First();

                    systemMsg += @"Edit product: [" + currObj.PRODUCT_NAME + "]";

                    if (currObj.PRODUCT_NAME != _values.PRODUCT_NAME)
                    {
                        systemMsg += apiCommon.GetParamMsg("product name", currObj.PRODUCT_NAME, _values.PRODUCT_NAME);
                    }

                    if (currObj.PRODUCT_DESC != _values.PRODUCT_DESC)
                    {
                        systemMsg += apiCommon.GetParamMsg("product desc", currObj.PRODUCT_DESC, _values.PRODUCT_DESC);
                    }
                    if (currObj.UNIT_COST != _values.UNIT_COST)
                    {
                        systemMsg += apiCommon.GetParamMsg("unit cost", currObj.UNIT_COST.ToString("F2"), _values.UNIT_COST.ToString("F2"));
                    }
                    if (currObj.UNIT_SELLING_PRICE != _values.UNIT_SELLING_PRICE)
                    {
                        systemMsg += apiCommon.GetParamMsg("unit cost", currObj.UNIT_SELLING_PRICE.ToString("F2"), _values.UNIT_SELLING_PRICE.ToString("F2"));
                    }
                    if (currObj.QUANTITY != _values.QUANTITY)
                    {
                        systemMsg += apiCommon.GetParamMsg("unit price", currObj.QUANTITY.ToString(), _values.QUANTITY.ToString());
                    }


                }
                else if (action == UserActions.UPDATE_STATUS)
                {
                    string title = (_values.PRODUCT_ACTIVE_FLAG == "Y") ? "Activate" : "Deactivate";
                    systemMsg += title + " product name: [" + _values.PRODUCT_NAME + "]";

                    string currStatus = (_values.PRODUCT_ACTIVE_FLAG == "Y") ? "Active" : "Inactive";
                    string newStatus = (_values.PRODUCT_ACTIVE_FLAG == "Y") ? "Inactive" : "Active";

                    systemMsg += apiCommon.GetParamMsg("status", currStatus, newStatus);
                }
                else if (action == UserActions.DELETE)
                {
                    systemMsg += @"Delete delete: [" + _values.PRODUCT_NAME + "]";
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

