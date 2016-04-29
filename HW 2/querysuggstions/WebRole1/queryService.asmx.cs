using System.IO;
using System.Web.Services;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using Query_Suggestion;
using System.Diagnostics;

namespace WebRole1
{
    /// <summary>
    /// Summary description for queryService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class queryService : System.Web.Services.WebService
    {

        public static Trie trie;

        [WebMethod]
        public void downloadWiki()
        {
            var path = (Server.MapPath("~/page_titles.txt"));
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("querysuggestions");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("page_titles.txt");
            using (var fileStream = System.IO.File.OpenWrite(path))
            {
                blockBlob.DownloadToStream(fileStream);
            }
        }

        [WebMethod]
        public string createTrie()
        {
            trie = new Trie();
            int count = 0;
            string lastLine = "";
            PerformanceCounter PC = new PerformanceCounter("Memory", "Available MBytes");
            foreach (var line in File.ReadLines(Server.MapPath("~/page_titles.txt")))
            //foreach (var line in File.ReadLines(@"C:\Users\Colin\Dropbox\INFO344\HW 2\page_titles.txt"))
                {
                    if (count % 1000 == 0)
                {
                    if (PC.NextValue() < 35)
                    {
                        lastLine = line;
                        break;
                    }
                }
                count++;
                trie.addLine(line);
                lastLine = line;
            }
            return lastLine + " " + count;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string findSuggestions(string prefix)
        {
            return new JavaScriptSerializer().Serialize(trie.findCompletions(prefix));
        }

        [WebMethod]
        public float getMem()
        {
            PerformanceCounter PC = new PerformanceCounter("Memory", "Available MBytes");
            return PC.NextValue();
        }
    }
}
