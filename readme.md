# Azure Functions HTTP BulkCopy

This is an Azure function implementation of SQL Bulk Copy. This project is a straight-forward high performance Bulk Insert. The HTTP function takes a table name and a list of json objects in HTTP body and using dynamic mapping, inserts them into the sql table.
Performance is accomplished using SqlBulkCopy and FastMember ObjectReader which
improves reflection performance.

A similar implementation is outlined [here: # Bulk Upload In .NET Core](https://www.c-sharpcorner.com/article/bulk-upload-in-net-core/)
## Release Notes
Currently only supporting Insert. Adding table replace and upsert.

## Features
Dynamically insert data into any table with matching schema
## Deploy

The Sql connection string ("SQL_CONNECTION_STRING) must be set as an enviroment variable locally or in Application Settings in Azure. 
Once it is set on your dev environment, you can begin testing locally. When ready to publish, ensure that the  enviroment variable is set in your Application Settings.


## Performance

As testing is being standardized, performance will be posted soon.

## Limitations
Currently this can only insert data into a table within the same databse. Will add support for tables within same SQL server.


