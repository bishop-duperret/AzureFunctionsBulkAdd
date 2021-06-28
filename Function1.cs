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
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace AzureFunctionsBulkAdd
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", "post", Route = null)] HttpRequest req,
            ILogger log)
        {


            
            // read body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            //convert body into dynamic obj
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            string tableName = data.tableName;

            if (string.IsNullOrEmpty(tableName)) ReturnError("Must specify tableName in body");
           
            if (data.list == null) ReturnError("list cannot be null");


            //use a dynamic obj (e.g expandoObject) to dynamically insert row
            List<ExpandoObject> list = new List<ExpandoObject>();

            JArray objects = (JArray)data.list; //convert list to JArray

            foreach ( var obj in objects)
            {

               //TODO Measure the fastest way to do this
                list.Add( obj.ToObject<ExpandoObject>()); //convert JObject to expando
            }

            
            // if table exists, insert data
            if (SQLHelper.TableExists(tableName))
            {
                try
                {
                    //insert data using BulkCopy
                      BulkAddWorker.BulkCopy( list, tableName, Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING"));

                }
                catch (Exception e)
                {
                    //return 404  if exception 
                    return InternalError(e.ToString());

                }

            }
            else ReturnError("Table does not exist in database");


           
            //TODO add functionality to create table if one doesn't exist



            string responseMessage = "success";

            return new OkObjectResult(responseMessage);
        }

        static ActionResult ReturnError (string error)
        {


             return new BadRequestObjectResult( error);
        }

        static ActionResult InternalError(string error)
        {


            var errorObjectResult = new ObjectResult(error);
            errorObjectResult.StatusCode = StatusCodes.Status500InternalServerError;

            return errorObjectResult;

        }

    }
}
