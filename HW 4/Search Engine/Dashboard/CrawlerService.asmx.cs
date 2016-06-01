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
using System.Text.RegularExpressions;

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
        private static Dictionary<string, Tuple<string, DateTime>> cache;

        public CrawlerService()
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
            if (stats == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }
            if (table == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference("urltable");
                table.CreateIfNotExists();
            }
            if (cache == null)
            {
                cache = new Dictionary<string, Tuple<string, DateTime>>();
            }
        }

        [WebMethod]
        public void StartCrawling()
        {
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

        private void addRobots()
        {
            CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/robots.txt");
            XMLQueue.AddMessage(message);
            message = new CloudQueueMessage("http://www.bleacherreport.com/robots.txt");
            XMLQueue.AddMessage(message);
        }

        [WebMethod]
        public void StopCrawling()
        { 
            CloudQueueMessage adminMessage = new CloudQueueMessage("stop");
            admin.AddMessage(adminMessage);
        }

        [WebMethod]
        public void ClearAll()
        {
            StopCrawling();
            table.DeleteIfExists();
            stats.DeleteIfExists();
            queue.Clear();
            XMLQueue.Clear();
            admin.Clear();
        }

        [WebMethod]
        public string Search(string input)
        {
            if (cache.ContainsKey(input))
            {
                var threshhold = DateTime.Now.AddMinutes(-10);
                if (cache[input].Item2.AddMinutes(-10) <= threshhold)
                {
                    return cache[input].Item1;
                }
                else
                {
                    cache.Remove(input);
                }
            }

            List<Page> results = new List<Page>();

            var punctuation = input.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = input.ToLower().Split().Select(x => x.Trim(punctuation)).ToList();

            foreach (string word in words)
            {
                byte[] encodedWord = new UTF8Encoding().GetBytes(word);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedWord);
                string encoded = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                TableQuery<Page> query = new TableQuery<Page>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, encoded));
                var queryResults = table.ExecuteQuery(query).ToList();
                foreach (Page queryResult in queryResults)
                {
                    var Pagepunctuation = queryResult.title.Where(Char.IsPunctuation).Distinct().ToArray();
                    var Pagewords = queryResult.title.ToLower().Split().Select(x => x.Trim(punctuation)).ToList();
                    bool matches = words.All(x => Pagewords.Contains(x));
                    if (matches)
                    {
                        results.Add(queryResult);
                    }
                }
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string serialized;

            if (results.Count > 0)
            {
                var orderedResults = results
                .GroupBy(x => new
                {
                    x.url,
                    x.title
                })
                .Select(x => new
                {
                    Count = wordCount(x.Key.title.ToLower(), words),
                    Title = x.Key.title,
                    Url = x.Key.url
                })
                .OrderByDescending(x => x.Count)
                .Take(20)
                .ToList();

                serialized = serializer.Serialize(orderedResults);
                cache[input] = new Tuple<string, DateTime>(serialized, DateTime.Now);
            }
            else
            {
                serialized = serializer.Serialize("No Results");
            }
            return serialized;
        }

        private int wordCount(string input, List<string> words)
        {
            var punctuation = input.Where(Char.IsPunctuation).Distinct().ToArray();
            var inputWords = input.ToLower().Split().Select(x => x.Trim(punctuation)).ToList();

            int count = 0;
            foreach (string word in words)
            {
                count = count + inputWords.Count(x => x.Equals(word));
            }
            return count;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetSystemInfo()
        {
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

            TableQuery<Error> query = new TableQuery<Error>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "error"));

            List<Tuple<string, string>> results = new List<Tuple<string, string>>();
            foreach (Error entity in stats.ExecuteQuery(query))
            {
                results.Add(new Tuple<string, string>(entity.error, entity.url));
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStatus()
        {
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