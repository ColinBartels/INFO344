using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

namespace WebApplication1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {


        public static int counter = 0;
        [WebMethod]
        public string HelloWorld(string name)
        {
            counter++;
            return "hello world " + name + " " + counter;

        }
       
        [WebMethod]
        public string read()
        {

            string filename = HttpRuntime.AppDomainAppPath + @"\output.txt";
            string text = "";
            using (StreamWriter sw = new StreamWriter(filename))
            {
                for (int i = 0; i < 10; i++)
                {
                    sw.WriteLine(i);
                    text += i;
                }
            }

            using (StreamReader sr = new StreamReader(filename))
            {
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();
                    Console.WriteLine(line);
                }
            }
            return text;

        }

        [WebMethod]
        public string settings()
        {
            return System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"];
        }

        [WebMethod]
        public int[] OddNumbers(int n)
        {
            List<int> oddNumbers = new List<int>();

            for (int i = 0; i < n; i++)
            {
                if (i % 2 == 1)
                {
                    oddNumbers.Add(i);
                }
            }
            return oddNumbers.ToArray();
        }

        [WebMethod]
        public string downloadFile()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("info344test");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("Hello-Blob.txt");

            string text;
            using (var memoryStream = new MemoryStream())
            {
                blockBlob.DownloadToStream(memoryStream);
                text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return text;
        }

        [WebMethod]
        public string firstLine()
        {

        }
        
    }
}
