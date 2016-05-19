using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.WindowsAzure.Storage.Table;
using ClassLibrary;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static CloudQueue queue = null;
        private static CloudTable table = null;

        [WebMethod]
        public void CalculateSumUsingWorkerRole(int a, int b, int c)
        {
            if (queue == null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Retrieve a reference to a container.
                queue = queueClient.GetQueueReference("numbersum");

                // Create the queue if it doesn't already exist
                queue.CreateIfNotExists();
            }
            

            CloudQueueMessage message = new CloudQueueMessage(a + "," + b + "," + c);
            queue.AddMessage(message);
        }

        [WebMethod]
        public List<string> ReadSumFromTableStorage()
        {
            if (table == null)
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));
                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                table = tableClient.GetTableReference("sumresults");

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();
            }

            TableQuery<Sum> query = new TableQuery<Sum>();

            List<string> results = new List<string>();
            foreach (Sum entity in table.ExecuteQuery(query))
            {
                results.Add(entity.PartitionKey.ToString());
            }
            return results;
        }
    }
}
