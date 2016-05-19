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
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using ClassLibrary;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private static CloudQueue queue = null;
        private static CloudTable table = null;

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
            if (queue == null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=info344test;AccountKey=KtM9ioPeJEGVEwnn4dZgCF+LlDruLQIB4/YJvWt/jG5xM6RxDCQF76xnfGTY5Y0mBKZxoFuqh8RNcrm/oEWqOg=="); 

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Retrieve a reference to a container.
                queue = queueClient.GetQueueReference("numbersum");

                // Create the queue if it doesn't already exist
                queue.CreateIfNotExists();
            }

            if (table == null)
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=info344test;AccountKey=KtM9ioPeJEGVEwnn4dZgCF+LlDruLQIB4/YJvWt/jG5xM6RxDCQF76xnfGTY5Y0mBKZxoFuqh8RNcrm/oEWqOg==");

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                table = tableClient.GetTableReference("sumresults");

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();
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

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                CloudQueueMessage message = queue.GetMessage();
                if (message != null)
                {
                    queue.DeleteMessage(message);
                    string messageString = message.AsString;
                    string[] nums = messageString.Split(',');
                    int sum = 0;
                    foreach (string number in nums)
                    {
                        int num = int.Parse(number);
                        sum += num;
                    }
                    Sum newSum = new Sum(sum);
                    newSum.sum = sum;

                    // Create the TableOperation object that inserts the customer entity.
                    TableOperation insertOperation = TableOperation.Insert(newSum);

                    // Execute the insert operation.
                    table.Execute(insertOperation);

                }


                Trace.TraceInformation("Working");
                await Task.Delay(10000);
            }
        }
    }
}
