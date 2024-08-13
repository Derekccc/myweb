using MY_WEBSITE_API.Controllers.Common;
using Logger.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DatabaseAccessor.DatabaseAccessor
{
    public class DatabaseAccessorMSSQL
    {
        private bool ConnectDatabase(out SqlConnection Connection)
        {
            #region //MS SQL
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString_DB"].ConnectionString;//Configuration.GetConnectionString("DefaultConnection");
                Connection = new SqlConnection(connectionString);
                Connection.Open();
                return true;
            }
            catch (Exception ex)
            {

                ApiCommonController apiCommonController = new ApiCommonController();
                apiCommonController.WebApiLog(LogType.ERROR_TYPE, "", apiCommonController.MethodName(), ex.ToString());

                Connection = null;
                return false;
            }
            #endregion
        }

        private bool ConnectDatabase(out SqlConnection Connection, string connectionString)
        {
            #region //MS SQL
            try
            {
                Connection = new SqlConnection(connectionString);
                Connection.Open();
                return true;
            }
            catch (Exception ex)
            {

                ApiCommonController apiCommonController = new ApiCommonController();
                apiCommonController.WebApiLog(LogType.ERROR_TYPE, "", apiCommonController.MethodName(), ex.ToString());

                Connection = null;
                return false;
            }
            #endregion
        }

        private void DisconnectDatabase(ref SqlConnection Connection)
        {
            try
            {
                #region //MS SQL
                if (Connection != null)
                {
                    Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ApiCommonController apiCommonController = new ApiCommonController();
                apiCommonController.WebApiLog(LogType.ERROR_TYPE, "", apiCommonController.MethodName(), ex.ToString());
                DisconnectDatabase2(ref Connection);
            }
        }
        private void DisconnectDatabase2(ref SqlConnection Connection)
        {
            try
            {
                #region //MS SQL
                if (Connection != null)
                {
                    Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
                #endregion
            }
            catch (Exception ex)
            {
                ApiCommonController apiCommonController = new ApiCommonController();
                apiCommonController.WebApiLog(LogType.ERROR_TYPE, "", apiCommonController.MethodName(), ex.ToString());

            }
        }

        public DataTable ExecuteQuery(string methodName, String userID, string Sql, Hashtable SqlParams)
        {
            #region //MS SQL (SELECT, INSERT, UPDATE, DELETE)
            DataTable dt = new DataTable("Data");
            SqlConnection Connection = null;

            try
            {
                if (ConnectDatabase(out Connection))
                {
                    using (SqlCommand Command = Connection.CreateCommand())
                    {
                        Command.CommandTimeout = 300;//600; //0 = wait forever
                        Command.CommandText = Sql;

                        if (SqlParams != null)
                        {
                            foreach (DictionaryEntry Parameter in SqlParams)
                            {
                                object value = Parameter.Value;

                                if (value == null)
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                                }
                                else
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                                }
                            }
                        }

                        using (SqlDataReader dr = Command.ExecuteReader())
                        {
                            dt.Load(dr);
                        }

                        int RowCount = (dt == null) ? 0 : dt.Rows.Count;

                        if (RowCount > 0)
                        {
                            try
                            {
                                if (dt.Rows[0]["SystemTranError"].ToString().Length > 0)
                                {
                                    String SystemTranError = dt.Rows[0]["SystemTranError"].ToString();
                                    HandleError(methodName, Sql, SqlParams, SystemTranError, userID, "Api");
                                    return null;
                                }
                            }
                            catch (Exception ex)
                            {
                                //query no transaction error
                            }
                        }
                    }

                    DisconnectDatabase(ref Connection);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (Connection != null)
                {
                    DisconnectDatabase(ref Connection);
                }
                HandleError(methodName, Sql, SqlParams, ex.Message, userID, "Api");
                return null;
            }

            return dt;
            #endregion

        }

        public DataTable ExecuteNonQuery(string methodName, String userID, List<string> Sql, Hashtable SqlParams, string ReturnSql)
        {
            #region //MS SQL (SELECT, INSERT, UPDATE, DELETE) (If have RetuenSql, will select output)
            DataTable dt = new DataTable("Data");
            SqlConnection Connection = null;
            try
            {
                if (ConnectDatabase(out Connection))
                {
                    using (SqlCommand Command = Connection.CreateCommand())
                    {
                        Command.Transaction = Connection.BeginTransaction();
                        Command.CommandTimeout = 300;//600; //0 = wait forever

                        if (SqlParams != null)
                        {
                            foreach (DictionaryEntry Parameter in SqlParams)
                            {
                                object value = Parameter.Value;

                                if (value == null)
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                                }
                                else
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                                }
                            }
                        }

                        for (int i = 0; i < Sql.Count; i++)
                        {
                            Command.CommandText = Sql[i];
                            int rowAffected = Command.ExecuteNonQuery();
                        }

                        if (ReturnSql != null)
                        {
                            if (ReturnSql != String.Empty)
                            {
                                Command.CommandText = ReturnSql;
                                using (SqlDataReader dr = Command.ExecuteReader())
                                {
                                    dt.Load(dr);
                                }
                            }
                        }

                        Command.Transaction.Commit();
                    }

                    DisconnectDatabase(ref Connection);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (Connection != null)
                {
                    DisconnectDatabase(ref Connection);
                }

                string Error_SqlStr = String.Empty;
                for (int i = 0; i < Sql.Count(); i++)
                {
                    Error_SqlStr += Sql[i] + System.Environment.NewLine;
                }
                if (ReturnSql != null)
                {
                    if (ReturnSql != String.Empty)
                    {
                        Error_SqlStr += ReturnSql + System.Environment.NewLine;
                    }
                }
                HandleError(methodName, Error_SqlStr, SqlParams, ex.Message, userID, "Api");
                return null;
            }

            return dt;
            #endregion

        }

        public DataTable ExecuteNonQuery(string methodName, String userID, List<string> Sql, Hashtable SqlParams, string ReturnSql, string ConnectionString)
        {
            #region //MS SQL (SELECT, INSERT, UPDATE, DELETE) (If have RetuenSql, will select output)
            DataTable dt = new DataTable("Data");
            SqlConnection Connection = null;
            try
            {
                if (ConnectDatabase(out Connection, ConnectionString))
                {
                    using (SqlCommand Command = Connection.CreateCommand())
                    {
                        Command.Transaction = Connection.BeginTransaction();
                        Command.CommandTimeout = 300;//600; //0 = wait forever

                        if (SqlParams != null)
                        {
                            foreach (DictionaryEntry Parameter in SqlParams)
                            {
                                object value = Parameter.Value;

                                if (value == null)
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                                }
                                else
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                                }
                            }
                        }

                        for (int i = 0; i < Sql.Count; i++)
                        {
                            Command.CommandText = Sql[i];
                            int rowAffected = Command.ExecuteNonQuery();
                        }

                        if (ReturnSql != null)
                        {
                            if (ReturnSql != String.Empty)
                            {
                                Command.CommandText = ReturnSql;
                                using (SqlDataReader dr = Command.ExecuteReader())
                                {
                                    dt.Load(dr);
                                }
                            }
                        }

                        Command.Transaction.Commit();
                    }

                    DisconnectDatabase(ref Connection);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (Connection != null)
                {
                    DisconnectDatabase(ref Connection);
                }

                string Error_SqlStr = String.Empty;
                for (int i = 0; i < Sql.Count(); i++)
                {
                    Error_SqlStr += Sql[i] + System.Environment.NewLine;
                }
                if (ReturnSql != null)
                {
                    if (ReturnSql != String.Empty)
                    {
                        Error_SqlStr += ReturnSql + System.Environment.NewLine;
                    }
                }
                HandleError(methodName, Error_SqlStr, SqlParams, ex.Message, userID, "Api");
                return null;
            }

            return dt;
            #endregion

        }

        public SqlCommand BeginTransaction(string methodName)
        {
            SqlConnection Connection = null;
            try
            {
                if (ConnectDatabase(out Connection))
                {
                    SqlCommand Command = Connection.CreateCommand();
                    SqlTransaction trans = Connection.BeginTransaction();
                    Command.Transaction = trans;
                    return Command;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (Connection != null)
                {
                    DisconnectDatabase(ref Connection);
                }

                string errMsg = "Fail to connect Database. " + ex.Message;
                HandleError(methodName, null, null, errMsg, null, "Api");
                return null;
            }
        }

        public DataTable ExecuteQuery(string methodName, String userID, string Sql, Hashtable SqlParams, SqlCommand Command)
        {
            DataTable dt = new DataTable("Data");
            try
            {
                Command.CommandTimeout = 300;//600; //0 = wait forever
                Command.CommandText = Sql;

                if (SqlParams != null)
                {
                    Command.Parameters.Clear();

                    foreach (DictionaryEntry Parameter in SqlParams)
                    {
                        object value = Parameter.Value;

                        if (value == null)
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                        }
                        else
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                        }
                    }
                }

                using (SqlDataReader dr = Command.ExecuteReader())
                {
                    dt.Load(dr);
                }

                int RowCount = (dt == null) ? 0 : dt.Rows.Count;

                if (RowCount > 0)
                {
                    try
                    {
                        if (dt.Rows[0]["SystemTranError"].ToString().Length > 0)
                        {
                            String SystemTranError = dt.Rows[0]["SystemTranError"].ToString();
                            HandleError(methodName, Sql, SqlParams, SystemTranError, userID, "Api");
                            return null;
                        }
                    }
                    catch
                    {
                        //query no transaction error
                    }
                }
            }
            catch (Exception ex)
            {
                string Error_SqlStr = String.Empty;
                for (int i = 0; i < Sql.Count(); i++)
                {
                    Error_SqlStr += Sql[i] + System.Environment.NewLine;
                }
                HandleError(methodName, Error_SqlStr, SqlParams, ex.Message, userID, "Api");
                return null;
            }

            return dt;
        }

        public SqlCommand ExecuteNonQueryList(string methodName, String userID, List<string> Sql, Hashtable SqlParams, SqlCommand Command, string type)
        {
            //DataTable dt = new DataTable("Data");
            string sqlStr = string.Empty;
            try
            {
                Command.CommandTimeout = 300;//600; //0 = wait forever

                if (SqlParams != null)
                {
                    Command.Parameters.Clear();

                    foreach (DictionaryEntry Parameter in SqlParams)
                    {
                        object value = Parameter.Value;

                        if (value == null)
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                        }
                        else
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                        }
                    }
                }

                for (int i = 0; i < Sql.Count; i++)
                {
                    sqlStr = Sql[i];
                    Command.CommandText = sqlStr;
                    int rowAffected = Command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (Command != null)
                {
                    EndTransactionRollback(null, null, ref Command);
                }

                string Error_SqlStr = sqlStr + System.Environment.NewLine;

                //for (int i = 0; i < Sql.Count(); i++)
                //{
                //    Error_SqlStr += Sql[i] + System.Environment.NewLine;
                //}

                HandleError(methodName, Error_SqlStr, SqlParams, ex.Message, userID, type);

                return null;
            }

            return Command;
        }

        public SqlCommand ExecuteNonQuery(string methodName, String userID, string Sql, Hashtable SqlParams, SqlCommand Command, string type)
        {
            //DataTable dt = new DataTable("Data");
            try
            {
                Command.CommandTimeout = 300;//600; //0 = wait forever

                if (SqlParams != null)
                {
                    Command.Parameters.Clear();

                    foreach (DictionaryEntry Parameter in SqlParams)
                    {
                        object value = Parameter.Value;

                        if (value == null)
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                        }
                        else
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                        }
                    }
                }

                Command.CommandText = Sql;
                int rowAffected = Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (Command != null)
                {
                    EndTransactionRollback(null, null, ref Command);
                }

                string Error_SqlStr = Sql + System.Environment.NewLine;

                //for (int i = 0; i < Sql.Count(); i++)
                //{
                //    Error_SqlStr += Sql[i] + System.Environment.NewLine;
                //}

                HandleError(methodName, Error_SqlStr, SqlParams, ex.Message, userID, type);

                return null;
            }

            return Command;
        }

        public Boolean EndTransaction(string methodName, String userID, ref SqlCommand Command)
        {
            try
            {
                Command.CommandTimeout = 300;//600; //0 = wait forever

                Command.Transaction.Commit();

                SqlConnection tempConnection = Command.Connection;
                DisconnectDatabase(ref tempConnection);
                Command.Connection = tempConnection;

                Command.Dispose();
                Command = null;

                return true;
            }
            catch (Exception ex)
            {
                if (Command != null)
                {
                    EndTransactionRollback(null, null, ref Command);
                }
                string errorMsg = "Fail to commit SQL Transaction. " + ex.Message;
                HandleError(methodName, null, null, errorMsg, userID, "Api");
                return false;
            }
        }

        public Boolean EndTransactionRollback(string methodName, String userID, ref SqlCommand Command)
        {
            try
            {
                Command.CommandTimeout = 300;//600; //0 = wait forever

                Command.Transaction.Rollback();

                SqlConnection tempConnection = Command.Connection;
                DisconnectDatabase(ref tempConnection);
                Command.Connection = tempConnection;

                Command.Dispose();
                Command = null;

                return true;
            }
            catch (Exception ex)
            {
                if (Command.Connection != null)
                {
                    SqlConnection tempConnection = Command.Connection;
                    DisconnectDatabase(ref tempConnection);
                    Command.Connection = tempConnection;
                    Command.Dispose();
                    Command = null;
                }
                string errorMsg = "Fail to rollback SQL Transaction. " + ex.Message;
                HandleError(methodName, null, null, errorMsg, userID, "Api");
                return false;
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string methodName, String userID, string query, Hashtable SqlParams, string type)
        {
            IList<T> Datalist = new List<T>();
            SqlConnection Connection = null;

            try
            {
                if (ConnectDatabase(out Connection))
                {
                    using (SqlCommand Command = Connection.CreateCommand())
                    {
                        Command.CommandTimeout = 300;//600; //0 = wait forever
                        Command.CommandText = query;

                        if (SqlParams != null)
                        {
                            foreach (DictionaryEntry Parameter in SqlParams)
                            {
                                object value = Parameter.Value;

                                if (value == null)
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                                }
                                else
                                {
                                    Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                                }
                            }
                        }

                        using (SqlDataReader reader = Command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                T t = System.Activator.CreateInstance<T>();
                                Type obj = t.GetType();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    object tempValue = null;

                                    if (reader.IsDBNull(i))
                                    {
                                        tempValue = null;
                                    }
                                    else
                                    {
                                        tempValue = reader.GetValue(i);
                                    }
                                    obj.GetProperty(reader.GetName(i)).SetValue(t, tempValue, null);
                                }
                                Datalist.Add(t);
                            }
                        }
                    }

                    DisconnectDatabase(ref Connection);
                }
                else
                {
                    return null;
                }
                return Datalist;
            }
            catch (Exception ex)
            {
                if (Connection != null)
                {
                    DisconnectDatabase(ref Connection);
                }

                HandleError(methodName, query, SqlParams, ex.Message, userID, type);
                return null;
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string methodName, String userID, string query, Hashtable SqlParams, string type, SqlCommand Command)
        {
            IList<T> Datalist = new List<T>();
            DataTable dt = new DataTable("Data");
            try
            {
                Command.CommandTimeout = 300;//600; //0 = wait forever
                Command.CommandText = query;

                if (SqlParams != null)
                {
                    Command.Parameters.Clear();

                    foreach (DictionaryEntry Parameter in SqlParams)
                    {
                        object value = Parameter.Value;

                        if (value == null)
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), DBNull.Value));
                        }
                        else
                        {
                            Command.Parameters.Add(new SqlParameter(Parameter.Key.ToString(), Parameter.Value));
                        }
                    }
                }

                using (SqlDataReader reader = Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T t = System.Activator.CreateInstance<T>();
                        Type obj = t.GetType();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            object tempValue = null;

                            if (reader.IsDBNull(i))
                            {
                                tempValue = null;
                            }
                            else
                            {
                                tempValue = reader.GetValue(i);
                            }
                            obj.GetProperty(reader.GetName(i)).SetValue(t, tempValue, null);
                        }
                        Datalist.Add(t);
                    }
                }
                return Datalist;
            }
            catch (Exception ex)
            {
                //string Error_SqlStr = String.Empty;
                //for (int i = 0; i < query.Count(); i++)
                //{
                //    Error_SqlStr += query[i] + System.Environment.NewLine;
                //}
                //HandleError(methodName, Error_SqlStr, SqlParams, ex.Message, userID, type);

                HandleError(methodName, query, SqlParams, ex.Message, userID, type);

                return null;
            }
        }

        //MS SQL HandleError
        public void HandleError(string methodName, string Sql, Hashtable SqlParams, String errorMsg, String userID, string type)
        {
            String Params = getHashtableData(SqlParams);
            //String LogMsg = Sql + "\r\n" + " PARAS ( " + Params + ")" + "\r\n\r\n" + methodName + "\r\n" + errorMsg;
            String LogMsg = "";

            if (userID != null)
                LogMsg += "User Id: " + userID + "\r\n";

            if (methodName != null)
                LogMsg += methodName + "\r\n";

            if (Sql != null)
                LogMsg += Sql + "\r\n";

            if (SqlParams != null)
                LogMsg += " PARAS (" + Params + ")" + "\r\n";

            if (errorMsg != null)
                LogMsg += errorMsg + "\r\n";

            ILogger Log = LoggerFactory.Logger;

            if (type.Equals("Web"))
            {
                Log.WebLog(LogType.ERROR_TYPE, LogMsg);
            }
            else if (type.Equals("Api"))
            {
                Log.WebApiLog(LogType.ERROR_TYPE, LogMsg);
            }
        }

        protected void errorLog(string category, string errorMethod, string errorlog, string userID)
        {

            Hashtable sqlParams = new Hashtable();
            sqlParams["@CATEGORY_NAME"] = category;
            sqlParams["@MESSAGE_FROM"] = errorMethod;
            sqlParams["@MESSAGE_OTHER"] = errorlog;
            if (userID != null)
                sqlParams["@USER_ID"] = userID;
            else
                sqlParams["@USER_ID"] = "system";//ConfigurationManager.AppSettings["systemID"];

            String query = @" 
                INSERT INTO [SYSTEMLOG] ([CATEGORY_NAME],[DATETIME],[MESSAGE_FROM],[MESSAGE_OTHER],[UPDATE_BY]) VALUES
                (@CATEGORY_NAME,GETDATE(),@MESSAGE_FROM,@MESSAGE_OTHER,@USER_ID)
            ";
            List<String> queryList = new List<string>();
            queryList.Add(query);

            DataTable dt = ExecuteNonQuery("errorLog", userID, queryList, sqlParams, string.Empty);

        }

        protected string getHashtableData(Hashtable h)
        {
            string output = "";
            if (h == null)
            {
                output = "null";
            }
            else
            {
                foreach (string key in h.Keys)
                {
                    output += String.Format("{0}: {1} ;", key, h[key]);
                }
            }
            return output;
        }

    }
}