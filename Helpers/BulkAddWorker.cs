using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsBulkAdd.Helpers
{
    class BulkAddWorker
    {
        static readonly int BULK_COPY_TIMEOUT = 9000; 

        static readonly int BULK_COPY_BATCH_SIZE = 300000;


        /// <summary>
        /// 
        ///  This implementation uses BulkCopy and Fastmember to provide a high performant insert into
        ///  Azure SQL and SQL Server
        /// 
        /// </summary>
        /// <param name="rowsIn"></param>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>

        public static async Task BulkCopy(List<dynamic> rowsIn, string tableName,  string connectionString)
        {


            using (var bcp = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.TableLock)) { 


                string[] columnNames = SQLHelper.GetColumnName(tableName);



            // Note: columnNames MUST be in exact same order as the table 
            using (var reader = ObjectReader.Create(rowsIn, columnNames))  // this allows the bulk copy to write faster to tables
            {
                // sets timeout for batch insertion
                bcp.BulkCopyTimeout = BULK_COPY_TIMEOUT;


                // this can be tuned for better performance
                bcp.BatchSize = BULK_COPY_BATCH_SIZE;


                bcp.EnableStreaming = true;

                bcp.DestinationTableName = tableName;

                //inserts data asyncronously
                await bcp.WriteToServerAsync(reader);

                bcp.Close();



            } //end of ObjectReader using


            } //end of bulk copy using
        }



 


        


    }

}
