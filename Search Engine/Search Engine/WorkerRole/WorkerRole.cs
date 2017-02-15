using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.IO;
using System.Xml;
using System.Text;
using ClassLibrary;
using System.Security.Cryptography;
using HtmlAgilityPack;
using System.Configuration;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static CloudQueue queue;
        private static CloudQueue xmlQueue;
        private static CloudQueue admin;
        private static CloudTable table;
        private static CloudTable stats;
        private static HashSet<string> dups;
        private static List<string> disallow;
        private static Crawler crawler;
        private static bool running;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            init();
            running = true;

            if (disallow == null)
            {
                disallow = new List<string>();
            }

            if (crawler == null)
            {
                crawler = new Crawler();
            }

            if (dups == null)
            {
                dups = new HashSet<string>();
            }

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private void init()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            if (queue == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference("urlqueue");
                queue.CreateIfNotExists();
            }

            if (xmlQueue == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                xmlQueue = queueClient.GetQueueReference("xmlqueue");
                xmlQueue.CreateIfNotExists();
            }

            if (admin == null)
            {
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                admin = queueClient.GetQueueReference("admin");
                admin.CreateIfNotExists();
            }

            if (table == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference("urltable");
                table.CreateIfNotExists();
            }

            if (stats == null)
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                stats = tableClient.GetTableReference("stats");
                stats.CreateIfNotExists();
            }
        }

        private void addToHTMLQueue(string loc, string currentRoot)
        {
            if (!loc.EndsWith(".xml"))
            {
                if (loc.StartsWith("/") && !loc.StartsWith("//"))
                {
                    loc = "http://www." + currentRoot + ".com" + loc;
                }
                else if (loc.StartsWith("//"))
                {
                    loc = "http:" + loc;
                }
            }

            if (
                !dups.Contains(loc) 
                && !disallow.Any(loc.Contains) 
                && (loc.Contains("cnn.com") || loc.Contains("bleacherreport.com/articles/"))
                )
            {
                CloudQueueMessage newMessage = new CloudQueueMessage(loc);
                queue.AddMessageAsync(newMessage);
                dups.Add(loc);
            }
        }

        private void addToXMLQueue(string url)
        {
            if (!dups.Contains(url) && !disallow.Any(url.Contains) && (url.Contains("cnn.com") || url.Contains("bleacherreport.com")))
            {
                CloudQueueMessage newMessage = new CloudQueueMessage(url);
                xmlQueue.AddMessageAsync(newMessage);
                dups.Add(url);
            }
        }

        private void addToTable(string title, string url, DateTime date)
        {
            var punctuation = title.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = title.ToLower().Split().Select(x => x.Trim(punctuation)).ToList();
            List<string> newWords = new List<string>();
            foreach (string word in words)
            {
                if (word.Count() > 0 && !word.Contains(".com"))
                {
                    newWords.Add(word);
                }
            }
            
            //Hashes URL
            byte[] encodedUrl = new UTF8Encoding().GetBytes(url);
            byte[] urlhash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedUrl);
            string encodedUrlString = BitConverter.ToString(urlhash)
               .Replace("-", string.Empty)
               .ToLower();

            foreach (string word in newWords)
            {
                //Hashes Word
                byte[] encodedWord = new UTF8Encoding().GetBytes(word);
                byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedWord);
                string encodedWordString = BitConverter.ToString(hash)
                   .Replace("-", string.Empty)
                   .ToLower();

                //Adds new page to table
                Page page = new Page(title, url, date, encodedWordString, encodedUrlString);
                TableOperation insertOperation = TableOperation.InsertOrReplace(page);
                table.Execute(insertOperation);
            }

            //Updates last 10 urls
            TableOperation getLast10 = TableOperation.Retrieve<Last10>("last10", "last10");
            TableResult result = stats.Execute(getLast10);
            Last10 last10;
            if (result.Result != null)
            {
                last10 = ((Last10)result.Result);
                last10.updateUrls(url);
            }
            else
            {
                last10 = new Last10(url);
            }
            TableOperation updateQueue = TableOperation.InsertOrReplace(last10);
            stats.Execute(updateQueue);

            //Updates table size
            int size;
            TableOperation retrieveOperation = TableOperation.Retrieve<Size>("size", "indexsize");
            TableResult retrievedResult = stats.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                size = ((Size)retrievedResult.Result).size;
            }
            else
            {
                size = 0;
            }
            Size sizeObject = new Size(size + 1, "indexsize");
            TableOperation updateSize = TableOperation.InsertOrReplace(sizeObject);
            stats.Execute(updateSize);
        }

        private void incrementTotalCrawled()
        {
            int size;
            TableOperation retrieveOperation = TableOperation.Retrieve<Size>("size", "totalsize");
            TableResult retrievedResult = stats.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                size = ((Size)retrievedResult.Result).size;
            }
            else
            {
                size = 0;
            }
            Size sizeObject = new Size(size + 1, "totalsize");
            TableOperation updateSize = TableOperation.InsertOrReplace(sizeObject);
            stats.Execute(updateSize);
        }

        private async Task UpdateStats()
        {
            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;

            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            cpuCounter.NextValue();

            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            while (true)
            {
                SystemStats statsObject = new SystemStats(Math.Ceiling(ramCounter.NextValue()).ToString(), Math.Ceiling(cpuCounter.NextValue()).ToString());

                TableOperation insertOperation = TableOperation.InsertOrReplace(statsObject);
                stats.ExecuteAsync(insertOperation);

                await Task.Delay(3000);
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            UpdateStats();
            while (!cancellationToken.IsCancellationRequested)
            {
                CloudQueueMessage adminMessage = admin.GetMessage();
                if (adminMessage != null)
                {
                    if (adminMessage.AsString.Equals("start"))
                    {
                        init();
                        running = true;
                    }
                    else if (adminMessage.AsString.Equals("stop"))
                    {
                        running = false;
                    }
                    admin.DeleteMessage(adminMessage);
                }
                if (running)
                {
                    xmlQueue.FetchAttributes();
                    queue.FetchAttributes();
                    string currentRoot;
                    //XML
                    if (xmlQueue.ApproximateMessageCount > 0)
                    {
                        CloudQueueMessage XMLMessage = xmlQueue.GetMessage(new TimeSpan(0, 5, 0));

                        Status statusEntity = new Status("loading");
                        TableOperation updateStatus = TableOperation.InsertOrReplace(statusEntity);
                        stats.Execute(updateStatus);

                        string messageString = XMLMessage.AsString;

                        if (messageString.Contains("cnn.com"))
                        {
                            currentRoot = "cnn";
                        }
                        else
                        {
                            currentRoot = "bleacherreport";
                        }

                        if (messageString.EndsWith(".xml"))
                        {
                            List<string> urls = new List<string>();

                            XmlDocument doc = new XmlDocument();
                            doc.Load(messageString);
                            var root = doc.DocumentElement;

                            if (root.Name.Equals("sitemapindex"))
                            {
                                urls = crawler.parseXMLSitemapIndex(doc);
                                foreach (string url in urls)
                                {
                                    addToXMLQueue(url);
                                }
                            }
                            else if (root.Name.Equals("urlset"))
                            {
                                urls = crawler.parseXMLUrlset(doc);
                                foreach (string url in urls)
                                {
                                    addToHTMLQueue(url, currentRoot);
                                }
                            }
                        }
                        //Parses robots.txt
                        else if (messageString.EndsWith("robots.txt"))
                        {
                            WebClient wc = new WebClient();
                            MemoryStream stream = new MemoryStream(wc.DownloadData(messageString));
                            StreamReader reader = new StreamReader(stream);
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.StartsWith("Sitemap:"))
                                {
                                    string[] split = line.Split(' ');
                                    string url = split[1].Trim();
                                    if (url.Equals("http://bleacherreport.com/sitemap/nba.xml"))
                                    {
                                        addToXMLQueue(url);
                                    }
                                    else if (messageString.Equals("http://www.cnn.com/robots.txt"))
                                    {
                                        addToXMLQueue(url);
                                    }
                                }
                                else if (line.StartsWith("Disallow:"))
                                {
                                    string[] split = line.Split(' ');
                                    string url = split[1].Trim();
                                    string fullURL = messageString.Remove(messageString.IndexOf("/robots.txt"), "/robots.txt".Length);
                                    fullURL = fullURL + url;
                                    disallow.Add(fullURL);
                                }
                            }
                        }
                        xmlQueue.DeleteMessage(XMLMessage);
                    }
                    //HTML
                    else if (queue.ApproximateMessageCount > 0)
                    {
                        CloudQueueMessage message = queue.GetMessage();
                        Status statusEntity = new Status("crawling");
                        TableOperation updateStatus = TableOperation.InsertOrReplace(statusEntity);
                        stats.Execute(updateStatus);

                        string messageString = message.AsString;

                        if (messageString.Contains("cnn.com"))
                        {
                            currentRoot = "cnn";
                        }
                        else
                        {
                            currentRoot = "bleacherreport";
                        }

                        try
                        {
                            HtmlWeb web = new HtmlWeb();
                            HtmlDocument doc = web.Load(messageString);
                            if (web.StatusCode != HttpStatusCode.OK)
                            {
                                Error error = new Error(web.StatusCode.ToString(), messageString);
                                TableOperation insert = TableOperation.Insert(error);
                                stats.Execute(insert);
                            }
                            else
                            {
                                List<string> urls = crawler.parseHTML(doc);
                                foreach (string url in urls)
                                {
                                    addToHTMLQueue(url, currentRoot);
                                }
                                addToTable(crawler.getLastPageTitle(), messageString, DateTime.Now);
                            }
                        }
                        catch(WebException e)
                        {
                            Error error = new Error(e.Message, messageString);
                            TableOperation insert = TableOperation.Insert(error);
                            stats.Execute(insert);
                            queue.DeleteMessage(message);
                        }
                        queue.DeleteMessage(message);
                        incrementTotalCrawled();
                    }
                    //Queues empty
                    else
                    {
                        running = false;
                    }
                }
                else
                {
                    Status statusEntity = new Status("idle");
                    TableOperation updateStatus = TableOperation.InsertOrReplace(statusEntity);
                    stats.Execute(updateStatus);
                    await Task.Delay(1000);
                }
            }
        }
    }
}