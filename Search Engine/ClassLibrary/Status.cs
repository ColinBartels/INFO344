using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ClassLibrary
{
    public class Status : TableEntity
    {
        public Status(string status)
        {
            this.PartitionKey = "status";
            this.RowKey = "status";
            this.status = status;
        }

        public Status() { }

        public string status { get; set; }
    }
}