using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsBulkAdd.Helpers
{
    class SQLHelper
    {

        /// <summary>
        /// 
        ///  This method creates and open an SQL connection
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetSQLConnection()
        {

            // configuration manager is deprecated. Environment variable are now the reccomended approach
            string connectionString = System.Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

            //instantiate connection
            SqlConnection myConnection = new SqlConnection(connectionString);

            //open the connection
            myConnection.Open();

            //return Sql Connection
            return myConnection;


        }



        public void SwapStagingAndDestination(SqlConnection conn)
        {
            // Swap live and staging
            /* Note: An extra table, DataTable_Old, is required to temporarily hold the data being replaced before it is moved into DataTable_Staging. The rename-based approach did not require this extra table. */

            string firstTran = "BEGIN TRAN TRUNCATE TABLE #destinationTable;  ALTER TABLE #stagingTable SWITCH TO #destinationTable;  COMMIT";

            SqlCommand cmd2 = new SqlCommand(firstTran, conn);

            cmd2.ExecuteNonQuery();

        }

        /// <summary>
        ///  This method creates a staging table for when a table needs to be wiped and replaced
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tempTableName"></param>
        /// <param name="conn"></param>
        public void CreateStagingFromExisting(string sourceTable, string tempTableName, SqlConnection conn)
        {

            string createTemp = "SELECT TOP(0) * into  #tempTable FROM #sourceTable";

            //define destination temp table
            createTemp = createTemp.Replace("#tempTable", tempTableName);


            //define source table

            createTemp = createTemp.Replace("#sourceTable", sourceTable);


            SqlCommand cmd = new SqlCommand(createTemp, conn);
            cmd.ExecuteNonQuery();


        }

        /// <summary>
        /// 
        ///  Retrieves column names of a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string[] GetColumnName(string tableName)
        {

            List<string> columnName = new List<string>();


            string columnNameQuery = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{tableName}'";


            using (var connection = SQLHelper.GetSQLConnection())
            {
                SqlCommand command = new SqlCommand(columnNameQuery, connection);

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        columnName.Add((string)reader[3]);
                    }

                }
            }


            return columnName.ToArray();

        }

        public static bool TableExists (string tableName)
        {
            bool exists = false;

            using ( var sqlConnection = SQLHelper.GetSQLConnection()) { 

            var checkTableIfExistsCommand = new SqlCommand("IF EXISTS (SELECT 1 FROM sysobjects WHERE name =  '" + tableName + "') SELECT 1 ELSE SELECT 0", sqlConnection);

                exists = checkTableIfExistsCommand.ExecuteScalar().ToString().Equals("1");
            }

            return exists;


        }

        /// <summary>
        /// 
        /// CreateTable : Creates a SQL table based on the DataTable row object.
        /// See GetCreateTableDDL to see supported types
        /// 
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="table"></param>
        /// <param name="sqlConnection"></param>

        public void CreateTable(string tableName, DataTable table, SqlConnection sqlConnection)
        {

            // check if connection is not open. If not open
            if (sqlConnection.State != ConnectionState.Open) { sqlConnection.Open(); }
            

          
            if ( TableExists(tableName))
            {
                string createStatement = GetCreateTableDDL(tableName, table);

                var createTableCommand = new SqlCommand(createStatement, sqlConnection);

                createTableCommand.ExecuteNonQuery();

            }
               
           

        }


        /// <summary>
        /// 
        ///  Creates the string to create sql table 
        ///  
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="table"></param>
        /// <returns></returns>

        public static string GetCreateTableDDL(string tableName, DataTable table)
        {
            var ddl = new StringBuilder();
            ddl.AppendLine($"create table [{tableName}] (");
            foreach (DataColumn col in table.Columns)
            {
                ddl.Append($"  [{col.ColumnName}] {GetSqlType(col.DataType)}, ");
            }
            ddl.Length = ddl.Length - ", ".Length;
            ddl.Append(")");

            return ddl.ToString();
        }


        static string GetSqlType(Type dataTableColunmType)
        {
            //per type mappings here 
            //https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
            if (dataTableColunmType == typeof(string) || dataTableColunmType == typeof(String))
            {
                return "nvarchar(250)";
            }
            else if (dataTableColunmType == typeof(int) || dataTableColunmType == typeof(Int64))
            {
                return "int";
            }
            else if (dataTableColunmType == typeof(Single))
            {
                return "real";
            }
            else if (dataTableColunmType == typeof(double))
            {
                return "float";
            }
            else if (dataTableColunmType == typeof(float))
            {
                return "float";
            }
            else if (dataTableColunmType == typeof(DateTime))
            {
                return "datetime";
            }
            else if (dataTableColunmType == typeof(DateTime?))
            {
                return "datetime";
            }
            else if (dataTableColunmType == typeof(byte[]))
            {
                return "varbinary(max)";
            }

            else if (dataTableColunmType == typeof(Guid))
            {
                return "uniqueidentifier";
            }
            else if (dataTableColunmType == typeof(DateTimeOffset))
            {
                return "datetimeoffset";
            }
            else if (dataTableColunmType == typeof(bool))
            {
                return "bit";
            }

            else
            {
                throw new NotSupportedException($"Type {dataTableColunmType.Name} not supported");
            }


        }
    }
}
