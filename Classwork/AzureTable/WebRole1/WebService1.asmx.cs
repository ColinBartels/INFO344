using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

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
        private static CloudTable table;
        private static List<NBAPlayerStats> players;

        [WebMethod]
        public void ReadPlayerFile()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("nbaplayerstats");
            table.CreateIfNotExists();

            string filename = System.Web.HttpContext.Current.Server.MapPath(@"/stats.csv");
            List<string> filedata = new List<string>();
            using (StreamReader SR = new StreamReader(filename))
            {
                string line;
                while ((line = SR.ReadLine()) != null)
                {
                    filedata.Add(line);
                }
            }
            
            var nbaplayers = filedata.Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new NBAPlayerStats(x[0], x[21]))
                .Take(30)
                .ToArray();

            players = new List<NBAPlayerStats>();
            foreach (NBAPlayerStats player in nbaplayers)
            {
                players.Add(player);
            }
            
        }

        [WebMethod]
        public void InsertAllPlayers()
        {
            foreach (NBAPlayerStats player in players)
            {
                TableOperation insertOperation = TableOperation.Insert(player);
                table.Execute(insertOperation);
            }
        }

        [WebMethod]
        public List<string> SearchAllPlayers(string start, string end)
        {
            TableQuery<NBAPlayerStats> rangeQuery = new TableQuery<NBAPlayerStats>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, start.ToUpper()),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, end.ToUpper()))
                );
            List<string> output = new List<string>();
            foreach (NBAPlayerStats entity in table.ExecuteQuery(rangeQuery))
            {
                string line = entity.Name + " | " + entity.PPG;
                output.Add(line);
            }
            return output;
        }
    }
}
