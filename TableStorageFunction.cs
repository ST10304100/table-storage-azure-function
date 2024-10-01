using Azure.Data.Tables;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FileShareFunctionApp
{
    public class TableStorageFunction
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<TableStorageFunction> _logger; // Add a logger field

        public TableStorageFunction(ILogger<TableStorageFunction> logger)
        {
            _logger = logger;
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("Products");
            _tableClient.CreateIfNotExists();
        }

        [Function("AddProductFunction")]
        public async Task<IActionResult> Run(

    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {

            _logger.LogInformation("AddProductFunction processed a request for a product");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Product product = JsonConvert.DeserializeObject<Product>(requestBody);

            if (product == null)
            {
                return new BadRequestObjectResult("Invalid product data."); // Return an error message indicating invalid data.
            }


            await _tableClient.AddEntityAsync(product); // Use the instance's TableClient to add the entity.

            string responseMessage = $"Product {product.Name} added successfully.";


            return new OkObjectResult(responseMessage);
        }

    }

    // Define the Product class implementing ITableEntity
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Ensure the property names match what you are setting in the controller
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ProductDescription { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public string ImageUrlPath { get; set; }  // Add this if you want to store the image URL


    }
}
