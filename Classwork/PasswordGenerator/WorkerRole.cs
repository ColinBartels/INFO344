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
using System.Text;
using System.Security.Cryptography;
using ClassLibrary1;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static CloudQueue queue;
        private static CloudTable table;

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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=info344test;AccountKey=KtM9ioPeJEGVEwnn4dZgCF+LlDruLQIB4/YJvWt/jG5xM6RxDCQF76xnfGTY5Y0mBKZxoFuqh8RNcrm/oEWqOg==");

            if (queue == null)
            {
                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Retrieve a reference to a container.
                queue = queueClient.GetQueueReference("passwordqueue");

                // Create the queue if it doesn't already exist
                queue.CreateIfNotExists();
            }
            

            if (table == null)
            {
                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                table = tableClient.GetTableReference("accounts");

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
                Trace.TraceInformation("Working");
                await Task.Delay(1000);

                CloudQueueMessage message = queue.GetMessage();
                if (message != null)
                {
                    queue.DeleteMessage(message);
                    string messageString = message.AsString;
                    byte[] encodedPassword = new UTF8Encoding().GetBytes(messageString + "info344");
                    byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
                    string password = BitConverter.ToString(hash)
                        .Replace("-", string.Empty)
                        .ToLower();

                    Account newAccount = new Account(messageString, password);
                    newAccount.username = messageString;
                    newAccount.password = password;

                    TableOperation insertOperation = TableOperation.Insert(newAccount);

                    // Execute the insert operation.
                    table.Execute(insertOperation);

                    TableOperation retrieveOperation = TableOperation.Retrieve<Counter>("counter", "counterrow");
                    TableResult retrievedResult = table.Execute(retrieveOperation);
                    Counter updateEntity = (Counter)retrievedResult.Result;

                    if (updateEntity != null)
                    {
                        // Change the phone number.
                        updateEntity.count = updateEntity.count + 1;

                        // Create the Replace TableOperation.
                        TableOperation updateOperation = TableOperation.Replace(updateEntity);

                        // Execute the operation.
                        table.Execute(updateOperation);
                    }else
                    {
                        Counter counter = new Counter("count");
                        counter.count = 1;

                        TableOperation insertCounter = TableOperation.InsertOrReplace(newAccount);

                        // Execute the insert operation.
                        table.Execute(insertCounter);
                    }

                }
            }
        }
    }
}
