using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace Queue
{
    /// <summary>
    /// Summary description for QueueService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class QueueService : System.Web.Services.WebService
    {
        private static CloudQueue queue;

        [WebMethod]
        public void AddMessage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("myurls");
            queue.CreateIfNotExists();
            CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/index.html");
            queue.AddMessage(message);
        }

        [WebMethod]
        public void RemoveMessage()
        {
            CloudQueueMessage message2 = queue.GetMessage(TimeSpan.FromMinutes(5));
            queue.DeleteMessage(message2);
        }
    }
}
