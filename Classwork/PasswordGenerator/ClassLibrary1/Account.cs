using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;


namespace ClassLibrary1
{
    public class Account : TableEntity
    {
        public Account(string username, string password)
        {
            this.PartitionKey = username;
            this.RowKey = username;
        }

        public Account() { }

        public string username { get; set; }
        public string password { get; set; }
    }
}
