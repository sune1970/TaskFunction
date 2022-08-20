using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TaskFunction
{
    public class Function1
    {
        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("ApiCallLog"), StorageAccount("AzureWebJobsStorage")] ICollector<ApiCallLog> table,
            [Blob("api-calls-blob/{DateTime}.txt", FileAccess.ReadWrite, Connection = "AzureWebJobsStorage")] CloudBlockBlob outputBlob,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await CallRandomApi(table, outputBlob);
        }

        public static async Task CallRandomApi(ICollector<ApiCallLog> table, CloudBlockBlob outputBlob)
        {
            string URL = " https://api.publicapis.org/random?auth=null";

            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(URL);

            if (response.IsSuccessStatusCode)
            {
                LogInfo(response, table);
                await StorePayloadInBlob(outputBlob, response);
            }
            else
            {
                LogError(response, table);
            }

            client.Dispose();
        }

        public static async Task StorePayloadInBlob(CloudBlockBlob outputBlob, HttpResponseMessage response)
        {
            string content = await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();
            await outputBlob.UploadTextAsync(content);
        }

        public static void LogError(HttpResponseMessage response, ICollector<ApiCallLog> table)
        {
            ApiCallLog log = new ApiCallLog
            {
                PartitionKey = "Error",
                RowKey = Guid.NewGuid().ToString(),
                Timetamp = DateTime.Now,
                Description = $"{(int)response.StatusCode}, {response.ReasonPhrase}"
            };

            table.Add(log);
        }

        public static void LogInfo(HttpResponseMessage response, ICollector<ApiCallLog> table)
        {
            ApiCallLog log = new ApiCallLog
            {
                PartitionKey = "Info",
                RowKey = Guid.NewGuid().ToString(),
                Timetamp = DateTime.Now,
                Description = $"{(int)response.StatusCode}, {response.ReasonPhrase}",
            };

            table.Add(log);
        }
    }

}
