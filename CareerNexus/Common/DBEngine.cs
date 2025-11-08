using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;
using System.Diagnostics;

namespace CareerNexus.Common
{
    public static class DBEngine 
    {
        private static int result = 0;
        private static readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=CareerNexus;User Id=Adeel123;Password=test123;TrustServerCertificate=True;";
        private static readonly Serilog.ILogger _logger = Log.ForContext(typeof(DBEngine));


        #region ExecuteNonQuery
        public static bool ExecuteNonQuery(SqlCommand cmd,Databaseoperations? databaseoperations = null, string? query = null)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                cmd.Connection = conn;
                cmd.CommandTimeout = int.MaxValue;

                var operation = GetOperationName(databaseoperations);

                if (!databaseoperations.HasValue)
                {
                    operation = "SQL Query";
                }

                if (!string.IsNullOrWhiteSpace(query) || databaseoperations.HasValue)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    result = cmd.ExecuteNonQuery();

                    timer.Stop();
                    TimeSpan timeTaken = timer.Elapsed;

                    var timeDiff = timeTaken.ToString(@"m\:ss\.fff");

                    LogDBActionWithTime(operation, timeDiff, query);
                }

                else
                {
                    result = cmd.ExecuteNonQuery();
                }

                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

                conn.Dispose();
            }
        }

        public static int ExecuteNonQueryRowCount(SqlCommand cmd, string? query = null)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandTimeout = int.MaxValue;

                Stopwatch timer = Stopwatch.StartNew();
                result = cmd.ExecuteNonQuery();
                timer.Stop();

                LogQuery("ExecuteNonQueryRowCount", query, timer.Elapsed);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ExecuteNonQueryRowCount failed");
                throw;
            }
        }
        public static void LogDBActionWithTime(string? databaseOperation = null, string? timeDiff = null, string? sqlBlock = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sqlBlock))
                {
                    _logger.Debug($"Operation : {databaseOperation} | Execution Time : {timeDiff}");
                }

                else
                {
                    _logger.Debug($"Operation : {databaseOperation} | Execution Time : {timeDiff} | SQL Block : {sqlBlock}");
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
        public static string GetOperationName(Databaseoperations? databaseOperation)
        {
            var operation = string.Empty;

            switch (databaseOperation)
            {
                case Databaseoperations.Insert:
                    operation = "INSERT";
                    break;
                case Databaseoperations.Select:
                    operation = "SELECT";
                    break;
                case Databaseoperations.Update:
                    operation = "UPDATE";
                    break;
                case Databaseoperations.Delete:
                    operation = "DELETE";
                    break;
                case Databaseoperations.GetDataSet:
                    operation = "GETDATASET";
                    break;
                case Databaseoperations.GetDataTable:
                    operation = "GetDataTable";
                    break;
                case Databaseoperations.BulkUpdate:
                    operation = "BulkUpdate";
                    break;
                case Databaseoperations.BulkInsert:
                    operation = "BulkInsert";
                    break;
                case Databaseoperations.SQLBlock:
                    operation = "SQLBlock";
                    break;
            }

            return operation;
        }
        #endregion

        #region ExecuteScalar
        public static long ExecuteScalar(SqlCommand cmd, Databaseoperations? databaseOperation = null, string? query = null, bool? logFlagOnAndOff = null)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                long value = 0;
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                cmd.Connection = conn;
                cmd.CommandTimeout = int.MaxValue;

                var operation = GetOperationName(databaseOperation);

                if (!databaseOperation.HasValue)
                {
                    operation = "SQL Query";
                }

                if (!string.IsNullOrWhiteSpace(query) || databaseOperation.HasValue)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    var firstColumn = cmd.ExecuteScalar();

                    timer.Stop();
                    TimeSpan timeTaken = timer.Elapsed;
                    var timeDiff = timeTaken.ToString(@"m\:ss\.fff");

                    LogDBActionWithTime(operation, timeDiff, query);

                    if (firstColumn != null)
                    {
                        value = Convert.ToInt64(firstColumn);
                    }
                }

                else
                {
                    var firstColumn = cmd.ExecuteScalar();
                    if (firstColumn != null)
                    {
                        value = Convert.ToInt64(firstColumn);
                    }
                }


                return value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

                conn.Dispose();
            }
        }
        #endregion

        #region GetDataTable
        public static DataTable GetDataTable(SqlCommand cmd, Databaseoperations? databaseOperation = null, string? query = null, bool? logFlagOnAndOff = null)
        {
            cmd.CommandTimeout = int.MaxValue;
            DataTable dt = new DataTable();

            var operation = GetOperationName(databaseOperation);

            if (!databaseOperation.HasValue)
            {
                operation = "SQL Query";
            }

            if (!string.IsNullOrWhiteSpace(query) || databaseOperation.HasValue)
            {
                var timer = new Stopwatch();
                timer.Start();

                dt = GetDataSet(cmd).Tables[0];

                timer.Stop();
                TimeSpan timeTaken = timer.Elapsed;
                var timeDiff = timeTaken.ToString(@"m\:ss\.fff");

                if (logFlagOnAndOff == null || logFlagOnAndOff == true)
                {
                    LogDBActionWithTime(operation, timeDiff, query);
                }

            }

            else
            {
                dt = GetDataSet(cmd).Tables[0];
            }

            return dt;
        }
        public static DataSet GetDataSet(SqlCommand cmd, Databaseoperations? databaseOperation = null, string? query = null)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            DataSet ds = new DataSet();
            try
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                cmd.Connection = conn;
                cmd.CommandTimeout = int.MaxValue;

                var operation = GetOperationName(databaseOperation);

                if (!databaseOperation.HasValue)
                {
                    operation = "SQL Query";
                }

                if (!string.IsNullOrWhiteSpace(query) || databaseOperation.HasValue)
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    adpt.Fill(ds);
                    adpt.Dispose();

                    timer.Stop();
                    TimeSpan timeTaken = timer.Elapsed;
                    var timeDiff = timeTaken.ToString(@"m\:ss\.fff");

                    LogDBActionWithTime(operation, timeDiff, query);
                }

                else
                {
                    SqlDataAdapter adpt = new SqlDataAdapter(cmd);
                    adpt.Fill(ds);
                    adpt.Dispose();
                }

                return ds;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

                conn.Dispose();
            }

        }
        #endregion

        #region BulkInsert
        public static void BulkInsert(DataTable dt, string tableName, List<SqlBulkCopyColumnMapping> columnMappings, string? query = null)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            using SqlBulkCopy bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = tableName,
                BulkCopyTimeout = int.MaxValue
            };

            foreach (var map in columnMappings)
                bulk.ColumnMappings.Add(map);

            try
            {
                conn.Open();

                Stopwatch timer = Stopwatch.StartNew();
                bulk.WriteToServer(dt);
                timer.Stop();

                LogQuery("BulkInsert", query, timer.Elapsed);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "BulkInsert failed");
                throw;
            }
        }
        public static async Task<SqlDataReader> ExecuteReaderAsync(SqlCommand cmd, Databaseoperations? databaseOperation = null, string? query = null)
        {
            var conn = new SqlConnection(connectionString);
            try
            {
                await conn.OpenAsync();
                cmd.Connection = conn;
                cmd.CommandTimeout = int.MaxValue;

                var operation = GetOperationName(databaseOperation);

                if (!databaseOperation.HasValue)
                {
                    operation = "SQL Query";
                }

                var timer = new Stopwatch();
                timer.Start();

                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection); // ensures conn closes with reader.Close()

                timer.Stop();
                TimeSpan timeTaken = timer.Elapsed;
                var timeDiff = timeTaken.ToString(@"m\:ss\.fff");

                // Log execution time
                if (!string.IsNullOrWhiteSpace(query) || databaseOperation.HasValue)
                {
                    LogDBActionWithTime(operation, timeDiff, query);
                }

                return reader;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error executing async reader for query: {Query}", query);
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
                conn.Dispose();
                throw;
            }
        }
        #endregion

        #region Transactional Execution
        public static bool ExecuteNonQueryWithTransaction(List<SqlCommand> commands, string? query = null)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction txn = conn.BeginTransaction();

            try
            {
                foreach (var cmd in commands)
                {
                    cmd.Connection = conn;
                    cmd.Transaction = txn;
                    cmd.CommandTimeout = int.MaxValue;
                    result = cmd.ExecuteNonQuery();

                    if (result <= 0)
                        throw new Exception("One of the queries failed");
                }

                txn.Commit();
                return true;
            }
            catch (Exception ex)
            {
                txn.Rollback();
                _logger.Error(ex, "Transaction rolled back");
                return false;
            }
        }
        #endregion

        #region Log
        private static void LogQuery(string operation, string? query, TimeSpan timeTaken)
        {
            _logger.Information("[{Operation}] executed in {Time} - SQL: {Query}", operation, timeTaken.ToString(@"m\:ss\.fff"), query ?? "N/A");
        }
        #endregion
    }
}
