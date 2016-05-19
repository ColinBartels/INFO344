using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;
using System.Security.Cryptography;
using ClassLibrary;
using System.Web.Script.Services;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace WebRole1
{
    /// <summary>
    /// Summary description for CrawlerService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class CrawlerService : System.Web.Services.WebService
    {
        private static CloudQueue queue;
        private static CloudQueue admin;
        private static CloudQueue XMLQueue;
        private static CloudTable table;
        private static CloudTable stats;

        [WebMethod]
        public void StartCrawling()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   CloudConfigurationManager.GetSetting("StorageConnectionString"));
            if (admin == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                admin = queueClient.GetQueueReference("admin");
                admin.CreateIfNotExists();
            }
            if (XMLQueue == null)
            {
                
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                XMLQueue = queueClient.GetQueueReference("xmlqueue");
                XMLQueue.CreateIfNotExists();
            }
            if (queue == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference("urlqueue");
                queue.CreateIfNotExists();
            }
            queue.FetchAttributes();
            int? queueSize = queue.ApproximateMessageCount;
            XMLQueue.FetchAttributes();
            int? XMLQueueSize = XMLQueue.ApproximateMessageCount;

            if (queueSize == 0 && XMLQueueSize == 0)
            {
                addRobots();
            }

            CloudQueueMessage adminMessage = new CloudQueueMessage("start");
            admin.AddMessage(adminMessage);
        }

        [WebMethod]
        public void addRobots()
        {
            if (XMLQueue == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                XMLQueue = queueClient.GetQueueReference("xmlqueue");
                XMLQueue.CreateIfNotExists();
            }
            CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/robots.txt");
            XMLQueue.AddMessage(message);
            message = new CloudQueueMessage("http://www.bleacherreport.com/robots.txt");
            XMLQueue.AddMessage(message);
        }

        [WebMethod]
        public void StopCrawling()
        {
            if (admin == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                admin = queueClient.GetQueueReference("admin");
                admin.CreateIfNotExists();
            }
            CloudQueueMessage adminMessage = new CloudQueueMessage("stop");
            admin.AddMessage(adminMessage);
        }

        [WebMethod]
        public void ClearAll()
        {
            StopCrawling();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                  CloudConfigurationManager.GetSetting("StorageConnectionString"));
            if (table == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference("urltable");
            }
            table.DeleteIfExists();

            if (stats == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
            }
            stats.DeleteIfExists();

            if (queue == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference("urlqueue");
                queue.CreateIfNotExists();
            }
            queue.Clear();

            if (XMLQueue == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                XMLQueue = queueClient.GetQueueReference("xmlqueue");
                XMLQueue.CreateIfNotExists();
            }
            XMLQueue.Clear();

            if (admin == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                admin = queueClient.GetQueueReference("admin");
                admin.CreateIfNotExists();
            }
            admin.Clear();
        }

        [WebMethod]
        public string GetPageTitle(string url)
        {
            if (table == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                  CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference("urltable");
                table.CreateIfNotExists();
            }

            byte[] encodedUrl = new UTF8Encoding().GetBytes(url);

            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedUrl);

            string encoded = BitConverter.ToString(hash)
               .Replace("-", string.Empty)
               .ToLower();

            TableQuery<Page> query = new TableQuery<Page>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, encoded));
            var results = table.ExecuteQuery(query).ToArray();
            if (results.Count() > 0)
            {
                return new JavaScriptSerializer().Serialize(results[0].title);
            }
            else
            {
                return new JavaScriptSerializer().Serialize("URL not found");
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSystemInfo()
        {
            if (stats == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                  CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }
            TableQuery<SystemStats> query = new TableQuery<SystemStats>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "stats"));
            var results = stats.ExecuteQuery(query).ToArray();
            if (results.Length > 0)
            {
                return new JavaScriptSerializer().Serialize(results[0].memory + ":" + results[0].cpu);
            }
            else
            {
                return new JavaScriptSerializer().Serialize("0:0");
            }
            
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetLast10()
        {
            if (stats == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                     CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }
            TableOperation retrieveOperation = TableOperation.Retrieve<Last10>("last10", "last10");
            TableResult retrievedResult = stats.Execute(retrieveOperation);
            if (retrievedResult.Result != null && ((Last10)retrievedResult.Result).urls != null)
            {
                return new JavaScriptSerializer().Serialize(((Last10)retrievedResult.Result).urls);
            }
            else
            {
                return new JavaScriptSerializer().Serialize("");
            }
            
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSize()
        {
            int tableSize;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                     CloudConfigurationManager.GetSetting("StorageConnectionString"));
            if (stats == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }

            if (queue == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference("urlqueue");
                queue.CreateIfNotExists();
            }
            TableOperation retrieveOperation = TableOperation.Retrieve<Size>("size", "indexsize");
            TableResult retrievedResult = stats.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                tableSize = ((Size)retrievedResult.Result).size;
            }
            else
            {
                tableSize = 0;
            }

            int totalSize;
            TableOperation total = TableOperation.Retrieve<Size>("size", "totalsize");
            TableResult totalResult = stats.Execute(total);
            if (totalResult.Result != null)
            {
                totalSize = ((Size)totalResult.Result).size;
            }
            else
            {
                totalSize = 0;
            }

            queue.FetchAttributes();
            int? queueSize = queue.ApproximateMessageCount;
            return new JavaScriptSerializer().Serialize(totalSize.ToString() + ":" + queueSize.ToString() + ":" + tableSize.ToString());
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetErrors()
        {
            if (stats == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                     CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }

            TableQuery<Error> query = new TableQuery<Error>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "error"));

            List<Error> results = new List<Error>();
            foreach (Error entity in stats.ExecuteQuery(query))
            {
                results.Add(entity);
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStatus()
        {
            if (stats == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                     CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }
            TableOperation getStatus = TableOperation.Retrieve<Status>("status", "status");
            TableResult status = stats.Execute(getStatus);
            if (status.Result != null)
            {
                return new JavaScriptSerializer().Serialize(((Status)status.Result).status);
            }
            else
            {
                return new JavaScriptSerializer().Serialize("");
            }
        }
    }
}