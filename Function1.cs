using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctionsBulkAdd.Helpers;
using System.Collections.Generic;

namespace AzureFunctionsBulkAdd
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
          

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            string tableName = data.tableName;

            List <dynamic>  list = data.list;

           await BulkAddWorker.BulkCopy(list, tableName, Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING"));


             

            string responseMessage = "";

            return new OkObjectResult(responseMessage);
        }


    }
}
