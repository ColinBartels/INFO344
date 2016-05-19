using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.WindowsAzure.Storage.Table;
using System.Security.Cryptography;
using System.Text;
using ClassLibrary1;

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
        private static CloudQueue queue;
        private static CloudTable table;

        [WebMethod]
        public string CreateAccountInWorkerRole(string username)
        {
            if (queue == null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Retrieve a reference to a container.
                queue = queueClient.GetQueueReference("passwordqueue");

                // Create the queue if it doesn't already exist
                queue.CreateIfNotExists();
            }
            

            CloudQueueMessage message = new CloudQueueMessage(username);
            queue.AddMessage(message);

            return "Sucessfully created account";

        }

        [WebMethod]
        public string GetPasswordFromTableStorage(string username)
        {
            if (table == null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                table = tableClient.GetTableReference("accounts");

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<Account>(username, username);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
            {
                return "Password: " + ((Account)retrievedResult.Result).password;
            }
            else
            {
                return "User not found";
            }
           

        }

        [WebMethod]
        public string CountPasswords()
        {
            if (table == null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                table = tableClient.GetTableReference("accounts");

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<Counter>("counter", "counter");

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if ((Counter)retrievedResult.Result != null)
            {
                return "Passwords: " + ((Counter)retrievedResult.Result).count;
            }
            else
            {
                return "Counter Not Found";
            }
        }
    }
}
