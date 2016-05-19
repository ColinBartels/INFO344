using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace ReverseWords
{
    /// <summary>
    /// Summary description for ReverseWords
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ReverseWords : System.Web.Services.WebService
    {

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ReverseWord(string input)
        {
            string[] words = input.Split(' ');
            string output = "";
            for (int i = words.Length - 1; i >= 0; i--)
            {
                output += words[i] + " ";
            }
            return new JavaScriptSerializer().Serialize(output);
        }

        [WebMethod]
        public void SubmitPhrases(string input)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("reversewords");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            Phrase phrase = new Phrase (input, ReverseWord(input));
            phrase.normal = input;
            phrase.reverse = ReverseWord(input);

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(phrase);
            // Execute the insert operation.
            table.Execute(insertOperation);
        }

        public List<string> GetAllPhrases()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
             CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("reversewords");

            var entities = table.ExecuteQuery(new TableQuery<Phrase>());
            List<string> output = new List<string>();
            foreach (Phrase entity in entities)
            {
                output.Add(entity.PartitionKey + " " + entity.RowKey);
            }
            return output;
        }
    }
}
